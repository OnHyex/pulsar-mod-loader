#nullable enable
using HarmonyLib;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PulsarModLoader.Content.Components
{
    public class ComponentReflectionConstructor<TComp> where TComp : PLShipComponent
    {
        public TComp CreateWithThreeArgsExplicit(Type specificComponentType, object arg1, object arg2, object arg3)
        {
            if (!ReflectionCache.TryGetValue(specificComponentType, out var constructor))
            {
                // Define the expected types of your three arguments
                var argTypes = new[] { arg1.GetType(), arg2.GetType(), arg3.GetType() };

                // Find the public constructor
                constructor = specificComponentType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, argTypes, null);

                if (constructor == null)
                {
                    throw new Exception($"3 Parameter Constructor not found for type {specificComponentType.Name}");
                }

                ReflectionCache.TryAdd(specificComponentType, constructor);
            }
            TComp comp = (TComp)constructor.Invoke(new[] { arg1, arg2, arg3 });
            if (comp is not null)
            {
                return comp;
            }
            throw new Exception($"Constructor failed for {specificComponentType.Name}");
        }
        protected readonly Dictionary<Type, ConstructorInfo> ReflectionCache = new Dictionary<Type, ConstructorInfo>();
    }
    public abstract class ComponentModManager<TComp> where TComp : PLShipComponent
    {
        protected readonly int VanillaMaxType = 0;
        protected readonly int _SlotType = 0;
        protected readonly Dictionary<PulsarMod, List<Type>> componentsByMod = new Dictionary<PulsarMod, List<Type>>();
        protected readonly List<Type> components = new List<Type>(64);

        protected internal ComponentModManager(int SlotType)
        {
            _SlotType = SlotType;
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                foreach (Type t in asm.GetTypes())
                {
                    if (typeof(TComp).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        if (componentsByMod.ContainsKey(mod))
                        {
                            componentsByMod[mod].Add(t);
                        }
                        else
                        {
                            componentsByMod[mod] = new List<Type>() { t };
                        }
                        components.Add(t);
                    }
                }
            }
        }
        protected internal ComponentModManager(int SlotType, int MaxType) : this(SlotType)
        {
            VanillaMaxType = MaxType;
        }
        protected readonly ComponentReflectionConstructor<TComp> constructorFactory = new ComponentReflectionConstructor<TComp>();
        public virtual TComp? CreateComponent(int SubType, int Level, int SubTypeData)
        {
            Type compType = components[SubType];
            try
            {
                return constructorFactory.CreateWithThreeArgsExplicit(compType, SubType, Level, (short)SubTypeData);
            }
            catch (Exception ex)
            {
                Logger.Info($"Failed to create modded component {compType.Name}, {ex}");
            }
            return null;
        }
        protected virtual void HandleModUnLoaded(PulsarMod? mod)
        {
            if (mod is not null && componentsByMod.ContainsKey(mod))
            {
                componentsByMod.Remove(mod);
                components.Clear();
                foreach (var kvp in componentsByMod)
                {
                    foreach (Type type in kvp.Value)
                    {
                        components.Add(type);
                    }
                }
            }
        }
        public virtual int GetIDFromName(string name)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].Name == name)
                {
                    return i + VanillaMaxType;
                }
            }
            return -1;
        }

        public virtual int GetIDFromType(Type type)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] == type)
                {
                    return i + VanillaMaxType;
                }
            }
            return -1;
        }

        public IReadOnlyList<Type> ModTypes
        {
            get
            {
                return components.AsReadOnly();
            }
        }
    }
    public abstract class ComponentModManager<TComp,TEnum> : ComponentModManager<TComp> where TComp : PLShipComponent where TEnum : Enum
    {
        protected ComponentModManager(int SlotType) : base (SlotType, Enum.GetValues(typeof(TEnum)).Length)
        {
            Logger.Info($"{typeof(TComp).Name} MaxTypeint: {VanillaMaxType - 1}");
        }
    }
    public abstract class ComponentModManager<TComp,TLegacyModComp,TEnum> : ComponentModManager<TComp> where TComp : PLShipComponent where TLegacyModComp : ComponentModBase where TEnum : Enum 
    {
        protected readonly LegacyComponentCompatabilityFactory<TComp, TLegacyModComp> legacyComponentFactory;

        protected ComponentModManager(int SlotType) : base (SlotType, Enum.GetValues(typeof(TEnum)).Length)
        {
            Logger.Info($"{typeof(TComp).Name} MaxTypeint: {VanillaMaxType - 1}");

            legacyComponentFactory = new LegacyComponentCompatabilityFactory<TComp, TLegacyModComp>(typeof(TEnum), (ILGenerator il) => { BaseClassConstructor(il); }, (Dictionary<MethodInfo, MethodInfo> dictionary) => { ModComponentSubtypeMethods(dictionary); });
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                Assembly asm = mod.GetType().Assembly;
                foreach (Type t in asm.GetTypes())
                {
                    if (typeof(TLegacyModComp).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    {
                        //Logger.Info($"Loading {typeof(TMod).Name} from assembly");
                        TLegacyModComp handler = (TLegacyModComp)Activator.CreateInstance(t);
                        if (GetIDFromName(handler.Name) == -1)
                        {
                            try
                            {
                                Type generatedType = legacyComponentFactory.DefineNewType(handler);
                                if (componentsByMod.ContainsKey(mod))
                                {
                                    componentsByMod[mod].Add(generatedType);
                                }
                                else
                                {
                                    componentsByMod[mod] = new List<Type>() { generatedType };
                                }
                                components.Add(generatedType);
                                legacyCompLookup.Add(generatedType, handler);
                            }
                            catch (Exception ex)
                            {
                                Logger.Info($"Could not add legacy component {typeof(TLegacyModComp).Name} from {mod.Name} due to error {ex.ToString()}, StackTrace: {Environment.StackTrace}");
                            }

                            Logger.Info($"Added legacy component {typeof(TLegacyModComp).Name}: '{handler.Name}' with ID '{GetIDFromName(handler.Name)}' to dynamic builder");
                        }
                        else
                        {
                            Logger.Info($"Could not add legacy component {typeof(TLegacyModComp).Name} from {mod.Name} with the duplicate name of '{handler.Name}' to dynamic builder");
                        }
                    }
                }
            }
            UpdateLegacyModComps();
        }
        protected virtual void ComponentModConstructor(TComp comp, ComponentModBase legacyComp, int subType, int level, short subTypeData)
        {
            comp.Name = legacyComp.Name;
            comp.Desc = legacyComp.Description;
            comp.m_MarketPrice = legacyComp.MarketPrice;
            comp.CargoVisualPrefabID = legacyComp.CargoVisualID;
            comp.CanBeDroppedOnShipDeath = legacyComp.CanBeDroppedOnShipDeath;
            comp.Experimental = legacyComp.Experimental;
            comp.Unstable = legacyComp.Unstable;
            comp.Contraband = legacyComp.Contraband;
            comp.Price_LevelMultiplierExponent = legacyComp.Price_LevelMultiplierExponent;
            comp.m_IconTexture = legacyComp.IconTexture;
        }
        protected abstract void BaseClassConstructor(ILGenerator il);
        protected abstract void ModComponentSubtypeMethods(Dictionary<MethodInfo,MethodInfo> methodOverrides);
        public override TComp? CreateComponent(int SubType, int Level, int SubTypeData)
        {
            TComp? comp = base.CreateComponent(SubType, Level, SubTypeData);
            if (comp is ILegacyComponent LegacyComp)
            {
                ComponentModConstructor(comp, LegacyComp.GetComponentMod(), SubType, Level, (short)SubTypeData);
            }
            return comp;
        }
        public readonly List<TLegacyModComp?> legacyModComps = new List<TLegacyModComp?>();
        private readonly Dictionary<Type, TLegacyModComp> legacyCompLookup = new Dictionary<Type, TLegacyModComp>();
        protected override void HandleModUnLoaded(PulsarMod? mod)
        {
            base.HandleModUnLoaded(mod);
            UpdateLegacyModComps();
        }
        private void UpdateLegacyModComps()
        {
            legacyModComps.Clear();
            legacyCompLookup.Clear();
            foreach (var type in components)
            {
                if (typeof(ILegacyComponent).IsAssignableFrom(type))
                {
                    FieldInfo legacyField = type.GetField("_legacy", BindingFlags.Static | BindingFlags.NonPublic);
                    TLegacyModComp comp = (TLegacyModComp)legacyField.GetValue(null);
                    legacyModComps.Add(comp);
                    legacyCompLookup.Add(type, comp);
                }
                else
                {
                    legacyModComps.Add(null);
                }
            }
        }
        
        public override int GetIDFromName(string name)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].Name == name || (legacyCompLookup.TryGetValue(components[i], out TLegacyModComp legacy) && legacy.Name == name))
                {
                    return i + VanillaMaxType;
                }
            }
            return -1;
        }
        public override int GetIDFromType(Type type)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] == type || (legacyCompLookup.TryGetValue(components[i], out TLegacyModComp legacy) && legacy.GetType() == type))
                {
                    return i + VanillaMaxType;
                }
            }
            return -1;
        }
    }
    public interface ILegacyComponent
    {
        public Type GetLegacyType();
        public ComponentModBase GetComponentMod();
    }
}
