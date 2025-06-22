using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Enums;

namespace Characters
{
    public class Wolf : Creature
    {
        protected override void Start()
        {
            base.Start();
            StartCoroutine(DoGrowl());
            weaponMode = WeaponMode.Slash;
            attackAnimationDuration = 0.7f;
            attackHitMark = 0.2f;
        }

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

        //    SPINDEATH = "Death";
        //}

        // While the wolf has a combat target and it is not currently attacking, it will growl at random intervals
        private IEnumerator DoGrowl()
        {
            while (true)
            {
                if (combatTarget != null && state == CharacterState.Normal)
                {
                    float waitTime = Random.Range(2f, 5f);

                    yield return new WaitForSeconds(waitTime);

                    if (state == CharacterState.Normal)
                        audioManager.PlayOneShot(fmodEvents.wolfGrowlSound, transform.position);
                }


                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}