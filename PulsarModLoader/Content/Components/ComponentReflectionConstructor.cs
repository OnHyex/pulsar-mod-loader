#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PulsarModLoader.Utilities;

namespace PulsarModLoader.Content.Components
{
    public sealed class ComponentReflectionConstructor<TComp> where TComp : PLShipComponent
    {
        private enum ArgumentType
        {
            SubType,
            Level,
            SubTypeDataShort,
            SubTypeDataInt
        }

        private sealed class ConstructorInfoData
        {
            public ConstructorInfo Constructor { get; }
            public ArgumentType[] ArgumentTypes { get; }

            public ConstructorInfoData(ConstructorInfo constructor, ArgumentType[] argumentTypes)
            {
                Constructor = constructor;
                ArgumentTypes = argumentTypes;
            }
        }

        private readonly Dictionary<string, ConstructorInfoData> ReflectionCache = new();

        public void RebuildCache(List<Type> types)
        {
            //No need to clear cache as all previously contained types even if removed from the manager don't affect the function of the cache and constructor

            foreach (Type t in types)
            {
                if (ReflectionCache.ContainsKey(t.FullName))
                {
                    continue;
                }

                ConstructorInfo? constructor = null;
                ArgumentType[]? argumentTypes = null;

                foreach (ConstructorInfo candidate in t.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (TryBuildArgumentMapping(candidate, out ArgumentType[] mapping))
                    {
                        constructor = candidate;
                        argumentTypes = mapping;
                        break;
                    }
                }

                if (constructor is not null && argumentTypes is not null)
                {
                    ReflectionCache[t.FullName] = new ConstructorInfoData(constructor, argumentTypes);
                }
                else
                {
                    Logger.Info($"Could not find valid locally declared constructor for {t.Name}");
                }
            }
        }

        private static bool TryBuildArgumentMapping(ConstructorInfo constructor, out ArgumentType[] mapping)
        {
            ParameterInfo[] parameters = constructor.GetParameters();

            mapping = new ArgumentType[parameters.Length];

            // Prevent something like:
            // Component(int subType, int subType)
            bool hasSubType = false;
            bool hasLevel = false;
            bool hasSubTypeData = false;

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];

                if (parameter.Name is null)
                {
                    mapping = Array.Empty<ArgumentType>();
                    return false;
                }

                string name = parameter.Name;

                if (name.Equals("inSubType", StringComparison.OrdinalIgnoreCase) || name.Equals("inType", StringComparison.OrdinalIgnoreCase))
                {
                    // subType must be an int
                    if ((parameter.ParameterType != typeof(int) && !parameter.ParameterType.IsEnum) || hasSubType)
                    {
                        mapping = Array.Empty<ArgumentType>();
                        return false;
                    }

                    mapping[i] = ArgumentType.SubType;
                    hasSubType = true;
                }
                else if (name.Equals("inLevel", StringComparison.OrdinalIgnoreCase))
                {
                    // level must be an int
                    if (parameter.ParameterType != typeof(int) || hasLevel)
                    {
                        mapping = Array.Empty<ArgumentType>();
                        return false;
                    }

                    mapping[i] = ArgumentType.Level;
                    hasLevel = true;
                }
                else if (name.Equals("inSubTypeData", StringComparison.OrdinalIgnoreCase))
                {
                    // subTypeData can be either short or int
                    if (hasSubTypeData) 
                    { 
                        mapping = Array.Empty<ArgumentType>(); return false; 
                    }
                    if (parameter.ParameterType == typeof(short))
                    {
                        mapping[i] = ArgumentType.SubTypeDataShort;
                    }
                    else if (parameter.ParameterType == typeof(int))
                    { 
                        mapping[i] = ArgumentType.SubTypeDataInt; 
                    }
                    else
                    { 
                        mapping = Array.Empty<ArgumentType>(); return false; 
                    }
                    hasSubTypeData = true;
                }
                else
                {
                    // Unknown parameter.
                    mapping = Array.Empty<ArgumentType>();
                    return false;
                }
            }

            return true;
        }

        public TComp CreateComponent(Type specificComponentType, int subType, int level, int subTypeData)
        {
            if (!ReflectionCache.TryGetValue(
                    specificComponentType.FullName,
                    out ConstructorInfoData? constructorInfo))
            {
                throw new Exception(
                    $"No Constructor found for {specificComponentType.Name}");
            }

            object[] args = new object[constructorInfo.ArgumentTypes.Length];

            for (int i = 0; i < constructorInfo.ArgumentTypes.Length; i++)
            {
                switch (constructorInfo.ArgumentTypes[i])
                {
                    case ArgumentType.SubType:
                        args[i] = subType;
                        break;

                    case ArgumentType.Level:
                        args[i] = level;
                        break;

                    case ArgumentType.SubTypeDataShort:
                        //Required to create a new object that is specifically short otherwise the c# runtime won't actually cast it to short as it likes keeping things as ints even when specifically cast or requested as shorts
                        short x = (short)subTypeData;
                        args[i] = x;
                        break;
                    case ArgumentType.SubTypeDataInt:
                        int y = (int)subTypeData;
                        args[i] = y;
                        break;
                    default:
                        throw new Exception(
                            $"Unknown argument mapping for {specificComponentType.Name}");
                }
            }
            TComp? comp = (TComp?)constructorInfo.Constructor.Invoke(args);

            if (comp is null)
            {
                throw new Exception(
                    $"Constructor failed for {specificComponentType.Name}");
            }
            return comp;
        }

    }
}
