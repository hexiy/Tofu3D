# Instanced Rendering System Redesign

## Current State

The instancing system works for the common case but breaks in several edge cases:
toggling renderer components, changing material/mesh at runtime, scene reloads,
and adding/removing components dynamically. This document analyzes the root causes
and proposes targeted changes to make the system more intuitive and robust.

---

## Problem Analysis

### 1. Index-based references are fragile (ROOT CAUSE of most bugs)

The system uses integer indexes into a `List<InstancedGroupDefinition>` that gets
elements removed at runtime:

- `ObjectInstancingData.InstancedRenderingDefinitionIndex` is an index into `_groupDefinitions`
- `ShaderGroup.GroupDefinitionIndexes` is a `List<int>` of indexes into `_groupDefinitions`
- `_sharedInstancedBuffers` is a `Dictionary<int, SharedInstancingBuffer>` keyed by definition index

When `RemoveObject` calls `_groupDefinitions.Remove(...)`, the list compacts and **all
indexes above the removed element shift down by one**. Nothing updates those shifted
indexes in:
- Other `ShaderGroup.GroupDefinitionIndexes` entries
- Other `ObjectInstancingData.InstancedRenderingDefinitionIndex` values

This means after any removal, existing objects can point to the wrong group definition
or to an index beyond the list bounds. The band-aid check at line 147
(`if (shaderGroup.Value.GroupDefinitionIndexes[0] >= _groupDefinitions.Count)`)
just skips broken entries rather than fixing them.

**This is why toggling components, scene reloads, and dynamic add/remove all break.**

### 2. `UpdateObjectData` is a god method

A single method handles add, update, AND removal via a `remove: bool` parameter.
The control flow branches on three independent conditions:
- `InstancedRenderingDefinitionIndex == -1` (no buffer assigned yet)
- `StartingIndexInBuffer == -1` (not added to a buffer yet)
- `remove == true` (should remove instead of add/update)

This creates ~8 code paths through one method, making it hard to reason about.
Each renderer must call it with `remove: true/false` which is not intuitive.

### 3. Enable/disable lifecycle is not handled by the base class

Each renderer subclass must independently remember to:
- Call `UpdateObjectData(remove: true)` in `OnDisabled()`
- Mark `InstancingDataDirty = true` in `OnEnabled()`

`ModelRenderer.OnEnabled()` has this commented out:
```csharp
// ObjectInstancingData.InstancingDataDirty = true;
// ObjectInstancingData.MatrixDirty = true;
```

For static objects, if `InstancingDataDirty` is false on re-enable, the static check
in `UploadRenderData` skips the object entirely. It never gets re-added to the buffer.
**This is the specific toggle bug.**

`TextRenderer` and `ParticleSystemRenderer` each have their own `OnDisabled` logic
for their multiple instancing data entries, duplicating the pattern.

### 4. Material/mesh changes don't trigger group reassignment

When a renderer's `Material` or `RuntimeMesh` changes at runtime, its
`InstancedRenderingDefinitionIndex` still points to the old group. The object stays
in the old buffer with the old material/shader. There's no detection or migration
mechanism. **This is why runtime material/mesh changes break.**

### 5. `SharedInstancingBuffer.RemoveObject` is O(n) and error-prone

Removal shifts all buffer data after the removed slot and updates all subsequent
objects' `StartingIndexInBuffer` values. This is O(n) per removal and touches every
object in the buffer. It also means removal during iteration can cause issues.

### 6. Uniform-setting logic lives in the wrong place

`SetGlobalUniforms` and `SetMaterialSpecificUniforms` (lines 435-673) deal with
lighting, fog, shadows, camera matrices, environment cubemaps, etc. None of this
is specific to instancing. It makes `InstancedRenderingSystem` 861 lines when the
actual instancing logic is maybe 300. This couples the instancing system to every
rendering feature, making both harder to extend.

### 7. `RenderShaderGroups` has hidden pass-type branching

The method checks `CurrentRenderPassType` and branches into different render paths:
- MousePicking: uses `_mousePickingMaterial.Shader`
- Depth passes: uses `_depthMaterial.Shader` or `_depthWithOverrideMaterial.Shader`
- Opaques/UI/Transparency: uses the material's own shader

