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
            audio_format = "wav",
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
                    yield return PlayWavFromBytes(audioBytes);
                }
                else
                {
                    Debug.LogWarning("TTS response received, but 'data' is empty.");
                }
            }
        }
    }

    private IEnumerator PlayWavFromBytes(byte[] audioData)
    {
#if !UNITY_WEBGL
    // Works for standalone/editor
    string tempWavPath = Path.Combine(Application.streamingAssetsPath, "temp.wav");
    File.WriteAllBytes(tempWavPath, audioData);

    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempWavPath, AudioType.WAV))
    {
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load WAV audio: " + www.error);
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
#else
        // WebGL fallback: parse WAV manually
        AudioClip clip = WavBytesToAudioClip(audioData);
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Failed to parse WAV data in WebGL.");
        }
        yield return null;
#endif
    }


    private AudioClip WavBytesToAudioClip(byte[] wavFile)
    {
        try
        {
            int channels = BitConverter.ToInt16(wavFile, 22);
            int sampleRate = BitConverter.ToInt32(wavFile, 24);
            int subchunk2 = BitConverter.ToInt32(wavFile, 40);
            int samples = subchunk2 / 2;

            float[] data = new float[samples];
            int offset = 44;

            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(wavFile, offset);
                data[i] = sample / 32768f;
                offset += 2;
            }

            AudioClip audioClip = AudioClip.Create("TTS_Clip", samples, channels, sampleRate, false);
            audioClip.SetData(data, 0);
            return audioClip;
        }
        catch (Exception e)
        {
            Debug.LogError("WAV parsing failed: " + e.Message);
            return null;
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
