using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Enums;
using AudioManagement;
using UnityEngine.U2D.Animation;
using Items;
using System;
using Core;
using Sirenix.OdinInspector;

namespace Characters
{

    public enum NPCState
    {
        Moving,
        Attacking,
        Defensive,
        TargetSeen,
        TargetBlocking
    }

    public struct DamagePacket
    {
        public float damageAmount;
        public Transform source;
        public float blockStaminaDrawMultiplier;
    }

    public abstract class Character : MonoBehaviour
    {
        #region Serialized Fields

        [FoldoutGroup("Component References"), SerializeField] protected Rigidbody2D rigidBody;
        [FoldoutGroup("Component References"), SerializeField] protected CircleCollider2D attackCircle;
        [FoldoutGroup("Component References"), SerializeField] protected CircleCollider2D hitColliderCircle;
        [FoldoutGroup("Component References"), SerializeField] protected CapsuleCollider2D collisionColliderCircle;
        [FoldoutGroup("Component References"), SerializeField] protected CharacterAnimationController animationController;

        //[SerializeField] protected EquipmentHandler equipmentHandler;
        [SerializeField] protected Transform statusEffectsParent;
        [SerializeField] protected GameObject statusEffectPrefab;
        //[SerializeField] protected SpriteRenderer weaponSR, weaponTrailSR, shieldSR, shieldTrailSR;

        [FoldoutGroup("Character Stats"), SerializeField] protected float movementSpeed = 150f;
        public float defaultMovementSpeed { get; protected set; }
        [field: SerializeField, FoldoutGroup("Character Stats, Health")] public float MaxHealth { get; protected set; }
        [FoldoutGroup("Character Stats, Health"), SerializeField] protected float _hitPointsCurrent;

        [Header("The amount of time that needs to pass after taking damage where the character starts to recover hp")]
        [FoldoutGroup("Character Stats, Health"), SerializeField] protected float healthRecoveryTime = 5;
        [FoldoutGroup("Character Stats, Health"), SerializeField, Range(0, 1)] protected float healthRecoveryModifier = 0.5f;
        [ShowInInspector, ReadOnly] protected float _healthRecoveryCooldown = 0;

        [field: SerializeField, FoldoutGroup("Character Stats")] public float MaxStamina { get; protected set; }
        [FoldoutGroup("Character Stats"), SerializeField] protected float _staminaCurrent;

        [FoldoutGroup("Character Stats"), SerializeField] protected float interactionDistance;
        [field: SerializeField, FoldoutGroup("Character Stats")] public float MagicaMax { get; protected set; }

        [FoldoutGroup("Character Stats"), SerializeField] protected float sprintSpeed = 200f;

        [FoldoutGroup("Character Stats, Stamina"), SerializeField, Range(0, 100)] protected float staminaReductionModifier = 1;
        [FoldoutGroup("Character Stats, Stamina"), SerializeField, Range(0, 1)] protected float staminaRecoveryModifier = 0.5f;

        [Header("If the character's stamina ever falls to 0, it will not Recovery for the given time")]
        [FoldoutGroup("Character Stats, Stamina"), SerializeField] protected float staminaExhaustionTime = 2.5f;

        [FoldoutGroup("Character Stats"), SerializeField] protected float magicaRecoverySpeed = 0.5f;

        [Header("The multiplier for the stamina recovery while the character is in blocking stance")]
        [FoldoutGroup("Character Stats, Stamina"), SerializeField] protected float staminaRecoveryBlockingMultiplier = 0.2f;

        [Header("The time after a successful block that the character will be unable to regain stamina")]
        [FoldoutGroup("Character Stats, Stamina"), SerializeField] protected float staminaPostBlockRecoveryDelayDuration = 1f;

        [FoldoutGroup("Character Stats, Combat"), SerializeField] protected float chargeAttackMinTime = 0.4f, chargeAttackMaxTime = 1;
        [FoldoutGroup("Character Stats, Combat"), SerializeField] protected float chargeAttackMinHoldTime = 0.25f;
        [FoldoutGroup("Character Stats, Combat"), SerializeField] protected float minHeavyDamageMultiplier = 1.25f, maxHeavyDamageMultiplier = 2.5f;

        [Header("The stamina drain multiplier to an attacked target with a shield drawn")]
        [FoldoutGroup("Character Stats, Combat"), SerializeField] protected float minHeavyBlockStaminaMultiplier = 1f, maxHeavyBlockStaminaMultiplier = 2.5f;

        protected float _damageChargeMultiplier = 1;
        protected float _chargeHoldTime;
        protected bool _holdingCharge;

        // Combat Context Variables
        protected float _exhaustionEndTime; // The time remaining until the NPC can regain stamina after being exhausted
        protected float _blockRegenEndTime;

        public Action OnAttackStart, OnAttackEnd;

        protected float staminaCurrent
        {
            get => _staminaCurrent;
            set
            {
                if (value <= 0f)
                {
                    if (Time.time >= _exhaustionEndTime )
                    {
                        _exhaustionEndTime  = Time.time + staminaExhaustionTime;
                    }
                }

                _staminaCurrent = Mathf.Clamp(value, 0, MaxStamina);

                // Update the sprint time
                OnUpdateStaminaBar?.Invoke(_staminaCurrent);
            }
        }

