using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime;
using UnityEngine;

[assembly: MelonInfo(typeof(BabyStepsQuickSave.Core), "BabyStepsQuickSave", "1.0.0", "Caleb Orchard", null)]
[assembly: MelonGame("DefaultCompany", "BabySteps")]

namespace BabyStepsQuickSave
{
    public class Core : MelonMod
    {
        DevCheatChaperone dCC;
        DevCheatsGoHere dCGH;
        public override void OnInitializeMelon()
        {
            GameObject go = new GameObject("DevCheatChaperone_Mod");
            dCC = go.AddComponent<DevCheatChaperone>();
            dCGH = go.AddComponent<DevCheatsGoHere>();

            dCC.
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
        }
    }
}