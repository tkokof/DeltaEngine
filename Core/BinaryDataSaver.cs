﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DeltaEngine.Content;

namespace DeltaEngine.Core
{
	/// <summary>
	/// Easily save data objects with the full type names like other Serializers, but much faster.
	/// </summary>
	internal static class BinaryDataSaver
	{
		internal static void TrySaveData(object data, Type type, BinaryWriter writer)
		{
			try
			{
				SaveData(data, type, writer);
			}
			catch (Exception ex)
			{
				throw new UnableToSave(data, ex);
			}
		}

		internal class UnableToSave : Exception
		{
			public UnableToSave(object data, Exception exception)
				: base(data.ToString(), exception) {}
		}

		private static void SaveData(object data, Type type, BinaryWriter writer)
		{
			if (data == null)
				throw new NullReferenceException();
			if (data is ContentData)
			{
				writer.Write((data as ContentData).Name);
				return;
			}
			if (type.Name.StartsWith("Xml"))
				throw new DoNotSaveXmlDataAsBinaryData(data);
			if (data is Stream && !(data is MemoryStream))
				throw new OnlyMemoryStreamSavingIsSupported(data);
			if (SaveIfIsPrimitiveData(data, type, writer))
				return;
			if (type == typeof(MemoryStream))
				SaveMemoryStream(data, writer);
			else if (type == typeof(byte[]))
				SaveByteArray(data, writer);
			else if (type == typeof(char[]))
				SaveCharArray(data, writer);
			else if (data is IList || type.IsArray)
				SaveArray(data as IList, writer);
			else if (data is IDictionary || type == typeof(Dictionary<,>))
				SaveDictionary(data as IDictionary, writer);
			else if (type.IsClass || type.IsValueType)
				SaveClassData(data, type, writer);
		}

		private class DoNotSaveXmlDataAsBinaryData : Exception
		{
			public DoNotSaveXmlDataAsBinaryData(object data)
				: base(data.ToString()) {}
		}

		private class OnlyMemoryStreamSavingIsSupported : Exception
		{
			public OnlyMemoryStreamSavingIsSupported(object data)
				: base(data.ToString()) {}
		}

		private static bool SaveIfIsPrimitiveData(object data, Type type, BinaryWriter writer)
		{
			switch (Type.GetTypeCode(type))
			{
			case TypeCode.Boolean:
				writer.Write((bool)data);
				return true;
			case TypeCode.Byte:
				writer.Write((byte)data);
				return true;
			case TypeCode.Char:
				writer.Write((char)data);
				return true;
			case TypeCode.Decimal:
				writer.Write((decimal)data);
				return true;
			case TypeCode.Double:
				writer.Write((double)data);
				return true;
			case TypeCode.Single:
				writer.Write((float)data);
				return true;
			case TypeCode.Int16:
				writer.Write((short)data);
				return true;
			case TypeCode.Int32:
				writer.Write((int)data);
				return true;
			case TypeCode.Int64:
				writer.Write((long)data);
				return true;
			case TypeCode.String:
				writer.Write((string)data);
				return true;
			case TypeCode.SByte:
				writer.Write((sbyte)data);
				return true;
			case TypeCode.UInt16:
				writer.Write((ushort)data);
				return true;
			case TypeCode.UInt32:
				writer.Write((uint)data);
				return true;
			case TypeCode.UInt64:
				writer.Write((ulong)data);
				return true;
			}
			return false;
		}

		private static void SaveMemoryStream(object data, BinaryWriter writer)
		{
			var stream = data as MemoryStream;
			writer.WriteNumberMostlyBelow255((int)stream.Length);
			writer.Write(stream.ToArray());
		}

		private static void SaveByteArray(object data, BinaryWriter writer)
		{
			writer.WriteNumberMostlyBelow255(((byte[])data).Length);
			writer.Write((byte[])data);
		}

		private static void SaveCharArray(object data, BinaryWriter writer)
		{
			writer.WriteNumberMostlyBelow255(((char[])data).Length);
			writer.Write((char[])data);
		}

		private static void SaveArray(IList list, BinaryWriter writer)
		{
			writer.WriteNumberMostlyBelow255(list.Count);
			if (list.Count == 0)
				return;
			if (AreAllElementsTheSameType(list))
				SaveArrayWhenAllElementsAreTheSameType(list, writer);
			else
				SaveArrayWhenAllElementsAreNotTheSameType(list, writer);
		}

		internal static void WriteNumberMostlyBelow255(this BinaryWriter writer, int number)
		{
			if (number < 255)
				writer.Write((byte)number);
			else
			{
				writer.Write((byte)255);
				writer.Write(number);
			}
		}

		private static bool AreAllElementsTheSameType(IList list)
		{
			var firstType = BinaryDataExtensions.GetTypeOrObjectType(list[0]);
			foreach (object element in list)
				if (BinaryDataExtensions.GetTypeOrObjectType(element) != firstType)
					return false;
			return true;
		}

