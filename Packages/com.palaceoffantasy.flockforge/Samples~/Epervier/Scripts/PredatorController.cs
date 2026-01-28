using UnityEngine;

namespace PalaceOfFantasy.FlockForge.Samples.Epervier
{
    public class PredatorController : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _dashSpeed = 20f;
        [SerializeField] private float _dashDuration = 0.2f;

        private float _dashTimer;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
            _rb.useGravity = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _dashTimer = _dashDuration;
            }
        }

        private void FixedUpdate()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(h, 0, v).normalized;

            float currentSpeed = _dashTimer > 0 ? _dashSpeed : _speed;
            _dashTimer -= Time.fixedDeltaTime;

            if (input.sqrMagnitude > 0.1f)
            {
                _rb.velocity = input * currentSpeed;
                transform.forward = Vector3.Lerp(transform.forward, input, 10f * Time.fixedDeltaTime);
            }
            else
            {
                _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, 5f * Time.fixedDeltaTime);
            }
        }
    }
}
