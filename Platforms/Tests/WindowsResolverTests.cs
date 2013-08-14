﻿using DeltaEngine.Datatypes;
using DeltaEngine.Graphics;
using DeltaEngine.Graphics.Mocks;
using DeltaEngine.Mocks;
using DeltaEngine.Rendering.Shapes;
using NUnit.Framework;

namespace DeltaEngine.Platforms.Tests
{
	public class WindowsResolverTests
	{
		//ncrunch: no coverage start
		[TestFixtureSetUp]
		public void CreateWindowsResolver()
		{
			resolver = new EmptyWindowsResolver();
			Assert.NotNull(resolver);
		}

		private EmptyWindowsResolver resolver;

		[TestFixtureTearDown]
		public void DisposeWindowsResolver()
		{
			resolver.Dispose();
		}

		private class EmptyWindowsResolver : AppRunner
		{
			public void Register(object instance)
			{
				RegisterInstance(instance);
			}

			protected override void RegisterMediaTypes() {}
		}

		[Test, Category("Slow")]
		public void RegisterNonRenderableObject()
		{
			var rect = new Rectangle(Point.Half, Size.Half);
			resolver.Register(rect);
		}

		[Test, Category("Slow")]
		public void RegisterRenderableObject()
		{
			using (var window = new MockWindow())
			using (var device = new MockDevice(window))
			{
				device.Clear();
				device.Present();
				resolver.Register(device);
				resolver.RegisterSingleton<Drawing>();
				resolver.Register(new Line2D(Point.One, Point.Zero, Color.Red));
			}
		}
	}
}