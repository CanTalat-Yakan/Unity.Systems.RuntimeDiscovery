# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# Runtime Discovery

> Quick overview: A tiny runtime utility that centralizes scene discovery and reflection helpers used by other Unity Essentials runtime systems.

Runtime Discovery gives you two things:
1) A consistent way to find all `MonoBehaviour` instances in loaded scenes.
2) Reusable reflection helpers (binding flags, attribute checks, inheritance walking) that other systems can build on (for example: dependency injection, monitoring, inspector tooling).

## Features
- Scene discovery
  - `FindAllMonoBehaviours()` returns all `MonoBehaviour` instances in currently loaded scenes
- Reflection helpers
  - Common `BindingFlags` presets (`InstanceMembers`, `AllMembers`)
  - `HasAttribute(...)` to check attributes on members
  - `IsCompilerGenerated(...)` helper to ignore compiler generated backing fields / lambdas
  - `AnyMemberInHierarchy(...)` to walk a type’s inheritance chain and query fields/properties/methods
- Lightweight
  - Single runtime file, no external dependencies

## Requirements
- Unity 6000.0+
- Works at runtime (no editor API)

## Usage

### Find all MonoBehaviours in loaded scenes
```csharp
using UnityEngine;
using UnityEssentials;

public class PrintAllBehaviours : MonoBehaviour
{
    private void Start()
    {
        var all = RuntimeDiscovery.FindAllMonoBehaviours();
        Debug.Log($"Found {all.Length} behaviours");
    }
}
```

### Check for an attribute on a member
```csharp
using System;
using System.Reflection;
using UnityEngine;
using UnityEssentials;

public class AttributeCheckExample : MonoBehaviour
{
    private void Start()
    {
        var member = typeof(Transform).GetProperty(nameof(Transform.position));
        var hasObsolete = RuntimeDiscovery.HasAttribute(member, typeof(ObsoleteAttribute));
        Debug.Log($"Transform.position is obsolete? {hasObsolete}");
    }
}
```

### Scan inheritance chain for a matching member
Example: Detect if a component type (or any of its base types) contains at least one `[SerializeField]` field.

```csharp
using System;
using System.Reflection;
using UnityEngine;
using UnityEssentials;

public class HierarchyScanExample : MonoBehaviour
{
    private void Start()
    {
        var type = GetType();

        var hasAnySerializedField = RuntimeDiscovery.AnyMemberInHierarchy(
            type,
            predicate: m => m is FieldInfo f && RuntimeDiscovery.HasAttribute(f, typeof(SerializeField)),
            flags: RuntimeDiscovery.InstanceMembers);

        Debug.Log($"{type.Name} has [SerializeField] somewhere in its hierarchy? {hasAnySerializedField}");
    }
}
```

## API Overview
- `RuntimeDiscovery.InstanceMembers`
  - `BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic`
  - Useful for typical runtime “injection” or “populate” flows.

- `RuntimeDiscovery.AllMembers`
  - `BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic`
  - Useful for tooling/inspection/monitoring flows.

- `RuntimeDiscovery.FindAllMonoBehaviours()`
  - Returns `MonoBehaviour[]` from loaded scenes using `Object.FindObjectsByType`.

- `RuntimeDiscovery.HasAttribute(MemberInfo member, Type attributeType, bool inherit = true)`
  - Wrapper around `Attribute.IsDefined`.

- `RuntimeDiscovery.IsCompilerGenerated(MemberInfo member)`
  - Checks `[CompilerGenerated]` so you can skip backing fields and generated methods.

- `RuntimeDiscovery.AnyMemberInHierarchy(Type type, Func<MemberInfo,bool> predicate, BindingFlags flags = AllMembers)`
  - Walks `type`, `type.BaseType`, ... and checks fields, properties, and methods.

## Notes and Limitations
- Discovery scope
  - `FindAllMonoBehaviours()` searches loaded scenes. It won’t include assets/prefabs that aren’t instantiated.
- FindObjectsByType behavior
  - Unity’s object-finding APIs have version-specific behavior regarding inactive objects; this module intentionally keeps the call in one place so it’s easy to adjust if Unity changes behavior.
- Performance
  - Scene discovery and reflection can be expensive if used every frame. Prefer caching results or scanning on load.

## Files in This Package
- `Runtime/RuntimeDiscovery.cs` – Scene discovery + reflection helper utilities
- `Runtime/UnityEssentials.RuntimeDiscovery.asmdef` – Runtime assembly definition

## Tags
unity, runtime, reflection, discovery, scan, utilities
