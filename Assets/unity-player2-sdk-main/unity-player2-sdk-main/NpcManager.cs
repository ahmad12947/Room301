using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine.Serialization;
using UnityEngine.UI;
[Serializable]
public class Function
{
    public string name;
    public string description;
    public List<FunctionArgument> functionArguments;

    public SerializableFunction ToSerializableFunction()
    {
        
        var props = new Dictionary<string, SerializedArguments>();
        
        for (int i = 0; i < functionArguments.Count; i++)
        {
            var arg = functionArguments[i];
            props[arg.argumentName] = new SerializedArguments
            {
                type = arg.argumentType,
                description = arg.argumentDescription
            };
        }
        Debug.Log(props);
        return new SerializableFunction
        {
            name = name,
            description = description,
            parameters = new Parameters
            {
                Properties = props,
                required = functionArguments.FindAll(arg => arg.required).ConvertAll(arg => arg.argumentName),
            }
        };
    }
}


[Serializable]
public class FunctionArgument
{
    public string argumentName;
    public string argumentType;
    public string argumentDescription;
    public bool required;
}



public class NpcManager : MonoBehaviour
{   //01955d76-ed5b-7451-92d6-5ef579d3ed28
    [Header("Config")]
    [SerializeField] public string gameId = "your-game-id";
    private HashSet<string> _calledFunctions = new HashSet<string>();
    private Player2NpcResponseListener _responseListener;
    
    [Header("Functions")] 
    [SerializeField] public List<Function> functions;
    [SerializeField] public UnityEvent<FunctionCall> functionHandler;

    public readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy(),
        }
    };
    
    public List<SerializableFunction> GetSerializableFunctions()
    {
        var serializableFunctions = new List<SerializableFunction>();

        foreach (var function in functions)
        {
            // Skip already called functions
            if (_calledFunctions.Contains(function.name)) continue;

            serializableFunctions.Add(function.ToSerializableFunction());
        }

        return serializableFunctions;
    }
    
    
    
    private const string BaseUrl = "http://localhost:4315/v1";
    
    public static string GetBaseUrl() 
    {
        return BaseUrl;
    }

    private void Awake() 
    {
        // Add the component and configure it
        _responseListener = gameObject.AddComponent<Player2NpcResponseListener>();
        _responseListener.SetGameId(gameId);
        _responseListener.JsonSerializerSettings = JsonSerializerSettings;
        
        Debug.Log($"NpcManager initialized with gameId: {gameId}");
    }

    private void Start()
    {
        // Ensure listener starts after all components are initialized
        if (_responseListener != null && !_responseListener.IsListening)
        {
            Debug.Log("Starting response listener from NpcManager");
            _responseListener.StartListening();
        }
        
    }

    public void RegisterNpc(string id, Text onNpcResponse)
    {
        if (_responseListener == null)
        {
            Debug.LogError("Response listener is null! Cannot register NPC.");
            return;
        }

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("Cannot register NPC with empty ID");
            return;
        }

        Debug.Log($"Registering NPC with ID: {id}");

        var onNpcApiResponse = new UnityEvent<NpcApiChatResponse>();
        onNpcApiResponse.AddListener((response) =>
        {
            if (response != null)
            {
                if (!string.IsNullOrEmpty(response.message))
                {
                    string cleanMessage = System.Text.RegularExpressions.Regex.Replace(response.message, @"^<[^>]+>\s*", "");
                    Debug.Log($"Updating UI for NPC {id}: {response.message}");
                    onNpcResponse.text = cleanMessage;
                Player2TTS.SpeakWithVoice(cleanMessage, "01955d76-ed5b-7451-92d6-5ef579d3ed28");
                }


                if (response.command != null)
                {
                    foreach (var functionCall in response.command)
                    {
                       
                        functionHandler.Invoke(functionCall.ToFunctionCall());

                        // Optionally remove it from function definitions so AI won’t consider it again
                        functions.RemoveAll(f => f.name == functionCall.name);
                    }
                }
            }

        });

        _responseListener.RegisterNpc(id, onNpcApiResponse);

        // Ensure listener is running after registering
        if (!_responseListener.IsListening)
        {
            Debug.Log("Listener was not running, starting it now");
            _responseListener.StartListening();
        }
    }

    public void UnregisterNpc(string id)
    {
        if (_responseListener != null)
        {
            _responseListener.UnregisterNpc(id);
        }
    }

    public bool IsListenerActive()
    {
        return _responseListener != null && _responseListener.IsListening;
    }

    public void StartListener()
    {
        if (_responseListener != null)
        {
            _responseListener.StartListening();
        }
    }

    public void StopListener()
    {
        if (_responseListener != null)
        {
            _responseListener.StopListening();
        }
    }

    private void OnDestroy()
    {
        if (_responseListener != null)
        {
            _responseListener.StopListening();
        }
    }

    // Add this method for debugging
    [ContextMenu("Debug Listener Status")]
    public void DebugListenerStatus()
    {
        if (_responseListener == null)
        {
            Debug.Log("Response listener is NULL");
        }
        else
        {
            Debug.Log($"Response listener status: IsListening={_responseListener.IsListening}, GameId={_responseListener.GameId}");
        }
    }
}

[Serializable]
public class SerializableFunction
{
    public string name;
    public string description;
    public Parameters parameters;
    
}

[Serializable]
public class Parameters
{
    public Dictionary<string, SerializedArguments> Properties { get; set; }
    public List<string> required;
    public string type = "object";
}

[Serializable]
public class SerializedArguments
{
    public string type;
    public string description;
}