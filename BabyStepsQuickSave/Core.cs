using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(BabyStepsQuickSave.Core), "FlyAndQuickSave", "2.0.0", "Caleb Orchard; Edit by Fynnoverse", null)]
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
            var go = new GameObject("DevCheatChaperone_Mod");
            dCC = go.AddComponent<DevCheatChaperone>();
            dCGH = go.AddComponent<DevCheatsGoHere>();

            // Find Dudest on first update instead to avoid null reference
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
                    var dudest = GameObject.Find("Dudest");
                    if (dudest == null)
                    {
                        MelonLogger.Warning("[FlyAndQuickSave] 'Dudest' GameObject nicht gefunden! Bitte erst ins Spiel laden.");
                        return;
                    }
                    pm = dudest.GetComponent<PlayerMovement>();
                    if (pm == null)
                    {
                        MelonLogger.Warning("[FlyAndQuickSave] PlayerMovement-Komponente nicht gefunden!");
                        return;
                    }
                }
                pm.ToggleFlyCam();
                flyCamActive = !flyCamActive;
                MelonLogger.Msg("[FlyAndQuickSave] FlyCam " + (flyCamActive ? "aktiviert" : "deaktiviert"));
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
                var scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll == 0 || pm == null) return;
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

        private void HandleFlyCamMovement()
        {
            // Calculate movement direction based on key inputs
            var moveDirection = Vector3.zero;

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
            var speedBoost = 1f;
            var shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (shiftHeld && ctrlHeld)
                speedBoost = 7f;
            else if (ctrlHeld)
                speedBoost = 5f;
            else if (shiftHeld)
                speedBoost = 2f;

            // Align movement to fly cam rotation
            var flyCamTransform = pm.flyCam.transform;
            var alignedMovement = flyCamTransform.TransformDirection(moveDirection);

            // Apply speed and move the camera
            var velocity = alignedMovement * Mathf.Abs(flyCamSpeed * flyCamSpeedMultiplier * speedBoost);
            flyCamTransform.position += velocity * Time.deltaTime;
        }

        private void HandleFlyCamScrollWheel()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll == 0) return;
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                // FOV adjustment
                if (scroll > 0f) pm.IncreaseFlyCamFOV();
                else pm.DecreaseFlyCamFOV();
            }
            else
            {
                // Speed adjustment (custom implementation)
                flyCamSpeedMultiplier = scroll > 0f ? Mathf.Min(flyCamSpeedMultiplier * 1.2f, 10f) : Mathf.Max(flyCamSpeedMultiplier * 0.8f, 0.1f);
            }
        }
    }
}