        protected float _magicaCurrent;
        protected float magicaCurrent
        {
            get => _magicaCurrent;
            set
            {
                _magicaCurrent = Mathf.Clamp(value, 0, MagicaMax);

                // Update the magica bar
                OnUpdateMagicaBar?.Invoke(_magicaCurrent);
            }
        }


        public float HitPoints
        {
            get => _hitPointsCurrent;

            set
            {
                // Store the current hp
                float hpCurrent = _hitPointsCurrent;

                if (value < 0)
                {
                    _hitPointsCurrent = 0;
                }

                else if (value > MaxHealth)
                {
                    _hitPointsCurrent = MaxHealth;
                }

                else
                {
                    _hitPointsCurrent = value;
                }

                // The character just lost health
                if (_hitPointsCurrent < hpCurrent)
                {
                    _healthRecoveryCooldown = healthRecoveryTime;
                }

                if (_hitPointsCurrent == 0)
                {
                    DoDeath();
                }

                UpdateHealthBar();
            }
        }


        public float MovementSpeed
        {
            get => movementSpeed;
            set => SetMovementSpeed(value);
        }

        //Front and back sorting layers for weapons
        private const int frontLayer = 8;
        private const int backLayer = 0;

        // Weapon and Shield
        public WeaponItem equippedWeapon;
        public ShieldItem equippedShield;

        // Bar Update Events
        public Action<float> OnUpdateHealthBar;
        public Action<float> OnUpdateStaminaBar;
        public Action<float> OnUpdateMagicaBar;
        public Action<NPCState> OnUpdateCombatState;

        public Action OnHideHealthBar, OnShowHealthBar;

        #endregion

        #region Current Character State

        [SerializeField] protected CharacterState _state = CharacterState.Normal;
        protected CharacterState state
        {
            get { return _state; }
        }

        protected ViewDirection currentViewDirection = ViewDirection.BottomRight;
        protected bool weaponSheathed = true;
        protected string currentAnimationState;

        #endregion

        #region Attack Variables

        [SerializeField] protected float attackAnimationDuration;
        public float AttackAnimationDuration { get => attackAnimationDuration; }

        protected float attackHitMark;
        [SerializeField] protected float blockAnimationDuration = 0.3f;
        protected WeaponMode weaponMode = WeaponMode.Slash;
        public WeaponMode WeaponMode { get => weaponMode; }
        protected WeaponType weaponType = WeaponType.Unarmed;
        [SerializeField] private float weaponRange;

        [SerializeField] protected float weaponAngle;

        [FoldoutGroup("Projectile Firing")]
        [SerializeField] protected float projectileSpawnDistance = 0.45f;

        [FoldoutGroup("Projectile Firing/Spell Casting")]
        [SerializeField] protected float spellCastTime = 0.4f;
        [FoldoutGroup("Projectile Firing/Spell Casting")]
        [SerializeField] protected float spellFireTime = 0.4f;
        [FoldoutGroup("Projectile Firing/Spell Casting")]
        [SerializeField] protected float spellThrowTime = 0.5f;
        [FoldoutGroup("Projectile Firing/Spell Casting")]
        [SerializeField] protected float spellSpawnDistance = 0.25f;

        //[SerializeField] protected bool shieldRaise;
        // Temp
        // [SerializeField] protected float shieldRaiseTime = 0.3f;

        #endregion

        #region Other Constants and Variables

        private const float blockAngle = 90;
        protected bool sucessfulEnemyBlock = false;

        // This value is marked true if the player is holding a spell, if true, magica will not Recovery
        protected bool isHoldingSpell;

        #endregion

        #region Temporary Attack Duration Values

        float slashAttackDuration = 0.26f, swingAttackDuration = 0.3f, thrustAttackDuration = 0.3f, twoHandedAttackDuration = 0.6f;
        float slashHitMark = 0.1f, swingAttackMark = 0.1f, thrustAttackMark = 0.1f, twoHandedHitMark = 0.3f;

        protected List<float> twoHandedAnimationDurations = new List<float> { 0.6f, 0.5f };
        protected List<float> twoHandedHitMarks = new List<float> { 0.3f, 0.2f };

        #endregion

        #region Events

        public Action OnHit;
        public Action OnDeath;
        public static Action<NPC> OnActivateLootPoint;

        #endregion

        #region Factions and Status Effects

        [SerializeField] public List<Faction> factions;
        protected List<ActiveStatusEffect> statusEffects = new();

        #endregion

        #region View Direction

        public ViewDirection characterViewDirection = ViewDirection.BottomRight;

        #endregion

        #region Abstract Methods
        public abstract void SetMovementSpeed(float value);

        #endregion

        #region State Control

        /// <summary>
        /// Returns true if the character is in a state where they are able to move
        /// </summary>
        /// <returns></returns>
        protected bool InRunningState()
        {
            return state == CharacterState.Normal;
        }

        protected bool InAttackingState()
        {
            return
                state == CharacterState.Attacking ||
                state == CharacterState.Blocking;
        }

        public bool IsBlocking()
        {
            return state == CharacterState.Blocking;
        }

