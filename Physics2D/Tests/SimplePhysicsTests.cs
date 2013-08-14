﻿using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;
using DeltaEngine.Rendering;
using DeltaEngine.Rendering.Shapes;
using DeltaEngine.Rendering.Sprites;
using DeltaEngine.ScreenSpaces;
using NUnit.Framework;

namespace DeltaEngine.Physics2D.Tests
{
	public class SimplePhysicsTests : TestWithMocksOrVisually
	{
		[SetUp]
		public void CreateWindowForScreenSpace()
		{
			Resolve<Window>();
		}

		[Test]
		public void FallingEffectIsRemovedAfterOneSecond()
		{
			var sprite = CreateFallingSpriteWhichExpires();
			CheckFallingEffectStateAfterOneSecond(sprite);
		}

		private static Sprite CreateFallingSpriteWhichExpires()
		{
			var sprite = new Sprite(new Material(Shader.Position2DUv, "DeltaEngineLogo"), Rectangle.One);
			sprite.Add(new SimplePhysics.Data
			{
				Velocity = Point.Half,
				Gravity = new Point(1.0f, 2.0f),
				RotationSpeed = 100.0f,
				Duration = 1.0f
			});
			sprite.Start<SimplePhysics.Move>();
			sprite.Color = Color.Red;
			return sprite;
		}

		private void CheckFallingEffectStateAfterOneSecond(Entity2D entity)
		{
			AdvanceTimeAndUpdateEntities(1.0f);
			Assert.AreEqual(1.534f, entity.DrawArea.Center.X, 0.01f);
			Assert.AreEqual(2.059f, entity.DrawArea.Center.Y, 0.01f);
			Assert.AreEqual(100.0f, entity.Rotation, 5.0f);
		}

		[Test]
		public void RenderSlowlyFallingLogo()
		{
			CreateFallingSprite();
		}

		private void CreateFallingSprite()
		{
			var sprite = new Sprite(new Material(Shader.Position2DUv, "DeltaEngineLogo"), screenCenter);
			sprite.Add(new SimplePhysics.Data
			{
				Velocity = new Point(0.0f, -0.3f),
				RotationSpeed = 100.0f,
				Gravity = new Point(0.0f, 0.1f),
			});
			sprite.Color = Color.Red;
			sprite.Start<SimplePhysics.Move>();
		}

		private readonly Rectangle screenCenter = Rectangle.FromCenter(Point.Half, new Size(0.2f));

		[Test]
		public void RenderFallingCircle()
		{
			var ellipse = new Ellipse(Point.Half, 0.1f, 0.1f, Color.Blue);
			ellipse.Add(new SimplePhysics.Data
			{
				Velocity = new Point(0.1f, -0.1f),
				Gravity = new Point(0.0f, 0.1f)
			});
			ellipse.Start<SimplePhysics.Move>();
		}

		[Test]
		public void RenderMovingCircleUsingExtension()
		{
			var ellipse = new Ellipse(Point.Half, 0.1f, 0.1f, Color.Blue);
			ellipse.StartMoving(new Point(0.1f, -0.1f));
		}

		[Test]
		public void RenderFallingCircleUsingExtension()
		{
			var ellipse = new Ellipse(Point.Half, 0.1f, 0.1f, Color.Blue);
			ellipse.StartFalling(new Point(0.1f, -0.1f), new Point(0.0f, 0.1f));
		}


		[Test]
		public void RenderRotatingRect()
		{
			var rect = new FilledRect(Rectangle.FromCenter(Point.Half, new Size(0.2f)), Color.Orange)
			{
				Rotation = 0
			};
			rect.Add(new SimplePhysics.Data { Gravity = Point.Zero, RotationSpeed = 5 });
			rect.Start<SimplePhysics.Rotate>();
		}

		[Test]
		public void RenderRotatingRectViaExtensionMethod()
		{
			new FilledRect(Rectangle.FromCenter(Point.Half, new Size(0.2f)), Color.Red).StartRotating(5);
		}

		[Test]
		public void BounceOffScreenEdge()
		{
			var rect =
				new FilledRect(new Rectangle(ScreenSpace.Current.Viewport.TopLeft, new Size(0.2f)),
					Color.Red);
			rect.Add(new SimplePhysics.Data { Gravity = Point.Zero, Velocity = new Point(-0.1f, 0.0f) });
			rect.Start<SimplePhysics.Move>();
			rect.Start<SimplePhysics.BounceIfAtScreenEdge>();
			AdvanceTimeAndUpdateEntities();
			Assert.AreEqual(0.1f, rect.Get<SimplePhysics.Data>().Velocity.X);
		}

		[Test, CloseAfterFirstFrame]
		public void RotateAdvancesAngleCorrectly()
		{
			var rect = new FilledRect(new Rectangle(ScreenSpace.Current.Viewport.TopLeft, new Size(0.2f)),
				Color.Red);
			rect.Rotation = 0;
			rect.Add(new SimplePhysics.Data { Gravity = Point.Zero, RotationSpeed = 0.1f });
			rect.Start<SimplePhysics.Rotate>();
			AdvanceTimeAndUpdateEntities();
			Assert.Greater(rect.Rotation, 0);
		}

		[Test]
		public void RenderMovingUvSprite()
		{
			var sprite = new Sprite(new Material(Shader.Position2DUv, "DeltaEngineLogo"), Rectangle.One);
			sprite.SetNewUV(new Rectangle(0, 0, 5, 5), FlipMode.Vertical);
			sprite.StartMovingUv(Point.One);
		}
	}
}