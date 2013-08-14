﻿using DeltaEngine.Commands;
using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;
using DeltaEngine.Rendering.Fonts;
using DeltaEngine.Rendering.Shapes;
using NUnit.Framework;

namespace DeltaEngine.Input.Tests
{
	public class MouseButtonTriggerTests : TestWithMocksOrVisually
	{
		[Test]
		public void PressLeftMouseButtonToCloseWindow()
		{
			new FontText(FontXml.Default, "Press Left Mouse Button to close window", Rectangle.One);
			new Command(() => Resolve<Window>().CloseAfterFrame()).Add(new MouseButtonTrigger());
		}

		[Test]
		public void ClickAndHoldToShowRedEllipseAtMousePosition()
		{
			var ellipse = new Ellipse(new Rectangle(-0.1f, -0.1f, 0.1f, 0.1f), Color.Red);
			new Command(position => ellipse.Center = position).Add(new MouseButtonTrigger(State.Pressed));
		}

		[Test, CloseAfterFirstFrame]
		public void Create()
		{
			var trigger = new MouseButtonTrigger(MouseButton.Right, State.Pressed);
			Assert.AreEqual(MouseButton.Right, trigger.Button);
			Assert.AreEqual(State.Pressed, trigger.State);
			Assert.AreEqual(MouseButton.Left, new MouseButtonTrigger().Button);
			Assert.AreEqual(State.Pressing, new MouseButtonTrigger().State);
			Assert.AreEqual(MouseButton.Left, new MouseButtonTrigger(State.Pressed).Button);
			Assert.AreEqual(State.Pressed, new MouseButtonTrigger(State.Pressed).State);
		}

		[Test, CloseAfterFirstFrame]
		public void CreateFromString()
		{
			var trigger = new MouseButtonTrigger("Right Pressed");
			Assert.AreEqual(MouseButton.Right, trigger.Button);
			Assert.AreEqual(State.Pressed, trigger.State);
			Assert.Throws<MouseButtonTrigger.CannotCreateMouseButtonTriggerWithoutButton>(
				() => new MouseButtonTrigger(""));
		}
	}
}