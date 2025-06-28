using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Behaviour
{
    public class UtilityAIWeightsContainer : MonoBehaviour
    {
        #region Move To Attack Fields

        [FoldoutGroup("Move To Attack"), Range(0f, 1f), LabelText("Recent Hit"), OnValueChanged("RebalanceMTAFields", includeChildren: false)]
        public float moveToAttack_recentHit = 0.1157351f;

        [FoldoutGroup("Move To Attack"), Range(0f, 1f), LabelText("Health Diff"), OnValueChanged("RebalanceMTAFields", includeChildren: false)]
        public float moveToAttack_healthDiff = 0.2500196f;

        [FoldoutGroup("Move To Attack"), Range(0f, 1f), LabelText("Stamina Diff"), OnValueChanged("RebalanceMTAFields", includeChildren: false)]
        public float moveToAttack_stamDiff = 0.2177424f;

        [FoldoutGroup("Move To Attack"), Range(0f, 1f), LabelText("Stamina Ready"), OnValueChanged("RebalanceMTAFields", includeChildren: false)]
        public float moveToAttack_stamReady = 0.1901557f;

        #endregion

        #region Combat Stance Fields

        [FoldoutGroup("Combat Stance"), Range(0f, 1f), LabelText("Recent Hit"), OnValueChanged("RebalanceCSFields", includeChildren: false)]
        public float combatStance_recentHit = 0.2500196f;

        [FoldoutGroup("Combat Stance"), Range(0f, 1f), LabelText("Health Diff"), OnValueChanged("RebalanceCSFields", includeChildren: false)]
        public float combatStance_healthDiff = 0.1157351f;

        [FoldoutGroup("Combat Stance"), Range(0f, 1f), LabelText("Stamina Diff"), OnValueChanged("RebalanceCSFields", includeChildren: false)]
        public float combatStance_stamDiff = 0.2263472f;

        [FoldoutGroup("Combat Stance"), Range(0f, 1f), LabelText("Stamina Ready"), OnValueChanged("RebalanceCSFields", includeChildren: false)]
        public float combatStance_stamReady = 0.2177424f;

        #endregion

        #region Light Attack Fields

        [FoldoutGroup("Light Attack"), Range(0f, 1f), LabelText("Proximity Weight"), OnValueChanged("RebalanceLAFields", includeChildren: false)]
        public float lightAttack_proximityWeight = 0.6f;
        [FoldoutGroup("Light Attack"), Range(0f, 1f), LabelText("Low Stamina Bonus"), Tooltip("The lower the stamina, the more it makes sense to attack with a light attack, if between 0-1 between 25%-40% stamina, 1-0 between 40% and 75% stamina"), OnValueChanged("RebalanceLAFields", includeChildren: false)]
        public float lightAttack_staminaLightBonus = 0.4f;
        [FoldoutGroup("Light Attack"), Range(0f, 1f), LabelText("Health Difference Bonus"), Tooltip("The lower the health, the less we want to attack and would rather back off"), OnValueChanged("RebalanceLAFields", includeChildren: false)]
        public float lightAttack_healthDiffBonus = 0f;
        [FoldoutGroup("Light Attack"), Range(0f, 1f), LabelText("Enemy Low Stamina Bonus"), Tooltip("The lower the health, the less we want to attack and would rather back off"), OnValueChanged("RebalanceLAFields", includeChildren: false)]
        public float lightAttack_enemyLowStaminaBonus = 0f;

        #endregion

        #region Attack Block Fields

        [FoldoutGroup("Attack Block"), Range(0f, 1f), LabelText("Stamina Weight"), Tooltip("The more stamina we have, the safer it is to block"), OnValueChanged("RebalanceAttackBlockFields", includeChildren: false)]
        public float block_staminaWeight = 0.6f;
        [FoldoutGroup("Attack Block"), Range(0f, 1f), LabelText("Distance Weight"), Tooltip("The closer we are to the target, the more it makes sense to block rather than to dodge"), OnValueChanged("RebalanceAttackBlockFields", includeChildren: false)]
        public float block_distanceWeight = 0.4f;
        [FoldoutGroup("Attack Block"), Range(0f, 1f), LabelText("Health Weight"), OnValueChanged("RebalanceAttackBlockFields", includeChildren: false)]
        public float block_lowHealthWeight = 0f;

        #endregion

        #region Dodge Fields

        [FoldoutGroup("Dodging"), Range(0f, 1f), LabelText("Safe Area Weight"), OnValueChanged("RebalanceDodgeFields", includeChildren: false)]
        public float dodge_inSafeAreaWeight = 0.6f;
        [FoldoutGroup("Dodging"), Range(0f, 1f), LabelText("Abundant Stamina Weight"), Tooltip("The more stamina we have, the safer it is to probably just block"), OnValueChanged("RebalanceDodgeFields", includeChildren: false)]
        public float dodge_staminaAbundanceWeight = 0.4f;
        [FoldoutGroup("Dodging"), Range(0f, 1f), LabelText("Enemy Heavy Attack"), OnValueChanged("RebalanceDodgeFields", includeChildren: false)]
        public float dodge_enemyHeavyAttackWeight = 0f;
        [FoldoutGroup("Dodging"), Range(0f, 1f), LabelText("Abundant Health Weight"), OnValueChanged("RebalanceDodgeFields", includeChildren: false)]
        public float dodge_healthAbundantWeight = 0f;

        #endregion

        #region Passive Block Fields

        [FoldoutGroup("Passive Block"), Range(0f, 1f), LabelText("Target Proximity Weight"), OnValueChanged("RebalancePassiveBlockFields", includeChildren: false)]
        public float passiveBlock_lowDistanceWeight      = 0.6f;
        [FoldoutGroup("Passive Block"), Range(0f, 1f), LabelText("Low Health Weight"), OnValueChanged("RebalancePassiveBlockFields", includeChildren: false)]
        public float passiveBlock_lowHealthWeight = 0.4f;
        [FoldoutGroup("Passive Block"), Range(0f, 1f), LabelText("Low Stamina Weight"), OnValueChanged("RebalancePassiveBlockFields", includeChildren: false)]
        public float passiveBlock_lowStaminaWeight = 0f;

        #endregion

        #region Start Heavy Attack Fields

        [FoldoutGroup("Heavy Attack"), Range(0f, 1f), LabelText("Proximity Bonus"), OnValueChanged("RebalanceHeavyAttackFields", includeChildren: false)]
        public float heavyAttack_proximityWeight = 0.6f;
        [FoldoutGroup("Heavy Attack"), Range(0f, 1f), LabelText("Low Enemy Health Penalty"), OnValueChanged("RebalanceHeavyAttackFields", includeChildren: false)]
        public float heavyAttack_lowEnemyHealthWeight = 0.4f;
        [FoldoutGroup("Heavy Attack"), Range(0f, 1f), LabelText("High Stamina Bonus"), OnValueChanged("RebalanceHeavyAttackFields", includeChildren: false)]
        public float heavyAttack_highStaminaWeight = 0f;
        [FoldoutGroup("Heavy Attack"), Range(0f, 1f), LabelText("Low Health Penalty"), OnValueChanged("RebalanceHeavyAttackFields", includeChildren: false)]
        public float heavyAttack_lowHealthWeight = 0f;

        #endregion

        #region Release Heavy Attack Fields

        [FoldoutGroup("Release Heavy Attack"), Range(0f, 1f), LabelText("Hold Percent Bonus"), OnValueChanged("RebalanceReleaseHeavyAttackFields", includeChildren: false)]
        public float releaseHeavyAttack_holdPercentWeight = 0.6f;
        [FoldoutGroup("Release Heavy Attack"), Range(0f, 1f), LabelText("Enemy Distance Bonus"), OnValueChanged("RebalanceReleaseHeavyAttackFields", includeChildren: false)]
        public float releaseHeavyAttack_enemyDistanceWeight = 0.4f;
        [FoldoutGroup("Release Heavy Attack"), Range(0f, 1f), LabelText("Enemy Charging Heavy Bonus"), OnValueChanged("RebalanceReleaseHeavyAttackFields", includeChildren: false)]
        public float releaseHeavyAttack_enemyChargingHeavyAttackWeight = 0f;
        [FoldoutGroup("Release Heavy Attack"), Range(0f, 1f), LabelText("Enemy Attack Block Bonus"), OnValueChanged("RebalanceReleaseHeavyAttackFields", includeChildren: false)]
        public float releaseHeavyAttack_enemyAttackBlockWeight = 0f;

        #endregion

        private bool _isRebalancing = false; // Prevents infinite loop when rebalancing weights

        #region Indivudual Rebalance Methods

        protected void RebalanceMTAFields()
        {
            List<float> currentFields = new()
            {
                moveToAttack_recentHit,
                moveToAttack_healthDiff,
                moveToAttack_stamDiff,
                moveToAttack_stamReady
            };

            List<float> newFields = Rebalance(currentFields);

            if (newFields == null)
            {
                return;
            }

            moveToAttack_recentHit = newFields[0];
            moveToAttack_healthDiff = newFields[1];
            moveToAttack_stamDiff = newFields[2];
            moveToAttack_stamReady = newFields[3];
        }

        protected void RebalanceCSFields()
        {
            List<float> currentFields = new()
            {
                combatStance_recentHit,
                combatStance_healthDiff,
                combatStance_stamDiff,
                combatStance_stamReady,
            };

            List<float> newFields = Rebalance(currentFields);

            if (newFields == null)
            {
                return;
            }

            combatStance_recentHit = newFields[0];
            combatStance_healthDiff = newFields[1];
            combatStance_stamDiff = newFields[2];
            combatStance_stamReady = newFields[3];
        }

        protected void RebalanceLAFields()
            {

            List<float> currentFields = new()
            {
                lightAttack_proximityWeight,
                lightAttack_staminaLightBonus,
                lightAttack_healthDiffBonus,
                lightAttack_enemyLowStaminaBonus
            };

            List<float> newFields = Rebalance(currentFields);

            if (newFields == null)
            {
                return;
            }

            lightAttack_proximityWeight = newFields[0];
            lightAttack_staminaLightBonus = newFields[1];
            lightAttack_healthDiffBonus = newFields[2];
            lightAttack_enemyLowStaminaBonus = newFields[3];
        }

        public void RebalanceAttackBlockFields()
        {
            List<float> currentFields = new()
            {
                block_staminaWeight,
                block_distanceWeight,
                block_lowHealthWeight
            };
            List<float> newFields = Rebalance(currentFields);
            if (newFields == null)
            {
                return;
            }
            block_staminaWeight = newFields[0];
            block_distanceWeight = newFields[1];
            block_lowHealthWeight = newFields[2];
        }

        public void RebalancePassiveBlockFields()
            {
            List<float> currentFields = new()
            {
                passiveBlock_lowDistanceWeight,
                passiveBlock_lowHealthWeight,
                passiveBlock_lowStaminaWeight
            };
            List<float> newFields = Rebalance(currentFields);
            if (newFields == null)
            {
                return;
            }
            passiveBlock_lowDistanceWeight = newFields[0];
            passiveBlock_lowHealthWeight = newFields[1];
            passiveBlock_lowStaminaWeight = newFields[2];
        }

        public void RebalanceDodgeFields()
        {
            List<float> currentFields = new()
            {
                dodge_inSafeAreaWeight,
                dodge_staminaAbundanceWeight,
                dodge_enemyHeavyAttackWeight,
                dodge_healthAbundantWeight
            };
            List<float> newFields = Rebalance(currentFields);
            if (newFields == null)
            {
                return;
            }
            dodge_inSafeAreaWeight = newFields[0];
            dodge_staminaAbundanceWeight = newFields[1];
            dodge_enemyHeavyAttackWeight = newFields[2];
            dodge_healthAbundantWeight = newFields[3];
        }

        public void RebalanceHeavyAttackFields()
        {
            List<float> currentFields = new()
            {
                heavyAttack_proximityWeight,
                heavyAttack_lowEnemyHealthWeight,
                heavyAttack_highStaminaWeight,
                heavyAttack_lowHealthWeight
            };
            List<float> newFields = Rebalance(currentFields);
            if (newFields == null)
            {
                return;
            }
            heavyAttack_proximityWeight = newFields[0];
            heavyAttack_lowEnemyHealthWeight = newFields[1];
            heavyAttack_highStaminaWeight = newFields[2];
            heavyAttack_lowHealthWeight = newFields[3];
        }

        public void RebalanceReleaseHeavyAttackFields()
        {
            List<float> currentFields = new()
            {
                releaseHeavyAttack_holdPercentWeight,
                releaseHeavyAttack_enemyDistanceWeight,
                releaseHeavyAttack_enemyChargingHeavyAttackWeight,
                releaseHeavyAttack_enemyAttackBlockWeight
            };
            List<float> newFields = Rebalance(currentFields);
            if (newFields == null)
            {
                return;
            }
            releaseHeavyAttack_holdPercentWeight = newFields[0];
            releaseHeavyAttack_enemyDistanceWeight = newFields[1];
            releaseHeavyAttack_enemyChargingHeavyAttackWeight = newFields[2];
            releaseHeavyAttack_enemyAttackBlockWeight = newFields[3];
        }

        #endregion

        #region Utilities

        protected List<float> Rebalance(List<float> weights)
        {
            // Prevent infinite loop when we change values inside the method
            if (_isRebalancing)
                return null;

            _isRebalancing = true;

            // Total of the others
            float totalOthers = 0;

            foreach (float weight in weights)
            {
                totalOthers += weight;
            }

            if (totalOthers == 0f) { weights[0] = 1f; _isRebalancing = false; return null; }

            // If total > 1, scale *all* weights so sum = 1
            float scale = 1f / totalOthers;

            for (int i = 0; i < weights.Count; i++)
            {
                weights[i] *= scale;
            }

            _isRebalancing = false;

            return weights;
        }

        #endregion
    }
}