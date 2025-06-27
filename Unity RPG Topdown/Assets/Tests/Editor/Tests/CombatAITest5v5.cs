
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

public class CombatAITest5v5
{
    [UnityTest]
    public IEnumerator Test1v1CombatTrial()
    {
        Debug.Log("Test Started");

        List<TeamRunData> runDataList = new List<TeamRunData>();

        // Set the random seed for reproducibility
        UnityEngine.Random.InitState(123456);

        int aiCount = 5;

        for (int i = 0; i < 25; i++)
        {
            // Load test scene
            yield return SceneManager.LoadSceneAsync("CombatTestScene", LoadSceneMode.Single);

            List<NPC> aiAList = new();
            List<NPC> aiBList = new();

            for (int j = 0; j < aiCount; j++)
            {
                var newAIA = GameObject.Instantiate(Resources.Load<GameObject>("AI Dummies/Base AI Dummy"));
                aiAList.Add(newAIA.GetComponent<NPC>());

                newAIA.transform.position = new Vector3(-5, j); // Spread out AIs

                var newAIB = GameObject.Instantiate(Resources.Load<GameObject>("AI Dummies/Utility AI Dummy"));
                aiBList.Add(newAIB.GetComponent<NPC>());

                newAIB.transform.position = new Vector3(5, j); // Spread out AIs
            }

            yield return new WaitForFixedUpdate();

            // Set the game speed to 5x
            Time.timeScale = 10f;

            float maxDuration = 30000;
            float elapsedTime = 0f;

            while (elapsedTime < maxDuration)
            {
                if (GetCombinedHealthOfTeam(aiAList) <= 0 || GetCombinedHealthOfTeam(aiBList) <= 0)
                    break;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            string result;
            if (GetCombinedHealthOfTeam(aiAList) <= 0 && GetCombinedHealthOfTeam(aiBList) <= 0)
                result = "Draw";
            else if (GetCombinedHealthOfTeam(aiAList) <= 0)
                result = "Utility AI";
            else if (GetCombinedHealthOfTeam(aiBList) <= 0)
                result = "Base AI";
            else
                result = "No winner (Timeout)";

            TeamRunData runData = new TeamRunData
            {
                result = result,
                aiAStats = GetCombatStatsFromTeam(aiAList),
                aiBStats = GetCombatStatsFromTeam(aiBList),
                elapsedTime = elapsedTime
            };

            runDataList.Add(runData);

            yield return new WaitForFixedUpdate();
        }

        WriteTestResults(runDataList);

        Assert.Pass();
    }

    public float GetCombinedHealthOfTeam(List<NPC> team)
    {
        float sumHealth = 0f;

        foreach (var npc in team)
        {
            if (npc == null || npc.CurrentHealth <= 0)
                continue; // Skip dead or null NPCs

            sumHealth += npc.CurrentHealth;
        }

        return sumHealth;
    }

    public List<CombatStats> GetCombatStatsFromTeam(List<NPC> team)
    {
        List<CombatStats> statsList = new List<CombatStats>();
        foreach (var npc in team)
        {
            if (npc == null)
            {
                Debug.LogError("NPC is null in GetCombatStatsFromTeam, this should not happen!");
                continue; // Skip null NPCs
            }
            statsList.Add(npc.combatStats);
        }
        return statsList;
    }

    public struct TeamRunData
    {
        public string result;
        public List<CombatStats> aiAStats;
        public List<CombatStats> aiBStats;
        public float elapsedTime;

        public TeamRunData(string result, List<CombatStats> aiAStats, List<CombatStats> aiBStats, float elapsedTime)
        {
            this.result = result;
            this.aiAStats = aiAStats;
            this.aiBStats = aiBStats;
            this.elapsedTime = elapsedTime;
        }

        public float GetTotalDamageDealt(int team)
        {
            List<CombatStats> statsList = team == 0 ? aiAStats : aiBStats;

            float totalDamage = 0f;
            foreach (var stats in statsList)
            {
                totalDamage += stats.TotalDamageDealt;
            }
            return totalDamage;
        }

        public int GetTotalLightAttacks(int team)
        {
            List<CombatStats> statsList = team == 0 ? aiAStats : aiBStats;

            int totalLightAttacks = 0;
            foreach (var stats in statsList)
            {
                totalLightAttacks += stats.LightAttacks;
            }
            return totalLightAttacks;
        }

        public int GetTotalHeavyAttacks(int team)
        {
            List<CombatStats> statsList = team == 0 ? aiAStats : aiBStats;

            int totalHeavyAttacks = 0;
            foreach (var stats in statsList)
            {
                totalHeavyAttacks += stats.HeavyAttacks;
            }
            return totalHeavyAttacks;
        }

        public int GetTotalSuccessfulBlocks(int team)
        {
            List<CombatStats> statsList = team == 0 ? aiAStats : aiBStats;

            int totalSuccessfulBlocks = 0;
            foreach (var stats in statsList)
            {
                totalSuccessfulBlocks += stats.SuccessfulBlocks;
            }
            return totalSuccessfulBlocks;
        }

        public int GetTotalFailedBlocks(int team)
        {
            List<CombatStats> statsList = team == 0 ? aiAStats : aiBStats;

            int totalFailedBlocks = 0;
            foreach (var stats in statsList)
            {
                totalFailedBlocks += stats.FailedBlocks;
            }
            return totalFailedBlocks;
        }
    }

    public void WriteTestResults(List<TeamRunData> runs)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string version = "A1"; 
        string testType = "5v5";
        string fileName = $"CombatResults_{version}_{testType}_{timestamp}.csv";
        string filePath = Path.Combine(Application.dataPath, "TestResults", fileName);

        string folderPath = Path.Combine(Application.dataPath, "TestResults");
        Directory.CreateDirectory(folderPath); 

        string fullFilePath = Path.Combine(folderPath, fileName);

        using (var writer = new StreamWriter(fullFilePath))
        {
            writer.WriteLine($"# AI Version: {version}");
            writer.WriteLine($"# Test Type: {testType}");
            writer.WriteLine($"# Time: {timestamp}");

            writer.WriteLine("Run Index, Winner,Elapsed time,A Damage,B Damage,A Light Attacks,B Light Attacks,A Heavy Attacks,B Heavy Attacks,A Successful Blocks,B Successful Blocks,A Failed Blocks,B Failed Blocks");

            for (int i = 0; i < runs.Count; i++)
            {
                TeamRunData run = runs[i];

                writer.WriteLine($"{i},{run.result},{run.elapsedTime}," +  // Add the comma here
         $"{run.GetTotalDamageDealt(0)},{run.GetTotalDamageDealt(1)}," +
         $"{run.GetTotalLightAttacks(0)},{run.GetTotalLightAttacks(1)}," +
         $"{run.GetTotalHeavyAttacks(0)},{run.GetTotalHeavyAttacks(1)}," +
         $"{run.GetTotalSuccessfulBlocks(0)},{run.GetTotalSuccessfulBlocks(1)}," +
         $"{run.GetTotalFailedBlocks(0)},{run.GetTotalFailedBlocks(1)}");


            }
        }

        AssetDatabase.Refresh(); // if you want to see it appear in the Editor
    }

}

#endif