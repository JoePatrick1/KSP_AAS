using System;
using System.Collections.Generic;
using UnityEngine;
using KSP;
using System.Text.RegularExpressions;

namespace AutoAbortSystem
{
    public class ModuleAAS : PartModule
    {
       
        List<ModuleAAS> AASmodules = new List<ModuleAAS>();
        List<Part> parts = new List<Part>();

        private Vector2 partsScroll = Vector2.zero;

        [KSPField(isPersistant = true)]
        private bool aborted = false;

        [KSPField(isPersistant = true)]
        private bool armed = false;
        [KSPField(isPersistant = true)]
        private bool explosiveTriggerEnabled = true;
        [KSPField(isPersistant = true)]
        private bool vertSpeedTriggerEnabled = false;
        [KSPField(isPersistant = true)]
        private bool gForceTriggerEnabled = false;

        [KSPField(isPersistant = true)]
        public string explosiveAbortTriggerName;

        public Part explosiveAbortTrigger;
        [KSPField(isPersistant = true)]
        public float vertSpeedTrigger = 40;
        [KSPField(isPersistant = true)]
        public float gForceTrigger = 6;
        [KSPField(isPersistant = true)]
        public string vertSpeedTriggerS = "40";
        [KSPField(isPersistant = true)]
        public string gForceTriggerS = "6";

        [KSPField(isPersistant = true)]
        public bool vertSpeedAbort;
        [KSPField(isPersistant = true)]
        public bool gForceAbort;
        [KSPField(isPersistant = true)]
        public double vertSpeed;
        [KSPField(isPersistant = true)]
        public double gForce;

        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                Events["toggleActive"].guiName = "Active: " + armed;
                Events["toggleActive"].active = !aborted;
                Actions["toggleAction"].active = !aborted;
                Actions["armAction"].active = !aborted;
                Actions["disarmAction"].active = !aborted;

                foreach (Part part in part.vessel.parts)
                {
                    if (!parts.Contains(part)) {
                        parts.Add(part);
                        if (part.gameObject.GetComponent<ModuleAAS>()) AASmodules.Add(part.GetComponent<ModuleAAS>());
                        if (!part.gameObject.GetComponent<ExplosionDetector>())
                        {
                            ExplosionDetector exDet = part.gameObject.AddComponent<ExplosionDetector>();
                            exDet.vesselAAS = this;
                            exDet.partModule = part;
                            exDet.triggerEnabled = true;
                            
                        }
                    }
                }

                if(vessel.verticalSpeed < -vertSpeedTrigger && armed)
                {
                    if (!aborted && vertSpeedTriggerEnabled)
                    {
                        Debug.Log("<color=red>ABORTING - AAS Negative Vertical Velocity Detected!</color> - " + vessel.verticalSpeed);
                        part.vessel.ActionGroups.SetGroup(KSPActionGroup.Abort, true);
                        aborted = true;
                        status = "Aborted! AAS Negative Vertical Velocity Detected";
                        vertSpeed = vessel.verticalSpeed;
                        vertSpeedAbort = true;
                    }
                }

