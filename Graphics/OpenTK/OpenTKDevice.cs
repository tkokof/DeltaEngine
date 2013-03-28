﻿using System.Drawing;
using DeltaEngine.Platforms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using Point = DeltaEngine.Datatypes.Point;

namespace DeltaEngine.Graphics.OpenTK
{
	/// <summary>
	/// The OpenGL color buffer is cleared in Run and shown in Present.
	/// </summary>
	public sealed class OpenTKDevice : Device
	{
		public OpenTKDevice(Window window)
		{
			this.window = window;
			if (window.Title == "")
				window.Title = "OpenTK Device";
			InitGL();
			InitializeModelViewMatrix();
			InitializeProjectionMatrix();
			window.ViewportSizeChanged += size => InitializeProjectionMatrix();
		}

		private readonly Window window;

		private void InitGL()
		{
			windowInfo = Utilities.CreateWindowsWindowInfo(window.Handle);
			context = new GraphicsContext(GraphicsMode.Default, windowInfo);
			context.MakeCurrent(windowInfo);
			context.LoadAll();
		}

		private IWindowInfo windowInfo;
		private GraphicsContext context;

		private static void InitializeModelViewMatrix()
		{
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
		}

		private void InitializeProjectionMatrix()
		{
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			var width = (int)window.ViewportPixelSize.Width;
			var height = (int)window.ViewportPixelSize.Height;
			GL.Ortho(0, width, height, 0, -1, 1);
			GL.Viewport(0, 0, width, height);
		}

		public void Run()
		{
			var color = window.BackgroundColor;
			if (color.A <= 0)
				return;

			GL.ClearColor(Color.FromArgb(color.R, color.G, color.B));
			GL.Clear(ClearBufferMask.ColorBufferBit);
		}

		public void Present()
		{
			if (window.IsVisible && context != null)
				context.SwapBuffers();
		}

		public void Dispose()
		{
			if (context != null)
				context.Dispose();
			context = null;
		}
	}
}