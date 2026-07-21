# Tofu3D: OpenGL 4.1 → WebGPU Migration Plan

## Table of Contents
1. [Goals & Decisions](#1-goals--decisions)
2. [Current Architecture Overview](#2-current-architecture-overview)
3. [Target Architecture](#3-target-architecture)
4. [Graphics Abstraction Layer Design](#4-graphics-abstraction-layer-design)
5. [WebGPU Backend Design](#5-webgpu-backend-design)
6. [OpenGL Backend (Refactoring Existing Code)](#6-opengl-backend-refactoring-existing-code)
7. [Shader System Migration (GLSL → WGSL)](#7-shader-system-migration-glsl--wgsl)
8. [Windowing & Input](#8-windowing--input)
9. [ImGui Integration](#9-imgui-integration)
10. [WASM Export Pipeline (NativeAOT-LLVM)](#10-wasm-export-pipeline-nativeaot-llvm)
11. [Dependency Migration](#11-dependency-migration)
12. [Audio Migration](#12-audio-migration)
13. [Asset System Changes](#13-asset-system-changes)
14. [Script Compilation Changes](#14-script-compilation-changes)
15. [Migration Phases](#15-migration-phases)
16. [WebGPU Concepts to Learn](#16-webgpu-concepts-to-learn)
17. [Risks & Open Questions](#17-risks--open-questions)

---

## 1. Goals & Decisions

### Goals
- Migrate Tofu3D from OpenGL 4.1 to WebGPU as the primary rendering API
- Enable exporting games as WebAssembly (WASM) that run in browsers via WebGPU
- Keep the editor as a native desktop application
- Still use C# throughout (engine, editor, and exported games)

### Key Decisions

| Decision | Choice | Rationale |
|---|---|---|
| WebGPU bindings | **Silk.NET.WebGPU** | Raw 1:1 bindings over wgpu-native C API. No high-level rendering framework — you write raw WebGPU code. Lets you learn the actual WebGPU API. |
| Backend strategy | **Dual-backend with abstraction** | Create `IGraphicsDevice` / `IBuffer` / `ITexture` etc. interfaces with both OpenGL and WebGPU implementations. Lets you migrate incrementally — editor stays working throughout. |
| WASM approach | **NativeAOT-LLVM + Emscripten** | Best performance, smallest binary. No runtime Roslyn scripting in browser — user scripts are pre-compiled at export time (like Unity). |
| Editor scope | **Native desktop only** | Editor stays as a desktop app. Only exported games target WASM. Avoids ImGui-in-browser, virtual file system, and many browser-specific challenges. |
| Future option | **CoreCLR on WASM (when ready)** | CoreCLR-on-WASM is targeting .NET 11 preview / .NET 12 stable. Architecture should be designed so this can be swapped in later for runtime scripting in browser. |

---

## 2. Current Architecture Overview

### What Exists Today

**Rendering**: OpenGL 4.1 Core Profile via OpenTK 4.7.4. No abstraction layer — `GL.*` calls are spread across ~57 files with ~719 direct OpenGL calls. A `global using GL = OpenTK.Graphics.OpenGL4.GL;` alias is used everywhere.

**Key rendering files and their GL coupling**:
- `Source/OpenGL/Shader.cs` (63 GL calls) — Shader compilation, uniform management
- `Source/Framebuffer.cs` (57 GL calls) — FBO creation, texture attachments
- `Source/BufferFactory.cs` (34 GL calls) — VAO/VBO/EBO creation
- `Source/ImGui/ImGuiController.cs` (146 GL calls) — ImGui rendering backend
- `Source/Components/Renderers/SpriteRenderer.cs` (32 GL calls)
- `Source/Components/Renderers/ModelRenderer.cs` (29 GL calls)
- `Source/Scene/InstancedRenderingSystem.cs` (31 GL calls)
- All `RenderPass*.cs` files (6-14 GL calls each)

**Render pipeline structure** (this is good and worth keeping):
- `RenderingSystem` → manages multiple `RenderTargetPipeline` instances
- `RenderTargetPipeline` → collection of `RenderPass` objects + framebuffers
- `RenderPass` (abstract) → base for: Opaques, Transparency, ZPrePass, Skybox, MousePicking, BloomThreshold, BloomPostProcess, PostProcess, DirectionalLightShadowDepth, PointLightShadowDepth, UI

**Shaders**: 24 GLSL files in `EditorResources/Shaders/` with custom format (`//[VERTEX]`, `//[FRAGMENT]` markers). GLSL 4.10 core. Hot-reloading via `ShaderManager` + `AssetsWatcher`.

**Windowing**: OpenTK `GameWindow` (GLFW-based), OpenGL 4.1 Core context.

**Dependencies** (see [Section 11](#11-dependency-migration) for full migration plan):
- OpenTK 4.7.4, ImGui.NET 1.87.3, BepuPhysics 2.5.0-beta.24, SharpAudio 1.0.65-beta, SixLabors.ImageSharp 2.1.3, Microsoft.CodeAnalysis.CSharp 4.12.0 (Roslyn), Newtonsoft.Json 9.0.1

**Script system**: Runtime C# compilation via Roslyn (Microsoft.CodeAnalysis.CSharp). User scripts are compiled at runtime in the editor.

---

## 3. Target Architecture

### Project Structure (Proposed)

```
Tofu3D/
├── Tofu3D.Engine/              # Engine core (API-agnostic)
│   ├── Rendering/              # Abstraction interfaces
│   │   ├── IGraphicsDevice.cs
│   │   ├── IBuffer.cs
│   │   ├── ITexture.cs
│   │   ├── IShader.cs
│   │   ├── IFramebuffer.cs
│   │   ├── IRenderPipeline.cs
│   │   ├── ICommandBuffer.cs
│   │   └── ...
│   ├── Scene/                  # Scene graph, game objects
│   ├── Components/             # Component system
│   ├── Physics/                # BepuPhysics integration
│   ├── Audio/                  # Audio abstraction
│   ├── Assets/                 # Asset loading (API-agnostic)
│   ├── Input/                  # Input abstraction
│   └── ...
│
├── Tofu3D.Graphics.OpenGL/     # OpenGL backend (refactored from existing code)
│   ├── GLGraphicsDevice.cs
│   ├── GLBuffer.cs
│   ├── GLTexture.cs
│   ├── GLShader.cs
│   ├── GLFramebuffer.cs
│   └── ...
│
├── Tofu3D.Graphics.WebGPU/     # WebGPU backend (new)
│   ├── WGPUGraphicsDevice.cs
│   ├── WGPUBuffer.cs
│   ├── WGPUTexture.cs
│   ├── WGPUShader.cs
│   ├── WGPUFramebuffer.cs
│   └── ...
│
├── Tofu3D.Editor/              # Desktop editor (native, uses either backend)
│   ├── ImGui/                  # ImGui integration
│   ├── EditorPanels/           # Hierarchy, Inspector, SceneView, etc.
│   └── ...
│
├── Tofu3D.Runtime/             # Minimal runtime (for exported games)
│   ├── Program.cs              # WASM entry point
│   └── ...
│
├── Tofu3D.Export/              # Export pipeline
│   ├── ScriptCompiler.cs       # Pre-compile user scripts for WASM
│   ├── AssetPacker.cs          # Package assets for web
│   └── ...
│
└── EditorResources/
    └── Shaders/
        ├── GLSL/               # OpenGL shaders (existing)
        └── WGSL/               # WebGPU shaders (new)
```

### Key Architectural Principles

1. **Engine core knows nothing about OpenGL or WebGPU** — it only talks to interfaces
2. **Render passes use the abstraction layer** — no `GL.*` or `WGPU.*` calls in render pass code
3. **The editor links against both backends** — can switch at runtime (useful for testing)
4. **Exported games link against WebGPU only** — smaller binary, no OpenGL code shipped
5. **Shaders are dual-format** — each shader has both GLSL and WGSL versions; the engine loads the appropriate one based on the active backend

---

## 4. Graphics Abstraction Layer Design

This is the most critical part of the migration. The abstraction must accommodate both OpenGL's state-machine model and WebGPU's explicit command-buffer model. **Lean toward WebGPU's paradigm** — it's the target, and OpenGL can emulate the modern concepts.

### Core Interfaces

```csharp
namespace TofuEngine.Rendering;

/// <summary>
/// The top-level graphics device. Creates resources, submits commands.
/// One instance per backend (GLGraphicsDevice, WGPUGraphicsDevice).
/// </summary>
public interface IGraphicsDevice
{
    // Resource creation
    IBuffer CreateBuffer(BufferDescriptor descriptor);
    ITexture CreateTexture(TextureDescriptor descriptor);
    IShader CreateShader(ShaderDescriptor descriptor);
    IFramebuffer CreateFramebuffer(FramebufferDescriptor descriptor);
    IRenderPipeline CreateRenderPipeline(RenderPipelineDescriptor descriptor);
    ISampler CreateSampler(SamplerDescriptor descriptor);

    // Command submission
    ICommandBuffer BeginCommandBuffer();
    void SubmitCommandBuffer(ICommandBuffer commandBuffer);

    // Swap chain / present
    void Present();

    // Capabilities
    GraphicsDeviceCapabilities Capabilities { get; }
}

/// <summary>
/// Generic buffer for vertex, index, or uniform data.
/// WebGPU maps this to WGPUBuffer. OpenGL maps this to VBO/UBO.
/// </summary>
public interface IBuffer : IDisposable
{
    BufferType Type { get; }          // Vertex, Index, Uniform, Storage
    int SizeInBytes { get; }
    BufferUsage Usage { get; }        // Static, Dynamic, Stream

    void SetData<T>(Span<T> data) where T : struct;
    void SetData(IntPtr data, int sizeInBytes);
}

/// <summary>
/// Texture resource. WebGPU: WGPUTexture + WGPUTextureView.
/// OpenGL: texture object + bind slots.
/// </summary>
public interface ITexture : IDisposable
{
    int Width { get; }
    int Height { get; }
    TextureFormat Format { get; }
    TextureUsageFlags Usage { get; }

    void SetData(IntPtr data, int width, int height, TextureFormat format);
    void GenerateMipmaps();
}

/// <summary>
/// Shader program. WebGPU: WGSL shader module.
/// OpenGL: linked GLSL program.
/// </summary>
public interface IShader : IDisposable
{
    // WebGPU: bind group layouts are derived from the shader
    // OpenGL: uniform locations are queried from the program
    void SetUniform(string name, float value);
    void SetUniform(string name, Vector2 value);
    void SetUniform(string name, Vector3 value);
    void SetUniform(string name, Vector4 value);
    void SetUniform(string name, Matrix4x4 value);
    void SetUniform(string name, int value);
    void SetTexture(string name, ITexture texture, int slot);
}
```

### The Uniform Problem — Important Design Note

OpenGL and WebGPU handle shader uniforms very differently:

- **OpenGL**: Set uniforms by name/location after binding the program. `GL.UniformMatrix4(location, ...)`. Stateful, immediate.
- **WebGPU**: Uniforms are organized into **bind groups** with explicit layouts. You write data into a uniform buffer, then bind the bind group before drawing. No "set uniform by name" at draw time.

**Recommended approach**: The abstraction should expose a **uniform buffer object (UBO)** model rather than per-uniform setting. This maps cleanly to WebGPU and is also supported in OpenGL 3.1+ (which you have via 4.1).

```csharp
/// <summary>
/// A uniform buffer with a defined layout.
/// Both WebGPU and OpenGL support UBOs.
/// This replaces the current "set uniform by name" approach.
/// </summary>
public interface IUniformBuffer : IDisposable
{
    void SetData<T>(ref T data) where T : struct;
    void Bind(int bindingPoint);
}
```

**Migration impact**: The current `Shader.cs` has methods like `SetMatrix4X4`, `SetFloat`, `SetVector3` that call `GL.Uniform*` directly. These need to be replaced with a UBO-based approach where you fill a struct and upload it as a buffer. This is a significant change but necessary for WebGPU compatibility.

### Command Buffer Interface

WebGPU uses explicit command buffers (record commands, then submit). OpenGL is immediate-mode (commands execute as you call them). The abstraction should use the command buffer model — the OpenGL backend can execute commands immediately inside the command buffer methods.

```csharp
public interface ICommandBuffer : IDisposable
{
    IRenderPassEncoder BeginRenderPass(RenderPassDescriptor descriptor);
    void EndRenderPass();
    
    // For compute passes (future)
    IComputePassEncoder BeginComputePass();
    void EndComputePass();
}

public interface IRenderPassEncoder
{
    void SetPipeline(IRenderPipeline pipeline);
    void SetVertexBuffer(int slot, IBuffer buffer);
    void SetIndexBuffer(IBuffer buffer, IndexFormat format);
    void SetUniformBuffer(int bindingPoint, IUniformBuffer buffer);
    void SetTexture(int bindingPoint, ITexture texture, ISampler sampler);
    void SetViewport(int x, int y, int width, int height);
    void SetScissor(int x, int y, int width, int height);
    void Draw(int vertexCount, int instanceCount = 1, int firstVertex = 0, int firstInstance = 0);
    void DrawIndexed(int indexCount, int instanceCount = 1, int firstIndex = 0, int vertexOffset = 0, int firstInstance = 0);
    void End();
}
```

### Render Pipeline Object

WebGPU requires explicit pipeline objects (vertex state, fragment state, depth state, blend state). OpenGL has these as global state. The abstraction should use pipeline objects — the OpenGL backend sets the corresponding GL state when the pipeline is bound.

```csharp
public interface IRenderPipeline : IDisposable
{
    // Opaque handle — backend-specific
}

public class RenderPipelineDescriptor
{
    public IShader Shader { get; set; }
    public VertexLayout[] VertexLayouts { get; set; }
    public PrimitiveTopology Topology { get; set; }
    public BlendState BlendState { get; set; }
    public DepthStencilState DepthStencilState { get; set; }
    public ColorTargetState[] ColorTargets { get; set; }
}
```

### What This Means for Existing Code

Every file that currently calls `GL.*` needs to be refactored to use these interfaces. The general pattern:

**Before (OpenGL direct):**
```csharp
GL.UseShaderProgram(ProgramId);
GL.BindVertexArray(vao);
GL.UniformMatrix4(loc, false, ref matrix);
GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
```

**After (via abstraction):**
```csharp
var cmd = _device.BeginCommandBuffer();
var pass = cmd.BeginRenderPass(renderPassDesc);
pass.SetPipeline(_pipeline);
pass.SetVertexBuffer(0, _vertexBuffer);
pass.SetUniformBuffer(0, _cameraUniformBuffer);
pass.Draw(vertexCount);
pass.End();
_device.SubmitCommandBuffer(cmd);
```

---

## 5. WebGPU Backend Design

### WebGPU Concepts Mapping

| Tofu3D Abstraction | WebGPU (wgpu-native) | Notes |
|---|---|---|
| `IGraphicsDevice` | `WGPUDevice` + `WGPUQueue` + `WGPUSurface` | Created via `wgpuCreateDevice`, surface from window handle |
| `IBuffer` | `WGPUBuffer` | `wgpuDeviceCreateBuffer` with usage flags |
| `ITexture` | `WGPUTexture` + `WGPUTextureView` | `wgpuDeviceCreateTexture` |
| `IShader` | `WGPUShaderModule` | WGSL source compiled via `wgpuDeviceCreateShaderModule` |
| `IFramebuffer` | Render pass color/depth attachments | WebGPU has no persistent FBO — attachments are specified per render pass |
| `IRenderPipeline` | `WGPURenderPipeline` | `wgpuDeviceCreateRenderPipeline` — explicit pipeline state |
| `IUniformBuffer` | `WGPUBuffer` with `Uniform` usage | Bound via bind group |
| `ISampler` | `WGPUSampler` | `wgpuDeviceCreateSampler` |
| `ICommandBuffer` | `WGPUCommandEncoder` + `WGPUCommandBuffer` | Record then submit to queue |
| `IRenderPassEncoder` | `WGPURenderPassEncoder` | `wgpuCommandEncoderBeginRenderPass` |
| Bind groups | `WGPUBindGroup` + `WGPUBindGroupLayout` | Replaces OpenGL's uniform/location binding |

### Key Differences from OpenGL to Handle

1. **No default framebuffer**: WebGPU renders into textures. The swap chain texture is acquired each frame via `wgpuSurfaceGetCurrentTexture`. There's no "bind 0 as framebuffer."

2. **No VAOs**: WebGPU doesn't have vertex array objects. Vertex buffer layout is specified in the render pipeline descriptor, and buffers are bound per-draw.

3. **Bind groups instead of uniforms**: You can't set a uniform by name. You define bind group layouts, create bind groups with specific resources, and bind them before drawing. This requires pre-declaring what resources a shader needs.

4. **Pipeline objects are immutable**: In OpenGL, you can change blend mode, depth test, etc. at any time. In WebGPU, these are baked into the pipeline object. Changing blend mode = creating a new pipeline. **Design your renderer to cache pipelines by state hash.**

5. **No glEnable/glDisable**: State like depth testing, blending, face culling is all in the pipeline descriptor, not toggled globally.

6. **Texture format differences**: WebGPU uses `WGPUTextureFormat` enum (e.g., `BGRA8Unorm` vs OpenGL's `RGBA8`). The swap chain format may differ from what you're used to.

### WebGPU Surface Creation (Desktop)

For the desktop editor, you need to create a WebGPU surface from the native window. With Silk.NET.WebGPU + OpenTK's GLFW window:

```csharp
// Pseudocode — get the native window handle from OpenTK
var windowHandle = Tofu.Window.NativeWindow.Handle; // GLFWwindow*

// Create WebGPU surface from GLFW window
// wgpu-native provides wgpuCreateSurfaceFrom* functions per platform
var surface = WGPU.CreateSurfaceFromGLFW(instance, windowHandle);
```

**Note**: You'll need platform-specific surface creation (Win32 `HWND`, macOS `NSWindow`, X11/Wayland). wgpu-native provides helpers for GLFW windows which simplify this.

### WebGPU Surface Creation (WASM/Browser)

In the browser, WebGPU gets the canvas from JavaScript. The WASM module receives a JS canvas handle and creates a surface from it. This is a completely different code path from desktop:

```csharp
// In WASM, via JS interop:
// 1. JS gets the canvas element: const canvas = document.getElementById("canvas");
// 2. JS gets the WebGPU context: const context = canvas.getContext("webgpu");
// 3. JS passes the context to C# via interop
// 4. C# creates a surface from the browser-provided handle
```

This means the `WGPUGraphicsDevice` constructor needs two paths: desktop (from native window handle) and WASM (from JS canvas context).

---

## 6. OpenGL Backend (Refactoring Existing Code)

### Strategy: Wrap, Don't Rewrite

The OpenGL backend should wrap the existing OpenGL code behind the abstraction interfaces. This is mostly mechanical refactoring:

1. **`GLShader` implements `IShader`** — wraps the existing `Shader.cs` class. Uniform setting methods call `GL.Uniform*` as before.
2. **`GLFramebuffer` implements `IFramebuffer`** — wraps the existing `Framebuffer.cs` class.
3. **`GLBuffer` implements `IBuffer`** — wraps VBO/EBO creation from `BufferFactory.cs`.
4. **`GLTexture` implements `ITexture`** — wraps texture creation from `TextureLoader.cs`.
5. **`GLCommandBuffer` implements `ICommandBuffer`** — executes GL commands immediately (no actual buffering).
6. **`GLRenderPassEncoder` implements `IRenderPassEncoder`** — maps to GL state changes + draw calls.
7. **`GLRenderPipeline` implements `IRenderPipeline`** — stores pipeline state, applies it on bind.

### The Command Buffer "Fake" for OpenGL

OpenGL is immediate-mode, so `ICommandBuffer` for OpenGL just executes commands right away:

```csharp
public class GLCommandBuffer : ICommandBuffer
{
    public IRenderPassEncoder BeginRenderPass(RenderPassDescriptor descriptor)
    {
        // Immediately bind the framebuffer, set clear color, clear
        var encoder = new GLRenderPassEncoder(descriptor);
        return encoder;
    }
    
    public void EndRenderPass() { /* no-op for GL */ }
    public void Dispose() { /* no-op for GL */ }
}
```

### VAO Management

OpenGL needs VAOs; WebGPU doesn't. The `GLRenderPipeline` can own a VAO that's created from the `VertexLayout` in the pipeline descriptor. When `SetVertexBuffer` is called, it binds into the VAO. This keeps VAO management inside the GL backend.

---

## 7. Shader System Migration (GLSL → WGSL)

### The Challenge

You have 24 GLSL shaders that need WGSL equivalents. GLSL and WGSL are very different languages:

| Aspect | GLSL | WGSL |
|---|---|---|
| Version directive | `#version 410 core` | None (implicit) |
| Entry points | `void main()` | `@vertex` / `@fragment` annotations |
| In/out variables | `in vec3 pos;` / `out vec4 color;` | `@location(0) in pos : vec3<f32>;` |
| Uniforms | `uniform mat4 mvp;` | Bind group entries: `@group(0) @binding(0) var<uniform> mvp: mat4x4<f32>;` |
| Texture sampling | `texture2D(tex, uv)` | `textureSample(tex, sampler, uv)` |
| Types | `vec3`, `mat4` | `vec3<f32>`, `mat4x4<f32>` |
| Output | `gl_FragColor` | `@location(0) out color : vec4<f32>;` |

### Strategy: Manual Conversion + Dual Shader Files

**Do not use a transpiler.** Since you want to learn WebGPU, manually convert each shader. This gives you full control and understanding. Maintain both GLSL and WGSL versions:

```
EditorResources/Shaders/
├── GLSL/
│   ├── ModelRenderer.glsl
│   ├── SpriteRenderer.glsl
│   └── ...
└── WGSL/
    ├── ModelRenderer.wgsl
    ├── SpriteRenderer.wgsl
    └── ...
```

### Shader Format Changes

Your current custom format uses `//[VERTEX]` and `//[FRAGMENT]` markers. For WGSL, you don't need to split vertex/fragment — WGSL uses `@vertex` and `@fragment` annotations in a single file. However, for the abstraction to work with both backends, consider:

**Option A: Keep custom format for both**
```
//[VERTEX]
@vertex
fn vs_main(@location(0) pos: vec3<f32>) -> @builtin(position) vec4<f32> {
    ...
}
//[FRAGMENT]
@fragment
fn fs_main(@location(0) uv: vec2<f32>) -> @location(0) vec4<f32> {
    ...
}
```

**Option B: WGSL files are standard WGSL (no markers), GLSL keeps current format**
The shader loader detects `.wgsl` vs `.glsl` and parses accordingly.

**Recommendation**: Option B — let WGSL files be standard WGSL. The `IShader` implementation for each backend handles parsing differently. This keeps WGSL clean and standard.

### Uniform/Bind Group Layout Declaration

This is the biggest change. In GLSL, you just declare `uniform mat4 mvp;` and set it by name. In WGSL, you must declare bind groups explicitly:

```wgsl
// WGSL
@group(0) @binding(0) var<uniform> camera: CameraUniform;
@group(0) @binding(1) var tex: texture_2d<f32>;
@group(0) @binding(2) var sampler: sampler;

struct CameraUniform {
    viewProj: mat4x4<f32>,
    viewPos: vec3<f32>,
};
```

You'll need to define a **bind group layout convention** for Tofu3D. For example:
- **Group 0**: Global/frame data (camera, time, screen size)
- **Group 1**: Material data (textures, material uniforms)
- **Group 2**: Object data (model matrix, object ID)

This convention must be consistent across all shaders. The OpenGL backend maps these to uniform buffer binding points.

### Shader Hot-Reloading

The current `ShaderManager` + `AssetsWatcher` system can be kept. It just needs to:
1. Watch both `.glsl` and `.wgsl` files
2. Reload via the appropriate `IShader` implementation based on the active backend
3. Recreate pipeline objects (since WebGPU pipelines are immutable and reference shaders)

---

## 8. Windowing & Input

### Desktop (Editor + Native Game Export)

**Current**: OpenTK `GameWindow` (GLFW-based) with OpenGL context.

**Options**:

**Option A: Keep OpenTK for windowing, add Silk.NET.WebGPU for WebGPU only**
- OpenTK's `GameWindow` creates an OpenGL context. For the WebGPU backend, you'd create a WebGPU surface from the same GLFW window handle.
- Pro: Minimal windowing changes. Existing input code mostly works.
- Con: You're always creating an OpenGL context even when using WebGPU (wasteful but harmless). Two windowing/graphics dependencies.
- Con: OpenTK's GameWindow forces GL context creation. You'd need to use OpenTK's `NativeWindow` (no GL context) when running WebGPU-only.

**Option B: Switch to Silk.NET.Windowing (GLFW) for both backends**
- Silk.NET.Windowing wraps GLFW and doesn't force a GL context.
- You create either a GL context or a WebGPU surface depending on the backend.
- Pro: Unified windowing. No forced GL context. Silk.NET already provides WebGPU bindings.
- Con: Need to port windowing code from OpenTK to Silk.NET.Windowing. Input handling changes.

**Option C: Use raw GLFW via Silk.NET.GLFW**
- Maximum control, minimal abstraction.
- Pro: Full control, learn the windowing layer.
- Con: More boilerplate for window management and input.

**Recommendation**: **Option B (Silk.NET.Windowing)** — it unifies windowing under Silk.NET, doesn't force a GL context, and Silk.NET already provides the WebGPU bindings you're using. The porting effort for windowing is small compared to the rendering migration.

### Input

Port `MouseInput.cs` and `KeyboardInput.cs` to use Silk.NET.Windowing's input system (or Silk.NET.Input). The current code uses OpenTK's `MouseState`/`KeyboardState` — Silk.NET has equivalent APIs.

### WASM (Browser)

In WASM, there's no native window. The "window" is an HTML `<canvas>` element. Input comes from browser events (mouse, keyboard, touch). You'll need:
- JS interop to get canvas and create WebGPU surface
- JS interop to receive input events (or use Silk.NET's WASM input support if available)
- Touch input support (mobile browsers)

---

## 9. ImGui Integration

### Current State
`ImGuiController.cs` has 146 direct `GL.*` calls — it's a custom OpenGL ImGui renderer backend.

### Migration Approach

The editor stays native desktop, so ImGui doesn't need to work in WASM. But it does need to work with both OpenGL and WebGPU backends.

**Options**:

1. **Create an `IImGuiRenderer` interface** with `GLImGuiRenderer` (existing code, refactored) and `WGPUImGuiRenderer` (new). The editor uses whichever matches the active backend.

2. **Use Silk.NET's ImGui integration** — Silk.NET has `Silk.NET.OpenGL.ImGui` and potentially WebGPU ImGui bindings. But this might be a higher-level wrapper than you want.

3. **Use the official ImGui WebGPU backend** (imgui_impl_wgpu) via P/Invoke or Silk.NET bindings.

**Recommendation**: Option 1 — create an `IImGuiRenderer` interface. Keep the existing GL ImGui renderer as `GLImGuiRenderer`. Write a new `WGPUImGuiRenderer` using raw WebGPU calls (this is a good learning exercise for WebGPU — rendering textured quads with a pipeline).

**Note**: The ImGui rendering is self-contained (it generates its own draw lists, vertex data, and textures). It doesn't go through your render pass system. This makes it a good isolated first target for WebGPU implementation.

---

## 10. WASM Export Pipeline (NativeAOT-LLVM)

### Overview

When a user clicks "Export to Web" in the editor, the export pipeline:

1. **Compiles user scripts** → produces a game assembly (DLL) using Roslyn (desktop-only)
2. **Packages assets** → bundles all needed assets into a web-friendly format
3. **AOT-compiles** the runtime + game assembly → WASM via NativeAOT-LLVM + Emscripten
4. **Generates HTML/JS shell** → loads the WASM module, creates canvas, initializes WebGPU

### NativeAOT-LLVM Setup

```xml
<!-- Tofu3D.Runtime.csproj (for WASM export) -->
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RuntimeIdentifier>browser-wasm</RuntimeIdentifier>
    <OutputType>Exe</OutputType>
    <PublishAot>true</PublishAot>
    <PublishReadyToRun>true</PublishReadyToRun>
    <!-- Use NativeAOT-LLVM for WASM -->
    <UseNativeAotLlvm>true</UseNativeAotLlvm>
</PropertyGroup>
```

**Note**: NativeAOT-LLVM for WASM is still being upstreamed (PR #126633 in dotnet/runtime). You may need to use the experimental `feature/NativeAOT-LLVM` branch or the `dotnet-experimental` NuGet feed initially. Track this PR for when it's merged into a stable release.

### NativeAOT Constraints

NativeAOT has important constraints that affect engine design:

1. **No reflection.emit / DynamicMethod** — No runtime code generation. All code must be known at compile time.
2. **Limited reflection** — Some reflection is supported but trimmed aggressively. Use `[DynamicDependency]` or `[DynamicallyAccessedMembers]` attributes where needed.
3. **No `Microsoft.CodeAnalysis.CSharp` (Roslyn)** — Cannot compile C# at runtime. This is why scripts must be pre-compiled at export time.
4. **Trimming** — Unused code is removed. Be careful with reflection-based serialization. Consider `System.Text.Json` with source generators instead of `Newtonsoft.Json`.
5. **AOT compatibility analysis** — Run `dotnet publish` with `PublishAot=true` early and often to catch AOT-incompatible code.

### Script Pre-Compilation at Export

```csharp
// In Tofu3D.Export/ScriptCompiler.cs (desktop editor only)
public class ScriptCompiler
{
    public byte[] CompileScripts(string scriptsFolder)
    {
        // Use Roslyn to compile all .cs files in the scripts folder
        // into a single assembly DLL
        // Reference Tofu3D.Engine.dll and all engine dependencies
        // Return the compiled assembly bytes
        
        // This assembly gets embedded into the WASM build
        // and loaded via Assembly.Load at runtime
    }
}
```

**Important**: Even though Roslyn can't run in WASM, the compiled assembly CAN be loaded and executed. `Assembly.Load(byte[])` works in NativeAOT for pre-compiled assemblies (verify this — it may require specific AOT settings). If not, scripts must be compiled into the main assembly at export time rather than loaded dynamically.

**Alternative approach**: At export time, merge user scripts into the runtime project source and compile everything together as one WASM binary. This avoids runtime assembly loading entirely. Simpler, more reliable for AOT.

### HTML/JS Shell

The exported game needs an HTML page that:
1. Loads the WASM module (via `dotnet.js` loader)
2. Creates a `<canvas>` element
3. Gets `WebGPU` context from canvas
4. Passes canvas/WebGPU context to C# via JS interop
5. Handles input events (mouse, keyboard, touch) and forwards to C#
6. Handles resize events

```html
<!-- Generated index.html (simplified) -->
<!DOCTYPE html>
<html>
<head><title>Tofu3D Game</title></head>
<body>
    <canvas id="game-canvas"></canvas>
    <script type="module">
        import { dotnet } from "./dotnet.js";
        const { setModuleImports, getAssemblyExports } = await dotnet.create();
        
        const canvas = document.getElementById("game-canvas");
        const context = canvas.getContext("webgpu");
        
        // Pass canvas to C# engine
        setModuleImports("tofu", {
            getCanvas: () => canvas,
            getWebGPUContext: () => context,
            getCanvasWidth: () => canvas.width,
            getCanvasHeight: () => canvas.height,
        });
        
        // Start the game
        await getAssemblyExports("Tofu3D.Runtime").Program.Main();
    </script>
</body>
</html>
```

### Asset Packaging for Web

Assets need to be fetchable by the WASM module. Options:
- **Embed assets in the WASM binary** — simplest, but large binary and no streaming
- **Serve assets as separate files** — fetched via HTTP at runtime. Need a virtual file system abstraction that can read from HTTP or local disk.
- **Package as a single .zip/.tar** — fetched once, extracted in memory

**Recommendation**: Serve as separate files with a virtual file system. The `AssetLoadManager` already abstracts loading — add an `IFileSystem` interface with `PhysicalFileSystem` (desktop) and `HttpFileSystem` (WASM) implementations.

---

## 11. Dependency Migration

### Dependencies That Stay (Desktop Editor)

| Package | Status | Notes |
|---|---|---|
| ImGui.NET 1.87.3 | Keep (desktop only) | Editor UI. Not needed in WASM game export. |
| BepuPhysics 2.5.0-beta.24 | Keep | Pure C#, should be AOT-compatible. Verify. |
| SixLabors.ImageSharp 2.1.3 | Keep | Image loading. Should be AOT-compatible. |
| LibNoise 0.2.0 | Keep | Pure C#, AOT-compatible. |
| RectpackSharp 1.2.0 | Keep | Pure C#, AOT-compatible. |
| Microsoft.CodeAnalysis.CSharp 4.12.0 | Keep (desktop only) | Roslyn for runtime scripting in editor + script pre-compilation at export. NOT in WASM. |
| Microsoft.Build.* | Keep (desktop only) | Build system. NOT in WASM. |
| NativeFileDialogSharp 0.6.0-alpha | Keep (desktop only) | File dialogs. NOT in WASM. |

### Dependencies That Change

| Package | Action | Replacement |
|---|---|---|
| OpenTK 4.7.4 | **Replace with Silk.NET** | Silk.NET.Windowing + Silk.NET.Input for windowing/input. Silk.NET.OpenGL for OpenGL backend. |
| OpenTK.redist.glfw | **Remove** | Silk.NET.Windowing includes GLFW redistribution. |
| Newtonsoft.Json 9.0.1 | **Replace with System.Text.Json** | Newtonsoft.Json has poor AOT compatibility (heavy reflection). System.Text.Json with source generators is AOT-friendly. This is a significant serialization refactor. |
| System.Drawing.Common 8.0.0 | **Remove** | Not available on WASM. Use SixLabors.ImageSharp exclusively. |

### Dependencies That Need WASM Alternatives

| Package | Issue | WASM Solution |
|---|---|---|
| SharpAudio 1.0.65-beta | Uses native audio APIs. Not WASM-compatible. | Web Audio API via JS interop. Create `IAudioEngine` interface with `SharpAudioEngine` (desktop) and `WebAudioEngine` (WASM). |
| Silk.NET.WebGPU (wgpu-native) | Native library, needs WASM build. | wgpu-native compiled to WASM via Emscripten, or use browser's native WebGPU via JS interop. **This is a critical path — see below.** |

### The wgpu-native WASM Question

`wgpu-native` is a Rust library. For WASM, there are two approaches:

1. **Compile wgpu-native to WASM via Emscripten** — produces a WASM module that wraps wgpu. Your C# code (also WASM) calls into it. This is complex but gives you the same API on desktop and WASM.

2. **Use browser's native WebGPU directly via JS interop** — the browser already implements WebGPU. Your C# WASM code calls JavaScript WebGPU API via interop. No wgpu-native needed in WASM. But this means your WebGPU backend has two code paths: wgpu-native (desktop) and JS interop (WASM).

3. **Use Silk.NET's WASM WebGPU support** — if Silk.NET.WebGPU supports WASM targets (it may, via Emscripten bindings to wgpu). Check Silk.NET's WASM support status.

**Recommendation**: Research Silk.NET's WASM support first. If it works, use it for both desktop and WASM. If not, use approach 2 (JS interop for WASM, wgpu-native for desktop) — this means the `WGPUGraphicsDevice` has a desktop path and a WASM path internally, but the interface is the same.

**This is one of the biggest open questions — see [Section 17](#17-risks--open-questions).**

---

## 12. Audio Migration

### Current: SharpAudio
Uses native audio APIs (miniaudio). Not WASM-compatible.

### Proposed: Audio Abstraction

```csharp
public interface IAudioEngine
{
    IAudioSource CreateSource(AudioClip clip);
    void Play(IAudioSource source);
    void SetVolume(IAudioSource source, float volume);
    void Set3DPosition(IAudioSource source, Vector3 position);
    // ...
}

// Desktop: wraps SharpAudio
public class SharpAudioEngine : IAudioEngine { ... }

// WASM: uses Web Audio API via JS interop
public class WebAudioEngine : IAudioEngine { ... }
```

Audio is lower priority than rendering — can be stubbed in WASM initially and implemented later.

---

## 13. Asset System Changes

### Current
- `AssetImportManager` watches Assets folder, converts to `.tofuasset` files
- `AssetLoadManager` loads `.tofuasset` files at runtime
- Uses `Newtonsoft.Json` for serialization
- Uses `SixLabors.ImageSharp` for image loading
- Uses `System.Drawing.Common` in some places

### Changes Needed

1. **Replace Newtonsoft.Json with System.Text.Json** — for AOT compatibility. This affects all serialization code (`Serialization/` folder, `SceneSerializer`, asset importers/loaders). Use source generators for polymorphic serialization.

2. **Remove System.Drawing.Common** — replace any usage with ImageSharp equivalents.

3. **Add `IFileSystem` abstraction** — for WASM asset loading:
   ```csharp
   public interface IFileSystem
   {
       Stream OpenRead(string path);
       bool Exists(string path);
       string[] GetFiles(string path, string searchPattern);
       // ...
   }
   
   public class PhysicalFileSystem : IFileSystem { ... }  // Desktop
   public class HttpFileSystem : IFileSystem { ... }      // WASM (fetch via HTTP)
   public class EmbeddedFileSystem : IFileSystem { ... }  // Assets embedded in binary
   ```

4. **Texture upload via abstraction** — `TextureLoader.cs` currently calls `GL.TexImage2D` directly. This needs to go through `ITexture.SetData()`.

---

## 14. Script Compilation Changes

### Current (Editor)
- User writes C# scripts in `Scripts/` folder
- `Microsoft.CodeAnalysis.CSharp` (Roslyn) compiles them at runtime
- Scripts are loaded as an in-memory assembly
- Hot-reloading when script files change

### Editor (No Change)
The editor keeps using Roslyn for runtime scripting. This is the development experience — fast iteration with hot-reload.

### WASM Export (New)
At export time:
1. Collect all user script `.cs` files
2. Compile them into a game assembly using Roslyn (on desktop)
3. Either:
   - **Embed the game assembly** in the WASM build and `Assembly.Load` it at runtime, OR
   - **Merge scripts into the runtime project** and compile everything as one WASM binary (recommended for AOT)
4. The exported game has no Roslyn dependency — scripts are pre-compiled

### Script API Constraints for WASM
User scripts that will be exported to WASM must be AOT-compatible:
- No `System.Reflection.Emit`
- No `DynamicMethod`
- Limited reflection (use `[DynamicallyAccessedMembers]` where needed)
- No `Newtonsoft.Json` (use `System.Text.Json`)

The engine API should guide users toward AOT-compatible patterns. Consider adding an analyzer that warns about AOT-incompatible code in user scripts.

---

## 15. Migration Phases

### Phase 0: Preparation (No rendering changes)
- [ ] Split project into `Tofu3D.Engine`, `Tofu3D.Editor` assemblies (or at least logical folders)
- [ ] Replace `Newtonsoft.Json` with `System.Text.Json` + source generators
- [ ] Remove `System.Drawing.Common` usage
- [ ] Add `IFileSystem` abstraction (keep using physical FS only)
- [ ] Verify BepuPhysics AOT compatibility with a test publish
- [ ] Set up Silk.NET.Windowing + Silk.NET.Input (replace OpenTK windowing)
- [ ] Verify the editor still works with Silk.NET windowing + OpenTK OpenGL

### Phase 1: Graphics Abstraction Layer
- [ ] Design and implement all interfaces (`IGraphicsDevice`, `IBuffer`, `ITexture`, etc.)
- [ ] Implement `GLGraphicsDevice` and all GL backend classes by wrapping existing code
- [ ] Refactor all render passes to use the abstraction instead of `GL.*` directly
- [ ] Refactor all component renderers (ModelRenderer, SpriteRenderer, etc.) to use abstraction
- [ ] Refactor ImGui rendering to use `IImGuiRenderer` interface
- [ ] **Verify: Editor works identically to before, now through the abstraction layer**

### Phase 2: WebGPU Backend (Desktop)
- [ ] Add Silk.NET.WebGPU NuGet package
- [ ] Implement `WGPUGraphicsDevice` — device, queue, surface creation from desktop window
- [ ] Implement `WGPUBuffer`, `WGPUTexture`, `WGPUShader`, `WGPUFramebuffer`, `WGPURenderPipeline`
- [ ] Implement `WGPUCommandBuffer` and `WGPURenderPassEncoder`
- [ ] Implement `WGPUImGuiRenderer` (good first test — isolated rendering)
- [ ] Convert first shader to WGSL (start with a simple one like `BoxRenderer.glsl`)
- [ ] Get a single triangle/box rendering with WebGPU on desktop
- [ ] Progressively convert remaining 23 shaders to WGSL
- [ ] Implement WebGPU versions of all render passes
- [ ] **Verify: Editor works with WebGPU backend on desktop**

### Phase 3: Dual-Backend Testing
- [ ] Add runtime backend switching (e.g., command-line flag or editor setting)
- [ ] Test all features with both OpenGL and WebGPU backends
- [ ] Fix discrepancies between backends
- [ ] Performance comparison and optimization

### Phase 4: WASM Export Pipeline
- [ ] Set up NativeAOT-LLVM + Emscripten build pipeline
- [ ] Create `Tofu3D.Runtime` project (minimal runtime without editor)
- [ ] Implement script pre-compilation at export time
- [ ] Implement asset packaging for web
- [ ] Implement `HttpFileSystem` for WASM asset loading
- [ ] Implement WebGPU surface creation from browser canvas (JS interop)
- [ ] Implement WASM input handling (mouse, keyboard, touch via JS interop)
- [ ] Generate HTML/JS shell for exported games
- [ ] **Verify: A simple game exports and runs in a browser**

### Phase 5: Polish & Feature Parity
- [ ] Audio abstraction + Web Audio API for WASM
- [ ] Touch input support for mobile browsers
- [ ] Asset streaming / lazy loading for web
- [ ] WebGPU-specific optimizations (e.g., pipeline caching, bind group pooling)
- [ ] Error handling and debugging for WASM builds
- [ ] Size optimization (trimming, compression) for WASM binary

---

## 16. WebGPU Concepts to Learn

As you implement the WebGPU backend, these are the key concepts you'll learn:

### Core Concepts
1. **Adapter and Device** — `wgpuRequestAdapter` → `wgpuAdapterRequestDevice`. The adapter is the physical GPU; the device is your logical connection to it.
2. **Queue** — All command buffers are submitted to the device's queue. `wgpuQueueSubmit`.
3. **Surface and Swap Chain** — `wgpuSurfaceGetCurrentTexture` gets the current frame's texture. Configure format, present mode.
4. **Buffers** — Created with usage flags (`Vertex`, `Index`, `Uniform`, `Storage`, `MapWrite`, `MapRead`). Written via `wgpuQueueWriteBuffer` or mapped.
5. **Textures and Texture Views** — `WGPUTexture` is the resource; `WGPUTextureView` is how you bind it. Created with dimension, format, usage flags.
6. **Samplers** — Filtering and addressing modes. Separate from textures (unlike OpenGL's sampler objects which are optional).
7. **Bind Groups and Bind Group Layouts** — The replacement for OpenGL uniforms. A layout describes the structure; a bind group binds actual resources.
8. **Pipeline Layout** — Collection of bind group layouts. Used to create render pipelines.
9. **Render Pipelines** — Immutable object containing vertex/fragment shaders, vertex layouts, blend state, depth state, primitive topology.
10. **Command Encoders and Pass Encoders** — Record commands into a command buffer. `WGPUCommandEncoder` → `WGPURenderPassEncoder` → submit.
11. **Shader Modules** — WGSL source compiled to a `WGPUShaderModule`.

### WebGPU vs OpenGL Mental Model Shift
- OpenGL: "Set state, then draw. State persists."
- WebGPU: "Create objects upfront. Record commands into a buffer. Submit the buffer. Everything is explicit."

### Recommended Learning Resources
- [webgpufundamentals.org](https://webgpufundamentals.org) — JS-focused but concepts are the same
- [wgpu-native documentation](https://github.com/gfx-rs/wgpu-native) — the C API you'll be calling
- [WebGPU specification](https://www.w3.org/TR/webgpu/) — the authoritative reference
- [WGSL specification](https://www.w3.org/TR/WGSL/) — the shading language
- Silk.NET.WebGPU examples and tests

---

## 17. Risks & Open Questions

### High Risk

1. **wgpu-native on WASM**: Can wgpu-native (via Silk.NET.WebGPU) be compiled to WASM and called from C# WASM code? Or do you need JS interop to the browser's native WebGPU? **This needs to be validated early in Phase 4.** If wgpu-native doesn't work in WASM, the `WGPUGraphicsDevice` needs a JS interop code path for browser WebGPU.

2. **NativeAOT-LLVM maturity**: NativeAOT-LLVM for WASM is being upstreamed (PR #126633). It may have bugs or missing features. You may need to use experimental builds. Track the dotnet/runtime PR status.

3. **AOT compatibility of existing code**: The codebase uses reflection, Newtonsoft.Json, runtime Roslyn — all problematic for AOT. The serialization migration to System.Text.Json + source generators is significant work. Run `dotnet publish -r browser-wasm` early to discover AOT issues.

4. **Silk.NET.Windowing migration**: Porting from OpenTK's `GameWindow` to Silk.NET.Windowing may have edge cases (high-DPI, multi-monitor, fullscreen, etc.). Test thoroughly.

### Medium Risk

5. **Pipeline object explosion**: WebGPU pipelines are immutable. If your renderer changes state frequently (blend modes, depth modes), you may create many pipeline objects. Need a pipeline cache keyed by state hash.

6. **Bind group management**: Managing bind groups efficiently is important for WebGPU performance. You need to cache and reuse bind groups, not create them per-draw. This is a new concept with no OpenGL equivalent.

7. **WGSL shader conversion**: 24 shaders to convert manually. Some may use GLSL features that translate awkwardly to WGSL. The instanced rendering shaders (672 lines) will be the most complex.

8. **ImGui with WebGPU**: Writing a WebGPU ImGui backend from scratch is non-trivial but well-documented (imgui_impl_wgpu exists as reference).

### Low Risk

9. **BepuPhysics on WASM**: Pure C# library, likely AOT-compatible, but needs verification.

10. **Image loading on WASM**: ImageSharp should work, but browser image decoding might be faster via browser APIs. Can optimize later.

### Open Questions (Need Investigation)

1. **Does Silk.NET.WebGPU support WASM targets?** Check Silk.NET's WASM support matrix. If yes, this simplifies the WASM path enormously.

2. **Can you load pre-compiled assemblies in NativeAOT WASM?** If not, user scripts must be compiled into the main binary at export time (no runtime assembly loading).

3. **What's the binary size of a minimal Tofu3D WASM game?** NativeAOT should produce small binaries, but with BepuPhysics + ImageSharp + engine code, it could be several MB. May need aggressive trimming.

4. **Does Silk.NET.Windowing support all the features you currently use from OpenTK?** (Monitor scale detection, frame limiting, fullscreen, etc.)

5. **WebGPU feature support across browsers**: WebGPU is supported in Chrome/Edge but Safari support is still evolving (as of 2025). Firefox support is in progress. What's the target browser support?

---

## Appendix A: File-by-File Migration Impact

### Files Requiring Major Changes (Direct GL calls → Abstraction)

| File | GL Calls | Migration Impact |
|---|---|---|
| `Source/ImGui/ImGuiController.cs` | 146 | Rewrite as `IImGuiRenderer` implementation (GL + WebGPU versions) |
| `Source/OpenGL/Shader.cs` | 63 | Split into `GLShader` (existing) + `WGPUShader` (new). Add UBO support. |
| `Source/Framebuffer.cs` | 57 | Split into `GLFramebuffer` + `WGPUFramebuffer`. Note: WebGPU has no persistent FBO. |
| `Source/BufferFactory.cs` | 34 | Split into `GLBufferFactory` + `WGPUBufferFactory`. |
| `Source/Components/Renderers/SpriteRenderer.cs` | 32 | Refactor to use abstraction interfaces. |
| `Source/Scene/InstancedRenderingSystem.cs` | 31 | Refactor to use abstraction. Instancing maps well to WebGPU. |
| `Source/Components/Renderers/ModelRenderer.cs` | 29 | Refactor to use abstraction interfaces. |
| `Source/ImGui/ImGuiTexture.cs` | 21 | Refactor to use `ITexture`. |
| `Source/ImGui/ImGuiShader.cs` | 17 | Refactor to use `IShader`. |
| `Source/Components/TextureLoader.cs` | 11 | Refactor to use `ITexture.SetData()`. |
| `Source/Components/CubemapTextureLoader.cs` | 9 | Refactor to use `ITexture` (cubemap support). |
| All `RenderPass*.cs` files | 6-14 each | Refactor to use `ICommandBuffer` + `IRenderPassEncoder`. |
| `Source/Window.cs` | 12 | Port to Silk.NET.Windowing. |
| `Source/Scene/SharedInstancingBuffer.cs` | 8 | Refactor to use `IBuffer`. |
| `Source/TextureHelper.cs` | 2 | Refactor to use `ITexture` binding. |
| `Source/GeometryBuffer.cs` | 5 | Refactor to use `IBuffer` + vertex layout. |
| `Source/Input/MouseInput.cs` | 5 | Port to Silk.NET.Input. |
| `Source/Input/KeyboardInput.cs` | 3 | Port to Silk.NET.Input. |

### Files Requiring Moderate Changes (Serialization, AOT compatibility)

| File | Change |
|---|---|
| All files in `Source/Serialization/` | Replace Newtonsoft.Json with System.Text.Json + source generators |
| `Source/Scene/SceneSerializer.cs` | Same serialization migration |
| All asset importers/loaders | Serialization migration + `IFileSystem` abstraction |
| `Source/Tofu.cs` | Initialize graphics device based on selected backend |

### Files Requiring Minimal/No Changes

| File | Notes |
|---|---|
| `Source/GameObject/` | No rendering code, no GL calls |
| `Source/Components/` (non-renderer) | Physics, logic components are API-agnostic |
| `Source/Physics/` | BepuPhysics integration, no GL calls |
| `Source/MonoGame/` | Math utilities, no GL calls |
| `Source/Editor/` (UI panels) | Use ImGui, no direct GL calls (except through ImGui renderer) |

---

## Appendix B: Comparison of OpenGL and WebGPU Rendering Flow

### OpenGL Flow (Current)
```
1. GL.UseProgram(shaderProgram)
2. GL.BindVertexArray(vao)
3. GL.UniformMatrix4(mvpLocation, false, ref mvp)  // Set uniforms by location
4. GL.ActiveTexture(TextureUnit.Texture0)
5. GL.BindTexture(TextureTarget.Texture2D, textureId)
6. GL.Uniform1(texLocation, 0)
7. GL.Enable(EnableCap.DepthTest)                    // Global state
8. GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboId)
9. GL.Viewport(0, 0, width, height)
10. GL.Clear(ClearBufferMask.ColorBufferBit | DepthBufferBit)
11. GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount)
12. GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0)
13. GL.UseProgram(0)
```

### WebGPU Flow (Target)
```
1. // Pipeline, bind groups, buffers created upfront (not per-frame)

2. var surfaceTexture = wgpuSurfaceGetCurrentTexture(surface)
3. var surfaceView = wgpuTextureCreateView(surfaceTexture)

4. var encoder = wgpuDeviceCreateCommandEncoder(device)
5. var renderPass = wgpuCommandEncoderBeginRenderPass(encoder, {
       colorAttachments: [{ view: surfaceView, clearValue: {r:0,g:0,b:0,a:1}, loadOp: Clear, storeOp: Store }],
       depthStencilAttachment: { view: depthView, depthClearValue: 1.0, depthLoadOp: Clear, depthStoreOp: Store }
   })

6. wgpuRenderPassEncoderSetPipeline(renderPass, pipeline)
7. wgpuRenderPassEncoderSetBindGroup(renderPass, 0, cameraBindGroup)    // Camera uniforms
8. wgpuRenderPassEncoderSetBindGroup(renderPass, 1, materialBindGroup)  // Textures, material
9. wgpuRenderPassEncoderSetBindGroup(renderPass, 2, objectBindGroup)    // Model matrix
10. wgpuRenderPassEncoderSetVertexBuffer(renderPass, 0, vertexBuffer, 0, vertexBufferSize)
11. wgpuRenderPassEncoderSetIndexBuffer(renderPass, indexBuffer, Uint16, 0, indexBufferSize)
12. wgpuRenderPassEncoderDrawIndexed(renderPass, indexCount, 1, 0, 0, 0)

13. wgpuRenderPassEncoderEnd(renderPass)
14. var commandBuffer = wgpuCommandEncoderFinish(encoder)
15. wgpuQueueSubmit(queue, 1, &commandBuffer)
16. wgpuSurfacePresent(surface)
```

### Key Differences
- **WebGPU creates objects upfront**: Pipelines, bind groups, buffers are created once, not per-frame
- **WebGPU records commands**: You build a command buffer, then submit it. OpenGL executes immediately.
- **WebGPU has no global state**: Everything is explicit — no "current program" or "current texture" state
- **WebGPU bind groups replace uniforms**: Resources are grouped and bound together, not set individually by name
