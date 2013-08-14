﻿using DeltaEngine.Datatypes;
using DeltaEngine.Mocks;
using NUnit.Framework;

namespace DeltaEngine.Tests
{
	public class WindowTests
	{
		[SetUp]
		public void InitializeWindow()
		{
			window = new MockWindow();
		}

		private Window window;

		[Test]
		public void CreateWindow()
		{
			Assert.IsTrue(window.Visibility);
		}

		[Test]
		public void GetTitle()
		{
			window.Title = "TestTitle";
			Assert.AreEqual("TestTitle", window.Title);
		}

		[Test]
		public void ChangeTotalSize()
		{
			Assert.AreEqual(new Size(640, 360), window.ViewportPixelSize);
			Size changedSize = window.TotalPixelSize;
			window.ViewportSizeChanged += size => changedSize = size;
			window.ViewportPixelSize = new Size(200, 200);
			Assert.AreEqual(new Size(200, 200), window.ViewportPixelSize);
			Assert.IsTrue(window.ViewportPixelSize.Width <= 200);
			Assert.IsTrue(window.ViewportPixelSize.Height <= 200);
			Assert.IsTrue(changedSize.Width <= 200);
			Assert.IsTrue(changedSize.Height <= 200);
		}

		/// <summary>
		/// Use the DeviceTests.SetFullscreenResolution to see the real resolution switching
		/// </summary>
		[Test]
		public void SetFullscreenMode()
		{
			var newFullscreenSize = new Size(1024, 768);
			Assert.IsFalse(window.IsFullscreen);
			bool fullscreenChangedWasCalled = false;
			window.FullscreenChanged += (size, b) => fullscreenChangedWasCalled = true;
			window.SetFullscreen(newFullscreenSize);
			Assert.IsTrue(window.IsFullscreen);
			Assert.AreEqual(newFullscreenSize, window.TotalPixelSize);
			Assert.IsTrue(fullscreenChangedWasCalled);
		}

		[Test]
		public void SwitchToFullscreenAndWindowedMode()
		{
			Size sizeBeforeFullscreen = window.TotalPixelSize;
			window.SetFullscreen(new Size(1024, 768));
			window.SetWindowed();
			Assert.IsFalse(window.IsFullscreen);
			Assert.AreEqual(sizeBeforeFullscreen, window.TotalPixelSize);
		}

		[Test]
		public void CheckMessageBoxAndClipboard()
		{
			Assert.AreEqual("OK", window.ShowMessageBox("Test Message", "Any Message", new string[0]));
			var logger = new MockLogger();
			window.CopyTextToClipboard("Test");
			Assert.AreEqual("Copied to mock clipboard: Test", logger.LastMessage);
		}
	}
}