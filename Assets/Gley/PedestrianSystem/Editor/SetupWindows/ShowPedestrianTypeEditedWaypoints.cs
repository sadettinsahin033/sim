using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEngine;

namespace Gley.PedestrianSystem.Editor
{
    public class ShowPedestrianTypeEditedWaypoints : ShowWaypointsWindow
    {
        private readonly float _scrollAdjustment = 161;


        public override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _waypointsOfInterest = _pedestrianWaypointData.GetPedestrianTypeEditedWaypoints();
            _showDeleteButton = true;
            return this;
        }


        protected override void TopPart()
        {
            EditorGUI.BeginChangeCheck();
            _editorSave.EditorColors.WaypointColor = EditorGUILayout.ColorField("Waypoint Color", _editorSave.EditorColors.WaypointColor);
            _editorSave.EditorColors.AgentColor = EditorGUILayout.ColorField("Pedestrian Type Color", _editorSave.EditorColors.AgentColor);
            EditorGUI.EndChangeCheck();

            if (GUILayout.Button("Delete all pedestrian edited waypoints"))
            {
                if (EditorUtility.DisplayDialog("Delete All Waypoints", "Are you sure you want to delete all pedestrian edited waypoints?", "Yes", "No"))
                {
                    foreach (var waypoint in _waypointsOfInterest)
                    {
                        waypoint.PedestrianLocked = false;
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


        public override void DrawInScene()
        {
            _pedestrianWaypointDrawer.ShowPedestrianTypeEditedWaypoints(_editorSave.EditorColors.WaypointColor, _editorSave.EditorColors.AgentColor);
            base.DrawInScene();
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
            waypoint.PedestrianLocked = false;
            EditorUtility.SetDirty(waypoint);
            RefreshWaypointsOfInterest();
        }


        protected void RefreshWaypointsOfInterest()
        {
            _pedestrianWaypointData.LoadAllData();
            _waypointsOfInterest = _pedestrianWaypointData.GetPedestrianTypeEditedWaypoints();
            SceneView.RepaintAll();
        }
    }
}