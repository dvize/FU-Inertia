using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using EFT.UI.Health;
using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;


namespace dvize.BulletTime
{
    [BepInPlugin("com.dvize.FUInertia", "dvize.FUInertia", "1.5.0")]

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
        public static ConfigEntry<float>   InertialLimitsStep;
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
        public static ConfigEntry<float> DurationPower;
        public static ConfigEntry<float> PenaltyPower;
        public static ConfigEntry<float> BaseJumpPenalty;
        public static ConfigEntry<float> BaseJumpPenaltyDuration;
        public static ConfigEntry<Vector2> TiltStartSideBackSpeed;
        public static ConfigEntry<Vector2> TiltMaxSideBackSpeed;
        public static ConfigEntry<Vector2> TiltAcceleration;
        public static ConfigEntry<Vector2> InertiaTiltCurveMin;
        public static ConfigEntry<Vector2> InertiaTiltCurveMax;
        public static ConfigEntry<Vector2> TiltInertiaMaxSpeed;
        public static ConfigEntry<Vector2> ExitMovementStateSpeedThreshold;
        public static ConfigEntry<Vector2> SpeedLimitDurationMin;
        public static ConfigEntry<Vector2> SpeedLimitDurationMax;
        public static ConfigEntry<float> MinDirectionBlendTime;
        public static ConfigEntry<float> SuddenChangesSmoothness;
        public static ConfigEntry<Vector2> MaxTimeWithoutInput;
        public static ConfigEntry<Vector2> ProneDirectionAccelerationRange;
        public static ConfigEntry<Vector2> ProneSpeedAccelerationRange;
        public static ConfigEntry<Vector2> CrouchSpeedAccelerationRange;
        public static ConfigEntry<Vector2> WeaponFlipSpeed;
        public static ConfigEntry<float> tiltSpeed;
        public static ConfigEntry<float> tiltSensitivity;
        public static ConfigEntry<float> tiltchangingspeed;
        public static ConfigEntry<float> bodygravity;
        public static ConfigEntry<float> effectorlinkweight;
        

