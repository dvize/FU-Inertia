using Aki.Reflection.Patching;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using HarmonyLib;
using RootMotion.FinalIK;
using UnityEngine;


namespace dvize.FUInertia
{
    [BepInPlugin("com.dvize.FUInertia", "dvize.FUInertia", "1.9.2")]

    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> PluginEnabled;
        public static ConfigEntry<Vector3> baseInertia;
        public static ConfigEntry<float> Inertia;
        public static ConfigEntry<float> MoveDiagonalInertia;
        public static ConfigEntry<Vector2> MoveTimeRange;
        public static ConfigEntry<Vector2> DiagonalTime;
        public static ConfigEntry<Vector2> DiagonalStayTimeRange;
        public static ConfigEntry<float> MoveSideInertia;
        public static ConfigEntry<Vector3> InertiaLimits;
        public static ConfigEntry<float> InertialLimitsStep;
        public static ConfigEntry<Vector2> SprintSpeedInertiaCurveMin;
        public static ConfigEntry<Vector2> SprintSpeedInertiaCurveMax;
        public static ConfigEntry<Vector2> SprintBrakeInertia;
        public static ConfigEntry<Vector2> SprintTransitionMotionPreservation;
        public static ConfigEntry<Vector2> SprintAccelerationLimits;
        public static ConfigEntry<Vector2> PreSprintAccelerationLimits;
        public static ConfigEntry<Vector2> WalkInertia;
        public static ConfigEntry<Vector2> SpeedInertiaAfterJump;
        public static ConfigEntry<Vector2> SideTime;
        public static ConfigEntry<Vector2> InertiaBackwardsCoefficient;
        public static ConfigEntry<float> StrafeInertionCoefficient;
        public static ConfigEntry<float> intertiaInputMaxSpeed;

        public static ConfigEntry<float> tiltSpeed;
        public static ConfigEntry<float> tiltSensitivity;
        public static ConfigEntry<float> tiltchangingspeed;
        public static ConfigEntry<float> bodygravity;
        public static ConfigEntry<float> effectorlinkweight;
        public static ConfigEntry<bool> RemoveSpeedLimitWeight;
        public static ConfigEntry<bool> RemoveSpeedLimitSurfaceNormal;
        public static ConfigEntry<bool> RemoveSpeedLimitArmor;
        public static ConfigEntry<bool> RemoveSpeedLimitAiming;

        void Awake()
        {
            PluginEnabled = Config.Bind(
                "Main Settings",
                "Plugin on/off",
                true,
                "");

            StrafeInertionCoefficient = Config.Bind(
                "Main Settings",
                "Strafe Inertion Coeffecient",
                0f,
                "EFT HardSettings - No Default Value");

            intertiaInputMaxSpeed = Config.Bind(
                "Main Settings",
                "Inertia Input Max Speed",
                0f,
                "EFT HardSettings - 100f default");

            tiltSpeed = Config.Bind(
                "Final IK",
                "tilt Speed (peek)",
                1200f,
                "FinalIK - Default Settings: 6");

            tiltSensitivity = Config.Bind(
                "Final IK",
                "tilt Sensitivity (peek)",
                1000f,
                "FinalIK - Default Settings: 0.07");

            bodygravity = Config.Bind(
                "Final IK",
                "body gravity (body part gravity)",
                0f,
                "FinalIK - Default Settings: None");

            effectorlinkweight = Config.Bind(
                "Final IK",
                "Effector Link Weight (Weight of using this effector)",
                0f,
                "FinalIK - Default Settings: None");

            tiltchangingspeed = Config.Bind(
                "Main Settings",
                "tilt changingspeed",
                1000f,
                "FinalIK - Default Settings: 10");

            RemoveSpeedLimitWeight = Config.Bind(
                "Limiters",
                "Remove Speed Limit Weight",
                true,
                "Default value is off for normal gameplay");

            RemoveSpeedLimitSurfaceNormal = Config.Bind(
                "Limiters",
                "Remove Speed Limit Surface Normal",
                true,
                "Default value is off for normal gameplay");

            RemoveSpeedLimitArmor = Config.Bind(
                "Limiters",
                "Remove Speed Limit Armor",
                true,
                "Default value is off for normal gameplay");

            RemoveSpeedLimitAiming = Config.Bind(
                "Limiters",
                "Remove Speed Limit Aiming",
                true,
                "Default value is off for normal gameplay");

            new inertiaOnWeightUpdatedPatch().Enable();
        }

