using Gley.UrbanSystem;
using Gley.UrbanSystem.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if GLEY_PEDESTRIAN_SYSTEM
using PedestrianTypes = Gley.PedestrianSystem.User.PedestrianTypes;
#else
using PedestrianTypes = Gley.PedestrianSystem.PedestrianTypes;
#endif

namespace Gley.PedestrianSystem.Editor
{
    public class PedestrianRoutesSetupWindow : PedestrianSetupWindow
    {
        private readonly float _scrollAdjustment = 104;

        private PedestrianWaypointEditorData _pedestrianWaypointData;
        private PedestrianWaypointDrawer _pedestrianWaypointDrawer;
        private List<int> _pedestrianTypes;


        public override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _pedestrianWaypointData = new PedestrianWaypointEditorData();
            _pedestrianWaypointDrawer = new PedestrianWaypointDrawer(_pedestrianWaypointData);
            _pedestrianWaypointDrawer.OnWaypointClicked += WaypointClicked;
            _pedestrianTypes = new List<int>((int[])System.Enum.GetValues(typeof(PedestrianTypes)));

            RoutesColorUtility.SyncRoutesColors(_pedestrianTypes, _editorSave.AgentRoutes);

            return this;
        }


        public override void DrawInScene()
        {
            for (int i = 0; i < _pedestrianTypes.Count; i++)
            {
                if (_editorSave.AgentRoutes.Active[i])
                {
                    _pedestrianWaypointDrawer.ShowWaypointsWithPedestrian((PedestrianTypes)i, _editorSave.AgentRoutes.RoutesColor[i]);
                }
            }

            base.DrawInScene();
        }


        protected override void ScrollPart(float width, float height)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.Width(width - SCROLL_SPACE), GUILayout.Height(height - _scrollAdjustment));
            EditorGUILayout.LabelField("Car Routes: ");
            for (int i = 0; i < _pedestrianTypes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(ConvertIndexToEnumName(i), GUILayout.MaxWidth(150));
                _editorSave.AgentRoutes.RoutesColor[i] = EditorGUILayout.ColorField(_editorSave.AgentRoutes.RoutesColor[i]);
                Color oldColor = GUI.backgroundColor;
                if (_editorSave.AgentRoutes.Active[i])
                {
                    GUI.backgroundColor = Color.green;
                }
                if (GUILayout.Button("View", GUILayout.MaxWidth(BUTTON_DIMENSION)))
                {
                    _editorSave.AgentRoutes.Active[i] = !_editorSave.AgentRoutes.Active[i];
                    SceneView.RepaintAll();
                }
                GUI.backgroundColor = oldColor;
                EditorGUILayout.EndHorizontal();
            }

            base.ScrollPart(width, height);
            EditorGUILayout.EndScrollView();
        }


        private int GetNrOfDifferentAgents()
        {
            return System.Enum.GetValues(typeof(PedestrianTypes)).Length;
        }


        private void WaypointClicked(WaypointSettingsBase clickedWaypoint, bool leftClick)
        {
            _window.SetActiveWindow(typeof(EditPedestrianWaypointWindow), true);
        }


        private string ConvertIndexToEnumName(int i)
        {
            return ((PedestrianTypes)i).ToString();
        }


        public override void DestroyWindow()
        {
            if (_pedestrianWaypointDrawer != null)
            {
                _pedestrianWaypointDrawer.OnWaypointClicked -= WaypointClicked;
                _pedestrianWaypointDrawer.OnDestroy();
            }
            base.DestroyWindow();
        }
    }
}
