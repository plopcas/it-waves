using UnityEngine;

namespace ITWaves.UI
{
    /// Animates a snake sprite in the Boot scene - fades in and moves down towards center.
    /// Attach this to the Snake game object in the Boot scene.
    public class BootSnakeAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField, Tooltip("Duration of fade in animation (seconds).")]
        private float fadeInDuration = 2f;

        [SerializeField, Tooltip("Duration of movement animation (seconds).")]
        private float moveDuration = 3f;

        [SerializeField, Tooltip("Target Y position (world or local depending on moveInLocalSpace).")]
        private float targetYPosition = 0f;

        [SerializeField, Tooltip("Delay before starting animation (seconds).")]
        private float startDelay = 0.5f;

        [SerializeField, Tooltip("Use local space for movement instead of world space.")]
        private bool moveInLocalSpace = true;

        [Header("Components")]
        [SerializeField, Tooltip("SpriteRenderer component to fade (optional, will auto-detect).")]
        private SpriteRenderer spriteRenderer;

        private float elapsedTime = 0f;
        private Vector3 startPosition;
        private bool isAnimating = false;
        private bool fadeComplete = false;
        private bool moveComplete = false;

        private void Awake()
        {
            // Auto-detect SpriteRenderer if not assigned
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                Debug.LogError("[BootSnakeAnimator] No SpriteRenderer found on this GameObject!");
                enabled = false;
                return;
            }

            // Set initial alpha to 0
            SetAlpha(0f);

            // Store starting position
            startPosition = moveInLocalSpace ? transform.localPosition : transform.position;
        }

        private void Start()
        {
            // Start animation after delay
            Invoke(nameof(StartAnimation), startDelay);
        }

        private void StartAnimation()
        {
            isAnimating = true;
            elapsedTime = 0f;
        }

        private void Update()
        {
            if (!isAnimating)
            {
                return;
            }

            elapsedTime += Time.deltaTime;

            // Fade in animation
            if (!fadeComplete)
            {
                float fadeProgress = Mathf.Clamp01(elapsedTime / fadeInDuration);
                float alpha = Mathf.Lerp(0f, 1f, fadeProgress);
                SetAlpha(alpha);

                if (fadeProgress >= 1f)
                {
                    fadeComplete = true;
                }
            }

            // Move down animation
            if (!moveComplete)
            {
                float moveProgress = Mathf.Clamp01(elapsedTime / moveDuration);
                float easedProgress = EaseInOutCubic(moveProgress);

                Vector3 targetPos;
                if (moveInLocalSpace)
                {
                    targetPos = new Vector3(startPosition.x, targetYPosition, startPosition.z);
                    transform.localPosition = Vector3.Lerp(startPosition, targetPos, easedProgress);
                }
                else
                {
                    targetPos = new Vector3(startPosition.x, targetYPosition, startPosition.z);
                    transform.position = Vector3.Lerp(startPosition, targetPos, easedProgress);
                }

                if (moveProgress >= 1f)
                {
                    moveComplete = true;
                }
            }

            // Stop animating when both animations are complete
            if (fadeComplete && moveComplete)
            {
                isAnimating = false;
            }
        }

        private void SetAlpha(float alpha)
        {
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }

        private float EaseInOutCubic(float t)
        {
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }
    }
}

