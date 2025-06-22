using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Characters;
using DataSave;

namespace NPCManagement
{
    public class NPCDirectory : MonoBehaviour
    {
        public static NPCDirectory Instance;

        public List<string> LivingNPCDirectory = new();
        public List<string> DeadNPCDirectory { get; private set; } = new();

        [HideInInspector] public UnityEvent<NPC> NPCDeathEvent;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("Multiple instances of NPCDirectory are present!");
                Destroy(gameObject);
                return;
            }

            NPC.OnNPCCreated += AddNPC;
        }

        public void AddNPC(NPC npc)
        {
            if (!npc.HasID() || npc == null)
                return;

            Debug.Log("NPC Added: " + npc.ID);

            // Adds a new NPC ID to the directory if it is not already present
            if (!LivingNPCDirectory.Contains(npc.ID))
            {
                LivingNPCDirectory.Add(npc.ID);
            }

            npc.OnDeath += () => RemoveNPC(npc.ID);
        }

        public void RemoveNPC(string npcID)
        {
            // An event is null if there are no subscribers, thus a question mark is added to the end of the event
            NPCDeathEvent?.Invoke(GetLivingNPCWithID(npcID));

            LivingNPCDirectory.Remove(npcID);

            DeadNPCDirectory.Add(npcID);

            // Sets the NPC's Data to Dead.

            // TODO: Possible problem with 
            NPCData npcData = SaveLoadManager.LoadNPCData<NPCData>(npcID);
            if (npcData != null)
            {
                // Modify the desired property, for example:
                npcData.isDead = true;

                // Save the updated data
                SaveLoadManager.SaveNPCData(npcData, npcID);
            }
        }

        public NPC GetLivingNPCWithID(string ID)
        {
            foreach (string npID in LivingNPCDirectory)
            {
                if (npID == ID)
                {
                    // Iterate through all NPCs in the scene to find the one with the matching ID
                    NPC[] npcsInScene = FindObjectsOfType<NPC>();

                    foreach (NPC npc in npcsInScene)
                    {
                        if (npc.ID == ID)
                        {
                            // Found the NPC with the matching ID
                            return npc;
                        }
                    }
                }
            }

            return null;
        }

        public NPC GetDeadNPCWithID(string ID)
        {
            foreach (string npID in DeadNPCDirectory)
            {
                if (npID == ID)
                {
                    // Iterate through all NPCs in the scene to find the one with the matching ID
                    NPC[] npcsInScene = FindObjectsOfType<NPC>();

                    foreach (NPC npc in npcsInScene)
                    {
                        if (npc.ID == ID)
                        {
                            // Found the NPC with the matching ID
                            return npc;
                        }
                    }
                }
            }

            return null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                SaveAllNPCData();
            }

            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                LoadAllNPCData();
            }

            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                ResetGameState();
            }
        }

        #region Utilities

        private List<string> GetAllNPCs()
        {
            List<string> list = new List<string>();

            foreach (var item in LivingNPCDirectory)
            {
                list.Add(item);
            }

            foreach (var item in DeadNPCDirectory)
            {
                list.Add(item);
            }

            return list;
        }

        public void ResetGameState()
        {
            foreach (string npcID in GetAllNPCs())
            {
                // Load NPC data from SaveLoadManager
                NPCData npcData = SaveLoadManager.LoadNPCData<NPCData>(npcID);

                if (npcData != null)
                {
                    npcData.ResetToDefault();
                    SaveLoadManager.SaveNPCData(npcData, npcID);
                }
            }

            LoadAllNPCData();
        }

        public void SaveAllNPCData()
        {
            //Debug.LogError("NPC Data Saving to be implemented!!");

            //foreach (string npcID in LivingNPCDirectory)
            //{
            //    NPC npc = GetLivingNPCWithID(npcID);

            //    if (npc == null)
            //    {
            //        continue;
            //    }

            //    Dialogue npcDialogueComponent;

            //    // Create an NPCData object to store the NPC's data
            //    NPCData npcData = new()
            //    {
            //        currentDialogueGroup = npcDialogueComponent.GetCurrentDialogueGroupName(),
            //        isDead = !npc.isActiveAndEnabled
            //    };

            //    // Save the NPC data
            //    SaveLoadManager.SaveNPCData(npcData, npc.ID);
            //}
        }

        public void LoadAllNPCData()
        {
           // Debug.LogError("NPC Data Loading to be implemented!!");

            //foreach (string npcID in LivingNPCDirectory)
            //{
            //    // Load NPC data from SaveLoadManager
            //    NPCData npcData = SaveLoadManager.LoadNPCData(npcID) as NPCData;

            //    if (npcData != null)
            //    {
            //        // Find or spawn the NPC with the matching ID
            //        NPC npc = GetLivingNPCWithID(npcID);

            //        // NPC data matches an NPC in the scene
            //        if (npc != null)
            //        {
            //            Dialogue npcDialogueComponent;

            //            if (!npc.TryGetComponent(out npcDialogueComponent))
            //            {
            //                Debug.LogError("Problem while saving NPC data, no dialogue component found within NPC");
            //            }

            //            npcDialogueComponent.SetCurrentDialogueGroup(npcData.currentDialogueGroup);
            //            npc.gameObject.SetActive(!npcData.isDead);

            //            // Update other NPC data fields as needed
            //        }
            //    }
            //}
        }

        #endregion
    }
}