This is a switch statement spread across the method. Adding a new pass type requires
finding and modifying this method.

### 8. Dead code

- `SharedInstancingBuffer.EmptyStartIndexes` - never populated, never read effectively
- `InstancedRenderingBufferParameters` class - only used in commented-out `StaticGeometryBaker`
- `InstancedRenderingSystem.Reset()` - never called from anywhere
- `StaticGeometryBaker.cs` - entirely commented out
- `SpriteRendererInstanced.cs` - entirely commented out
- `BoxRenderer.cs`, `GradientRenderer.cs`, `SpriteRenderer.cs`, `TextureRenderer.cs` - all commented out

### 9. TextRenderer's dual ObjectInstancingData is confusing

`TextRenderer` inherits from `ModelRenderer`, which creates a single
`ObjectInstancingData` in `Awake()`. But `TextRenderer` uses a
`List<ObjectInstancingData> RendererInstancingDatas` for its per-character instancing.
The inherited single `ObjectInstancingData` is only used as a dirty-flag holder,
never for actual buffer indexing. This is confusing for anyone reading the code.

---

## Design Principles

1. **Direct references over indexes** - Never use list indexes as identity. Use object references.
2. **One method, one job** - Split add/update/remove into separate methods.
3. **Base class handles lifecycle** - `Renderer` handles enable/disable cleanup, not each subclass.
4. **Detect changes, don't require manual notification** - Material/mesh changes should be detected automatically.
5. **Keep it concrete** - No new abstractions unless they earn their keep. This is a game engine, not a framework.
6. **Instancing system = grouping + buffering + drawing** - Uniforms are someone else's job.

---

## Proposed Changes

### Change 1: Replace index-based references with direct object references

**What changes:**

`ObjectInstancingData`:
```csharp
// Before
public int InstancedRenderingDefinitionIndex = -1;
public int StartingIndexInBuffer = -1;

// After
public SharedInstancingBuffer? Buffer;           // null = not in any buffer
public int StartingIndexInBuffer = -1;            // -1 = not yet added to the buffer
```

`ShaderGroup`:
```csharp
// Before
public class ShaderGroup {
    public List<int> GroupDefinitionIndexes = new();
    public Shader Shader;
}

// After
public class ShaderGroup {
    public List<SharedInstancingBuffer> Buffers = new();
    public Shader Shader;
}
```

`InstancedRenderingSystem`:
```csharp
// Before
private List<InstancedGroupDefinition> _groupDefinitions = new();
private Dictionary<int, SharedInstancingBuffer> _sharedInstancedBuffers = new();
private Dictionary<int, ShaderGroup> _shaderGroups = new();

// After
private Dictionary<int, ShaderGroup> _shaderGroups = new();  // keyed by shader ProgramId
// SharedInstancingBuffer owns its own InstancedGroupDefinition
// ShaderGroup owns its list of SharedInstancingBuffer
```

`SharedInstancingBuffer` already has `InstancedGroupDefinition` as a field, so no
change needed there. The `_groupDefinitions` list and `_sharedInstancedBuffers`
dictionary are no longer needed - `ShaderGroup.Buffers` replaces both.

**Why this fixes the core bugs:**

When a buffer is removed (because its last object was removed), we just remove it
from `ShaderGroup.Buffers`. No indexes shift. No other object's reference changes.
Every `ObjectInstancingData` still points to its correct buffer (or null if removed).

### Change 2: Split `UpdateObjectData` into three methods

```csharp
// Called when a renderer needs to be added to the instancing system
// (first time, or after being re-enabled, or after mesh/material change)
public void AddObject(Renderer renderer, ref ObjectInstancingData data,
    Matrix4x4? modelMatrix = null, Vector2? uvOffset = null)

// Called every frame to update the object's buffer data (model matrix, etc.)
// Detects if mesh/material changed and auto-migrates to the correct buffer
public void UpdateObject(Renderer renderer, ref ObjectInstancingData data,
    Matrix4x4? modelMatrix = null, Vector2? uvOffset = null)

// Called when a renderer is disabled or destroyed
public void RemoveObject(ref ObjectInstancingData data)
```

