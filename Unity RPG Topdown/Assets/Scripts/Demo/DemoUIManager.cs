using Characters;
using UnityEngine;

public class DemoUIManager : MonoBehaviour
{
    private Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    [SerializeField] Vector3 TownTP;
    [SerializeField] Vector3 CaveTP;

    [SerializeField] private GameObject townTransform, caveTransform;

    [SerializeField] private GameObject enemyParentTransform;

    public void TeleportTown()
    {
        townTransform.SetActive(true);
        player.transform.position = TownTP;
        player.DisableTorch();
        caveTransform.SetActive(false);
    }

    public void TeleportCave()
    {
        caveTransform.SetActive(true);
        player.transform.position = CaveTP;
        player.EnableTorch();
        townTransform.SetActive(false);
    }

    public void EnableEnemies()
    {
        enemyParentTransform.SetActive(true);
    }

    public void DisableEnemies()
    {
        enemyParentTransform.SetActive(false);
    }
}
