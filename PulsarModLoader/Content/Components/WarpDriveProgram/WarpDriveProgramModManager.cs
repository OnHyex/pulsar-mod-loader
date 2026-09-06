using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Logger = PulsarModLoader.Utilities.Logger;
using PulsarModLoader.Content.Components.InternalHelperClasses;

namespace PulsarModLoader.Content.Components.WarpDriveProgram
{
    public class WarpDriveProgramModManager : LegacyComponentModManager<PLWarpDriveProgram, WarpDriveProgramMod, EWarpDriveProgramType>
    {
        public readonly int VanillaWarpDriveProgramMaxType = 0;
        private static WarpDriveProgramModManager m_instance = null;
        public readonly List<WarpDriveProgramMod> WarpDriveProgramTypes = new List<WarpDriveProgramMod>();
        public static WarpDriveProgramModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new WarpDriveProgramModManager();
                }
                return m_instance;
            }
        }

        WarpDriveProgramModManager() : base((int)ESlotType.E_COMP_PROGRAM)
        {
            VanillaWarpDriveProgramMaxType = VanillaMaxType;
            WarpDriveProgramTypes = legacyModComps;
        }
        /// <summary>
        /// Finds WarpDriveProgram type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find WarpDriveProgram.
        /// </summary>
        /// <param name="WarpDriveProgramName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetWarpDriveProgramIDFromName(string WarpDriveProgramName) => GetIDFromName(WarpDriveProgramName);
        protected override void ComponentModConstructor(PLWarpDriveProgram comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            WarpDriveProgramMod program = legacyComp as WarpDriveProgramMod;
            comp.MaxLevelCharges = program.MaxLevelCharges;
            comp.Level = program.MaxLevelCharges;
            comp.IsVirus = program.IsVirus;
            comp.VirusType = (EVirusType)program.VirusSubtype;
            comp.ShortName = program.ShortName;
            comp.ShieldBooster_ActiveTime = program.ActiveTime;
        }
        static ConstructorInfo constructor = typeof(PLWarpDriveProgram).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EWarpDriveProgramType), typeof(int), typeof(short) }, null);
        protected override void BaseClassConstructor(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Call, constructor);
            il.Emit(OpCodes.Ret);
        }
        protected override void ModComponentSubtypeMethods(Dictionary<MethodInfo, MethodInfo> methodOverrides)
        {
            MethodInfo method = methodOverrides[ComponentModMethods.finalLateAddStats];
            methodOverrides.Remove(ComponentModMethods.finalLateAddStats);
            methodOverrides.Add(typeof(LegacyWarpDriveProgramHelperMethods).GetMethod(nameof(LegacyWarpDriveProgramHelperMethods.LegacyFinalLateAddStats), BindingFlags.Public | BindingFlags.Static), method);
            methodOverrides.Add(typeof(LegacyWarpDriveProgramHelperMethods).GetMethod(nameof(LegacyWarpDriveProgramHelperMethods.LegacyGetActiveTimerAlpha), BindingFlags.Public | BindingFlags.Static), typeof(PLWarpDriveProgram).GetMethod(nameof(PLWarpDriveProgram.GetActiveTimerAlpha), BindingFlags.Public | BindingFlags.Instance));
            methodOverrides.Add(typeof(LegacyWarpDriveProgramHelperMethods).GetMethod(nameof(LegacyWarpDriveProgramHelperMethods.LegacyExecute), BindingFlags.Public | BindingFlags.Static), typeof(PLWarpDriveProgram).GetMethod(nameof(PLWarpDriveProgram.ExecuteBasedOnType), BindingFlags.NonPublic | BindingFlags.Instance));
        }
        public static PLWarpDriveProgram CreateWarpDriveProgram(int Subtype, int level)
        {
            return CreateWarpDriveProgram(Subtype, level, 0);
        }
        public static PLWarpDriveProgram CreateWarpDriveProgram(int Subtype, int level, short inSubTypeData)
        {
            if (Instance.TryCreateComponent(Subtype, level, inSubTypeData, out PLWarpDriveProgram comp))
            {
                return comp;
            }
            return new PLWarpDriveProgram((EWarpDriveProgramType)Subtype, level, inSubTypeData);
        }
    }
    //Converts hashes to WarpDrivePrograms.
    [HarmonyPatch(typeof(PLWarpDriveProgram), "CreateWarpDriveProgramFromHash")]
    class WarpDriveProgramHashFix
    {
        static bool Prefix(int inSubType, int inLevel, short inSubTypeData, ref PLShipComponent __result)
        {
            __result = WarpDriveProgramModManager.CreateWarpDriveProgram(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
    //[HarmonyPatch(typeof(PLWarpDriveProgram), "FinalLateAddStats")]
    //class WarpDriveProgramFinalLateAddStatsPatch
    //{
    //    static void Postfix(PLWarpDriveProgram __instance)
    //    {
    //        int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count && Time.time - __instance.ShieldBooster_LastActivationTime < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].ActiveTime)
    //        {
    //            WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].FinalLateAddStats(__instance);
    //        }
    //    }
    //}
    //[HarmonyPatch(typeof(PLWarpDriveProgram), "ExecuteBasedOnType")]
    //class WarpDriveProgramExecuteBasedOnTypePatch
    //{
    //    static void Prefix(PLWarpDriveProgram __instance)
    //    {
    //        int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count)
    //        {
    //            if (WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].IsVirus) 
    //            {
    //                PLServer.Instance.photonView.RPC("AddToSendQueue", PhotonTargets.All, new object[] {
    //                    __instance.ShipStats.Ship.ShipID,
    //                    __instance.ShipStats.Ship.VirusSendQueueCounter + 1,
    //                    WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].VirusSubtype,
    //                    PLServer.Instance.GetEstimatedServerMs()
    //                });
    //                PulsarModLoader.Utilities.Messaging.Notification($"{WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].VirusSubtype}");
    //            }
    //            else
    //            {
    //                __instance.ShieldBooster_LastActivationTime = Time.time;
    //                WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].Execute(__instance);
    //            }
    //        }
    //    }
    //}
    [HarmonyPatch(typeof(PLServer), "AddToSendQueue")]
    class WarpDriveProgramAddToSendQueuePatch
    {
        static bool Prefix(int shipID, int sendQueueID, int virusType, int serverTime)
        {
            Debug.Log("AddToSendQueue: shipID-" + shipID.ToString() + "   sendQueueID-" + sendQueueID.ToString());
            PLServer.Instance.StartCoroutine(LateAddToSendQueueReplacement(shipID, sendQueueID, virusType, serverTime));
            return false;
        }
        private static IEnumerator LateAddToSendQueueReplacement(int shipID, int sendQueueID, int virusType, int serverTime)
        {
            PLShipInfoBase ship = null;
            while (ship == null)
            {
                ship = PLEncounterManager.Instance.GetShipFromID(shipID);
                if (ship == null)
                {
                    yield return new WaitForSeconds(0.05f);
                }
            }
            if (!ship.VirusSendQueue.ForwardDictionary.ContainsKey(sendQueueID))
            {
                PLVirus plvirus = Virus.VirusModManager.CreateVirus(virusType, 0);
                plvirus.NetID = -1;
                plvirus.InitialTime = serverTime;
                ship.VirusSendQueue.Add(sendQueueID, plvirus);
                plvirus.Sender = ship;
                Debug.Log("adding virus from send queue: id-" + sendQueueID.ToString() + "   name-" + plvirus.Name);
            }
            yield break;
        }
    }
    //[HarmonyPatch(typeof(PLWarpDriveProgram), "GetActiveTimerAlpha")]
    //class WarpDriveProgramGetActiveTimerAlphaPatch
    //{
    //    static void Postfix(PLWarpDriveProgram __instance, ref float __result)
    //    {
    //        int subtypeformodded = __instance.SubType - WarpDriveProgramModManager.Instance.VanillaWarpDriveProgramMaxType;
    //        if (subtypeformodded > -1 && subtypeformodded < WarpDriveProgramModManager.Instance.WarpDriveProgramTypes.Count)
    //        {
    //            __result = Mathf.Clamp01((Time.time - __instance.ShieldBooster_LastActivationTime) / WarpDriveProgramModManager.Instance.WarpDriveProgramTypes[subtypeformodded].ActiveTime);
    //        }
    //    }
    //}
}
