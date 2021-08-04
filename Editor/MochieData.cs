using UnityEditor;
using UnityEngine;

namespace Pumkin.MochieTools
{
    internal class MochieData
    {
        public static class Styles
        {
            public static GUIStyle EditorLine { get; private set; }
            public static GUIStyle RoundedBox { get; private set; }
            public static GUIStyle IconButton { get; private set; }
            public static GUIStyle MediumButton { get; private set; }
            public static GUIStyle BigButton { get; private set; }
            public static GUIStyle TitleLabel { get; private set; }
            
            
            static Styles()
            {
                EditorLine = new GUIStyle("box")
                {
                    border = new RectOffset(1, 1, 1, 1), 
                    margin = new RectOffset(5, 5, 1, 1),
                    padding = new RectOffset(1, 1, 1, 1),
                };
                
                RoundedBox = new GUIStyle("helpBox")
                {
                    margin = new RectOffset(3, 3, 3, 3),
                    padding = new RectOffset(10, 10, 6, 3),
                    border = new RectOffset(6, 6, 6, 6),
                    fontSize = 12,
                    alignment = TextAnchor.UpperLeft,
                    stretchWidth = true,
                };
                
                IconButton = new GUIStyle("button")
                {
                    fixedWidth = 18f,
                    fixedHeight = 18f,
                    imagePosition = ImagePosition.ImageOnly,
                    padding = new RectOffset(0, 0, 0, 0),
                };
                
                MediumButton = new GUIStyle("button")
                {
                    fixedHeight = 20f,
                    margin = new RectOffset(2,2,4,6)
                };

                BigButton = new GUIStyle("button")
                {
                    fixedHeight = 30f,
                    margin = new RectOffset(2,2,4,6)
                };
                
                TitleLabel = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 15, stretchHeight = true, clipping = TextClipping.Overflow
                };
            }
        }

        public static class Icons
        {
            public static Texture Options { get; private set; }
            public static Texture Default { get; private set; }
            public static Texture Add { get; private set; }
            public static Texture Remove { get; private set; }
            public static Texture RemoveAll { get; private set; }

            static Icons()
            {
                Default = EditorGUIUtility.IconContent("DefaultAsset Icon")?.image;
                Options = EditorGUIUtility.IconContent("LookDevPaneOption")?.image 
                    ? EditorGUIUtility.IconContent("LookDevPaneOption")?.image : Default;
                
                Add = EditorGUIUtility.IconContent("Toolbar Plus")?.image 
                    ? EditorGUIUtility.IconContent("Toolbar Plus")?.image : Default;
                
                Remove = EditorGUIUtility.IconContent("Toolbar Minus")?.image 
                    ? EditorGUIUtility.IconContent("Toolbar Minus")?.image : Default;
                
                RemoveAll = EditorGUIUtility.IconContent("vcs_delete")?.image 
                    ? EditorGUIUtility.IconContent("vcs_delete")?.image : Default;
            }
        }
    }
}