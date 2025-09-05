using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace VynEngine.UI;

internal sealed unsafe class ImGuiGlRenderer : IDisposable
{
    private static ImGuiGlRenderer? _current;

    private readonly GL _gl;
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
        _current = this;

        CreateDeviceObjects();
        CreateFontsTexture();

        InitMultiViewportSupport();
    }

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

        var lastActiveTexture = _gl.GetInteger(GetPName.ActiveTexture);
        _gl.ActiveTexture(TextureUnit.Texture0);
        var lastProgram = _gl.GetInteger(GetPName.CurrentProgram);
        var lastTexture = _gl.GetInteger(GetPName.TextureBinding2D);
        var lastSampler = _gl.GetInteger(GetPName.SamplerBinding);
        var lastArrayBuf = _gl.GetInteger(GetPName.ArrayBufferBinding);
        var lastVao = _gl.GetInteger(GetPName.VertexArrayBinding);
        Span<int> lastPolygonMode = stackalloc int[2];
        _gl.GetInteger(GetPName.PolygonMode, out lastPolygonMode[0]);
        Span<int> lastViewport = stackalloc int[4];
        _gl.GetInteger(GetPName.Viewport, out lastViewport[0]);
        Span<int> lastScissorBox = stackalloc int[4];
        _gl.GetInteger(GetPName.ScissorBox, out lastScissorBox[0]);
        var lastBlendSrcRgb = _gl.GetInteger(GetPName.BlendSrcRgb);
        var lastBlendDstRgb = _gl.GetInteger(GetPName.BlendDstRgb);
        var lastBlendSrcAlpha = _gl.GetInteger(GetPName.BlendSrcAlpha);
        var lastBlendDstAlpha = _gl.GetInteger(GetPName.BlendDstAlpha);
        var lastEqRgb = _gl.GetInteger(GetPName.BlendEquationRgb);
        var lastEqAlpha = _gl.GetInteger(GetPName.BlendEquationAlpha);
        var lastEnBlend = _gl.IsEnabled(EnableCap.Blend);
        var lastEnCull = _gl.IsEnabled(EnableCap.CullFace);
        var lastEnDepth = _gl.IsEnabled(EnableCap.DepthTest);
        var lastEnStencil = _gl.IsEnabled(EnableCap.StencilTest);
        var lastEnScissor = _gl.IsEnabled(EnableCap.ScissorTest);
        var lastEnPrimRestart = _gl.IsEnabled(EnableCap.PrimitiveRestart);

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

            for (var i = 0; i < drawList.CmdBuffer.Size; i++)
            {
                var cmd = drawList.CmdBuffer[i];

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

        if (lastProgram == 0 || _gl.IsProgram((uint)lastProgram)) _gl.UseProgram((uint)lastProgram);
        _gl.BindTexture(TextureTarget.Texture2D, (uint)lastTexture);
        _gl.BindSampler(0, (uint)lastSampler);
        _gl.ActiveTexture((TextureUnit)lastActiveTexture);
        _gl.BindVertexArray((uint)lastVao);
        _gl.BindBuffer(GLEnum.ArrayBuffer, (uint)lastArrayBuf);
        _gl.BlendEquationSeparate((GLEnum)lastEqRgb, (GLEnum)lastEqAlpha);
        _gl.BlendFuncSeparate((BlendingFactor)lastBlendSrcRgb, (BlendingFactor)lastBlendDstRgb,
            (BlendingFactor)lastBlendSrcAlpha, (BlendingFactor)lastBlendDstAlpha);
        Toggle(EnableCap.Blend, lastEnBlend);
        Toggle(EnableCap.CullFace, lastEnCull);
        Toggle(EnableCap.DepthTest, lastEnDepth);
        Toggle(EnableCap.StencilTest, lastEnStencil);
        Toggle(EnableCap.ScissorTest, lastEnScissor);
        Toggle(EnableCap.PrimitiveRestart, lastEnPrimRestart);
        _gl.PolygonMode(TriangleFace.FrontAndBack, (PolygonMode)lastPolygonMode[0]);
        _gl.Viewport(lastViewport[0], lastViewport[1], (uint)lastViewport[2], (uint)lastViewport[3]);
        _gl.Scissor(lastScissorBox[0], lastScissorBox[1], (uint)lastScissorBox[2], (uint)lastScissorBox[3]);
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
        _current?._gl.Clear(ClearBufferMask.ColorBufferBit);
        _current?.RenderDrawData(vp.DrawData);
    }

    public void Dispose()
    {
        ShutdownMultiViewportSupport();

        if (_fontTexture != 0) _gl.DeleteTexture(_fontTexture);
        if (_vao != 0) _gl.DeleteVertexArray(_vao);
        if (_vbo != 0) _gl.DeleteBuffer(_vbo);
        if (_ebo != 0) _gl.DeleteBuffer(_ebo);
        if (_shader != 0) _gl.DeleteProgram(_shader);

        if (ReferenceEquals(_current, this)) _current = null;
    }
}