using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Gabbyz.Attribution
{
	public static class Parse
	{
		/// <summary>
		/// Parses a string.
		/// </summary>
		/// <param name="s">The string.</param>
		/// <returns>The same string.</returns>
		public static string ParseString(string s) => s;
		
		/// <summary>
		/// Parses a byte with hexadecimal, octal and binary modifiers.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static byte ParseByte(string s)
		{
			if (s.StartsWith("0x") || s.StartsWith("0X"))
				return Convert.ToByte(s[2..], 16);
			else if (s.StartsWith("0b") || s.StartsWith("0B"))
				return Convert.ToByte(s[2..], 2);
			else if (s.StartsWith("0o") || s.StartsWith("0O"))
				return Convert.ToByte(s[2..], 8);
			else
				return Convert.ToByte(s);
		}

		/// <summary>
		/// Parses a signed byte with hexadecimal, octal and binary modifiers.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static sbyte ParseSByte(string s)
		{
			if (s.StartsWith("0x") || s.StartsWith("0X"))
				return Convert.ToSByte(s[2..], 16);
			else if (s.StartsWith("0b") || s.StartsWith("0B"))
				return Convert.ToSByte(s[2..], 2);
			else if (s.StartsWith("0o") || s.StartsWith("0O"))
				return Convert.ToSByte(s[2..], 8);
			else
				return Convert.ToSByte(s);
		}

		/// <summary>
		/// Parses an unsigned short with hexadecimal, octal and binary modifiers.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static ushort ParseUShort(string s)
		{
			if (s.StartsWith("0x") || s.StartsWith("0X"))
				return Convert.ToUInt16(s[2..], 16);
			else if (s.StartsWith("0b") || s.StartsWith("0B"))
				return Convert.ToUInt16(s[2..], 2);
			else if (s.StartsWith("0o") || s.StartsWith("0O"))
				return Convert.ToUInt16(s[2..], 8);
			else
				return Convert.ToUInt16(s);
		}

		/// <summary>
		/// Parses a signed short with hexadecimal, octal and binary modifiers.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static short ParseShort(string s)
		{
			if (s.StartsWith("0x") || s.StartsWith("0X"))
				return Convert.ToInt16(s[2..], 16);
			else if (s.StartsWith("0b") || s.StartsWith("0B"))
				return Convert.ToInt16(s[2..], 2);
			else if (s.StartsWith("0o") || s.StartsWith("0O"))
				return Convert.ToInt16(s[2..], 8);
			else
				return Convert.ToInt16(s);
		}

		/// <summary>
		/// Parses an unsigned integer with hexadecimal, octal and binary modifiers.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static uint ParseUInt(string s)
		{
			if (s.StartsWith("0x") || s.StartsWith("0X"))
				return Convert.ToUInt32(s[2..], 16);
			else if (s.StartsWith("0b") || s.StartsWith("0B"))
				return Convert.ToUInt32(s[2..], 2);
			else if (s.StartsWith("0o") || s.StartsWith("0O"))
				return Convert.ToUInt32(s[2..], 8);
			else
				return Convert.ToUInt32(s);
		}

		/// <summary>
		/// Parses a signed integer with hexadecimal, octal and binary modifiers.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static int ParseInt(string s)
		{
			if (s.StartsWith("0x") || s.StartsWith("0X"))
				return Convert.ToInt32(s[2..], 16);
			else if (s.StartsWith("0b") || s.StartsWith("0B"))
				return Convert.ToInt32(s[2..], 2);
			else if (s.StartsWith("0o") || s.StartsWith("0O"))
				return Convert.ToInt32(s[2..], 8);
			else
				return Convert.ToInt32(s);
		}

		/// <summary>
		/// Parses an unsigned long with hexadecimal, octal and binary modifiers.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static ulong ParseULong(string s)
		{
			if (s.StartsWith("0x") || s.StartsWith("0X"))
				return Convert.ToUInt64(s[2..], 16);
			else if (s.StartsWith("0b") || s.StartsWith("0B"))
				return Convert.ToUInt64(s[2..], 2);
			else if (s.StartsWith("0o") || s.StartsWith("0O"))
				return Convert.ToUInt64(s[2..], 8);
			else
				return Convert.ToUInt64(s);
		}

		/// <summary>
		/// Parses a signed long with hexadecimal, octal and binary modifiers.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static long ParseLong(string s)
		{
			if (s.StartsWith("0x") || s.StartsWith("0X"))
				return Convert.ToInt64(s[2..], 16);
			else if (s.StartsWith("0b") || s.StartsWith("0B"))
				return Convert.ToInt64(s[2..], 2);
			else if (s.StartsWith("0o") || s.StartsWith("0O"))
				return Convert.ToInt64(s[2..], 8);
			else
				return Convert.ToInt64(s);
		}

		/// <summary>
		/// Parses a float.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static float ParseFloat(string s) => Convert.ToSingle(s);

		/// <summary>
		/// Parses a double.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static double ParseDouble(string s) => Convert.ToDouble(s);

		/// <summary>
		/// Parses a bool, accepting case insensitive yes, no, true, false, t, f, y and n.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static bool ParseBool(string s)
		{
			if (string.Compare(s, "true", true) == 0)
			{
				return true;
			}
			else if (string.Compare(s, "false", true) == 0)
			{
				return false;
			}
			else if (string.Compare(s, "t", true) == 0)
			{
				return true;
			}
			else if (string.Compare(s, "f", true) == 0)
			{
				return false;
			}
			else if (string.Compare(s, "yes", true) == 0)
			{
				return true;
			}
			else if (string.Compare(s, "no", true) == 0)
			{
				return false;
			}
			else if (string.Compare(s, "y", true) == 0)
			{
				return true;
			}
			else if (string.Compare(s, "n", true) == 0)
			{
				return false;
			}

			throw new FormatException($"{s} is not a valid boolean (true/false/yes/no/t/f/y/n)");
		}

		/// <summary>
		/// Parses a FileInfo.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static FileInfo ParseFileInfo(string s) => new FileInfo(s);

		/// <summary>
		/// Parses a DirectoryInfo.
		/// </summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The parsed value.</returns>
		public static DirectoryInfo ParseDirectoryInfo(string s) => new DirectoryInfo(s);
	}
}
