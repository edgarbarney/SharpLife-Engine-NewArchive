/***
*
*	Copyright (c) 1996-2001, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology").  Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
*   This source code contains proprietary and confidential information of
*   Valve LLC and its suppliers.  Access to this code is restricted to
*   persons who have executed a written SDK license with Valve.  Any access,
*   use or distribution of this code by or to any unlicensed person is illegal.
*
****/

using SDL2;
using Serilog;
using System;
using System.Diagnostics;
using Veldrid;
using Veldrid.OpenGL;

namespace SharpLife.Engine.Client.UI.Rendering
{
    internal static class GraphicsDeviceUtils
    {
        private static SwapchainSource GetSwapchainSource(IntPtr window)
        {
            SDL.SDL_SysWMinfo sysWmInfo = new SDL.SDL_SysWMinfo();
            SDL.SDL_GetVersion(out sysWmInfo.version);
            SDL.SDL_GetWindowWMInfo(window, ref sysWmInfo);
            switch (sysWmInfo.subsystem)
            {
                case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS:
                    ref var w32Info = ref sysWmInfo.info.win;
                    return SwapchainSource.CreateWin32(w32Info.window, w32Info.hdc);
                case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_X11:
                    ref var x11Info = ref sysWmInfo.info.x11;
                    return SwapchainSource.CreateXlib(
                        x11Info.display,
                        x11Info.window);
                case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_COCOA:
                    ref var cocoaInfo = ref sysWmInfo.info.cocoa;
                    var nsWindow = cocoaInfo.window;
                    return SwapchainSource.CreateNSWindow(nsWindow);
                default:
                    throw new PlatformNotSupportedException("Cannot create a SwapchainSource for " + sysWmInfo.subsystem + ".");
            }
        }

        private static GraphicsDevice CreateDefaultOpenGLGraphicsDevice(
            ILogger logger,
            GraphicsDeviceOptions options,
            Window window,
            GraphicsBackend backend)
        {
            SDL.SDL_ClearError();
            var sdlHandle = window.WindowHandle;

            var sysWmInfo = new SDL.SDL_SysWMinfo();
            SDL.SDL_GetVersion(out sysWmInfo.version);
            SDL.SDL_GetWindowWMInfo(sdlHandle, ref sysWmInfo);

            SetSDLGLContextAttributes(options, backend);

            var contextHandle = SDL.SDL_GL_CreateContext(sdlHandle);
            var error = SDL.SDL_GetError();
            if (!string.IsNullOrEmpty(error))
            {
                throw new VeldridException(
                    $"Unable to create OpenGL Context: \"{error}\". This may indicate that the system does not support the requested OpenGL profile, version, or Swapchain format.");
            }

            if (0 != SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_RED_SIZE, out var r))
            {
                r = 0;
                logger.Information("Failed to get GL RED size ({0})", SDL.SDL_GetError());
            }

            if (0 != SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_GREEN_SIZE, out var g))
            {
                g = 0;
                logger.Information("Failed to get GL GREEN size ({0})", SDL.SDL_GetError());
            }

