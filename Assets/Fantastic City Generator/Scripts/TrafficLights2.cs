using System.Collections;
using UnityEngine;

namespace FCG
{
    public class TrafficLights2 : MonoBehaviour
    {
        [Header("Debug")]
        public bool debugLogs = false;

        private float countTime = 0;
        private int step = 0;

        private int status;

        private Coroutine delayedResetCoroutine;

        public TrafficLight trafficLight_N;
        public TrafficLight trafficLight_S;
        public TrafficLight trafficLight_E;
        public TrafficLight trafficLight_W;

        public enum TrafficDirection
        {
            North,
            South,
            East,
            West
        }

        public enum LightColor
        {
            Red,
            Yellow,
            Green
        }

        void Start()
        {
            countTime = 0;
            step = 0;

            status = (Random.Range(1, 8) < 4) ? 13 : 31;

            EnabledObjects(status);

            if (debugLogs)
            {
                Debug.Log("[TrafficLights2] Başlangıç status: " + status + " | step: " + step + " | countTime: " + countTime);
            }

            InvokeRepeating(nameof(TrafficLightTurn), Random.Range(0, 4), 1);
        }

        private void TrafficLightTurn()
        {
            countTime += 1;

            if (step == 0)
            {
                if (countTime > 16) // Ana kırmızı / yeşil süresi
                {
                    countTime = 0;
                    step = 1;

                    if (status == 13)
                        status = 12;
                    else if (status == 31)
                        status = 21;

                    EnabledObjects(status);

                    if (debugLogs)
                    {
                        Debug.Log("[TrafficLights2] Ana faz bitti. Sarı faza geçildi. Status: " + status + " | step: " + step);
                    }
                }
            }
            else if (step == 1)
            {
                if (countTime >= 5) // Sarı süresi
                {
                    countTime = 0;
                    step = 2;

                    if (status == 12)
                        status = 41;
                    else if (status == 21)
                        status = 14;

                    EnabledObjects(44);

                    if (debugLogs)
                    {
                        Debug.Log("[TrafficLights2] Sarı faz bitti. Yaya/ara faza geçildi. Görünen status: 44 | iç status: " + status + " | step: " + step);
                    }
                }
            }
            else if (step == 2)
            {
                if (countTime >= 7) // Yaya / ara geçiş süresi
                {
                    countTime = 0;
                    step = 0;

                    if (status == 14)
                        status = 13;
                    else if (status == 41)
                        status = 31;

                    EnabledObjects(status);

                    if (debugLogs)
                    {
                        Debug.Log("[TrafficLights2] Yaya/ara faz bitti. Yeni ana faz başladı. Status: " + status + " | step: " + step);
                    }
                }
            }
        }

        private void EnabledObjects(int st)
        {
            string statusText = st.ToString();

            if (statusText.Length < 2)
                statusText = "0" + statusText;

            if (trafficLight_N)
                trafficLight_N.SetStatus(statusText.Substring(0, 1));

            if (trafficLight_S)
                trafficLight_S.SetStatus(statusText.Substring(0, 1));

            if (trafficLight_E)
                trafficLight_E.SetStatus(statusText.Substring(1, 1));

            if (trafficLight_W)
                trafficLight_W.SetStatus(statusText.Substring(1, 1));

            if (debugLogs)
            {
                Debug.Log(
                    "[TrafficLights2] Işıklar güncellendi. Status: " + st +
                    " | N/S: " + statusText.Substring(0, 1) +
                    " | E/W: " + statusText.Substring(1, 1)
                );
            }
        }

        public void ResetPhaseAfterDelay(TrafficDirection playerDirection, LightColor targetColor, float delay)
        {
            if (delayedResetCoroutine != null)
            {
                StopCoroutine(delayedResetCoroutine);

                if (debugLogs)
                {
                    Debug.Log("[TrafficLights2] Önceki gecikmeli reset iptal edildi.");
                }
            }

            delayedResetCoroutine = StartCoroutine(ResetPhaseRoutine(playerDirection, targetColor, delay));
        }

        private IEnumerator ResetPhaseRoutine(TrafficDirection playerDirection, LightColor targetColor, float delay)
        {
            if (debugLogs)
            {
                Debug.Log(
                    "[TrafficLights2] Reset isteği alındı. Yön: " + playerDirection +
                    " | Hedef renk: " + targetColor +
                    " | Delay: " + delay + " sn"
                );
            }

            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            ResetPhaseNow(playerDirection, targetColor);

            delayedResetCoroutine = null;
        }

        private void ResetPhaseNow(TrafficDirection playerDirection, LightColor targetColor)
        {
            int newStatus = GetStatusForDirectionAndColor(playerDirection, targetColor);

            status = newStatus;
            countTime = 0;

            if (targetColor == LightColor.Yellow)
            {
                step = 1;
            }
            else
            {
                step = 0;
            }

            EnabledObjects(status);

            if (debugLogs)
            {
                Debug.Log(
                    "[TrafficLights2] Faz resetlendi. Oyuncu yönü: " + playerDirection +
                    " | Hedef renk: " + targetColor +
                    " | Yeni status: " + status +
                    " | step: " + step +
                    " | countTime sıfırlandı."
                );
            }
        }

        private int GetStatusForDirectionAndColor(TrafficDirection playerDirection, LightColor targetColor)
        {
            bool isNorthSouth =
                playerDirection == TrafficDirection.North ||
                playerDirection == TrafficDirection.South;

            bool isEastWest =
                playerDirection == TrafficDirection.East ||
                playerDirection == TrafficDirection.West;

            if (isNorthSouth)
            {
                if (targetColor == LightColor.Red)
                    return 13; // N/S kırmızı, E/W yeşil

                if (targetColor == LightColor.Yellow)
                    return 21; // N/S sarı, E/W kırmızı

                if (targetColor == LightColor.Green)
                    return 31; // N/S yeşil, E/W kırmızı
            }

            if (isEastWest)
            {
                if (targetColor == LightColor.Red)
                    return 31; // E/W kırmızı, N/S yeşil

                if (targetColor == LightColor.Yellow)
                    return 12; // E/W sarı, N/S kırmızı

                if (targetColor == LightColor.Green)
                    return 13; // E/W yeşil, N/S kırmızı
            }

            return status;
        }
    }
}