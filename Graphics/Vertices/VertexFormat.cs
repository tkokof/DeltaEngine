﻿using System;
using System.Linq;
using DeltaEngine.Extensions;

namespace DeltaEngine.Graphics.Vertices
{
	/// <summary>
	/// The format for a vertex. eg. it contains position, color and texture.
	/// </summary>
	public class VertexFormat : IEquatable<VertexFormat>
	{
		private VertexFormat() {}

		public VertexFormat(VertexElement[] elements)
		{
			Elements = elements;
			foreach (var vertexElement in elements)
				ComputeElementOffset(vertexElement);
		}

		public VertexElement[] Elements { get; private set; }

		private void ComputeElementOffset(VertexElement vertexElement)
		{
			vertexElement.Offset = Stride;
			Stride += vertexElement.Size;
		}

		public int Stride { get; private set; }

		public bool Is3D
		{
			get { return Elements.Any(t => t.ElementType == VertexElementType.Position3D); }
		}
		public bool HasColor
		{
			get { return Elements.Any(t => t.ElementType == VertexElementType.Color); }
		}
		public bool HasUV
		{
			get { return Elements.Any(t => t.ElementType == VertexElementType.TextureUV); }
		}

		public VertexElement GetElementFromType(VertexElementType type)
		{
			return Elements.FirstOrDefault(vertexElement => vertexElement.ElementType == type);
		}

		public static bool operator ==(VertexFormat f1, VertexFormat f2)
		{
			return (object)f1 != null && f1.Equals(f2);
		}

		public bool Equals(VertexFormat other)
		{
			if ((object)other == null || Elements.Length != other.Elements.Length)
				return false;
			for (int i = 0; i < Elements.Length; ++i)
				if (!Elements[i].Equals(other.Elements[i]))
					return false;
			return true;
		}

		public static bool operator !=(VertexFormat f1, VertexFormat f2)
		{
			return (object)f1 == null || !f1.Equals(f2);
		}

		public override bool Equals(object other)
		{
			return other is VertexFormat && Equals((VertexFormat)other);
		}

		public override int GetHashCode()
		{
			return Elements.GetHashCode() ^ Stride.GetHashCode();
		}

		public override string ToString()
		{
			return "VertexFormat: " + Elements.ToText() + ", Stride=" + Stride;
		}

		public static readonly VertexFormat Position2DUv =
			new VertexFormat(new[]
			{
				new VertexElement(VertexElementType.Position2D),
				new VertexElement(VertexElementType.TextureUV)
			});

		public static readonly VertexFormat Position2DColor =
			new VertexFormat(new[]
			{
				new VertexElement(VertexElementType.Position2D),
				new VertexElement(VertexElementType.Color)
			});

		public static readonly VertexFormat Position2DColorUv =
			new VertexFormat(new[]
			{
				new VertexElement(VertexElementType.Position2D),
				new VertexElement(VertexElementType.Color),
				new VertexElement(VertexElementType.TextureUV)
			});

		public static readonly VertexFormat Position3DUv =
			new VertexFormat(new[]
			{
				new VertexElement(VertexElementType.Position3D),
				new VertexElement(VertexElementType.TextureUV)
			});

		public static readonly VertexFormat Position3DColor =
			new VertexFormat(new[]
			{
				new VertexElement(VertexElementType.Position3D),
				new VertexElement(VertexElementType.Color)
			});

		public static readonly VertexFormat Position3DColorUv =
			new VertexFormat(new[]
			{
				new VertexElement(VertexElementType.Position3D),
				new VertexElement(VertexElementType.Color),
				new VertexElement(VertexElementType.TextureUV)
			});
	}
}