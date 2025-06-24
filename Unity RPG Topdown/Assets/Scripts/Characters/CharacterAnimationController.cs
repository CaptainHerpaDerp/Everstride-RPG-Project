using AudioManagement;
using Core.Enums;
using Items;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Characters
{
    public class CharacterAnimationController : MonoBehaviour
    {
        // The currently playing animation
        protected string currentAnimationState;

        [Header("The parent of all the cosmetics")]
        [SerializeField] protected Transform cosmeticsTransform;

        private List<Transform> cosmeticTransforms;

        // The iteration of the attack animation, used for weapons with multiple attack animations
        protected int AttackIteration => parentCharacter.attackIteration;

        //Front and back sorting layers for weapons
        private const int frontLayer = 8;
        private const int backLayer = 0;

        // The coroutine for the flash hit effect
        private IEnumerator flashHitCR = null;

        [SerializeField] private Character parentCharacter;
        [SerializeField] protected SpriteRenderer weaponSR, weaponTrailSR, shieldSR, shieldTrailSR;

        // The parent for the weapon trail to be excluded from hit effects
        [SerializeField] protected Transform weaponTrailTransform;

        // The colour of the sprite when hit
        private Color hitColour = new Color(1, 0, 0);
        private Color defaultColour = Color.white;

        // The duration of the hit flash effect
        private const float hitFlashDuration = 0.10f;

        private int renderModifier;

        // Manual frameset walk animation
        [BoxGroup("Walk frametime settings"), SerializeField] private float airTimeMs = 0.2f;

        [BoxGroup("Walk frametime settings"), SerializeField] private float groundTimeMsMin, groundTimeMsMax;

        [BoxGroup("Walk frametime settings"), SerializeField] private float runSpeedMinModifier = 65f, runSpeedMaxModifier = 150;

        [BoxGroup("Walk frametime settings"), SerializeField] private AnimationCurve groundTimeCurve = AnimationCurve.Linear(0, 1, 1, 0);

        private Coroutine walkCoroutine;

        private ViewDirection currentWalkDirectionEnum;
        private string currentWalkEquippedWeapon;
        private bool isBlocking => parentCharacter.IsBlocking();

        #region Animation Keys
        [HideInInspector]
        public string
            IDLEBOTTOMRIGHT, IDLEBOTTOMLEFT, IDLETOPRIGHT, IDLETOPLEFT,
            WALKBOTTOMRIGHT, WALKBOTTOMLEFT, WALKTOPRIGHT, WALKTOPLEFT,
            HITBOTTOMRIGHT, HITBOTTOMLEFT, HITTOPRIGHT, HITTOPLEFT,
            JUMPBOTTOMRIGHT, JUMPBOTTOMLEFT, JUMPTOPRIGHT, JUMPTOPLEFT,

            STUNBOTTOMRIGHT, STUNBOTTOMLEFT, STUNTOPRIGHT, STUNTOPLEFT,

            //Animation keys for an npc with a single permanent attack variation (eg. wolf attacks)
            BASEATTACKBOTTOMRIGHT, BASEATTACKBOTTOMLEFT, BASEATTACKTOPRIGHT, BASEATTACKTOPLEFT,

            SLASHATTACKBOTTOMRIGHT, SLASHATTACKBOTTOMLEFT, SLASHATTACKTOPRIGHT, SLASHATTACKTOPLEFT,
            SLASHCHARGEATTACKBOTTOMRIGHT, SLASHCHARGEATTACKBOTTOMLEFT, SLASHCHARGEATTACKTOPRIGHT, SLASHCHARGEATTACKTOPLEFT,


            SWINGATTACKRIGHT, SWINGATTACKLEFT, SWINGATTACKTOP, SWINGATTACKBOTTOM, SWINGATTACKTOPLEFT, SWINGATTACKBOTTOMLEFT, SWINGATTACKTOPRIGHT, SWINGATTACKBOTTOMRIGHT,
            SWINGWALKBOTTOMRIGHT, SWINGWALKBOTTOMLEFT, SWINGWALKTOPRIGHT, SWINGWALKTOPLEFT,
            SWINGIDLEBOTTOMRIGHT, SWINGIDLEBOTTOMLEFT, SWINGIDLETOPRIGHT, SWINGIDLETOPLEFT,

            THRUSTATTACKRIGHT, THRUSTATTACKLEFT, THRUSTATTACKTOP, THRUSTATTACKBOTTOM, THRUSTATTACKBOTTOMRIGHT, THRUSTATTACKBOTTOMLEFT, THRUSTATTACKTOPRIGHT, THRUSTATTACKTOPLEFT,
            THRUSTWALKBOTTOMRIGHT, THRUSTWALKBOTTOMLEFT, THRUSTWALKTOPRIGHT, THRUSTWALKTOPLEFT,
            THRUSTIDLEBOTTOMRIGHT, THRUSTIDLEBOTTOMLEFT, THRUSTIDLETOPRIGHT, THRUSTIDLETOPLEFT,

            RANGEDATTACKBOTTOMRIGHT, RANGEDATTACKBOTTOMLEFT, RANGEDATTACKTOPRIGHT, RANGEDATTACKTOPLEFT,

            RAISESHIELDBOTTOMRIGHT, RAISESHIELDBOTTOMLEFT, RAISESHIELDTOPRIGHT, RAISESHIELDTOPLEFT,
            BLOCKSHIELDBOTTOMRIGHT, BLOCKSHIELDBOTTOMLEFT, BLOCKSHIELDTOPRIGHT, BLOCKSHIELDTOPLEFT,

            DRAWBOWRIGHT, DRAWBOWLEFT, DRAWBOWTOP, DRAWBOWBOTTOM,
            RELEASEBOWRIGHT, RELEASEBOWLEFT, RELEASEBOWTOP, RELEASEBOWBOTTOM,

            TWOHANDEDATTACKBOTTOMRIGHT, TWOHANDEDATTACKBOTTOMLEFT, TWOHANDEDATTACKTOPRIGHT, TWOHANDEDATTACKTOPLEFT,
            TWOHANDEDATTACK2BOTTOMRIGHT, TWOHANDEDATTACK2BOTTOMLEFT, TWOHANDEDATTACK2TOPRIGHT, TWOHANDEDATTACK2TOPLEFT,

            TWOHANDEDIDLEBOTTOMRIGHT, TWOHANDEDIDLEBOTTOMLEFT, TWOHANDEDIDLETOPRIGHT, TWOHANDEDIDLETOPLEFT,
            TWOHANDEDWALKBOTTOMRIGHT, TWOHANDEDWALKBOTTOMLEFT, TWOHANDEDWALKTOPRIGHT, TWOHANDEDWALKTOPLEFT,

            TWOHANDEDRAISEBLOCKBOTTOMRIGHT, TWOHANDEDRAISEBLOCKBOTTOMLEFT, TWOHANDEDRAISEBLOCKTOPRIGHT, TWOHANDEDRAISEBLOCKTOPLEFT,
            TWOHANDEDBLOCKBOTTOMRIGHT, TWOHANDEDBLOCKBOTTOMLEFT, TWOHANDEDBLOCKTOPRIGHT, TWOHANDEDBLOCKTOPLEFT,

            BOWIDLEBOTTOMRIGHT, BOWIDLEBOTTOMLEFT, BOWIDLETOPRIGHT, BOWIDLETOPLEFT,
            BOWWALKBOTTOMRIGHT, BOWWALKBOTTOMLEFT, BOWWALKTOPRIGHT, BOWWALKTOPLEFT,

            SPELLCASTBOTTOMRIGHT, SPELLCASTBOTTOMLEFT, SPELLCASTTOPRIGHT, SPELLCASTTOPLEFT,
            SPELLHOLDBOTTOMRIGHT, SPELLHOLDBOTTOMLEFT, SPELLHOLDTOPRIGHT, SPELLHOLDTOPLEFT,
            SPELLTHROWBOTTOMRIGHT, SPELLTHROWBOTTOMLEFT, SPELLTHROWTOPRIGHT, SPELLTHROWTOPLEFT,

            SOULDIE,
            SPINDEATH, SPINDEATHSINK;

        #endregion

        #region Animation Dictionaries

        protected Dictionary<ViewDirection, string> IdleAnimationKeyPairs;
        protected Dictionary<ViewDirection, string> WalkAnimationKeyPairs;
        protected Dictionary<ViewDirection, string> HitAnimationKeyPairs;

        public Dictionary<AttackDirection, Vector2> AttackDirectionPairs;

        // Weapons
        protected Dictionary<ViewDirection, string> RaiseBlockAnimationKeyPairs;
        protected Dictionary<WeaponMode, Dictionary<ViewDirection, string>> WeaponTypeRaiseBlockDictionaryPairs;

        protected Dictionary<ViewDirection, string> BlockAnimationKeyPairs;
        protected Dictionary<WeaponMode, Dictionary<ViewDirection, string>> WeaponTypeBlockDictionaryPairs;

        Dictionary<AttackDirection, string> SwingAnimationKeys;
        protected Dictionary<ViewDirection, string> SwingIdleAnimationKeyPairs;
        protected Dictionary<ViewDirection, string> SwingWalkAnimationKeyPairs;

        Dictionary<AttackDirection, string> BowDrawAnimationKeys;
        protected Dictionary<AttackDirection, string> BowReleaseAnimationKeys;
        protected Dictionary<ViewDirection, string> BowIdleAnimationKeyPairs;
        protected Dictionary<ViewDirection, string> BowWalkAnimationKeyPairs;

        Dictionary<AttackDirection, string> SlashAttackAnimationKeys;
        protected Dictionary<ViewDirection, string> SlashIdleAnimationKeyPairs;
        protected Dictionary<ViewDirection, string> SlashWalkAnimationKeyPairs;
        protected Dictionary<ViewDirection, string> SlashChargeAttackAnimationKeyPairs;

        Dictionary<AttackDirection, string> TwoHandedAttackAnimationKeys1;
        Dictionary<AttackDirection, string> TwoHandedAttackAnimationKeys2;

        protected Dictionary<ViewDirection, string> TwoHandedIdleAnimationKeyPairs;
        protected Dictionary<ViewDirection, string> TwoHandedWalkAnimationKeyPairs;
        protected Dictionary<ViewDirection, string> TwoHandedRaiseBlockAnimatioinKeyPairs;
        protected Dictionary<ViewDirection, string> TwoHandedBlockAnimationKeyPairs;

        Dictionary<AttackDirection, string> ThrustAnimationKeys;
        protected Dictionary<ViewDirection, string> ThrustIdleAnimationKeyPairs;
        protected Dictionary<ViewDirection, string> ThrustWalkAnimationKeyPairs;

        // Dictionary to sort shield and weapon layers when attacking based on view direction to avoid overlapping or clipping
        protected Dictionary<WeaponMode, Dictionary<ViewDirection, int[]>> WeaponModeDictionaryPairs;

        protected Dictionary<ViewDirection, int[]> ThrustWeaponViewDirectionSorting;
        protected Dictionary<ViewDirection, int[]> RangedWeaponViewDirectionSorting;
        protected Dictionary<ViewDirection, int[]> TwoHandedWeaponViewDirectionSorting;
        protected Dictionary<ViewDirection, int[]> SlashWeaponViewDirectionSorting;
        protected Dictionary<ViewDirection, int[]> SwingWeaponViewDirectionSorting;
        protected Dictionary<ViewDirection, int[]> SpellWeaponViewDirectionSorting;

        protected Dictionary<ViewDirection, int[]> ShieldRunViewDirectionSorting;

        // Dictinary to sort shield when blockiong based on view direction
        protected Dictionary<ViewDirection, int> ViewDirectionShieldSorting;

        protected Dictionary<AttackDirection, string> CastSpellAnimationKeyPairs;
        protected Dictionary<ViewDirection, string> HoldSpellAnimationKeyPairs;
        protected Dictionary<ViewDirection, string> ThrowSpellAnimationKeyPairs;

        // Dictionary to convert view direction to sprite resolver categories
        protected Dictionary<ViewDirection, string> viewDirectionToCategory;

        protected List<Vector2> helmetAnimationPositions;
        protected Dictionary<ViewDirection, List<Vector2>> torsoAnimationPositions;

        #endregion

        #region Parent Character Getters

        protected WeaponItem EquippedWeapon => parentCharacter.equippedWeapon;
        protected ShieldItem EquippedShield => parentCharacter.equippedShield;
        protected bool WeaponSheathed => parentCharacter.IsWeaponSheathed();
        protected ViewDirection CurrentViewDirection => parentCharacter.characterViewDirection;
        protected WeaponMode CurrentWeaponMode => parentCharacter.WeaponMode;

        #endregion

        // Singleton References
        protected AudioManager audioManager;
        protected FMODEvents fmodEvents;

        private void Start()
        {
            audioManager = AudioManager.Instance;
            fmodEvents = FMODEvents.Instance;

            InitializeAnimations();
            DefineDictionaries();

            cosmeticTransforms = GetCosmeticTransforms();
        }

        protected virtual void DefineDictionaries()
        {
            #region Render Priority Dictionaries
            SwingWeaponViewDirectionSorting = new Dictionary<ViewDirection, int[]>
        {
            // Weapon, Shield (no shield for this weapon)
            { ViewDirection.BottomRight, new int[] { frontLayer, frontLayer + 1 } },
            { ViewDirection.BottomLeft, new int[] { frontLayer, frontLayer + 1 } },
            { ViewDirection.TopRight, new int[] { frontLayer, backLayer } },
            { ViewDirection.TopLeft, new int[] { backLayer, frontLayer + 1 } }
        };


            SlashWeaponViewDirectionSorting = new Dictionary<ViewDirection, int[]>
        {
            // Weapon, Shield (no shield for this weapon)
            { ViewDirection.BottomRight, new int[] { frontLayer, frontLayer } },
            { ViewDirection.BottomLeft, new int[] { backLayer, frontLayer } },
            { ViewDirection.TopRight, new int[] { frontLayer, backLayer } },
            { ViewDirection.TopLeft, new int[] { backLayer, frontLayer } }
        };


            TwoHandedWeaponViewDirectionSorting = new Dictionary<ViewDirection, int[]>
        {
            // Weapon, Shield (no shield for this weapon)
            { ViewDirection.BottomRight, new int[] { frontLayer, frontLayer } },
            { ViewDirection.BottomLeft, new int[] { frontLayer, backLayer } },
            { ViewDirection.TopRight, new int[] { backLayer, backLayer } },
            { ViewDirection.TopLeft, new int[] { backLayer, backLayer } }
        };

            ThrustWeaponViewDirectionSorting = new Dictionary<ViewDirection, int[]>
        {
            // Weapon, Shield
            { ViewDirection.BottomRight, new int[] { frontLayer + 1, frontLayer } },
            { ViewDirection.BottomLeft, new int[] { backLayer, frontLayer } },
            { ViewDirection.TopRight, new int[] { frontLayer, backLayer } },
            { ViewDirection.TopLeft, new int[] { backLayer, frontLayer } }
        };

            RangedWeaponViewDirectionSorting = new Dictionary<ViewDirection, int[]>
        {
            // Weapon, Shield (no shield for this weapon)
            { ViewDirection.BottomRight, new int[] { frontLayer, frontLayer } },
            { ViewDirection.BottomLeft, new int[] { frontLayer, backLayer } },
            { ViewDirection.TopRight, new int[] { frontLayer, backLayer } },
            { ViewDirection.TopLeft, new int[] { frontLayer, backLayer } }
        };

            SpellWeaponViewDirectionSorting = new Dictionary<ViewDirection, int[]>
            {
                { ViewDirection.BottomRight, new int[] { frontLayer, backLayer } },
                { ViewDirection.BottomLeft, new int[] { frontLayer, backLayer } },
                { ViewDirection.TopLeft, new int[] { frontLayer, backLayer } },
                { ViewDirection.TopRight, new int[] { frontLayer, backLayer }}
            };

            WeaponModeDictionaryPairs = new Dictionary<WeaponMode, Dictionary<ViewDirection, int[]>>
        {
            { WeaponMode.Thrust, ThrustWeaponViewDirectionSorting },
            { WeaponMode.TwoHanded, TwoHandedWeaponViewDirectionSorting },
            { WeaponMode.Ranged, RangedWeaponViewDirectionSorting },
            { WeaponMode.Slash, SlashWeaponViewDirectionSorting},
            { WeaponMode.Swing, SwingWeaponViewDirectionSorting},
            { WeaponMode.Spell, SpellWeaponViewDirectionSorting }

        };

            ViewDirectionShieldSorting = new Dictionary<ViewDirection, int>
        {
            { ViewDirection.BottomRight, frontLayer },
            { ViewDirection.BottomLeft, frontLayer },
            { ViewDirection.TopRight, backLayer },
            { ViewDirection.TopLeft, backLayer }
        };

            #endregion

            IdleAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, IDLEBOTTOMRIGHT },
            { ViewDirection.BottomLeft, IDLEBOTTOMLEFT},
            { ViewDirection.TopRight, IDLETOPRIGHT },
            { ViewDirection.TopLeft, IDLETOPLEFT }
        };

            WalkAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, WALKBOTTOMRIGHT },
            { ViewDirection.BottomLeft, WALKBOTTOMLEFT },
            { ViewDirection.TopRight, WALKTOPRIGHT },
            { ViewDirection.TopLeft, WALKTOPLEFT }
        };

            HitAnimationKeyPairs = new Dictionary<ViewDirection, string>
            {
            { ViewDirection.BottomRight, HITBOTTOMRIGHT },
            { ViewDirection.BottomLeft, HITBOTTOMLEFT },
            { ViewDirection.TopRight, HITTOPRIGHT },
            { ViewDirection.TopLeft, HITTOPLEFT }
            };

            RaiseBlockAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, RAISESHIELDBOTTOMRIGHT },
            { ViewDirection.BottomLeft, RAISESHIELDBOTTOMLEFT },
            { ViewDirection.TopRight, RAISESHIELDTOPRIGHT },
            { ViewDirection.TopLeft, RAISESHIELDTOPLEFT }
        };

            BlockAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, BLOCKSHIELDBOTTOMRIGHT },
            { ViewDirection.BottomLeft, BLOCKSHIELDBOTTOMLEFT },
            { ViewDirection.TopRight, BLOCKSHIELDTOPRIGHT },
            { ViewDirection.TopLeft, BLOCKSHIELDTOPLEFT }
        };

            AttackDirectionPairs = new Dictionary<AttackDirection, Vector2>
        {
            { AttackDirection.Right, Vector2.right },
            { AttackDirection.Left, Vector2.left },
            { AttackDirection.Up, Vector2.up },
            { AttackDirection.Down, Vector2.down },
            { AttackDirection.TopRight, new Vector2(0.1f, 0.1f) },
            { AttackDirection.BottomRight, new Vector2(0.1f, -0.1f) },
            { AttackDirection.BottomLeft, new Vector2(-0.1f, -0.1f) },
            { AttackDirection.TopLeft, new Vector2(-0.1f, 0.1f) }
        };

            #region Swing Animation Key

            SwingAnimationKeys = new Dictionary<AttackDirection, string>
        {
            { AttackDirection.Right, SWINGATTACKRIGHT },
            { AttackDirection.Left, SWINGATTACKLEFT },
            { AttackDirection.Up, SWINGATTACKTOP },
            { AttackDirection.Down, SWINGATTACKBOTTOM },
            { AttackDirection.TopRight, SWINGATTACKTOPRIGHT},
            { AttackDirection.TopLeft, SWINGATTACKTOPLEFT},
            { AttackDirection.BottomRight, SWINGATTACKBOTTOMRIGHT},
            { AttackDirection.BottomLeft, SWINGATTACKBOTTOMLEFT}
        };

            SwingIdleAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, SWINGIDLEBOTTOMRIGHT },
            { ViewDirection.BottomLeft, SWINGIDLEBOTTOMLEFT },
            { ViewDirection.TopRight, SWINGIDLETOPRIGHT },
            { ViewDirection.TopLeft, SWINGIDLETOPLEFT }
        };

            SwingWalkAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, SWINGWALKBOTTOMRIGHT },
            { ViewDirection.BottomLeft, SWINGWALKBOTTOMLEFT },
            { ViewDirection.TopRight, SWINGWALKTOPRIGHT },
            { ViewDirection.TopLeft, SWINGWALKTOPLEFT }
        };

            #endregion

            #region Thrust Animation Keys

            ThrustAnimationKeys = new Dictionary<AttackDirection, string>
        {
            { AttackDirection.Right, THRUSTATTACKRIGHT },
            { AttackDirection.Left, THRUSTATTACKLEFT },
            { AttackDirection.Up, THRUSTATTACKTOP },
            { AttackDirection.Down, THRUSTATTACKBOTTOM },
            { AttackDirection.BottomRight, THRUSTATTACKBOTTOMRIGHT },
            { AttackDirection.BottomLeft, THRUSTATTACKBOTTOMLEFT },
            { AttackDirection.TopRight, THRUSTATTACKTOPRIGHT },
            { AttackDirection.TopLeft, THRUSTATTACKTOPLEFT}

        };

            ThrustIdleAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, THRUSTIDLEBOTTOMRIGHT },
            { ViewDirection.BottomLeft, THRUSTIDLEBOTTOMLEFT },
            { ViewDirection.TopRight, THRUSTIDLETOPRIGHT },
            { ViewDirection.TopLeft, THRUSTIDLETOPLEFT }
        };

            ThrustWalkAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, THRUSTWALKBOTTOMRIGHT },
            { ViewDirection.BottomLeft, THRUSTWALKBOTTOMLEFT },
            { ViewDirection.TopRight, THRUSTWALKTOPRIGHT },
            { ViewDirection.TopLeft, THRUSTWALKTOPLEFT }
        };

            #endregion

            #region Bow Animation Keys
            BowDrawAnimationKeys = new Dictionary<AttackDirection, string>
        {
            { AttackDirection.Right, DRAWBOWRIGHT },
            { AttackDirection.Left, DRAWBOWLEFT },
            { AttackDirection.Up, DRAWBOWTOP },
            { AttackDirection.Down, DRAWBOWBOTTOM }
        };

            BowIdleAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, BOWIDLEBOTTOMRIGHT },
            { ViewDirection.BottomLeft, BOWIDLEBOTTOMLEFT },
            { ViewDirection.TopRight, BOWIDLETOPRIGHT },
            { ViewDirection.TopLeft, BOWIDLETOPLEFT }
        };

            BowWalkAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, BOWWALKBOTTOMRIGHT },
            { ViewDirection.BottomLeft, BOWWALKBOTTOMLEFT },
            { ViewDirection.TopRight, BOWWALKTOPRIGHT },
            { ViewDirection.TopLeft, BOWWALKTOPLEFT }
        };

            BowReleaseAnimationKeys = new Dictionary<AttackDirection, string>
            {
                { AttackDirection.Right, RELEASEBOWRIGHT },
                { AttackDirection.Left, RELEASEBOWLEFT },
                { AttackDirection.Up, RELEASEBOWTOP },
                { AttackDirection.Down, RELEASEBOWBOTTOM }
            };


            #endregion

            SlashAttackAnimationKeys = new Dictionary<AttackDirection, string>
        {
            { AttackDirection.BottomRight, SLASHATTACKBOTTOMRIGHT },
            { AttackDirection.BottomLeft, SLASHATTACKBOTTOMLEFT },
            { AttackDirection.TopRight, SLASHATTACKTOPRIGHT },
            { AttackDirection.TopLeft, SLASHATTACKTOPLEFT }
        };

            SlashChargeAttackAnimationKeyPairs = new Dictionary<ViewDirection, string>
            {
                { ViewDirection.BottomRight, SLASHCHARGEATTACKBOTTOMRIGHT },
                { ViewDirection.BottomLeft, SLASHCHARGEATTACKBOTTOMLEFT },
                { ViewDirection.TopRight, SLASHCHARGEATTACKTOPRIGHT },
                { ViewDirection.TopLeft, SLASHCHARGEATTACKTOPLEFT }
            };

            #region Two Handed Animation Keys

            TwoHandedAttackAnimationKeys1 = new Dictionary<AttackDirection, string>
        {
            { AttackDirection.BottomRight, TWOHANDEDATTACKBOTTOMRIGHT },
            { AttackDirection.BottomLeft, TWOHANDEDATTACKBOTTOMLEFT },
            { AttackDirection.TopRight, TWOHANDEDATTACKTOPRIGHT },
            { AttackDirection.TopLeft, TWOHANDEDATTACKTOPLEFT }
        };

            TwoHandedAttackAnimationKeys2 = new Dictionary<AttackDirection, string>
        {
            { AttackDirection.BottomRight, TWOHANDEDATTACK2BOTTOMRIGHT },
            { AttackDirection.BottomLeft, TWOHANDEDATTACK2BOTTOMLEFT },
            { AttackDirection.TopRight, TWOHANDEDATTACK2TOPRIGHT },
            { AttackDirection.TopLeft, TWOHANDEDATTACK2TOPLEFT }
        };

            TwoHandedIdleAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, TWOHANDEDIDLEBOTTOMRIGHT },
            { ViewDirection.BottomLeft, TWOHANDEDIDLEBOTTOMLEFT },
            { ViewDirection.TopRight, TWOHANDEDIDLETOPRIGHT },
            { ViewDirection.TopLeft, TWOHANDEDIDLETOPLEFT }
        };

            TwoHandedWalkAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, TWOHANDEDWALKBOTTOMRIGHT },
            { ViewDirection.BottomLeft, TWOHANDEDWALKBOTTOMLEFT },
            { ViewDirection.TopRight, TWOHANDEDWALKTOPRIGHT },
            { ViewDirection.TopLeft, TWOHANDEDWALKTOPLEFT }
        };

            TwoHandedRaiseBlockAnimatioinKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, TWOHANDEDRAISEBLOCKBOTTOMRIGHT },
            { ViewDirection.BottomLeft, TWOHANDEDRAISEBLOCKBOTTOMLEFT },
            { ViewDirection.TopRight, TWOHANDEDRAISEBLOCKTOPRIGHT },
            { ViewDirection.TopLeft, TWOHANDEDRAISEBLOCKTOPLEFT }
        };

            TwoHandedBlockAnimationKeyPairs = new Dictionary<ViewDirection, string>
        {
            { ViewDirection.BottomRight, TWOHANDEDBLOCKBOTTOMRIGHT },
            { ViewDirection.BottomLeft, TWOHANDEDBLOCKBOTTOMLEFT },
            { ViewDirection.TopRight, TWOHANDEDBLOCKTOPRIGHT },
            { ViewDirection.TopLeft, TWOHANDEDBLOCKTOPLEFT }
        };

            #endregion

            #region Weapon Type Dictionary

            WeaponTypeRaiseBlockDictionaryPairs = new Dictionary<WeaponMode, Dictionary<ViewDirection, string>>
        {
            { WeaponMode.TwoHanded, TwoHandedRaiseBlockAnimatioinKeyPairs }
        };

            WeaponTypeBlockDictionaryPairs = new Dictionary<WeaponMode, Dictionary<ViewDirection, string>>
        {
            { WeaponMode.TwoHanded, TwoHandedBlockAnimationKeyPairs }
        };

            #endregion

            #region Spell Dictionaries

            CastSpellAnimationKeyPairs = new Dictionary<AttackDirection, string>
            {
                { AttackDirection.BottomRight, SPELLCASTBOTTOMRIGHT },
                { AttackDirection.BottomLeft, SPELLCASTBOTTOMLEFT },
                { AttackDirection.TopRight, SPELLCASTTOPRIGHT },
                { AttackDirection.TopLeft, SPELLCASTTOPLEFT }
            };

            HoldSpellAnimationKeyPairs = new Dictionary<ViewDirection, string>
            {
                { ViewDirection.BottomRight, SPELLHOLDBOTTOMRIGHT },
                { ViewDirection.BottomLeft, SPELLHOLDBOTTOMLEFT },
                { ViewDirection.TopRight, SPELLHOLDTOPRIGHT },
                { ViewDirection.TopLeft, SPELLHOLDTOPLEFT }
            };

            ThrowSpellAnimationKeyPairs = new Dictionary<ViewDirection, string>
            {
                { ViewDirection.BottomRight, SPELLTHROWBOTTOMRIGHT },
                { ViewDirection.BottomLeft, SPELLTHROWBOTTOMLEFT },
                { ViewDirection.TopRight, SPELLTHROWTOPRIGHT },
                { ViewDirection.TopLeft, SPELLTHROWTOPLEFT }
            };

            #endregion

            viewDirectionToCategory = new()       
            {
                { ViewDirection.BottomRight, "BR" },
                { ViewDirection.BottomLeft, "BL" },
                { ViewDirection.TopRight, "TR" },
                { ViewDirection.TopLeft, "TL" }
            };

            torsoAnimationPositions = new()
            {
                {ViewDirection.BottomRight, new List<Vector2>() { new Vector2(0, 0), new Vector2(0, -0.0625f), new Vector2(0, 0), new Vector2(0, -0.0625f) } },
                {ViewDirection.BottomLeft, new List<Vector2>() { new Vector2(0.0625f, 0), new Vector2(0.0625f, -0.0625f), new Vector2(0.0625f, 0), new Vector2(0.0625f, -0.0625f) } },
                {ViewDirection.TopLeft, new List<Vector2>() { new Vector2(0.0625f, 0), new Vector2(0.0625f, -0.0625f), new Vector2(0.0625f, 0), new Vector2(0.0625f, -0.0625f) } },
                {ViewDirection.TopRight, new List<Vector2>() { new Vector2(0, 0), new Vector2(0, -0.0625f), new Vector2(0, 0), new Vector2(0, -0.0625f) } }
            };

            helmetAnimationPositions = new()
            {
               new Vector2(0, 0.0625f), new Vector2(0, 0), new Vector2(0, 0.0625f), new Vector2(0, 0)
            };
        }
        protected virtual void InitializeAnimations()
        {
            WALKBOTTOMRIGHT = "WalkBR";
            WALKBOTTOMLEFT = "WalkBL";
            WALKTOPRIGHT = "WalkTR";
            WALKTOPLEFT = "WalkTL";

            IDLEBOTTOMRIGHT = "IdleBR";
            IDLEBOTTOMLEFT = "IdleBL";
            IDLETOPRIGHT = "IdleTR";
            IDLETOPLEFT = "IdleTL";

            HITBOTTOMRIGHT = "HitBR";
            HITBOTTOMLEFT = "HitBL";
            HITTOPRIGHT = "HitTR";
            HITTOPLEFT = "HitTL";

            #region Two Handed Animation Keys
            TWOHANDEDATTACKTOPRIGHT = "TwoHandedAttackTR";
            TWOHANDEDATTACKTOPLEFT = "TwoHandedAttackTL";
            TWOHANDEDATTACKBOTTOMRIGHT = "TwoHandedAttackBR";
            TWOHANDEDATTACKBOTTOMLEFT = "TwoHandedAttackBL";

            TWOHANDEDATTACK2TOPRIGHT = "TwoHandedAttack2TR";
            TWOHANDEDATTACK2TOPLEFT = "TwoHandedAttack2TL";
            TWOHANDEDATTACK2BOTTOMRIGHT = "TwoHandedAttack2BR";
            TWOHANDEDATTACK2BOTTOMLEFT = "TwoHandedAttack2BL";

            TWOHANDEDIDLEBOTTOMRIGHT = "TwoHandedIdleBR";
            TWOHANDEDIDLEBOTTOMLEFT = "TwoHandedIdleBL";
            TWOHANDEDIDLETOPRIGHT = "TwoHandedIdleTR";
            TWOHANDEDIDLETOPLEFT = "TwoHandedIdleTL";

            TWOHANDEDWALKBOTTOMRIGHT = "TwoHandedWalkBR";
            TWOHANDEDWALKBOTTOMLEFT = "TwoHandedWalkBL";
            TWOHANDEDWALKTOPRIGHT = "TwoHandedWalkTR";
            TWOHANDEDWALKTOPLEFT = "TwoHandedWalkTL";

            TWOHANDEDRAISEBLOCKBOTTOMRIGHT = "TwoHandedRaiseBlockBR";
            TWOHANDEDRAISEBLOCKBOTTOMLEFT = "TwoHandedRaiseBlockBL";
            TWOHANDEDRAISEBLOCKTOPRIGHT = "TwoHandedRaiseBlockTR";
            TWOHANDEDRAISEBLOCKTOPLEFT = "TwoHandedRaiseBlockTL";

            TWOHANDEDBLOCKBOTTOMRIGHT = "TwoHandedBlockBR";
            TWOHANDEDBLOCKBOTTOMLEFT = "TwoHandedBlockBL";
            TWOHANDEDBLOCKTOPRIGHT = "TwoHandedBlockTR";
            TWOHANDEDBLOCKTOPLEFT = "TwoHandedBlockTL";
            #endregion

            #region Bow Animation Keys

            BOWIDLEBOTTOMRIGHT = "BowIdleBR";
            BOWIDLEBOTTOMLEFT = "BowIdleBL";
            BOWIDLETOPRIGHT = "BowIdleTR";
            BOWIDLETOPLEFT = "BowIdleTL";

            BOWWALKBOTTOMRIGHT = "BowWalkBR";
            BOWWALKBOTTOMLEFT = "BowWalkBL";
            BOWWALKTOPRIGHT = "BowWalkTR";
            BOWWALKTOPLEFT = "BowWalkTL";

            DRAWBOWRIGHT = "DrawBowR";
            DRAWBOWLEFT = "DrawBowL";
            DRAWBOWTOP = "DrawBowT";
            DRAWBOWBOTTOM = "DrawBowB";

            RELEASEBOWRIGHT = "ReleaseBowR";
            RELEASEBOWLEFT = "ReleaseBowL";
            RELEASEBOWTOP = "ReleaseBowT";
            RELEASEBOWBOTTOM = "ReleaseBowB";

            #endregion

            SLASHATTACKTOPRIGHT = "SlashAttackTR";
            SLASHATTACKTOPLEFT = "SlashAttackTL";
            SLASHATTACKBOTTOMRIGHT = "SlashAttackBR";
            SLASHATTACKBOTTOMLEFT = "SlashAttackBL";

            SLASHCHARGEATTACKTOPRIGHT = "SlashChargeTR";
            SLASHCHARGEATTACKBOTTOMLEFT = "SlashChargeBL";
            SLASHCHARGEATTACKBOTTOMRIGHT = "SlashChargeBR";
            SLASHCHARGEATTACKTOPLEFT = "SlashChargeTL";


            #region Swing Animation Keys

            SWINGATTACKRIGHT = "SwingAttackR";
            SWINGATTACKLEFT = "SwingAttackL";
            SWINGATTACKBOTTOM = "SwingAttackB";
            SWINGATTACKTOP = "SwingAttackT";
            SWINGATTACKTOPRIGHT = "SwingAttackTR";
            SWINGATTACKTOPLEFT = "SwingAttackTL";
            SWINGATTACKBOTTOMRIGHT = "SwingAttackBR";
            SWINGATTACKBOTTOMLEFT = "SwingAttackBL";

            SWINGIDLEBOTTOMRIGHT = "SwingIdleBR";
            SWINGIDLEBOTTOMLEFT = "SwingIdleBL";
            SWINGIDLETOPRIGHT = "SwingIdleTR";
            SWINGIDLETOPLEFT = "SwingIdleTL";

            SWINGWALKBOTTOMRIGHT = "SwingWalkBR";
            SWINGWALKBOTTOMLEFT = "SwingWalkBL";
            SWINGWALKTOPRIGHT = "SwingWalkTR";
            SWINGWALKTOPLEFT = "SwingWalkTL";

            #endregion

            #region Thrust Animation Keys

            THRUSTATTACKRIGHT = "ThrustAttackR";
            THRUSTATTACKLEFT = "ThrustAttackL";
            THRUSTATTACKBOTTOM = "ThrustAttackB";
            THRUSTATTACKTOP = "ThrustAttackT";
            THRUSTATTACKBOTTOMRIGHT = "ThrustAttackBR";
            THRUSTATTACKBOTTOMLEFT = "ThrustAttackBL";
            THRUSTATTACKTOPRIGHT = "ThrustAttackTR";
            THRUSTATTACKTOPLEFT = "ThrustAttackTL";

            THRUSTIDLEBOTTOMRIGHT = "ThrustIdleBR";
            THRUSTIDLEBOTTOMLEFT = "ThrustIdleBL";
            THRUSTIDLETOPRIGHT = "ThrustIdleTR";
            THRUSTIDLETOPLEFT = "ThrustIdleTL";

            THRUSTWALKBOTTOMRIGHT = "ThrustWalkBR";
            THRUSTWALKBOTTOMLEFT = "ThrustWalkBL";
            THRUSTWALKTOPRIGHT = "ThrustWalkTR";
            THRUSTWALKTOPLEFT = "ThrustWalkTL";

            #endregion

            BLOCKSHIELDBOTTOMLEFT = "BlockShieldBL";
            BLOCKSHIELDBOTTOMRIGHT = "BlockShieldBR";
            BLOCKSHIELDTOPLEFT = "BlockShieldTL";
            BLOCKSHIELDTOPRIGHT = "BlockShieldTR";

            RAISESHIELDBOTTOMLEFT = "RaiseShieldBL";
            RAISESHIELDBOTTOMRIGHT = "RaiseShieldBR";
            RAISESHIELDTOPLEFT = "RaiseShieldTL";
            RAISESHIELDTOPRIGHT = "RaiseShieldTR";

            SPELLCASTBOTTOMRIGHT = "CastSpellBR";
            SPELLCASTBOTTOMLEFT = "CastSpellBL";
            SPELLCASTTOPRIGHT = "CastSpellTR";
            SPELLCASTTOPLEFT = "CastSpellTL";

            SPELLHOLDBOTTOMRIGHT = "HoldSpellBR";
            SPELLHOLDBOTTOMLEFT = "HoldSpellBL";
            SPELLHOLDTOPRIGHT = "HoldSpellTR";
            SPELLHOLDTOPLEFT = "HoldSpellTL";

            SPELLTHROWBOTTOMRIGHT = "ThrowSpellBR";
            SPELLTHROWBOTTOMLEFT = "ThrowSpellBL";
            SPELLTHROWTOPRIGHT = "ThrowSpellTR";
            SPELLTHROWTOPLEFT = "ThrowSpellTL";

            SPINDEATH = "SpinDeath";
            SPINDEATHSINK = "SpinDeathSin";
        }

        #region Animation Methods

        public virtual void ChangeAnimationState(string newState)
        {
            // Stops the same animation from interrupting itself
            if (currentAnimationState == newState) return;

            SetCosmeticAnimationState(newState);

            currentAnimationState = newState;
        }

        public void EnableCosmeticsAnimators()
        {
            foreach (var cosmetic in cosmeticTransforms)
            {
                if (cosmetic == null)
                    continue;

                if (cosmetic.gameObject.activeInHierarchy)
                {
                    if (!cosmetic.TryGetComponent(out Animator animator))
                    {
                        Debug.LogWarning("No animator found on the cosmetic transform.");
                        continue;
                    }

                    animator.enabled = true;
                }
            }

            if (walkCoroutine != null)
            {
                StopCoroutine(walkCoroutine);
                walkCoroutine = null;
            }
        }

        public void DisableCosmeticAnimators()
        {
            foreach (var cosmetic in cosmeticTransforms)
            {
                if (cosmetic == null)
                    continue;

                if (cosmetic.gameObject.activeInHierarchy)
                {
                    if (!cosmetic.TryGetComponent(out Animator animator))
                    {
                        Debug.LogWarning("No animator found on the cosmetic transform.");
                        continue;
                    }

                    animator.enabled = false;
                }
            }
        }

        protected void SetCosmeticAnimationState(string newState)
        {
            if (cosmeticsTransform == null || !cosmeticsTransform.gameObject.activeInHierarchy)
                return;

            foreach (var cosmetic in cosmeticTransforms)
            {
                if (cosmetic == null)
                    continue;

                if (cosmetic.gameObject.activeInHierarchy)
                {
                    if (!cosmetic.TryGetComponent(out Animator animator))
                    {
                        Debug.LogWarning("No animator found on the cosmetic transform.");
                        continue;
                    }

                    animator.Play(newState);
                }
            }
        }

        public void SetCosmeticAnimationSpeed(float setSpeed)
        {
            if (cosmeticsTransform == null || !cosmeticsTransform.gameObject.activeInHierarchy)
                return;

            foreach (var cosmetic in cosmeticTransforms)
            {
                if (cosmetic.gameObject.activeInHierarchy)
                {
                    cosmetic.GetComponent<Animator>().speed = setSpeed;
                }
            }
        }

        private List<Transform> GetCosmeticTransforms()
        {
            List<Transform> returnList = new();

            for (int i = 0; i < cosmeticsTransform.childCount; i++)
            {
                Transform cosmeticTransform = cosmeticsTransform.GetChild(i);

                returnList.Add(cosmeticTransform);
            }

            return returnList;
        }

        public void DoDeathAnimation()
        {
            ChangeAnimationState(SPINDEATH);
        }

        #endregion

        #region Default Animation Methods

        public void DoIdleAnimation(ViewDirection viewDirection)
        {
            ChangeAnimationState(GetIdleAnimationForDirection(viewDirection));
        }

        protected string GetIdleAnimationForDirection(ViewDirection direction)
        {
            switch (direction)
            {
                case ViewDirection.TopRight: return IDLETOPRIGHT;
                case ViewDirection.TopLeft: return IDLETOPLEFT;
                case ViewDirection.BottomRight: return IDLEBOTTOMRIGHT;
                case ViewDirection.BottomLeft: return IDLEBOTTOMLEFT;
                default: return IDLEBOTTOMLEFT; // Default to bottom left if no match
            }
        }
        
        /// <summary>
        /// Break out of any ongoing walk loop coroutines
        /// </summary>
        public void StopWalkLoop()
        {
            if (walkCoroutine != null)
            {
                StopCoroutine(walkCoroutine);
                walkCoroutine = null;
            }

            // Re-enable all cosmetics animators
            EnableCosmeticsAnimators();
        }

        /// <summary>
        /// Disable all cosmetics animators and start a walk couroutine which manually sets frames
        /// </summary>
        /// <param name="viewDirection"></param>
        public void DoWalkAnimation(ViewDirection viewDirection, bool weaponEquipped = false, WeaponMode weaponMode = WeaponMode.Slash)
        {
            // Disables all animators amongst all cosmetics so that we can manually set the walking frames
            DisableCosmeticAnimators();

            // Ensure the current walk direction is always updated, so if the coroutine is running, the direction is correct
            currentWalkDirectionEnum = viewDirection;

            // A weapon might be drawn/sheathed while running, so we need to be able to dynamically modify the equipped weapon string to modify the running coroutine
            if (weaponEquipped == true)
            {
                currentWalkEquippedWeapon = GetWeaponAnimationName(weaponMode);
            }
            else
            {
                currentWalkEquippedWeapon = "";
            }

            if (walkCoroutine == null)
            {
                walkCoroutine = StartCoroutine(StartWalkLoop());
            }
        }

        private IEnumerator StartWalkLoop()
        {
            List<SpriteResolver> spriteResolvers = new();
            SpriteResolver helmetSpriteResolver = null, torsoSpriteResolver = null;

            // Add main body resolver
            if (TryGetComponent(out SpriteResolver bodyResolver))
                spriteResolvers.Add(bodyResolver);

            // Disable main Animator if needed
            if (TryGetComponent(out Animator mainAnimator))
                mainAnimator.enabled = false;

            // Add and disable cosmetics
            foreach (Transform transform in cosmeticsTransform)
            {
                // Disable the animator for everything
                if (transform.TryGetComponent(out Animator cosmeticAnimator))
                    cosmeticAnimator.enabled = false;                               

                if (transform.TryGetComponent(out SpriteResolver resolver))
                {
                    if (transform.name == "Helmet")
                    {
                        helmetSpriteResolver = resolver;
                    }
                    else if (transform.name == "Torso")
                    {
                        torsoSpriteResolver = resolver;
                    }
                    else
                    {
                        spriteResolvers.Add(resolver);
                    }
                }
            }

            // Ensure changes are processed before starting loop
            yield return null;

            // The current frame
            int currentFrame = 0;

            /* 
             * Helmet and Torsos are different that they only have 4 frames collectively, where the animator positions them based on the frame,
             * This means we need to manually set the frame for each sprite resolver as well as the position of the item
            */

            while (true)
            {
                // Get the view direction string 
                string viewDirectionString = viewDirectionToCategory[currentWalkDirectionEnum];

                string walkDirection = "";

                // The walking direction for all animated cosmetics

                walkDirection = currentWalkEquippedWeapon + "Walk" + viewDirectionString;           

                foreach (var resolver in spriteResolvers)
                {
                    resolver.SetCategoryAndLabel(walkDirection, $"{walkDirection}{currentFrame + 1}");
                }

                // Set the frame for the helmet and torso

                // Set the helmet's direction based on the view direction
                helmetSpriteResolver.SetCategoryAndLabel("AllDirections", viewDirectionString);

                // By default, set the torso's direction to "Front"
                string torsoDirection = "Front";

                // If the character is running in a northern direction, set the torso's direction to "Back"
                if (viewDirectionString == "TL" || viewDirectionString == "TR")
                {
                    torsoDirection = "Back";
                }

                torsoSpriteResolver.SetCategoryAndLabel("BaseFrames", torsoDirection);

                if (helmetSpriteResolver != null)
                {
                    helmetSpriteResolver.transform.localPosition = helmetAnimationPositions[currentFrame];
                }

                if (torsoSpriteResolver != null)
                {
                    torsoSpriteResolver.transform.localPosition = torsoAnimationPositions[currentWalkDirectionEnum][currentFrame];
                }

                if (currentFrame % 2 == 0)
                {
                    yield return new WaitForSeconds(airTimeMs); // You can vary this duration for different frames
                }
                else
                {
                    yield return new WaitForSeconds(GetCurrentGroundTime()); // You can vary this duration for different frames

                }

                currentFrame = (currentFrame + 1) % 4;
            }
        }

        /// <summary>
        /// Depending on the movement speed of the character, return an appropriate ground time 
        /// </summary>
        /// <returns></returns>
        private float GetCurrentGroundTime()
        {
            float currentSpeed = Mathf.Clamp(parentCharacter.MovementSpeed, runSpeedMinModifier, runSpeedMaxModifier);

            float t = Mathf.InverseLerp(runSpeedMinModifier, runSpeedMaxModifier, currentSpeed);

            float curveValue = groundTimeCurve.Evaluate(t); // Range should be 0–1

            float groundTime = Mathf.Lerp(groundTimeMsMax, groundTimeMsMin, curveValue);

           // Debug.Log($"[Curve] Ground Time: {groundTime}, Speed: {currentSpeed}, t: {t}, Curve: {curveValue}");

            return groundTime;
        }


        #endregion

        #region Weapon Animation Methods

        public void DoWeaponIdleAnimation(ViewDirection viewDirection)
        {
            ChangeAnimationState(GetWeaponIdleAnimationForDirection(CurrentViewDirection));
        }

        public void DoWeaponAttackAnimation(AttackDirection attackDirection)
        {
            ChangeAnimationState(GetWeaponAnimationKey(attackDirection));
        }

        public void DoWeaponChargeAttackAnimation(ViewDirection viewDirection)
        {
            UpdateWeaponSortingLayer();
            ChangeAnimationState(GetWeaponChargeAnimationForDirection(viewDirection));
        }

        /// <summary>
        /// Based on the given weapon mode, return the animation string corresponding to the weapon mode
        /// </summary>
        /// <param name="weaponMode"></param>
        /// <returns></returns>
        protected string GetWeaponAnimationName(WeaponMode weaponMode)
        {
            switch (weaponMode)
            {
                case WeaponMode.Thrust:
                case WeaponMode.Slash:
                    return "Thrust";

                case WeaponMode.TwoHanded:
                    return "TwoHanded";

                case WeaponMode.Swing:
                    return "Swing";

                case WeaponMode.Ranged:
                    return "Bow";
            }

            Debug.LogWarning("No weapon animation name found for this weapon mode!");
            return "";
        }

        /// <summary>
        /// Returns a weapon animation based on the characters view direction. Requires the weapon to be have a diagonal attack
        /// </summary>
        /// <param name="CurrentViewDirection"></param>
        /// <returns></returns>
        protected string GetWeaponAnimationKey(AttackDirection attackDirection)
        {
            switch (CurrentWeaponMode)
            {
                case WeaponMode.Slash:
                    return SlashAttackAnimationKeys[attackDirection];

                case WeaponMode.TwoHanded:

                    if (AttackIteration == 0)
                    {
                        return TwoHandedAttackAnimationKeys1[attackDirection];
                    }

                    if (AttackIteration == 1)
                    {
                        return TwoHandedAttackAnimationKeys2[attackDirection];
                    }

                    else
                    {
                        Debug.LogWarning("Attack iteration for two handed weapon is out of range!");
                    }

                    break;

                case WeaponMode.Swing:
                    return SwingAnimationKeys[attackDirection];

                case WeaponMode.Thrust:
                    return ThrustAnimationKeys[attackDirection];

                case WeaponMode.Ranged:
                    return BowDrawAnimationKeys[attackDirection];

                case WeaponMode.Spell:
                    Debug.Log("Spell cast animation");

                    return CastSpellAnimationKeyPairs[attackDirection];

                // In case of a creature with no weapon and a default animation

                default:
                    Debug.LogWarning("No weapon animation found for this weapon mode!");
                    break;
            }

            return "";
        }

        /// <summary>
        /// Returns the animation based on the equipped weapon (which is assumed as not null)
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected string GetWeaponWalkAnimationForDirection(ViewDirection direction)
        {
            switch (EquippedWeapon.weaponMode)
            {
                case WeaponMode.TwoHanded:
                    return TwoHandedWalkAnimationKeyPairs[direction];

                case WeaponMode.Ranged:
                    return BowWalkAnimationKeyPairs[direction];

                // Thrust and Slash weapons share the same walk animation
                case WeaponMode.Thrust:
                case WeaponMode.Slash:
                    return ThrustWalkAnimationKeyPairs[direction];

                case WeaponMode.Swing:
                    return SwingWalkAnimationKeyPairs[direction];

                default:
                    Debug.LogWarning("No walk animation for this weapon");
                    return WalkAnimationKeyPairs[direction];
            }
        }

        /// <summary>
        /// Returns the idle animation corresponding to an equipped weapon (if any)
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected string GetWeaponIdleAnimationForDirection(ViewDirection direction)
        {
            switch (EquippedWeapon.weaponMode)
            {
                case WeaponMode.TwoHanded:
                    return TwoHandedIdleAnimationKeyPairs[direction];

                case WeaponMode.Ranged:
                    return BowIdleAnimationKeyPairs[direction];

                // Thrust and Slash weapons share the same attack animation
                case WeaponMode.Thrust:
                case WeaponMode.Slash:
                    return ThrustIdleAnimationKeyPairs[direction];

                case WeaponMode.Swing:
                    return SwingIdleAnimationKeyPairs[direction];

                default:
                    Debug.LogWarning("No idle animation for this weapon");
                    return IdleAnimationKeyPairs[direction];
            }
        }

        protected string GetWeaponChargeAnimationForDirection(ViewDirection direction)
        {
            switch (EquippedWeapon.weaponMode)
            {           
                case WeaponMode.Slash:
                    return SlashChargeAttackAnimationKeyPairs[direction];
                default:
                    Debug.LogError("No charge animation for this weapon");
                    return IdleAnimationKeyPairs[direction];
            }
        }

        public void DoRaiseBlockAnimation(ViewDirection CurrentViewDirection)
        {
            ChangeAnimationState(GetRaiseBlockAnimation(CurrentViewDirection));
        }

        protected string GetRaiseBlockAnimation(ViewDirection CurrentViewDirection)
        {
            if (parentCharacter.HasShield())
            {
                //Debug.Log("Has shield");
                return RaiseBlockAnimationKeyPairs[CurrentViewDirection];
            }

            else if (parentCharacter.HasBlockableWeapon())
            {
                return WeaponTypeRaiseBlockDictionaryPairs[EquippedWeapon.weaponMode][CurrentViewDirection];
            }

            Debug.LogWarning("No animation found for the given view direction!");
            return "";

        }

        public void DoBlockAnimation(ViewDirection CurrentViewDirection)
        {
            ChangeAnimationState(GetBlockAnimation(CurrentViewDirection));
        }

        protected string GetBlockAnimation(ViewDirection CurrentViewDirection)
        {
            if (parentCharacter.HasShield())
            {
                Debug.Log("Has shield");
                return BlockAnimationKeyPairs[CurrentViewDirection];
            }

            else if (parentCharacter.HasBlockableWeapon())
            {
                return WeaponTypeBlockDictionaryPairs[EquippedWeapon.weaponMode][CurrentViewDirection];
            }

            Debug.LogWarning("No animation found for the given view direction!");
            return "";
        }

        #endregion

        #region Spell Animation Methods

        public void DoSpellHoldAnimation(ViewDirection viewDirection)
        {
            ChangeAnimationState(HoldSpellAnimationKeyPairs[viewDirection]);
        }

        public void DoSpellThrowAnimation(ViewDirection viewDirection)
        {
            ChangeAnimationState(ThrowSpellAnimationKeyPairs[viewDirection]);
        }

        #endregion

        #region Bow Animation Methods

        public void DoBowReleaseAnimation(float angle)
        {
            if (angle >= 45 && angle < 135)
            {
                SetCosmeticAnimationState(RELEASEBOWTOP);
            }
            else if (angle >= 135 || angle < -135)
            {
                SetCosmeticAnimationState(RELEASEBOWLEFT);
            }
            else if (angle >= -135 && angle < -45)
            {
                SetCosmeticAnimationState(RELEASEBOWBOTTOM);
            }
            else if (angle >= -45 && angle < 45)
            {
                SetCosmeticAnimationState(RELEASEBOWRIGHT);
            }
        }

        #endregion

        #region Sorting Layer Methods

        /// <summary>
        /// Based on the current view direction, sort the weapon and shield sprites to the correct layer
        /// </summary>
        public void UpdateWeaponSortingLayer()
        {
            // Check if there is a weapon equipped
            if (EquippedWeapon == null || WeaponSheathed)
            {
                return;
            }

            if (WeaponModeDictionaryPairs == null)
            {
                Debug.LogWarning("Cannot update weapon sorting layer, WeaponModeDictionaryPairs is null!");
                return;
            }

            WeaponMode weaponMode = EquippedWeapon.weaponMode;

            int weaponLayer = WeaponModeDictionaryPairs[weaponMode][CurrentViewDirection][0];
            int shieldLayer = WeaponModeDictionaryPairs[weaponMode][CurrentViewDirection][1];

            weaponSR.sortingOrder = weaponLayer;
            weaponTrailSR.sortingOrder = weaponLayer;

            shieldSR.sortingOrder = shieldLayer;
            shieldTrailSR.sortingOrder = shieldLayer;
        }

        /// <summary>
        /// By default, blocking towards the Bottom Right will have shield 
        /// </summary>
        public void UpdateBlockWeaponSortingLayer()
        {
            if (ViewDirectionShieldSorting == null)
            {
                Debug.LogWarning("Cannot update weapon sorting layer, ViewDirectionWeaponSorting is null!");
                return;
            }

            int shieldLayer = ViewDirectionShieldSorting[CurrentViewDirection];

            // If using a weapon that can block, change the weapon's sorting layer to the shield's sorting layer
            if (EquippedWeapon != null && EquippedWeapon.canBlock)
            {
                weaponSR.sortingOrder = shieldLayer;
                weaponTrailSR.sortingOrder = shieldLayer;
            }

            shieldSR.sortingOrder = shieldLayer;
            shieldTrailSR.sortingOrder = shieldLayer;
        }

        #endregion

        #region Render Layer Methods

        public void ModifyRenderLayer(int modification)
        {
            ResetRenderLayerModification();

            renderModifier = modification;

            foreach (var item in cosmeticTransforms)
            {
                item.GetComponent<SpriteRenderer>().rendererPriority += renderModifier;
            }
        }

        public void ResetRenderLayerModification()
        {
            foreach (var item in cosmeticTransforms)
            {
                item.GetComponent<SpriteRenderer>().rendererPriority -= renderModifier;
            }

            renderModifier = 0;
        }

        #endregion

        #region Character Hit Methods

        public void DoStaggerAnimation(ViewDirection currentViewDirection)
        {
            ChangeAnimationState(HitAnimationKeyPairs[currentViewDirection]);
        }

        public void DoFlashHit()
        {
            if (flashHitCR == null)
            {
                flashHitCR = FlashHitCR();
                StartCoroutine(flashHitCR);
            }
        }

        protected IEnumerator FlashHitCR()
        {
            List<Color> cosmeticColours = new List<Color>();

            if (cosmeticsTransform != null)
                foreach (var cosmetic in cosmeticTransforms)
                {
                    if (cosmetic != weaponTrailTransform)
                    {
                        cosmeticColours.Add(cosmetic.GetComponent<SpriteRenderer>().color);
                        cosmetic.GetComponent<SpriteRenderer>().color = hitColour;
                    }
                }

            yield return new WaitForSeconds(hitFlashDuration);

            if (cosmeticsTransform != null)
            {
                int cosmeticIndex = 0;
                foreach (var cosmetic in cosmeticTransforms)
                {
                    if (cosmetic != weaponTrailTransform)
                    {
                        cosmetic.GetComponent<SpriteRenderer>().color = cosmeticColours[cosmeticIndex];
                        cosmeticIndex++;
                    }
                }
            }
            //for (int i = 0; i < cosmeticTransforms.Length; i++)
            //{

            //    cosmeticTransforms[i].GetComponent<SpriteRenderer>().color = cosmeticColours[i];
            //}

            flashHitCR = null;

            yield break;
        }

        #endregion

        public void FlashHideWeaponTrail(float time)
        {
            if (weaponTrailTransform != null)
                StartCoroutine(FlashHideWeaponTrailCR(time));
        }

        // Temporary
        protected IEnumerator FlashHideWeaponTrailCR(float time)
        {
            weaponTrailTransform.gameObject.SetActive(false);
            yield return new WaitForSeconds(time);
            weaponTrailTransform.gameObject.SetActive(true);
            yield break;
        }
    }
}