                if (vessel.geeForce > gForceTrigger && armed)
                {
                    if (!aborted && gForceTriggerEnabled)
                    {
                        Debug.Log("<color=red>ABORTING - AAS High G-Force Detected!</color> - " + vessel.geeForce);
                        part.vessel.ActionGroups.SetGroup(KSPActionGroup.Abort, true);
                        aborted = true;
                        status = "Aborted! AAS High G-Force Detected";
                        gForce = vessel.geeForce;
                        gForceAbort = true;
                    }
                }
                

                
            }
            
        }

        private bool showGUI;
        private bool showExplosiveConfigureWindow;
        private bool showVertSpeedConfigureWindow;
        private bool showGForceConfigureWindow;

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUnfocused = false, guiName = "Toggle GUI")]
        public void toggleGUI()
        {
            showGUI = !showGUI;
        }

        private Rect mainWindowRect = new Rect(100, 100, 400, 155);
        private Rect explosionConfigureWindowRect = new Rect(100, 290, 400, 500);
        private Rect vertSpeedConfigureWindowRect = new Rect(100, 290, 280, 55);
        private Rect gForceConfigureWindowRect = new Rect(100, 290, 170, 55);
        public void OnGUI()
        {
            if (HighLogic.LoadedSceneIsFlight && vessel.isActiveVessel && showGUI && AASmodules.Count > 0 && this == AASmodules.ToArray()[0])
            {
                mainWindowRect = GUILayout.Window(616173, mainWindowRect, mainWindow, "Automatic Abort System", HighLogic.Skin.window);
                if (showExplosiveConfigureWindow)
                    explosionConfigureWindowRect = GUILayout.Window(616174, explosionConfigureWindowRect, explosionConfigureWindow, "Configure (Explosion)", HighLogic.Skin.window);
                if (showVertSpeedConfigureWindow)
                    vertSpeedConfigureWindowRect = GUILayout.Window(616175, vertSpeedConfigureWindowRect, vertSpeedConfigureWindow, "Configure (Negative Vertical Speed)", HighLogic.Skin.window);
                if (showGForceConfigureWindow)
                    gForceConfigureWindowRect = GUILayout.Window(616176, gForceConfigureWindowRect, gForceConfigureWindow, "Configure (G-Force)", HighLogic.Skin.window);
            }
            

        }

        [KSPField(isPersistant = true)]
        public string status = "Off";
        public void mainWindow(int windowID)
        {
            GUIStyle AAStoggle = new GUIStyle(HighLogic.Skin.toggle);
            AAStoggle.margin.right = 180;
            GUIStyle AASbutton = new GUIStyle(HighLogic.Skin.button);
            AASbutton.fixedHeight = 20;
            GUIStyle AAStitle = new GUIStyle(HighLogic.Skin.label);
            AAStitle.stretchWidth = true;
            AAStitle.alignment = TextAnchor.MiddleCenter;
            GUIStyle AASstatuslabel = new GUIStyle(HighLogic.Skin.label);
            AASstatuslabel.stretchWidth = true;
            AASstatuslabel.alignment = TextAnchor.MiddleRight;
            AASstatuslabel.margin.bottom = 0;
            GUIStyle AASstatusInfo = new GUIStyle();
            AASstatusInfo.stretchWidth = true;
            AASstatusInfo.alignment = TextAnchor.MiddleCenter;
            AASstatusInfo.margin.top = 0;
            AASstatusInfo.normal.textColor = Color.white;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Status: ", AASstatuslabel);
            GUILayout.Label(status);
            GUILayout.EndHorizontal();
            if (explosiveAbort)
                GUILayout.Label("Part: " + explosiveAbortTriggerName,AASstatusInfo);
            /*if (vertSpeedAbort)
                GUILayout.Label("Speed: " + (vertSpeed*-1).ToString("F1") + "m/s", AASstatusInfo);
            if (gForceAbort)
                GUILayout.Label("G-Force: " + gForce.ToString("F1") + "g", AASstatusInfo);*/

            GUILayout.BeginVertical(HighLogic.Skin.box);
            GUILayout.Label("Abort Triggers",AAStitle);
            GUILayout.BeginHorizontal();
            explosiveTriggerEnabled = GUILayout.Toggle(explosiveTriggerEnabled, "Explosion", AAStoggle);
            if (GUILayout.Button("Configure", AASbutton))
            {
                showExplosiveConfigureWindow = !showExplosiveConfigureWindow;
                if (showExplosiveConfigureWindow)
                {
                    showVertSpeedConfigureWindow = false;
                    showGForceConfigureWindow = false;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            gForceTriggerEnabled = GUILayout.Toggle(gForceTriggerEnabled, "High G-Force", AAStoggle);
            if (GUILayout.Button("Configure", AASbutton))
            {
                showGForceConfigureWindow = !showGForceConfigureWindow;
                if (showGForceConfigureWindow)
                {
                    showExplosiveConfigureWindow = false;
                    showVertSpeedConfigureWindow = false;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            vertSpeedTriggerEnabled = GUILayout.Toggle(vertSpeedTriggerEnabled, "Negative Vertical Velocity", AAStoggle);
            if (GUILayout.Button("Configure", AASbutton))
            {
                showVertSpeedConfigureWindow = !showVertSpeedConfigureWindow;
                if (showVertSpeedConfigureWindow)
                {
                    showExplosiveConfigureWindow = false;
                    showGForceConfigureWindow = false;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow();

        }

        Rect scrollRect = new Rect(0, 0, 0, 0);
        bool highlightCheck;
        bool mouseContained;

        public void explosionConfigureWindow(int windowID)
        {
            GUILayout.Label("The abort sequence will only be triggered if one of the selected parts explodes");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All", HighLogic.Skin.button))
            {
                foreach (Part part in parts)
                {
                    if (part && part.vessel == vessel)
                        part.gameObject.GetComponent<ExplosionDetector>().triggerEnabled = true;
                    
                }
            }
            if (GUILayout.Button("Deselect All", HighLogic.Skin.button))
            {
                foreach (Part part in parts)
                {
                    if (part && part.vessel == vessel)
                        part.gameObject.GetComponent<ExplosionDetector>().triggerEnabled = false;
                    
                }
            }
            GUILayout.EndHorizontal();

         
            GUI.skin = HighLogic.Skin;
            partsScroll = GUILayout.BeginScrollView(partsScroll);
            GUI.skin = null;
            GUILayout.BeginVertical();

            if (scrollRect.Contains(Event.current.mousePosition))
            {
                highlightCheck = false;
                mouseContained = true;
            }
            else if (!highlightCheck)
                highlightCheck = true;
            
            else
                mouseContained = false;
            

            foreach (Part part in parts)
            {
                if (part && part.vessel == vessel)
                {
                    part.gameObject.GetComponent<ExplosionDetector>().triggerEnabled = GUILayout.Toggle(part.gameObject.GetComponent<ExplosionDetector>().triggerEnabled, part.partInfo.title, HighLogic.Skin.toggle);
                    Rect hovercheck = GUILayoutUtility.GetLastRect();
                    hovercheck.width = 305;
                    


                    if (hovercheck.Contains(Event.current.mousePosition) && mouseContained)
                    {
                        part.SetHighlightColor(Color.cyan);
                        part.SetHighlight(true, false);
                        part.highlightType = Part.HighlightType.AlwaysOn;
                        part.gameObject.GetComponent<ExplosionDetector>().highlightPart = true;
                        
                    }
                    else if (part.gameObject.GetComponent<ExplosionDetector>().highlightPart == true)
                    {
                        part.SetHighlightColor(Part.defaultHighlightPart);
                        part.SetHighlight(false, false);
                        part.highlightType = Part.HighlightType.OnMouseOver;
                        part.gameObject.GetComponent<ExplosionDetector>().highlightPart = false;
                    }

                }
                
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            
            scrollRect = GUILayoutUtility.GetLastRect();
            scrollRect.y += partsScroll.y;

            
            GUI.DragWindow();
        }

        public void vertSpeedConfigureWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Negative Vertical Speed", HighLogic.Skin.label);

            string oldText = vertSpeedTriggerS;
            vertSpeedTriggerS = GUILayout.TextField(vertSpeedTriggerS, 8, HighLogic.Skin.textField);

            float temp;
            if (float.TryParse(vertSpeedTriggerS, out temp))
                vertSpeedTrigger = Mathf.Clamp(temp, 0, 99999999);
            else
            {
                if(vertSpeedTriggerS != "") vertSpeedTriggerS = oldText;
                else
                {
                    vertSpeedTrigger = 0;
                    vertSpeedTriggerS = "";
                }
            }
            
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        public void gForceConfigureWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max G-Force", HighLogic.Skin.label);

            string oldText = gForceTriggerS;
            gForceTriggerS = GUILayout.TextField(gForceTriggerS, 8, HighLogic.Skin.textField);

            float temp;
            if (float.TryParse(gForceTriggerS, out temp))
                gForceTrigger = Mathf.Clamp(temp, 0, 99999999);
            else
            {
                if (gForceTriggerS != "") gForceTriggerS = oldText;
                else
                {
                    gForceTrigger = 0;
                    gForceTriggerS = "";
                }
            }

            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        public void OnDestroy()
        {
            AASmodules.Remove(this);
        }

        [KSPField(isPersistant = true)]
        private bool explosiveAbort;
        public void explosionDetected()
        {
            if (!aborted && explosiveTriggerEnabled && armed)
            {
                Debug.Log("<color=red>ABORTING - AAS EXPLOSION DETECTED!</color> - " + explosiveAbortTrigger.partInfo.title);
                explosiveAbortTriggerName = explosiveAbortTrigger.partInfo.title;
                part.vessel.ActionGroups.SetGroup(KSPActionGroup.Abort, true);
                aborted = true;
                status = "Aborted! AAS Explosion Detected";
                explosiveAbort = true;
            }
        }

        [KSPAction("Arm")]
        public void armAction(KSPActionParam param)
        {
            armed = true;
            status = "Active";
        }

        [KSPAction("Disarm")]
        public void disarmAction(KSPActionParam param)
        {
            armed = false;
            status = "Off";
        }

        [KSPAction("Toggle")]
        public void toggleAction(KSPActionParam param)
        {
            toggleActive();
        }

        [KSPAction("Arm Explosion Detection")]
        public void armEDAction(KSPActionParam param)
        {
            explosiveTriggerEnabled = true;
        }

        [KSPAction("Disarm Explosion Detection")]
        public void disarmEDAction(KSPActionParam param)
        {
            explosiveTriggerEnabled = false;
        }

        [KSPAction("Toggle Explosion Detection")]
        public void toggleEDAction(KSPActionParam param)
        {
            explosiveTriggerEnabled = !explosiveTriggerEnabled;
        }

        [KSPAction("Arm High-G Detection")]
        public void armHGAction(KSPActionParam param)
        {
            gForceTriggerEnabled = true;
        }

        [KSPAction("Disarm High-G Detection")]
        public void disarmHGAction(KSPActionParam param)
        {
            gForceTriggerEnabled = false;
        }

        [KSPAction("Toggle High-G Detection")]
        public void toggleHGAction(KSPActionParam param)
        {
            gForceTriggerEnabled = !gForceTriggerEnabled;
        }

        [KSPAction("Arm Neg-Vert-Vel Detection")]
        public void armNVVDAction(KSPActionParam param)
        {
            vertSpeedTriggerEnabled = true;
        }

        [KSPAction("Disarm Neg-Vert-Veln Detection")]
        public void disarmNVVAction(KSPActionParam param)
        {
            vertSpeedTriggerEnabled = false;
        }

        [KSPAction("Toggle Neg-Vert-Vel Detection")]
        public void toggleNVVAction(KSPActionParam param)
        {
            vertSpeedTriggerEnabled = !vertSpeedTriggerEnabled;
        }

        [KSPAction("Toggle GUI")]
        public void toggleGUIAction(KSPActionParam param)
        {
            showGUI = !showGUI;
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUnfocused = false, guiName = "Active: ")]
        public void toggleActive()
        {
            armed = !armed;
            if (armed) status = "Active";
            else status = "Off";
        }

    }
}
