using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowBehaviour : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    Animator animator;
    [System.NonSerialized] public GameObject ArrowExclusion;
    Vector3 targetPosition;
    Vector2 direction;

    public float rotationModifier;
    public float rotationSpeed;
    public float ArrowSpeed;

    float depth = 0.30F;
    bool hit, cr;

    public bool AimPrediction;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    public void setTarget(Transform Target)
    {
        if (AimPrediction)
            targetPosition = PredictPosition(Target.GetComponent<Rigidbody2D>());
        if (!AimPrediction)
            targetPosition = Target.transform.position;
        direction = transform.position - targetPosition;
        animator.speed = Vector3.Distance(transform.position, Target.transform.position) / 5 * 0.1f * ArrowSpeed;
        RotateToTarget();
    }

    Vector3 PredictPosition(Rigidbody2D targetRigid)
    {
        Vector3 pos = targetRigid.position;
        Vector3 dir = targetRigid.velocity;

        float dist = (pos - transform.position).magnitude;

        return pos + (dist / ArrowSpeed) * dir;
    }


    void OnCollisionEnter2D(Collision2D other)
    {
        if (!hit)
        {
            ArrowStick(other);
        }
    }
    void ArrowStick(Collision2D col)
    {
        if (col.gameObject == ArrowExclusion)
        {
            return;
        }

        if (col.gameObject.layer == 8)
        {
            transform.parent = col.transform;
            Disable();
            return;
        }

        Destroy(gameObject);
        return;

    }

    private void Disable()
    {
        hit = true;
        Destroy(GetComponent<Rigidbody2D>());
        Destroy(GetComponent<Collider2D>());
    }


    void MoveToTarget()
    {
        // rb.position = Vector3.MoveTowards(transform.position, targetPosition, 0.01f * ArrowSpeed);
        rb.velocity = -direction.normalized * ArrowSpeed;
    }

    void RotateToTarget()
    {
        Vector3 vectorToTarget = targetPosition - transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - rotationModifier;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, 1000);
    }


    private void Update()
    {
        if (targetPosition != null)
        {
            if (!hit)
            {
                MoveToTarget();
            }

        }

    }

}