        protected virtual void SetState(CharacterState newState)
        {
            // Prevents state changes after death
            if (state == CharacterState.Death)
                return;

            switch (newState)
            {
                case CharacterState.Attacking:
                    animationController.StopWalkLoop();
                    break;

                case CharacterState.Blocking:
                    animationController.StopWalkLoop();
                    break;

            }

            _state = newState;
        }

        #endregion

        public bool IsAttacking()
        {
            return state == CharacterState.Attacking;
        }

        public bool IsRangedAttacker()
        {
            if (equippedWeapon == null)
                return false;

            if (equippedWeapon.weaponMode == WeaponMode.Ranged)
                return true;

            return false;
        }

        public bool CanBlock()
        {
            if (HasShield() || HasBlockableWeapon())
                return true;

            return false;
        }

        public bool HasShield()
        {
            return equippedShield != null;
        }

        // Returns true if the character has a weapon that can block (eg. longsword)
        public bool HasBlockableWeapon()
        {
            if (equippedWeapon != null && equippedWeapon.canBlock)
            {
                return true;
            }

            return false;
        }

        #region Singleton Fields

        protected EventBus eventBus;
        protected AudioManager audioManager;
        protected FMODEvents fmodEvents;

        #endregion

        #region Public Getters

        public int attackIteration = 0;

        protected ViewDirection viewDirection
        {
            get => characterViewDirection;

            set
            {
                characterViewDirection = value;
            }
        }

        public float WeaponRange
        {
            get => weaponRange;

            set
            {
                weaponRange = value;
                attackCircle.radius = value;
            }
        }

        public float CurrentHealthPercentage
        {
            get => (_hitPointsCurrent / MaxHealth);
        }

        public float CurrentHealth => _hitPointsCurrent;

        public float CurrentStaminaPercentage
        {
            get => (staminaCurrent / MaxStamina);
        }

        public float CurrentStamina => _staminaCurrent;

        public virtual float PhysDamageTotal
        {
            get => equippedWeapon.weaponDamage * _damageChargeMultiplier;
        }

        public float ArmourTotal
        {
            get => 0;
            //get => equipmentHandler.TotalPhyiscalResistance;
        }

        #endregion

        protected abstract void UpdateHealthBar();

        protected virtual void Awake()
        {
            if (rigidBody == null)
                rigidBody = GetComponent<Rigidbody2D>();

            if (hitColliderCircle == null)
            {
                hitColliderCircle = GetComponent<CircleCollider2D>();
            }

            if (attackCircle == null)
                attackCircle = transform.GetChild(1).GetComponent<CircleCollider2D>();

            _blockRegenEndTime = 0;
            _exhaustionEndTime = 0;
        }

        protected virtual void Start()
        {
            eventBus = EventBus.Instance;
            audioManager = AudioManager.Instance;
            fmodEvents = FMODEvents.Instance;

            if (eventBus != null)
            {
                SubscribeToBusEvents();
            }
            else
            {
                Debug.LogError("Event bus is null! Cannot subscribe to events!");
            }

            defaultMovementSpeed = movementSpeed;
            StartCoroutine(CycleStatusEffects());

            // By default, the character is unarmed
            //SetUnarmed();
        }

        protected virtual void Update()
        {
            if (state != CharacterState.Death)
            {
                RecoverMagica();
                RecoverHealth();
                RecoverStamina();
            }
        }

        protected void RecoverMagica()
        {
            if (isHoldingSpell)
                return;

            if (magicaCurrent < MagicaMax)
            {
                magicaCurrent += (MagicaMax * magicaRecoverySpeed) * Time.deltaTime;
                // magicaCurrent += magicaRecoverySpeed * Time.deltaTime;
            }
        }

        /// <summary>
        /// Constantly recovers the character's stamina, applying factors such as the character currently blocking
        /// </summary>
        protected void RecoverStamina()
        {
            if (staminaCurrent < MaxStamina)
            {

                // If we’re still in block‐regen pause, skip regen
                if (Time.time < _blockRegenEndTime )
                    return;

                // If we’re still in exhaustion, skip regen
                if (Time.time < _exhaustionEndTime )
                    return;


                // If the character is attacking
                if (state == CharacterState.Attacking)
                {
                    return;
                }

                float baseRecovery = (MaxStamina * staminaRecoveryModifier) * Time.deltaTime;

                if (state == CharacterState.Blocking)
                    baseRecovery *= staminaRecoveryBlockingMultiplier;
                

                staminaCurrent += baseRecovery;
            }
        }

        /// <summary>
        /// Constantly recovers the character's health, unless the health recovery cooldown is not set to 0, in which case, the method will reduce the cooldown time
        /// </summary>
        protected void RecoverHealth()
        {
            if (_healthRecoveryCooldown > 0)
            {
                _healthRecoveryCooldown -= Time.deltaTime;
                return;
            }

            if (HitPoints < MaxHealth)
            {
                float baseRecovery = (MaxHealth * healthRecoveryModifier) * Time.deltaTime;

                HitPoints += baseRecovery;
            }
        }

        protected void ReduceStamina(float amount)
        {
            staminaCurrent -= amount;
            if (staminaCurrent < 0)
                staminaCurrent = 0;
        }

        protected virtual void SubscribeToBusEvents() { }

        #region Status Effect Methods

