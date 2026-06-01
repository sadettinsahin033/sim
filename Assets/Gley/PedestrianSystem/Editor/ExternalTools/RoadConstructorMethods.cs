#if GLEY_ROADCONSTRUCTOR_TRAFFIC
using Gley.UrbanSystem;
using PampelGames.RoadConstructor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Gley.PedestrianSystem.Editor
{
    public class RoadConstructorMethods : UnityEditor.Editor
    {
        private static string RoadConstructorWaypointsHolder
        {
            get
            {
                return $"{PedestrianSystemConstants.PACKAGE_NAME}/{UrbanSystemConstants.EDITOR_HOLDER}/RoadConstructorWaypoints";
            }
        }

        private static string RoadConstructorIntersectionHolder
        {
            get
            {
                return $"{PedestrianSystemConstants.PACKAGE_NAME}/{UrbanSystemConstants.EDITOR_HOLDER}/RoadConstructorIntersections";
            }
        }

        private static string RoadConstructorConnectionsHolder
        {
            get
            {
                return $"{PedestrianSystemConstants.PACKAGE_NAME}/{UrbanSystemConstants.EDITOR_HOLDER}/RoadConstructorConnections";
            }
        }

        public static void ExtractWaypoints(List<int> pedestrianTypes)
        {
            Dictionary<PampelGames.RoadConstructor.Waypoint, PedestrianWaypointSettings> forwardConnections;

            Debug.Log("Extracting waypoints");
            forwardConnections = new Dictionary<PampelGames.RoadConstructor.Waypoint, PedestrianWaypointSettings>();
            DestroyImmediate(GameObject.Find(RoadConstructorWaypointsHolder));
            DestroyImmediate(GameObject.Find(RoadConstructorIntersectionHolder));
            DestroyImmediate(GameObject.Find(RoadConstructorConnectionsHolder));

            var roadConstructor = FindFirstObjectByType<RoadConstructor>();

            Transform intersectionHolder = MonoBehaviourUtilities.GetOrCreateGameObject(RoadConstructorIntersectionHolder, true).transform;
            Transform waypointsHolder = MonoBehaviourUtilities.GetOrCreateGameObject(RoadConstructorWaypointsHolder, true).transform;
            Transform connectorsHolder = MonoBehaviourUtilities.GetOrCreateGameObject(RoadConstructorConnectionsHolder, true).transform;

            var forwardWaypoints = new List<PampelGames.RoadConstructor.Waypoint>();
            var backwardWaypoints = new List<PampelGames.RoadConstructor.Waypoint>();

            //extract forward waypoints
            forwardWaypoints = ExtractRoadWaypoints(roadConstructor.GetRoads(), waypointsHolder, pedestrianTypes, ref forwardConnections, TrafficLaneDirection.Forward, "Path");

            forwardWaypoints.AddRange(ExtractRoadWaypoints(roadConstructor.GetIntersections(), connectorsHolder, pedestrianTypes, ref forwardConnections, TrafficLaneDirection.Forward, "Intersection"));

            LinkPedestrianWaypoints(forwardWaypoints, forwardConnections);

            RemoveDuplicateWaypoints(0.1f, ref forwardWaypoints, ref forwardConnections);

            AddMissingConnections(forwardWaypoints, forwardConnections);

            RemoveCircularConnections(forwardWaypoints, forwardConnections);

            AddConnectWaypoints(forwardWaypoints, forwardConnections);

            ////check
            //for (var i = 0; i < forwardWaypoints.Count; i++)
            //{
            //    if (forwardConnections.TryGetValue(forwardWaypoints[i], out var trafficWaypoint))
            //    {
            //        if (trafficWaypoint.neighbors.Count == 0)
            //        {
            //            Debug.Log("0 neighbors" + trafficWaypoint, trafficWaypoint);
            //            Debug.Log("Original waypoint " + forwardWaypoints[i].name, forwardWaypoints[i]);
            //        }
            //        if (trafficWaypoint.prev.Count == 0)
            //        {
            //            Debug.Log("0 prevs" + trafficWaypoint, trafficWaypoint);
            //            Debug.Log("Original waypoint " + forwardWaypoints[i].name, forwardWaypoints[i]);
            //        }

            //        for (int j = 0; j < trafficWaypoint.neighbors.Count; j++)
            //        {
            //            if (!trafficWaypoint.neighbors[j].prev.Contains(trafficWaypoint))
            //            {
            //                Debug.Log("Not linked " + trafficWaypoint.neighbors[j], trafficWaypoint);
            //                Debug.Log("Original waypoint " + forwardWaypoints[i].name, forwardWaypoints[i]);
            //            }
            //        }
            //    }
            //}

#if !GLEY_TRAFFIC_SYSTEM
            CreateIntersections(connectorsHolder, intersectionHolder);
#endif
            PedestrianEditorBridgeInitializer.Register();
            Debug.Log("Done");
        }

        private static void AddConnectWaypoints(List<PampelGames.RoadConstructor.Waypoint> forwardWaypoints, Dictionary<PampelGames.RoadConstructor.Waypoint, PedestrianWaypointSettings> forwardConnections)
        {
            foreach (var waypoint in forwardWaypoints)
            {
                if (forwardConnections.TryGetValue(waypoint, out var trafficWaypoint))
                {
                    if(trafficWaypoint.name.Contains(UrbanSystemConstants.ConnectionWaypointName))
                    {
                        for(int i = 0;i<trafficWaypoint.neighbors.Count;i++)
                        {
                            if (trafficWaypoint.neighbors[i].name.Contains(UrbanSystemConstants.WaypointNamePrefix))
                            {
                                trafficWaypoint.neighbors[i].name += "-" + UrbanSystemConstants.ConnectionEdgeName;
                            }
                        }
                        for (int i = 0; i < trafficWaypoint.prev.Count; i++)
                        {
                            if (trafficWaypoint.prev[i].name.Contains(UrbanSystemConstants.WaypointNamePrefix))
                            {
                                trafficWaypoint.prev[i].name += "-" + UrbanSystemConstants.ConnectionEdgeName;
                            }
                        }
                    }
                }
            }
        }


        private  static List<PedestrianWaypointSettings> GetWaypointsWithMultipleNeighbors(List<PampelGames.RoadConstructor.Waypoint> forwardWaypoints, Dictionary<PampelGames.RoadConstructor.Waypoint, PedestrianWaypointSettings> forwardConnections)
        {
            var result = new List<PedestrianWaypointSettings>();

            foreach (var waypoint in forwardWaypoints)
            {
                if (forwardConnections.TryGetValue(waypoint, out var trafficWaypoint))
                {
                    if (trafficWaypoint.neighbors.Count > 1)
                    {
                        result.Add(trafficWaypoint);
                    }
                }
            }
            return result;
        }

        private static void RemoveCircularConnections(List<PampelGames.RoadConstructor.Waypoint> forwardWaypoints, Dictionary<PampelGames.RoadConstructor.Waypoint, PedestrianWaypointSettings> forwardConnections)
        {
            for (var i = 0; i < forwardWaypoints.Count; i++)
            {
                if (forwardConnections.TryGetValue(forwardWaypoints[i], out var trafficWaypoint))
                {
                    List<WaypointSettingsBase> neighborsToRemove = new List<WaypointSettingsBase>();
                    List<WaypointSettingsBase> prevsToRemove = new List<WaypointSettingsBase>();

                    for (int j = 0; j < trafficWaypoint.prev.Count; j++)
                    {
                        for (int k = 0; k < trafficWaypoint.neighbors.Count; k++)
                        {
                            if (trafficWaypoint.prev[j] == trafficWaypoint.neighbors[k])
                            {
                                if (trafficWaypoint.prev.Count > 1 && j<k)
                                {
                                    prevsToRemove.Add(trafficWaypoint.prev[j]);
                                }
                                else
                                {
                                    neighborsToRemove.Add(trafficWaypoint.prev[j]);
                                }
                            }
                        }
                    }

                    if(neighborsToRemove.Count>0 || prevsToRemove.Count>0)
                    {
                        //Debug.Log($"For {trafficWaypoint.name}", trafficWaypoint);
                        foreach (var neighbor in neighborsToRemove)
                        {
                            //Debug.Log($"Remove neighbor {neighbor.name}");
                            trafficWaypoint.neighbors.Remove(neighbor);
                        }
                        foreach (var prev in prevsToRemove)
                        {
                            //Debug.Log($"Remove prev {prev.name}");
                            trafficWaypoint.prev.Remove(prev);
                        }
                    }
                }
            }
        }


        private static void AddMissingConnections(List<PampelGames.RoadConstructor.Waypoint> forwardWaypoints, Dictionary<PampelGames.RoadConstructor.Waypoint, PedestrianWaypointSettings> forwardConnections)
        {
            for (var i = 0; i < forwardWaypoints.Count; i++)
            {
                if (forwardConnections.TryGetValue(forwardWaypoints[i], out var trafficWaypoint))
                {
                    for (int j = 0; j < trafficWaypoint.neighbors.Count; j++)
                    {
                        if (!trafficWaypoint.neighbors[j].prev.Contains(trafficWaypoint))
                        {
                            trafficWaypoint.neighbors[j].prev.Add(trafficWaypoint);
                        }
                    }
                    for (int j = 0; j < trafficWaypoint.prev.Count; j++)
                    {
                        if (!trafficWaypoint.prev[j].neighbors.Contains(trafficWaypoint))
                        {
                            trafficWaypoint.prev[j].neighbors.Add(trafficWaypoint);
                        }
                    }
                }
            }
        }


        private static void LinkPedestrianWaypoints(List<PampelGames.RoadConstructor.Waypoint> waypoints, Dictionary<PampelGames.RoadConstructor.Waypoint, PedestrianWaypointSettings> connectionsList)
        {
            for (var i = 0; i < waypoints.Count; i++)
            {
                if (connectionsList.TryGetValue(waypoints[i], out var trafficWaypoint))
                {
                    for (int j = 0; j < waypoints[i].next.Count; j++)
                    {
                        if (connectionsList.TryGetValue(waypoints[i].next[j], out var neighbor))
                        {
                            trafficWaypoint.neighbors.Add(neighbor);
                        }
                    }
                    for (int j = 0; j < waypoints[i].prev.Count; j++)
                    {
                        if (connectionsList.TryGetValue(waypoints[i].prev[j], out var neighbor))
                        {
                            trafficWaypoint.prev.Add(neighbor);
                        }
                    }
                }
            }
        }


        private static List<PampelGames.RoadConstructor.Waypoint> ExtractRoadWaypoints<T>(List<T> roadObjects, Transform waypointsHolder, List<int> pedestrianTypes, ref Dictionary<PampelGames.RoadConstructor.Waypoint, PedestrianWaypointSettings> connections, TrafficLaneDirection laneDirection, string name) where T : SceneObject
        {
            var result = new List<PampelGames.RoadConstructor.Waypoint>();
            for (var i = 0; i < roadObjects.Count; i++)
            {
                var trafficLanes = roadObjects[i].GetTrafficLanes(TrafficLaneType.Pedestrian, laneDirection);
                if (trafficLanes.Count == 0)
                {
                    continue;
                }

                var roadName = $"{name}_{roadObjects[i].name.Split("-")[1]}";
                Transform road = GetOrCreateGameObject(roadName, waypointsHolder, trafficLanes[0].spline.Knots.First().Position, true).transform;
                Vector3 averagePosition = Vector3.zero;
                for (var j = 0; j < trafficLanes.Count; j++)
                {
                    var waypoints = trafficLanes[j].GetWaypoints();
                    if (trafficLanes[j].crossing)
                    {
                        var average = Vector3.zero;
                        for (int k = 0; k < waypoints.Count; k++)
                        {
                            average += waypoints[k].transform.position;
                        }
                        average = average / waypoints.Count;
                        if ((averagePosition - average).sqrMagnitude < 1)
                        {
                            continue;
                        }
                        else
                        {
                            averagePosition = average;
                        }
                    }

                    Transform lane = MonoBehaviourUtilities.CreateGameObject($"{UrbanSystemConstants.LaneNamePrefix}_{j}", road, trafficLanes[j].spline.Knots.First().Position, true).transform;

                    result.AddRange(waypoints);
                    if (waypoints.Count > 0)
                    {
                        CreatePedestrianWaypoints(lane, waypoints, trafficLanes[j].width, $"{road.name}-{lane.name}", false, pedestrianTypes, trafficLanes[j].crossing, ref connections);
                    }
                }
            }
            return result;
        }


        private static GameObject GetOrCreateGameObject(string name, Transform parent, Vector3 position, bool addEditorTag)
        {
            var obj = parent.Find(name);
            GameObject go;
            if (obj == null)
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, false);
                go.transform.position = position;
            }
            else
            {
                go = obj.gameObject;
            }

            if (addEditorTag)
            {
                go.tag = UrbanSystemConstants.EDITOR_TAG;
            }
            return go;
        }

        private static void CreateIntersections(Transform holder, Transform intersectionHolder)
        {
            for (int i = 0; i < holder.childCount; i++)
            {
                if (holder.GetChild(i).childCount > 2)
                {
                    var intersection = MonoBehaviourUtilities.CreateGameObject(holder.GetChild(i).name, intersectionHolder, holder.GetChild(i).position, true);
                    var intersectionScript = intersection.AddComponent<StreetCrossingComponent>();
                    var stopWaypoints = new List<PedestrianWaypointSettings>();
                    var directionWaypoints = new List<PedestrianWaypointSettings>();
                    for (int j = 0; j < holder.GetChild(i).childCount; j++)
                    {
                        //iterate through lanes
                        for (int k = 0; k < holder.GetChild(i).GetChild(j).childCount; k++)
                        {
                            if (holder.GetChild(i).GetChild(j).GetChild(k).name.Contains(UrbanSystemConstants.ConnectionEdgeName))
                            {
                                var waypoint = holder.GetChild(i).GetChild(j).GetChild(k).GetComponent<PedestrianWaypointSettings>();
                                stopWaypoints.Add(waypoint);
                                for (int l = 0; l < waypoint.neighbors.Count; l++)
                                {
                                    if (waypoint.neighbors[l].name.Contains(UrbanSystemConstants.ConnectionWaypointName))
                                    {
                                        directionWaypoints.Add((PedestrianWaypointSettings)waypoint.neighbors[l]);
                                    }
                                }
                                for (int l = 0; l < waypoint.prev.Count; l++)
                                {
                                    if (waypoint.prev[l].name.Contains(UrbanSystemConstants.ConnectionWaypointName))
                                    {
                                        directionWaypoints.Add((PedestrianWaypointSettings)waypoint.prev[l]);
                                    }
                                }
                            }
                        }
                    }

                    intersectionScript.SetStopWaypoints(stopWaypoints);
                    intersectionScript.SetDirectionWaypoints(directionWaypoints);
                }
            }
        }

        static void RemoveDuplicateWaypoints(float minDistance, ref List<PampelGames.RoadConstructor.Waypoint> forwardWaypoints, ref Dictionary<PampelGames.RoadConstructor.Waypoint, PedestrianWaypointSettings> forwardConnections)
        {
            float cellSize = minDistance;
            Dictionary<Vector3Int, PampelGames.RoadConstructor.Waypoint> grid = new();

            for (var i = forwardWaypoints.Count - 1; i > 0; i--)
            {
                var wp = forwardWaypoints[i];
                var pos = wp.transform.position;



                Vector3Int cell = new Vector3Int(Mathf.FloorToInt(pos.x / cellSize), Mathf.FloorToInt(pos.y / cellSize), Mathf.FloorToInt(pos.z / cellSize));
                PampelGames.RoadConstructor.Waypoint found = null;

                for (int x = -1; x <= 1 && found == null; x++)
                {
                    for (int y = -1; y <= 1 && found == null; y++)
                    {
                        for (int z = -1; z <= 1 && found == null; z++)
                        {
                            Vector3Int neighborCell = cell + new Vector3Int(x, y, z);

                            if (grid.TryGetValue(neighborCell, out var other))
                            {
                                if (Vector3.SqrMagnitude(other.transform.position - pos) <= minDistance)
                                {
                                    found = other;
                                }
                            }
                        }
                    }
                }

                if (found != null)
                {
                    var pedestrianWaypointToUpdate = forwardConnections.GetValueOrDefault(found);
                    var pedestrianWaypointToDelete = forwardConnections.GetValueOrDefault(wp);

                    MoveConnections(pedestrianWaypointToUpdate, pedestrianWaypointToDelete);

                    forwardConnections.Remove(wp);
                    forwardWaypoints.RemoveAt(i);
                    DestroyImmediate(pedestrianWaypointToDelete.gameObject);
                }
                else
                {
                    grid[cell] = wp;
                }
            }
        }

        static void MoveConnections(PedestrianWaypointSettings pedestrianWaypointToUpdate, PedestrianWaypointSettings pedestrianWaypointToDelete)
        {
            if (pedestrianWaypointToUpdate.neighbors.Contains(pedestrianWaypointToDelete))
            {
                pedestrianWaypointToUpdate.neighbors.Remove(pedestrianWaypointToDelete);
                for (int k = 0; k < pedestrianWaypointToDelete.neighbors.Count; k++)
                {
                    if (!pedestrianWaypointToUpdate.neighbors.Contains(pedestrianWaypointToDelete.neighbors[k]) && pedestrianWaypointToDelete.neighbors[k] != pedestrianWaypointToUpdate)
                    {
                        pedestrianWaypointToUpdate.neighbors.Add(pedestrianWaypointToDelete.neighbors[k]);
                    }

                    pedestrianWaypointToDelete.neighbors[k].prev.Remove(pedestrianWaypointToDelete);
                    pedestrianWaypointToDelete.neighbors[k].prev.Add(pedestrianWaypointToUpdate);
                }
            }
            else
            {
                if (pedestrianWaypointToUpdate.prev.Contains(pedestrianWaypointToDelete))
                {
                    pedestrianWaypointToUpdate.prev.Remove(pedestrianWaypointToDelete);
                    pedestrianWaypointToDelete.neighbors.Remove(pedestrianWaypointToUpdate);
                    for (int k = 0; k < pedestrianWaypointToDelete.prev.Count; k++)
                    {
                        pedestrianWaypointToDelete.prev[k].neighbors.Remove(pedestrianWaypointToDelete);
                        pedestrianWaypointToDelete.prev[k].neighbors.Add(pedestrianWaypointToUpdate);
                        pedestrianWaypointToUpdate.prev.Add(pedestrianWaypointToDelete.prev[k]);
                    }

                    for (int k = 0; k < pedestrianWaypointToDelete.neighbors.Count; k++)
                    {
                        pedestrianWaypointToDelete.neighbors[k].prev.Remove(pedestrianWaypointToDelete);
                        pedestrianWaypointToDelete.neighbors[k].prev.Add(pedestrianWaypointToUpdate);
                        pedestrianWaypointToUpdate.neighbors.Add(pedestrianWaypointToDelete.neighbors[k]);
                    }
                }
                else
                {
                    pedestrianWaypointToUpdate.prev.AddRange(pedestrianWaypointToDelete.prev);
                    pedestrianWaypointToUpdate.neighbors.AddRange(pedestrianWaypointToDelete.neighbors);
                    for (int k = 0; k < pedestrianWaypointToDelete.prev.Count; k++)
                    {
                        pedestrianWaypointToDelete.prev[k].neighbors.Remove(pedestrianWaypointToDelete);
                        pedestrianWaypointToDelete.prev[k].prev.Remove(pedestrianWaypointToDelete);
                        pedestrianWaypointToDelete.prev[k].neighbors.Add(pedestrianWaypointToUpdate);
                    }

                    for (int k = 0; k < pedestrianWaypointToDelete.neighbors.Count; k++)
                    {
                        pedestrianWaypointToDelete.neighbors[k].prev.Remove(pedestrianWaypointToDelete);
                        pedestrianWaypointToDelete.neighbors[k].neighbors.Remove(pedestrianWaypointToDelete);
                        pedestrianWaypointToDelete.neighbors[k].prev.Add(pedestrianWaypointToUpdate);
                    }
                }
            }
        }


        private static void CreatePedestrianWaypoints(Transform waypointsHolder, List<PampelGames.RoadConstructor.Waypoint> waypoints, float laneWidth, string name, bool intersection, List<int> vehicleTypes, bool crossing, ref Dictionary<PampelGames.RoadConstructor.Waypoint, PedestrianWaypointSettings> result)
        {
            PedestrianWaypointCreator waypointCreator = new PedestrianWaypointCreator();
            for (int i = 0; i < waypoints.Count; i++)
            {
                var waypointName = name;
                if (crossing)
                {
                    waypointName += "-" + UrbanSystemConstants.ConnectionWaypointName + i;
                }
                else
                {
                    waypointName += "-Waypoint_" + i;
                }

                var transform = waypointCreator.CreateWaypoint(waypointsHolder, waypoints[i].transform.position, waypointName, vehicleTypes, laneWidth);
                result.Add(waypoints[i], transform.GetComponent<PedestrianWaypointSettings>());
            }
        }
    }
}
#endif