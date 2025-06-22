using UnityEngine;
using Interactables;
using Characters;

public class AreaTransporter : Collidable
{
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private AreaTransporter targetArea;

    [Header("The Transform that will be enabled when the player enters this transporter")]
    [SerializeField] private Transform areaToLoad;

    [Header("The area that will be deactiated when the player enters this transporter")]
    [SerializeField] private Transform linkedArea;

    // Temp 
    [SerializeField] private bool torchStatusOnEntry = false;

    protected override void Start()
    {
        //if (areaToLoad != null)
        //{
        //    areaToLoad.gameObject.SetActive(false);
        //}

        base.Start();
    }

    protected override void OnCollide(Collider2D collider)
    {
        if (collider == null)
            return;

        if (collider.name == "Player")
        {
            // Enables the area the player is going to
            if (areaToLoad != null)
            {
                areaToLoad.gameObject.SetActive(true);
            }

            // Disables the area that the player is coming from
            if (linkedArea != null)
            {
                linkedArea.gameObject.SetActive(false);
            }

            // Temp
            if (torchStatusOnEntry == true)
            {
                collider.GetComponent<Player>().EnableTorch();
            }
            else
            {
                collider.GetComponent<Player>().DisableTorch();
            }

            collider.transform.position = targetArea.spawnPoint.position;
        }
    }
}
