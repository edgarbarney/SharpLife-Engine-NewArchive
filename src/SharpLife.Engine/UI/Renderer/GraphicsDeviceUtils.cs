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
using Veldrid;
using Veldrid.OpenGL;

namespace SharpLife.Engine.UI.Renderer
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

        private static GraphicsDevice CreateOpenGLGraphicsDevice(ILogger logger, Window window, GraphicsDeviceOptions options)
        {
            window.GetSize(out var width, out var height);

            var glContextHandle = SDL.SDL_GL_CreateContext(window.WindowHandle);

            if (glContextHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create SDL Window");
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

            var platformInfo = new OpenGLPlatformInfo(
                glContextHandle,
                SDL.SDL_GL_GetProcAddress,
                context => SDL.SDL_GL_MakeCurrent(window.WindowHandle, context),
                SDL.SDL_GL_GetCurrentContext,
                () => SDL.SDL_GL_MakeCurrent(IntPtr.Zero, IntPtr.Zero),
                SDL.SDL_GL_DeleteContext,
                () => SDL.SDL_GL_SwapWindow(window.WindowHandle),
                sync => SDL.SDL_GL_SetSwapInterval(sync ? 1 : 0));

            return GraphicsDevice.CreateOpenGL(options, platformInfo, (uint)width, (uint)height);
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
                    return CreateOpenGLGraphicsDevice(logger, window, options);

                case GraphicsBackend.Vulkan:
                    return CreateVulkanGraphicsDevice(window, options);

                default: throw new NotSupportedException($"Graphics backend {backend} not supported");
            }
        }
    }
}
