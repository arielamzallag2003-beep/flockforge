using UnityEngine;
using UnityEngine.InputSystem;

namespace ithappy.Animals_FREE
{
    [RequireComponent(typeof(CreatureMover))]
    public class MovePlayerInput : MonoBehaviour
    {
        [Header("Input Actions (Optional)")]
        [SerializeField] private InputActionReference m_Move;   // Vector2
        [SerializeField] private InputActionReference m_Jump;   // Button
        [SerializeField] private InputActionReference m_Run;    // Button

        [Header("Fixed Top-Down Camera")]
        [Tooltip("Fixed camera (or camera pivot) transform used to define movement directions.")]
        [SerializeField] private Transform m_Camera;

        [Header("Top-Down Settings")]
        [SerializeField] private bool m_CameraRelative = true;
        [SerializeField] private float m_TargetDistance = 10f;

        private CreatureMover m_Mover;

        private Vector2 m_Axis;
        private bool m_IsRun;
        private bool m_IsJump;
        private Vector3 m_Target;

        // Fallback actions
        private InputAction _moveFallback;
        private InputAction _jumpFallback;
        private InputAction _runFallback;

        private InputAction MoveAction => (m_Move != null && m_Move.action != null) ? m_Move.action : _moveFallback;
        private InputAction JumpAction => (m_Jump != null && m_Jump.action != null) ? m_Jump.action : _jumpFallback;
        private InputAction RunAction => (m_Run != null && m_Run.action != null) ? m_Run.action : _runFallback;

        private void Awake()
        {
            m_Mover = GetComponent<CreatureMover>();
            CreateFallbackActions();
        }

        private void OnEnable()
        {
            MoveAction?.Enable();
            JumpAction?.Enable();
            RunAction?.Enable();
        }

        private void OnDisable()
        {
            MoveAction?.Disable();
            JumpAction?.Disable();
            RunAction?.Disable();
        }

        private void Update()
        {
            GatherInput();
            ApplyInput();
        }

        private void GatherInput()
        {
            Vector2 raw = MoveAction != null ? MoveAction.ReadValue<Vector2>() : Vector2.zero;
            if (raw.sqrMagnitude > 1f) raw = raw.normalized;

            // CreatureMover's "right" is flipped (Cross(up, forward) => left),
            // so fix left/right here.
            m_Axis = new Vector2(raw.x, raw.y);

            m_IsRun = RunAction != null && RunAction.IsPressed();
            m_IsJump = JumpAction != null && JumpAction.WasPressedThisFrame();

            // IMPORTANT: target defines the *reference forward* (camera/look), not where we move.
            // Keep it constant and camera-aligned.
            Vector3 referenceForward = GetCameraPlanarForward();
            m_Target = transform.position + referenceForward * m_TargetDistance;
        }

        private void ApplyInput()
        {
            if (m_Mover == null) return;
            m_Mover.SetInput(in m_Axis, in m_Target, in m_IsRun, m_IsJump);
        }

        private Vector3 GetCameraPlanarForward()
        {
            // If not camera relative or no camera, use world forward.
            if (!m_CameraRelative || m_Camera == null)
                return Vector3.forward;

            // If your camera looks straight down, camera.forward projects to ~0 on XZ.
            // In that case, camera.up usually represents "screen up" on the ground plane.
            Vector3 f = m_Camera.forward; f.y = 0f;
            if (f.sqrMagnitude > 0.0001f)
                return f.normalized;

            Vector3 u = m_Camera.up; u.y = 0f;
            if (u.sqrMagnitude > 0.0001f)
                return u.normalized;

            return Vector3.forward;
        }

        private void CreateFallbackActions()
        {
            _moveFallback = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");

            // AZERTY (ZQSD)
            _moveFallback.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/z")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/q")
                .With("Right", "<Keyboard>/d");

            // QWERTY (WASD)
            _moveFallback.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            // Arrows
            _moveFallback.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            _moveFallback.AddBinding("<Gamepad>/leftStick");

            _runFallback = new InputAction("Run", InputActionType.Button);
            _runFallback.AddBinding("<Keyboard>/leftShift");
            _runFallback.AddBinding("<Gamepad>/leftStickPress");

            _jumpFallback = new InputAction("Jump", InputActionType.Button);
            _jumpFallback.AddBinding("<Keyboard>/space");
            _jumpFallback.AddBinding("<Gamepad>/buttonSouth");
        }
    }
}
