using System.Collections.Generic;
using UnityEngine;

#if GLEY_PEDESTRIAN_SYSTEM
using PedestrianTypes = Gley.PedestrianSystem.User.PedestrianTypes;
#else
using PedestrianTypes = Gley.PedestrianSystem.PedestrianTypes;
#endif

namespace Gley.PedestrianSystem
{
    public delegate int PedestrianSpawnWaypointSelector(List<Vector2Int> neighbors, Vector3 position, Vector3 direction, PedestrianTypes pedestrianType, bool useWaypointPriority);

    public class Delegates
    {
        /// <summary>
        /// Controls the selection of a free waypoint to instantiate a new pedestrian.
        /// </summary>
        /// <param name="spawnWaypointSelectorDelegate">An implementation PedestrianSpawnWaypointSelector delegate.</param>
        public static void SetSpawnWaypointSelector(PedestrianSpawnWaypointSelector spawnWaypointSelectorDelegate)
        {
#if GLEY_PEDESTRIAN_SYSTEM
            PedestrianManager.Instance.WaypointManager?.SetSpawnWaypointSelector(spawnWaypointSelectorDelegate);
#endif
        }
    }
}
