using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace YamLib
{
	public static class ToolBox
	{
		public static T Deserialize<T>(byte[] xmlData)
		{
			if (xmlData == null)
				return default;

			var stringReader = new StringReader(Encoding.UTF8.GetString(xmlData));
			var serializer = new XmlSerializer(typeof(T));
			return (T) serializer.Deserialize(stringReader);
		}

		public static byte[] Serialize<T>(T dataToSerialize)
		{
			if (dataToSerialize == null)
				return default;

			var stringwriter = new StringWriter();
			var serializer = new XmlSerializer(typeof(T));
			serializer.Serialize(stringwriter, dataToSerialize);
			return Encoding.UTF8.GetBytes(stringwriter.ToString());
		}

		public static T[] CompileArrays<T>(T[] array, T[] appendArray, params T[][] additional)
		{
			long resultLength = array.Length + appendArray.Length;
			var offsetSize = resultLength;

			foreach (var arr in additional) resultLength += arr.Length;

			var result = new T[resultLength];

			array.CopyTo(result, 0);
			appendArray.CopyTo(result, array.Length);

			foreach (var arr in additional)
			{
				arr.CopyTo(result, offsetSize);
				offsetSize += arr.Length;
			}

			return result;
		}

		public static T[] SubArray<T>(this T[] data, int index, int length)
		{
			var result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}

		public static object ObjToArr(object obj)
		{
			return new object[1] {obj};
		}

		public static object ArrToObj(object obj)
		{
			return !(obj is Array) || ((object[]) obj).Length < 1 ? null : ((object[]) obj)[0];
		}

		/// <summary>
		///     Turns a object into an array with the size of 1
		/// </summary>
		/// <typeparam name="T">
		///     Type of input object
		/// </typeparam>
		/// <param name="obj">
		///     Object to put into array
		/// </param>
		/// <returns>
		///     New array of <typeparamref name="T" />, with object inside, or null if input is invalid
		/// </returns>
		public static object ObjToArr<T>(object obj)
		{
			return !(obj is T) ? null : new T[1] {(T) obj};
		}

		/// <summary>
		///     Turns an array with the size of 1 to an object
		/// </summary>
		/// <typeparam name="T">
		///     Type output array should be made of
		/// </typeparam>
		/// <param name="obj">
		///     Array object to get child from
		/// </param>
		/// <returns>
		///     Child of input array, or null if input is invalid
		/// </returns>
		public static object ArrToObj<T>(object obj)
		{
			return !(obj is T[]) || ((T[]) obj).Length < 1 ? null : (object) ((T[]) obj)[0];
		}

		public static byte[] ToNetworkLayer(int value)
		{
			var bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);
			return bytes;
		}

		public static int ToHostLayer(byte[] bytes)
		{
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);
			return BitConverter.ToInt32(bytes, 0);
		}

		public static string MakeConsoleSafe(string potentiallyUnsafeString)
		{
			if (String.IsNullOrEmpty(potentiallyUnsafeString))
				return potentiallyUnsafeString;

			var bytes = Encoding.Unicode.GetBytes(potentiallyUnsafeString);
			var asciiBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, bytes);
			return Encoding.ASCII.GetString(asciiBytes);
		}

		public static string FormatLog(ulong num, char suffix)
		{
			int i = 0;
			float fnum = (float)num;

			for (; fnum > 1024; i++)
				fnum /= 1024;

			string format = "###0.00";
			switch (i)
			{
				case 0:
					return $"{fnum.ToString(format)} {suffix}";
				case 1:
					return $"{fnum.ToString(format)} K{suffix}";
				case 2:
					return $"{fnum.ToString(format)} M{suffix}";
				case 3:
					return $"{fnum.ToString(format)} G{suffix}";
				case 4:
					return $"{fnum.ToString(format)} T{suffix}";
				case 5:
					return $"{fnum.ToString(format)} P{suffix}";
				default:
					return $"{fnum.ToString(format)} E{suffix}";
			}
			/*
				case 7:
					return $"{fnum.ToString("0000.00")} Z{suffix}";
				case 8:
					return $"{fnum.ToString("0000.00")} Y{suffix}";
				case 9:
					return $"{fnum.ToString("0000.00")} B{suffix}";
				default:
					return $"{fnum.ToString("0000.00")} WTF{suffix}";
			*/
		}
	}
}
 