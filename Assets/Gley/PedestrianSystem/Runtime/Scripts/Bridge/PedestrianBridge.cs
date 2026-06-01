using Gley.UrbanSystem;
using UnityEngine;

namespace Gley.PedestrianSystem
{
    public class PedestrianBridge : IPedestrianBridge
    {
#if GLEY_PEDESTRIAN_SYSTEM
        public bool IsInitialized => PedestrianManager.Instance != null && PedestrianManager.Instance.IsInitialized();
#else
        public bool IsInitialized => false;
#endif

        //public int GetPedestriansCrossing(object intersection)
        //{
        //    if (intersection is GenericIntersection gi)
        //    {
        //        return gi.GetPedestriansCrossing().Count;
        //    }
        //    return 0;
        //}

        public bool IsStopWaypoint(int waypointIndex)
        {
#if GLEY_PEDESTRIAN_SYSTEM
            return PedestrianManager.Instance.PedestrianWaypointsDataHandler.IsStop(waypointIndex);
#else
            return false;
#endif
        }

        public Vector3 GetWaypointPosition(int waypointIndex)
        {
#if GLEY_PEDESTRIAN_SYSTEM
            return PedestrianManager.Instance.PedestrianWaypointsDataHandler.GetPosition(waypointIndex);
#else
            return Vector3.zero;
#endif
        }
    }
}
