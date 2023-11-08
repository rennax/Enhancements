using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppTMPro;
using UnityEngine;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;
using static Il2Cpp.Messages;
using static Enhancements.Transport.Messages;
using static Enhancements.Enhancements.Accuracy;
using static Enhancements.Enhancements.Clock;
using UnityEngine.UI;
using System.Reflection.Emit;
using Il2CppInterop.Runtime.Injection;

namespace Enhancements.Enhancements
{
    [RegisterTypeInIl2Cpp]
    public class Accuracy : MonoBehaviour
    {
        public Accuracy(IntPtr ptr) : base(ptr) { }

        // Optional, only used in cases where you want to instantiate this class in the mono-side
        // Don't use this on MonoBehaviours / Components!
        public Accuracy() : base(ClassInjector.DerivedConstructorPointer<Accuracy>()) => ClassInjector.DerivedConstructorBody(this);


        private float onBeatHits = 0;
        private int totalHits = 0;
        private float beatAccuracy = 0;

        //Used to find the gun UI
        private const string pointerPath = "OpenXR_VRCameraRig(Clone)/TrackingSpace/Right Hand/Pointer";
        private const int gunIndex = 0;
        private const string gunUIPath = "Pivot/MenuParent/LeftGunMenu/Pivot";
        private const string nameOfPrefab = "";

        //Scene graph
        Transform pointer;
        Transform gun;
        Transform uiParent;
        GameObject uiPrefab;

        //UI
        GameObject accuracyContainer, onbeatContainer;
        TMP_Text accuracyValue, accuracyLabel;
        TMP_Text onbeatValue, onbeatLabel;

        private Image accuracyImage;
        private Image onbeatImage;
        private Sprite accuracyIcon;
        private Sprite onbeatIcon;

        PlayerActionManager actionManager;
        

        private void Awake()
        {
            var messenger = Transport.Messenger.Default;

            Messenger.Default.Register<PlayerHit>(new Action<PlayerHit>(OnPlayerHit));
            Messenger.Default.Register<EnemyHitEvent>(new Action<EnemyHitEvent>(OnEnemyHit));
            Messenger.Default.Register<EnemyKillEvent>(new Action<EnemyKillEvent>(OnEnemyKill));
            Messenger.Default.Register<GameStartEvent>(new Action<GameStartEvent>(OnGameStart));
            Messenger.Default.Register<GunsChanged>(new Action<GunsChanged>(OnGunChanged));
        }

        private void OnGunChanged(GunsChanged obj)
        {
            Destroy(accuracyContainer);
            accuracyContainer = null;
            
            Destroy(onbeatContainer);
            onbeatContainer = null;

            gun = null;
            uiParent = null;
            uiPrefab = null;
        }

        private void OnDestroy()
        {
        }

        //We use update to initialize fields since many of the objects are not available at start.
        //Actual updating of ui is performed in late update
        private void Update()
        {
            //Use guarding to ensure objects are instantiated
            if (actionManager == null)
            {
                GameObject tmp = GameObject.Find("/Managers/PlayerActionManager");
                if (tmp != null)
                    actionManager = tmp.GetComponent<PlayerActionManager>();
                return;
            }

            if (pointer == null)
            {
                pointer = GameObject.Find(pointerPath).transform;
                MelonLogger.Msg("Found pointer");
                return;
            }

            if (gun == null)
            {
                if (pointer.transform.childCount > 0)
                {
                    gun = pointer.GetChild(gunIndex);
                    MelonLogger.Msg("Found gun");
                }
                return;
            }

            if (uiParent == null)
            {
                uiParent = gun.Find(gunUIPath);
                MelonLogger.Msg("Found UI Parent");
                return;
            }

            if (uiPrefab == null)
            {
                uiPrefab = uiParent.Find("MultiplierDisplay").gameObject;
                MelonLogger.Msg("Found UI prefab");
                return;
            }

            if (accuracyContainer == null)
            {
                accuracyContainer = Instantiate(uiPrefab, uiParent.transform);
                Destroy(accuracyContainer.GetComponent<IntDisplay>()); //We dont use the int display


                accuracyContainer.transform.localPosition = new Vector3(-0.0375f, 0.030f, 0);
                accuracyContainer.transform.Find("Square").gameObject.SetActive(false);

                accuracyValue = accuracyContainer.transform.Find("Value").GetComponent<TMP_Text>();
                accuracyValue.transform.localPosition = new Vector3(0.083f, 0, 0);
                RectTransform rect = accuracyValue.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(0.9f, rect.sizeDelta.y);
                accuracyValue.fontSize = 2.5f;
                accuracyValue.alignment = TextAlignmentOptions.MidlineLeft;

                accuracyLabel = accuracyContainer.transform.Find("Label").GetComponent<TMP_Text>();

                accuracyLabel.text = "Acc:  ";
                MelonLogger.Msg("Instantiated accuary ui");
                return;
            }

            if (onbeatContainer == null)
            {
                onbeatContainer = Instantiate(uiPrefab, uiParent.transform);
                Destroy(onbeatContainer.GetComponent<IntDisplay>()); //We dont use the int display



                onbeatContainer.transform.localPosition = new Vector3(-0.0375f, 0.005f, 0);
                onbeatContainer.transform.Find("Square").gameObject.SetActive(false);

                onbeatValue = onbeatContainer.transform.Find("Value").GetComponent<TMP_Text>();
                onbeatValue.transform.localPosition = new Vector3(0.08f, 0, 0);
                RectTransform rect = onbeatValue.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(0.9f, rect.sizeDelta.y);
                onbeatValue.fontSize = 2.5f;
                onbeatValue.alignment = TextAlignmentOptions.MidlineLeft;


                onbeatLabel = onbeatContainer.transform.Find("Label").GetComponent<TMP_Text>();

                onbeatLabel.text = "Beat: ";
                MelonLogger.Msg("Instantiated onbeat ui");
                return;
            }

            //TODO find icons and replace label

        }

