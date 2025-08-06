using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Player2TTS : MonoBehaviour
{
    public AudioSource audioSource;
    public static Player2TTS Instance { get; private set; }

    private const string DefaultVoiceId = "01955d76-ed5b-7451-92d6-5ef579d3ed28";
    private const string TtsApiUrl = "http://localhost:4315/v1/tts/speak";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Prevent duplicates
    }

    public static void SpeakWithVoice(string text, string voiceId = null)
    {
        if (Instance == null)
        {
            Debug.LogError("Player2TTS is not in the scene!");
            return;
        }

        string idToUse = string.IsNullOrEmpty(voiceId) ? DefaultVoiceId : voiceId;
        Instance.StartCoroutine(Instance.SendTTSRequest(text, idToUse));
    }

    private IEnumerator SendTTSRequest(string text, string voiceId)
    {
        string jsonBody = JsonUtility.ToJson(new TTSRequest
        {
            audio_format = "mp3",
            play_in_app = false,
            speed = 1f,
            text = text,
            voice_gender = "female",
            voice_language = "en_US",
            voice_ids = new string[] { voiceId }
        });

        using (UnityWebRequest www = UnityWebRequest.Put(TtsApiUrl, jsonBody))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("TTS Request failed: " + www.error);
            }
            else
            {
                string json = www.downloadHandler.text;
                TTSResponse response = JsonUtility.FromJson<TTSResponse>(json);

                if (!string.IsNullOrEmpty(response.data))
                {
                    byte[] audioBytes = Convert.FromBase64String(response.data);
                    yield return PlayMP3FromBytes(audioBytes);
                }
                else
                {
                    Debug.LogWarning("TTS response received, but 'data' is empty.");
                }
            }
        }
    }

    private IEnumerator PlayMP3FromBytes(byte[] audioData)
    {
        string tempFilePath = Path.Combine(Application.persistentDataPath, "tts_audio.mp3");
        File.WriteAllBytes(tempFilePath, audioData);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempFilePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load audio clip: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    [Serializable]
    public class TTSRequest
    {
        public string audio_format;
        public bool play_in_app;
        public float speed;
        public string text;
        public string voice_gender;
        public string voice_language;
        public string[] voice_ids;
    }

    [Serializable]
    public class TTSResponse
    {
        public string data;
    }
}
