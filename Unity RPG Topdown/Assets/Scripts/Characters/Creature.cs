using System.Collections;
using UnityEngine;
using Core.Enums;

namespace Characters
{
    /// <summary>
    /// An NPC class that does not utilize a weapon or other equipment
    /// </summary>
    public class Creature : NPC
    {
        [SerializeField] protected float AttackDamage;

        protected override void Start()
        {
            base.Start();
            weaponMode = WeaponMode.Slash;
            attackAnimationDuration = 0.7f;
            attackHitMark = 0.2f;
        }

        public override float PhysDamageTotal => AttackDamage;

        protected override IEnumerator AttackInRange()
        {
            while (true)
            {
                if (combatTarget != null)
                {
                    if (combatTarget != null && combatTargetCollider == null)
                    {
                        combatTargetCollider = combatTarget.GetCollider();
                    }

                    // Checks if the distance between the npc and the target is less than the npc's attack radius plus the target's circle collider
                    if (Vector3.Distance(transform.position, combatTarget.transform.position) < attackCircle.radius + combatTargetCollider.radius)
                    {
                        if (state == CharacterState.Normal)
                        {
                            // Disables movement
                            mover.Stop();

                            // Changes to attack state
                            SetState(CharacterState.Attacking);

                            Vector3 direction = (combatTarget.transform.position - transform.position).normalized;

                            float angle = Vector2.SignedAngle(Vector2.right, direction);

                            PlayAttackSound();

                            AttackWithAngle(angle);

                            yield return new WaitForSeconds(attackAnimationDuration);

                            if (sucessfulEnemyBlock)
                            {
                                sucessfulEnemyBlock = false;

                                yield return new WaitForSeconds(attackAnimationDuration * 2);
                            }

                            if (state == CharacterState.Death)
                                yield break;

                            SetState(CharacterState.Normal);

                            mover.Resume();

                            //  DoCurrentViewIdle();
                            yield return new WaitForSeconds(attackCooldown);
                        }
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }

        public override void AttackWithAngle(float angle)
        {
            AttackDirection attackDirection = AttackDirection.BottomRight;

            //if (angle >= 0 && angle < 90)
            //{
            //    viewDirection = ViewDirection.TopRight;
            //    ChangeAnimationState(animKeys.BASEATTACKTOPRIGHT);
            //    attackDirection = AttackDirection.TopRight;
            //}
            //else if (angle >= 90 && angle < 180)
            //{
            //    viewDirection = ViewDirection.TopLeft;
            //    ChangeAnimationState(animKeys.BASEATTACKTOPLEFT);
            //    attackDirection = AttackDirection.TopLeft;
            //}
            //else if (angle >= -180 && angle < -90)
            //{
            //    viewDirection = ViewDirection.BottomLeft;
            //    ChangeAnimationState(animKeys.BASEATTACKBOTTOMLEFT);
            //    attackDirection = AttackDirection.BottomLeft;
            //}
            //else if (angle >= -90 && angle < 0)
            //{
            //    viewDirection = ViewDirection.BottomRight;
            //    ChangeAnimationState(animKeys.BASEATTACKBOTTOMRIGHT);
            //    attackDirection = AttackDirection.BottomRight;
            //}

            StartCoroutine(AttackHitMark(attackDirection));
        }

        //protected override void ChangeAnimationState(string newState)
        //{
        //    // Stops the same animation from interrupting itself
        //    if (currentAnimationState == newState)
        //    {
        //        return;
        //    }

        //    SetCosmeticAnimationState(newState);

        //    currentAnimationState = newState;
        //}

        //public override void DoCurrentViewIdle()
        //{
        //    switch (viewDirection)
        //    {
        //        case ViewDirection.TopRight:
        //            ChangeAnimationState(animKeys.IDLETOPRIGHT);
        //            break;

        //        case ViewDirection.TopLeft:
        //            ChangeAnimationState(animKeys.IDLETOPLEFT);
        //            break;

        //        case ViewDirection.BottomRight:
        //            ChangeAnimationState(animKeys.IDLEBOTTOMRIGHT);
        //            break;

        //        case ViewDirection.BottomLeft:
        //            ChangeAnimationState(animKeys.IDLEBOTTOMLEFT);
        //            break;

        //    }
        //}
    }
}