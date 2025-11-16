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

        // Fly cam control variables
        public static bool flyCamActive = false;
        private float flyCamSpeed = 10f;
        private float flyCamSpeedMultiplier = 1f;

        public override void OnInitializeMelon()
        {
            GameObject go = new GameObject("DevCheatChaperone_Mod");
            dCC = go.AddComponent<DevCheatChaperone>();
            dCGH = go.AddComponent<DevCheatsGoHere>();

            // Find Dudest on first update instead to avoid null reference

            // Apply Harmony patches
            var harmony = new HarmonyLib.Harmony("BabyStepsQuickSave.InputOverride");
            harmony.PatchAll();
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
                flyCamActive = !flyCamActive;
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                dCGH.TeleportToCutscene();
            }
            else if (Input.GetKey(KeyCode.F4))
            {
                dCGH.IncrementCutscene();
            }

            // Handle fly cam controls when active
            if (flyCamActive && pm != null && pm.flyCam != null)
            {
                HandleFlyCamMovement();
                HandleFlyCamScrollWheel();
            }
            else
            {
                // Handle scroll wheel normally when not in fly cam
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

        private void HandleFlyCamMovement()
        {
            // Calculate movement direction based on key inputs
            Vector3 moveDirection = Vector3.zero;

            // Forward/Backward (W/S)
            if (Input.GetKey(KeyCode.W))
                moveDirection += Vector3.forward;
            if (Input.GetKey(KeyCode.S))
                moveDirection += Vector3.back;

            // Left/Right (A/D)
            if (Input.GetKey(KeyCode.A))
                moveDirection += Vector3.left;
            if (Input.GetKey(KeyCode.D))
                moveDirection += Vector3.right;

            // Up/Down (E/Q)
            if (Input.GetKey(KeyCode.E))
                moveDirection += Vector3.up;
            if (Input.GetKey(KeyCode.Q))
                moveDirection += Vector3.down;

            // Normalize to prevent faster diagonal movement
            if (moveDirection.magnitude > 1f)
                moveDirection.Normalize();

            // Calculate speed boost based on modifier keys
            float speedBoost = 1f;
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (shiftHeld && ctrlHeld)
                speedBoost = 7f;
            else if (ctrlHeld)
                speedBoost = 5f;
            else if (shiftHeld)
                speedBoost = 2f;

            // Align movement to fly cam rotation
            Transform flyCamTransform = pm.flyCam.transform;
            Vector3 alignedMovement = flyCamTransform.TransformDirection(moveDirection);

            // Apply speed and move the camera
            Vector3 velocity = alignedMovement * Mathf.Abs(flyCamSpeed * flyCamSpeedMultiplier * speedBoost);
            flyCamTransform.position += velocity * Time.deltaTime;
        }

        private void HandleFlyCamScrollWheel()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    // FOV adjustment
                    if (scroll > 0f) pm.IncreaseFlyCamFOV();
                    else pm.DecreaseFlyCamFOV();
                }
                else
                {
                    // Speed adjustment (custom implementation)
                    if (scroll > 0f)
                        flyCamSpeedMultiplier = Mathf.Min(flyCamSpeedMultiplier * 1.2f, 10f);
                    else
                        flyCamSpeedMultiplier = Mathf.Max(flyCamSpeedMultiplier * 0.8f, 0.1f);
                }
            }
        }
    }

    // Block Input class methods when fly cam is active
    [HarmonyPatch(typeof(Input))]
    public static class InputPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetKey", typeof(KeyCode))]
        public static bool GetKeyPrefix(KeyCode key, ref bool __result)
        {
            if (!Core.flyCamActive) return true;

            // Allow fly cam control keys to pass through
            if (key == KeyCode.W || key == KeyCode.A || key == KeyCode.S ||
                key == KeyCode.D || key == KeyCode.Q || key == KeyCode.E ||
                key == KeyCode.LeftShift || key == KeyCode.RightShift ||
                key == KeyCode.LeftControl || key == KeyCode.RightControl ||
                key == KeyCode.LeftAlt || key == KeyCode.RightAlt ||
                key == KeyCode.F1 || key == KeyCode.F3 || key == KeyCode.F4 ||
                key == KeyCode.F9 || key == KeyCode.F10)
            {
                return true;
            }

            // Block all other keys
            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetKeyDown", typeof(KeyCode))]
        public static bool GetKeyDownPrefix(KeyCode key, ref bool __result)
        {
            if (!Core.flyCamActive) return true;

            // Allow fly cam toggle and control keys
            if (key == KeyCode.F1 || key == KeyCode.F3 || key == KeyCode.F4 ||
                key == KeyCode.F9 || key == KeyCode.F10)
            {
                return true;
            }

            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetAxis", typeof(string))]
        public static bool GetAxisPrefix(string axisName, ref float __result)
        {
            if (!Core.flyCamActive) return true;

            // Allow mouse movement axes and scroll wheel for fly cam
            if (axisName == "Mouse ScrollWheel" ||
                axisName == "Mouse X" ||
                axisName == "Mouse Y")
            {
                return true;
            }

            // Block all other axes
            __result = 0f;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetMouseButton", typeof(int))]
        public static bool GetMouseButtonPrefix(int button, ref bool __result)
        {
            if (!Core.flyCamActive) return true;

            // Allow left click (button 0)
            if (button == 0) return true;

            // Block all other mouse buttons
            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetMouseButtonDown", typeof(int))]
        public static bool GetMouseButtonDownPrefix(int button, ref bool __result)
        {
            if (!Core.flyCamActive) return true;

            // Allow left click (button 0)
            if (button == 0) return true;

            // Block all other mouse button downs
            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetMouseButtonUp", typeof(int))]
        public static bool GetMouseButtonUpPrefix(int button, ref bool __result)
        {
            if (!Core.flyCamActive) return true;

            // Allow left click (button 0)
            if (button == 0) return true;

            // Block all other mouse button ups
            __result = false;
            return false;
        }
    }
}