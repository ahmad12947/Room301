using System.Collections;
using UnityEngine;

namespace AdvancedHorrorFPS
{
    public class BlinkEffect : MonoBehaviour
    {
        public Color startColor = Color.green;
        public Color endColor = Color.black;
        [Range(0, 10)]
        public float speed = 1f;

        public Renderer targetRenderer; // Assign in Inspector

        private MaterialPropertyBlock propBlock;
        private int emissionColorID;

        void Awake()
        {
            if (targetRenderer == null)
            {
                Debug.LogWarning($"BlinkEffect on {gameObject.name}: targetRenderer is not assigned.");
                enabled = false;
                return;
            }

            // Prepare property block and emission ID
            propBlock = new MaterialPropertyBlock();
            emissionColorID = Shader.PropertyToID("_EmissionColor");

            // Make sure emission is enabled on the material itself (in editor)
            targetRenderer.material.EnableKeyword("_EMISSION");
        }

        private void Start()
        {
            if (!AdvancedGameManager.Instance.blinkOnInteractableObjects)
            {
                this.enabled = false;
            }
        }

        public void Disable()
        {
            speed = 0;
            StartCoroutine(DisableNow());
        }

        IEnumerator DisableNow()
        {
            yield return new WaitForSeconds(0.5f);
            this.enabled = false;
        }

        void Update()
        {
            float lerp = Mathf.PingPong(Time.time * speed, 1f);
            Color emissionColor = Color.Lerp(startColor, endColor, lerp);

            // Use property block to safely update emission
            targetRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor(emissionColorID, emissionColor);
            targetRenderer.SetPropertyBlock(propBlock);
        }
    }
}