**`AddObject` logic:**
1. Find or create the `ShaderGroup` for the renderer's material shader
2. Find or create a `SharedInstancingBuffer` in that group matching (mesh, material, isStatic)
3. Call `buffer.AddObject(ref data)` which assigns `data.Buffer` and `data.StartingIndexInBuffer`
4. Copy the object's data to the buffer

**`UpdateObject` logic:**
1. If `data.Buffer == null`, call `AddObject` and return
2. Check if `data.Buffer.InstancedGroupDefinition` still matches the renderer's current mesh+material
   - If not: call `RemoveObject(ref data)`, then `AddObject(renderer, ref data, ...)` (migration)
3. Copy the object's updated data to the buffer

**`RemoveObject` logic:**
1. If `data.Buffer == null`, return
2. Call `data.Buffer.RemoveObject(data)` which removes the entry and compacts
3. If buffer is now empty, remove it from its `ShaderGroup.Buffers`
   - If `ShaderGroup.Buffers` is now empty, remove the `ShaderGroup`
4. Set `data.Buffer = null`, `data.StartingIndexInBuffer = -1`

**Renderer call sites become:**
```csharp
// ModelRenderer.UploadRenderData (the common path)
Tofu.InstancedRenderingSystem.UpdateObject(this, ref ObjectInstancingData);

// ModelRenderer.OnDisabled
Tofu.InstancedRenderingSystem.RemoveObject(ref ObjectInstancingData);

// TextRenderer removing a character
Tofu.InstancedRenderingSystem.RemoveObject(ref objectInstancingData);
```

### Change 3: Handle enable/disable in the base `Renderer` class

```csharp
// In Renderer.cs
public override void OnEnabled()
{
    if (ObjectInstancingData != null)
    {
        ObjectInstancingData.InstancingDataDirty = true;
        ObjectInstancingData.MatrixDirty = true;
    }
    base.OnEnabled();  // fires Scene.ComponentEnabled, which adds to render queue
}

public override void OnDisabled()
{
    if (ObjectInstancingData != null)
    {
        Tofu.InstancedRenderingSystem.RemoveObject(ref ObjectInstancingData);
    }
    base.OnDisabled();  // fires Scene.ComponentDisabled, which removes from render queue
}
```

For `TextRenderer` and `ParticleSystemRenderer` which have multiple instancing data
entries, they override `OnDisabled` to clean up their additional entries, then call
`base.OnDisabled()` for the common case:

```csharp
// TextRenderer
public override void OnDisabled()
{
    foreach (var data in RendererInstancingDatas)
    {
        var d = data;
        Tofu.InstancedRenderingSystem.RemoveObject(ref d);
    }
    RendererInstancingDatas.Clear();
    base.OnDisabled();  // handles the inherited ObjectInstancingData
}
```

This eliminates the need for each subclass to remember the cleanup pattern.

### Change 4: Automatic mesh/material change detection in `UpdateObject`

In `UpdateObject`, compare the renderer's current `RuntimeMesh` and `Material` against
`data.Buffer.InstancedGroupDefinition`:

```csharp
bool meshChanged = data.Buffer.InstancedGroupDefinition.RuntimeMesh != renderer.RuntimeMesh;
bool materialChanged = data.Buffer.InstancedGroupDefinition.Material != renderer.Material;

if (meshChanged || materialChanged)
{
    RemoveObject(ref data);
    AddObject(renderer, ref data, modelMatrix, uvOffset);
    return;
}
```

Note: `InstancedGroupDefinition` is a `record`, so the `!=` comparison uses value
equality. But since `RuntimeMesh` and `Asset_Material` are reference types, we should
compare by reference. Using `!=` on the record would compare the fields by value,
which for reference-type fields means reference comparison. This is correct - we want
to detect when the renderer points to a different mesh/material object.

**This fixes the runtime material/mesh change bug without requiring renderers to
manually notify the system.**

### Change 5: Simplify `SharedInstancingBuffer.RemoveObject` with swap-removal

