using HarmonyLib;
using PulsarModLoader.Content.Components.AutoTurret;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Cinemachine.DocumentationSortingAttribute;

namespace PulsarModLoader.Content.Components.HullPlating
{
    public class HullPlatingModManager : LegacyInstantiatableComponentModManager<PLHullPlating, HullPlatingMod, EHullPlatingType>
    {
        public readonly int VanillaHullPlatingMaxType = 0;
        private static HullPlatingModManager m_instance = null;
        public readonly List<HullPlatingMod> HullPlatingTypes = new List<HullPlatingMod>();
        public static HullPlatingModManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new HullPlatingModManager();
                }
                return m_instance;
            }
        }

        HullPlatingModManager() : base((int)ESlotType.E_COMP_HULLPLATING)
        {
            VanillaHullPlatingMaxType = VanillaMaxType;
            HullPlatingTypes = legacyModComps;
        }
        protected override string GetLegacyComponentName(HullPlatingMod comp)
        {
            return comp.Name;
        }
        protected override Type GetComponentType(HullPlatingMod comp)
        {
            Type type = GetConstructorTypeFromMethodBody(AccessTools.PropertyGetter(comp.GetType(), nameof(HullPlatingMod.PLHullPlating)));
            if (type is not null)
            {
                return type;
            }
            throw new Exception($"What is going on in {comp.Name} get component property");
        }
        /// <summary>
        /// Finds HullPlating type equivilent to given name and returns Subtype ID needed to spawn. Returns -1 if couldn't find HullPlating.
        /// </summary>
        /// <param name="HullPlatingName">Name of Component</param>
        /// <returns>Subtype ID of component</returns>
        public int GetHullPlatingIDFromName(string HullPlatingName) => GetIDFromName(HullPlatingName);
        public static PLHullPlating CreateHullPlating(int Subtype, int level, int Subtypedata = 0)
        {
            if (Instance.TryCreateComponent(Subtype, level, Subtypedata, out PLHullPlating comp))
            {
                return comp;
            }
            return new PLHullPlating((EHullPlatingType)Subtype, level);
        }
    }
    //Converts hashes to HullPlatings.
    [HarmonyPatch(typeof(PLHullPlating), "CreateHullPlatingFromHash")]
    class HullPlatingHashFix
    {
        static bool Prefix(int inSubType, int inLevel, int inSubTypeData, ref PLShipComponent __result)
        {
            __result = HullPlatingModManager.CreateHullPlating(inSubType, inLevel, inSubTypeData);
            return false;
        }
    }
}
