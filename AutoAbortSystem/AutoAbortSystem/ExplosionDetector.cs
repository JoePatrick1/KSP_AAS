using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoAbortSystem
{
    class ExplosionDetector : MonoBehaviour
    {

        public ModuleAAS vesselAAS;
        public Part partModule;
        public bool triggerEnabled;
        public bool highlightPart;

        public void OnDestroy()
        {
            if (partModule.vessel == vesselAAS.vessel && triggerEnabled)
            {
                vesselAAS.explosiveAbortTrigger = partModule;
                vesselAAS.explosionDetected();
            }
        }
    }

    

}
