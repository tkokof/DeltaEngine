﻿using System.Collections.Generic;
using System.Globalization;
using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Rendering.Fonts;
using DeltaEngine.ScreenSpaces;

namespace Asteroids
{
	public class HudInterface
	{
		public HudInterface()
		{
			hudFont = ContentLoader.Load<FontXml>("Tahoma30");
			ScoreDisplay = new FontText(hudFont, "0",
				new Rectangle(ScreenSpace.Current.Viewport.Left, ScreenSpace.Current.Viewport.Top, 0.1f,
					0.05f));
			ScoreDisplay.RenderLayer = (int)AsteroidsRenderLayer.UserInterface;
			gameOverText = new FontText(hudFont, "", Rectangle.FromCenter(0.5f, 0.5f, 0.8f, 0.4f));
			gameOverText.RenderLayer = (int)AsteroidsRenderLayer.UserInterface;
		}

		private readonly FontXml hudFont;

		public FontText ScoreDisplay { get; private set; }

		public List<FontText> metaInfoTexts = new List<FontText>();

		public void SetScoreText(int score)
		{
			ScoreDisplay.Text = score.ToString(CultureInfo.InvariantCulture);
		}

		public void SetGameOverText()
		{
			gameOverText.Text = "Game Over! \n [Space] \n to restart";
			gameOverText.Visibility = Visibility.Show;
		}

		private readonly FontText gameOverText;

		public void SetIngameMode()
		{
			gameOverText.Visibility = Visibility.Hide;
		}
	}
}