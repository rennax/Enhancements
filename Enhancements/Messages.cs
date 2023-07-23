using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2Cpp;

namespace Enhancements.Transport
{
    public class Messages
    {
        public class OnGunChange
        {
            public GameObject gunBase;
        }

        public class GameEnd
        {

        }

        public class GameStart
        {

        }

        public class ShowLeftHand
        {
            public bool show;
        }

        public class ShowRightHand
        {
            public bool show;
        }

        public class UpdateAccuracyDisplay
        {
            public ScoreItem scoreItem;
        }

        public class UpdatedInternalScore
        {
            public float OnBeatAccuracy { get; set; }
            public float Accuracy { get; set; }
            public int Score { get; set; }
        }

        public class UpdateHits
        {
            public int Hits { get; set; }
        }

        public class UpdateHitsTaken
        {
            public int HitsTaken { get; set; }
        }


        public class OnLateUpdate
        { }
        
        public class OnUpdate
        { }

        public class ConfigUpdated
        {
            public Config config;
        }

    }
}
