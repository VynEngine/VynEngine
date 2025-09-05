using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace VynEngine.UI;

internal sealed unsafe class ImGuiGlRenderer : IDisposable
{
    private static ImGuiGlRenderer? s_current;

    private readonly GL _gl;

    // Device objects
    private uint _fontTexture;
    private uint _shader;
    private int _locTex;
    private int _locProj;
    private uint _vbo;
    private uint _ebo;
    private uint _vao;

    public ImGuiGlRenderer(GL gl)
    {
        _gl = gl;
        s_current = this;

        CreateDeviceObjects();
        CreateFontsTexture();

        InitMultiViewportSupport();
    }

    // ---------- Frame API ----------
    public void NewFrame()
    {
        if (_shader == 0) CreateDeviceObjects();
        if (_fontTexture == 0) CreateFontsTexture();
    }

    public void RenderDrawData(ImDrawDataPtr drawData)
    {
        var fbWidth = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
        var fbHeight = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);
        if (fbWidth <= 0 || fbHeight <= 0) return;

        var last_active_texture = _gl.GetInteger(GetPName.ActiveTexture);
        _gl.ActiveTexture(TextureUnit.Texture0);
        var last_program = _gl.GetInteger(GetPName.CurrentProgram);
        var last_texture = _gl.GetInteger(GetPName.TextureBinding2D);
        var last_sampler = _gl.GetInteger(GetPName.SamplerBinding);
        var last_array_buf = _gl.GetInteger(GetPName.ArrayBufferBinding);
        var last_vao = _gl.GetInteger(GetPName.VertexArrayBinding);
        Span<int> last_polygon_mode = stackalloc int[2];
        _gl.GetInteger(GetPName.PolygonMode, out last_polygon_mode[0]);
        Span<int> last_viewport = stackalloc int[4];
        _gl.GetInteger(GetPName.Viewport, out last_viewport[0]);
        Span<int> last_scissor_box = stackalloc int[4];
        _gl.GetInteger(GetPName.ScissorBox, out last_scissor_box[0]);
        var last_blend_src_rgb = _gl.GetInteger(GetPName.BlendSrcRgb);
        var last_blend_dst_rgb = _gl.GetInteger(GetPName.BlendDstRgb);
        var last_blend_src_alpha = _gl.GetInteger(GetPName.BlendSrcAlpha);
        var last_blend_dst_alpha = _gl.GetInteger(GetPName.BlendDstAlpha);
        var last_eq_rgb = _gl.GetInteger(GetPName.BlendEquationRgb);
        var last_eq_alpha = _gl.GetInteger(GetPName.BlendEquationAlpha);
        var last_en_blend = _gl.IsEnabled(EnableCap.Blend);
        var last_en_cull = _gl.IsEnabled(EnableCap.CullFace);
        var last_en_depth = _gl.IsEnabled(EnableCap.DepthTest);
        var last_en_stencil = _gl.IsEnabled(EnableCap.StencilTest);
        var last_en_scissor = _gl.IsEnabled(EnableCap.ScissorTest);
        var last_en_prim_restart = _gl.IsEnabled(EnableCap.PrimitiveRestart);

        var vao = _gl.GenVertexArray();
        SetupRenderState(drawData, fbWidth, fbHeight, vao);

        var clipOff = drawData.DisplayPos;
        var clipScale = drawData.FramebufferScale;

