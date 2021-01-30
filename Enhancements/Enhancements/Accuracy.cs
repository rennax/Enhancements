using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using MelonLoader;
using Harmony;

namespace Enhancements.Enhancements
{
    public class Accuracy
    {
        private int onBeatHits = 0;
        private int totalHits = 0;

        private Config config = null;

        GameObject parentGO;
        GameObject lookAtGO;
        GameObject go;
        RectTransform rect;
        
        TextMeshPro text = null;

        PlayerActionManager playerActionManager;

        public Accuracy(Config accuracyConfig)
        {
            if (accuracyConfig == null)
                config = new Config();
            else
                config = accuracyConfig;
            MelonLogger.Log("Loaded accuracy config: \n" + config.ToString());

            //We dont actually parent, we just want to know the location
            parentGO = GameObject.Find("/UnityXR_VRCameraRig(Clone)/TrackingSpace/Right Hand");
            lookAtGO = GameObject.Find("/UnityXR_VRCameraRig(Clone)/TrackingSpace/Head");
            go = new GameObject();
            go.transform.parent = parentGO.transform;

            text = go.AddComponent<TextMeshPro>();
            text.richText = true;
            text.fontSize = config.fontSize;
            text.color = config.color;

            //Placeholder text until a level is launched
            string richText = ""; 
            richText += $"<size=100%>0.00%<size=100%> <b>Acc</b>\n";
            richText += $"<size=100%>0.00%<size=100%> <b>Beat</b>";
            text.SetText(richText);

            rect = go.transform.GetComponent<RectTransform>();
            rect.sizeDelta = config.sizeDelta;
            rect.localRotation = new Quaternion(0, 180, 0, 0);
            rect.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            rect.localPosition = config.offset;

            playerActionManager = GameObject.Find("/Managers/PlayerActionManager").GetComponent<PlayerActionManager>();

            var messenger = Messenger.Default;
            messenger.Register<Messages.GameStart>(OnGameStart);
            messenger.Register<Messages.GameEnd>(OnGameEnd);
            messenger.Register<Messages.UpdateAccuracyDisplay>(UpdateAccuracyDisplay);
            messenger.Register<Messages.ShowRightHand>(ShowRightHand);
            messenger.Register<Messages.OnLateUpdate>(OnLateUpdate);
            messenger.Register<Messages.ConfigUpdated>(OnConfigUpdate);
        }


        ~Accuracy()
        {
            //Messenger.Default.UnRegister<Messages.OnGunChange>(GunChanged);
            var messenger = Messenger.Default;

            messenger.UnRegister<Messages.GameStart>(OnGameStart);
            messenger.UnRegister<Messages.GameEnd>(OnGameEnd);
            messenger.UnRegister<Messages.UpdateAccuracyDisplay>(UpdateAccuracyDisplay);
            messenger.UnRegister<Messages.ShowRightHand>(ShowRightHand);
            messenger.UnRegister<Messages.OnLateUpdate>(OnLateUpdate);
            messenger.UnRegister<Messages.ConfigUpdated>(OnConfigUpdate);
        }

        private void OnConfigUpdate(Messages.ConfigUpdated msg)
        {
            MelonLogger.Log("Accuracy Updated config");
            this.config = msg.config.accuracyConfig;
            rect.localPosition = config.offset;
        }

        private void OnGameStart()
        {
            MelonLogger.Log("OnGameStart");
            onBeatHits = 0;
            totalHits = 0;
        }

        private void OnGameEnd()
        {
            MelonLogger.Log("OnGameEnd");
        }

        private void ShowRightHand(Messages.ShowRightHand msg)
        {
            go.SetActive(msg.show);
        }

        private void OnLateUpdate()
        {
            //Rotate towards the vr head.
            rect.transform.LookAt(lookAtGO.transform);
            rect.localRotation *= Quaternion.Euler(0, 180f, 0);
        }

        private void UpdateAccuracyDisplay(Messages.UpdateAccuracyDisplay msg)
        {
            //MelonLogger.Log("UpdateAccuracyDisplay called");
            ScoreItem score = msg.scoreItem;
            GameData playerData = playerActionManager.playerData;

            //Accounting for melee hits is easy or something..
            if (score.onBeatValue == 100)
            {
                onBeatHits += 1;
                totalHits += 1;
            }
            else if (score.onBeatValue == 200)
            {
                onBeatHits += 2;
                totalHits += 2;
            }
            else if (score.onBeatValue == 400)
            {
                onBeatHits += 4;
                totalHits += 4;
            }
            else
                totalHits++; // This is just normal pistol shot without onbeat value

            float beatAccuracy = 0;
            if (totalHits > 0)
            {
                beatAccuracy = ((float)onBeatHits) / (float)totalHits;
            }

            string richText = "";
            richText += $"<size=100%>{(playerData.accuracy * 100f):0.00}%<size=100%> <b>Acc</b>\n";
            richText += $"<size=100%>{(beatAccuracy * 100f):0.00}%<size=100%> <b>Beat</b>";
            if (config.showHits)
                richText += $"\n<size=100%>{playerData.hits}<size=100%> <b>Hits</b>";
            if (config.showHitsTaken)
                richText += $"\n<size=100%>{playerData.timesPlayerHit}<size=100%> <b>Hits Taken</b>";

            text.SetText(richText);
        }

        public class Config
        {
            public int fontSize;
            public Vector3 offset;
            public Vector2 sizeDelta;
            public Color32 color;
            public bool showHits;
            public bool showHitsTaken;


            public Config()
            {
                this.fontSize = 2;
                this.offset = new Vector3(0.0f, 0.1f, 0.05f);
                this.sizeDelta = new Vector2(1.4f, 0.5f);
                this.color = new Color32(242, 0, 0, 255);
                this.showHits = true;
                this.showHitsTaken = true;
            }

            public override string ToString()
            {
                string str = "\n";
                str += $"font size: {fontSize}\n";
                str += $"offset: ({offset.x}, {offset.y}, {offset.z})\n";
                str += $"size delta (window size): ({sizeDelta.x}, {sizeDelta.y})\n";
                str += $"font color: R={color.r}, G={color.g}, B={color.b}, A={color.a}\n";

                return str;
            }
        }
    }
}
