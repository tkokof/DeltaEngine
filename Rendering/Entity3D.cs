﻿using DeltaEngine.Datatypes;
using DeltaEngine.Entities;

namespace DeltaEngine.Rendering
{
	/// <summary>
	/// Base entity for 3D objects.
	/// </summary>
	public class Entity3D : DrawableEntity
	{
		public Entity3D()
			: this(Vector.Zero, Quaternion.Identity) {}

		public Entity3D(Vector position, Quaternion orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		public Vector Position { get; set; }
		public Quaternion Orientation { get; set; }

		protected override void NextUpdateStarted()
		{
			lastPosition = Position;
			lastOrientation = Orientation;
			base.NextUpdateStarted();
		}

		private Vector lastPosition;
		private Quaternion lastOrientation;

		public override sealed T Get<T>()
		{
			float interpolation = EntitiesRunner.CurrentDrawInterpolation;
			if (EntitiesRunner.Current.State == UpdateDrawState.Draw && typeof(T) == typeof(Vector))
				return (T)(object)lastPosition.Lerp(Position, interpolation);
			if (EntitiesRunner.Current.State == UpdateDrawState.Draw && typeof(T) == typeof(Quaternion))
				return (T)(object)lastOrientation.Lerp(Orientation, interpolation);
			if (typeof(T) == typeof(Vector))
				return (T)(object)Position;
			if (typeof(T) == typeof(Quaternion))
				return (T)(object)Orientation;
			return base.Get<T>();
		}

		public override sealed bool Contains<T>()
		{
			return typeof(T) == typeof(Vector) || typeof(T) == typeof(Quaternion) || base.Contains<T>();
		}

		public override sealed Entity Add<T>(T component)
		{
			if (typeof(T) == typeof(Vector) || typeof(T) == typeof(Quaternion))
				throw new ComponentOfTheSameTypeAddedMoreThanOnce();
			return base.Add(component);
		}

		public override sealed void Set<T>(T component)
		{
			if (typeof(T) == typeof(Vector))
				Position = (Vector)(object)component;
			if (typeof(T) == typeof(Quaternion))
				Orientation = (Quaternion)(object)component;
			base.Set(component);
		}
	}
}