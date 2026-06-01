using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEngine;

namespace Gley.PedestrianSystem.Editor
{
    public class ShowPriorityEditedWaypoints : ShowWaypointsWindow
    {
        private readonly float _scrollAdjustment = 161;


        public override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _waypointsOfInterest = _pedestrianWaypointData.GetPriorityEditedWaypoints();
            _showDeleteButton = true;
            return this;
        }


        public override void DrawInScene()
        {
            _pedestrianWaypointDrawer.ShowPriorityEditedWaypoints(_editorSave.EditorColors.WaypointColor, _editorSave.EditorColors.PriorityColor);
            base.DrawInScene();
        }


        protected override void TopPart()
        {
            EditorGUI.BeginChangeCheck();
            _editorSave.EditorColors.WaypointColor = EditorGUILayout.ColorField("Waypoint Color",_editorSave.EditorColors.WaypointColor);
            _editorSave.EditorColors.PriorityColor = EditorGUILayout.ColorField("Priority Color",_editorSave.EditorColors.PriorityColor);
            EditorGUI.EndChangeCheck();

            if (GUILayout.Button("Delete all priority edited waypoints"))
            {
                if (EditorUtility.DisplayDialog("Delete All Waypoints", "Are you sure you want to delete all priority edited waypoints?", "Yes", "No"))
                {
                    foreach (var waypoint in _waypointsOfInterest)
                    {
                        waypoint.priorityLocked = false;
                        EditorUtility.SetDirty(waypoint);
                    }
                    RefreshWaypointsOfInterest();
                }
            }

            if (GUI.changed)
            {
                SceneView.RepaintAll();
            }
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));
            base.ScrollPart(width, height);
            GUILayout.EndScrollView();
        }

        protected override void DeleteWaypoint(PedestrianWaypointSettings waypoint)
        {
            base.DeleteWaypoint(waypoint);
            waypoint.priorityLocked = false;
            EditorUtility.SetDirty(waypoint);
            RefreshWaypointsOfInterest();
        }


        protected void RefreshWaypointsOfInterest()
        {
            _pedestrianWaypointData.LoadAllData();
            _waypointsOfInterest = _pedestrianWaypointData.GetPriorityEditedWaypoints();
            SceneView.RepaintAll();
        }
    }
}
