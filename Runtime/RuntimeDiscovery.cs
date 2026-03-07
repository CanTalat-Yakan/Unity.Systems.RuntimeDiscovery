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
        /// A cached collection of all MonoBehaviour instances currently present in the scene.
        /// The collection is lazily refreshed and stored per frame, meaning that its value
        /// is recalculated only when accessed for the first time in a new frame. Subsequent
        /// accesses within the same frame use the cached data instead of recalculating.
        /// </summary>
        /// <remarks>
        /// This property is useful for scenarios where frequent access to all MonoBehaviour
        /// instances is required, as it avoids repeated expensive calls to Unity's object
        /// discovery functions. Ensure to call <see cref="ResetAllMonoBehavioursCache"/>
        /// if manual recalculations are needed before the next frame.
        /// </remarks>
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetAllMonoBehavioursCache()
        {
            s_allMonoBehavioursCached = null;
            s_allMonoBehavioursCachedFrame = -1;
        }

        private static MonoBehaviour[] s_allMonoBehavioursCached;
        private static int s_allMonoBehavioursCachedFrame = -1;

        public static MonoBehaviour[] FindAllMonoBehaviours() =>
            UnityEngine.Object.FindObjectsByType<MonoBehaviour>();

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