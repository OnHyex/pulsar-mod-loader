using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using PulsarModLoader.Content.Components.InternalHelperClasses;

namespace PulsarModLoader.Content.Components.Missile
{
    public class MissileModManager : LegacyComponentModManager<PLTrackerMissile, MissileMod, ETrackerMissileType>
    {
        public readonly int VanillaMissileMaxType = 0;
        private static MissileModManager m_instance = null;
        public readonly List<MissileMod> MissileTypes = new List<MissileMod>();
        public static MissileModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MissileModManager();
                }
                return m_instance;
            }
        }

        MissileModManager() : base((int)ESlotType.E_COMP_TRACKERMISSILE)
        {
            VanillaMissileMaxType = VanillaMaxType;
            MissileTypes = legacyModComps;
        }
        /// <summary>
        /// Finds Missile type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find Missile.
        /// </summary>
        /// <param name="MissileName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetMissileIDFromName(string MissileName) => GetIDFromName(MissileName);
        protected override void ComponentModConstructor(PLTrackerMissile comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            base.ComponentModConstructor(comp, legacyComp, subType, level, subTypeData);
            MissileMod missile = legacyComp as MissileMod;
            comp.Damage = missile.Damage;
            comp.Speed = missile.Speed;
            comp.DamageType = missile.DamageType;
            comp.MissileRefillPrice = missile.MissileRefillPrice;
            comp.AmmoCapacity = missile.AmmoCapacity;
            comp.PrefabID = missile.PrefabID;
        }
        static ConstructorInfo constructor = typeof(PLTrackerMissile).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(ETrackerMissileType), typeof(int), typeof(int) }, null);
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
            //not used for this component type
            methodOverrides.Remove(ComponentModMethods.getStatLineLeft);
            methodOverrides.Remove(ComponentModMethods.getStatLineRight);
            return;
        }
        public static PLTrackerMissile CreateMissile(int Subtype, int level, int inSubTypeData = 0)
        {
            if (Instance.TryCreateComponent(Subtype, level, inSubTypeData, out PLTrackerMissile comp))
            {
                return comp;
            }
            return new PLTrackerMissile((ETrackerMissileType)Subtype, level, inSubTypeData);
        }
    }
    //Converts hashes to Missiles.
    [HarmonyPatch(typeof(PLTrackerMissile), "CreateTrackerMissileFromHash")]
    class MissileHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = MissileModManager.CreateMissile(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
}