		private static void SaveArrayWhenAllElementsAreTheSameType(IList list, BinaryWriter writer)
		{
			var arrayType = list[0] != null && list[0].GetType().Name == "RuntimeType"
				? ArrayElementType.AllTypesAreTypes : ArrayElementType.AllTypesAreTheSame;
			var firstElementType = BinaryDataExtensions.GetTypeOrObjectType(list[0]);
			if (arrayType == ArrayElementType.AllTypesAreTheSame && firstElementType == typeof(object) &&
				list[0] == null)
				arrayType = ArrayElementType.AllTypesAreNull;
			writer.Write((byte)arrayType);
			if (arrayType == ArrayElementType.AllTypesAreNull)
				return;
			if (arrayType == ArrayElementType.AllTypesAreTheSame)
				writer.Write(list[0].GetShortNameOrFullNameIfNotFound());
			foreach (object value in list)
				TrySaveData(value, firstElementType, writer);
		}

		private static void SaveArrayWhenAllElementsAreNotTheSameType(IEnumerable list,
			BinaryWriter writer)
		{
			writer.Write((byte)ArrayElementType.AllTypesAreDifferent);
			foreach (object value in list)
				SaveElementWithItsType(writer, value);
		}

		public enum ArrayElementType : byte
		{
			AllTypesAreDifferent,
			AllTypesAreTypes,
			AllTypesAreTheSame,
			AllTypesAreNull
		}

		private static void SaveElementWithItsType(BinaryWriter writer, object value)
		{
			writer.Write(value.GetShortNameOrFullNameIfNotFound());
			writer.Write(value != null);
			if (value != null)
				TrySaveData(value, BinaryDataExtensions.GetTypeOrObjectType(value), writer);
		}

		private static void SaveDictionary(IDictionary data, BinaryWriter writer)
		{
			writer.WriteNumberMostlyBelow255(data.Count);
			if (data.Count == 0)
				return;
			if (AreAllDictionaryValuesTheSameType(data))
				SaveDictionaryWhenAllValuesAreTheSameType(data, writer);
			else
				SaveDictionaryWhenAllValuesAreNotTheSameType(data, writer);
		}

		private static bool AreAllDictionaryValuesTheSameType(IDictionary data)
		{
			Type firstType = null;
			foreach (object element in data.Values)
			{
				if (firstType == null)
					firstType = BinaryDataExtensions.GetTypeOrObjectType(element);
				else if (BinaryDataExtensions.GetTypeOrObjectType(element) != firstType)
					return false;
			}
			return true;
		}

		private static void SaveDictionaryWhenAllValuesAreTheSameType(IDictionary data,
			BinaryWriter writer)
		{
			writer.Write((byte)ArrayElementType.AllTypesAreTheSame);
			Type keyType = null;
			Type valueType = null;
			var pair = data.GetEnumerator();
			while(pair.MoveNext())
			{
				if (keyType == null)
				{
					keyType = BinaryDataExtensions.GetTypeOrObjectType(pair.Key);
					valueType = BinaryDataExtensions.GetTypeOrObjectType(pair.Value);
					writer.Write(pair.Key.GetShortNameOrFullNameIfNotFound());
					writer.Write(pair.Value.GetShortNameOrFullNameIfNotFound());
				}
				TrySaveData(pair.Key, keyType, writer);
				TrySaveData(pair.Value, valueType, writer);
			}
		}

		private static void SaveDictionaryWhenAllValuesAreNotTheSameType(IDictionary data,
			BinaryWriter writer)
		{
			writer.Write((byte)ArrayElementType.AllTypesAreDifferent);
			Type keyType = null;
			var pair = data.GetEnumerator();
			while (pair.MoveNext())
			{
				if (keyType == null)
				{
					keyType = BinaryDataExtensions.GetTypeOrObjectType(pair.Key);
					writer.Write(pair.Key.GetShortNameOrFullNameIfNotFound());
				}
				writer.Write(pair.Value.GetShortNameOrFullNameIfNotFound());
				TrySaveData(pair.Key, keyType, writer);
				TrySaveData(pair.Value, BinaryDataExtensions.GetTypeOrObjectType(pair.Value), writer);
			}
		}

		private static void SaveClassData(object data, Type type, BinaryWriter writer)
		{
			foreach (FieldInfo field in type.GetBackingFields())
			{
				object fieldData = field.GetValue(data);
				Type fieldType = field.FieldType;
				if (fieldType.DoNotNeedToSaveType() || fieldType == type)
					continue;
				if (fieldType.IsClass)
				{
					writer.Write(fieldData != null);
					if (fieldData == null)
						continue;
					fieldType = fieldData.GetType();
					if (fieldType.NeedToSaveTypeName())
						writer.Write(fieldData.GetShortNameOrFullNameIfNotFound());
				}
				TrySaveData(fieldData, fieldType, writer);
			}
		}
	}
}