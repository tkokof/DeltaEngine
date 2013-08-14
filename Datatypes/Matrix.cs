﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using DeltaEngine.Extensions;

namespace DeltaEngine.Datatypes
{
	/// <summary>
	/// 4x4 Matrix from 16 floats, access happens via indexer, optimizations done in BuildService.
	/// </summary>
	[DebuggerDisplay("Matrix(Right={Right},\nUp={Up},\nFront={Front},\nTranslation={Translation})")]
	public struct 
		Matrix : IEquatable<Matrix>
	{
		public Matrix(params float[] values)
			: this()
		{
			for (int i = 0; i < 16; i++)
				this[i] = values[i];
		}

		public float this[int index]
		{
			get
			{
				if (index >= 0 && index < 16)
					return GetValues[index];
				throw new IndexOutOfRangeException();
			}
			set
			{
				SetValue(index, value);
			}
		}

		public float[] GetValues
		{
			get
			{
				return new[]
				{ m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44 };
			}
		}

		private float m11;
		private float m12;
		private float m13;
		private float m14;
		private float m21;
		private float m22;
		private float m23;
		private float m24;
		private float m31;
		private float m32;
		private float m33;
		private float m34;
		private float m41;
		private float m42;
		private float m43;
		private float m44;

		private void SetValue(int index, float value)
		{
			if (index == 0) 
				m11 = value;
			else if (index == 1) 
				m12 = value;
			else if (index == 2) 
				m13 = value;
			else if (index == 3) 
				m14 = value; 
			else if (index == 4) 
				m21 = value;
			else if (index == 5) 
				m22 = value;
			else if (index == 6) 
				m23 = value;
			else if (index == 7) 
				m24 = value; 
			else if (index == 8) 
				m31 = value;
			else if (index == 9) 
				m32 = value;
			else if (index == 10) 
				m33 = value;
			else if (index == 11) 
				m34 = value; 
			else if (index == 12) 
				m41 = value;
			else if (index == 13) 
				m42 = value;
			else if (index == 14) 
				m43 = value;
			else if (index == 15) 
				m44 = value;
		}

		public Vector Right
		{
			get { return new Vector(m11, m12, m13);}
		}
		public Vector Up
		{
			get { return new Vector(m21, m22, m23); }
		}
		public Vector Front
		{
			get { return new Vector(m31, m32, m33); }
		}
		public Vector Translation
		{
			get { return new Vector(m41, m42, m43); }
			set
			{
				m41 = value.X;
				m42 = value.Y;
				m43 = value.Z;
			}
		}

		public static readonly Matrix Identity = 
			new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Matrix));

		public static Matrix CreateScale(float scaleX, float scaleY, float scaleZ)
		{
			return new Matrix(scaleX, 0, 0, 0, 0, scaleY, 0, 0, 0, 0, scaleZ, 0, 0, 0, 0, 1);
		}

		/// <summary>
		/// THe multiplication order is first Z, then Y and finally X
		/// </summary>
		public static Matrix CreateRotationZyx(float x, float y, float z)
		{
			var cx = MathExtensions.Cos(x);
			var sx = MathExtensions.Sin(x);
			var cy = MathExtensions.Cos(y);
			var sy = MathExtensions.Sin(y);
			var cz = MathExtensions.Cos(z);
			var sz = MathExtensions.Sin(z);
			return new Matrix(new[] {
				cy * cz, cy * sz, -sy, 0.0f,
				sx * sy * cz + cx * -sz, sx * sy * sz + cx * cz, sx * cy, 0.0f,
				cx * sy * cz + sx * sz, cx * sy * sz + -sx * cz, cx * cy, 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f});
		}

		public static Matrix CreateTranslation(float x, float y, float z)
		{
			return new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, x, y, z, 1);
		}

		public static Matrix Transpose(Matrix matrix)
		{
			return new Matrix(
				matrix[0], matrix[4], matrix[8], matrix[12],
				matrix[1], matrix[5], matrix[9], matrix[13],
				matrix[2], matrix[6], matrix[10], matrix[14],
				matrix[3], matrix[7], matrix[11], matrix[15]);
		}

		public static Matrix Invert(Matrix matrix)
		{
			var subFactors = new float[19];
			subFactors[0] = matrix.m33 * matrix.m44 - matrix.m43 * matrix.m34;
			subFactors[1] = matrix.m32 * matrix.m44 - matrix.m42 * matrix.m34;
			subFactors[2] = matrix.m32 * matrix.m43 - matrix.m42 * matrix.m33;
			subFactors[3] = matrix.m31 * matrix.m44 - matrix.m41 * matrix.m34;
			subFactors[4] = matrix.m31 * matrix.m43 - matrix.m41 * matrix.m33;
			subFactors[5] = matrix.m31 * matrix.m42 - matrix.m41 * matrix.m32;
			subFactors[6] = matrix.m23 * matrix.m44 - matrix.m43 * matrix.m24;
			subFactors[7] = matrix.m22 * matrix.m44 - matrix.m42 * matrix.m24;
			subFactors[8] = matrix.m22 * matrix.m43 - matrix.m42 * matrix.m23;
			subFactors[9] = matrix.m21 * matrix.m44 - matrix.m41 * matrix.m24;
			subFactors[10] = matrix.m21 * matrix.m43 - matrix.m41 * matrix.m23;
			subFactors[11] = matrix.m22 * matrix.m44 - matrix.m42 * matrix.m24;
			subFactors[12] = matrix.m21 * matrix.m42 - matrix.m41 * matrix.m22;
			subFactors[13] = matrix.m23 * matrix.m34 - matrix.m33 * matrix.m24;
			subFactors[14] = matrix.m22 * matrix.m34 - matrix.m32 * matrix.m24;
			subFactors[15] = matrix.m22 * matrix.m33 - matrix.m32 * matrix.m23;
			subFactors[16] = matrix.m21 * matrix.m34 - matrix.m31 * matrix.m24;
			subFactors[17] = matrix.m21 * matrix.m33 - matrix.m31 * matrix.m23;
			subFactors[18] = matrix.m21 * matrix.m32 - matrix.m31 * matrix.m22;
			var inverse = new Matrix(
				+(matrix.m22 * subFactors[0] - matrix.m23 * subFactors[1] + matrix.m24 * subFactors[2]),
				-(matrix.m12 * subFactors[0] - matrix.m13 * subFactors[1] + matrix.m14 * subFactors[2]),
				+(matrix.m12 * subFactors[6] - matrix.m13 * subFactors[7] + matrix.m14 * subFactors[8]),
				-(matrix.m12 * subFactors[13] - matrix.m13 * subFactors[14] + matrix.m14 * subFactors[15]),
				-(matrix.m21 * subFactors[0] - matrix.m23 * subFactors[3] + matrix.m24 * subFactors[4]),
				+(matrix.m11 * subFactors[0] - matrix.m13 * subFactors[3] + matrix.m14 * subFactors[4]),
				-(matrix.m11 * subFactors[6] - matrix.m13 * subFactors[9] + matrix.m14 * subFactors[10]),
				+(matrix.m11 * subFactors[13] - matrix.m13 * subFactors[16] + matrix.m14 * subFactors[17]),
				+(matrix.m21 * subFactors[1] - matrix.m22 * subFactors[3] + matrix.m24 * subFactors[5]),
				-(matrix.m11 * subFactors[1] - matrix.m12 * subFactors[3] + matrix.m14 * subFactors[5]),
				+(matrix.m11 * subFactors[11] - matrix.m12 * subFactors[9] + matrix.m14 * subFactors[12]),
				-(matrix.m11 * subFactors[14] - matrix.m12 * subFactors[16] + matrix.m14 * subFactors[18]),
				-(matrix.m21 * subFactors[2] - matrix.m22 * subFactors[4] + matrix.m23 * subFactors[5]),
				+(matrix.m11 * subFactors[2] - matrix.m12 * subFactors[4] + matrix.m13 * subFactors[5]),
				-(matrix.m11 * subFactors[8] - matrix.m12 * subFactors[10] + matrix.m13 * subFactors[12]),
				+(matrix.m11 * subFactors[15] - matrix.m12 * subFactors[17] + matrix.m13 * subFactors[18]));
			float determinant = matrix.m11 * inverse.m11 + matrix.m12 * inverse.m21 +
				matrix.m13 * inverse.m31 + matrix.m14 * inverse.m41;
			inverse /= determinant;
			return inverse;
		}

		public static Matrix CreatePerspective(float fieldOfView, float aspectRatio,
			float nearPlaneDistance, float farPlaneDistance)
		{
			float focalLength = 1.0f / MathExtensions.Tan(fieldOfView * 0.5f);
			float inverseDepth = 1.0f / (farPlaneDistance - nearPlaneDistance);
			return new Matrix(
				focalLength, 0.0f, 0.0f, 0.0f,
				0.0f, focalLength / aspectRatio, 0.0f, 0.0f,
				0.0f, 0.0f, -inverseDepth * (farPlaneDistance + nearPlaneDistance), -1.0f,
				0.0f, 0.0f, -inverseDepth * (2.0f * farPlaneDistance * nearPlaneDistance), 0.0f);
		}

		public static Matrix CreateOrthoProjection(Size viewportSize)
		{
			return new Matrix(
				2.0f / viewportSize.Width, 0.0f, 0.0f, 0.0f,
				0.0f, 2.0f / -viewportSize.Height, 0.0f, 0.0f,
				0.0f, 0.0f, -1.0f, 0.0f,
				-1.0f, 1.0f, 0.0f, 1.0f);
		}

		public static Matrix CreateOrthoProjection(Size viewportSize, float nearPlane, float farPlane)
		{
			var invDepth = 1.0f / (farPlane - nearPlane);
			return new Matrix(
				2.0f / viewportSize.Width, 0.0f, 0.0f, 0.0f,
				0.0f, 2.0f / viewportSize.Height, 0.0f, 0.0f,
				0.0f, 0.0f, -2.0f * invDepth, 0.0f,
				0.0f, 0.0f, -1.0f * (nearPlane + farPlane) * invDepth, 1.0f);
		}

		public static Matrix CreateLookAt(Vector cameraPosition, Vector cameraTarget, Vector cameraUp)
		{
			var forward = Vector.Normalize(cameraTarget - cameraPosition);
			var up = Vector.Normalize(cameraUp);
			var side = Vector.Cross(forward, up);
			up = Vector.Cross(side, forward);
			return new Matrix(
				side.X, up.X, -forward.X, 0.0f,
				side.Y, up.Y, -forward.Y, 0.0f,
				side.Z, up.Z, -forward.Z, 0.0f,
				-Vector.Dot(side, cameraPosition),
				-Vector.Dot(up, cameraPosition),
				Vector.Dot(forward, cameraPosition), 1.0f);
		}

		public static Vector TransformNormal(Vector normal, Matrix matrix)
		{
			return new Vector(
				normal.X * matrix.m11 + normal.Y * matrix.m21 + normal.Z * matrix.m31,
				normal.X * matrix.m12 + normal.Y * matrix.m22 + normal.Z * matrix.m32,
				normal.X * matrix.m13 + normal.Y * matrix.m23 + normal.Z * matrix.m33);
		}

		public static Vector TransformHomogeneousCoordinate(Vector coord, Matrix matrix)
		{
			var vector = new Vector(
				coord.X * matrix.m11 + coord.Y * matrix.m21 + coord.Z * matrix.m31 + matrix.m41,
				coord.X * matrix.m12 + coord.Y * matrix.m22 + coord.Z * matrix.m32 + matrix.m42,
				coord.X * matrix.m13 + coord.Y * matrix.m23 + coord.Z * matrix.m33 + matrix.m43);
			float w = coord.X * matrix.m14 + coord.Y * matrix.m24 + coord.Z * matrix.m34 + matrix.m44;
			return vector / w;
		}

		public static Matrix CreateRotationX(float degrees)
		{
			float cosValue = MathExtensions.Cos(degrees);
			float sinValue = MathExtensions.Sin(degrees);
			return new Matrix(
				1f, 0f, 0f, 0f,
				0f, cosValue, sinValue, 0f,
				0f, -sinValue, cosValue, 0f,
				0f, 0f, 0f, 1f);
		}

		public static Matrix CreateRotationY(float degrees)
		{
			float cosValue = MathExtensions.Cos(degrees);
			float sinValue = MathExtensions.Sin(degrees);
			return new Matrix(
				cosValue, 0f, -sinValue, 0f,
				0f, 1f, 0f, 0f,
				sinValue, 0f, cosValue, 0f,
				0f, 0f, 0f, 1f);
		}

		public static Matrix CreateRotationZ(float degrees)
		{
			float cosValue = MathExtensions.Cos(degrees);
			float sinValue = MathExtensions.Sin(degrees);
			return new Matrix(
				cosValue, sinValue, 0f, 0f,
				-sinValue, cosValue, 0f, 0f,
				0f, 0f, 1f, 0f,
				0f, 0f, 0f, 1f);
		}

		/// <summary>
		/// Further details on how to compute matrix from quaternion:
		/// http://renderfeather.googlecode.com/hg-history/034a1900d6e8b6c92440382658d2b01fc732c5de/Doc/optimized%2520Matrix%2520quaternion%2520conversion.pdf
		/// </summary>
		public static Matrix FromQuaternion(Quaternion quaternion)
		{
			float qxx = quaternion.X * quaternion.X;
			float qyy = quaternion.Y * quaternion.Y;
			float qzz = quaternion.Z * quaternion.Z;
			float qxy = quaternion.X * quaternion.Y;
			float qzw = quaternion.Z * quaternion.W;
			float qxz = quaternion.X * quaternion.Z;
			float qyw = quaternion.Y * quaternion.W;
			float qyz = quaternion.Y * quaternion.Z;
			float qxw = quaternion.X * quaternion.W;
			return new Matrix(
				1.0f - 2.0f * (qyy + qzz), 2.0f * (qxy + qzw), 2.0f * (qxz - qyw), 0.0f,
				2.0f * (qxy - qzw), 1.0f - 2.0f * (qxx + qzz), 2.0f * (qyz + qxw), 0.0f,
				2.0f * (qxz + qyw), 2.0f * (qyz - qxw), 1.0f - 2.0f * (qxx + qyy), 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f);
		}

		/// <summary>
		/// More details how to calculate Matrix Determinants: http://en.wikipedia.org/wiki/Determinant
		/// </summary>
		public float GetDeterminant()
		{
			float det33X44 = this[15] * this[10] - this[14] * this[11];
			float det32X44 = this[9] * this[15] - this[13] * this[11];
			float det32X43 = this[9] * this[14] - this[13] * this[10];
			float det31X44 = this[8] * this[15] - this[12] * this[11];
			float det31X43 = this[8] * this[14] - this[12] * this[10];
			float det31X42 = this[8] * this[13] - this[12] * this[9];
			float det11 = this[0] * (this[5] * det33X44 - this[6] * det32X44 + this[7] * det32X43);
			float det12 = this[1] * (this[4] * det33X44 - this[6] * det31X44 + this[7] * det31X43);
			float det13 = this[2] * (this[4] * det32X44 - this[5] * det31X44 + this[7] * det31X42);
			float det14 = this[3] * (this[4] * det32X43 - this[5] * det31X43 + this[6] * det31X42);
			return det11 - det12 + det13 - det14;
		}

		public static bool operator ==(Matrix matrix1, Matrix matrix2)
		{
			return matrix1.Equals(matrix2);
		}

		public bool Equals(Matrix other)
		{
			for (int i = 0; i < 16; i++)
				if (!this[i].IsNearlyEqual(other[i]))
					return false;
			return true;
		}

		public static bool operator !=(Matrix matrix1, Matrix matrix2)
		{
			return !(matrix1 == matrix2);
		}

		public bool IsNearlyEqual(Matrix matrix)
		{
			for (int i = 0; i < 16; i++)
				if (!this[i].IsNearlyEqual(matrix[i]))
					return false;

			return true;
		}

		public static Matrix operator /(Matrix matrix, float scalar)
		{
			var value = 1.0f / scalar;
			return new Matrix(
				matrix.m11 * value, matrix.m12 * value, matrix.m13 * value, matrix.m14 * value,
				matrix.m21 * value, matrix.m22 * value, matrix.m23 * value, matrix.m24 * value,
				matrix.m31 * value, matrix.m32 * value, matrix.m33 * value, matrix.m34 * value,
				matrix.m41 * value, matrix.m42 * value, matrix.m43 * value, matrix.m44 * value);
		}

		public static Vector operator *(Matrix matrix, Vector vector)
		{
			return new Vector(
				vector.X * matrix.m11 + vector.Y * matrix.m21 + vector.Z * matrix.m13 + matrix.m41,
				vector.X * matrix.m12 + vector.Y * matrix.m22 + vector.Z * matrix.m23 + matrix.m42,
				vector.X * matrix.m13 + vector.Y * matrix.m23 + vector.Z * matrix.m33 + matrix.m43);
		}

		public static Matrix operator*(Matrix matrix1, Matrix matrix2)
		{
			var result = new float[16];
			for (int i = 0; i < 4; i++)
				for (int j = 0; j < 4; j++)
					for (int k = 0; k < 4; k++)
						result[i * 4 + j] += matrix1[i * 4 + k] * matrix2[k * 4 + j];

			return new Matrix(result);
		}

		public override int GetHashCode()
		{
			return (int)GetValues.Aggregate((a, b) => a.GetHashCode() ^ b.GetHashCode());
		}

		public override bool Equals(object obj)
		{
			return obj is Matrix && Equals((Matrix)obj);
		}

		public override string ToString()
		{
			return string.Join(", ", GetValues);
		}
	}
}