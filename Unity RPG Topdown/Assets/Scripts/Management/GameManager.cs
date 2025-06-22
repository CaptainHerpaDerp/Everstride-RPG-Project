using NPCManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Management
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        private NPCDirectory npcDirectory;

        private void Awake()
        {
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 1;

            if (GameManager.instance != null)
            {
                Destroy(gameObject);
                return;
            }

            else
            {
                instance = this;
            }

            //DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            // Load player data at the start of the game
            npcDirectory = FindObjectOfType<NPCDirectory>();
            npcDirectory.ResetGameState();
        }

        public void SaveGame()
        {
            npcDirectory.SaveAllNPCData();
        }

        public void LoadNPCData()
        {
            npcDirectory = FindObjectOfType<NPCDirectory>();
            npcDirectory.LoadAllNPCData();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LoadNPCData();
        }
    }
}