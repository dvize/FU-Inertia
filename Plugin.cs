using System;
using System.Diagnostics;
using System.Reflection;
using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using HarmonyLib;
using RootMotion.FinalIK;
using UnityEngine;
using VersionChecker;

namespace dvize.FUInertia
{
    [BepInPlugin("com.dvize.FUInertia", "dvize.FUInertia", "2.1.0")]
    [BepInDependency("com.spt-aki.core", "3.6.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> tiltSpeed;
        public static ConfigEntry<float> tiltSensitivity;
        public static ConfigEntry<float> bodygravity;
        public static ConfigEntry<float> effectorlinkweight;

        void Awake()
        {
            CheckEftVersion();

            tiltSpeed = Config.Bind(
               "Final IK",
               "tilt Speed (peek)",
               1000f,
               "FinalIK - Default Settings: 6");

            tiltSensitivity = Config.Bind(
                "Final IK",
                "tilt Sensitivity (peek)",
                10f,
                "FinalIK - Default Settings: 0.07");

            bodygravity = Config.Bind(
                "Final IK",
                "body gravity (body part gravity)",
                0f,
                "FinalIK - Default Settings: None");

            new inertiaOnWeightUpdatedPatch().Enable();
            new SprintAccelerationPatch().Enable();
            new ManualAnimatorPatchInertia().Enable();
            //new FinalIKPatchBodyTilt().Enable();
            //new FinalIKPatch().Enable();
            new UpdateWeightLimitsPatch().Enable();
        }

        Player player;
        RootMotion.FinalIK.Inertia inertiaIK;
        RootMotion.FinalIK.BodyTilt bodytilt;
        private void Update()
        {
            try
            {
                if (Singleton<AbstractGame>.Instance.InRaid && Camera.main.transform.position != null)
                {
                    if (player == null)
                    {
                        player = Singleton<GameWorld>.Instance.MainPlayer;
                    }

                    if (inertiaIK == null)
                    {
                        inertiaIK = Singleton<Inertia>.Instance;
                    }

                    if (bodytilt == null)
                    {
                        bodytilt = Singleton<BodyTilt>.Instance;
                    }

                    bodytilt.tiltSpeed = Plugin.tiltSpeed.Value;
                    bodytilt.tiltSensitivity = Plugin.tiltSensitivity.Value;

                    foreach (Inertia.Body body in inertiaIK.bodies)
                    {
                        body.transform.position = Vector3.zero;
                        body.transform.localPosition = Vector3.zero;
                        body.gravity = Plugin.bodygravity.Value;

                        foreach (Inertia.Body.EffectorLink effectorlink in body.effectorLinks)
                        {
                            effectorlink.weight = Plugin.effectorlinkweight.Value;
                        }
                    }


                }
            }
            catch { }


        }

        private void CheckEftVersion()
        {
            // Make sure the version of EFT being run is the correct version
            int currentVersion = FileVersionInfo.GetVersionInfo(BepInEx.Paths.ExecutablePath).FilePrivatePart;
            int buildVersion = TarkovVersion.BuildVersion;
            if (currentVersion != buildVersion)
            {
                Logger.LogError($"ERROR: This version of {Info.Metadata.Name} v{Info.Metadata.Version} was built for Tarkov {buildVersion}, but you are running {currentVersion}. Please download the correct plugin version.");
                EFT.UI.ConsoleScreen.LogError($"ERROR: This version of {Info.Metadata.Name} v{Info.Metadata.Version} was built for Tarkov {buildVersion}, but you are running {currentVersion}. Please download the correct plugin version.");
                throw new Exception($"Invalid EFT Version ({currentVersion} != {buildVersion})");
            }
        }

    }


    public class inertiaOnWeightUpdatedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass713), "OnWeightUpdated");
        }

        [PatchPrefix]
        private static bool Prefix(GClass713 __instance, bool ___bool_7)
        {
            Player player = Singleton<GameWorld>.Instance.MainPlayer;
            if (!___bool_7 && player.InteractablePlayer.IsYourPlayer)
            {
                BackendConfigSettingsClass.GClass1367 stamina = Singleton<BackendConfigSettingsClass>.Instance.Stamina;
                float num = player.InteractablePlayer.Skills.CarryingWeightRelativeModifier * player.InteractablePlayer.HealthController.CarryingWeightRelativeModifier;
                Vector2 vector = new Vector2(player.InteractablePlayer.HealthController.CarryingWeightAbsoluteModifier, player.InteractablePlayer.HealthController.CarryingWeightAbsoluteModifier);

                //inertia crap
                BackendConfigSettingsClass.GClass1371 inertia = Singleton<BackendConfigSettingsClass>.Instance.Inertia;
                inertia.SideTime = Vector2.zero;
                inertia.DiagonalTime = Vector2.zero;
                inertia.MinMovementAccelerationRangeRight = new Vector2(0f, 0f);
                inertia.MaxMovementAccelerationRangeRight = new Vector2(0f, 0f);
                inertia.AverageRotationFrameSpan = 1;
                inertia.SuddenChangesSmoothness = 0f;

                __instance.Inertia = 0f;
                __instance.MoveSideInertia = 0f;
                __instance.MoveDiagonalInertia = 0f;

                //Vector3 vector2 = new Vector3(inertia.InertiaLimitsStep * (float)player.InteractablePlayer.Skills.Strength.SummaryLevel, inertia.InertiaLimitsStep * (float)player.InteractablePlayer.Skills.Strength.SummaryLevel, 0f);
                __instance.BaseInertiaLimits = Vector3.zero;
                __instance.WalkOverweightLimits = stamina.WalkOverweightLimits * num + vector;
                __instance.BaseOverweightLimits = stamina.BaseOverweightLimits * num + vector;
                __instance.SprintOverweightLimits = stamina.SprintOverweightLimits * num + vector;
                __instance.WalkSpeedOverweightLimits = stamina.WalkSpeedOverweightLimits * num + vector;
                return false;
            }
            __instance.WalkOverweightLimits.Set(9000f, 10000f);
            __instance.BaseOverweightLimits.Set(9000f, 10000f);
            __instance.SprintOverweightLimits.Set(9000f, 10000f);
            __instance.WalkSpeedOverweightLimits.Set(9000f, 10000f);

            return false;
        }
    }

    public class UpdateWeightLimitsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass713), "UpdateWeightLimits");
        }

        [PatchPostfix]
        static void Postfix(GClass713 __instance)
        {
            // Set the Vector2 variables to zero. Something here causes strength to raise properly
            __instance.BaseInertiaLimits = Vector3.zero;
        }
    }

    public class SprintAccelerationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass1667), "SprintAcceleration");
        }

        [PatchPrefix]
        private static bool Prefix(GClass1667 __instance, float deltaTime, Player ___player_0, GClass765 ___gclass765_0)
        {

            bool inRaid = Singleton<AbstractGame>.Instance.InRaid;

            if (___player_0.IsYourPlayer && inRaid)
            {
                var sprintAcceleration = ___player_0.Physical.SprintAcceleration;

                float num = sprintAcceleration * deltaTime;
                float num2 = (___player_0.Physical.SprintSpeed * __instance.SprintingSpeed + 1f) * __instance.StateSprintSpeedLimit;

                float num3 = 1f;

                num2 = Mathf.Clamp(num2 * num3, 0.1f, num2);

                __instance.SprintSpeed = Mathf.Clamp(__instance.SprintSpeed + num * Mathf.Sign(num2 - __instance.SprintSpeed), 0.01f, num2);

                return false;

            }

            // return false to skip the original method
            return false;
        }
    }

    public class ManualAnimatorPatchInertia : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass1681), "ManualAnimatorMoveUpdate");
        }

        [PatchPostfix]
        static void Postfix(GClass1681 __instance, float deltaTime)
        {
            var float_1 = (float)AccessTools.Field(typeof(GClass1681), "float_1").GetValue(__instance);
            var float_2 = (float)AccessTools.Field(typeof(GClass1681), "float_2").GetValue(__instance);
            var bool_0 = (bool)AccessTools.Field(typeof(GClass1681), "bool_0").GetValue(__instance);
            var vector2_0 = (Vector2)AccessTools.Field(typeof(GClass1681), "vector2_0").GetValue(__instance);
            var MovementContext = AccessTools.Field(typeof(GClass1681), "MovementContext").GetValue(__instance) as GClass1664;

            if (float_1 > float_2)
            {
                bool_0 = true;
            }
            if (vector2_0.IsZero())
            {
                float_1 += deltaTime;

                //remove the smoothed character movement speed cap
                /*if (MovementContext.SmoothedCharacterMovementSpeed > 0.35f)
                {
                    MovementContext.SmoothedCharacterMovementSpeed = Mathf.Lerp(MovementContext.SmoothedCharacterMovementSpeed, 0.35f, Singleton<BackendConfigSettingsClass>.Instance.Inertia.SuddenChangesSmoothness * deltaTime);
                }*/
            }

            AccessTools.Method(typeof(GClass1681), "ProcessRotation").Invoke(__instance, new object[] { deltaTime });
            if (!bool_0)
            {
                AccessTools.Method(typeof(GClass1681), "ProcessAnimatorMovement").Invoke(__instance, new object[] { deltaTime });
            }
        }
    }

    public class FinalIKPatch : ModulePatch
    {
        static FieldInfo firstUpdateField = AccessTools.Field(typeof(Inertia.Body), "firstUpdate");
        static FieldInfo directionField = AccessTools.Field(typeof(Inertia.Body), "direction");
        static FieldInfo lazyPointField = AccessTools.Field(typeof(Inertia.Body), "lazyPoint");
        static FieldInfo deltaField = AccessTools.Field(typeof(Inertia.Body), "delta");
        static FieldInfo lastPositionField = AccessTools.Field(typeof(Inertia.Body), "lastPosition");
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(RootMotion.FinalIK.Inertia.Body), "Update");
        }

        [PatchPrefix]
        static bool Prefix(Inertia.Body __instance, IKSolverFullBodyBiped solver, float weight, float deltaTime)
        {
            if (__instance.transform == null)
            {
                return false;
            }

            // Reset firstUpdate to false
            firstUpdateField.SetValue(__instance, false);

            // Set direction to instantaneous movement
            directionField.SetValue(__instance, (__instance.transform.position - (Vector3)lazyPointField.GetValue(__instance)) / deltaTime);

            // Set lazyPoint to current position
            lazyPointField.SetValue(__instance, __instance.transform.position);

            // Set delta and lastPosition to zero
            deltaField.SetValue(__instance, Vector3.zero);
            lastPositionField.SetValue(__instance, Vector3.zero);

            foreach (Inertia.Body.EffectorLink effectorLink in __instance.effectorLinks)
            {
                solver.GetEffector(effectorLink.effector).positionOffset += ((Vector3)lazyPointField.GetValue(__instance) - __instance.transform.position) * effectorLink.weight * weight;
            }
            return false;
        }
    }

    


}