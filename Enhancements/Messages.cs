using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Enhancements
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
