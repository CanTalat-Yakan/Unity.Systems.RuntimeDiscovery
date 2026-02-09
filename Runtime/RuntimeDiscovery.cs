using System;
using System.Reflection;
using UnityEngine;

namespace UnityEssentials
{
    /// <summary>
    /// Centralized Unity scene discovery + reflection helpers used by runtime systems.
    /// </summary>
    public static class RuntimeDiscovery
    {
        // Public/nonpublic instance members only (typical for injection).
        public const BindingFlags InstanceMembers =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Public/nonpublic, instance+static members (typical for monitoring/inspection).
        public const BindingFlags AllMembers =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Lazily fetched snapshot of all MonoBehaviours.
        /// Cached for the remainder of the current frame, then automatically refreshed next frame.
        /// 
        /// Call <see cref="ResetAllMonoBehavioursCache"/> to force a refetch on next access.
        /// </summary>
        public static MonoBehaviour[] AllMonoBehavioursCached
        {
            get
            {
                // If we already computed this frame, reuse.
                var frame = Time.frameCount;
                if (s_allMonoBehavioursCached != null && s_allMonoBehavioursCachedFrame == frame)
                    return s_allMonoBehavioursCached;

                s_allMonoBehavioursCached = FindAllMonoBehaviours();
                s_allMonoBehavioursCachedFrame = frame;
                return s_allMonoBehavioursCached;
            }
        }

        /// <summary>
        /// Clears the cached MonoBehaviour snapshot (sets it to null). The next access to
        /// <see cref="AllMonoBehavioursCached"/> will refetch.
        /// </summary>
        public static void ResetAllMonoBehavioursCache()
        {
            s_allMonoBehavioursCached = null;
            s_allMonoBehavioursCachedFrame = -1;
        }

        private static MonoBehaviour[] s_allMonoBehavioursCached;
        private static int s_allMonoBehavioursCachedFrame = -1;

        /// <summary>
        /// Finds MonoBehaviours in the current loaded scenes.
        /// Includes inactive objects on supported Unity versions.
        /// </summary>
        public static MonoBehaviour[] FindAllMonoBehaviours() =>
            UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        public static bool HasAttribute(MemberInfo member, Type attributeType, bool inherit = true) =>
            Attribute.IsDefined(member, attributeType, inherit);

        public static bool IsCompilerGenerated(MemberInfo member) =>
            Attribute.IsDefined(member, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: true);

        /// <summary>
        /// Walks the inheritance chain and returns true if any member matches the provided predicate.
        /// </summary>
        public static bool AnyMemberInHierarchy(Type type, Func<MemberInfo, bool> predicate, BindingFlags flags = AllMembers)
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                var fields = t.GetFields(flags);
                for (var i = 0; i < fields.Length; i++)
                    if (predicate(fields[i])) return true;

                var props = t.GetProperties(flags);
                for (var i = 0; i < props.Length; i++)
                    if (predicate(props[i])) return true;

                var methods = t.GetMethods(flags);
                for (var i = 0; i < methods.Length; i++)
                    if (predicate(methods[i])) return true;
            }

            return false;
        }
    }
}