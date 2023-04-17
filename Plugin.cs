using Aki.Reflection.Patching;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using HarmonyLib;
using RootMotion.FinalIK;
using UnityEngine;
using System;

namespace dvize.FUInertia
{
    [BepInPlugin("com.dvize.FUInertia", "dvize.FUInertia", "2.0.0")]

    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> tiltSpeed;
        public static ConfigEntry<float> tiltSensitivity;
        public static ConfigEntry<float> bodygravity;
        public static ConfigEntry<float> effectorlinkweight;

        void Awake()
        {
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
            new ManualAnimatorPatch1SideStep().Enable();
            new FinalIKPatchBodyTilt().Enable();
            new FinalIKPatch().Enable();
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

    }


    public class inertiaOnWeightUpdatedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass703), "OnWeightUpdated");
        }

        [PatchPrefix]
        private static bool Prefix(GClass703 __instance)
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

            return false;
        }
    }

    public class UpdateWeightLimitsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass703), "UpdateWeightLimits");
        }

        [PatchPostfix]
        static void Postfix(GClass703 __instance)
        {

            // Set the Vector2 variables to zero
            __instance.BaseInertiaLimits = Vector2.zero;
            __instance.WalkOverweightLimits = Vector2.zero;
            __instance.BaseOverweightLimits = Vector2.zero;
            __instance.SprintOverweightLimits = Vector2.zero;
            __instance.WalkSpeedOverweightLimits = Vector2.zero;

        }
    }

    public class SprintAccelerationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass1604), "SprintAcceleration");
        }

        [PatchPrefix]
        private static bool Prefix(GClass1604 __instance, float deltaTime)
        {
            try
            {
                var player0 = AccessTools.Field(typeof(GClass1604), "player_0").GetValue(__instance) as Player;
                bool inRaid = Singleton<AbstractGame>.Instance.InRaid;

                if (player0.IsYourPlayer && inRaid)
                {
                    var sprintAcceleration = player0.Physical.SprintAcceleration;

                    float num = sprintAcceleration * deltaTime;
                    float num2 = (player0.Physical.SprintSpeed * __instance.SprintingSpeed + 1f) * __instance.StateSprintSpeedLimit;
                    __instance.SprintSpeed = Mathf.Clamp(__instance.SprintSpeed + num * Mathf.Sign(num2 - __instance.SprintSpeed), 0.01f, num2);

                }
                else
                {
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Logger.LogError(e);
            }

            // return false to skip the original method
            return false;
        }
    }

    public class ManualAnimatorPatch1SideStep : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass1619), "ManualAnimatorMoveUpdate");
        }

        [PatchPostfix]
        static void Postfix(GClass1619 __instance, float deltaTime)
        {
            var movementContext = AccessTools.Field(typeof(GClass1619), "MovementContext").GetValue(__instance) as GClass1604;
           /* var float_3 = (float)AccessTools.Field(typeof(GClass1619), "float_3").GetValue(__instance);
            var float_2 = (float)AccessTools.Field(typeof(GClass1619), "float_2").GetValue(__instance);
            var float_1 = (float)AccessTools.Field(typeof(GClass1619), "float_1").GetValue(__instance);
*/
            switch (__instance.Type)
            {
                case EStateType.None:
                    //movementContext.SetSidestep(float_3);
                    movementContext.SetSidestep(0f);
                    break;
                case EStateType.In:
                    //movementContext.SetSidestep(float_2 + (float_3 - float_2) * (float_1 / __instance.StateLength));
                    movementContext.SetSidestep(0f);
                    break;
                case EStateType.Out:
                    //movementContext.SetSidestep(float_3 + (float_2 - float_3) * (float_1 / __instance.StateLength));
                    movementContext.SetSidestep(0f);
                    break;
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

    public class FinalIKPatchBodyTilt : ModulePatch
    {
        static FieldInfo lastForwardField = AccessTools.Field(typeof(BodyTilt), "lastForward");
        static FieldInfo tiltAngleField = AccessTools.Field(typeof(BodyTilt), "tiltAngle");
        static FieldInfo ikField = AccessTools.Field(typeof(BodyTilt), "ik");

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(RootMotion.FinalIK.BodyTilt), "OnModifyOffset");
        }

        [PatchPostfix]
        static void Postfix(BodyTilt __instance)
        {
            var tiltAngle = (float)tiltAngleField.GetValue(__instance);
            var ik = (FullBodyBipedIK)ikField.GetValue(__instance);

            Quaternion quaternion = Quaternion.FromToRotation((Vector3)lastForwardField.GetValue(__instance), __instance.transform.forward);
            float num = 0f;
            Vector3 zero = Vector3.zero;
            quaternion.ToAngleAxis(out num, out zero);
            if (zero.y > 0f)
            {
                num = -num;
            }
            num *= __instance.tiltSensitivity * 0.01f;
            num /= Time.deltaTime;
            num = Mathf.Clamp(num, -1f, 1f);
            tiltAngleField.SetValue(__instance, num);
            float num2 = Mathf.Abs(tiltAngle) / 1f;
            if (tiltAngle < 0f)
            {
                __instance.poseRight.Apply(ik.solver, num2);
            }
            else
            {
                __instance.poseLeft.Apply(ik.solver, num2);
            }
            lastForwardField.SetValue(__instance, __instance.transform.forward);
        }
    }

    

}