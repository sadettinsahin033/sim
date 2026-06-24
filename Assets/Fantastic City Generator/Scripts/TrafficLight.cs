using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCG
{
    public class TrafficLight : MonoBehaviour
    {
        public GameObject Green;
        public GameObject Yellow;
        public GameObject Red;
        public GameObject Pedestrians;
        public GameObject StopCollider;
        public GameObject StopPedestrianCollider;

        public enum CurrentLightColor
        {
            Red,
            Yellow,
            Green,
            Pedestrian
        }

        [Header("Current State")]
        [SerializeField] private CurrentLightColor currentColor = CurrentLightColor.Red;

        public CurrentLightColor GetCurrentColor()
        {
            return currentColor;
        }

        public bool IsRed()
        {
            return currentColor == CurrentLightColor.Red;
        }

        public bool IsYellow()
        {
            return currentColor == CurrentLightColor.Yellow;
        }

        public bool IsGreen()
        {
            return currentColor == CurrentLightColor.Green;
        }

        public bool IsPedestrianPhase()
        {
            return currentColor == CurrentLightColor.Pedestrian;
        }

        public void SetStatus(string status)
        {
            Red.SetActive(status == "1");
            Yellow.SetActive(status == "2");
            Green.SetActive(status == "3");
            Pedestrians.SetActive(status == "4");
            StopCollider.SetActive(status != "3");
            StopPedestrianCollider.SetActive(status != "4");

            if (status == "1")
                currentColor = CurrentLightColor.Red;
            else if (status == "2")
                currentColor = CurrentLightColor.Yellow;
            else if (status == "3")
                currentColor = CurrentLightColor.Green;
            else if (status == "4")
                currentColor = CurrentLightColor.Pedestrian;
            else
                currentColor = CurrentLightColor.Red;
        }
    }
}