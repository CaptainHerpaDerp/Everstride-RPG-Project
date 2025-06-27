using Codice.CM.Client.Differences;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace CustomOdinScripts
{
    public abstract class WeightSlidersOdin : ScriptableObject
    {
        protected List<float> weights;
        protected bool _isRebalancing;

        protected virtual void Rebalance()
        {
            // Prevent infinite loop when we change values inside the method
            if (_isRebalancing)
                return;

            _isRebalancing = true;

            // Total of the others
            float totalOthers = 0;

            foreach (float weight in weights)
            {
                totalOthers += weight;
            }

            if (totalOthers == 0f) { weights[0] = 1f; _isRebalancing = false; return; }

            // If total > 1, scale *all* weights so sum = 1
            float scale = 1f / totalOthers;

            for (int i = 0; i < weights.Count; i++)
            {
                weights[i] *= scale;
            }

            _isRebalancing = false;
        }

        protected virtual float GetScale()
        {
            // Total of the others
            float totalOthers = 0;

            foreach (float weight in weights)
            {
                totalOthers += weight;
            }

            // If total > 1, scale *all* weights so sum = 1
            float scale = 1f / totalOthers;

            return scale;
        }

        protected virtual void RebalanceWeights()
        {
            if (weights == null || weights.Count == 0)
            {
                Debug.LogError("New weights list is null or has incorrect size.");
                return;
            }

            float scale = GetScale();

   
        }
    }

    [CreateAssetMenu(menuName = "AI/MoveToAttack Weights (Odin)")]
    public class MoveToAttackWeightsOdin : WeightSlidersOdin
    {
        [Range(0f, 1f), OnValueChanged("Rebalance", includeChildren: false)]
        public float healthDiffWeight = 0;

        [Range(0f, 1f), OnValueChanged("Rebalance", includeChildren: false)]
        public float recentHit = 0;

        [Range(0f, 1f), OnValueChanged("Rebalance", includeChildren: false)]
        public float hpRisk = 0;

        [Range(0f, 1f), OnValueChanged("Rebalance", includeChildren: false)]
        public float stamDiff = 0;

        [Range(0f, 1f), OnValueChanged("Rebalance", includeChildren: false)]
        public float stamReady = 0;

        protected override void Rebalance()
        {
            weights = new List<float>
            {
                healthDiffWeight,
                recentHit,
                hpRisk,
                stamDiff,
                stamReady
            };

            Debug.Log("Rebalancing");

            base.Rebalance();
        }     
    }

    public static class SOCreator
    {

        public static T CreateWeightSlider<T>(string goName) where T : WeightSlidersOdin
        {
#if UNITY_EDITOR
            // 1. make the object in memory
            T asset = ScriptableObject.CreateInstance<T>();

            // 2. (optional) save it as an asset so it persists
            const string folder = "Assets/AI/Weights";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/AI", "Weights");

            string path = AssetDatabase.GenerateUniqueAssetPath(
                              $"{folder}/Weights_{goName}.asset");

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return asset;
#else
        // Play-time build: just keep a non-saved instance
        weights = ScriptableObject.CreateInstance<MoveToAttackWeightsOdin>();
        weights.hideFlags = HideFlags.HideAndDontSave;
#endif
        }
    }
}