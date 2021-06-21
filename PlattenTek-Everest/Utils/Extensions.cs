using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace PlattekMod.Utils {
    internal static class ReflectionExtensions {
        public delegate object GetField(object o);

        public delegate object GetStaticField();

        private const BindingFlags StaticInstanceAnyVisibility =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> CachedFieldInfos = new();
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> CachedPropertyInfos = new();
        private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> CachedMethodInfos = new();

        public static FieldInfo GetFieldInfo(this Type type, string name) {
            if (!CachedFieldInfos.ContainsKey(type)) {
                CachedFieldInfos[type] = new Dictionary<string, FieldInfo>();
            }

            if (!CachedFieldInfos[type].ContainsKey(name)) {
                return CachedFieldInfos[type][name] = type.GetField(name, StaticInstanceAnyVisibility);
            } else {
                return CachedFieldInfos[type][name];
            }
        }

        public static PropertyInfo GetPropertyInfo(this Type type, string name) {
            if (!CachedPropertyInfos.ContainsKey(type)) {
                CachedPropertyInfos[type] = new Dictionary<string, PropertyInfo>();
            }

            if (!CachedPropertyInfos[type].ContainsKey(name)) {
                return CachedPropertyInfos[type][name] = type.GetProperty(name, StaticInstanceAnyVisibility);
            } else {
                return CachedPropertyInfos[type][name];
            }
        }

        public static MethodInfo GetMethodInfo(this Type type, string name) {
            if (!CachedMethodInfos.ContainsKey(type)) {
                CachedMethodInfos[type] = new Dictionary<string, MethodInfo>();
            }

            if (!CachedMethodInfos[type].ContainsKey(name)) {
                return CachedMethodInfos[type][name] = type.GetMethod(name, StaticInstanceAnyVisibility);
            } else {
                return CachedMethodInfos[type][name];
            }
        }

        public static IEnumerable<FieldInfo> GetFieldInfos(this Type type, BindingFlags bindingFlags = StaticInstanceAnyVisibility,
            bool filterBackingField = false) {
            IEnumerable<FieldInfo> fieldInfos = type.GetFields(bindingFlags);
            if (filterBackingField) {
                fieldInfos = fieldInfos.Where(info => !info.Name.EndsWith("k__BackingField"));
            }

            return fieldInfos;
        }
    }

    internal static class CommonExtensions {
        public static T Apply<T>(this T obj, Action<T> action) {
            action(obj);
            return obj;
        }
    }

    internal static class StringExtensions {
        public static bool IsNullOrEmpty(this string text) {
            return string.IsNullOrEmpty(text);
        }

        public static bool IsNotNullOrEmpty(this string text) {
            return !string.IsNullOrEmpty(text);
        }

        public static bool IsNullOrWhiteSpace(this string text) {
            return string.IsNullOrWhiteSpace(text);
        }

        public static bool IsNotNullOrWhiteSpace(this string text) {
            return !string.IsNullOrWhiteSpace(text);
        }
    }

    internal static class EnumerableExtensions {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) {
            return !enumerable.Any();
        }

        public static bool IsNotEmpty<T>(this IEnumerable<T> enumerable) {
            return !enumerable.IsEmpty();
        }

    }
}