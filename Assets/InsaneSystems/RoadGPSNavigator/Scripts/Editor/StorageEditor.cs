using UnityEditor;
using UnityEngine;

namespace InsaneSystems.RoadNavigator
{
    [CustomEditor(typeof(Storage))]
    public class StorageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var guiEnabled = GUI.enabled;
            
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Please, change settings when Playmode is not running.", MessageType.Info);
                GUI.enabled = false;
            }
            
            DrawDefaultInspector();

            GUI.enabled = guiEnabled;
        }
    }
}