```csharp
public void RemoveObject(ObjectInstancingData removedData)
{
    if (removedData.StartingIndexInBuffer < 0) return;

    int removedIndex = removedData.StartingIndexInBuffer;
    int lastIndex = (NumberOfObjects - 1) * InstancedVertexDataLayoutDefinition.CountOfFloats;

    // If not the last object, copy the last object's data into the removed slot
    if (removedIndex != lastIndex)
    {
        Array.Copy(InstancingBuffer, lastIndex, InstancingBuffer, removedIndex,
            InstancedVertexDataLayoutDefinition.CountOfFloats);

        // Update the moved object's index
        int indexOfMoved = ObjectInstancingDatas.Count - 1;
        var movedData = ObjectInstancingDatas[indexOfMoved];
        movedData.StartingIndexInBuffer = removedIndex;
        ObjectInstancingDatas[indexOfMoved] = movedData;
    }

    // Remove the last entry from the list
    int indexOfRemoved = ObjectInstancingDatas.FindIndex(o => o.Guid == removedData.Guid);
    ObjectInstancingDatas.RemoveAt(indexOfRemoved);

    NumberOfObjects--;
    NeedsUpload = true;
}
```

This is O(1) instead of O(n) and only touches one other object (the one being swapped).
No mass index updates needed.

### Change 6: Move uniform-setting out of `InstancedRenderingSystem`

Extract `SetGlobalUniforms` and `SetMaterialSpecificUniforms` into a separate class:

```csharp
public class InstancingUniformSetter
{
    public void SetGlobalUniforms(Asset_Material material) { ... }
    public void SetMaterialSpecificUniforms(Asset_Material material) { ... }
}
```

`InstancedRenderingSystem` holds a reference to this and calls it during rendering.
The uniform setter deals with lighting, fog, shadows, camera, textures - all the
things that are NOT about instancing.

This makes `InstancedRenderingSystem` focused on its actual job: grouping objects
by (mesh, material, shader), managing GPU buffers, and issuing draw calls. The
uniform logic can be extended independently.

**This is optional but recommended.** It's the biggest refactor and touches the most
code. If you'd rather keep it as-is for now, the other changes still stand on their own.

### Change 7: Clean up dead code

Remove or delete:
- `SharedInstancingBuffer.EmptyStartIndexes` (never populated)
- `InstancedRenderingBufferParameters` class (only used in commented-out code)
- `InstancedRenderingSystem.Reset()` (never called)
- `InstancedRenderingSystem.CopyObjectDataToBuffer(ref float[] buffer, InstancedRenderingBufferParameters)` overload (dead)
- Commented-out files: `StaticGeometryBaker.cs`, `SpriteRendererInstanced.cs`,
  `BoxRenderer.cs`, `GradientRenderer.cs`, `SpriteRenderer.cs`, `TextureRenderer.cs`
  (either delete them or move to an archive folder - they add noise)

### Change 8: Clarify TextRenderer's instancing data

`TextRenderer` inherits `ObjectInstancingData` from `ModelRenderer.Awake()` but only
uses it as a dirty-flag holder. Options:

**Option A (minimal):** Add a comment explaining the inherited `ObjectInstancingData`
is only used for dirty flags, and the actual instancing uses `RendererInstancingDatas`.

**Option B (cleaner):** Make `Renderer.ObjectInstancingData` nullable. `TextRenderer`
overrides `Awake()` to not create it, and uses its own dirty flag mechanism. This
requires changing the `Renderer.Update()` and `Renderer.UpdateModelMatrix()` methods
to null-check `ObjectInstancingData`.

**Recommendation:** Option A for now. Option B is cleaner but touches the base class
and all subclasses. Not worth the risk unless you're already refactoring the base class.

---

## New Architecture Overview

After changes 1-5 (the core changes), the system looks like:

```
InstancedRenderingSystem
├── _shaderGroups: Dictionary<int, ShaderGroup>    (keyed by shader ProgramId)
│   └── ShaderGroup
│       ├── Shader
│       └── Buffers: List<SharedInstancingBuffer>
│           └── SharedInstancingBuffer
│               ├── InstancedGroupDefinition (RuntimeMesh, Material, IsStatic)
│               ├── InstancingBuffer: float[]         (CPU-side instance data)
│               ├── ObjectInstancingDatas: List<ObjectInstancingData>
│               ├── Vbo, Vao, Ebo
│               └── AddObject / RemoveObject / SetupAndUpload
│
├── AddObject(renderer, ref data)                   (find/create buffer, add)
├── UpdateObject(renderer, ref data)                (detect changes, update buffer)
├── RemoveObject(ref data)                          (remove from buffer, cleanup)
└── RenderShaderGroups(InstancingRenderMode)        (iterate groups, draw)
```

