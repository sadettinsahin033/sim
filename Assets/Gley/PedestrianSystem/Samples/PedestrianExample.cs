using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace Gley.PedestrianSystem
{
    public class PedestrianExample : MonoBehaviour
    {
        private const float _crossingTimer = 10;

        private float _time = 0;
        private void Update()
        {
            if (API.IsInitialized())
            {
                // Automatic change the crossing state.
                _time += Time.deltaTime;

                if (_time > _crossingTimer)
                {
                    _time = 0;
                    API.SetCrossingState("TrafficLightsIntersection1", !API.GetCrossingState("TrafficLightsIntersection1"));
                    API.SetCrossingState("TrafficLightsCrossing2", !API.GetCrossingState("TrafficLightsCrossing2"));
                }


                // Manual change the crossing state.
                if (GetKeyDownAlpha1())
                {
                    API.SetCrossingState("TrafficLightsIntersection1", !API.GetCrossingState("TrafficLightsIntersection1"));
                }

                if (GetKeyDownAlpha2())
                {
                    API.SetCrossingState("TrafficLightsCrossing2", !API.GetCrossingState("TrafficLightsCrossing2"));
                }
            }

            if (GetKeyDownESC())
            {
                Application.Quit();
            }
        }

        private bool GetKeyDownAlpha1()
        {
#if !ENABLE_LEGACY_INPUT_MANAGER
            return Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Alpha1);
#endif
        }

        private bool GetKeyDownAlpha2()
        {
#if !ENABLE_LEGACY_INPUT_MANAGER
            return Keyboard.current != null && Keyboard.current.digit2Key.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Alpha2);
#endif
        }

        private bool GetKeyDownESC()
        {
#if !ENABLE_LEGACY_INPUT_MANAGER
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}