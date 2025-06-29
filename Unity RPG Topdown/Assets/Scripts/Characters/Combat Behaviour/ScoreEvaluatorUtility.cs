using CBTSystem.Enumerations;
using CBTSystem.ScriptableObjects.Nodes;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters.Behaviour
{
    [RequireComponent(typeof(UtilityAIWeightsContainer))]
    public class ScoreEvaluatorUtility : MonoBehaviour
    {
        [BoxGroup("Component References"), SerializeField] private UtilityAIWeightsContainer weights;
        [BoxGroup("Component References"), SerializeField] private UtilityAICombatBehaviourManager utilityAI;

        [BoxGroup("Scoring Modifiers, Move Attack"), SerializeField] private float lastHitThresholdTime = 3;

        public float EvaluateUtility(CBTActionType action, CombatContext ctx)
        {
            float recentHit = 0;
            float rawHealthDelta = 0;
            float hpDiffFactor = 0;
            float rawStamDelta = 0;
            float stamDiffFactor = 0;
            float stamSufficient = 0;
            float distanceFactor = 0;
            float lowStamina;
            float rise, fall;
            float lowHealth;
            float rawScore = 0;
            float totalResourceDiff = 0; // average of health and stamina difference

            switch (action)
            {
                case CBTActionType.MoveToStanceRadius:

                    // highest when you’re very far
                    return Mathf.Clamp01((ctx.DistanceToTarget - utilityAI.currentDefensiveRadius) / utilityAI.currentDefensiveRadius);

                case CBTActionType.MoveToAttackRange:

                    /**** Considerations ****/

                    /* How is my health, especially relative to the target's health? Can I risk going in for an attack?
                     * Is the target attacking? If so, I should not go in for an attack, as I might get hit.
                     * How is my stamina compared to my enemy's? If it is low, i should probably keep my distance and let my stamina regenerate
                     * How is my health compared to my enemy's? If it is a lot lower, I should definately keep my distance and let my stamina regenerate
                     * If my health is slightly lower than my enemy's but my stamina is much higher, I can probably risk going in for an attack, as I can block or dodge their attacks
                     * How long has it been since I was last hit? If it has been a while, I can be more aggressive and go in for an attack
                     * Is the target holding a heavy attack? If so, I should not go in for an attack, as I might get hit.
                     */



                    //float noRecentHit = High(ctx.TimeSinceLastHit, 3f, 5f);   // 0 → 1
                    //float healthier = High(ctx.CurrentHealthPercentage -
                    //                         ctx.TargetHealthPercentage, 0f, .4f);
                    //float moreStam = High(ctx.CurrentStaminaPercentage -
                    //                         ctx.TargetStaminaPercentage, 0f, .3f);
                    //float staminaOkay = High(ctx.CurrentStaminaPercentage, .3f, .6f);

                    //return noRecentHit * weights.moveToAttack_recentHit +
                    //        healthier * weights.moveToAttack_healthDiff +
                    //        moreStam * weights.moveToAttack_stamDiff +
                    //        staminaOkay * weights.moveToAttack_stamReady;

                    if (ctx.DistanceToTarget < ctx.IdealLightAttackRange)
                    {
                        return 0f; // Already at the target's ideal range, no need to move closer
                    }

                    // Take the time since last hit into account (If we have not been hit in a while, we can be more aggressive)
                    float t = Mathf.InverseLerp(3.0f, 5.0f, ctx.TimeSinceLastHit);
                    //  t = 0  (≤3 s) … 1 (≥5 s)
                    recentHit = Mathf.SmoothStep(0f, 1f, t);

                    // The difference of my health to the target's health into consideration (If my health is lower than the target's, I should be more defensive)
                    rawHealthDelta = ctx.CurrentHealthPercentage - ctx.TargetHealthPercentage; // −1 .. +1
                    hpDiffFactor = Mathf.Clamp01((rawHealthDelta + 1f) * 0.5f);   // 0..1 symmetric
                    // .75 (more)

                    // The difference of my stamina to the target's stamina into consideration (If my stamina is lower than the target's, I should be more defensive)
                    rawStamDelta = ctx.CurrentStaminaPercentage - ctx.TargetStaminaPercentage;
                    stamDiffFactor = Mathf.Clamp01((rawStamDelta + 1f) * 0.5f);
                    // .65 (more)

                    stamSufficient = Mathf.InverseLerp(0.2f, 0.5f, ctx.CurrentStaminaPercentage); // 0.2 is the minimum stamina to attack (score 0), 0.5 will raise the score to 1

                    totalResourceDiff = (hpDiffFactor + stamDiffFactor) / 2f; // average of health and stamina difference

                    float moreResources = ValueHigh(totalResourceDiff, .4f, .7f); // 1 low, 0 high - if the enemy has more resources than us, we should be more defensive

                    // *** Target Attacking *** //

                    // If the target is attacking, we should not go in for an attack
                    if (ctx.SeenIncomingAttack > 0f)
                    {
                        // If the target is attacking, we should not go in for an attack
                        return 0f;
                    }

                    return
                        (recentHit * weights.moveToAttack_recentHit +

                        hpDiffFactor * weights.moveToAttack_healthDiff +

                        stamDiffFactor * weights.moveToAttack_stamDiff +

                        stamSufficient * weights.moveToAttack_stamReady+

                        moreResources
                        );

                // Combat stance is where the character will encircle their target, maintaining their distance while being ready to attack or block
                case CBTActionType.CombatStance:

                    /**** Considerations ****/

                    /* 
                     * If our health is lower than our target, we need to be more defensive, and stay in combat stance
                     * If our stamina is lower than our target, we need to be more defensive, and stay in combat stance
                     * If we were recently hit, we might want to back off
                     */

                    //recentHit = Low(ctx.TimeSinceLastHit, 0f, 3f);      // 1 → 0
                    //float weaker = Low(ctx.CurrentHealthPercentage -
                    //                          ctx.TargetHealthPercentage, -.4f, 0f);
                    //float lessStam = Low(ctx.CurrentStaminaPercentage -
                    //                          ctx.TargetStaminaPercentage, -.3f, 0f);
                    //float lowStamina = Low(ctx.CurrentStaminaPercentage, .2f, .5f);

                    //return recentHit * weights.combatStance_recentHit +
                    //        weaker * weights.combatStance_healthDiff +
                    //        lessStam * weights.combatStance_stamDiff +
                    //        lowStamina * weights.combatStance_stamReady;

                    // The more recently we were hit, the more we want to back off
                    recentHit = 1f - Mathf.InverseLerp(0f, lastHitThresholdTime, ctx.TimeSinceLastHit);

                    // The higher the health of the enemy compared to us, the more we want to back off
                    rawHealthDelta = ctx.TargetHealthPercentage - ctx.CurrentHealthPercentage;
                    hpDiffFactor = Mathf.Clamp01((rawHealthDelta + 1f) * 0.5f);   // 0..1 symmetric

                    rawStamDelta = ctx.TargetStaminaPercentage - ctx.CurrentStaminaPercentage;
                    stamDiffFactor = Mathf.Clamp01((rawStamDelta + 1f) * 0.5f);

                    totalResourceDiff = (hpDiffFactor + stamDiffFactor) / 2f; // average of health and stamina difference

                    stamSufficient = 1f - Mathf.InverseLerp(0.2f, 0.5f, ctx.CurrentStaminaPercentage);

                    rawScore =

                        recentHit *
                        totalResourceDiff *
                        stamSufficient;

                    return ApplyCompensationToScore(rawScore, 3) * weights.combatStance_score;


                    rawScore =

                        recentHit *
                        totalResourceDiff *
                        stamSufficient * weights.combatStance_score;

                    return ApplyCompensationToScore(rawScore, 3);

                //case CBTActionType.InstantBlock:

                //    /**** Considerations ****/

                //    /* 
                //     * The variables to evaluate are if we are relatively close to the target (too far to dodge), 
                //     * if our health is low, we dodging might be too risky
                //     * if our stamina is too low, we might not be able to block the attack (afford the cost)
                //     * if our stamina is at 0 or below 5%, we should not block at all
                //     * if the enemy's stamina is high, and we have both stamina and health to expend, we should block to reduce the enemy's stamina, and fight back later where they can't block anymore
                //     */

                //    //if (ctx.SeenIncomingAttack == 0) return 0f;
                //    //if (ctx.CurrentStaminaPercentage < .1f) return 0f;   // avoid stagger

                //    //float veryClose = Low(ctx.DistanceToTarget, 0f, ctx.TargetAttackRange);
                //    //float stamEnough = High(ctx.CurrentStaminaPercentage, .25f, .5f);

                //    //return veryClose * weights.block_staminaCheapWeight +
                //    //        stamEnough * weights.block_dangerScoreWeight;

                //    // (gated first)
                //    if (ctx.SeenIncomingAttack == 0) return 0f;
                //    if (ctx.CurrentStaminaPercentage < 0.05f) return 0f;     // avoid stagger

                //    float lowDistance = ValueLow(ctx.DistanceToTarget, 0, ctx.TargetAttackRange); // 1 close, 0 far
                //    float staminaEnough = ValueHigh(ctx.CurrentStaminaPercentage, 0.25f, 0.5f); // 1 high, 0 low

                //    return
                //         (lowDistance * weights.block_staminaCheapWeight +
                //         staminaEnough * weights.block_dangerScoreWeight) * riskAversion;

                case CBTActionType.HoldBlock:

                   // Only consider blocking when an incoming attack is actually detected
                    if (ctx.SeenIncomingAttack <= 0f)
                        return 0f;

                    // Hard-gate: avoid blocking if stamina is too low to hold (below 10%).
                    if (ctx.CurrentStaminaPercentage < 0.1f)
                        return 0f;

                    if (ctx.DistanceToTarget > ctx.TargetAttackRange * 1.1f)
                        return 0f; // Don't block if too far away.

                    float dangerScore = ValueHigh(ctx.IncomingDamage, ctx.TargetLightAttackDamage, ctx.TargetMinHeavyAttackDamage);

                    // Find out how much stamina we will lose when blocking the attack
                    float staminaCost = (ctx.MaxStamina * (ctx.StaminaPercDrainPerBlock * ctx.TargetCurHeavyAttackMultiplierDrain)) / ctx.MaxStamina;
                    float staminaCheap = 1f - staminaCost;

                    float healthHigher = ValueHigh(ctx.CurrentHealthPercentage -
                            ctx.TargetHealthPercentage, 0f, .3f);

                    // 1 when trading is ok, 0 when risky
                    float oppCheap = 1f - healthHigher;

                    // Combine weighted factors and apply risk-aversion multiplier.
                    rawScore =
                        (dangerScore *
                         staminaCheap *
                         oppCheap);

                    return ApplyCompensationToScore(rawScore, 3) * weights.block_master;

                case CBTActionType.DodgeAttack:

                    /**** Considerations ****/

                    /* 
                     * The variables to evaluate are if we are too close to the target (low chance to successfly move out of the way),
                     * depending on the speed of the target weapon's base attack, we might not be able to dodge in time
                     * if the target is performing a heavy attack, we have a higher chance of being able to dodge
                     * the lower our stamina, the higher our favorability to dodge, as we cannot block
                     */

                    //if (ctx.SeenIncomingAttack == 0) return 0f;

                    //float safeZone = High(ctx.DistanceToTarget,
                    //                      ctx.TargetAttackRange * .8f, ctx.TargetAttackRange);
                    //float stamLow = Low(ctx.CurrentStaminaPercentage, 0f, .25f);

                    //return safeZone * weights.dodge_inSafeAreaWeight +
                    //        stamLow * weights.dodge_staminaAbundanceWeight;

                    // (gated first)
                    if (ctx.SeenIncomingAttack == 0) return 0f;

                    if (ctx.TargetHeavyAttackChargePercentage <= 0)
                    {
                        return 0;
                    }

                    // Lerp: Tells you how far along the line between 'a' and 'b' the value is. If the value is at 'a', return 0, if the value is at 'b' return 1.
                    rise = Mathf.Clamp01(Mathf.InverseLerp(ctx.TargetAttackRange * 0.90f, ctx.TargetAttackRange * 1.1f, ctx.DistanceToTarget));

                    // In this inverseLerp, we subtract by 1, becuase we want to fade out the score as the stamina percentage increases (higher = worse)
                    fall = Mathf.Clamp01(1 - Mathf.InverseLerp(ctx.TargetAttackRange * 1.1f, ctx.TargetAttackRange * 1.2f, ctx.DistanceToTarget));

                    // Combine the two lerps into a bump                 
                    float dodgeCurve = Mathf.Min(rise, fall);

                    // Much more favourable when the enemy is performing a heavy attack, as we can dodge out of the way more easily
                    float enemyHeavyAttack = ctx.TargetHeavyAttackChargePercentage > 0f ? 1 : 0f; // 1 if heavy attack, 0 if not

                    rawScore =

                       dodgeCurve *
                       enemyHeavyAttack;

                    return ApplyCompensationToScore(rawScore, 2) * weights.dodge_master;

                case CBTActionType.LightAttack:

                    /**** Considerations ****/

                    /* How is my current distance to the enemy, if is just barely within my light attack range, i might wait to wait util i move closer to them, as the chance of hitting them is low.
                     * How is my stamina? If it is on the lower side, a light attack might be smarter than a heavy attack, as it costs less stamina.
                     * 
                     */

                    //if (ctx.DistanceToTarget > ctx.LightAttackRange)
                    //    return 0f;

                    //float close = Low(ctx.DistanceToTarget,
                    //                      ctx.IdealLightAttackRange, ctx.LightAttackRange);
                    //close = close * close * (3f - 2f * close);               // smoothstep

                    //float stamPeak = Mathf.Min(High(ctx.CurrentStaminaPercentage, .25f, .4f),
                    //                             Low(ctx.CurrentStaminaPercentage, .4f, .8f));

                    //float enemyHighHP = High(ctx.TargetHealthPercentage, .2f, .6f);

                    //return close * weights.lightAttack_proximityWeight +
                    //        stamPeak * weights.lightAttack_staminaLightBonus +
                    //        enemyHighHP * weights.lightAttack_lowHealthBonus;

                    // *** Distance to target ***

                    float currentAttackOpt = Mathf.InverseLerp(ctx.IdealLightAttackRange, ctx.LightAttackRange, ctx.DistanceToTarget);

                    // 1 when at ideal range, 0 when at max range
                    float proximity = 1f - currentAttackOpt;

                    // Clamp the value
                    proximity = Mathf.Clamp01(proximity);

                    // Smoothstep for a softer falloff so that it ramps up nicely
                    proximity = proximity * proximity * (3f - 2f * proximity); // This formula gives a smooth transition from 0 to 1

                    // *** Current Stamina ***

                    // The goal is to have a curve that turns on between 25% and 40%, peaks at 40%, then fades out by 75%.

                    // Lerp: Tells you how far along the line between 'a' and 'b' the value is. If the value is at 'a', return 0, if the value is at 'b' return 1.
                    rise = Mathf.Clamp01(Mathf.InverseLerp(0.25f, 0.40f, ctx.CurrentStaminaPercentage));

                    // In this inverseLerp, we subtract by 1, becuase we want to fade out the score as the stamina percentage increases (higher = worse)
                    fall = Mathf.Clamp01(1 - Mathf.InverseLerp(0.40f, 0.75f, ctx.CurrentStaminaPercentage));

                    // Combine the two lerps into a bump
                    // We use m in here because we want to get the lower value of the two lerps, as we want to fade out the score as the stamina percentage increases
                    float staminaLightBonus = Mathf.Min(rise, fall);

                    // *** Current Health Difference***

                    // The higher the health of the enemy compared to us, the more we want to back off
                    rawHealthDelta = ctx.TargetHealthPercentage - ctx.CurrentHealthPercentage;
                    hpDiffFactor = Mathf.Clamp01((rawHealthDelta + 1f) * 0.5f);   // 0..1 symmetric

                    float enemyLowStamina = ValueLow(ctx.TargetStaminaPercentage, 0, 0.3f); // 1 low, 0 high - if the enemy has low stamina, we can hit them more easily

                    float highStamina = ValueHigh(ctx.CurrentStaminaPercentage, 0.4f, 0.7f); // 1 high, 0 low - if we have high stamina, we can hit them more easily

                    // *** Enemy Low Stamina Bonus ***

                    return

                        (proximity * weights.lightAttack_proximityWeight +
                        staminaLightBonus * weights.lightAttack_staminaLightBonus +
                        hpDiffFactor * weights.lightAttack_healthDiffBonus +
                        enemyLowStamina * weights.lightAttack_enemyLowStaminaBonus +
                        highStamina * weights.lightAttack_highStaminaBonus

                        );

                case CBTActionType.StartHeavyAttack:

                    /**** Considerations ****/

                    /* How is my current stamina? Higher stamina means I could use some to perform a heavy attack.
                     * How low is the enemy's health? If it is quite low, i should try to finish them off now with light attacks, rather than let them regain health and stamina while i charge for a heavy attack.
                     * How far is the enemy from me? The closer they are, the higher the chance of them getting hit by the heavy attack. Otherwise I might as well just perform a light attack.
                     */

                    if (ctx.CurrentStaminaPercentage < 0.4f)          // hard gate
                        return 0f;

                    //float near = Low(ctx.DistanceToTarget, 0f, ctx.HeavyAttackRange * .6f);
                    //float highStam = High(ctx.CurrentStaminaPercentage, .4f, .8f);
                    //enemyHighHP = High(ctx.TargetHealthPercentage, .15f, .4f);
                    float myHighHP = ValueHigh(ctx.CurrentHealthPercentage, .3f, .6f);

                    //return near * weights.heavyAttack_proximityWeight +
                    //        highStam * weights.heavyAttack_highStaminaWeight +
                    //        enemyHighHP * weights.heavyAttack_lowEnemyHealthWeight +
                    //        myHighHP * weights.heavyAttack_lowHealthWeight;

                    // WANT to attack if we have stamina
                    float staminaFactor = ValueHigh(ctx.CurrentStaminaPercentage, 0.4f, 1f); // 1 high, 0 low

                    // DONT want to attack if the enemy's health is low
                    float enemyHealthFactor = ValueHigh(ctx.TargetHealthPercentage, .1f, 0.3f); // 1 high, 0 low

                    // WANT to attack if the enemy is close enough                          
                    distanceFactor = ValueLow(ctx.DistanceToTarget, 0, ctx.HeavyAttackRange * 0.6f); // 1 close, 0 far

                    // DONT want to attack if my health is low, as I might get hit while charging
                    float healthFactor = ValueHigh(ctx.CurrentHealthPercentage, 0.2f, 0.5f); // 1 high, 0 low

                    // high when we have stamina & are close
                    return

                        (distanceFactor * weights.heavyAttack_proximityWeight +
                        enemyHealthFactor * weights.heavyAttack_lowEnemyHealthWeight +
                        staminaFactor * weights.heavyAttack_highStaminaWeight +
                        healthFactor * weights.heavyAttack_lowHealthWeight +
                        myHighHP * weights.heavyAttack_highHealthWeight);


                case CBTActionType.ReleaseHeavyAttack:

                    /**** Considerations ****/

                    /* What is the current stamina drain of the heavy attack?
                     * Is the target blocking? I'd rather them not to be blocking.
                     * What is the current hit coverage area, do i have time to continue charging my heavy attack, or are the enemy about to leave my attack range?
                     * Is the target charging their own heavy attack? If so, I should release mine to interrupt them.
                     * Is the target blocking, and if so, what is their current stamina at? Will the attack i perform stagger them? It'd be better to stagger the opponent rather than let the regain stamina and block my attack.
                     */

                    float currentHoldPerentage = ctx.TargetHeavyAttackChargePercentage;

                    // WANT to release the farther the target is, as we want to hit them before they can escape
                    distanceFactor = Mathf.Clamp01(1 - Mathf.InverseLerp(ctx.TargetAttackRange * 0.8f, ctx.TargetAttackRange, ctx.DistanceToTarget));

                    float enemyChargingHeavyAttack = ctx.TargetHeavyAttackChargePercentage > 0f ? 1f : 0f; // 1 if the enemy is charging a heavy attack, 0 otherwise

                    float enemyBlocking = ctx.TargetBlocking;

                    // only peaks once min‐charge is met, else zero
                    return
                        (currentHoldPerentage * weights.releaseHeavyAttack_holdPercentWeight +
                        distanceFactor * weights.releaseHeavyAttack_enemyDistanceWeight +
                        enemyChargingHeavyAttack * weights.releaseHeavyAttack_enemyChargingHeavyAttackWeight +
                        enemyBlocking * weights.releaseHeavyAttack_enemyAttackBlockWeight);

                case CBTActionType.Retreat:

                    /**** Considerations ****/

                    /* 
                     * If our health is really low, we should retreat
                     * If our health is lower than the target's, we should retreat
                     * If we are too close to the target, we should retreat
                     * If our stamina is low, we should retreat
                     */


                    lowHealth = ValueLow(ctx.CurrentHealthPercentage, 0, 0.2f);

                    distanceFactor = ValueLow(ctx.DistanceToTarget, 0.5f, ctx.TargetAttackRange); // 1 close, 0 far 

                    lowStamina = ValueLow(ctx.CurrentStaminaPercentage, 0, 0.15f); // 1 low, 0 high

                    rawStamDelta = ctx.TargetStaminaPercentage - ctx.CurrentStaminaPercentage;
                    stamDiffFactor = Mathf.Clamp01((rawStamDelta + 1f) * 0.5f);
  
                    rawHealthDelta = ctx.TargetHealthPercentage - ctx.CurrentHealthPercentage;
                    hpDiffFactor = Mathf.Clamp01((rawHealthDelta + 1f) * 0.5f);   // 0..1 symmetric 
                    totalResourceDiff = (rawHealthDelta + rawStamDelta) / 2f; // average of health and stamina difference

                    float diffHigh = ValueHigh(totalResourceDiff, .4f, .7f); // 1 high, 0 low - if the difference is high, we should retreat

                    rawScore =
                        lowHealth * 
                        distanceFactor * 
                        lowStamina *
                        diffHigh;

                    return ApplyCompensationToScore(rawScore, 4) * weights.retreat_master;

                default:
                    Debug.LogError($"Action {action} not implemented in utility evaluation.");
                    return 0f;

            }
        }

        public static float[] SoftMax(float[] scores, float temperature = 1.0f)
        {
            int n = scores.Length;
            float max = scores.Max();
            float sum = 0f;
            float[] exps = new float[n];

            for (int i = 0; i < n; i++)
            {
                float e = Mathf.Exp((scores[i] - max) / temperature);
                exps[i] = e;
                sum += e;
            }

            for (int i = 0; i < n; i++)
            {
                exps[i] /= sum;
            }

            return exps;
        }

        public static int Sample(IList<float> probs)
        {
            float rand = UnityEngine.Random.value; // [0,1)
            float cumulative = 0f;
            for (int i = 0; i < probs.Count; i++)
            {
                cumulative += probs[i];
                if (rand <= cumulative)
                    return i;
            }
            return probs.Count - 1; // fallback due to rounding
        }


        #region Utilities

        /// <summary>
        /// Helper to check if a score is within 0 and 1 and flags it if it isnt
        /// </summary>
        public bool WithinBounds(float score)
        {
            if (score < 0 || score > 1)
            {
                Debug.LogWarning($"Score {score} is out of bounds! It should be between 0 and 1.");
                return false;
            } 

            return true;
        }

        public float ApplyCompensationToScore(float rawScore, int numConsiderations)
        {
            float modFactor = 1f - (1f / numConsiderations);
            float makeUp = (1f - rawScore) * modFactor; // how much we need to make up for
            return rawScore + (makeUp * rawScore);
        }

        public float ValueLow(float pct, float min, float max)
        {
            return Mathf.Clamp01(1 - Mathf.InverseLerp(min, max, pct));
        }

        public float ValueHigh(float pct, float min, float max)
        {
            return Mathf.InverseLerp(min, max, pct);
        }

        #endregion
    }
}
