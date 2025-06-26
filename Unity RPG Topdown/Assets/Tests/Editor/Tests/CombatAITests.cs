
#if UNITY_EDITOR

using Characters;
using NUnit.Framework;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Collections.Generic;

public class CombatAITests
{
    [UnityTest]
    public IEnumerator Test1v1CombatTrial()
    {
        Debug.Log("Test Started");

        List<RunData> runDataList = new List<RunData>();

        // Set the random seed for reproducibility
        Random.InitState(123456);

        for (int i = 0; i < 50; i++)
        {
            // Load test scene
            yield return SceneManager.LoadSceneAsync("CombatTestScene", LoadSceneMode.Single);

            // Spawn agents
            var aiA = GameObject.Instantiate(Resources.Load<GameObject>("AI Dummies/Base AI Dummy"));
            var aiB = GameObject.Instantiate(Resources.Load<GameObject>("AI Dummies/Utility AI Dummy"));

            aiA.transform.position = new Vector3(-5, 0);
            aiB.transform.position = new Vector3(5, 0);

            NPC npcA = aiA.GetComponent<NPC>();
            NPC npcB = aiB.GetComponent<NPC>();

            Assert.IsNotNull(npcA, "Base AI Dummy prefab does not contain an NPC component.");
            Assert.IsNotNull(npcB, "Utility AI Dummy prefab does not contain an NPC component.");

            yield return new WaitForSeconds(0.25f); // Wait for NPCs to initialize

            // Set the game speed to 5x
            Time.timeScale = 10f;

            float maxDuration = 300; // Fight shouldn't last longer than 5 minutes.. right?
            float elapsedTime = 0f;

            while (elapsedTime < maxDuration)
            {
                if (npcA.CurrentHealth <= 0 || npcB.CurrentHealth <= 0)
                    break;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            string result;
            if (npcA.CurrentHealth <= 0 && npcB.CurrentHealth <= 0)
                result = "Draw";
            else if (npcA.CurrentHealth <= 0)
                result = "Utility AI";
            else if (npcB.CurrentHealth <= 0)
                result = "Base AI";
            else
                result = "No winner (Timeout)";

            var aiAStats = npcA.combatStats;
            var aiBStats = npcB.combatStats;

            RunData runData = new RunData
            {
                result = result,
                aiAStats = aiAStats,
                aiBStats = aiBStats,
                elapsedTime = elapsedTime
            };

            runDataList.Add(runData);

            yield return new WaitForSeconds(0.25f); // Wait a bit before the next trial
        }


        WriteTestResults(runDataList);

        Assert.Pass();
    }

    public struct RunData
    {
        public string result;
        public CombatStats aiAStats;
        public CombatStats aiBStats;
        public float elapsedTime;

        public RunData(string result, CombatStats aiAStats, CombatStats aiBStats, float elapsedTime)
        {
            this.result = result;
            this.aiAStats = aiAStats;
            this.aiBStats = aiBStats;
            this.elapsedTime = elapsedTime;
        }
    }

    public void WriteTestResults(List<RunData> runs)
    {
        using (var writer = new StreamWriter("Assets/TestResults/CombatResults.csv"))
        {
            writer.WriteLine("Winner,Elapsed time,A Damage,B Damage,A Light Attacks,B Light Attacks,A Heavy Attacks,B Heavy Attacks,A Successful Blocks,B Successful Blocks,A Failed Blocks,B Failed Blocks");

            for (int i = 0; i < runs.Count; i++)
            {
                RunData run = runs[i];

                writer.WriteLine($"{run.result},{run.elapsedTime}," +  // Add the comma here
         $"{run.aiAStats.TotalDamageDealt},{run.aiBStats.TotalDamageDealt}," +
         $"{run.aiAStats.LightAttacks},{run.aiBStats.LightAttacks}," +
         $"{run.aiAStats.HeavyAttacks},{run.aiBStats.HeavyAttacks}," +
         $"{run.aiAStats.SuccessfulBlocks},{run.aiBStats.SuccessfulBlocks}," +
         $"{run.aiAStats.FailedBlocks},{run.aiBStats.FailedBlocks}");


            }
        }

        AssetDatabase.Refresh(); // if you want to see it appear in the Editor
    }

}

#endif