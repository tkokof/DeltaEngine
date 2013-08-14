﻿using System;
using System.IO;
using DeltaEngine.Extensions;

namespace DeltaEngine.Content
{
	/// <summary>
	/// Bunch of images used in a sprite animation. Its ContentMetaData entry contains only images.
	/// </summary>
	public class ImageAnimation : ContentData
	{
		public ImageAnimation(string contentName)
			: base(contentName) {}

		public ImageAnimation(Image[] images, float duration)
			: base("<GeneratedImageAnimation>")
		{
			if (images.Length < 1)
				throw new NoImagesGivenNeedAtLeastOne();
			Frames = images;
			DefaultDuration = duration;
		}

		private class NoImagesGivenNeedAtLeastOne : Exception {}

		public Image[] Frames { get; private set; }
		public float DefaultDuration { get; private set; }
		
		protected override void DisposeData()
		{
			foreach (Image frame in Frames)
				frame.Dispose();
		}

		protected override void LoadData(Stream fileData)
		{
			var imageNames = MetaData.Get("ImageNames", "").SplitAndTrim(',');
			if (imageNames.Length < 1)
				throw new NoImagesGivenNeedAtLeastOne();
			Frames = new Image[imageNames.Length];
			for (int num = 0; num < imageNames.Length; num++)
				Frames[num] = ContentLoader.Load<Image>(imageNames[num]);
			DefaultDuration = MetaData.Get("DefaultDuration", 3.0f);
		}

		public void UpdateMaterialDiffuseMap(int currentFrame, Material material)
		{
			material.DiffuseMap = Frames[currentFrame];
		}

		public Material CreateMaterial(Shader shader)
		{
			return new Material(shader, null) { Animation = this };
		}
	}
}