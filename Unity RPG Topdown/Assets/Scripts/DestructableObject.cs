using System.Collections;
using UnityEngine;

public enum ObjectType { Wall, Object }

public class DestructableObject : MonoBehaviour
{
    [SerializeField] GameObject wallPhase1;
    [SerializeField] GameObject wallPhase2;

    [SerializeField] Transform tempObjects;

    BoxCollider2D bc;

    public ObjectType type;

    private IEnumerator triggerCR = null;

    void Start()
    {
        tempObjects = transform.Find("TempObjects");
        bc = GetComponent<BoxCollider2D>();
    }

    IEnumerator TriggerCR()
    {
        Debug.Log("trigger");
        if (wallPhase1 == null)
        {
            //Destruction of second phase
            GameObject.Destroy(wallPhase2);
        }

        if (wallPhase1 != null)
        {
            //Destruction of first phase
            Destroy(tempObjects.gameObject);
            GameObject.Destroy(wallPhase1);
        }
        yield return new WaitForEndOfFrame();
        if (wallPhase2 == null)
        {
            bc.enabled = false;           
        }

        triggerCR = null;
    }

    public void Trigger()
    {
        if (triggerCR == null)
        {
            triggerCR = TriggerCR();
            StartCoroutine(triggerCR);
        }
    }

}
