using Characters;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIWindows
{
    public class NPCGUI : ValueBar 
    {
        [SerializeField] private GameObject healthBarParent;
        [SerializeField] private RectTransform backgroundRectTransform;
        [SerializeField] TextMeshProUGUI npcNameTextComponent;

        [BoxGroup("State UI"), SerializeField] private Image stateUIImage;

        [BoxGroup("State UI Textures"), SerializeField] private Sprite movingSprite, attackingSprite, defensiveSprite, targetSeenSprite, targetBlockingSprite;

        private NPCState currentState;

        protected override void Start()
        {
            base.Start();

            InitializeValueBar(parentCharacter.HitPointsMax);
        }

        protected override void OnEnable()
        {
            // Listen to all events that will affect the health bar.
            parentCharacter.OnUpdateHealthBar += UpdateValueBar;
            parentCharacter.OnHideHealthBar += HideValueBar;
            parentCharacter.OnShowHealthBar += ShowValueBar;
            parentCharacter.OnUpdateCombatState += SetState;
        }

        protected override void OnDisable()
        {
            // Stop listening to all events that will affect the health bar.
            parentCharacter.OnUpdateHealthBar -= UpdateValueBar;
            parentCharacter.OnHideHealthBar -= HideValueBar;
            parentCharacter.OnShowHealthBar -= ShowValueBar;
            parentCharacter.OnUpdateCombatState -= SetState;
        }

        public void SetState(NPCState state)
        {
            if (currentState == state)
                return;

            currentState = state;

            switch (state)
            {
                case NPCState.Moving:
                    stateUIImage.sprite = movingSprite;
                    break;
                case NPCState.Attacking:
                    stateUIImage.sprite = attackingSprite;
                    break;
                case NPCState.Defensive:
                    stateUIImage.sprite = defensiveSprite;
                    break;
                case NPCState.TargetSeen:
                    stateUIImage.sprite = targetSeenSprite;
                    break;
                case NPCState.TargetBlocking:
                    stateUIImage.sprite = targetBlockingSprite;
                    break;
            }
        }

        public override void InitializeValueBar(float maxValue)
        {
            // Set the NPC name on the health bar
            SetName(parentCharacter.GetComponent<NPC>().NPCName);

            base.InitializeValueBar(maxValue);

            parentCharacter.OnUpdateHealthBar += UpdateValueBar;
        }
        protected override IEnumerator AdjustBarWidth(float amount)
        {
            healthBarParent.SetActive(true);
            ShowHealthBar();

            if (fadeOutCR != null)
            {
                StopCoroutine(fadeOutCR);
            }

            // Enable thje background bar
            backgroundRectTransform.gameObject.SetActive(true);

            // Set the width of the background bar (grey bar) to the current width
            backgroundRectTransform.sizeDelta = new Vector2(foregroundRectTransform.rect.width, backgroundRectTransform.rect.height);

            // Set the width of the foreground bar (main red bar) to the target width
            foregroundRectTransform.sizeDelta = new Vector2(TargetWidth, foregroundRectTransform.rect.height);

            // While the width of the foreground bar is not equal to the target width, lerp the width of the foreground bar to the target width
            while (backgroundRectTransform.rect.width != TargetWidth)
            {
                backgroundRectTransform.sizeDelta = new Vector2(Mathf.Lerp(backgroundRectTransform.rect.width, TargetWidth, Time.deltaTime * animationSpeed), backgroundRectTransform.rect.height);

                yield return null;
            }

            if (fadeOutCR != null)
            {
                StopCoroutine(fadeOutCR);
            }

            fadeOutCR = StartCoroutine(FadeOutValueBar());

            yield break;
        }
        //protected override IEnumerator AdjustBarWidth(float amount)
        //{
        //    healthBarParent.SetActive(true);
        //    ShowHealthBar();

        //    if (fadeOutCR != null)
        //    {
        //        StopCoroutine(fadeOutCR);
        //    }

        //    backgroundRectTransform.gameObject.SetActive(true);

        //    RectTransform suddenChangeBar = amount >= 0 ? backgroundRectTransform : foregroundRectTransform;
        //    RectTransform slowChangeBar = amount >= 0 ? foregroundRectTransform : backgroundRectTransform;

        //    suddenChangeBar.rect.Set(suddenChangeBar.rect.x, suddenChangeBar.rect.y, TargetWidth, suddenChangeBar.rect.height);

        //    float epsilon = 0.01f; // A small value to check for close-enough equality.

        //    while (Mathf.Abs(slowChangeBar.rect.width - TargetWidth) > epsilon)
        //    {
        //        print("adjusting");

        //        slowChangeBar.rect.Set(
        //            slowChangeBar.rect.x,
        //            slowChangeBar.rect.y, 
        //            Mathf.Lerp(slowChangeBar.rect.width, TargetWidth, Time.deltaTime * animationSpeed), 
        //            slowChangeBar.rect.height);

        //        yield return null;
        //    }

        //    slowChangeBar.rect.Set(slowChangeBar.rect.x, slowChangeBar.rect.y, TargetWidth, slowChangeBar.rect.height);

        //    backgroundRectTransform.gameObject.SetActive(false);

        //    if (fadeOutCR != null)
        //    {
        //        StopCoroutine(fadeOutCR);
        //    }

        //    fadeOutCR = StartCoroutine(FadeOutHealthBar());
       // }

        public void SetName(string name)
        {
            npcNameTextComponent.text = name;
        }

        private void ShowHealthBar()
        {
            foreach (var element in rectElements)
            {
                Color newColor = element.GetComponent<Image>().color;
                newColor.a = 1f; // Fully visible.
                element.GetComponent<Image>().color = newColor;
            }
        }

        protected override void HideValueBar()
        {
            healthBarParent.SetActive(false);
            backgroundRectTransform.gameObject.SetActive(false);

            foreach (var element in rectElements)
            {
                Color newColor = element.GetComponent<Image>().color;
                newColor.a = 0f; // Fully transparent.
                element.GetComponent<Image>().color = newColor;
            }
        }
    }
}