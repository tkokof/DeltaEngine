﻿using DeltaEngine.Editor.Core;

namespace DeltaEngine.Editor.EmptyEditorPlugin
{
	/// <summary>
	/// Code-behind of EmptyEditorPluginView.xaml
	/// </summary>
	public partial class EmptyEditorPluginView : EditorPluginView
	{
		public EmptyEditorPluginView()
		{
			InitializeComponent();
		}

		public void Init(Service service)
		{
			if (!(DataContext is EmptyEditorPluginViewModel))
				DataContext = new EmptyEditorPluginViewModel();
		}

		public string ShortName
		{
			get { return "EmptyEditorPlugin"; }
		}

		public string Icon
		{
			get { return "Icons/New.png"; }
		}

		public bool RequiresLargePane
		{
			get { return false; }
		}
	}
}