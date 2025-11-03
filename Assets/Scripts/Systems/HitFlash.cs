using System.Collections;
using UnityEngine;

namespace ITWaves.Systems
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitFlash : MonoBehaviour
    {
        [Header("Flash Settings")]
        [SerializeField, Tooltip("Colour to flash.")]
        private Color flashColour = Color.white;
        
        [SerializeField, Tooltip("Flash duration in seconds.")]
        private float flashDuration = 0.1f;
        
        private SpriteRenderer spriteRenderer;
        private Color originalColour;
        private Coroutine flashCoroutine;
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColour = spriteRenderer.color;
        }
        
        public void Flash()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            
            flashCoroutine = StartCoroutine(FlashRoutine());
        }
        
        private IEnumerator FlashRoutine()
        {
            spriteRenderer.color = flashColour;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColour;
            flashCoroutine = null;
        }
        
        private void OnDisable()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }
            spriteRenderer.color = originalColour;
        }
    }
}