        async void Awake()
        {
            PluginEnabled = Config.Bind(
                "Main Settings",
                "Plugin on/off",
                true,
                "");

            baseInertia = Config.Bind(
                "Main Settings",
                "Physical:Base Inertia Limits",
                Vector3.zero,
                "no default value");

            Inertia = Config.Bind(
                "Main Settings",
                "Physical: Inertia",
                0f,
                "No default value");

            MoveDiagonalInertia = Config.Bind(
                "Main Settings",
                "Physical: Move Diagonal Inertia",
                0f,
                "No default value");

            DiagonalTime = Config.Bind(
                "Inertia Class",
                "Diagonal Time",
                Vector2.zero,
                "Inertia Class - Default Settings: (1.5, 1.5)");

            DiagonalStayTimeRange = Config.Bind(
                "Inertia Class",
                "Diagonal Time",
                Vector2.zero,
                "Inertia Class - Default Settings: (0.1, 0.2)");

            MoveSideInertia = Config.Bind(
                "Main Settings",
                "Physical: Move Side Inertia",
                0f,
                "No default value");

            MoveTimeRange = Config.Bind(
                "Inertia Class",
                "Move Time Range",
                Vector2.zero,
                "Inertia Class - Default Settings: (0.15, 0.5)");

            InertiaLimits = Config.Bind(
                "Main Settings",
                "Inertial Limits",
                Vector3.zero,
                "Inertia Class - Default Settings: (0, 65, 1)");

            InertialLimitsStep = Config.Bind(
                "Inertia Class",
                "Inertial Limits Step",
                0f,
                "Inertia Class - Default Settings: 0.3");

            SprintSpeedInertiaCurveMin = Config.Bind(
                "Inertia Class",
                "Sprint Speed Inertia Curve Min",
                Vector2.zero,
                "Inertia Class - Default Settings: (0, 1)");

            SprintSpeedInertiaCurveMax = Config.Bind(
                "Inertia Class",
                "Sprint Speed Inertia Curve Max",
                Vector2.zero,
                "Inertia Class - Default Settings: (2.5, 0.3)");

            SprintBrakeInertia = Config.Bind(
                "Inertia Class",
                "Sprint Brake Inertia",
                Vector2.zero,
                "Inertia Class - Default Settings: (0, 55)");

            SprintTransitionMotionPreservation = Config.Bind(
                "Inertia Class",
                "Sprint Transition Motion Preservation",
                Vector2.zero,
                "Inertia Class - Default Settings: (0.7, 0.9)");

            SprintAccelerationLimits = Config.Bind(
                "Inertia Class",
                "Sprint Acceleration Limits",
                new Vector2(10f, 1.6f),
                "Inertia Class - Default Settings: (5, 0.8)");

            PreSprintAccelerationLimits = Config.Bind(
                "Inertia Class",
                "PreSprint Acceleration Limits",
                new Vector2(10f, 1.6f),
                "Inertia Class - Default Settings: (5, 0.8)");

            WalkInertia = Config.Bind(
                "Inertia Class",
                "Walk Inertia",
                Vector2.zero,
                "Inertia Class - Default Settings: (0.05f, 0.5f)");

            SpeedInertiaAfterJump = Config.Bind(
                "Inertia Class",
                "Speed Inertia After Jump",
                Vector2.zero,
                "Inertia Class - Default Settings: (1, 1.4)");

            SideTime = Config.Bind(
                "Inertia Class",
                "Side Time",
                Vector2.zero,
                "Inertia Class - Default Settings: (2, 2)");

            InertiaBackwardsCoefficient = Config.Bind(
                "Inertia Class",
                "Inertia Backwards Coefficent",
                Vector2.zero,
                "Inertia Class - Default Settings: (1, 0.8)");

            StrafeInertionCoefficient = Config.Bind(
                "Main Settings",
                "Strafe Inertion Coeffecient",
                -5f,
                "EFT HardSettings - No Default Value");

            DurationPower = Config.Bind(
                "Inertia Class",
                "Duration Power",
                0f,
                "Inertia Class - Default Settings: 1.58");

            PenaltyPower = Config.Bind(
                "Inertia Class",
                "Penalty Power",
                0f,
                "Inertia Class - Default Settings: 1.12");

            BaseJumpPenalty = Config.Bind(
                "Inertia Class",
                "BaseJump Penalty",
                0f,
                "Inertia Class - Default Settings: 0.15");

            BaseJumpPenaltyDuration = Config.Bind(
                "Inertia Class",
                "BaseJump Penalty",
                0f,
                "Inertia Class - Default Settings: 0.3");

            TiltStartSideBackSpeed = Config.Bind(
                "Inertia Class",
                "Tilt StartSideBackSpeed",
                new Vector2(480f, 480f),
                "Inertia Class - Default Settings: (0.5, 0.2)");

            TiltMaxSideBackSpeed = Config.Bind(
                "Inertia Class",
                "Tilt MaxSideBackSpeed",
                new Vector2(1200f, 1200f),
                "Inertia Class - Default Settings: (1, 0.7)");

            TiltAcceleration = Config.Bind(
                "Inertia Class",
                "Tilt Acceleration",
                new Vector2(480f, 480f),
                "Inertia Class - Default Settings: (4.5, 1.5)");

            InertiaTiltCurveMin = Config.Bind(
                "Inertia Class",
                "Inertia Tilt CurveMin",
                Vector2.zero,
                "Inertia Class - Default Settings: (0, 0.3)");

            InertiaTiltCurveMax = Config.Bind(
                "Inertia Class",
                "Inertia Tilt CurveMax",
                Vector2.zero,
                "Inertia Class - Default Settings: (1, 0.1)");

            TiltInertiaMaxSpeed = Config.Bind(
                "Inertia Class",
                "Tilt Inertia MaxSpeed",
                Vector2.zero,
                "Inertia Class - Default Settings: (1.2, 1)");

            ExitMovementStateSpeedThreshold = Config.Bind(
                "Inertia Class",
                "Exit MovementStateSpeed Threshold",
                Vector2.zero,
                "Inertia Class - Default Settings: (0.02, 0.02");

            SpeedLimitDurationMin = Config.Bind(
                "Inertia Class",
                "SpeedLimit Duration Min",
                Vector2.zero,
                "Inertia Class - Default Settings: (0.02, 0.02");

            SpeedLimitDurationMax = Config.Bind(
                "Inertia Class",
                "SpeedLimit Duration Max",
                Vector2.zero,
                "Inertia Class - Default Settings: (0.02, 0.02)");

            MinDirectionBlendTime = Config.Bind(
                "Inertia Class",
                "Min Direction BlendTime",
                0f,
                "Inertia Class - Default Settings: 0.1");

            SuddenChangesSmoothness = Config.Bind(
                "Inertia Class",
                "Sudden Changes Smoothness",
                0f,
                "Inertia Class - Default Settings: 3");

            MaxTimeWithoutInput = Config.Bind(
                "Inertia Class",
                "Max TimeWithoutInput",
                Vector2.zero,
                "Inertia Class - Default Settings: (0.1, 0.3)");

            ProneDirectionAccelerationRange = Config.Bind(
                "Inertia Class",
                "Prone DirectionAccelerationRange",
                 new Vector2(5f, 5f),
                "Inertia Class - Default Settings: (2.5, 2.5)");

            ProneSpeedAccelerationRange = Config.Bind(
                "Inertia Class",
                "Prone SpeedAccelerationRange",
                new Vector2(5f, 5f),
                "Inertia Class - Default Settings: (1.2, 1.2)");

            CrouchSpeedAccelerationRange = Config.Bind(
                "Inertia Class",
                "CrouchSpeed AccelerationRange",
                new Vector2(5f, 5f),
                "Inertia Class - Default Settings: (1.2, 1.2)");

            WeaponFlipSpeed = Config.Bind(
                "Inertia Class",
                "Weapon FlipSpeed",
                new Vector2(2f, 4f),
                "Inertia Class - Default Settings: (0.5, 2)");

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

    }

        
        private void FixedUpdate()
        {
            if (Plugin.PluginEnabled.Value)
            {
                
                if (!Singleton<GameWorld>.Instantiated)
                {
                    return;
                }

                if (Camera.main == null)
                {
                    return;
                }

                try
                {
                    if (Singleton<GameWorld>.Instance.AllPlayers[0].IsYourPlayer)
                    {
                        var player = Singleton<GameWorld>.Instance.AllPlayers[0];
                        player.Physical.Inertia = Plugin.Inertia.Value;
                        player.Physical.MoveDiagonalInertia = Plugin.MoveDiagonalInertia.Value;
                        player.Physical.MoveSideInertia = Plugin.MoveSideInertia.Value;
                        player.Physical.BaseInertiaLimits = Plugin.baseInertia.Value;

                        var playeradditional = Singleton<GClass1173.GClass1228>.Instance;
                        playeradditional.DurationPower = Plugin.DurationPower.Value;
                        playeradditional.PenaltyPower = Plugin.PenaltyPower.Value;
                        playeradditional.BaseJumpPenalty = Plugin.BaseJumpPenalty.Value;
                        playeradditional.BaseJumpPenaltyDuration = Plugin.BaseJumpPenalty.Value;
                        playeradditional.InertiaLimits = Plugin.InertiaLimits.Value;
                        playeradditional.SprintBrakeInertia = Plugin.SprintBrakeInertia.Value;
                        playeradditional.WalkInertia = Plugin.WalkInertia.Value;
                        playeradditional.InertiaLimitsStep = Plugin.InertialLimitsStep.Value;
                        playeradditional.SpeedInertiaAfterJump = Plugin.SpeedInertiaAfterJump.Value;
                        playeradditional.TiltStartSideBackSpeed = Plugin.TiltStartSideBackSpeed.Value;
                        playeradditional.TiltMaxSideBackSpeed = Plugin.TiltStartSideBackSpeed.Value;
                        playeradditional.TiltAcceleration = Plugin.TiltAcceleration.Value;
                        playeradditional.InertiaTiltCurveMin = Plugin.InertiaTiltCurveMin.Value;
                        playeradditional.TiltInertiaMaxSpeed = Plugin.TiltInertiaMaxSpeed.Value;
                        playeradditional.SideTime = Plugin.SideTime.Value;
                        playeradditional.SprintSpeedInertiaCurveMin = Plugin.SprintSpeedInertiaCurveMin.Value;
                        playeradditional.SprintSpeedInertiaCurveMax = Plugin.SprintSpeedInertiaCurveMax.Value;
                        playeradditional.SprintTransitionMotionPreservation = Plugin.SprintTransitionMotionPreservation.Value;
                        playeradditional.InertiaBackwardCoef = Plugin.InertiaBackwardsCoefficient.Value;
                        playeradditional.ExitMovementStateSpeedThreshold = Plugin.ExitMovementStateSpeedThreshold.Value;
                        playeradditional.SpeedLimitDurationMin = Plugin.SpeedLimitDurationMin.Value;
                        playeradditional.SpeedLimitDurationMax = Plugin.SpeedLimitDurationMax.Value;
                        playeradditional.SprintTransitionMotionPreservation = Plugin.SprintTransitionMotionPreservation.Value;
                        playeradditional.SprintAccelerationLimits = Plugin.SprintAccelerationLimits.Value;
                        playeradditional.PreSprintAccelerationLimits = Plugin.PreSprintAccelerationLimits.Value;
                        playeradditional.DiagonalTime = Plugin.DiagonalTime.Value;
                        playeradditional.MinDirectionBlendTime = Plugin.MinDirectionBlendTime.Value;
                        playeradditional.MoveTimeRange = Plugin.MoveTimeRange.Value;
                        playeradditional.SuddenChangesSmoothness = Plugin.SuddenChangesSmoothness.Value;
                        playeradditional.MaxTimeWithoutInput = Plugin.MaxTimeWithoutInput.Value;
                        playeradditional.ProneDirectionAccelerationRange = Plugin.ProneDirectionAccelerationRange.Value;
                        playeradditional.ProneSpeedAccelerationRange = Plugin.ProneSpeedAccelerationRange.Value;
                        playeradditional.DiagonalStayTimeRange = Plugin.DiagonalStayTimeRange.Value;
                        playeradditional.CrouchSpeedAccelerationRange = Plugin.CrouchSpeedAccelerationRange.Value;
                        playeradditional.WeaponFlipSpeed = Plugin.WeaponFlipSpeed.Value;

                        EFTHardSettings settings = Singleton<EFTHardSettings>.Instance;
                        settings.StrafeInertionCoefficient = Plugin.StrafeInertionCoefficient.Value;
                        settings.TILT_CHANGING_SPEED = Plugin.tiltchangingspeed.Value;

                        RootMotion.FinalIK.Inertia inertiaIK = Singleton<Inertia>.Instance;
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
                        }


                    }
                }
                catch
                {
                    return;
                }
                
                
            }

        }

    }
}