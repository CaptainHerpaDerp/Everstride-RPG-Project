
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

public class CombatAITest1v1
{
    private const float MAX_DURATION = 300; // max time for each combat trial

    [UnityTest]
    public IEnumerator Test1v1CombatTrial()
    {
        // Set the random seed for reproducibility
        UnityEngine.Random.InitState(123456);

        int runs = 50;
        List<RunData> runDataList = new List<RunData>();

        for (int i = 0; i < runs; i++)
        {
            float elapsedTime = 0f;

            // Load test scene
            yield return SceneManager.LoadSceneAsync("CombatTestScene", LoadSceneMode.Single);
            yield return null; // wait for scene to load

            var aiA = GameObject.Instantiate(Resources.Load<GameObject>("AI Dummies/Base AI Dummy"));
            var aiB = GameObject.Instantiate(Resources.Load<GameObject>("AI Dummies/Utility AI Dummy"));
            aiA.transform.position = new Vector3(-5, 0);
            aiB.transform.position = new Vector3(5, 0);

            // have the camea follow AI B 
            Camera.main.transform.SetParent(aiB.transform);
            Camera.main.transform.localPosition = new Vector3(0, 10, 2);

            var npcA = aiA.GetComponent<NPC>();
            var npcB = aiB.GetComponent<NPC>();

            Assert.IsNotNull(npcA);
            Assert.IsNotNull(npcB);

            yield return new WaitForSecondsRealtime(0.25f);   // let Awake/Start finish
            Time.timeScale = 10f;

            while (elapsedTime < MAX_DURATION)
            {
                if (!npcA || !npcB ) break;                    
                if (npcA.CurrentHealth <= 0 || npcB.CurrentHealth <= 0) break;

                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            Time.timeScale = 1f;

            // —— determine result ——
            string result;
            if (npcA.CurrentHealth <= 0 && npcB.CurrentHealth <= 0)
                result = "Draw";
            else if (npcA.CurrentHealth <= 0)
                result = "Utility AI";
            else if (npcB.CurrentHealth <= 0)
                result = "Base AI";
            else
                result = "No winner (Timeout)";

            runDataList.Add(new RunData
            {
                result = result,
                aiAStats = npcA.combatStats,
                aiBStats = npcB.combatStats,
                elapsedTime = elapsedTime
            });

            // —— cleanup ——
            Camera.main.transform.SetParent(null);

            GameObject.Destroy(aiA);
            GameObject.Destroy(aiB);
            yield return null; 
            Resources.UnloadUnusedAssets();
            GC.Collect();
            yield return SceneManager.UnloadSceneAsync("CombatTestScene");
            yield return null;

            Debug.Log($"Run {i}: {result}");
        }

        Time.timeScale = 1f;  // restore normal time
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
        string version = "A3"; // or dynamically get from test config
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