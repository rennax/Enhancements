using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using Harmony;
using Enhancements.Enhancements;
using Newtonsoft.Json;
using UnityEngine;

namespace Enhancements
{
    public class ModEntry : MelonMod
    {
        const string configPath = "Mods/Enhancement";
        const string configFile = "config.json";
        static Clock clock = null;
        static Accuracy accuracy = null;

        FileSystemWatcher watcher = new FileSystemWatcher();
        static Config config;
        static bool configUpdated = false;
        private object configLock = new object();

        static GameObject rightHand;

        public override void OnApplicationStart()
        {
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }

            if (!File.Exists(Path.Combine(configPath, configFile)))
            {
                config = new Config()
                {
                    accuracyConfig = new Accuracy.Config(),
                    clockConfig = new Clock.Config(),
                };
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented,
                    Converters = new List<JsonConverter>{ 
                        new Converters.Vector3Converter(),
                        new Converters.Vector2Converter()
                    }
                };
                File.WriteAllText(
                    Path.Combine(configPath, configFile), 
                    JsonConvert.SerializeObject(config, settings)
                    ); 
            }
            else
            {
                string jsonText = File.ReadAllText(Path.Combine(configPath, configFile));
                config = JsonConvert.DeserializeObject<Config>(
                    jsonText, 
                    new Converters.Vector3Converter(), 
                    new Converters.Vector2Converter());
            }

            // Hot reload of config
            watcher.Path = configPath;

            // Watch for changes LastWrite
            watcher.NotifyFilter = NotifyFilters.LastWrite;

            // Only watch the config file.
            watcher.Filter = configFile;
            // Add event handlers.
            watcher.Changed += OnChanged;
            // Begin watching.
            watcher.EnableRaisingEvents = true;

        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            string jsonText = File.ReadAllText(e.FullPath);
            lock (configLock)
            {
                config = JsonConvert.DeserializeObject<Config>(jsonText, new Converters.Vector3Converter());
                configUpdated = true;
            }
            MelonLogger.Log("Config file changed");
        }


        [HarmonyPatch(typeof(CHInputController), "Awake", new System.Type[0] { })]
        public static class CHInputControllerMod
        {
            static bool first = false;
            public static void Postfix(CHInputController __instance)
            {
                rightHand = GameObject.Find("/UnityXR_VRCameraRig(Clone)/TrackingSpace/Right Hand");
                if (first == false)
                {
                    accuracy = new Accuracy(config.accuracyConfig);
                    first = true;
                }
            }
        }


        [HarmonyPatch(typeof(GameData), "AddScore", new System.Type[1] { typeof(ScoreItem) })]
        public static class GameDataMod
        {
            public static void Prefix(Gun __instance, ScoreItem score)
            {
                //MelonLogger.Log(score.ToString());
                //MelonLogger.Log($"score.onBeatValue {score.onBeatValue}");
                var msg = new Messages.UpdateAccuracyDisplay()
                {
                    scoreItem = score
                };
                Messenger.Default.Send<Messages.UpdateAccuracyDisplay>(msg);
            }
        }

        [HarmonyPatch(typeof(PlayerActionManager), "OnGameEnd", new System.Type[0] {})]
        public static class PlayerActionManager_OnGameEnd_Mod
        {
            public static void Postfix(PlayerActionManager __instance)
            {
                Messenger.Default.Send<Messages.GameEnd>();
            }
        }

        [HarmonyPatch(typeof(PlayerActionManager), "OnGameStart", new System.Type[0] { })]
        public static class PlayerActionManager_OnGameStart_Mod
        {
            public static void Postfix(PlayerActionManager __instance)
            {
                Messenger.Default.Send<Messages.GameStart>();
            }
        }

        static bool showingRight = false;

        public override void OnUpdate()
        {
            lock (configLock)
            {
                if (configUpdated)
                {
                    var msg = new Messages.ConfigUpdated()
                    {
                        config = config,
                    };
                    Messenger.Default.Send<Messages.ConfigUpdated>(msg);
                    configUpdated = false;
                }
            }

            if (rightHand != null)
            {
                Vector3 rot = rightHand.transform.rotation.eulerAngles;
                if (rot.z > 75 && rot.z < 103 &&
                    (rot.x > 330 && rot.x <= 360) || (rot.x < 22 && rot.x >= 0))
                {
                    if (showingRight == false)
                    {
                        showingRight = true;
                        Messenger.Default.Send<Messages.ShowRightHand>(new Messages.ShowRightHand() { show = true });
                    }
                }
                else
                {
                    if (showingRight == true)
                    {
                        Messenger.Default.Send<Messages.ShowRightHand>(new Messages.ShowRightHand() { show = false });
                        showingRight = false;
                    }
                }
            }
        }

        public override void OnLateUpdate()
        {
            Messenger.Default.Send<Messages.OnLateUpdate>();
        }
    }


    public class Config
    {
        public Clock.Config clockConfig;
        public Accuracy.Config accuracyConfig;
    }

}
