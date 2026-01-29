using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatedMoverProximityTrigger : MonoBehaviour
{
    [Header("Points")]
    public Transform pointA;
    public Transform pointB;

    [Header("Movement")]
    public float liftHeight = 3f;
    public float moveSpeed = 2f;
    public float arriveThreshold = 0.02f;

    [Header("Proximity Triggers")]
    public List<Transform> triggers = new List<Transform>();
    public float triggerRadius = 2f;

    [Tooltip("If true, the same trigger must leave the radius before it can trigger again.")]
    public bool requireLeaveToRetrigger = true;

    private bool goingToB = true;
    private bool isMoving = false;

    // Tracks which triggers are currently inside the radius (for re-arming)
    private HashSet<Transform> inside = new HashSet<Transform>();

    void Start()
    {
        // Optional: start exactly at A if assigned
        if (pointA != null)
            transform.position = pointA.position;
    }

    void Update()
    {
        if (isMoving) return;
        if (pointA == null || pointB == null) return;
        if (triggers == null || triggers.Count == 0) return;

        // Find if any trigger is close enough
        Transform triggering = GetAnyTriggerInRange();

        if (triggering != null)
        {
            // If we require leaving to retrigger, only trigger when it JUST entered range
            if (requireLeaveToRetrigger)
            {
                if (!inside.Contains(triggering))
                {
                    inside.Add(triggering);
                    StartCoroutine(MoveOnce());
                }
            }
            else
            {
                StartCoroutine(MoveOnce());
            }
        }

        // Update "inside" set so triggers can re-arm when they leave range
        if (requireLeaveToRetrigger)
            RefreshInsideSet();
    }

    Transform GetAnyTriggerInRange()
    {
        for (int i = 0; i < triggers.Count; i++)
        {
            Transform t = triggers[i];
            if (t == null) continue;

            if (Vector3.Distance(transform.position, t.position) <= triggerRadius)
                return t;
        }
        return null;
    }

    void RefreshInsideSet()
    {
        // Remove any transforms that have left the radius (or got destroyed)
        var toRemove = new List<Transform>();

        foreach (var t in inside)
        {
            if (t == null)
            {
                toRemove.Add(t);
                continue;
            }

            if (Vector3.Distance(transform.position, t.position) > triggerRadius)
                toRemove.Add(t);
        }

        for (int i = 0; i < toRemove.Count; i++)
            inside.Remove(toRemove[i]);
    }

    IEnumerator MoveOnce()
    {
        isMoving = true;

        Transform startPoint = goingToB ? pointA : pointB;
        Transform targetPoint = goingToB ? pointB : pointA;

        // Snap to exact start point (optional but keeps paths clean)
        transform.position = startPoint.position;

        // Lift
        yield return MoveToPosition(startPoint.position + Vector3.up * liftHeight);

        // Travel elevated
        yield return MoveToPosition(targetPoint.position + Vector3.up * liftHeight);

        // Drop
        yield return MoveToPosition(targetPoint.position);

        goingToB = !goingToB;
        isMoving = false;
    }

    IEnumerator MoveToPosition(Vector3 destination)
    {
        while (Vector3.Distance(transform.position, destination) > arriveThreshold)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = destination; // snap precisely
    }

    // Optional: visualize trigger radius in editor (selected)
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
