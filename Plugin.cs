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
    [BepInPlugin("com.dvize.FUInertia", "dvize.FUInertia", "1.1.0")]

    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> PluginEnabled;
        public static ConfigEntry<Vector3> baseInertia;
        public static ConfigEntry<float> Inertia;
        public static ConfigEntry<float> MoveDiagonalInertia;
        public static ConfigEntry<float> MoveSideInertia;


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

                }
                
                

                
            }

        }

    }
}