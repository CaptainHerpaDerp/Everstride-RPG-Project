using Characters.Behaviour;
using Characters.Utilities;
using Core;
using Core.Enums;
using Core.Interfaces;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Characters
{
    public class CombatStats
    {
        public float TotalDamageDealt = 0f;
        public int LightAttacks = 0;
        public int HeavyAttacks = 0;
        public int SuccessfulBlocks = 0;
        public int FailedBlocks = 0;
    }

    public class NPC : Character
    {
        public static event Action<NPC> OnNPCCreated;
        public CharacterState State => state;

        #region Serialized Fields

        [FoldoutGroup("Component References"), SerializeField] protected CharacterRadiusTracker characterRadiusTracker;
        [FoldoutGroup("Component References"), SerializeField] protected CharacterRetriever characterRetriever;
        [FoldoutGroup("Component References"), SerializeField] protected KnockbackController knockbackController;
        [FoldoutGroup("Component References"), SerializeField] protected Mover mover;

        [SerializeField] public string NPCName;
        [field: SerializeField] public string ID { get; protected set; }

        [SerializeField] private float minWalkVelocity;
        [SerializeField] private float hitAnimationDuration;
        [Header("Seconds Duration Of This NPC's Attack Animation")]
        [Header("Time Between Attacks In Seconds")]
        [SerializeField] protected float attackCooldown;
        public float AttackCooldown => attackCooldown;


        [Header("Distance Between This NPC And Its Target Before It Will Lose Sight of The Target (always higher than view range) 1.25x by default")]
        [SerializeField] protected float targetLossRange;
        [SerializeField] private float DeathTime;

        // Temporary 
        [SerializeField] int archerAttackRange;
        [SerializeField] private Vector3 lootSpawnOffset;
        private bool onAttackCooldown = false;
        private float _chargeHoldAngle;


        [SerializeField] private float blockingTime = 0.5f; // How long the NPC will block for
        private bool inBlockingTime;

        // Create an event with a string parameter

        // Testing
        public CombatStats combatStats { get; private set; } = new CombatStats();


        #endregion

        #region Private Fields


        [SerializeField] private float hitTime;
        private IEnumerator attackCR = null;
        Vector3 PrevPos;
        Vector3 NewPos;
        protected Vector3 ObjVelocity;

        [SerializeField] protected Character _combatTarget;
        public Character CombatTarget
        {
            get { if (_combatTarget == null) return null; return _combatTarget; }
        }

        // Combat Context Variables
        [ShowInInspector] protected bool _seenIncomingAttack = false;
        public CombatContext ctx
        {
            get
            {
                return new CombatContext(this);
            }
        }

        protected Character combatTarget
        {
            get => _combatTarget;

            set
            {
                SetNewCombatTarget(value);
            }
        }

        //Testing
        protected override void OnDamageDealt(float damage)
        {
            combatStats.TotalDamageDealt += damage;
        }

        protected void SetNewCombatTarget(Character newCombatTarget)
        {
            if (newCombatTarget == null)
                return;

            if (_combatTarget == newCombatTarget)
                return;

            // Unsubscribe from the old combat target's events
            if (_combatTarget != null)
            {
                _combatTarget.OnAttackStart -= () =>
                {
                    _seenIncomingAttack = true;
                };

                _combatTarget.OnAttackEnd -= () =>
                {
                    _seenIncomingAttack = false;
                };

            }

            // Set the new combat target
            _combatTarget = newCombatTarget;

            // Subscribe to the new combat target's events
            _combatTarget.OnAttackStart += () =>
            {
                _seenIncomingAttack = true;
            };

            _combatTarget.OnAttackEnd += () =>
            {
                _seenIncomingAttack = false;
            };

            combatTargetCollider = _combatTarget?.GetCollider();

        }

        protected CircleCollider2D combatTargetCollider;

        [BoxGroup("Defencive Stance"), SerializeField] public bool DoTargetViewLock;


        // Combat Context Variables
        private float _lastHitTime;

        #endregion

        #region Unity Callbacks

        private new void Awake()
        {
            if (rigidBody == null)
                rigidBody = GetComponent<Rigidbody2D>();
            if (attackCircle == null)
                attackCircle = transform.GetChild(1).GetComponent<CircleCollider2D>();
            if (hitColliderCircle == null)
                hitColliderCircle = GetComponent<CircleCollider2D>();
            if (mover == null)
                mover = GetComponent<Mover>();
            if (knockbackController == null)
                knockbackController = GetComponent<KnockbackController>();

            staminaCurrent = MaxStamina;
            HitPoints = MaxHealth;
            ObjVelocity = Vector3.zero;
        }

        protected override void Start()
        {
            base.Start();

            if (_combatTarget != null)
            {
                _combatTarget.OnAttackStart += () =>
                {                 
                    _seenIncomingAttack = true;
                }; 
             
                _combatTarget.OnAttackEnd += () =>
                {
                    _seenIncomingAttack = false;
                };
            }

            // Add this npc to the NPC Directory
            OnNPCCreated?.Invoke(this);

            // Sets the UI character name label to the name of the NPC

            //if (targetLossRange == 0 || targetLossRange < viewRange)
            //    targetLossRange = viewRange * 1.25f;

            //// Sets the radius of the radius tracker to the view range
            //characterRadiusTracker.GetComponent<CircleCollider2D>().radius = viewRange;

            if (movementSpeed != 0)
                mover.agent.speed = movementSpeed;
        }

        protected virtual void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(DoQuickView());
            //StartCoroutine(GetNewTarget());
            //StartCoroutine(AttackInRange());
            //StartCoroutine(MoveToCombatTargetInRange());
        }

        #endregion

        #region Abstact Overrides

        public override void SetMovementSpeed(float value)
        {
            mover.agent.speed = value;
        }

        #endregion

        #region Getter Utilities

        protected float AngleToCombatTarget()
        {
            if (combatTarget == null)
                return 0;

            Vector3 direction = (combatTarget.transform.position - transform.position).normalized;

            float angle = Vector2.SignedAngle(Vector2.right, direction);

            return angle;
        }

        #endregion

        #region Main Update Loop

        protected override void Update()
        {
            base.Update();

            GetNewCombatTarget();
            HandleMovement();
            UpdateAnimationState();

            // TODO: Make a method out of this
            if (combatTarget != null && state != CharacterState.Death && !knockbackController.IsKnockbackActive())
            {
                NavMeshPath path = new();

                mover.agent.CalculatePath(combatTarget.transform.position, path);

                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    Vector3 cornerA = path.corners[i];
                    Vector3 cornerB = path.corners[i + 1];

                    Debug.DrawRay(cornerA, cornerB - cornerA, Color.red);

                    // Perform a raycast between cornerA and cornerB to check for obstacles.

                    RaycastHit2D hit = (Physics2D.Raycast(cornerA, cornerB - cornerA));

                    if (!hit)
                        return;

                    if (hit.collider.gameObject.TryGetComponent<INPCPathInteractable>(out var door))
                    {
                        float distance = Vector2.Distance(transform.position, door.Position);

                        if (distance < interactionDistance)
                        {
                            door.Open();
                        }
                    }
                }
            }

            // Update the combat context
           // ctx = new CombatContext(this);
        }

        #endregion

        #region NPC Interaction and Dialogue

        public bool HasID()
        {
            if (ID != "" || ID != null)
                return true;

            return false;
        }

        #endregion

        #region Health
        protected override void EnterHitState(DamagePacket damagePacket)
        {
            // Returns if the character is already dead
            if (state == CharacterState.Death)
            {
                return;
            }

            // Update the last hit time to the current time
            _lastHitTime = Time.time;
            
            if (state == CharacterState.Blocking)
            {
                if (TryBlockIncomingDamage(damagePacket))
                {
                    combatStats.SuccessfulBlocks++;
                    return;
                }
                else
                {
                    combatStats.FailedBlocks++;
                }

            }

            // Checks to see if the incoming attack source was via projectile, in which case, an arrow will be added to the NPC's inventory

            //Projectile projectile = attackSource.GetComponent<Projectile>();
            //if (projectile != null)
            //{
            //    Item recoveryItem = projectile.GetComponentInChildren<RecoveryItem>().item;

            //    if (recoveryItem != null)
            //    {
            //        recoveryItem.quantity = 1;
            //        inventory.AddItemByName(recoveryItem);
            //    }
            //}

            // Play the hit sound
            PlayHitSound();

            animationController.DoFlashHit();

            OnHit?.Invoke();

            HitPoints -= damagePacket.damageAmount;

            // If the NPC has no more hit points, enter the death state.
            if (HitPoints <= 0)
            {
                DoDeath();
                return;
            }

            // Otherwise, enter the hit state.
            else
            {
                knockbackController.ApplyKnockback(damagePacket.source.transform.position);
                SetState(CharacterState.Hit);

                animationController.FlashHideWeaponTrail(hitTime);

                StartCoroutine(ExitHitState());
                return;
            }
        }

        protected override void DoDeath()
        {
            PlayDeathSound();

            if (attackCR != null)
            {
                StopCoroutine(attackCR);
                attackCR = null;
            }

            mover.Stop();

            StartCoroutine(EnterDeathState());
            return;
        }

        private IEnumerator EnterDeathState()
        {
            collisionColliderCircle.enabled = false;

            animationController.StopWalkLoop();

            OnHideHealthBar?.Invoke();
            OnDeath?.Invoke();

            // Disable the rigidbody so the NPC stops moving
            rigidBody.isKinematic = true;
            mover.Disable();

            SetState(CharacterState.Death);

            rigidBody.velocity = Vector2.zero;
            animationController.DoDeathAnimation();

            // Invokes the action that the loot spawner is listening to
            OnActivateLootPoint?.Invoke(this);

            //if (inventory.GetItems().Count > 0)
            //  LootSpawner.Instance.SpawnLootPoint(inventory.GetItems(), transform.position + lootSpawnOffset, hitColliderCircle.radius);

            // yield return new WaitForSeconds(DeathTime);
            yield break;
        }


        protected override void UpdateHealthBar()
        {
            OnUpdateHealthBar?.Invoke(_hitPointsCurrent);
        }

        #endregion

        #region Combat

        protected float DistanceToTarget()
        {
            if (combatTarget != null)
            {
                return Vector3.Distance(transform.position, combatTarget.transform.position);
            }

            return Mathf.Infinity;
        }


        private void EnterAttackCooldown()
        {
            onAttackCooldown = true;

            StartCoroutine(Utils.WaitDurationAndExecute(AttackCooldown, () =>
            {
                onAttackCooldown = false;
            }));
        }

        public void LightAttack(Transform targetTransform)
        {
            if (onAttackCooldown)
            {
                return;
            }

            if (CurrentStamina <= equippedWeapon.lightAttackStaminaCost)
            {
                return;
            }

            combatStats.LightAttacks++;

            attackCR = StartAttack(GetAngleToTarget(targetTransform));

            ReduceStamina(equippedWeapon.lightAttackStaminaCost);

            StartCoroutine(attackCR);

            EnterAttackCooldown();
        }

        /// <summary>
        /// This method is used to start a heavy attack on the target transform, but doesn't actually perform the attack.
        /// </summary>
        /// <param name="targetTransform"></param>
        public void StartHeavyAttack(Transform targetTransform)
        {
            if (CurrentStamina <= 0)
            {
                Debug.LogWarning("NPC " + NPCName + " tried to start a heavy attack without enough stamina!");
                return;
            }

            if (onAttackCooldown)
            {
                return;
            }

            // Disables movement
            mover.Stop();

            animationController.DoWeaponChargeAttackAnimation(viewDirection);

            SetState(CharacterState.Attacking);

            // Invoke the OnAttackStart event
            OnAttackStart?.Invoke();

            // Increase the heavy attack hold time
            if (chargeHoldTime < chargeAttackMaxTime)
            {
                chargeHoldTime += Time.deltaTime;
            }

            // Change holding charge to true, and set the charge hold angle to the angle to the target transform (capture once in this updated loop method)
            if (!_holdingCharge)
            {
                _holdingCharge = true;
                _chargeHoldAngle = GetAngleToTarget(targetTransform);
            }
        }

        public void EndHeavyAttack()
        {
            if (chargeHoldTime <= 0f)
            {
                Debug.LogWarning("NPC " + NPCName + " tried to end heavy attack when not charging!");
                return;
            }

            if (state != CharacterState.Attacking)
            {
                Debug.LogWarning("NPC " + NPCName + " tried to end heavy attack when not attacking!");
                return;
            }

            if (!_holdingCharge)
            {
                return;
            }

            combatStats.HeavyAttacks++;

            // Invoke the OnAttackEnd event
            OnAttackEnd?.Invoke();

            ReduceStamina(GetCurrentHeavyAttackStaminaCost());
            _damageChargeMultiplier = GetHeavyAttackDamageMultiplier(chargeHoldTime);

            attackCR = StartAttack(_chargeHoldAngle);
            StartCoroutine(attackCR);
            EnterAttackCooldown();

            _holdingCharge = false;
        }

        protected virtual void MoveToTarget(Transform target)
        {
            if (target == null)
                return;

            mover.SetTarget(target);
        }

        public void EnterBlockState(Transform source)
        {
            if (state == CharacterState.Blocking || state == CharacterState.Hit)
            {
                return;
            }

            // Firstly, check if the NPC has a shield equipped
            if (equippedShield == null)
            {
                Debug.LogWarning("NPC " + NPCName + " tried to block without a shield equipped!");
                return;
            }

            SetState(CharacterState.Blocking);

            // Set the blocking flag as true, the npc cannot leave the block state for as long as the blocking time is active
            inBlockingTime = true;

            // Exit the required block state after a certain time
            StartCoroutine(Utils.WaitDurationAndExecute(blockingTime, () =>
            {
                inBlockingTime = false;
            }));

            float angle = Vector2.SignedAngle(Vector2.right, (source.position - transform.position).normalized);

            BlockWithAngle(angle);
        }

        public void ExitBlockState()
        {
            if (state == CharacterState.Blocking)
            {
                // Reset the blocktime flag
                inBlockingTime = false;

                SetState(CharacterState.Normal);
                animationController.DoIdleAnimation(viewDirection);
            }
        }

        public bool CanExitBlockState()
        {
            if (state != CharacterState.Blocking)
            {
                return false;
            }

            if (state == CharacterState.Blocking && !inBlockingTime)
            {
                return true;
            }

            return false;
        }


        #endregion

        #region Combat and Movement Coroutines

        /// <summary>
        /// If the current combat target is new, get the closest one
        /// </summary>
        protected void GetNewCombatTarget()
        {
            if (combatTarget != null && !combatTarget.IsDead())
                return;

            Character character = characterRetriever.GetClosestEnemy(factions);

            if (character == null)
            {
                Debug.LogWarning("Couldn't find a valid character");
                return;
            }

            combatTarget = character;
        }

        protected virtual IEnumerator GetNewTarget()
        {
            if (characterRadiusTracker == null)
                Debug.Log("Character radius tracker not assigned to " + gameObject.name);

            while (true)
            {
                if (combatTarget == null)
                {
                    // Get all characters within the view range

                    foreach (var character in characterRadiusTracker.Characters)
                    {
                        if (character.IsDead())
                            continue;

                        if (!IsInFaction(character))
                        {
                            Debug.Log("Found a target");
                            combatTarget = character;
                            break;
                        }
                    }
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        protected virtual IEnumerator StartAttack(float attackAngle)
        {
            // Disables movement
            mover.Stop();

            // Changes to attack state
            SetState(CharacterState.Attacking);
            // Determine the attack direction based on the angle

            AttackWithAngle(attackAngle);

            // Invoke the OnAttackStart event
            OnAttackStart?.Invoke();

            yield return new WaitForSeconds(attackAnimationDuration);

            if (sucessfulEnemyBlock)
            {
                sucessfulEnemyBlock = false;

                animationController.FlashHideWeaponTrail(attackAnimationDuration * 2);
                //yield return new WaitForSeconds(attackAnimationDuration * 2);
            }

            // When the attack ends, invoke the OnAttackEnd event
            OnAttackEnd?.Invoke();

            if (state != CharacterState.Attacking)
                yield break;

            SetState(CharacterState.Normal);

            mover.Resume();

            DoCurrentViewIdle();

            attackCR = null;

            // Reset any charged attack hold time
            chargeHoldTime = 0f;

            // Reset the charge multiplier
            _damageChargeMultiplier = 1;

            // Resets the attack target if it is dead
            if (combatTarget != null && combatTarget.IsDead())
            {
                combatTarget = null;
            }

            yield break;
        }

        private float GetAngleToTarget(Transform targetTransform)
        {
            Vector3 direction = (targetTransform.position - transform.position).normalized;
            float angle = Vector2.SignedAngle(Vector2.right, direction);
            return angle;
        }

        private IEnumerator StartRangedAttack(Transform targetTransform)
        {
            // Disables movement
            mover.Stop();

            SetState(CharacterState.Attacking);

            Vector3 animationDirection = (targetTransform.position - transform.position).normalized;
            float animationAngle = Vector2.SignedAngle(Vector2.right, animationDirection);

            AttackWithAngle(animationAngle);

            yield return new WaitForSeconds(1);

            if (state != CharacterState.Attacking)
            {
                attackCR = null;
                yield break;
            }

            float angle = GetAngleToTarget(targetTransform);

            AttackWithAngle(angle);

            ProjectileFireData projectileFireData = new()
            {
                sourcePos = transform.position,
                fireAngle = angle,
                projectileSpawnDistance = this.projectileSpawnDistance,
                projectilePrefab = null,
                exclusionObject = this.gameObject
            };

            eventBus.Publish("FireProjectile", projectileFireData);

            SetState(CharacterState.Normal);
            mover.Resume();
            DoCurrentViewIdle();
            attackCR = null;
            yield break;
        }

        private IEnumerator ExitHitState()
        {
            // Wait for the hit animation to finish.
            yield return new WaitForSeconds(hitTime);

            // Revert the NPC to an idle state.     
            SetState(CharacterState.Normal);

            if (attackCR != null)
            {
                StopCoroutine(attackCR);
                attackCR = null;
            }

            yield break;
        }

        #endregion

        #region Animation and Movement Handling

        private void HandleMovement()
        {
            NewPos = transform.position;
            ObjVelocity = (NewPos - PrevPos) / Time.fixedDeltaTime;
            PrevPos = NewPos;
        }

        protected virtual void UpdateAnimationState()
        {
            if (state != CharacterState.Normal || knockbackController.IsKnockbackActive() || IsDead())
                return;

            float xAxis = 0;
            float yAxis = 0;

            if (ObjVelocity.x > 0)
            {
                xAxis = 1;
            }
            else if (ObjVelocity.x < 0)
            {
                xAxis = -1;
            }

            if (ObjVelocity.y > 0)
            {
                yAxis = 1;
            }

            else if (ObjVelocity.y < 0)
            {
                yAxis = -1;
            }

            if (xAxis == 1 && yAxis == 1)
            {
                //ChangeAnimationState(WALKTOPRIGHT);
                viewDirection = ViewDirection.TopRight;
            }
            else if (xAxis == 1 && yAxis == -1)
            {
                //ChangeAnimationState(WALKBOTTOMRIGHT);
                viewDirection = ViewDirection.BottomRight;
            }
            else if (xAxis == -1 && yAxis == 1)
            {
                //ChangeAnimationState(WALKTOPLEFT);
                viewDirection = ViewDirection.TopLeft;
            }
            else if (xAxis == -1 && yAxis == -1)
            {
                //ChangeAnimationState(WALKBOTTOMLEFT);
                viewDirection = ViewDirection.BottomLeft;
            }

            else if (yAxis == 1)
            {
                if (viewDirection == ViewDirection.TopLeft || viewDirection == ViewDirection.BottomLeft)
                {
                    //ChangeAnimationState(WALKTOPLEFT);
                    viewDirection = ViewDirection.TopLeft;
                }

                if (viewDirection == ViewDirection.TopRight || viewDirection == ViewDirection.BottomRight)
                {
                    //ChangeAnimationState(WALKTOPRIGHT);
                    viewDirection = ViewDirection.TopRight;
                }
            }

            else if (yAxis == -1)
            {
                if (viewDirection == ViewDirection.TopRight || viewDirection == ViewDirection.BottomRight)
                {
                    //ChangeAnimationState(WALKBOTTOMRIGHT);
                    viewDirection = ViewDirection.BottomRight;
                }

                if (viewDirection == ViewDirection.TopLeft || viewDirection == ViewDirection.BottomLeft)
                {
                    //ChangeAnimationState(WALKBOTTOMLEFT);
                    viewDirection = ViewDirection.BottomLeft;
                }
            }

            if ((xAxis != 0 || yAxis != 0))
            {
                // If the target is in a view lock, the NPC will face the target no matter what direction it is moving in
                if (DoTargetViewLock)
                {
                    Vector3 difference = (transform.position - CombatTarget.transform.position).normalized;

                    // Get an angle from 

                    float angle = Vector2.SignedAngle(Vector2.right, difference);

                    viewDirection = GetWalkDirecitonFromAngle(angle);
                }

                if (equippedWeapon != null)
                {
                    animationController.DoWalkAnimation(viewDirection, true, equippedWeapon.weaponMode);
                }
                else
                {
                    animationController.DoWalkAnimation(viewDirection);
                }
            }
            else
            {
                animationController.EnableCosmeticsAnimators();
                DoCurrentViewIdle();
            }
        }

        protected bool TargetInLos()
        {
            if (combatTarget == null)
            {
                return false;
            }

            // Predict the target's position based on its velocity
            RaycastHit2D hit = (Physics2D.Raycast(transform.position, (combatTarget.transform.position - transform.position)));

            if (hit.collider == null)
                return false;

            if (hit.collider.gameObject == combatTarget.gameObject)
            {
                return true;
            }

            return false;
        }

        public virtual void DoCurrentViewIdle()
        {
            if (equippedWeapon != null && equippedWeapon.weaponType != WeaponType.Unarmed)
            {
                animationController.DoWeaponIdleAnimation(viewDirection);
            }
            else
            {
                animationController.DoIdleAnimation(viewDirection);
            }
        }

        /* For unknown reasons, as soon as an NPC is loaded at the start, it undergoes a little bit of movement (maybe due to rigidbody?) 
        * This always sets the view direction to the top-right, meaning I am unable to set it directly to a different direction.
        * This coroutine crudely sets the direction to either left or right after a few milliseconds.
        */
        private IEnumerator DoQuickView()
        {
            yield return new WaitForSeconds(0.05f);

            int rand = UnityEngine.Random.Range(0, 2);

            if (rand == 0)
                viewDirection = ViewDirection.BottomRight;
            if (rand == 1)
                viewDirection = ViewDirection.BottomLeft;

            yield break;
        }

        #endregion
        
        #region Audio Methods

        protected override void PlayHitSound()
        {
            Debug.LogWarning("Player hit sound not implemented");
        }

        protected override void PlayDeathSound()
        {
            Debug.LogWarning("Player death sound not implemented");
        }

        protected override void PlayAttackSound()
        {
            Debug.LogWarning("Player attack sound not implemented");
        }

        #endregion

        #region Combat Context Pull Methods

        /// <summary>
        /// Returns the range of the NPC's light attack.
        /// </summary>
        /// <returns></returns>
        public float LightAttackRange()
        {
            if (equippedWeapon == null)
                return 0f;
            return equippedWeapon.weaponRange;
        }

        /// <summary>
        /// Returns the range of the NPC's heavy attack.
        /// </summary>
        /// <returns></returns>
        public float HeavyAttackRange()
        {
            if (equippedWeapon == null)
                return 0f;
            return equippedWeapon.weaponRange;
        }

        /// <summary>
        /// Returns the time since the NPC was last hit.
        /// </summary>
        /// <returns></returns>
        public float LastHitTime()
        {
            return _lastHitTime;
        }

        /// <summary>
        /// Returns the remaining duration that the NPC's stamina regeneration is blocked for.
        /// </summary>
        /// <returns></returns>
        public float EhaustionUntil()
        {
            return _exhaustionEndTime ;
        }

        /// <summary>
        /// When blocking an attack, stamina regeneration is blocked for a while. This value shows how long until it can regenerate stamina again.
        /// </summary>
        /// <returns></returns>
        public float StaminaRegenBlockedUntil()
        {
            return _blockRegenEndTime ;
        }

        /// <summary>
        /// Returns true if the NPC's combat target is attacking, false otherwise.
        /// </summary>
        /// <returns></returns>
        public bool SeenIncomingAttackFlag()
        {
            return _seenIncomingAttack;
        }


        #endregion
    }
}