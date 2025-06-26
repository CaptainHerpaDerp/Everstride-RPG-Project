
#if UNITY_EDITOR

using Characters;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CombatAITests
{
    [UnityTest]
    public IEnumerator Test1v1CombatTrial()
    {
        Debug.Log("Test Started");

        List<RunData> runDataList = new List<RunData>();

        // Set the random seed for reproducibility
        UnityEngine.Random.InitState(123456);

        for (int i = 0; i < 5; i++)
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

            yield return new WaitForFixedUpdate();

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

            yield return new WaitForFixedUpdate();
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
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string version = "A1"; // or dynamically get from test config
        string testType = "1v1";
        string fileName = $"CombatResults_{version}_{testType}_{timestamp}.csv";
        string filePath = Path.Combine(Application.dataPath, "TestResults", fileName);

        string folderPath = Path.Combine(Application.dataPath, "TestResults");
        Directory.CreateDirectory(folderPath); // Ensure folder exists

        string fullFilePath = Path.Combine(folderPath, fileName);

        using (var writer = new StreamWriter(fullFilePath))
        {
            writer.WriteLine($"# AI Version: {version}");
            writer.WriteLine($"# Test Type: {testType}");
            writer.WriteLine($"# Time: {timestamp}");

            writer.WriteLine("Run Index, Winner,Elapsed time,A Damage,B Damage,A Light Attacks,B Light Attacks,A Heavy Attacks,B Heavy Attacks,A Successful Blocks,B Successful Blocks,A Failed Blocks,B Failed Blocks");

            for (int i = 0; i < runs.Count; i++)
            {
                RunData run = runs[i];

                writer.WriteLine($"{i},{run.result},{run.elapsedTime}," +  // Add the comma here
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