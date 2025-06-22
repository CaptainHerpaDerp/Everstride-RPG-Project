using UnityEngine;
using Management;

namespace Interactables
{
    public class Collidable : MonoBehaviour
    {
        private ContactFilter2D filter;
        private BoxCollider2D boxCollider;
        private Collider2D[] hits = new Collider2D[10];
        protected GameManager instance;

        protected virtual void Start()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            instance = GameManager.instance;
        }

        protected virtual void Update()
        {
            boxCollider.OverlapCollider(filter, hits);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null || hits[i].name == "Colliders")
                {
                    continue;
                }

                OnCollide(hits[i]);

                hits[i] = null;
            }
        }

        protected virtual void OnCollide(Collider2D collider)
        {
            Debug.Log(collider.name);
        }

    }
}