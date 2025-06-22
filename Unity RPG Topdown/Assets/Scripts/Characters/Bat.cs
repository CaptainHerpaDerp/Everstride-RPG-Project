using System.Collections;
using UnityEngine;

namespace Characters
{
    public class Bat : Creature
    {
        //private enum BatSleepState { Sleeping, WakingUp, Awake, FallingAsleep }

        //[SerializeField] private BatSleepState sleepState = BatSleepState.Sleeping;

        //[SerializeField] private float sleepTransitionTime = 0.5f;
        //[SerializeField] private float sleepingAlpha = 0.5f;

        //// The position the bat will return to when it falls asleep
        //private Vector3 sleepPosition;

        //// Bat Specific Animation Keys
        //private const string STARTSLEEP = "StartSleep";
        //private const string SLEEPIDLE = "SleepIdle";
        //private const string WAKEUP = "EndSleep";

        //private IEnumerator ExitSleepState()
        //{
        //    StartCoroutine(SetAwakeAlphaCR());

        //    sleepState = BatSleepState.WakingUp;

        //    ChangeAnimationState(WAKEUP);

        //    yield return new WaitForSeconds(sleepTransitionTime);

        //    // Show the health bar after the bat wakes up
        //    OnShowHealthBar?.Invoke();
        //    sleepState = BatSleepState.Awake;

        //    yield break;
        //}

        //private IEnumerator EnterSleepState()
        //{
        //    StartCoroutine(SetSleepAlphaCR());

        //    sleepState = BatSleepState.FallingAsleep;

        //    ChangeAnimationState(STARTSLEEP);

        //    yield return new WaitForSeconds(sleepTransitionTime);

        //    // Hide the health bar while the bat is sleeping
        //    OnHideHealthBar?.Invoke();
        //    sleepState = BatSleepState.Sleeping;

        //    yield break;
        //}

        //private IEnumerator ReturnToSleepPosition()
        //{
        //    while (true)
        //    {
        //        if (combatTarget == null && sleepState == BatSleepState.Awake)
        //        {
        //            if (Vector3.Distance(transform.position, sleepPosition) > mover.agent.stoppingDistance + 0.1f)
        //            {
        //                mover.SetTarget(sleepPosition);
        //            }
        //            else
        //            {
        //                Debug.Log("Entering sleep state");
        //                StartCoroutine(EnterSleepState());
        //                mover.SetTarget(null);
        //            }
        //        }

        //        yield return new WaitForSeconds(0.1f);
        //    }
        //}

        //#region Overriden Methods

        //protected override void Start()
        //{
        //    base.Start();

        //    // Set the sleep position to the bat's current position
        //    sleepPosition = transform.position;

        //    // Health bar is hidden while the bat is sleeping
        //    OnShowHealthBar?.Invoke();
        //    attackAnimationDuration = 0.3f;
        //    attackHitMark = 0.2f;

        //    SetAlpha(sleepingAlpha);
        //}

        //protected override void OnEnable()
        //{
        //    base.OnEnable();
        //    StartCoroutine(ReturnToSleepPosition());
        //}

        ////protected override void InitializeAnimations()
        ////{
        ////    WALKBOTTOMLEFT = "IdleBL";
        ////    WALKBOTTOMRIGHT = "IdleBR";
        ////    WALKTOPLEFT = "IdleTL";
        ////    WALKTOPRIGHT = "IdleTR";

        ////    IDLEBOTTOMLEFT = "IdleBL";
        ////    IDLEBOTTOMRIGHT = "IdleBR";
        ////    IDLETOPLEFT = "IdleTL";
        ////    IDLETOPRIGHT = "IdleTR";

        ////    BASEATTACKBOTTOMRIGHT = "AttackBR";
        ////    BASEATTACKBOTTOMLEFT = "AttackBL";
        ////    BASEATTACKTOPRIGHT = "AttackTR";
        ////    BASEATTACKTOPLEFT = "AttackTL";

