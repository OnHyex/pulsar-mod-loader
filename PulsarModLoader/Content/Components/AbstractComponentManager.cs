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
    public abstract class ComponentModManager<TComp> where TComp : PLShipComponent
    {
        protected readonly int VanillaMaxType = 0;
        protected readonly int _SlotType = 0;
        protected readonly Dictionary<PulsarMod, List<Type>> componentsByMod = new Dictionary<PulsarMod, List<Type>>();
        protected readonly List<Type> components = new List<Type>(64);
        protected static List<Type> typesAlreadyProcessed = new();
        protected static bool TypeAlreadyProcessedChildType(List<Type> childTypes, Type t)
        {
            if (childTypes.Count == 0)
            {
                return false;
            }
            foreach (Type type in childTypes)
            {
                if (type.IsAssignableFrom(t))
                {
                    return true;
                }
            }
            return false;
        }
        protected internal ComponentModManager(int SlotType, int MaxType)
        {
            _SlotType = SlotType;
            VanillaMaxType = MaxType;

            Logger.Info($"{this.GetType().Name} MaxTypeint: {VanillaMaxType - 1}");

            ModManager.Instance.OnModUnloaded += HandleModUnLoaded;

            //List<Type> childTypes = new();
            //foreach (Type t in typesAlreadyProcessed)
            //{
            //    if (t is not null && typeof(TComp).IsAssignableFrom(t))
            //    {
            //        childTypes.Add(t);
            //    }
            //}
            //typesAlreadyProcessed.Add(typeof(TComp));

            Logger.Info($"{this.GetType().Name} loading modded components:");

            //Logger.Info($"Called From: {Environment.StackTrace}");

            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                foreach (Type t in mod.GetType().Assembly.GetTypes())
                {
                    if (ValidTypeCheck<TComp>(t))
                    {
                        if (componentsByMod.ContainsKey(mod))
                        {
                            componentsByMod[mod].Add(t);
                        }
                        else
                        {
                            componentsByMod[mod] = new List<Type>() { t };
                        }
                        Logger.Info($"Loaded {t.Name}");
                        components.Add(t);
                    }
                }
            }
            RebuildCaches();
        }
        protected virtual bool ValidTypeCheck<T>(Type type)
        {
            return typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract;
        }
        protected readonly ComponentReflectionConstructor<TComp> componentFactory = new ComponentReflectionConstructor<TComp>();
        protected virtual bool TryCreateComponent(int SubType, int Level, int SubTypeData, out TComp? comp)
        {
            comp = null;
            if (SubType >= VanillaMaxType)
            {
                int subid = SubType - VanillaMaxType;
                if (subid < 0 || subid >= components.Count)
                {
                    return false;
                }
                Type compType = components[subid];
                try
                {
                    comp = componentFactory.CreateComponent(compType, SubType, Level, (short)SubTypeData);
                    if (comp is not null)
                    {
                        comp.ActualSlotType = (ESlotType)_SlotType;
                        comp.SubType = SubType;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Info($"Failed to create modded component {compType.Name}, {ex}");
                    return false;
                }
            }
            return false;
        }
        public TComp? CreateComponentOfType<T>(int level = 0, short subTypeData = 0) where T : TComp
        {
            int subType = GetIDFromType(typeof(T));
            if (subType == -1)
                return null;
            if (TryCreateComponent(subType, level, subTypeData, out TComp? result))
                return result;
            return null;
        }
        protected virtual void HandleModUnLoaded(PulsarMod? mod)
        {
            if (mod is not null && componentsByMod.ContainsKey(mod))
            {
                List<Type> types = componentsByMod[mod];
                foreach (Type t in types)
                {
                    components.Remove(t);
                }
                componentsByMod.Remove(mod);
            }
            RebuildCaches();
        }
        protected readonly Dictionary<string, int> nameToIdCache = new();
        protected readonly Dictionary<Type, int> typeToIdCache = new();

        protected virtual void RebuildCaches()
        {
            nameToIdCache.Clear();
            typeToIdCache.Clear();
            for (int i = 0; i < components.Count; i++)
            {
                var type = components[i];
                int id = i + VanillaMaxType;
                nameToIdCache.TryAdd(type.Name, id);
                typeToIdCache.TryAdd(type, id);
            }
            componentFactory.RebuildCache(components);
        }

        public virtual int GetIDFromName(string name)
        {
            if (nameToIdCache.TryGetValue(name, out int id))
                return id;

            Logger.Info($"Cache miss on GetIDFromName, {name}");
            return -1;
        }

        public virtual int GetIDFromType(Type type)
        {
            if (typeToIdCache.TryGetValue(type, out int id))
                return id;

            Logger.Info($"Cache miss on GetIDFromType, {type}, {type.Name}");
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
        protected internal ComponentModManager(int SlotType) : base (SlotType, Enum.GetValues(typeof(TEnum)).Length)
        {
        }
    }
    public abstract class LegacyComponentModManager<TComp,TLegacyModComp> : ComponentModManager<TComp> where TComp : PLShipComponent where TLegacyModComp : ComponentModBase
    {
        protected readonly LegacyComponentCompatabilityFactory<TComp, TLegacyModComp> legacyComponentFactory;

        protected internal LegacyComponentModManager(int SlotType, int MaxType) : base (SlotType, MaxType)
        {
            legacyComponentFactory = new LegacyComponentCompatabilityFactory<TComp, TLegacyModComp>((ILGenerator il) => { BaseClassConstructor(il); }, (Dictionary<MethodInfo, MethodInfo> dictionary) => { ModComponentSubtypeMethods(dictionary); });
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                foreach (Type t in mod.GetType().Assembly.GetTypes())
                {
                    if (ValidTypeCheck<TLegacyModComp>(t))
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

                                RebuildCaches();
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
            RebuildCaches();
        }
        protected override void RebuildCaches()
        {
            nameToIdCache.Clear();
            typeToIdCache.Clear();
            legacyModComps.Clear();
            for (int i = 0; i < components.Count; i++)
            {
                var type = components[i];
                var id = i + VanillaMaxType;
                if (legacyComponentFactory.GeneratedTypeToLegacyComp.TryGetValue(type, out TLegacyModComp comp))
                {
                    if (comp is not null)
                    {
                        nameToIdCache.TryAdd(comp.Name, id);
                        legacyModComps.Add(comp);
                        type = comp.GetType();
                    }
                    else
                    {
                        nameToIdCache.TryAdd(type.Name, id);
                        legacyModComps.Add(null);
                    }
                    typeToIdCache.TryAdd(type, id);
                }
                else
                {
                    nameToIdCache.TryAdd(type.Name, id);
                    typeToIdCache.TryAdd(type, id);
                    legacyModComps.Add(null);
                }
                
            }
            componentFactory.RebuildCache(components);
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
        protected override bool TryCreateComponent(int SubType, int Level, int SubTypeData, out TComp? comp)
        {
            bool flag = base.TryCreateComponent(SubType, Level, SubTypeData, out comp);
            if (comp is ILegacyComponent LegacyComp)
            {
                ComponentModConstructor(comp, LegacyComp.GetComponentMod(), SubType, Level, (short)SubTypeData);
            }
            return flag;
        }
        protected readonly List<TLegacyModComp?> legacyModComps = new();
        
    }
    public abstract class LegacyComponentModManager<TComp, TLegacyModComp, TEnum> : LegacyComponentModManager<TComp, TLegacyModComp> where TComp : PLShipComponent where TLegacyModComp : ComponentModBase where TEnum : Enum
    {
        protected internal LegacyComponentModManager(int SlotType) : base(SlotType, Enum.GetValues(typeof(TEnum)).Length)
        {
        }
    }
    public abstract class LegacyInstantiatableComponentModManager<TComp, TLegacyModComp> : ComponentModManager<TComp> where TComp : PLShipComponent
    {
        protected abstract Type GetComponentType(TLegacyModComp comp);
        protected abstract string GetLegacyComponentName(TLegacyModComp comp);
        //Needs to exist as due to how I figure out what legacy mod class binds to the actual component I would otherwise run the constructor for that class.
        //Which for some currently existing components causes an infinite loop as those constructors references its own component mod manager instance which as the constructor didn't finish the instance is null so it creates a new instance so on a so forth
        internal Type? GetConstructorTypeFromMethodBody(MethodInfo method)
        {
            //Extract the IL bytes and the module context
            MethodBody body = method.GetMethodBody();
            byte[] ilBytes = body.GetILAsByteArray();
            Module module = method.Module;

            //Scan the bytes for the 'newobj' opcode (0x73)
            for (int i = 0; i < ilBytes.Length; i++)
            {
                if (ilBytes[i] == 0x73) // 0x73 is the byte representation of 'newobj'
                {
                    // The next 4 bytes make up a 32-bit Integer Metadata Token
                    int token = BitConverter.ToInt32(ilBytes, i + 1);

                    try
                    {
                        // Resolve the token into the actual constructor method info
                        ConstructorInfo ctor = (ConstructorInfo)module.ResolveMethod(token);

                        // Get the parent type of that constructor
                        Type constructedType = ctor.DeclaringType;

                        if (typeof(TComp).IsAssignableFrom(constructedType))
                        {
                            //This has to exist as Runtime types do not match typeof(someclass) exactly
                            constructedType = constructedType.UnderlyingSystemType;
                            return constructedType;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Couldn't resolve the token for that constructor
                        Logger.Info($"Could not resolve token 0x{token:X}: {ex.Message}");
                    }

                    // Skip the 4 token bytes we just read
                    i += 4;
                }
            }
            return null;
        }

        protected internal LegacyInstantiatableComponentModManager(int inSlotType, int MaxType) : base (inSlotType, MaxType)
        {
            foreach (PulsarMod mod in ModManager.Instance.GetAllMods())
            {
                foreach (Type t in mod.GetType().Assembly.GetTypes())
                {
                    if (typeof(TLegacyModComp).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    {
                        //Logger.Info($"Loading {typeof(TMod).Name} from assembly");
                        TLegacyModComp handler = (TLegacyModComp)Activator.CreateInstance(t);
                        Type componentType = GetComponentType(handler);
                        if (!components.Contains(componentType))
                        {
                            if (componentsByMod.ContainsKey(mod))
                            {
                                componentsByMod[mod].Add(componentType);
                            }
                            else
                            {
                                componentsByMod[mod] = new List<Type>() { componentType };
                            }
                            Logger.Info($"Loaded {t.Name}");
                            components.Add(componentType);
                        }
                        if (TypeToLegacyType.TryAdd(componentType, handler))
                        {
                            Logger.Info($"Loaded Legacy Turret Component {GetLegacyComponentName(handler)}");
                        }
                    }
                }
            }
            RebuildCaches();
        }
        protected readonly Dictionary<Type, TLegacyModComp> TypeToLegacyType = new();
        protected readonly List<TLegacyModComp?> legacyModComps = new();
        protected override void RebuildCaches()
        {
            nameToIdCache.Clear();
            typeToIdCache.Clear();
            legacyModComps.Clear();
            for (int i = 0; i < components.Count; i++)
            {
                var type = components[i];
                var id = i + VanillaMaxType;
                if (TypeToLegacyType.TryGetValue(type, out TLegacyModComp comp))
                {
                    if (comp is not null)
                    {
                        nameToIdCache.TryAdd(GetLegacyComponentName(comp), id);
                        legacyModComps.Add(comp);
                    }
                    else
                    {
                        nameToIdCache.TryAdd(type.Name, id);
                        legacyModComps.Add(default);
                    }
                }
                else
                {
                    nameToIdCache.TryAdd(type.Name, id);
                    legacyModComps.Add(default);
                }
                typeToIdCache.TryAdd(type, id);

            }
            componentFactory.RebuildCache(components);
        }
        protected override bool TryCreateComponent(int SubType, int Level, int SubTypeData, out TComp? comp)
        {
            bool flag = base.TryCreateComponent(SubType, Level, SubTypeData, out comp);
            if (comp is not null && TypeToLegacyType.ContainsKey(comp.GetType()))
            {
                comp.SubType = SubType;
                comp.Level = Level;
                comp.SubTypeData = (short)SubTypeData;
            }
            return flag;
        }
    }
    public abstract class LegacyInstantiatableComponentModManager<TComp, TLegacyModComp, TEnum> : LegacyInstantiatableComponentModManager<TComp, TLegacyModComp> where TComp : PLShipComponent where TEnum : Enum
    {
        protected internal LegacyInstantiatableComponentModManager(int inSlotType) : base(inSlotType, Enum.GetValues(typeof(TEnum)).Length)
        {
        }
    }
    public interface ILegacyComponent
    {
        public ComponentModBase GetComponentMod();
    }
    public enum EModdedTurretType
    {
        Normal,
        Mega,
        Auto
    }
    public class TurretType : Attribute
    {
        public EModdedTurretType type { get; set; }
        public TurretType(EModdedTurretType type)
        {
            this.type = type;
        }
    }
}
