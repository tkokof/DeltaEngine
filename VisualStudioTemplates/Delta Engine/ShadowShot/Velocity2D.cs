using DeltaEngine.Datatypes;
using DeltaEngine.Extensions;

namespace $safeprojectname$
{
	public class Velocity2D
	{
		public Velocity2D(Vector2D velocity, float maximumSpeed)
		{
			this.velocity = velocity;
			this.maximumSpeed = maximumSpeed;
		}

		public Vector2D velocity;
		public readonly float maximumSpeed;

		public void Accelerate(Vector2D acceleration2D)
		{
			Velocity += acceleration2D;
		}

		public Vector2D Velocity
		{
			get
			{
				return velocity;
			}
			set
			{
				velocity = value;
				CapAtMaximumSpeed();
			}
		}

		private void CapAtMaximumSpeed()
		{
			float speed = velocity.Length;
			if (speed > maximumSpeed)
				velocity *= maximumSpeed / speed;
		}

		public void Accelerate(float magnitude, float angle)
		{
			Velocity = new Vector2D(velocity.X + MathExtensions.Sin(angle) * magnitude, velocity.Y - 
				MathExtensions.Cos(angle) * magnitude);
		}

		public void Accelerate(float factor)
		{
			Velocity *= factor;
		}
	}
}