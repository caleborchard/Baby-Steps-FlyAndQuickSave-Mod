using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime;
using UnityEngine;

[assembly: MelonInfo(typeof(BabyStepsQuickSave.Core), "FlyAndQuickSave", "1.0.0", "Caleb Orchard", null)]
[assembly: MelonGame("DefaultCompany", "BabySteps")]

namespace BabyStepsQuickSave
{
    public class Core : MelonMod
    {
        DevCheatChaperone dCC;
        DevCheatsGoHere dCGH;
        PlayerMovement pm;
        public override void OnInitializeMelon()
        {
            GameObject go = new GameObject("DevCheatChaperone_Mod");
            GameObject dudest = GameObject.Find("Dudest");

            dCC = go.AddComponent<DevCheatChaperone>();
            dCGH = go.AddComponent<DevCheatsGoHere>();
            pm = dudest.GetComponent<PlayerMovement>();
        }
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                dCGH.SaveNatePos();
            }
            else if (Input.GetKeyDown(KeyCode.F10))
            {
                dCGH.LoadNatePos();
            }
            else if (Input.GetKeyDown(KeyCode.F1))
            {
                if (pm == null)
                {
                    GameObject dudest = GameObject.Find("Dudest");
                    pm = dudest.GetComponent<PlayerMovement>();
                }
                pm.ToggleFlyCam();
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0 && pm != null)
            {
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    if (scroll > 0f) pm.IncreaseFlyCamFOV();
                    else pm.DecreaseFlyCamFOV();
                }
                else
                {
                    if (scroll > 0f) pm.IncreadFlyCamSpd();
                    else pm.DecreaseFlyCamSpd();
                }
            }
        }
    }
}