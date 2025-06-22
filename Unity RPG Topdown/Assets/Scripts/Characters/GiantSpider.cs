using Characters;
using Core.Enums;
using System.Collections;
using UnityEngine;

namespace Characters
{
    public class GiantSpider : Creature
    {
        //[SerializeField] private GameObject webProjectile;

        //[Header("Giant Spider Specific Animation Keys")]
        //private const string WEBSHOOTATTACKBOTTOMRIGHT = "WebShotBR", WEBSHOOTATTACKBOTTOMLEFT = "WebShotBL",
        //WEBSHOOTATTACKTOPRIGHT = "WebShotTR", WEBSHOOTATTACKTOPLEFT = "WebShotTL";

        //[SerializeField] private float webShootDuration = 1.1f;
        //[SerializeField] private float webFireTime = 0.6f;
        //[SerializeField] private float minWebShootDistance = 3f;

        //// Optional
        //[SerializeField] private PolygonCollider2D combatArea;
        //private Vector3 startPoint;

        //protected override void Start()
        //{
        //    base.Start();
        //    weaponMode = WeaponMode.Slash;
        //    attackAnimationDuration = 0.65f;
        //    attackHitMark = 0.2f;

        //    startPoint = transform.position;
        //}

        //protected override IEnumerator AttackInRange()
        //{
        //    while (true)
        //    {
        //        if (!TargetInCombatArea())
        //        {
        //            mover.SetTarget(startPoint);
        //            combatTarget = null;
        //        }

        //        else if (combatTarget != null)
        //        {
        //            if (combatTarget != null && combatTargetCollider == null)
        //            {
        //                combatTargetCollider = combatTarget.GetCollider();
        //            }

        //            if (!TargetIsSlowed() && TargetInLos() && DistanceToTarget() > minWebShootDistance)
        //            {
        //                mover.Stop();

        //                state = CharacterState.Attacking;

        //                Vector3 direction = (combatTarget.transform.position - transform.position).normalized;

        //                float angle = Vector2.SignedAngle(Vector2.right, direction);

        //                if (angle >= 0 && angle < 90)
        //                {
        //                    viewDirection = ViewDirection.TopRight;
        //                    ChangeAnimationState(WEBSHOOTATTACKTOPRIGHT);
        //                }
        //                else if (angle >= 90 && angle < 180)
        //                {
        //                    viewDirection = ViewDirection.TopLeft;
        //                    ChangeAnimationState(WEBSHOOTATTACKTOPLEFT);
        //                }
        //                else if (angle >= -180 && angle < -90)
        //                {
        //                    viewDirection = ViewDirection.BottomLeft;
        //                    ChangeAnimationState(WEBSHOOTATTACKBOTTOMLEFT);
        //                }
        //                else if (angle >= -90 && angle < 0)
        //                {
        //                    viewDirection = ViewDirection.BottomRight;
        //                    ChangeAnimationState(WEBSHOOTATTACKBOTTOMRIGHT);
        //                }

        //                yield return new WaitForSeconds(webFireTime);

        //                if (TargetInLos())
        //                {
        //                    if (combatTarget != null)
        //                    {
        //                        FireWeb(combatTarget.transform);

        //                        yield return new WaitForSeconds(webShootDuration - webFireTime);
        //                    }

        //                    ChangeAnimationState(GetIdleAnimationForDirection(viewDirection));

        //                    if (state == CharacterState.Death)
        //                        yield break;

        //                    state = CharacterState.Normal;
        //                    mover.Resume();

        //                    yield return new WaitForSeconds(attackCooldown);
        //                }

        //                else
        //                {
        //                    state = CharacterState.Normal;
        //                    mover.Resume();
        //                    continue;
        //                }
        //            }

        //            // Checks if the distance between the npc and the target is less than the npc's attack radius plus the target's circle collider
        //            else if (Vector3.Distance(transform.position, combatTarget.transform.position) < attackCircle.radius + combatTargetCollider.radius)
        //            {
        //                if (state == CharacterState.Normal)
        //                {
        //                    // Disables movement
        //                    mover.Stop();

        //                    // Changes to attack state
        //                    state = CharacterState.Attacking;

        //                    Vector3 direction = (combatTarget.transform.position - transform.position).normalized;

        //                    float angle = Vector2.SignedAngle(Vector2.right, direction);

        //                    PlayRandomClipAtPoint(soundLibrary.attackSounds);

        //                    AttackWithAngle(angle);

        //                    yield return new WaitForSeconds(attackAnimationDuration);

        //                    if (sucessfulEnemyBlock)
        //                    {
        //                        sucessfulEnemyBlock = false;
        //                        ChangeAnimationState(GetIdleAnimationForDirection(viewDirection));
        //                        yield return new WaitForSeconds(attackAnimationDuration);
        //                    }

        //                    if (state == CharacterState.Death)
        //                        yield break;

        //                    state = CharacterState.Normal;

        //                    mover.Resume();

        //                    //  DoCurrentViewIdle();
        //                    yield return new WaitForSeconds(attackCooldown);
        //                }
        //            }
        //        }

        //        if (state == CharacterState.Death)
        //            yield break;

        //        yield return new WaitForFixedUpdate();
        //    }
        //}

        //private bool TargetIsSlowed()
        //{
        //    if (combatTarget != null)
        //    {
        //        return combatTarget.GetStatusEffect(StatusEffect.TempSlow);
        //    }
        //    return false;
        //}

        //protected override void DealDamageToCharacter(Character character)
        //{
        //    base.DealDamageToCharacter(character);
        //    character.ApplyStatusEffect(StatusEffect.Poison, 5, transform);
        //}

        //private bool TargetInCombatArea()
        //{
        //    if (combatArea == null)
        //    {
        //        return true;
        //    }

        //    if (combatTarget == null)
        //    {
        //        return false;
        //    }

        //    return combatArea.OverlapPoint(combatTarget.transform.position);
        //}

        //protected void FireWeb(Transform targetTransform)
        //{
        //    Vector3 predictedPosition = combatTarget.transform.position + combatTarget.GetVelocity();

        //    Vector3 direction = (predictedPosition - transform.position).normalized;

        //    float angle = Vector2.SignedAngle(Vector2.right, direction);

        //    if (webProjectile == null)
        //    {
        //        Debug.LogError("Web Projectile not set in the inspector");
        //        return;
        //    }

        //    ProjectileManager.Instance.FireProjectileFrom(transform.position, angle, webProjectile, 1);
        //}

        //protected override void InitializeAnimations()
        //{
        //    WALKBOTTOMLEFT = "WalkBL";
        //    WALKBOTTOMRIGHT = "WalkBR";
        //    WALKTOPLEFT = "WalkTL";
        //    WALKTOPRIGHT = "WalkTR";

        //    BASEATTACKBOTTOMRIGHT = "AttackBR";
        //    BASEATTACKBOTTOMLEFT = "AttackBL";
        //    BASEATTACKTOPRIGHT = "AttackTR";
        //    BASEATTACKTOPLEFT = "AttackTL";

        //    IDLEBOTTOMLEFT = "IdleBL";
        //    IDLEBOTTOMRIGHT = "IdleBR";
        //    IDLETOPLEFT = "IdleTL";
        //    IDLETOPRIGHT = "IdleTR";

        //    STUNBOTTOMLEFT = "StunBL";
        //    STUNBOTTOMRIGHT = "StunBR";
        //    STUNTOPLEFT = "StunTL";
        //    STUNTOPRIGHT = "StunTR";

        //    SPINDEATH = "Death";
        //}
    }
}