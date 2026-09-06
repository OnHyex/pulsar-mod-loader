using CodeStage.AntiCheat.ObscuredTypes;
using HarmonyLib;
using PulsarModLoader.Content.Components.Virus;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PulsarModLoader.Content.Components.WarpDrive
{
    public class WarpDriveModManager : LegacyComponentModManager<PLWarpDrive, WarpDriveMod, EWarpDriveType>
    {
        public readonly int VanillaWarpDriveMaxType = 0;
        private static WarpDriveModManager m_instance = null;
        public readonly List<WarpDriveMod> WarpDriveTypes = new List<WarpDriveMod>();
        public static WarpDriveModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new WarpDriveModManager();
                }
                return m_instance;
            }
        }

        WarpDriveModManager() : base((int)ESlotType.E_COMP_WARP)
        {
            VanillaWarpDriveMaxType = VanillaMaxType;
            WarpDriveTypes = legacyModComps;
        }
        /// <summary>
        /// Finds WarpDrive type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find WarpDrive.
        /// </summary>
        /// <param name="WarpDriveName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetWarpDriveIDFromName(string WarpDriveName) => GetIDFromName(WarpDriveName);
        protected override void ComponentModConstructor(PLWarpDrive comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            WarpDriveMod drive = legacyComp as WarpDriveMod;
            comp.ChargeSpeed = drive.ChargeSpeed;
            comp.WarpRange = drive.WarpRange;
            comp.EnergySignatureAmt = drive.EnergySignature;
            comp.NumberOfChargingNodes = drive.NumberOfChargesPerFuel;
            comp.m_MaxPowerUsage_Watts = drive.MaxPowerUsage_Watts;
        }
        static ConstructorInfo constructor = typeof(PLWarpDrive).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EWarpDriveType), typeof(int), typeof(short) }, null);
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
            return;
        }
        public static PLWarpDrive CreateWarpDrive(int Subtype, int level, short SubTypeData)
        {
            if (Instance.TryCreateComponent(Subtype, level, SubTypeData, out PLWarpDrive comp))
            {
                return comp;
            }
            return new PLWarpDrive((EWarpDriveType)Subtype, level, SubTypeData);
        }
    }
    //Converts hashes to WarpDrives.
    [HarmonyPatch(typeof(PLWarpDrive), "CreateWarpDriveFromHash")]
    class WarpDriveHashFix
    {
        static bool Prefix(int inSubType, int inLevel, short inSubTypeData, ref PLShipComponent __result)
        {
            __result = WarpDriveModManager.CreateWarpDrive(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
}
