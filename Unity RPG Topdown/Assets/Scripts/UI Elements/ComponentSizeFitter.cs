using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIElements
{
    public class ComponentSizeFitter : MonoBehaviour
    {
        [BoxGroup("Components"), SerializeField] private RectTransform rectTransform;

        [BoxGroup("Components"), SerializeField] private List<RectTransform> components;

        [BoxGroup("Components"), SerializeField] private float startWidth, startHeight, paddingX, paddingY;

        [BoxGroup("Settings"), SerializeField] private bool fitWidth, fitHeight;

        [SerializeField] private bool doRebuild = true;

        [ShowInInspector] private float preferredWidth, preferredHeight;

        protected float GetComponentHeight()
        {
            float componentHeight = 0;

            foreach (var component in components)
            {
                if (component != null && component.gameObject.activeInHierarchy)
                    componentHeight += component.rect.height;
            }

            return componentHeight;
        }

        protected float GetComponentWidth()
        {
            float componentWidth= 0;

            foreach (var component in components)
            {
                if (component != null && component.gameObject.activeInHierarchy)
                    componentWidth += component.rect.width;
            }

            return componentWidth;
        }

        protected virtual void SetSize()
        {

            preferredHeight = GetComponentHeight() + paddingY;
            preferredWidth = GetComponentWidth() + paddingX;


            if (fitWidth)
                rectTransform.sizeDelta = new Vector2(preferredWidth, rectTransform.sizeDelta.y);


            if (fitHeight)
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, preferredHeight);


            SetDirty();
        }

        protected virtual void OnEnable()
        {
            SetSize();
        }

        protected virtual void Start()
        {
            SetSize();
        }

        protected virtual void Update()
        {
            if (!doRebuild)
                return;


            SetSize();

        }

        protected void SetDirty()
        {
            if (!isActiveAndEnabled)
                return;

            foreach (RectTransform component in components)
            {
                if (component == null)
                    return;

                LayoutRebuilder.MarkLayoutForRebuild(component);
            }

            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected virtual void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            SetDirty();
        }
#endif
    }
}

