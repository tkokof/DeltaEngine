﻿using DeltaEngine.Datatypes;

namespace DeltaEngine.Scenes.UserInterfaces.Controls
{
	/// <summary>
	/// Holds information about the state of a control - eg. is the mouse/touch inside of it, or
	/// does the control have focus etc.
	/// </summary>
	public class InteractiveState
	{
		public bool IsInside { get; set; }
		public bool IsPressed { get; set; }
		public bool IsSelected { get; set; }
		public Point RelativePointerPosition { get; set; }
		public bool CanHaveFocus { get; set; }
		public bool HasFocus { get; set; }
		public bool WantsFocus { get; set; }
		public Point DragDelta { get; set; }
		public Rectangle DragArea
		{
			get { return dragArea; }
			set { dragArea = value; }
		}
		private Rectangle dragArea = Rectangle.Unused;
	}
}