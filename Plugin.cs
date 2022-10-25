using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using EFT.UI.Health;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace dvize.BulletTime
{
    [BepInPlugin("com.dvize.FUInertia", "dvize.FUInertia", "1.3.0")]

    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> PluginEnabled;
        public static ConfigEntry<Vector3> baseInertia;
        public static ConfigEntry<float> Inertia;
        public static ConfigEntry<float> MoveDiagonalInertia;
        public static ConfigEntry<float> MoveSideInertia;
        public static ConfigEntry<Vector3> InertialLimits;
        public static ConfigEntry<float>   InertialLimitsStep;
        public static ConfigEntry<Vector2> SprintSpeedInertiaCurveMax;
        public static ConfigEntry<Vector2> SprintBrakeInertia;
        public static ConfigEntry<Vector2> SprintTransitionMotionPreservation;
        public static ConfigEntry<Vector2> WalkInertia;
        public static ConfigEntry<Vector2> SpeedInertiaAfterJump;
        public static ConfigEntry<Vector2> TiltInertiaMaxSpeed;
        public static ConfigEntry<Vector2> SideTime;
        public static ConfigEntry<Vector2> InertiaBackwardsCoefficient;
        public static ConfigEntry<float> StrafeInertionCoefficient;
        async void Awake()
        {
            PluginEnabled = Config.Bind(
                "Main Settings",
                "Plugin on/off",
                true,
                "");

            baseInertia = Config.Bind(
                "Main Settings",
                "Base Inertia",
                Vector3.zero,
                "Set inertia base limits. Think this is related to weight on your person");

            Inertia = Config.Bind(
                "Main Settings",
                "Overall Inertia",
                0f,
                "Set inertia for forwards/backwards (i think)");

            MoveDiagonalInertia = Config.Bind(
                "Main Settings",
                "Move Diagonal Inertia",
                0f,
                "Set inertia for moving diagonally");

            MoveSideInertia = Config.Bind(
                "Main Settings",
                "Move Side Inertia",
                0f,
                "Set inertia for moving sideways");

            InertialLimits = Config.Bind(
                "Main Settings",
                "Inertial Limits",
                Vector3.zero,
                "Global Settings inertial limits?");

            InertialLimitsStep = Config.Bind(
                "Main Settings",
                "Inertial Limits Step",
                0f,
                "Global Settings.. No clue");

            SprintSpeedInertiaCurveMax = Config.Bind(
                "Main Settings",
                "Sprint Speed Inertia Curve Max",
                Vector2.zero,
                "Global Settings Sprint Speed Inertia Curve Max");

            SprintBrakeInertia = Config.Bind(
                "Main Settings",
                "Sprint Brake Inertia",
                Vector2.zero,
                "Global Settings Sprint Brake Inertia");

            SprintTransitionMotionPreservation = Config.Bind(
                "Main Settings",
                "Sprint Transition Motion Preservation",
                Vector2.zero,
                "Global Settings..no idea");

            WalkInertia = Config.Bind(
                "Main Settings",
                "Walk Inertia",
                Vector2.zero,
                "Global Settings..Walk Inertia");

            SpeedInertiaAfterJump = Config.Bind(
                "Main Settings",
                "Speed Inertia After Jump",
                Vector2.zero,
                "Global Settings..Inertia After Jump");

            TiltInertiaMaxSpeed = Config.Bind(
                "Main Settings",
                "Tilt Inertia Max Speed",
                Vector2.zero,
                "Global Settings..Tilt Inertia Max Speed");

            SideTime = Config.Bind(
                "Main Settings",
                "Side Time",
                Vector2.zero,
                "Global Settings..Side Time");

            InertiaBackwardsCoefficient = Config.Bind(
                "Main Settings",
                "Inertia Backwards Coefficent",
                Vector2.zero,
                "Global Settings..I don't know");

            StrafeInertionCoefficient = Config.Bind(
                "Main Settings",
                "Strafe Inertion Coeffecient",
                -5f,
                "EFT Hard Settings..hopefully strafe inertia");
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

                if (Singleton<GameWorld>.Instance.AllPlayers[0].IsYourPlayer)
                {
                    var player = Singleton<GameWorld>.Instance.AllPlayers[0];


                    player.Physical.Inertia = Plugin.Inertia.Value;
                    player.Physical.MoveDiagonalInertia = Plugin.MoveDiagonalInertia.Value;
                    player.Physical.MoveSideInertia = Plugin.MoveSideInertia.Value;
                    player.Physical.BaseInertiaLimits = Plugin.baseInertia.Value;

                    var playeradditional = Singleton<GClass1168.GClass1223>.Instance;
                    playeradditional.InertiaLimits = Plugin.InertialLimits.Value;
                    playeradditional.SprintBrakeInertia = Plugin.SprintBrakeInertia.Value;
                    playeradditional.WalkInertia = Plugin.WalkInertia.Value;
                    playeradditional.InertiaLimitsStep = Plugin.InertialLimitsStep.Value;
                    playeradditional.SpeedInertiaAfterJump = Plugin.SpeedInertiaAfterJump.Value;
                    playeradditional.TiltInertiaMaxSpeed = Plugin.TiltInertiaMaxSpeed.Value;
                    playeradditional.SideTime = Plugin.SideTime.Value;
                    playeradditional.SprintSpeedInertiaCurveMax = Plugin.SprintSpeedInertiaCurveMax.Value;
                    playeradditional.SprintTransitionMotionPreservation = Plugin.SprintTransitionMotionPreservation.Value;
                    playeradditional.InertiaBackwardCoef = Plugin.InertiaBackwardsCoefficient.Value;

                    EFTHardSettings settings = Singleton<EFTHardSettings>.Instance;

                    settings.StrafeInertionCoefficient = Plugin.StrafeInertionCoefficient.Value;
                }
                
                

                
            }

        }

    }
}