using System.Collections.Generic;
using UnityEngine;

#if GLEY_PEDESTRIAN_SYSTEM
using PedestrianTypes = Gley.PedestrianSystem.User.PedestrianTypes;
#else
using PedestrianTypes = Gley.PedestrianSystem.PedestrianTypes;
#endif

namespace Gley.PedestrianSystem
{
    /// <summary>
    /// Pedestrian behaviour that makes an pedestrian run away from a specific point in the scene.
    /// The pedestrian will continuously choose waypoints that are farthest from the danger point,
    /// and move towards them at a defined run speed.
    /// </summary>
    public class RunAwayFromPoint : PedestrianBehaviour
    {
        private float _runSpeed; // Speed to use when fleeing
        private Vector3 _pointToRunFrom; // The danger point to avoid
        private Pedestrian _pedestrian; // The pedestrian this behaviour is controlling
        private PedestrianWaypointManager _waypointManager; // Manages waypoint paths
        private PedestrianWaypointsDataHandler _pedestrianWaypointsDataHandler; // Provides waypoint queries and connections

        /// <summary>
        /// Called when this behaviour becomes active on a pedestrian.
        /// Initializes references and overrides waypoint selection logic.
        /// </summary>
        protected override void OnBecomeActive()
        {
#if GLEY_PEDESTRIAN_SYSTEM
            base.OnBecomeActive();
            // Get the pedestrian object by index
            _pedestrian = API.GetPedestrian(PedestrianIndex);

            // Prevent default waypoint logic, we will control path selection manually
            _pedestrian.HasExternalWaypointSelectionMethod = true;

            // Cache managers for waypoints and data access
            _waypointManager = PedestrianManager.Instance.WaypointManager;
            _pedestrianWaypointsDataHandler = PedestrianManager.Instance.PedestrianWaypointsDataHandler;

            // Listen for waypoint requests (when pedestrian needs a new destination)
            Events.OnCustomWaypointRequested += RequestNewWaypoint;

            // Clear the pedestrian’s current path so it can be rebuilt with our logic
            _waypointManager.RemovePath(PedestrianIndex);
#endif
        }

        /// <summary>
        /// Handles requests for a new waypoint by finding the farthest valid one from the danger point.
        /// </summary>
        private void RequestNewWaypoint(int pedestrianIndex)
        {
            // Find the next waypoint farthest from the danger point
            var waypointIndex = GetNewWaypointIndex(
                _waypointManager.GetLastWaypointIndexFromPath(pedestrianIndex),
                pedestrianIndex,
                _pedestrian.Type);

            // If a valid waypoint was found, add it to the pedestrian’s path and switch to it
            if (waypointIndex != PedestrianSystemConstants.INVALID_WAYPOINT_INDEX)
            {
                _waypointManager.AddWaypointToPath(pedestrianIndex, waypointIndex);
                _waypointManager.ChangeWaypoint(pedestrianIndex);
            }
        }

        /// <summary>
        /// Determines the best new waypoint index for fleeing:
        /// Picks the waypoint with the maximum distance from the danger point.
        /// </summary>
        internal int GetNewWaypointIndex(int currentWaypointIndex, int pedestrianIndex, PedestrianTypes pedestrianType)
        {
            // Get all possible next waypoints for the pedestrian
            List<int> possibleWaypoints = _pedestrianWaypointsDataHandler.GetPossibleWaypoints(currentWaypointIndex, pedestrianType);

            // If no waypoints available, return invalid index
            if (possibleWaypoints.Count <= 0)
            {
                return PedestrianSystemConstants.INVALID_WAYPOINT_INDEX;
            }

            float maxDistance = 0;
            int resultWaypointIndex = PedestrianSystemConstants.INVALID_WAYPOINT_INDEX;

            // Iterate through candidate waypoints
            foreach (var waypointIndex in possibleWaypoints)
            {
                // Get waypoint world position
                Vector3 waypointPosition = API.GetWaypointFromIndex(waypointIndex).Position;

                // Compute squared distance to avoid using expensive sqrt
                var distance = Vector3.SqrMagnitude(waypointPosition - _pointToRunFrom);

                // Keep the farthest waypoint
                if (distance > maxDistance)
                {
                    resultWaypointIndex = waypointIndex;
                    maxDistance = distance;
                }
            }
            return resultWaypointIndex;
        }

        /// <summary>
        /// Called when this behaviour is deactivated.
        /// Resets pedestrian to default waypoint logic and unsubscribes from events.
        /// </summary>
        protected override void OnBecameInactive()
        {
            base.OnBecameInactive();

            if (_pedestrian)
            {
                _pedestrian.HasExternalWaypointSelectionMethod = false;
            }

            Events.OnCustomWaypointRequested -= RequestNewWaypoint;
        }

        /// <summary>
        /// Sets the fleeing parameters: how fast to run and from which point.
        /// </summary>
        public void SetParams(float runSpeed, Vector3 pointToRunFrom)
        {
            _runSpeed = runSpeed;
            _pointToRunFrom = pointToRunFrom;
        }

        /// <summary>
        /// Executes the animation/movement each frame.
        /// Ignores default speed and uses the fleeing run speed.
        /// </summary>
        public override void Execute(float turnAngle, float moveSpeed)
        {
            AnimatorMethods.FreeWalk(turnAngle, _runSpeed);
        }

        /// <summary>
        /// Ensures cleanup when destroyed to prevent dangling event subscriptions.
        /// </summary>
        public override void OnDestroy()
        {
            Events.OnCustomWaypointRequested -= RequestNewWaypoint;
        }
    }
}
