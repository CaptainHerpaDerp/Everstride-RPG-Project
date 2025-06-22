using UnityEngine;
using UnityEngine.AI;

namespace Characters
{
    public class Mover : MonoBehaviour
    {
        [SerializeField] Transform target;
        private Vector3 targetPosition;
        public NavMeshAgent agent;
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            Vector3 velocity = agent.velocity;
        }

        public void SetTarget(Transform targetTransform)
        {
            targetPosition = Vector3.zero;
            target = targetTransform;
        }

        public void SetTarget(Vector3 position)
        {
            target = null;
            targetPosition = position;
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
            agent.isStopped = true;
        }

        public void Resume()
        {
            agent.isStopped = false;
        }

        public bool AgentArrived()
        {
            return agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending;
        }

        private void UpdateMover()
        {
            if (!agent.enabled)
                return;

            if (!agent.isStopped && target != null)
            {
                MoveTo(target.transform.position);
            }
            else if (!agent.isStopped && targetPosition != Vector3.zero)
            {
                MoveTo(targetPosition);
            }
        }

        void Update()
        {
            UpdateMover();
        }
    }
}