```
ObjectInstancingData
├── Guid                    (identity)
├── Buffer: SharedInstancingBuffer?   (direct ref, null = not in system)
├── StartingIndexInBuffer   (-1 = not yet added to buffer)
├── InstancingDataDirty     (needs re-upload)
└── MatrixDirty             (model matrix changed)
```

### Data flow per frame:

```
1. RenderingSystem.RenderAllRenderTargetPipelines()
   ├── Scene.UploadRenderData()                    [ONCE, before any pipeline]
   │   ├── RenderableComponentQueue.UploadRenderDataOpaques()
   │   │   └── foreach renderer: renderer.UploadRenderData()
   │   │       └── InstancedRenderingSystem.UpdateObject()  → writes to CPU buffer
   │   ├── UploadRenderDataTransparency()  (same)
   │   └── UploadRenderDataUI()            (same)
   │
   └── foreach pipeline:
       └── pipeline.RenderAllPasses()
           └── foreach pass:
               └── RenderPass.Render_GL()
                   └── InstancedRenderingSystem.RenderShaderGroups(mode)
                       └── foreach shaderGroup → buffer:
                           ├── buffer.SetupAndUploadIfNeeded()  → uploads to GPU if dirty
                           └── GL.DrawElementsInstanced / DrawArraysInstanced
```

### Lifecycle flow:

```
Component enabled (or awoken):
  → Renderer.OnEnabled()
    → ObjectInstancingData.InstancingDataDirty = true
    → Scene.ComponentEnabled → RenderableComponentQueue adds renderer
    → Next UploadRenderData() → UpdateObject() → AddObject() (because Buffer == null)

Component disabled:
  → Renderer.OnDisabled()
    → InstancedRenderingSystem.RemoveObject(ref data) → data.Buffer = null
    → Scene.ComponentDisabled → RenderableComponentQueue removes renderer

Material/mesh changed at runtime:
  → Next UploadRenderData() → UpdateObject()
    → Detects mesh/material mismatch with current buffer
    → RemoveObject() then AddObject() → migrates to correct buffer

Scene disposed:
  → InstancedRenderingSystem.ClearBuffers() → deletes all GPU resources
```

---

## Migration Path

Do these in order. Each step is independently testable:

1. **Change 5: Swap-removal in SharedInstancingBuffer** - Pure internal change, no API change. Test that add/remove still works.

2. **Change 1: Direct references** - Replace indexes with references. This is the biggest change but also the biggest bug fix. Update `ObjectInstancingData`, `ShaderGroup`, `InstancedRenderingSystem` together. Test toggling, scene reload.

3. **Change 2: Split UpdateObjectData** - Split into Add/Update/Remove. Update all call sites (ModelRenderer, TextRenderer, ParticleSystemRenderer). Test all three renderer types.

4. **Change 3: Base class lifecycle** - Move enable/disable handling to `Renderer`. Remove duplicate code from subclasses. Test toggling specifically.

5. **Change 4: Auto-detect mesh/material changes** - Add comparison in `UpdateObject`. Test runtime material/mesh swaps.

6. **Change 7: Dead code cleanup** - Safe to do anytime, but best after the above to avoid confusion.

7. **Change 6: Extract uniform setter** - Optional. Do this last if at all, since it's the largest refactor with the least bug-fixing value.

8. **Change 8: TextRenderer clarification** - Optional. Add a comment or make ObjectInstancingData nullable.

---

## What NOT to change

- **The upload-once-render-many pattern** - Calling `Scene.UploadRenderData()` once before all pipelines, then having each pass call `RenderShaderGroups`, is correct. The instance data is camera-independent; only uniforms change per pipeline. Keep this.

- **`InstancedVertexDataLayoutDefinition`** - The static layout definition is fine. It's simple and works.

- **The render pass architecture** - `RenderPass` / `RenderTargetPipeline` is clean and extensible. The instancing system plugs into it via `Render_GL()` which is the right seam.

- **`RenderableComponentQueue`** - The opaque/transparent/UI queue split is fine. The enable/disable event subscription pattern works.

- **`InstancingRenderMode` enum** - Simple and sufficient.
