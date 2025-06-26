using UnityEngine;
using UnityEngine.AI;

namespace Characters
{
    public class Mover : MonoBehaviour
    {
        [SerializeField] Transform movementTarget;
        private Vector3 movementTargetPosition;
        public NavMeshAgent agent;
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            Vector3 velocity = agent.velocity;
        }

        public void SetTarget(Transform target, float stopDistance)
        {
            Vector2 movementTargetPos = target.position;           // T
            Vector2 myPos = target.position;        // P

            // Direction from T → P (so pointing out from the movementTarget toward you)
            Vector2 dir = (myPos - movementTargetPos).normalized;

            // Desired destination = T + dir * stopDistance
            Vector2 dest = movementTargetPos + dir * stopDistance;

            // Now call your existing mover logic on that point
            SetTarget(dest);
        }

        public void SetTarget(Transform movementTargetTransform)
        {
            agent.stoppingDistance = 0f; // Default stopping distance
            movementTargetPosition = Vector3.zero;
            movementTarget = movementTargetTransform;
        }

        public void SetTarget(Vector3 position)
        {
            movementTarget = null;
            movementTargetPosition = position;
        }

        public void MoveTo(Vector3 destination)
        {
            agent.destination = destination;
            agent.isStopped = false;
        }

        public Vector3 GetVelocity()
        {
            return agent.velocity;
        }

        public void Disable()
        {
            agent.enabled = false;
        }

        public void Stop()
        {
            // check if the agent on a valid navmesh
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("Mover: Stop called on agent not on a valid NavMesh.");
                return;
            }

            agent.isStopped = true;
        }

        public void Resume()
        {
            // check if the agent on a valid navmesh
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("Mover: Resume called on agent not on a valid NavMesh.");
                return;
            }

            agent.isStopped = false;
        }

        public bool AgentArrived()
        {
            // check if the agent on a valid navmesh
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("Mover: Arrived called on agent not on a valid NavMesh.");
                return false;
            }

            return agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending;
        }

        private void UpdateMover()
        {
            if (!agent.enabled)
                return;

            if (!agent.isStopped && movementTarget != null)
            {
                MoveTo(movementTarget.transform.position);
            }
            else if (!agent.isStopped && movementTargetPosition != Vector3.zero)
            {
                MoveTo(movementTargetPosition);
            }
        }

        void Update()
        {
            UpdateMover();
        }
    }
}