        public void ApplyStatusEffect(StatusEffect effect, float duration, Transform source = null)
        {
            bool addVisualEffect = true;

            if (effect == StatusEffect.TempSlow)
            {
                StartCoroutine(SlowMovementSpeed(duration));
                addVisualEffect = false;
            }

            // Check if the character already has the status effect, if so, refresh the duration
            foreach (var statusEffect in statusEffects)
            {
                // If the character already has the status effect
                if (statusEffect.type == effect)
                {
                    // If the new duration is greater than the current duration, refresh the duration
                    if (statusEffect.duration < duration)
                    {
                        statusEffect.duration = duration;
                    }

                    return;
                }
            }

            ActiveStatusEffect activeStatusEffect;

            if (addVisualEffect)
            {
                GameObject statusEffectObject = Instantiate(statusEffectPrefab, statusEffectsParent);
                statusEffectObject.GetComponent<SpriteLibrary>().spriteLibraryAsset = StatusEffectManager.instance.GetLibraryAssetForEffect(effect);
                statusEffectObject.gameObject.SetActive(true);
                activeStatusEffect = new(effect, duration, statusEffectObject);
            }
            else
            {
                activeStatusEffect = new(effect, duration, null);
            }


            statusEffects.Add(activeStatusEffect);
        }

        protected virtual IEnumerator CycleStatusEffects()
        {
            while (true)
            {
                List<ActiveStatusEffect> effectsToRemove = new(statusEffects);
                foreach (var effect in effectsToRemove)
                {
                    switch (effect.type)
                    {
                        case StatusEffect.Poison:
                            HitPoints -= 2.5f;
                            break;
                    }

                    effect.duration--;

                    if (effect.duration <= 0)
                    {
                        Destroy(effect.visualEffectObj);
                        statusEffects.Remove(effect);
                    }
                }

                yield return new WaitForSeconds(1);
            }
        }

