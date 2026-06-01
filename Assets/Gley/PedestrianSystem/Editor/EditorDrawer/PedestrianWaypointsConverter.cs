using Gley.UrbanSystem;
using Gley.UrbanSystem.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gley.PedestrianSystem.Editor
{
    public class PedestrianWaypointsConverter :IWaypointsConverter
    {
        private readonly PedestrianWaypointEditorData _pedestrianWaypointEditorData;

        private Dictionary<PedestrianWaypointSettings, int> _editorWaypointsIndex;
        internal PedestrianWaypointsConverter()
        {
            _pedestrianWaypointEditorData = new PedestrianWaypointEditorData();
        }


        public void ConvertWaypoints()
        {
            VerifyPedestrianWaypoints();
            MapWaypoints();
            SetIntersectionProperties();
            SetCrossingComponentProperties();
            ConvertPedestrianWaypoints();
            AssignPedestrianWaypointsToCell();
            GeneratePathFindingWaypoints();
        }


        private void VerifyPedestrianWaypoints()
        {
            PedestrianWaypointSettings[] allPedestrianEditorWaypoints = _pedestrianWaypointEditorData.GetAllWaypoints();

            if (allPedestrianEditorWaypoints.Length <= 0)
            {
                Debug.LogWarning("No waypoints found. Go to Tools->Gley->Pedestrian System->Path Setup and create a path");
                return;
            }

            for (int i = 0; i < allPedestrianEditorWaypoints.Length; i++)
            {
                allPedestrianEditorWaypoints[i].VerifyAssignments(false);
                allPedestrianEditorWaypoints[i].ResetProperties();
            }
        }


        private void SetCrossingComponentProperties()
        {
            List<StreetCrossingComponent> allIntersectionComponents = GleyPrefabUtilities.GetAllComponents<StreetCrossingComponent>().ToList();
            for (int i = 0; i < allIntersectionComponents.Count; i++)
            {
                if (!allIntersectionComponents[i].VerifyAssignments())
                    return;

                List<PedestrianWaypointSettings> intersectionWaypoints = allIntersectionComponents[i].GetPedestrianWaypoints();
                for (int j = intersectionWaypoints.Count - 1; j >= 0; j--)
                {
                    intersectionWaypoints[j].InIntersection = true;
                }

                List<PedestrianWaypointSettings> directionWaypoints = allIntersectionComponents[i].GetDirectionWaypoints();
                for (int j = directionWaypoints.Count - 1; j >= 0; j--)
                {
                    directionWaypoints[j].Crossing = true;
                }
            }
        }


        private void ConvertPedestrianWaypoints()
        {
            PedestrianWaypointSettings[] allPedestrianEditorWaypoints = _pedestrianWaypointEditorData.GetAllWaypoints();

            // Convert to play waypoints
            var pedestrianWaypointsData = MonoBehaviourUtilities.GetOrCreateObjectScript<PedestrianWaypointsData>(PedestrianSystemConstants.PlayHolder, false);

            var pedestrianWaypoints = ConvertToPlayWaypoints(allPedestrianEditorWaypoints);
            pedestrianWaypointsData.SetPedestrianWaypoints(pedestrianWaypoints);
            SetParentTagsRecursively(pedestrianWaypointsData.gameObject);
        }

        private PedestrianWaypoint[] ConvertToPlayWaypoints(PedestrianWaypointSettings[] editorWaypoints)
        {
            var result = new PedestrianWaypoint[editorWaypoints.Length];
            for (int i = 0; i < editorWaypoints.Length; i++)
            {
                result[i] = ConvertToPlayWaypoint(editorWaypoints[i]);
            }
            return result;
        }
        public PedestrianWaypoint ConvertToPlayWaypoint(PedestrianWaypointSettings editorWaypoint)
        {
            return new PedestrianWaypoint(editorWaypoint.name,
                GetListIndex(editorWaypoint),
                editorWaypoint.transform.position,
                GetListIndex(editorWaypoint.neighbors),
                GetListIndex(editorWaypoint.prev),
                editorWaypoint.AllowedPedestrians,
                editorWaypoint.Crossing,
                editorWaypoint.LaneWidth,
                editorWaypoint.Left,
                editorWaypoint.eventData,
                editorWaypoint.triggerEvent);
        }

        private void MapWaypoints()
        {
            if (_pedestrianWaypointEditorData == null)
            {
                Debug.LogError("TrafficWaypointEditorData is null");
                _editorWaypointsIndex = new Dictionary<PedestrianWaypointSettings, int>();
                return;
            }


            PedestrianWaypointSettings[] allPedestrianEditorWaypoints = _pedestrianWaypointEditorData.GetAllWaypoints();

            if (allPedestrianEditorWaypoints == null || allPedestrianEditorWaypoints.Length == 0)
            {
                Debug.LogWarning("No waypoints found");
                _editorWaypointsIndex = new Dictionary<PedestrianWaypointSettings, int>();
                return;
            }

            _editorWaypointsIndex = new Dictionary<PedestrianWaypointSettings, int>(allPedestrianEditorWaypoints.Length);

            for (int i = 0; i < allPedestrianEditorWaypoints.Length; i++)
            {
                var wp = allPedestrianEditorWaypoints[i];

                if (wp == null)
                {
                    continue;
                }

                if (!_editorWaypointsIndex.ContainsKey(wp))
                {
                    _editorWaypointsIndex.Add(wp, i);
                }
                else
                {
                    Debug.Log(wp.name + " already exists", wp);
                }
            }
        }

        public int GetListIndex(PedestrianWaypointSettings editorWaypoint)
        {
            if (_editorWaypointsIndex == null)
            {
                Debug.LogError("Waypoint index not initialized. Call MapWaypoints() first.");
                return -1;
            }

            if (editorWaypoint == null)
            {
                Debug.LogError("Editor waypoint is null");
                return -1;
            }

            if (_editorWaypointsIndex.TryGetValue(editorWaypoint, out var index))
            {
                return index;
            }

            Debug.LogWarning("Waypoint not found in index: " + editorWaypoint.name, editorWaypoint);
            return -1;
        }

        public int[] GetListIndex(List<WaypointSettingsBase> editorWaypoints)
        {
            var result = new int[editorWaypoints.Count];
            for (int i = 0; i < editorWaypoints.Count; i++)
            {
                result[i] = GetListIndex((PedestrianWaypointSettings)editorWaypoints[i]);
            }
            return result;
        }

        public int[] GetListIndex(List<PedestrianWaypointSettings> editorWaypoints)
        {
            var result = new int[editorWaypoints.Count];
            for (int i = 0; i < editorWaypoints.Count; i++)
            {
                result[i] = GetListIndex((PedestrianWaypointSettings)editorWaypoints[i]);
            }
            return result;
        }

        private void SetParentTagsRecursively(GameObject obj)
        {
            Transform currentParent = obj.transform.parent;

            while (currentParent != null)
            {
                if (currentParent.gameObject.tag == UrbanSystemConstants.EDITOR_TAG)
                {
                    currentParent.gameObject.tag = "Untagged";
                }
                currentParent = currentParent.parent;
            }
        }

        private void AssignPedestrianWaypointsToCell()
        {
            PedestrianWaypointSettings[] allPedestrianEditorWaypoints = _pedestrianWaypointEditorData.GetAllWaypoints();

            GridData gridData;
            if (MonoBehaviourUtilities.TryGetSceneScript<GridData>(out var result))
            {
                gridData = result.Value;
            }
            else
            {
                Debug.LogError(result.Error);
                return;
            }


            // Assign pedestrian waypoint index to cell;
            for (int i = 0; i < allPedestrianEditorWaypoints.Length; i++)
            {
                if (allPedestrianEditorWaypoints[i].AllowedPedestrians.Count != 0)
                {
                    var cellData = gridData.GetCell(allPedestrianEditorWaypoints[i].transform.position);
                    gridData.AddPedestrianWaypoint(cellData, i);

                    if (allPedestrianEditorWaypoints[i].InIntersection == false)
                    {
                        if (!allPedestrianEditorWaypoints[i].name.Contains(UrbanSystemConstants.Connect))
                        {
                            gridData.AddPedestrianSpawnWaypoint(cellData, i, allPedestrianEditorWaypoints[i].AllowedPedestrians.Cast<int>().ToArray(), allPedestrianEditorWaypoints[i].priority);
                        }
                    }
                }
            }
        }


        private void SetIntersectionProperties()
        {
            //assign street crossing components
            var crossings = MonoBehaviourUtilities.FindObjectsByType<StreetCrossingComponent>();
            PedestrianWaypointSettings[] allPedestrianEditorWaypoints = _pedestrianWaypointEditorData.GetAllWaypoints();
            foreach (var crossing in crossings)
            {
                if (!crossing.VerifyAssignments())
                {
                    return;
                }
                crossing.SetComponentWaypoints(GetListIndex(crossing.GetStopWaypoints()));
            }

#if GLEY_TRAFFIC_SYSTEM
#if GLEY_PEDESTRIAN_SYSTEM

            IGenericIntersectionSettings[] allEditorIntersections = TrafficEditorBridgeRegistry.Bridge.GetAllIntersections();
            for (int i = 0; i < allEditorIntersections.Length; i++)
            {
                if (!allEditorIntersections[i].VerifyAssignments())
                    return;

                List<PedestrianWaypointSettings> intersectionWaypoints = PedestrianEditorBridge.ConvertToPedestrianWaypoints(allEditorIntersections[i].GetPedestrianWaypoints());
                for (int j = intersectionWaypoints.Count - 1; j >= 0; j--)
                {
                    intersectionWaypoints[j].InIntersection = true;
                }

                List<PedestrianWaypointSettings> directionWaypoints = PedestrianEditorBridge.ConvertToPedestrianWaypoints(allEditorIntersections[i].GetDirectionWaypoints());
                for (int j = directionWaypoints.Count - 1; j >= 0; j--)
                {
                    directionWaypoints[j].Crossing = true;
                }
            }

#endif
#endif
        }


        private void GeneratePathFindingWaypoints()
        {
            // Generate path finding waypoints
            bool pathfindingEnabled = new SettingsLoader(PedestrianSystemConstants.WindowSettingsPath).LoadSettingsAsset<PedestrianSettingsWindowData>().PathFindingEnabled;
            var modules = MonoBehaviourUtilities.GetOrCreateObjectScript<PedestrianModules>(PedestrianSystemConstants.PlayHolder, false);
            if (pathfindingEnabled)
            {
                PedestrianWaypointSettings[] allPedestrianEditorWaypoints = _pedestrianWaypointEditorData.GetAllWaypoints();
                var pedestrianPathFindingCreator = new PedestrianPathFindingCreator();
                pedestrianPathFindingCreator.GenerateWaypoints(allPedestrianEditorWaypoints,this);
                modules.SetModules(true);
            }
            else
            {
                modules.SetModules(false);
                if (MonoBehaviourUtilities.TryGetObjectScript<PathFindingData>(PedestrianSystemConstants.PlayHolder, out var result))
                {
                    GleyPrefabUtilities.DestroyImmediate(result.Value);
                }
            }
        }
    }
}