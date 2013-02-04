﻿using System;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using Microsoft.Xna.Framework;
using Color = DeltaEngine.Datatypes.Color;
using Point = DeltaEngine.Datatypes.Point;

namespace DeltaEngine.Platforms
{
	/// <summary>
	/// Window support via the buildin XNA Game.Window functionality; supports fullscreen mode.
	/// </summary>
	public class XnaWindow : Window
	{
		public XnaWindow(Game game)
		{
			this.game = game;
			game.Window.Title = StackTraceExtensions.GetEntryName();
			game.Window.AllowUserResizing = true;
			game.IsMouseVisible = true;
			game.Window.ClientSizeChanged += OnViewportSizeChanged;
			game.Window.OrientationChanged += (sender, args) => 
				OnOrientationChanged(GetOrientation(game.Window.CurrentOrientation));
			game.Exiting += (sender, args) => { IsClosing = true; };
			BackgroundColor = Color.Black;
			closeAfterOneFrameIfInIntegrationTest = !StackTraceExtensions.ContainsNoTestOrIsVisualTest();
		}

		private readonly Game game;
		private readonly bool closeAfterOneFrameIfInIntegrationTest;

		private void OnViewportSizeChanged(object sender, EventArgs e)
		{
			if (ViewportSizeChanged != null)
				ViewportSizeChanged(ViewportPixelSize);
		}

		public void OnOrientationChanged(Orientation obj)
		{
			Action<Orientation> handler = OrientationChanged;
			if (handler != null)
				handler(obj);
		}

		private Orientation GetOrientation(DisplayOrientation xnaOrientaion)
		{
			if (xnaOrientaion == DisplayOrientation.LandscapeLeft ||
				xnaOrientaion == DisplayOrientation.LandscapeRight)
				return Orientation.Landscape;

			return Orientation.Portrait;
		}

		public string Title
		{
			get { return game.Window.Title; }
			set { game.Window.Title = value; }
		}

		public bool IsVisible
		{
			get { return game.IsActive; }
		}

		public IntPtr Handle
		{
			get { return game.Window.Handle; }
		}

		public Size ViewportPixelSize
		{
			get { return new Size(game.Window.ClientBounds.Width, game.Window.ClientBounds.Height); }
		}

		public Size TotalPixelSize
		{
			get { return new Size(game.Window.ClientBounds.Width, game.Window.ClientBounds.Height); }
			set
			{
				game.Window.BeginScreenDeviceChange(false);
				game.Window.EndScreenDeviceChange(game.Window.ScreenDeviceName, (int)value.Width,
					(int)value.Height);
				OnViewportSizeChanged(game.Window, EventArgs.Empty);
			}
		}

		public Point PixelPosition
		{
			get { return new Point(game.Window.ClientBounds.X, game.Window.ClientBounds.Y); }
		}

		public Color BackgroundColor { get; set; }

		public bool IsFullscreen
		{
			get { return !game.Window.AllowUserResizing; }
			set
			{
				game.Window.AllowUserResizing = value;
				game.Window.BeginScreenDeviceChange(value);
				game.Window.EndScreenDeviceChange(game.Window.ScreenDeviceName);
			}
		}
		public bool IsClosing { get; private set; }

		public event Action<Size> ViewportSizeChanged;
		public event Action<Orientation> OrientationChanged;

		public void Run()
		{
			FrameworkDispatcher.Update();
			if (closeAfterOneFrameIfInIntegrationTest)
				game.Exit();
		}

		public void Dispose()
		{
			if (IsClosing)
				return;

			IsClosing = true;
			FrameworkDispatcher.Update();
			game.Exit();
		}
	}
}