        for (var n = 0; n < drawData.CmdListsCount; n++)
        {
            var drawList = drawData.CmdLists[n];

            var vtxSize = (nuint)(drawList.VtxBuffer.Size * sizeof(ImDrawVert));
            var idxSize = (nuint)(drawList.IdxBuffer.Size * sizeof(ushort));

            var vtxPtr = drawList.VtxBuffer.Data.ToPointer();
            var idxPtr = drawList.IdxBuffer.Data.ToPointer();

            _gl.BindBuffer(GLEnum.ArrayBuffer, _vbo);
            _gl.BufferData(GLEnum.ArrayBuffer, vtxSize, null, GLEnum.StreamDraw);
            _gl.BufferSubData(GLEnum.ArrayBuffer, 0, vtxSize, vtxPtr);

            _gl.BindBuffer(GLEnum.ElementArrayBuffer, _ebo);
            _gl.BufferData(GLEnum.ElementArrayBuffer, idxSize, null, GLEnum.StreamDraw);
            _gl.BufferSubData(GLEnum.ElementArrayBuffer, 0, idxSize, idxPtr);

            for (var cmd_i = 0; cmd_i < drawList.CmdBuffer.Size; cmd_i++)
            {
                var cmd = drawList.CmdBuffer[cmd_i];

                if (cmd.UserCallback != IntPtr.Zero)
                {
                    SetupRenderState(drawData, fbWidth, fbHeight, vao);
                }
                else
                {
                    Vector2 clipMin = new((cmd.ClipRect.X - clipOff.X) * clipScale.X,
                        (cmd.ClipRect.Y - clipOff.Y) * clipScale.Y);
                    Vector2 clipMax = new((cmd.ClipRect.Z - clipOff.X) * clipScale.X,
                        (cmd.ClipRect.W - clipOff.Y) * clipScale.Y);
                    if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y)
                        continue;

                    _gl.Scissor((int)clipMin.X, (int)(fbHeight - clipMax.Y),
                        (uint)(clipMax.X - clipMin.X), (uint)(clipMax.Y - clipMin.Y));

                    var tex = (uint)cmd.TextureId.ToInt64();
                    _gl.ActiveTexture(TextureUnit.Texture0);
                    _gl.BindTexture(TextureTarget.Texture2D, tex);

                    _gl.DrawElementsBaseVertex(PrimitiveType.Triangles,
                        cmd.ElemCount, DrawElementsType.UnsignedShort,
                        (void*)(cmd.IdxOffset * sizeof(ushort)), (int)cmd.VtxOffset);
                }
            }
        }

        _gl.DeleteVertexArray(vao);

        if (last_program == 0 || _gl.IsProgram((uint)last_program)) _gl.UseProgram((uint)last_program);
        _gl.BindTexture(TextureTarget.Texture2D, (uint)last_texture);
        _gl.BindSampler(0, (uint)last_sampler);
        _gl.ActiveTexture((TextureUnit)last_active_texture);
        _gl.BindVertexArray((uint)last_vao);
        _gl.BindBuffer(GLEnum.ArrayBuffer, (uint)last_array_buf);
        _gl.BlendEquationSeparate((GLEnum)last_eq_rgb, (GLEnum)last_eq_alpha);
        _gl.BlendFuncSeparate((BlendingFactor)last_blend_src_rgb, (BlendingFactor)last_blend_dst_rgb,
            (BlendingFactor)last_blend_src_alpha, (BlendingFactor)last_blend_dst_alpha);
        Toggle(EnableCap.Blend, last_en_blend);
        Toggle(EnableCap.CullFace, last_en_cull);
        Toggle(EnableCap.DepthTest, last_en_depth);
        Toggle(EnableCap.StencilTest, last_en_stencil);
        Toggle(EnableCap.ScissorTest, last_en_scissor);
        Toggle(EnableCap.PrimitiveRestart, last_en_prim_restart);
        _gl.PolygonMode(TriangleFace.FrontAndBack, (PolygonMode)last_polygon_mode[0]);
        _gl.Viewport(last_viewport[0], last_viewport[1], (uint)last_viewport[2], (uint)last_viewport[3]);
        _gl.Scissor(last_scissor_box[0], last_scissor_box[1], (uint)last_scissor_box[2], (uint)last_scissor_box[3]);
        return;

        void Toggle(EnableCap cap, bool on)
        {
            if (on) _gl.Enable(cap);
            else _gl.Disable(cap);
        }
    }

    private void SetupRenderState(ImDrawDataPtr drawData, int fbWidth, int fbHeight, uint vao)
    {
        _gl.Enable(GLEnum.Blend);
        _gl.BlendEquation(GLEnum.FuncAdd);
        _gl.BlendFuncSeparate(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha, GLEnum.One, GLEnum.OneMinusSrcAlpha);
        _gl.Disable(GLEnum.CullFace);
        _gl.Disable(GLEnum.DepthTest);
        _gl.Disable(GLEnum.StencilTest);
        _gl.Enable(GLEnum.ScissorTest);
        _gl.Disable(GLEnum.PrimitiveRestart);
        _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

        const int GL_UPPER_LEFT = 0x8CA2;
        var clipOrigin = _gl.GetInteger(GLEnum.ClipOrigin);
        var clipOriginLowerLeft = clipOrigin != GL_UPPER_LEFT;

        _gl.Viewport(0, 0, (uint)fbWidth, (uint)fbHeight);

        var L = drawData.DisplayPos.X;
        var R = drawData.DisplayPos.X + drawData.DisplaySize.X;
        var T = drawData.DisplayPos.Y;
        var B = drawData.DisplayPos.Y + drawData.DisplaySize.Y;
        if (!clipOriginLowerLeft)
        {
            (T, B) = (B, T);
        }

        var mvp = new[]
        {
            2.0f / (R - L), 0.0f, 0.0f, 0.0f,
            0.0f, 2.0f / (T - B), 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f, 0.0f,
            (R + L) / (L - R), (T + B) / (B - T), 0.0f, 1.0f
        };

        _gl.UseProgram(_shader);
        _gl.Uniform1(_locTex, 0);
        fixed (float* p = mvp)
            _gl.UniformMatrix4(_locProj, 1, false, p);

        _gl.BindSampler(0, 0);
        _gl.BindVertexArray(_vao != 0 ? _vao : vao);
        _gl.BindBuffer(GLEnum.ArrayBuffer, _vbo);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, _ebo);

        var stride = (uint)Unsafe.SizeOf<ImDrawVert>();
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)8);
        _gl.EnableVertexAttribArray(2);
        _gl.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, (void*)16);
    }

    private void CreateFontsTexture()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out _);

        _fontTexture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _fontTexture);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        _gl.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToPointer());

        io.Fonts.SetTexID((IntPtr)_fontTexture);
        io.Fonts.ClearTexData();
    }

    private void CreateDeviceObjects()
    {
        const string VS = """
                          #version 410
                          layout(location = 0) in vec2 Position;
                          layout(location = 1) in vec2 UV;
                          layout(location = 2) in vec4 Color;
                          uniform mat4 u_ProjMtx;
                          out vec2 Frag_UV;
                          out vec4 Frag_Color;
                          void main() {
                              Frag_UV = UV;
                              Frag_Color = Color;
                              gl_Position = u_ProjMtx * vec4(Position, 0, 1);
                          }
                          """;

        const string FS = """
                          #version 410
                          in vec2 Frag_UV;
                          in vec4 Frag_Color;
                          uniform sampler2D Texture;
                          layout(location = 0) out vec4 Out_Color;
                          void main() {
                              Out_Color = Frag_Color * texture(Texture, Frag_UV);
                          }
                          """;

        var vert = CompileShader(ShaderType.VertexShader, VS);
        var frag = CompileShader(ShaderType.FragmentShader, FS);
        _shader = _gl.CreateProgram();
        _gl.AttachShader(_shader, vert);
        _gl.AttachShader(_shader, frag);
        _gl.LinkProgram(_shader);
        _gl.GetProgram(_shader, GLEnum.LinkStatus, out int linked);
        if (linked == 0)
        {
            var log = _gl.GetProgramInfoLog(_shader);
            throw new Exception($"Shader link error: {log}");
        }

        _gl.DetachShader(_shader, vert);
        _gl.DetachShader(_shader, frag);
        _gl.DeleteShader(vert);
        _gl.DeleteShader(frag);

        _locTex = _gl.GetUniformLocation(_shader, "Texture");
        _locProj = _gl.GetUniformLocation(_shader, "u_ProjMtx");

        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _ebo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(GLEnum.ArrayBuffer, _vbo);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, _ebo);
        _gl.BindVertexArray(0);
    }

    private uint CompileShader(ShaderType type, string src)
    {
        var s = _gl.CreateShader(type);
        _gl.ShaderSource(s, src);
        _gl.CompileShader(s);
        _gl.GetShader(s, GLEnum.CompileStatus, out int ok);
        if (ok == 0)
        {
            var log = _gl.GetShaderInfoLog(s);
            throw new Exception($"Shader compile error ({type}): {log}");
        }

        return s;
    }

    private void InitMultiViewportSupport()
    {
        var pio = ImGui.GetPlatformIO();
        pio.Renderer_RenderWindow = (IntPtr)(delegate* unmanaged[Cdecl]<ImGuiViewportPtr, void>)&Renderer_RenderWindow;
    }

    private static void ShutdownMultiViewportSupport()
    {
        ImGui.DestroyPlatformWindows();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void Renderer_RenderWindow(ImGuiViewportPtr vp)
    {
        s_current?._gl.Clear(ClearBufferMask.ColorBufferBit);
        s_current?.RenderDrawData(vp.DrawData);
    }

    public void Dispose()
    {
        ShutdownMultiViewportSupport();

        if (_fontTexture != 0) _gl.DeleteTexture(_fontTexture);
        if (_vao != 0) _gl.DeleteVertexArray(_vao);
        if (_vbo != 0) _gl.DeleteBuffer(_vbo);
        if (_ebo != 0) _gl.DeleteBuffer(_ebo);
        if (_shader != 0) _gl.DeleteProgram(_shader);

        if (ReferenceEquals(s_current, this)) s_current = null;
    }
}