        ////    SPINDEATH = "Death";
        ////}

        //protected override IEnumerator GetNewTarget()
        //{
        //    if (characterRadiusTracker == null)
        //        Debug.Log("Character radius tracker not assigned to " + gameObject.name);

        //    while (true)
        //    {
        //        if (combatTarget == null)
        //        {
        //            // Get all characters within the view range

        //            foreach (var character in characterRadiusTracker.Characters)
        //            {
        //                if (character.IsDead())
        //                    continue;

        //                if (!IsInFaction(character))
        //                {
        //                    Debug.Log("Found a target");

        //                    // Exit the sleep state so the bat can move towards the target
        //                    StartCoroutine(ExitSleepState());

        //                    combatTarget = character;
        //                    break;
        //                }
        //            }
        //        }

        //        yield return new WaitForSeconds(0.1f);
        //    }
        //}

        //protected override IEnumerator MoveToCombatTargetInRange()
        //{
        //    while (true)
        //    {
        //        if (sleepState == BatSleepState.Awake && combatTarget != null && state != CharacterState.Death && !knockbackController.IsKnockbackActive())
        //        {
        //            if (Vector3.Distance(transform.position, combatTarget.transform.position) < targetLossRange)
        //            {
        //                mover.SetTarget(combatTarget.transform);
        //            }
        //            else
        //            {
        //                combatTarget = null;
        //                mover.SetTarget(null);
        //            }
        //        }

        //        yield return new WaitForSeconds(0.1f);
        //    }
        //}

        //public override void DoCurrentViewIdle()
        //{
        //    if (sleepState == BatSleepState.Sleeping)
        //    {
        //        ChangeAnimationState(SLEEPIDLE);
        //        return;
        //    }


        //    if (sleepState == BatSleepState.Awake)
        //        switch (viewDirection)
        //        {
        //            case ViewDirection.TopRight:
        //                ChangeAnimationState(animKeys.IDLETOPRIGHT);
        //                break;

        //            case ViewDirection.TopLeft:
        //                ChangeAnimationState(animKeys.IDLETOPLEFT);
        //                break;

        //            case ViewDirection.BottomRight:
        //                ChangeAnimationState(animKeys.IDLEBOTTOMRIGHT);
        //                break;

        //            case ViewDirection.BottomLeft:
        //                ChangeAnimationState(animKeys.IDLEBOTTOMLEFT);
        //                break;

        //        }
        //}

        //#endregion


        //#region Alpha Modifier Methods

        //private void SetAlpha(float value)
        //{
        //    for (int i = 0; i < cosmeticsTransform.childCount; i++)
        //    {
        //        cosmeticsTransform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, value);
        //    }
        //}

        ///// <summary>
        ///// Set the alpha of the costmetic transform's children to the sleep alpha over a period of time
        ///// </summary>
        //private IEnumerator SetSleepAlphaCR()
        //{
        //    float time = 0f;

        //    while (time < sleepTransitionTime)
        //    {
        //        time += Time.deltaTime;

        //        float alpha = Mathf.Lerp(1f, sleepingAlpha, time / sleepTransitionTime);

        //        for (int i = 0; i < cosmeticsTransform.childCount; i++)
        //        {
        //            cosmeticsTransform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, alpha);
        //        }

        //        yield return null;
        //    }

        //    yield break;
        //}

        ///// <summary>
        ///// Set the alpha of the costmetic transform's children to 1 over a period of time
        ///// </summary>
        //private IEnumerator SetAwakeAlphaCR()
        //{
        //    float time = 0f;

        //    while (time < sleepTransitionTime)
        //    {
        //        time += Time.deltaTime;

        //        float alpha = Mathf.Lerp(sleepingAlpha, 1f, time / sleepTransitionTime);

        //        for (int i = 0; i < cosmeticsTransform.childCount; i++)
        //        {
        //            cosmeticsTransform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, alpha);
        //        }

        //        yield return null;
        //    }

        //    yield break;
        //}
        //#endregion
    }
}