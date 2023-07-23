using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using HarmonyLib;
using Enhancements.Enhancements;
using Newtonsoft.Json;
using UnityEngine;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;

namespace Enhancements
{
    public class ModEntry : MelonMod
    {
        GameObject accuracyGO;
        Accuracy accuracy;



        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();
            ClassInjector.RegisterTypeInIl2Cpp<Accuracy>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (accuracyGO == null && Time.time > 5f)
            {
                accuracyGO = new GameObject();
                accuracyGO.name = "AccuracyDisplayManager";
                accuracy = accuracyGO.AddComponent<Accuracy>();
            }
        }
    }


    public class Config
    {
        public Clock.Config clockConfig;
        public Accuracy.Config accuracyConfig;
    }

}
