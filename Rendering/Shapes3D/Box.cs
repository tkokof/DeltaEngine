﻿using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Graphics;
using DeltaEngine.Graphics.Vertices;

namespace DeltaEngine.Rendering.Shapes3D
{
	/// <summary>
	/// Entity 3D representing a simple box.
	/// </summary>
	public class Box : Model
	{
		public Box(Vector size, Color color)
			: base(CreateBoxData(size, color), new Material(Shader.Position3DColor, ""))
		{
			Add(color);
			OnDraw<GeometryRender>();
		}

		private static Geometry CreateBoxData(Vector size, Color color)
		{
			var creationData = new GeometryCreationData(VertexFormat.Position3DColor, 8, 36);
			var geometry = ContentLoader.Create<Geometry>(creationData);
			geometry.SetData(ComputeVertices(size, color), BoxIndices);
			return geometry;
		}

		private static Vertex[] ComputeVertices(Vector size, Color color)
		{
			float right = size.X / 2.0f;
			float back = size.Z / 2.0f;
			float top = size.Y / 2.0f;
			float bottom = -size.Y / 2.0f;
			var vertices = new Vertex[]
			{
				new VertexPosition3DColor(new Vector(-right, -back, top), color),
				new VertexPosition3DColor(new Vector(right, -back, top), color),
				new VertexPosition3DColor(new Vector(-right, -back, bottom), color),
				new VertexPosition3DColor(new Vector(right, -back, bottom), color),
				new VertexPosition3DColor(new Vector(right, back, top), color),
				new VertexPosition3DColor(new Vector(-right, back, top), color),
				new VertexPosition3DColor(new Vector(right, back, bottom), color),
				new VertexPosition3DColor(new Vector(-right, back, bottom), color)
			};
			return vertices;
		}

		private static readonly short[] BoxIndices = new short[]
		{
			0, 1, 2, 2, 1, 3, 4, 5, 6, 6, 5, 7, 5, 0, 7, 7, 0, 2, 1, 4, 3, 3, 4, 6, 5, 4, 0, 0, 4, 1, 6
			, 7, 3, 3, 7, 2
		};
	}
}