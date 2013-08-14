﻿using DeltaEngine.Editor.Core;

namespace DeltaEngine.Editor.Helpers
{
	public class DesignEditorPlugin : EditorPluginView
	{
		public void Init(Service service) {}

		public string ShortName
		{
			get { return "Test Plugin"; }
		}

		public string Icon
		{
			get { return "Icons/Content.png"; }
		}

		public bool RequiresLargePane
		{
			get { return false; }
		}
	}
}