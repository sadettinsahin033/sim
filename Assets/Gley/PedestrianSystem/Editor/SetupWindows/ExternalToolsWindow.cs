using UnityEditor;
using UnityEngine;

using SetupWindowBase = Gley.UrbanSystem.Editor.SetupWindowBase;

namespace Gley.PedestrianSystem.Editor
{
    internal class ExternalToolsWindow : SetupWindowBase
    {
        protected override void TopPart()
        {
            base.TopPart();
            if (GUILayout.Button("Road Constructor"))
            {
                _window.SetActiveWindow(typeof(RoadConstructorSetup), true);
            }
            EditorGUILayout.Space();
        }
    }
}