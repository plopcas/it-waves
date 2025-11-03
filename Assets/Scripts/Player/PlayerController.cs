using UnityEngine;
using UnityEngine.InputSystem;

namespace ITWaves.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField, Tooltip("Use mouse for aiming (true) or right stick (false).")]
        private bool useMouseAim = true;

        private Vector2 aimInput;
        private Vector2 aimDirection;
        private Camera mainCamera;

        public Vector2 AimDirection => aimDirection;

        private void Awake()
        {
            mainCamera = Camera.main;
            aimDirection = Vector2.right;
        }

        public void OnLook(InputValue value)
        {
            aimInput = value.Get<Vector2>();
        }

        private void Update()
        {
            HandleAiming();
        }

        private void HandleAiming()
        {
            if (useMouseAim && mainCamera != null)
            {
                // Mouse aiming using new Input System
                Vector3 mousePos = Mouse.current.position.ReadValue();
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
                Vector2 direction = (worldPos - transform.position).normalized;

                if (direction.sqrMagnitude > 0.01f)
                {
                    aimDirection = direction;

                    // Rotate to face aim direction
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
            }
            else if (aimInput.sqrMagnitude > 0.1f)
            {
                // Gamepad aiming
                aimDirection = aimInput.normalized;

                float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }
}

