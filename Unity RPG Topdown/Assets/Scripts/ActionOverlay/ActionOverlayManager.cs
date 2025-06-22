using UnityEngine;
using Core.Enums;

namespace ActionOverlay
{
    public class ActionOverlayManager : MonoBehaviour
    {
        //public static ActionOverlayManager Instance { get; private set; }

        //[SerializeField] private Vector2 talkActionIconOffset;
        //[SerializeField] private Transform talkIcon;

        //[SerializeField] private Vector2 useActionIconOffset;
        //[SerializeField] private Transform useIcon;

        //[SerializeField] private Vector2 lootActionIconOffset;
        //[SerializeField] private Transform lootIcon;

        //[SerializeField] private Transform iconsParent;

        //private void Awake()
        //{
        //    if (Instance == null)
        //        Instance = this;
        //    else
        //    {
        //        Debug.LogWarning("ActionOverlayManager already exists. Deleting duplicate.");
        //        Destroy(gameObject);
        //    }
        //}

        //private void Start()
        //{
        //    playerFollower = FindObjectOfType<CameraPlayerFollower>();
        //}

        //public void DisplayIconActionAt(Transform target, ActionType actionType)
        //{
        //    HideAllDisplayIcons();

        //    switch (actionType)
        //    {
        //        case ActionType.Talk:
        //            DisplayTalkIconAt(target);
        //            break;
        //        case ActionType.Use:
        //            DisplayUseIconAt(target);
        //            break;
        //        case ActionType.Loot:
        //            DisplayLootIconAt(target);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //private void DisplayTalkIconAt(Transform target)
        //{
        //    talkIcon.gameObject.SetActive(true);
        //    talkIcon.transform.position = Camera.main.WorldToScreenPoint(target.position) + (Vector3)talkActionIconOffset;
        //    talkIcon.transform.localScale = new Vector2(1 + (0.1f * playerFollower.scrollLevel), 1 + (0.1f * playerFollower.scrollLevel));
        //}

        //private void DisplayUseIconAt(Transform target)
        //{
        //    useIcon.gameObject.SetActive(true);
        //    useIcon.transform.position = Camera.main.WorldToScreenPoint(target.position) + (Vector3)useActionIconOffset;
        //    useIcon.transform.localScale = new Vector2(1 + (0.1f * playerFollower.scrollLevel), 1 + (0.1f * playerFollower.scrollLevel));
        //}

        //private void DisplayLootIconAt(Transform target)
        //{
        //    lootIcon.gameObject.SetActive(true);
        //    lootIcon.transform.position = Camera.main.WorldToScreenPoint(target.position) + (Vector3)lootActionIconOffset;
        //    lootIcon.transform.localScale = new Vector2(1 + (0.1f * playerFollower.scrollLevel), 1 + (0.1f * playerFollower.scrollLevel));
        //}


        //public void HideDisplayAction()
        //{
        //    HideAllDisplayIcons();
        //}

        //private void HideAllDisplayIcons()
        //{
        //    if (gameObject.activeInHierarchy)
        //        foreach (Transform child in iconsParent)
        //        {
        //            if (child.gameObject.activeInHierarchy)
        //                child.gameObject.SetActive(false);
        //        }
        //}
    }
}