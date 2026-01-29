using UnityEngine;

public class TransformSpeedToBlendTreeStable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform measuredTransform; // assign the moving root!
    [SerializeField] private string speedParam = "Speed";

    [Header("Tuning")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float dampTime = 0.1f;

    [Tooltip("Seconds over which we average speed to avoid Update/FixedUpdate jitter.")]
    [SerializeField] private float sampleWindow = 0.08f;

    private Vector3 lastPos;
    private float accumTime;
    private float speedSmoothed;
    private int speedHash;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!measuredTransform) measuredTransform = transform; // fallback
        speedHash = Animator.StringToHash(speedParam);
    }

    void Start()
    {
        lastPos = measuredTransform.position;
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || !animator) return;

        Vector3 currentPos = measuredTransform.position;
        Vector3 delta = currentPos - lastPos;

        accumTime += dt;

        // only update speed every sampleWindow seconds
        if (accumTime >= sampleWindow)
        {
            float rawSpeed = delta.magnitude / accumTime; // units/sec over the window
            rawSpeed = Mathf.Clamp(rawSpeed, 0f, maxSpeed);

            speedSmoothed = rawSpeed;

            lastPos = currentPos;
            accumTime = 0f;
        }

        animator.SetFloat(speedHash, speedSmoothed, dampTime, dt);
    }
}
