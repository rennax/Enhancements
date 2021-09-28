using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using MelonLoader;
using HarmonyLib;

namespace Enhancements.Enhancements
{
    public class Accuracy
    {
        private int onBeatHits = 0;
        private int totalHits = 0;
        private float beatAccuracy = 0;


        private Config config = null;

        private GameObject parentGO;
        private GameObject lookAtGO;
        private GameObject go;
        private RectTransform rect;

        private TextMeshPro text = null;

        private PlayerActionManager playerActionManager;

        public Accuracy(Config accuracyConfig)
        {
            if (accuracyConfig == null)
            {
                config = new Config();
                MelonLogger.Msg("Did not find config. Creating new config");
            }
            else
            {
                config = accuracyConfig;
                MelonLogger.Msg("Loaded accuracy config: \n" + config.ToString());
            }

            //We dont actually parent, we just want to know the location
            parentGO = GameObject.Find("/UnityXR_VRCameraRig(Clone)/TrackingSpace/Right Hand");
            lookAtGO = GameObject.Find("/UnityXR_VRCameraRig(Clone)/TrackingSpace/Head");
            go = new GameObject();
            go.transform.parent = parentGO.transform;

            text = go.AddComponent<TextMeshPro>();
            text.richText = true;
            text.fontSize = config.fontSize;
            text.color = config.color;

            rect = go.transform.GetComponent<RectTransform>();
            rect.sizeDelta = config.sizeDelta;
            rect.localRotation = new Quaternion(0, 180, 0, 0);
            rect.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            rect.localPosition = config.offset;

            playerActionManager = GameObject.Find("/Managers/PlayerActionManager").GetComponent<PlayerActionManager>();

            var messenger = Messenger.Default;
            messenger.Register<Messages.GameStart>(OnGameStart);
            messenger.Register<Messages.GameEnd>(OnGameEnd);
            //messenger.Register<Messages.UpdateAccuracyDisplay>(UpdateAccuracyDisplay);
            messenger.Register<Messages.ShowRightHand>(ShowRightHand);
            messenger.Register<Messages.OnLateUpdate>(OnLateUpdate);
            messenger.Register<Messages.ConfigUpdated>(OnConfigUpdate);
            messenger.Register<HitDetails>(OnHit);
            messenger.Register<UpdateEvent>(OnUpdateEvent);

            global::Messenger.Default.Register<global::Messages.PlayerHit>(new Action<global::Messages.PlayerHit>(OnPlayerHit));

            UpdateUI(1, 1, 0, 0);
        }

        ~Accuracy()
        {
            var messenger = Messenger.Default;

            messenger.UnRegister<Messages.GameStart>(OnGameStart);
            messenger.UnRegister<Messages.GameEnd>(OnGameEnd);
            messenger.UnRegister<Messages.ShowRightHand>(ShowRightHand);
            messenger.UnRegister<Messages.OnLateUpdate>(OnLateUpdate);
            messenger.UnRegister<Messages.ConfigUpdated>(OnConfigUpdate);
            messenger.UnRegister<HitDetails>(OnHit);
            messenger.UnRegister<UpdateEvent>(OnUpdateEvent);
        }

        private void OnConfigUpdate(Messages.ConfigUpdated msg)
        {
            MelonLogger.Msg("Accuracy Updated config");
            this.config = msg.config.accuracyConfig;
            rect.localPosition = config.offset;
        }

        private void OnGameStart()
        {
            Reset();

            UpdateUI(1, 1, 0, 0);
        }

        private void OnGameEnd()
        {
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

        [HarmonyPatch(typeof(GameplayDatabase), "UpdateScore")]
        public static class GameDBMod
        {
            public static void Postfix(HitDetails hitDetails, ScoreItem scoreItem, int hitCount, int multShift)
            {
                Messenger.Default.Send(hitDetails);
            }
        }

        [HarmonyPatch(typeof(GameData), "AddScore")]
        public static class GameData_AddScoreHook
        {
            public static void Postfix(ScoreItem score)
            {
                Messenger.Default.Send<UpdateEvent>(new UpdateEvent { });
            }
        }

        private void OnHit(HitDetails hitDetails)
        {
            totalHits += hitDetails.severity; //Severity is aparently number of 100s so if boomstick kills chucknorris guy (800 total) severity is 4
            if (hitDetails.howOnBeat == 1)
                onBeatHits += hitDetails.severity;
            
            if (totalHits > 0)
            {
                beatAccuracy = ((float)onBeatHits) / (float)totalHits;
            }
        }

        void OnUpdateEvent(UpdateEvent obj)
        {
            GameData playerData = playerActionManager.gameData; // for accuracy only
            UpdateUI(playerData.accuracy, beatAccuracy, totalHits, playerData.timesPlayerHit);

            Messenger.Default.Send(new Messages.UpdateHits() { Hits = totalHits });
            Messenger.Default.Send(new Messages.UpdatedInternalScore { OnBeatAccuracy = beatAccuracy, Accuracy = playerData.accuracy, Score = playerData.score });
        }

        void OnPlayerHit(global::Messages.PlayerHit obj)
        {
            GameData playerData = playerActionManager.gameData;
            Messenger.Default.Send(new Messages.UpdateHitsTaken() { HitsTaken = playerData.timesPlayerHit });
            UpdateUI(playerData.accuracy, beatAccuracy, totalHits, playerData.timesPlayerHit);
        }

        private void UpdateUI(float accuracy, float onBeat, int hits, int hitsTaken)
        {
            string richText = "";
            richText += $"<size=100%>{(accuracy * 100f):0.00}%<size=100%> <b>Acc</b>\n";
            richText += $"<size=100%>{(onBeat * 100f):0.00}%<size=100%> <b>Beat</b>";
            if (config.showHits)
            {
                richText += $"\n<size=100%>{hits}<size=100%> <b>Hits</b>";
            }
            if (config.showHitsTaken)
            {
                richText += $"\n<size=100%>{hitsTaken}<size=100%> <b>Hits Taken</b>";
            }

            text.SetText(richText);
        }

        private void Reset()
        {
            onBeatHits = 0;
            totalHits = 0;
            beatAccuracy = 0;
            Messenger.Default.Send(new Messages.UpdateHitsTaken() { HitsTaken = 0 });
            Messenger.Default.Send(new Messages.UpdateHits() { Hits = 0});
            //Acc and OnBeat are 1 to show 100% similar to beat saber when game starts
            Messenger.Default.Send(new Messages.UpdatedInternalScore { OnBeatAccuracy = 1, Accuracy = 1, Score = 0 });
        }

        internal class UpdateEvent
        {

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
                str += $"show hits: {showHits}\n";
                str += $"show hits taken: {showHits}\n";
                return str;
            }
        }
    }
}
