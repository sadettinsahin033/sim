using UnityEngine;

namespace Gley.PedestrianSystem
{
    /// <summary>
    /// Pedestrian behaviour that makes a pedestrian run towards a specified destination point.
    /// </summary>
    public class RunTowardsPoint : PedestrianBehaviour
    {
        // Speed used while running towards the target
        private float _runSpeed;

        /// <summary>
        /// Sets the parameters for this behaviour: 
        /// how fast the pedestrian should run and where it should go.
        /// </summary>
        /// <param name="runSpeed">The running speed.</param>
        /// <param name="destination">The world-space point to run to.</param>
        public void SetParams(float runSpeed, Vector3 destination)
        {
            _runSpeed = runSpeed;

            // Assign the destination for this pedestrian
            API.SetDestination(PedestrianIndex, destination);

            // Subscribe to event so we know when the pedestrian reaches the destination
            Events.OnDestinationReached += PointReached;
        }

        /// <summary>
        /// Called when the pedestrian reaches the specified destination.
        /// Currently just logs a message, but can be extended for gameplay reactions.
        /// </summary>
        private void PointReached(int pedestrianIndex)
        {
            if (pedestrianIndex == PedestrianIndex)
            {
                Debug.Log("Point Reached");
            }
        }

        /// <summary>
        /// Executes the animation/movement for the pedestrian each frame.
        /// Ignores the default speed and uses the configured run speed.
        /// </summary>
        public override void Execute(float turnAngle, float moveSpeed)
        {
            AnimatorMethods.FreeWalk(turnAngle, _runSpeed);
        }

        /// <summary>
        /// Cleanup when this behaviour is destroyed.
        /// Ensures the event is unsubscribed to avoid memory leaks or stray callbacks.
        /// </summary>
        public override void OnDestroy()
        {
            Events.OnDestinationReached -= PointReached;
        }
    }
}
