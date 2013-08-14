﻿using DeltaEngine.Platforms;

namespace LogoApp
{
	/// <summary>
	/// Displays a number of colored moving logo sprites bouncing around.
	/// </summary>
	internal class Program : App
	{
		public Program()
		{
			for (int num = 0; num < 15; num++)
				new BouncingLogo();
		}

		public static void Main()
		{
			new Program().Run();
		}
	}
}