        Player player;
        BackendConfigSettingsClass.GClass1328 backendsettings;
        EFTHardSettings settings;
        private void FixedUpdate()
        {
            if (Plugin.PluginEnabled.Value)
            {
                try
                {
                    if (Singleton<AbstractGame>.Instance.InRaid && Camera.main.transform.position != null)
                    {
                        /*player = Singleton<GameWorld>.Instance.MainPlayer;
                        player.Physical.Inertia = Plugin.Inertia.Value;
                        player.Physical.MoveDiagonalInertia = Plugin.MoveDiagonalInertia.Value;
                        player.Physical.MoveSideInertia = Plugin.MoveSideInertia.Value;
                        player.Physical.BaseInertiaLimits = Plugin.baseInertia.Value;*/

                        // Retrieve the value of the field for the EFTHardSettings.efthardSettings_0 object
                        /*settings = Singleton<EFTHardSettings>.Instance;

                        settings.StrafeInertionCoefficient = Plugin.StrafeInertionCoefficient.Value;
                        settings.InertiaInputMaxSpeed = Plugin.intertiaInputMaxSpeed.Value;
                        settings.TILT_CHANGING_SPEED = Plugin.tiltchangingspeed.Value;
                        settings.StrafeInertionCurve.Evaluate(0f);
                        settings.InertiaTiltCurve.Evaluate(0f);
                        settings.PoseInertiaDamp.Evaluate(0f);
                        settings.PoseInertiaOverFallDistance.Evaluate(0f);*/

                        //backend configs
                        
                        /*backendsettings = Singleton<BackendConfigSettingsClass>.Instance.Inertia;

                        backendsettings.ExitMovementStateSpeedThreshold = Vector2.zero;
                        backendsettings.WalkInertia = Vector2.zero;
                        backendsettings.FallThreshold = 0.0f;
                        backendsettings.SpeedLimitAfterFallMin = Vector2.zero;
                        backendsettings.SpeedLimitAfterFallMax = Vector2.zero;
                        backendsettings.SpeedLimitDurationMin = Vector2.zero;
                        backendsettings.SpeedLimitDurationMax = Vector2.zero;
                        backendsettings.SpeedInertiaAfterJump = Vector2.zero;
                        backendsettings.BaseJumpPenaltyDuration = 0.0f;
                        backendsettings.DurationPower = 0.0f;
                        backendsettings.BaseJumpPenalty = 0.0f;
                        backendsettings.PenaltyPower = 0.0f;
                        backendsettings.InertiaTiltCurveMin = Vector2.zero;
                        backendsettings.InertiaTiltCurveMax = Vector2.zero;
                        backendsettings.InertiaBackwardCoef = Vector2.zero;
                        backendsettings.TiltInertiaMaxSpeed = Vector2.zero;
                        backendsettings.TiltStartSideBackSpeed = Vector2.zero;
                        backendsettings.TiltMaxSideBackSpeed = Vector2.zero;
                        backendsettings.TiltAcceleration = Vector2.zero;
                        backendsettings.AverageRotationFrameSpan = 0;
                        backendsettings.SprintSpeedInertiaCurveMin = Vector2.zero;
                        backendsettings.SprintSpeedInertiaCurveMax = Vector2.zero;
                        backendsettings.SprintBrakeInertia = Vector2.zero;
                        backendsettings.SprintTransitionMotionPreservation = Vector2.zero;
                        backendsettings.InertiaLimits = Vector3.zero;
                        backendsettings.WeaponFlipSpeed = Vector2.zero;
                        backendsettings.InertiaLimitsStep = 0.0f;
                        backendsettings.SprintAccelerationLimits = Vector2.zero;
                        backendsettings.PreSprintAccelerationLimits = Vector2.zero;
                        backendsettings.SideTime = Vector2.zero;*/

                        //Rootmotion
                        /*RootMotion.FinalIK.Inertia inertiaIK = Singleton<Inertia>.Instance;
                        RootMotion.FinalIK.BodyTilt bodytilt = Singleton<BodyTilt>.Instance;


                        bodytilt.tiltSpeed = Plugin.tiltSpeed.Value;
                        bodytilt.tiltSensitivity = Plugin.tiltSensitivity.Value;


                        foreach (Inertia.Body body in inertiaIK.bodies)
                        {
                            body.transform.position = new Vector3(0f, 0f, 0f);
                            body.transform.localPosition = new Vector3(0f, 0f, 0f);
                            body.gravity = Plugin.bodygravity.Value;

                            foreach (Inertia.Body.EffectorLink effectorlink in body.effectorLinks)
                            {
                                effectorlink.weight = Plugin.effectorlinkweight.Value;
                            }
                        }*/



                        if (Plugin.RemoveSpeedLimitWeight.Value)
                        {
                            player.RemoveStateSpeedLimit(Player.ESpeedLimit.Weight);
                        }

                        if (Plugin.RemoveSpeedLimitSurfaceNormal.Value)
                        {
                            player.RemoveStateSpeedLimit(Player.ESpeedLimit.SurfaceNormal);
                        }

                        if (Plugin.RemoveSpeedLimitArmor.Value)
                        {
                            player.RemoveStateSpeedLimit(Player.ESpeedLimit.Armor);
                        }

                        if (Plugin.RemoveSpeedLimitAiming.Value)
                        {
                            player.RemoveStateSpeedLimit(Player.ESpeedLimit.Aiming);
                        }

                    }


                }
                catch
                {

                }


            }

        }

    }


    public class inertiaOnWeightUpdatedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            try
            {
                //this isn't a general method.. looks like this engrained in every bot so i have to find the specific bot.
                return typeof(GClass703).GetMethod("OnWeightUpdated", BindingFlags.Instance | BindingFlags.Public);
            }
            catch
            {
                Logger.LogInfo("FUInertia: Failed to get target method");
            }

            return null;
        }

        [PatchPostfix]
        public static void Postfix(GClass703 __instance)
        {
            BackendConfigSettingsClass.GClass1328 inertia = Singleton<BackendConfigSettingsClass>.Instance.Inertia;
            __instance.Inertia = 0f;
            inertia.MinMovementAccelerationRangeRight = new Vector2(0f, 0f);
            inertia.MaxMovementAccelerationRangeRight = new Vector2(0f, 0f);
            EFTHardSettings.Instance.MovementAccelerationRange.MoveKey(1, new Keyframe(0f, 1f));
            inertia.SideTime = new Vector2(0f, 0f);
            inertia.DiagonalTime = new Vector2 (0f, 0f);

            __instance.MoveSideInertia = 0f;
            __instance.MoveDiagonalInertia = 0f;
            __instance.MinStepSound.SetDirty();
            __instance.TransitionSpeed.SetDirty();
            typeof(GClass703).GetMethod("method_3", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
            typeof(GClass703).GetMethod("method_7", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { 0f });
        }
    }

}