            if (0 != SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_BLUE_SIZE, out var b))
            {
                b = 0;
                logger.Information("Failed to get GL BLUE size ({0})", SDL.SDL_GetError());
            }

            if (0 != SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_ALPHA_SIZE, out var a))
            {
                a = 0;
                logger.Information("Failed to get GL ALPHA size ({0})", SDL.SDL_GetError());
            }

            if (0 != SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, out var depth))
            {
                depth = 0;
                logger.Information("Failed to get GL DEPTH size ({0})", SDL.SDL_GetError());
            }

            logger.Information($"GL_SIZES:  r:{r} g:{g} b:{b} a:{a} depth:{depth}");

            if (r <= 4 || g <= 4 || b <= 4 || depth <= 15 /*|| gl_renderer && Q_strstr(gl_renderer, "GDI Generic")*/)
            {
                throw new InvalidOperationException("Failed to create SDL Window, unsupported video mode. A 16-bit color depth desktop is required and a supported GL driver");
            }

            var result = SDL.SDL_GL_SetSwapInterval(options.SyncToVerticalBlank ? 1 : 0);

            OpenGLPlatformInfo platformInfo = new OpenGLPlatformInfo(
                contextHandle,
                SDL.SDL_GL_GetProcAddress,
                context => SDL.SDL_GL_MakeCurrent(sdlHandle, context),
                SDL.SDL_GL_GetCurrentContext,
                () => SDL.SDL_GL_MakeCurrent(IntPtr.Zero, IntPtr.Zero),
                SDL.SDL_GL_DeleteContext,
                () => SDL.SDL_GL_SwapWindow(sdlHandle),
                sync => SDL.SDL_GL_SetSwapInterval(sync ? 1 : 0));

            window.GetSize(out var width, out var height);

            return GraphicsDevice.CreateOpenGL(
                options,
                platformInfo,
                (uint)width,
                (uint)height);
        }

        private static void SetSDLGLContextAttributes(GraphicsDeviceOptions options, GraphicsBackend backend)
        {
            if (backend != GraphicsBackend.OpenGL && backend != GraphicsBackend.OpenGLES)
            {
                throw new VeldridException(
                    $"{nameof(backend)} must be {nameof(GraphicsBackend.OpenGL)} or {nameof(GraphicsBackend.OpenGLES)}.");
            }

            SDL.SDL_GLcontext contextFlags = options.Debug
                ? SDL.SDL_GLcontext.SDL_GL_CONTEXT_DEBUG_FLAG | SDL.SDL_GLcontext.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG
                : SDL.SDL_GLcontext.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG;

            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_FLAGS, (int)contextFlags);

            (int major, int minor) = GetMaxGLVersion(backend == GraphicsBackend.OpenGLES);

            if (backend == GraphicsBackend.OpenGL)
            {
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, major);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, minor);
            }
            else
            {
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_ES);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, major);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, minor);
            }

            int depthBits = 0;
            int stencilBits = 0;
            if (options.SwapchainDepthFormat.HasValue)
            {
                switch (options.SwapchainDepthFormat)
                {
                    case PixelFormat.R16_UNorm:
                        depthBits = 16;
                        break;
                    case PixelFormat.D24_UNorm_S8_UInt:
                        depthBits = 24;
                        stencilBits = 8;
                        break;
                    case PixelFormat.R32_Float:
                        depthBits = 32;
                        break;
                    case PixelFormat.D32_Float_S8_UInt:
                        depthBits = 32;
                        stencilBits = 8;
                        break;
                    default:
                        throw new VeldridException("Invalid depth format: " + options.SwapchainDepthFormat.Value);
                }
            }

            int result = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, depthBits);
            result = SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, stencilBits);

            if (options.SwapchainSrgbFormat)
            {
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_FRAMEBUFFER_SRGB_CAPABLE, 1);
            }
            else
            {
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_FRAMEBUFFER_SRGB_CAPABLE, 0);
            }
        }

        private static readonly object s_glVersionLock = new object();
        private static (int Major, int Minor)? s_maxSupportedGLVersion;
        private static (int Major, int Minor)? s_maxSupportedGLESVersion;

        private static (int Major, int Minor) GetMaxGLVersion(bool gles)
        {
            lock (s_glVersionLock)
            {
                (int Major, int Minor)? maxVer = gles ? s_maxSupportedGLESVersion : s_maxSupportedGLVersion;
                if (maxVer == null)
                {
                    maxVer = TestMaxVersion(gles);
                    if (gles) { s_maxSupportedGLESVersion = maxVer; }
                    else { s_maxSupportedGLVersion = maxVer; }
                }

                return maxVer.Value;
            }
        }

        private static (int Major, int Minor) TestMaxVersion(bool gles)
        {
            (int, int)[] testVersions = gles
                ? new[] { (3, 2), (3, 0) }
                : new[] { (4, 6), (4, 3), (4, 0), (3, 3), (3, 0) };

            foreach ((int major, int minor) in testVersions)
            {
                if (TestIndividualGLVersion(gles, major, minor)) { return (major, minor); }
            }

            return (0, 0);
        }

        private static bool TestIndividualGLVersion(bool gles, int major, int minor)
        {
            SDL.SDL_GLprofile profileMask = gles ? SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_ES : SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE;

            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)profileMask);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, major);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, minor);

            var window = SDL.SDL_CreateWindow(string.Empty, 0, 0, 1, 1, SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);

            var error = SDL.SDL_GetError();

            if (window == IntPtr.Zero || !string.IsNullOrEmpty(error))
            {
                SDL.SDL_ClearError();
                Debug.WriteLine($"Unable to create version {major}.{minor} {profileMask} context.");
                return false;
            }

            IntPtr context = SDL.SDL_GL_CreateContext(window);
            error = SDL.SDL_GetError();
            if (!string.IsNullOrEmpty(error))
            {
                SDL.SDL_ClearError();
                Debug.WriteLine($"Unable to create version {major}.{minor} {profileMask} context.");
                SDL.SDL_DestroyWindow(window);
                return false;
            }

            SDL.SDL_GL_DeleteContext(context);
            SDL.SDL_DestroyWindow(window);
            return true;
        }

        private static GraphicsDevice CreateVulkanGraphicsDevice(Window window, GraphicsDeviceOptions options)
        {
            window.GetSize(out var width, out var height);

            var swapChainDescription = new SwapchainDescription(
                GetSwapchainSource(window.WindowHandle),
                (uint)width,
                (uint)height,
                options.SwapchainDepthFormat,
                false
                );

            return GraphicsDevice.CreateVulkan(options, swapChainDescription);
        }

        public static GraphicsDevice CreateGraphicsDevice(ILogger logger, Window window, GraphicsDeviceOptions options, GraphicsBackend backend)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            switch (backend)
            {
                case GraphicsBackend.OpenGL:
                    return CreateDefaultOpenGLGraphicsDevice(logger, options, window, backend);

                case GraphicsBackend.Vulkan:
                    return CreateVulkanGraphicsDevice(window, options);

                default: throw new NotSupportedException($"Graphics backend {backend} not supported");
            }
        }
    }
}
