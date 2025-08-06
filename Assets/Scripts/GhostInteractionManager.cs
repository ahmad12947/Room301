using UnityEngine;
using TMPro;
using UnityEngine.UI;
using AdvancedHorrorFPS;
public class GhostInteractionManager : MonoBehaviour
{
    public Light hallwayLight, roomLight; // Assign in Inspector
    public GameObject Npc1, Npc2, Npc3, flashLight;
    public Text outputText, objectiveTxt;
    public BoxCollider fuzeBox;
    public BlinkEffect effect;
    public AudioSource audio;
    private bool runOnce = false;
    public void TurnOffLight(string lightName)
    {
        if (hallwayLight != null && hallwayLight.name == lightName && runOnce==false)
        {
            hallwayLight.enabled = false;
            roomLight.enabled = false;
            outputText.text = "STAY AWAY!! I warned you";
            objectiveTxt.text = "Find the fuze box and turn on the power.";
            Debug.Log($"[GhostInteraction] Turned off light: {lightName}");
            Player2TTS.SpeakWithVoice(outputText.text, "01955d76-ed5b-7451-92d6-5ef579d3ed28");
            fuzeBox.enabled = true;
            effect.enabled = true;
            flashLight.SetActive(true);
            audio.Play();
            Npc1.SetActive(false);
            Npc2.SetActive(true);
            runOnce = true;
        }
        else
        {
            Debug.LogWarning($"[GhostInteraction] Light named '{lightName}' not found or not assigned.");
        }
    }
    public string[] hints =
     {
        "Not all doors open with keys.",
        "She left something behind near the stairs.",
        "The power isn't stable tonight.",
        "The mirror shows more than your reflection."
    };

    public void GiveHint(FunctionCall call)
    {
        if (call.name != "GiveHint") return;
        
        string hint = hints[Random.Range(0, hints.Length)];
        Debug.Log($"[NPC Hint]: {hint}");
        outputText.text = hint;
        Player2TTS.SpeakWithVoice(outputText.text, "01955d76-ed5b-7451-92d6-5ef579d3ed28");
        //Npc2.SetActive(false);
        //Npc3.SetActive(true);

        // Optionally: show in a popup or UI Text if needed
        // Example: UIManager.Instance.ShowHint(hint);
    }


}
