﻿using DeltaEngine.Datatypes;
using DeltaEngine.Graphics;
using DeltaEngine.Mocks;
using DeltaEngine.Platforms;
using DeltaEngine.Rendering.Cameras;
using NUnit.Framework;

namespace DeltaEngine.Rendering.Tests.Cameras
{
	public class OrthoCameraTests : TestWithMocksOrVisually
	{
		[SetUp]
		public void InitializeEntityRunner()
		{
			new MockEntitiesRunner(typeof(MockUpdateBehavior));
			camera = new OrthoCamera(Resolve<Device>(), Resolve<Window>(), Size.One * 4.0f, Vector.Zero);
		}

		private OrthoCamera camera;

		[Test]
		public void CameraPosition()
		{
			Assert.AreEqual(Vector.Zero, camera.Position);
		}

		[Test]
		public void CameraSize()
		{
			Assert.AreEqual(4.0f, camera.Size.Width);
		}

		[Test]
		public void UpdateCameraSize()
		{
			camera.Size = new Size(6.0f);
			Assert.AreEqual(6.0f, camera.Size.Width);
			Assert.AreEqual(6.0f, camera.Size.Height);
		}

		[Test]
		public void UpdateCameraSizeAfterTimeInterval()
		{
			Assert.AreEqual(4.0f, camera.Size.Width);
			AdvanceTimeAndUpdateEntities(3.0f);
			camera.Size = new Size(6.0f);
			Assert.AreEqual(6.0f, camera.Size.Width);
		}
	}
}