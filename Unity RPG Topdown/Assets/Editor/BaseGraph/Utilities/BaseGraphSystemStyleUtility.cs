using UnityEditor;
using UnityEngine.UIElements;

namespace GraphSystem.Base.Utilities
{
    public static class BaseGraphSystemStyleUtility
    {
        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (var className in classNames)
            {
                element.AddToClassList(className);
            }

            return element;
        }

        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            foreach (var styleSheetName in styleSheetNames)
            {
                StyleSheet styleSheet = EditorGUIUtility.Load(styleSheetName) as StyleSheet;
                element.styleSheets.Add(styleSheet);
            }

            return element;
        }

        public static VisualElement AddStyleSheet(this VisualElement element, string styleSheetName)
        {
            StyleSheet styleSheet = EditorGUIUtility.Load(styleSheetName) as StyleSheet;
            element.styleSheets.Add(styleSheet);

            return element;
        }
    }
}