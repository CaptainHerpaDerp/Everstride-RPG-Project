
// Author: The Messy Coder
using UnityEngine;

namespace UIElements
{
    [AddComponentMenu("Layout/TMP Container Size Fitter")]
    public class HorizontalTextContainerFitter : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI m_TMProUGUI;

        [SerializeField]
        private float startWidth, padding;

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

        protected float m_PreferredWidth;
        public float PreferredWidth { get { return m_PreferredWidth; } }

        protected virtual void SetWidth()
        {
            if (TextMeshPro == null)
                return;

            m_PreferredWidth = TextMeshPro.preferredWidth + padding;

            if (m_PreferredWidth < startWidth)
                return;

            rectTransform.sizeDelta = new Vector2(m_PreferredWidth, rectTransform.sizeDelta.y);

            SetDirty();
        }

        protected virtual void OnEnable()
        {
            SetWidth();
        }

        protected virtual void Start()
        {
            //responseHandler = FindObjectOfType<ResponseHandler>();

            //responseHandler.responsesClosed.AddListener(StartRectRebuild);
            //responseHandler.responsesOpen.AddListener(StopRectRebuild);

            SetWidth();
        }

        protected virtual void Update()
        {
            if (PreferredWidth != TextMeshPro.preferredWidth && doRebuild)
            {
                SetWidth();
            }
        }

        private void StopRectRebuild()
        {
            doRebuild = false;
        }

        private void StartRectRebuild()
        {
            rectTransform.sizeDelta = new Vector2(startWidth, rectTransform.sizeDelta.y);
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