        /// <summary>
        /// Return true if the character has the given status effect
        /// </summary>
        public bool GetStatusEffect(StatusEffect effect)
        {
            foreach (var statusEffect in statusEffects)
            {
                if (statusEffect.type == effect)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        /// <summary>
        /// Slow the movement speed and increase it gradually over the duration
        /// </summary>
        /// <param name="duration"></param>
        protected virtual IEnumerator SlowMovementSpeed(float duration)
        {
            float slowAmount = 0.5f;
            movementSpeed = defaultMovementSpeed * slowAmount;

            while (movementSpeed < defaultMovementSpeed)
            {
                // Reduce the slow over the duration
                movementSpeed += (defaultMovementSpeed - movementSpeed) / duration;

                yield return new WaitForEndOfFrame();
            }

            yield break;
        }

        /// <summary>
        /// Check if the character is in the given faction
        /// </summary>
        /// <param name="faction"></param>
        /// <returns>Returns true if the given faction is part of this character's factions</returns>
        public bool IsInFaction(Faction faction)
        {
            return factions.Contains(faction);
        }

        public bool IsInFaction(List<Faction> factions)
        {
            foreach (var faction in factions)
            {
                if (IsInFaction(faction))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the other character is in any of this character's factions
        /// </summary>
        /// <param name="otherCharacter"></param>
        /// <returns>Returns true if the other character and this character share a faction</returns>
        public bool IsInFaction(Character otherCharacter)
        {
            foreach (var faction in factions)
            {
                if (otherCharacter.IsInFaction(faction))
                    return true;
            }

            return false;
        }

        #region Public Methods

        public bool IsDead()
        {
            if (state == CharacterState.Death || !gameObject.activeInHierarchy)
                return true;

            return false;
        }

        public bool IsDiagonalAttacking()
        {
            if (weaponMode == WeaponMode.Slash || weaponMode == WeaponMode.TwoHanded || weaponMode == WeaponMode.Spell)
                return true;

            return false;
        }

        public bool IsAllDirectionalAttacking()
        {
            if (weaponMode == WeaponMode.Swing || weaponMode == WeaponMode.Thrust)
                return true;

            return false;
        }

        public CircleCollider2D GetCollider()
        {
            return hitColliderCircle;
        }

        public void LookTowards(Transform source)
        {
            Vector3 direction = (source.position - transform.position).normalized;
            float angle = Vector2.SignedAngle(Vector2.right, direction);

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

        #endregion

        protected virtual void DoDeath()
        {
            return;
        }

        //public void AddItemByName(Item item)
        //{
        //
        //    .AddItemByName(item);
        //}

        //public void AddItem(Item item)
        //{
        //    inventory.AddItem(item);
        //}

        //public void AddItem(Item item, int quantity)
        //{
        //    item.quantity = quantity;
        //    inventory.AddItem(item);
        //}

        //public void AddItem(Item item, int quantity, string GUID)
        //{
        //    item.quantity = quantity;
        //    item.SetItemID(GUID);
        //    inventory.AddItem(item);
        //}

        //public List<Item> GetInventoryItems()
        //{
        //    return inventory.GetItems();
        //}


        //public void ClearInventory()
        //{
        //    inventory.ClearItems();
        //}

        #region Attack Methods   

        /// <summary>
        /// Return true if the character is able to block the incoming damage and if so, excecute any blocking logic
        /// </summary>
        /// <param name="incomingDamage"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        protected bool TryBlockIncomingDamage(DamagePacket damagePacket)
        {
            if (state == CharacterState.Blocking)
            {
                Vector2 blockVector = Vector2.right;

                switch (viewDirection)
                {
                    case ViewDirection.BottomLeft:
                        blockVector = new Vector2(-0.1f, -0.1f);
                        break;

                    case ViewDirection.BottomRight:
                        blockVector = new Vector2(0.1f, -0.1f);
                        break;

                    case ViewDirection.TopLeft:
                        blockVector = new Vector2(-0.1f, 0.1f);
                        break;

                    case ViewDirection.TopRight:
                        blockVector = new Vector2(0.1f, 0.1f);
                        break;
                }


                Vector3 direction = (damagePacket.source.position - transform.position).normalized;
                float angle = Vector2.SignedAngle(blockVector, direction);

                // Only block the damage if the angle the character is blocking in overlaps with the attacking angle, otherwise the block will fail
                if (Mathf.Abs(angle) <= blockAngle)
                {
                    // Play the shield block sound
                    audioManager.PlayOneShot(fmodEvents.metalShieldBlockSound, transform.position);

                    Character charComponent = damagePacket.source.GetComponent<Character>();

                    if (charComponent != null)
                    {
                        charComponent.sucessfulEnemyBlock = true;
                    }

                    // Block stamina regeneration for a period of time
                    DoBlockStaminaRecoveryTimeout();

                    // If the character can afford the cost to block, remove the stamina and return true, leading to a successful block
                    if (TryPayBlockStaminaCost(damagePacket.blockStaminaDrawMultiplier))
                    {
                        // Intake a portion of the base damage depending on the shield type
                        DealShieldDamage(damagePacket);

                        animationController.DoBlockAnimation(viewDirection);
                        StartCoroutine(ExitBlockState());

                        // Blocked successfully, so return true
                        return true;
                    }

                    // Otherwise, the character will be staggered and take full damage
                    else
                    {
                        HitPoints -= damagePacket.damageAmount;

                        if (HitPoints > 0)
                        EnterStaggerState();

                        // Block failed, so return false
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Blocks stamina regeneraton either until the regen pause duration has passed, or the character is no longer blocking
        /// </summary>
        private void DoBlockStaminaRecoveryTimeout()
        {
            _blockRegenEndTime  = Time.time + staminaPostBlockRecoveryDelayDuration;
        }

        private IEnumerator ExitBlockState()
        {
            // TODO: establish block hit duration
            yield return new WaitForSeconds(0.2f);

            if (state == CharacterState.Blocking)
            {
                animationController.DoRaiseBlockAnimation(viewDirection);
            }

            yield break;
        }

        protected void BlockWithAngle(float angle)
        {
            //  DoRaiseShieldSound();

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

            // Update the block weapon sorting layer
            animationController.UpdateBlockWeaponSortingLayer();
            animationController.DoRaiseBlockAnimation(viewDirection);
        }

        /// <summary>
        /// If the character has enough stamina, returns true and removes the samina, otherwise returns false
        /// </summary>
        /// <returns></returns>
        protected bool TryPayBlockStaminaCost(float staminaDrawMultiplier)
        {
            if (equippedShield == null)
            {
                Debug.LogError("Error: Trying to pay a block stamina cost without a shield equipped!");
                return false;
            }

            // For now, remove 15% of the max stamina to the current
            float totalCost = (MaxStamina * 0.15f) * staminaDrawMultiplier;

            // Return false if the character cannot afford the block cost, and sets the current stamina to 0
            if (staminaCurrent - totalCost <= 0)
            {
                staminaCurrent = 0;
                return false;
            }

            staminaCurrent -= (totalCost);

            return true;
        }

        protected void DealShieldDamage(DamagePacket damagePacket)
        {
            if (equippedShield == null)
            {
                Debug.LogError("Error: Trying to pay a block stamina cost without a shield equipped!");
                return;
            }

            // For now, remove between 85 and 100 percent of the incoming damage
            float percent = UnityEngine.Random.Range(85, 101);
            percent /= 100f;
            float finalDamage = damagePacket.damageAmount - (damagePacket.damageAmount * percent);

            //Debug.Log($"Reduced incoming damage from {baseDamage} to {finalDamage}");

            HitPoints -= finalDamage;
        }

        /// <summary>
        /// Return the ViewDirection that faces the given angle
        /// </summary>
        /// <param name="angle"></param>
        protected ViewDirection GetWalkDirecitonFromAngle(float angle)
        {
            if (angle >= 0 && angle < 90)
            {
                return ViewDirection.BottomLeft;
            }
            else if (angle >= 90 && angle < 180)
            {
                return ViewDirection.BottomRight;
            }
            else if (angle >= -180 && angle < -90)
            {
                return ViewDirection.TopRight;
            }
            else if (angle >= -90 && angle < 0)
            {
                return ViewDirection.TopLeft;
            }

            Debug.LogError("Angle is out of bounds! Angle: " + angle);
            return ViewDirection.BottomRight;
        }

        public virtual void AttackWithAngle(float angle)
        {
            DoWeaponSound();

            AttackDirection attackDirection = AttackDirection.BottomRight;

            // If the equipped weapon can attack in all directions, determine the attack direction based on the angle
            if (IsAllDirectionalAttacking())
            {
                if (angle < 0)
                {
                    angle += 360; // Make sure angle is positive for easier comparisons
                }

                float sectorAngle = 360f / 8; // Split the circle into 8 equal sectors

                int sector = Mathf.FloorToInt((angle + sectorAngle / 2) / sectorAngle) % 8;

                switch (sector)
                {
                    case 0:
                        viewDirection = ViewDirection.BottomRight;
                        attackDirection = AttackDirection.Right;
                        break;

                    case 1:
                        viewDirection = ViewDirection.TopRight;
                        attackDirection = AttackDirection.TopRight;
                        break;

                    case 2:
                        if (viewDirection == ViewDirection.BottomLeft)
                            viewDirection = ViewDirection.TopLeft;

                        if (viewDirection == ViewDirection.BottomRight)
                            viewDirection = ViewDirection.TopRight;

                        attackDirection = AttackDirection.Up;
                        break;

                    case 3:
                        viewDirection = ViewDirection.TopLeft;
                        attackDirection = AttackDirection.TopLeft;
                        break;

                    case 4:
                        viewDirection = ViewDirection.BottomLeft;
                        attackDirection = AttackDirection.Left;
                        break;

                    case 5:
                        viewDirection = ViewDirection.BottomLeft;
                        attackDirection = AttackDirection.BottomLeft;
                        break;

                    case 6:
                        if (viewDirection == ViewDirection.TopRight)
                            viewDirection = ViewDirection.BottomRight;

                        if (viewDirection == ViewDirection.TopLeft)
                            viewDirection = ViewDirection.BottomLeft;
                        attackDirection = AttackDirection.Down;
                        break;

                    case 7:
                        viewDirection = ViewDirection.BottomRight;
                        attackDirection = AttackDirection.BottomRight;
                        break;
                }
            }

            // If the attack is diagonal, determine the diagonal direction of the attack based on the angle 
            else if (IsDiagonalAttacking())
            {
                if (angle >= 0 && angle < 90)
                {
                    viewDirection = ViewDirection.TopRight;
                    attackDirection = AttackDirection.TopRight;
                }
                else if (angle >= 90 && angle < 180)
                {
                    viewDirection = ViewDirection.TopLeft;
                    attackDirection = AttackDirection.TopLeft;
                }
                else if (angle >= -180 && angle < -90)
                {
                    viewDirection = ViewDirection.BottomLeft;
                    attackDirection = AttackDirection.BottomLeft;
                }
                else if (angle >= -90 && angle < 0)
                {
                    viewDirection = ViewDirection.BottomRight;
                    attackDirection = AttackDirection.BottomRight;
                }
            }

            // If the attack is not diagonal, determine the straight direction of the attack based on the angle
            else
            {
                if (angle >= 45 && angle < 135)
                {
                    attackDirection = AttackDirection.Up;

                    if (viewDirection == ViewDirection.BottomLeft)
                        viewDirection = ViewDirection.TopLeft;

                    if (viewDirection == ViewDirection.BottomRight)
                        viewDirection = ViewDirection.TopRight;
                }
                else if (angle >= 135 || angle < -135)
                {
                    attackDirection = AttackDirection.Left;

                    viewDirection = ViewDirection.BottomLeft;
                }
                else if (angle >= -135 && angle < -45)
                {
                    attackDirection = AttackDirection.Down;

                    if (viewDirection == ViewDirection.TopRight)
                        viewDirection = ViewDirection.BottomRight;

                    if (viewDirection == ViewDirection.TopLeft)
                        viewDirection = ViewDirection.BottomLeft;
                }
                else if (angle >= -45 && angle < 45)
                {
                    attackDirection = AttackDirection.Right;
                    viewDirection = ViewDirection.BottomRight;
                }
            }

            animationController.UpdateWeaponSortingLayer();
            animationController.DoWeaponAttackAnimation(attackDirection);
            StartCoroutine(AttackHitMark(attackDirection));
        }

        protected void GetEnemiesInArea(AttackDirection attackDirection)
        {
            Vector2 attackVector = animationController.AttackDirectionPairs[attackDirection];

            // Get the center position of the CircleCollider2D
            Vector2 center = (Vector2)attackCircle.transform.position + attackCircle.offset;

            // Get all colliders within the circle's area
            Collider2D[] colliders = Physics2D.OverlapCircleAll(center, attackCircle.radius);

            // Create a list to store the GameObjects that are colliding with the circle
            List<GameObject> collidingGameObjects = new();

            // Iterate through the colliders array and add the GameObjects to the list
            foreach (Collider2D collider in colliders)
            {
                if (!collider.CompareTag("HitCollider"))
                {
                    continue;
                }

                GameObject collisionObj = collider.transform.parent.gameObject;

                if (collisionObj == this.gameObject)
                    continue;

                Vector3 direction = (collisionObj.transform.position - transform.position).normalized;
                float angle = Vector2.SignedAngle(attackVector, direction);

                // Considers the attack a "miss" if the angle between the two characters is greater than 45 degrees
                //  Debug.Log($"Angle : {Mathf.Abs(angle)} Weapon Angle: {weaponAngle}");

                if (Mathf.Abs(angle) > weaponAngle)
                    continue;

                Character hitCharacter = collisionObj.GetComponent<Character>();

                if (IsInFaction(hitCharacter))
                {
                    continue;
                }

                DealDamageToCharacter(hitCharacter);
                collidingGameObjects.Add(collisionObj);
            }
        }

        protected virtual void DealDamageToCharacter(Character character)
        {
            DamagePacket damagePacket = new DamagePacket
            {
                damageAmount = PhysDamageTotal,
                source = this.transform,
                blockStaminaDrawMultiplier = GetHeavyBlockStaminaDrawMultiplier()
            };

            OnDamageDealt(damagePacket.damageAmount);

            character.EnterHitState(damagePacket);
        }

        protected virtual void OnDamageDealt(float damage)
        {

        }

        public virtual Vector3 GetVelocity()
        {
            return Vector3.zero;
        }

        protected virtual void EnterHitState(DamagePacket damagePacket)
        {
            if (TryBlockIncomingDamage(damagePacket))
            {
                return;
            }

            audioManager.PlayOneShot(fmodEvents.humanHitSound, transform.position);

            animationController.DoFlashHit();

            HitPoints -= damagePacket.damageAmount;
        }

        protected virtual void EnterStaggerState()
        {
            SetState(CharacterState.Hit);

            animationController.DoFlashHit();

            animationController.DoStaggerAnimation(viewDirection);

            StartCoroutine(Utils.WaitDurationAndExecute(.3f, () =>
            {
                SetState(CharacterState.Normal);
            }));
        }

        public void DealDamage(DamagePacket damagePacket)
        {
            EnterHitState(damagePacket);
        }

        // Based on the current charge hold time, return how much stamina the heavy attack will cost
        protected float GetCurrentHeavyAttackStaminaCost()
        {
            if (equippedWeapon == null)
            {
                Debug.LogWarning("Trying to get heavy attack stamina cost without an equipped weapon!");
                return 0;
            }

            if (state != CharacterState.Attacking)
            {
                Debug.LogWarning("Trying to get heavy attack stamina cost when not attacking!");
                return 0;
            }

            float t = Mathf.Clamp01((_chargeHoldTime - chargeAttackMinTime) / (chargeAttackMaxTime - chargeAttackMinTime));

            float staminaCost = Mathf.Lerp(equippedWeapon.heavyAttackMinStaminaCost, equippedWeapon.heavyAttackMaxStaminaCost, t);

            return staminaCost;
        }

        protected float GetHeavyAttackDamageMultiplier()
        {
            if (equippedWeapon == null)
            {
                Debug.LogWarning("Trying to get heavy attack damage multiplier without an equipped weapon!");
                return 1;
            }
            if (state != CharacterState.Attacking)
            {
                Debug.LogWarning("Trying to get heavy attack damage multiplier when not attacking!");
                return 1;
            }

            float t = Mathf.Clamp01((_chargeHoldTime - chargeAttackMinTime) / (chargeAttackMaxTime - chargeAttackMinTime));
            float damageMultiplier = Mathf.Lerp(minHeavyDamageMultiplier, maxHeavyDamageMultiplier, t);

            //Debug.Log($"Muiltiplier : {damageMultiplier}");

            return damageMultiplier;
        }

        protected float GetHeavyBlockStaminaDrawMultiplier()
        {
            if (equippedWeapon == null)
            {
                Debug.LogWarning("Trying to get heavy attack damage multiplier without an equipped weapon!");
                return 1;
            }
            if (state != CharacterState.Attacking)
            {
                Debug.LogWarning("Trying to get heavy attack damage multiplier when not attacking!");
                return 1;
            }
            if (_chargeHoldTime < chargeAttackMinTime)
            {

                return 1;
            }

            float t = Mathf.Clamp01((_chargeHoldTime - chargeAttackMinTime) / (chargeAttackMaxTime - chargeAttackMinTime));
            float blockStaminaDrawMultiplier = Mathf.Lerp(minHeavyBlockStaminaMultiplier, maxHeavyBlockStaminaMultiplier, t);
            return blockStaminaDrawMultiplier;
        }

        public bool MinChargeTimeMet()
        {
            return _chargeHoldTime > chargeAttackMinHoldTime;
        }

        #endregion

        //public void DrinkPotion(Item item)
        //{
        //    if (item.itemType != ItemType.Consumable)
        //    {
        //        Debug.LogWarning("Item is not a consumable!");
        //        return;
        //    }

        //    if (item.ConsumableType == ConsumableType.HealthPotion)
        //    {
        //        HealForAmount(item.ConsumableQuantity);
        //    }

        //    SoundManager.PlaySound(soundLibrary.RandomClip(soundLibrary.drinkPotionSounds));
        //}

        //public void EatFood(Item item)
        //{
        //    if (item.itemType != ItemType.Consumable)
        //    {
        //        Debug.LogWarning("Item is not a consumable!");
        //        return;
        //    }

        //    if (item.ConsumableType == ConsumableType.Food)
        //    {
        //        HealForAmount(item.ConsumableQuantity);
        //    }

        //    SoundManager.PlaySound(soundLibrary.RandomClip(soundLibrary.eatFruitSounds));
        //}

        public void HealForAmount(float amount)
        {
            HitPoints += amount;
        }

        protected virtual IEnumerator AttackHitMark(AttackDirection direction)
        {
            if (weaponMode == WeaponMode.Ranged || weaponMode == WeaponMode.Spell)
                yield break;

            if (equippedWeapon != null && equippedWeapon.weaponType == WeaponType.LongSword)
            {
                yield return new WaitForSeconds(twoHandedHitMarks[attackIteration]);
            }
            else
            {
                yield return new WaitForSeconds(attackHitMark);
            }

            GetEnemiesInArea(direction);

            yield break;
        }

        public bool IsWeaponSheathed()
        {
            return weaponSheathed;
        }

        public void SheathWeapon()
        {
            weaponSheathed = true;
        }

        public void UnsheathWeapon()
        {
            weaponSheathed = false;
        }

        public void ToggleWeaponSheath()
        {
            weaponSheathed = !weaponSheathed;
        }

        #region Equipment Methods

        public void SetEquippedWeapon(WeaponItem weapon)
        {
            if (weapon == null)
            {
                SetUnarmed();
                return;
            }

            SetWeaponMode(weapon.weaponMode);
            SetWeaponType(weapon.weaponType);
            SetMeleeWeaponValues(weapon.weaponRange, weapon.weaponAngle);
            equippedWeapon = weapon;
        }

        public void SetEquippedShield(ShieldItem shield)
        {
            if (shield == null)
            {
                equippedShield = null;
                return;
            }

            equippedShield = shield;
        }

        public void SetUnarmed()
        {
            Debug.Log("Setting weapon mode unarmed");
            equippedWeapon = null;
            SetWeaponMode(WeaponMode.Slash);
            SetWeaponType(WeaponType.Unarmed);
            SetMeleeWeaponValues(0, 0);
        }

        public void SetWeaponMode(WeaponMode type)
        {
            weaponMode = type;

            switch (type)
            {
                case WeaponMode.Slash:
                    attackAnimationDuration = slashAttackDuration;
                    attackHitMark = slashHitMark;
                    break;

                case WeaponMode.Swing:
                    attackAnimationDuration = swingAttackDuration;
                    attackHitMark = swingAttackMark;
                    break;

                case WeaponMode.Thrust:
                    attackAnimationDuration = thrustAttackDuration;
                    attackHitMark = thrustAttackMark;
                    break;

                case WeaponMode.TwoHanded:
                    attackAnimationDuration = twoHandedAttackDuration;
                    attackHitMark = twoHandedHitMark;
                    break;
            }
        }

        public void SetWeaponType(WeaponType type)
        {
            weaponType = type;
        }

        public void SetMeleeWeaponValues(float range, float angle)
        {
            WeaponRange = range;
            weaponAngle = angle;
        }

        #endregion

        #region Audio Methods

        protected abstract void PlayHitSound();
        protected abstract void PlayDeathSound();
        protected abstract void PlayAttackSound();

        private void DoWeaponSound()
        {
            if (weaponMode == WeaponMode.Spell)
            {
                audioManager.PlayOneShot(fmodEvents.GetCorrespondingSpellCastSound(equippedWeapon.SpellType), transform.position);

                return;
            }

            switch (weaponType)
            {
                case WeaponType.Sword:
                    audioManager.PlayOneShot(fmodEvents.mediumSlashSound, transform.position);
                    break;

                case WeaponType.Flail:
                    audioManager.PlayOneShot(fmodEvents.flailSound, transform.position);
                    break;

                case WeaponType.Spear:
                    audioManager.PlayOneShot(fmodEvents.thrustSound, transform.position);
                    break;

                case WeaponType.LongSword:
                    audioManager.PlayOneShot(fmodEvents.heavySlashSound, transform.position);
                    break;

                case WeaponType.Bow:
                    audioManager.PlayOneShot(fmodEvents.bowDrawSound, transform.position);
                    break;
            }
        }

        private void DoRaiseShieldSound()
        {
            audioManager.PlayOneShot(fmodEvents.metalShieldBlockSound, transform.position);
        }

        #endregion
    }
}

namespace Characters
{
    /// <summary>
    /// Projectile fire data that is sent over an eventBus publish
    /// </summary>
    public struct ProjectileFireData
    {
        public Vector3 sourcePos;
        public float fireAngle;
        public float projectileSpawnDistance;
        public GameObject projectilePrefab;
        public GameObject exclusionObject;

        public ProjectileFireData(Vector3 sourcePos, float fireAngle, float projectileSpawnDistance, GameObject projectilePrefab, GameObject exclusionObject)
        {
            this.sourcePos = sourcePos;
            this.fireAngle = fireAngle;
            this.projectileSpawnDistance = projectileSpawnDistance;
            this.projectilePrefab = projectilePrefab;
            this.exclusionObject = exclusionObject;
        }
    }

    public struct SpellProjectileFireData
    {
        public Vector3 sourcePos;
        public float fireAngle;
        public float projectileSpawnDistance;
        public SpellType spellType;
        public GameObject exclusionObject;

        public SpellProjectileFireData(Vector3 sourcePos, float fireAngle, float projectileSpawnDistance, SpellType spellType, GameObject exclusionObject)
        {
            this.sourcePos = sourcePos;
            this.fireAngle = fireAngle;
            this.projectileSpawnDistance = projectileSpawnDistance;
            this.spellType = spellType;
            this.exclusionObject = exclusionObject;
        }
    }
}