        private void LateUpdate()
        {
            if (actionManager == null || uiParent == null || uiPrefab == null ||
                onbeatContainer == null || accuracyContainer == null ||
                onbeatLabel == null || onbeatValue==null || accuracyValue == null || accuracyLabel == null)
                return;


            if (GameManager.Instance.playing)
            {
                GameData data = actionManager.gameData;
                UpdateUI(data.accuracy, data.onBeat, totalHits, data.timesPlayerHit);
            }
            else
            {
                GameData data = ScoreManager.playerData;
                if (data == null) // is null when you initially launch the game
                    UpdateUI(1, 1, 0, 0);
                else
                    UpdateUI(data.accuracy, data.onBeat, totalHits, data.timesPlayerHit);
            }

        }


        private void OnGameStart(GameStartEvent obj)
        {
            Reset();
        }

        private void OnEnemyKill(EnemyKillEvent obj)
        {
            try
            {
                UpdateHitData(obj.hit);
            }
            catch (Exception e)
            {
                MelonLogger.Error(e.Message);
                MelonLogger.Error(e.StackTrace);
            }
        }

        private void OnEnemyHit(EnemyHitEvent obj)
        {
            try
            {
                UpdateHitData(obj.hit);
            }
            catch (Exception e)
            {
                MelonLogger.Error(e.Message);
                MelonLogger.Error(e.StackTrace);
            }
        }

        private void OnUpdateEvent()
        {
            GameData playerData = actionManager.gameData; // for accuracy only
            //UpdateUI(playerData.accuracy, beatAccuracy, totalHits, playerData.timesPlayerHit);

            Transport.Messenger.Default.Send(new Transport.Messages.UpdateHits { Hits = totalHits });
            Transport.Messenger.Default.Send(new Transport.Messages.UpdatedInternalScore { OnBeatAccuracy = beatAccuracy, Accuracy = playerData.accuracy, Score = playerData.score });
        }

        private void OnPlayerHit(Messages.PlayerHit obj)
        {
            GameData playerData = actionManager.gameData;
            Transport.Messenger.Default.Send(new Transport.Messages.UpdateHitsTaken() { HitsTaken = playerData.timesPlayerHit });
            //UpdateUI(playerData.accuracy, beatAccuracy, totalHits, playerData.timesPlayerHit);
        }

        private void UpdateHitData(HitData hit)
        {
            totalHits += hit.severity; //Severity is aparently number of 100s so if boomstick kills chucknorris guy (800 total) severity is 4

            onBeatHits += hit.howOnBeat * (float)hit.severity; //howOnBeat is now a float and we can use that to cover both normal and rythmic mods

            if (totalHits > 0)
            {
                beatAccuracy = onBeatHits / (float)totalHits;
            }

            OnUpdateEvent();
        }

        private void UpdateUI(float accuracy, float onBeat, int hits, int hitsTaken)
        {
            accuracyValue.text = $"{accuracy*100f:0.0}%";
            onbeatValue.text = $"{onBeat*100f:0.0}%";
        }

        private void Reset()
        {
            onBeatHits = 0;
            totalHits = 0;
            beatAccuracy = 0;
            Transport.Messenger.Default.Send(new Transport.Messages.UpdateHitsTaken() { HitsTaken = 0 });
            Transport.Messenger.Default.Send(new Transport.Messages.UpdateHits() { Hits = 0 });
            //Acc and OnBeat are 1 to show 100% similar to beat saber when game starts
            Transport.Messenger.Default.Send(new Transport.Messages.UpdatedInternalScore { OnBeatAccuracy = 1, Accuracy = 1, Score = 0 });

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
        }
        }
}
