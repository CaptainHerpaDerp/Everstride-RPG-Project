
// Author: The Messy Coder
using UnityEngine;

namespace UIElements
{
    [AddComponentMenu("Layout/TMP Container Size Fitter")]
    public class VerticalTextContainerFitter : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI m_TMProUGUI;

        [SerializeField]
        private float startHeight, padding = 40, maxHeight;

        private bool doRebuild = true;

        public TMPro.TextMeshProUGUI TextMeshPro
        {
            get
            {
                if (m_TMProUGUI == null && transform.GetComponentInChildren<TMPro.TextMeshProUGUI>())
                {
                    m_TMProUGUI = transform.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    m_TMPRectTransform = m_TMProUGUI.rectTransform;
                }
                return m_TMProUGUI;
            }
        }

        protected RectTransform m_RectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (m_RectTransform == null)
                {
                    m_RectTransform = GetComponent<RectTransform>();
                }
                return m_RectTransform;
            }
        }

        protected RectTransform m_TMPRectTransform;
        public RectTransform TMPRectTransform { get { return m_TMPRectTransform; } }

        protected float m_PreferredHeight;
        public float PreferredHeight { get { return m_PreferredHeight; } }

        protected virtual void SetHeight()
        {
            if (TextMeshPro == null)
                return;

            // Calculate the preferred height of the text, including padding
            m_PreferredHeight = TextMeshPro.preferredHeight + padding;

            // Adjust or remove constraints as necessary
            if (m_PreferredHeight < startHeight)
            {
                m_PreferredHeight = startHeight; // Enforce a minimum height
            }

            if (maxHeight > 0 && m_PreferredHeight > maxHeight)
            {
                m_PreferredHeight = maxHeight; // Enforce a maximum height
            }

            // Update the height of the rectTransform while preserving the width
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, m_PreferredHeight);

            // Mark the layout as dirty to trigger re-layout
            SetDirty();
        }


        protected virtual void OnEnable()
        {
            SetHeight();
        }

        protected virtual void Start()
        {
            //responseHandler = FindObjectOfType<ResponseHandler>();

            //responseHandler.responsesClosed.AddListener(StartRectRebuild);
            //responseHandler.responsesOpen.AddListener(StopRectRebuild);

            SetHeight();
        }

        protected virtual void Update()
        {
            // Only rebuild if the preferred height changes
            if (PreferredHeight != TextMeshPro.preferredHeight && doRebuild)
            {
                SetHeight();
            }
        }


        private void StopRectRebuild()
        {
            doRebuild = false;
        }

        private void StartRectRebuild()
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, startHeight);
            doRebuild = true;
        }

        #region MarkLayoutForRebuild
        public virtual bool IsActive()
        {
            return isActiveAndEnabled;
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

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

        #endregion
    }
}