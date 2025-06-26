using Core;
using Core.Enums;
using Core.Interfaces;
using Effects.Lighting;
using FMOD.Studio;
using Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public class Player : Character
    {
        // Only one instance of player, so it can be made into a singleton
        public static Player Instance;

        Transform playerTransform;

        //temp
        [SerializeField] private bool hasTorch;
        [SerializeField] GameObject TorchLightObj;
        [SerializeField] private CharacterTorchLight torchLight;

        private Dictionary<GameObject, ActionType> interactables = new();

        private bool canReleaseShield = false;
        private bool bowDrawn = false;
        private bool sprintEnabled = true;
        private bool isSprinting = false;

       // private bool sprintLock = false;

        private bool menuOpen = false;
        private bool lockMovement = false;

        private bool canSprint
        {
            get
            {
                // If we’re still in exhaustion, skip regen
                if (Time.time < _exhaustionEndTime)
                    return false;

                if (sprintEnabled && staminaCurrent > 0)
                    return true;
                else
                    return false;
            }
        }


        // Temp
        private const float drawTime = 0.3f;
        private const float releaseTime = 0.3f;
        private const float sprintAnimationSpeed = 1.3f;
        private const float chainAttackOpportunityTimeout = 0.3f;

        public bool lockYaxis;

        private float xAxis, yAxis;

        private Vector2 velocity;

        private IEnumerator chainAttackOpportunity;

        protected override void Awake()
        {
            base.Awake();

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected override void Start()
        {
            base.Start();

            movementSpeed = defaultMovementSpeed;

            playerTransform = transform;

            // Set the player's health to the max hit points
            HitPoints = MaxHealth;

            // Set the player's stamina to the max stamina
            staminaCurrent = MaxStamina;

            // Set the player's magica to the max magica
            magicaCurrent = MagicaMax;

            //TODO: LINK THIS WITHOUT REFERENCE TO MENU CONTEROLLER
            //menuController.MenuOpen.AddListener(LockPlayerControl);
            //menuController.MenuClosed.AddListener(UnlockPlayerControl);

            // Default values to fix github transfer bug
            if (movementSpeed == 0)
                movementSpeed = 200;

            if (MaxHealth == 0)
                MaxHealth = 10;

            if (interactionDistance == 0)
                interactionDistance = 1f;

            if (attackHitMark == 0)
                attackHitMark = 0.1f;

            defaultMovementSpeed = movementSpeed;
        }

        #region Abstact Overrides

        public override void SetMovementSpeed(float value)
        {
            movementSpeed = value;
        }

        #endregion

        protected override void SubscribeToBusEvents()
        {
            base.SubscribeToBusEvents();

            //Debug.Log("Subscribing to player events");
            eventBus.Subscribe<WeaponItem>("OnPlayerWeaponChanged", ChangeEquippedWeapon);
            eventBus.Subscribe<ShieldItem>("OnPlayerShieldChanged", ChangeEquippedShield);
        }

        protected override void UpdateHealthBar()
        {    
            OnUpdateHealthBar?.Invoke(_hitPointsCurrent);
        }

        /// <summary>
        /// Slow the movement speed and increase it gradually over the duration
        /// </summary>
        /// <param name="duration"></param>
        protected override IEnumerator SlowMovementSpeed(float duration)
        {
            sprintEnabled = false;
            StopSprinting();

            float slowAmount = 0.5f;
            movementSpeed = defaultMovementSpeed * slowAmount;

            while (movementSpeed < defaultMovementSpeed)
            {
                movementSpeed += 0.1f;
                yield return new WaitForEndOfFrame();
            }

            Debug.Log("Movement speed restored");
            sprintEnabled = true;
            movementSpeed = defaultMovementSpeed;

            yield break;
        }

        public void EnableTorch()
        {
            if (hasTorch)
            {
                TorchLightObj.SetActive(true);
            }
            else
            {
                torchLight.gameObject.SetActive(true);
            }
        }

        public void DisableTorch()
        {
            TorchLightObj.gameObject.SetActive(false);
        }

        public override Vector3 GetVelocity()
        {
            return velocity;
        }

        public void AddToInteractables(GameObject gameObject, ActionType action)
        {
            if (interactables.ContainsKey(gameObject))
            {
                return;
            }

            interactables.Add(gameObject, action);
        }

        public void RemoveFromInteractables(GameObject gameObject)
        {
            if (interactables.ContainsKey(gameObject))
            {
                interactables.Remove(gameObject);
            }
        }

        public void ModifyInteractableLabel(GameObject gameObject, ActionType newActionType)
        {
            if (interactables.ContainsKey(gameObject))
            {
                interactables[gameObject] = newActionType;
            }
        }

        public void LockPlayerControl(bool locksMovement = true)
        {
            lockMovement = locksMovement;
            menuOpen = true;
        }

        public void UnlockPlayerControl()
        {
            lockMovement = false;
            menuOpen = false;
        }

        private IEnumerator ExitAttackState(float angle)
        {
            if (equippedWeapon == null)
            {
                Debug.LogWarning("No weapon equipped, cannot attack.");
                yield break;
            }

            // Invoke the onattackstart event
            OnAttackStart?.Invoke();

            if (equippedWeapon.canChargeAttack)
            {
                // Update the view direction based on the mouse position so that the charge animation can face the right way
                UpdateViewDirection();

                animationController.DoWeaponChargeAttackAnimation(viewDirection);

                // Wait until the attack button is released
                while (Input.GetKey(KC.Attack))
                {
                    _chargeHoldTime += Time.deltaTime;
                    yield return null;
                }

                /* If the mouse is released but the charge time already exceeds the min heavy attack threshhold,
                 we need to wait for the remaining duration before releasing and performing a heavy attack */

                if (_chargeHoldTime > chargeAttackMinHoldTime && _chargeHoldTime < chargeAttackMinTime)
                {
                    float remainingChargeTime = chargeAttackMinTime - _chargeHoldTime;
                    yield return new WaitForSeconds(remainingChargeTime);
                }
            }

            // Calculate the damage charge multiplier based on chargeTime
            if (equippedWeapon.canChargeAttack && _chargeHoldTime >= chargeAttackMinTime)
            {
                _damageChargeMultiplier = GetHeavyAttackDamageMultiplier();
                ReduceStamina(GetCurrentHeavyAttackStaminaCost());
            }
            else
            {
                _damageChargeMultiplier = 1f;
                ReduceStamina(equippedWeapon.lightAttackStaminaCost);
            }

            AttackWithAngle(angle);

            if (equippedWeapon != null && equippedWeapon.weaponType == WeaponType.LongSword)
            {
                yield return new WaitForSeconds(twoHandedAnimationDurations[attackIteration]);
            }
            else
            {
                yield return new WaitForSeconds(attackAnimationDuration);
            }


            EndAttack();
            yield break;
        }

        private IEnumerator ExitBlockState()
        {
            yield return new WaitForSeconds(blockAnimationDuration);
            canReleaseShield = true;

            if (Input.GetKey(KC.Block) == false)
            {
                EndBlock();
            }

            if (Input.GetKeyUp(KC.Block))
            {
                EndBlock();
            }

            yield break;
        }

        private IEnumerator BowReleaseCR(float angle, Item recoverableItem = null)
        {
            yield return new WaitForSeconds(drawTime);
            bowDrawn = true;

            // If the attack button is not still held down, release the arrow
            if (!Input.GetKey(KC.Attack))
            {
                FireBowAtAngle(angle);
                StartCoroutine(ExitBowShotState());
            }

            yield break;
        }

        private IEnumerator SpellShootCR()
        {
            // While we wait for the spell cast time, we want to drain the player's magica by the spell cost, and drain it gradually over the spell cast time
            float spellCost = equippedWeapon.CastCost;
            StartCoroutine(DrainMagicaByAmount(spellCost, spellCastTime));

            yield return new WaitForSeconds(spellCastTime);

            // Play the spell hold loop sound
            EventInstance spellHoldLoop = audioManager.CreateInstance(fmodEvents.GetCorrespondingSpellHoldLoopSound(equippedWeapon.SpellType), transform.position);
            spellHoldLoop.start();

            // While the player is holding the mouse button, play the spell hold animation
            while (Input.GetKey(KC.Attack))
            {
                // Set the animation state to the spell hold state if the player is still holding the mouse button
                animationController.DoSpellHoldAnimation(viewDirection);

                // While the mouse is held down, update the view direction so that the player faces the mouse
                UpdateViewDirection();

                // If the player is holding the mouse button, they are holding the spell
                isHoldingSpell = true;

                yield return new WaitForEndOfFrame();
            }

            /* Mouse Release */

            // The player is no longer holding the spell
            isHoldingSpell = false;

            // Stop the loop sound
            spellHoldLoop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            spellHoldLoop.release();

            // Determine the attack direction based on the angle
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = (mousePosition - playerTransform.position).normalized;
            float angle = Vector2.SignedAngle(Vector2.right, direction);
        
            // Play the spell release sound
            audioManager.PlayOneShot(fmodEvents.GetCorrespondingSpellThrowSound(equippedWeapon.SpellType), transform.position);

            animationController.DoSpellThrowAnimation(viewDirection);

            // Wait for the spell fire time and then fire the spell
            StartCoroutine(Utils.WaitDurationAndExecute(spellFireTime, () =>
            {
                FireSpellAtAngle(angle);
            }));

            // Wait for the total spell cast time and then exit the spell state
            StartCoroutine(Utils.WaitDurationAndExecute(spellThrowTime, () =>
            {
                SetState(CharacterState.Normal);
            }));
            
        }

        private IEnumerator DrainMagicaByAmount(float amount, float castTime)
        {
            float drainAmount = amount / castTime;

            float timer = 0;

            while (timer < castTime)
            {
                magicaCurrent -= drainAmount * Time.deltaTime;

                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();   
            }
        }

        private IEnumerator ExitBowShotState()
        {
            bowDrawn = false;
            yield return new WaitForSeconds(0.3f);

            SetState(CharacterState.Normal);

            yield break;
        }

        #region State Control

        protected override void SetState(CharacterState newState)
        {
            switch (newState)
            {

                case CharacterState.Attacking:
                    animationController.StopWalkLoop();
                    StopSprinting();
                    break;

                case CharacterState.Blocking:
                    animationController.StopWalkLoop();
                    StopSprinting();
                    break;

            }

            _state = newState;
        }

        #endregion

        protected override void Update()
        {            
            base.Update();

            if (Input.GetKeyDown(KeyCode.K))
            {
                DoDeath();
            }

            if (InRunningState())
            {
                xAxis = Input.GetAxisRaw("Horizontal");
            }

            if (!lockYaxis && InRunningState())
            {
                yAxis = Input.GetAxisRaw("Vertical");
            }
            else
            {
                yAxis = 0;
                velocity.y = 0;
            }

            // Detect if the player is pressing the block button
            DoBlocking();

            HandleMovement();

            DoWeaponSheathing();

            DoSprinting();

            if (state != CharacterState.Attacking && state != CharacterState.Blocking && !menuOpen)
            {
                // Attack
                if (Input.GetKeyDown(KC.Attack))
                {
                    if (equippedWeapon == null)
                    {
                        Debug.LogWarning("No weapon equipped, cannot attack.");
                        return;
                    }

                    // If the weapon is sheathed, unsheath it and return
                    if (IsWeaponSheathed())
                    {
                        UnsheathWeapon();
                        return;
                    }

                    StopMovement();

                    if (chainAttackOpportunity != null)
                    {
                        StopCoroutine(chainAttackOpportunity);
                        chainAttackOpportunity = null;
                    }

                    SetState(CharacterState.Attacking);

                    // Determine the attack direction based on the angle
                    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector3 direction = (mousePosition - playerTransform.position).normalized;
                    float angle = Vector2.SignedAngle(Vector2.right, direction);

                    // Preforms a ranged attack if we are using a ranged weapon
                    if (weaponMode == WeaponMode.Ranged || weaponMode == WeaponMode.Spell)
                    {
                        // Checks to see if the player is attacking with a bow
                        if (equippedWeapon.weaponType == WeaponType.Bow)
                        {
                            // Check to see if the player has an arrow to shoot

                            //Item ammoItem = inventory.GetAmmunitionForWeapon(WeaponType.Bow, true);

                            // Play the bow draw sound
                            audioManager.PlayOneShot(fmodEvents.bowDrawSound, transform.position);

                            StartCoroutine(BowReleaseCR(angle));


                            //Item ammoItem = null;

                            //if (ammoItem == null)
                            //{
                            //    // Play the no arrow sound
                            //    //SoundManager.PlaySound(SoundManager.Sound.NoArrow);
                            //    StartCoroutine(ExitAttackState());
                            //    return;
                            //}

                            //else
                            //    StartCoroutine(BowReleaseCR(angle, ammoItem));
                        }

                        if (equippedWeapon.weaponType == WeaponType.Book)
                        {
                            Debug.Log("Casting spell");
                            
                            // Determine if the player has enough magica to cast the spell
                            if (magicaCurrent < equippedWeapon.CastCost)
                            {
                                Debug.Log("Not enough magica to cast spell");
                                SetState(CharacterState.Normal);
                                return;
                            }

                            StartCoroutine(SpellShootCR());
                        }

                        AttackWithAngle(angle);
                    }

                    // Preforms a melee attack
                    else
                    {
                        StartCoroutine(ExitAttackState(angle));
                       // AttackWithAngle(angle);
                    }
                }
            }

            if (Input.GetKeyUp(KC.Attack) && bowDrawn && state == CharacterState.Attacking)
            {
                StopSprinting();

                if (weaponMode == WeaponMode.Ranged)
                {
                    // Checks to see if the player is attacking with a bow
                    if (equippedWeapon.weaponType == WeaponType.Bow)
                    {
                        // Check to see if the player has an arrow to shoot

                        // Item ammoItem = inventory.GetAmmunitionForWeapon(WeaponType.Bow, false);

                        FireBowAtAngle(GetMouseAngle());
                        StartCoroutine(ExitBowShotState());
                        return;

                        Item ammoItem = null;

                        if (ammoItem == null)
                        {
                            // Play the no arrow sound
                            //SoundManager.PlaySound(SoundManager.Sound.NoArrow);
                        //    StartCoroutine(ExitAttackState());
                            return;
                        }

                        else
                        {
                            FireBowAtAngle(GetMouseAngle());
                        }
                    }
                }

            }

            if (state == CharacterState.Blocking && canReleaseShield && Input.GetKeyUp(KC.Block))
            {
                EndBlock();
            }

            if (state == CharacterState.Blocking)
            {

            }

            DoObjectInteraction();
        }

        private void DoWeaponSheathing()
        {
            if (Input.GetKeyDown(KC.SheathKey))
            {
                ToggleWeaponSheath();
            }
        }

        private void DoSprinting()
        {
            if (isSprinting && Input.GetKeyUp(KC.Sprint))
            {
                StopSprinting();
            }

            // If the sprint key is held and we can sprint, sprint.
            if (Input.GetKey(KC.Sprint) && canSprint)
            {
                if (!isSprinting && (xAxis != 0 || yAxis != 0))
                {
                    isSprinting = true;
                    movementSpeed = sprintSpeed;
                    animationController.SetCosmeticAnimationSpeed(sprintAnimationSpeed);
                }

                if (velocity.magnitude > 0)
                staminaCurrent -= Time.deltaTime * staminaReductionModifier;

                // If the sprint time reaches 0, we lock sprinting for a period of time, and it may not Recovery for this period
                if (staminaCurrent <= 0)
                {
                    StopSprinting();
                }
            }
        }

        private void DoBlocking()
        {
            // Block
            if (Input.GetMouseButton(1) && CanBlock() && state != CharacterState.Attacking && state != CharacterState.Blocking && state != CharacterState.Hit && !menuOpen)
            {
                StopSprinting();

                SetState(CharacterState.Blocking);

                canReleaseShield = false;

                // Start the coroutine to exit the block state after a certain time
                StartCoroutine(ExitBlockState());

                // Determine the attack direction based on the angle
                BlockWithAngle(GetMouseAngle());
            }
        }

        protected override void EnterStaggerState()
        {
            base.EnterStaggerState();

            StopMovement();
        }

        private void StopMovement()
        {
            xAxis = 0;
            yAxis = 0;

            velocity.x = 0;
            velocity.y = 0;

            StopSprinting();
        }

        private float GetMouseAngle()
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = (mousePosition - playerTransform.position).normalized;

            float angle = Vector2.SignedAngle(Vector2.right, direction);
            return angle;
        }
         
        #region animationKeys 

        private void EndAttack()
        {
            attackIteration++;
            if (attackIteration > 1)
                attackIteration = 0;

            // If the attack iteration is not 0, start the coroutine to lose the opportunity to chain attack
            if (attackIteration != 0)
            {
                if (chainAttackOpportunity == null)
                {
                    chainAttackOpportunity = DoLossAttackIterationOpportunity();
                    StartCoroutine(chainAttackOpportunity);
                }
            }

            // Invoke the OnAttackEnd event
            OnAttackEnd?.Invoke();

            // Reset the charge damage multiplier
            _damageChargeMultiplier = 1;

            // reset the hold time
            _chargeHoldTime = 0;

            // Reset the player state back to normal after the attack animation finishes
            SetState(CharacterState.Normal);
        }

        private void EndBlock()
        {
            SetState(CharacterState.Normal);
        }

        #endregion
        
        // Reset the attack iteration if the player does not continue the current attack chain within a certain time frame
        private IEnumerator DoLossAttackIterationOpportunity()
        {
            int currentattackIteration = attackIteration;

            yield return new WaitForSeconds(chainAttackOpportunityTimeout);

            if (attackIteration == currentattackIteration)
            {
                attackIteration = 0;
            }

            yield break;
        }

        private void DoObjectInteraction()
        {
            if (menuOpen)
                return;

            if (Input.GetKeyDown(KC.Interact))
            {
                // Create a copy of the interactables dictionary
                Dictionary<GameObject, ActionType> interactablesCopy = new(interactables);

                // Iterate through the copied dictionary
                foreach (var interactable in interactablesCopy)
                {
                    float distance = Vector2.Distance(transform.position, interactable.Key.transform.position);
                    if (distance < interactionDistance)
                    {
                        // Interact with the object
                        interactable.Key.GetComponent<IInteractable>().Interact(this, newAction =>
                        {
                            // Modify the interactable's action label in the original dictionary
                            interactables[interactable.Key] = newAction;

                        });

                        // If the interactable is a talk action, stop the movement animation (if any)
                        if (interactable.Value == ActionType.Talk)
                        {
                            DoCurrentViewIdle();
                        }
                    }
                    return;
                }
            }
        }

        private void HandleMovement()
        {
            // If dialogue is active or player is attacking, do not handle movement
            if (menuOpen || lockMovement || InAttackingState() || state == CharacterState.Death || state == CharacterState.Hit)
            {
                rigidBody.velocity = Vector2.zero;
                return;
            }

            // Movement input
            velocity.x = Mathf.Clamp(xAxis, -1, 1);
            velocity.y = Mathf.Clamp(yAxis, -1, 1);

            // Determine view direction and trigger walk animation
            if (xAxis != 0 || yAxis != 0)
            {
                DetermineViewDirection();
                DoCurrentViewWalkAnimation();
            }
            else
            {
                animationController.EnableCosmeticsAnimators();

                if (state == CharacterState.Blocking)
                {
                    // If the player is blocking, the logic will change a little bit 
                    
                }
                else
                {
                    DoCurrentViewIdle();
                }
            }

            animationController.UpdateWeaponSortingLayer();

            // Get the normalized velocity
            Vector2 normalizedVelocity = velocity.normalized;

            // Quantize position to grid
            transform.position = new Vector3(
                Mathf.Round(transform.position.x * 128) / 128,
                Mathf.Round(transform.position.y * 128) / 128,
                transform.position.z
            );

            // Set the rigidbody's velocity to the normalized version of velocity multiplied by the movement speed
            rigidBody.velocity = movementSpeed * normalizedVelocity;
        }

        /// <summary>
        /// Return the current view direction based on the mouse position
        /// </summary>
        /// <returns></returns>
        private void UpdateViewDirection()
        {
            float angle = GetMouseAngle();

            if (angle >= 0 && angle < 90)
            {
                viewDirection = ViewDirection.TopRight;
            }
            else if (angle >= 90 && angle < 180)
            {
                viewDirection = ViewDirection.TopLeft;
            }
            else if (angle >= -180 && angle < -90)
            {
                viewDirection = ViewDirection.BottomLeft;
            }
            else if (angle >= -90 && angle < 0)
            {
                viewDirection = ViewDirection.BottomRight;
            }
        }

        private void DetermineViewDirection()
        {
            // Determine view direction based on input
            if (xAxis == 1 && yAxis == 1) viewDirection = ViewDirection.TopRight;
            else if (xAxis == 1 && yAxis == -1) viewDirection = ViewDirection.BottomRight;
            else if (xAxis == -1 && yAxis == 1) viewDirection = ViewDirection.TopLeft;
            else if (xAxis == -1 && yAxis == -1) viewDirection = ViewDirection.BottomLeft;
            else if (xAxis == 1) viewDirection = viewDirection == ViewDirection.TopRight ? ViewDirection.TopRight : ViewDirection.BottomRight;
            else if (xAxis == -1) viewDirection = viewDirection == ViewDirection.TopLeft ? ViewDirection.TopLeft : ViewDirection.BottomLeft;
            else if (yAxis == 1) viewDirection = viewDirection == ViewDirection.TopLeft || viewDirection == ViewDirection.BottomLeft ? ViewDirection.TopLeft : ViewDirection.TopRight;
            else if (yAxis == -1) viewDirection = viewDirection == ViewDirection.TopRight || viewDirection == ViewDirection.BottomRight ? ViewDirection.BottomRight : ViewDirection.BottomLeft;
        }

        private void DoCurrentViewIdle()
        {
            animationController.UpdateWeaponSortingLayer();

            if (equippedWeapon == null || IsWeaponSheathed() || equippedWeapon != null && equippedWeapon.weaponType == WeaponType.Unarmed)
            {
                animationController.DoIdleAnimation(viewDirection);
            }
            else
            {
                animationController.DoWeaponIdleAnimation(viewDirection);
            }
        }

        private void DoCurrentViewWalkAnimation()
        {
            if (IsWeaponSheathed() || equippedWeapon == null)
            {
                animationController.DoWalkAnimation(viewDirection);
            }
            else
            {
                animationController.DoWalkAnimation(viewDirection, true, equippedWeapon.weaponMode);
            }
        }

        /// <summary>
        /// Plays the appropriate bow animation based on the angle of the attack, plays the bow release sound and fires an arrow projectile
        /// </summary>
        /// <param name="angle"></param>
        protected void FireBowAtAngle(float angle)
        {
            audioManager.PlayOneShot(fmodEvents.bowReleaseSound, transform.position);
            animationController.DoBowReleaseAnimation(angle);

            ProjectileFireData projectileFireData = new()
            {
                sourcePos = transform.position,
                fireAngle = angle,
                projectileSpawnDistance = this.projectileSpawnDistance,
                projectilePrefab = null,
                exclusionObject = this.gameObject
            };

            eventBus.Publish("FireProjectile", projectileFireData);
        }

        protected void FireSpellAtAngle(float angle)
        {
            Debug.Log("Firing spell at angle " + angle);

            SpellProjectileFireData data = new SpellProjectileFireData
            {
                sourcePos = transform.position,
                fireAngle = angle,
                spellType = equippedWeapon.SpellType,
                projectileSpawnDistance = this.spellSpawnDistance,
                exclusionObject = this.gameObject
            };

            eventBus.Publish("FireSpellProjectile", data);
        }

        private void StopSprinting()
        {
            if (!isSprinting)
                return;

            isSprinting = false;
            movementSpeed = defaultMovementSpeed;
            animationController.SetCosmeticAnimationSpeed(1f);
        }

        private void ChangeEquippedWeapon(WeaponItem item)
        {
            Debug.Log("Changing equipped weapon");

            equippedWeapon = item;
        }

        private void ChangeEquippedShield(ShieldItem item)
        {
            equippedShield = item;
        }

        protected override void DoDeath()
        {
            SetState(CharacterState.Death);

            animationController.DoDeathAnimation();
        }

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
    }
}
