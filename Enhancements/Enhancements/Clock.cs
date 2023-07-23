using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2CppTMPro;
using Il2Cpp;

namespace Enhancements.Enhancements
{
    
    public class Clock
    {
        public class Config
        {
            public Config()
            {
                this.fontSize = 2;
                this.onLeftHand = false;
                this.onRightHand = true;
            }

            public int fontSize;
            public bool onRightHand;
            public bool onLeftHand;
        }

        GameObject gunHud;
        GameObject clockGO;
        TMP_Text text = null;
        DateTime currentTime;
        bool fired = false;

        public Config config;


        public void Gun_Fire_Postfix(Gun __instance)
        {
            if (fired == true && gunHud != null)
            {
                return;
            }

            gunHud = GameObject.Find("/UnityXR_VRCameraRig(Clone)/TrackingSpace/Right Hand/Pointer/").transform.GetChild(0).Find("Pivot").Find("MenuParent").Find("LeftGunMenu").Find("Pivot").gameObject;

            GameObject multiplierDisplay = gunHud.transform.Find("MultiplierDisplay").gameObject;

            clockGO = GameObject.Instantiate(multiplierDisplay);
            clockGO.name = "PW Clock";
            clockGO.transform.parent = gunHud.transform;
            UnityEngine.Object.Destroy(clockGO.GetComponent<IntDisplay>());
            UnityEngine.Object.Destroy(clockGO.transform.Find("Value").gameObject);

            text = clockGO.GetComponentInChildren<TMP_Text>();
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1.2f, 0.5f);
            text.richText = true;
            text.fontStyle = FontStyles.Normal;
            clockGO.transform.localPosition = new Vector3(-0.015f, 0.06f, 0);
            clockGO.transform.localRotation = new Quaternion(0, 180, 0, 0);


            currentTime = DateTime.Now;
            fired = true;
        }

        public void Gun_OnDisable_Postfix(Gun __instance)
        {
            gunHud = null;
            GameObject.Destroy(clockGO);
        }


        public void OnUpdate()
        {
            if (text != null)
            {
                DateTime time = DateTime.Now;
                if (currentTime != time)
                {
                    currentTime = time;
                    text.SetText($"{time:h:mm:ss} <size=+1><b>{time:tt}</b>");
                }
            }
        }
    }
}
