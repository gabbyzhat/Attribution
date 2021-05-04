using System;
using System.Collections.Generic;
using System.Text;
using Gabbyz.Attribution;
using NUnit.Framework;

namespace Gabbyz.Attribution.Tests
{
	public class OptionTests
	{

		[Option(Long = "string", Short = 's')]
		public static string StringTest
		{
			set
			{
				Assert.AreEqual("im a string", value);
			}
		}

		[Option(Long = "double", Short = 'd')]
		public static double DoubleTest
		{
			set
			{
				Assert.AreEqual(2d, value);
			}
		}

		[Option(Long = "float", Short = 'f')]
		public static float FloatTest
		{
			set
			{
				Assert.AreEqual(4f, value);
			}
		}

		[Option(Long = "bool", Flag = true, Short = 'b')]
		public static bool FlagTest
		{
			set
			{
				Assert.AreEqual(true, value);
			}
		}

		[DefaultCommand]
		public static void Default(params string[] args)
		{

		}

		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void TestAll()
		{
			Loader.Main(typeof(OptionTests), new string[] {"--string", "im a string", "--double", "2", "--float", "4", "--bool" });
			Loader.Main(typeof(OptionTests), new string[] { "-s", "im a string", "-d", "2", "-f", "4", "-b" });
		}
		
		[Test]
		public void TestUnrecognized()
		{
			try
			{
				Loader.Main(typeof(OptionTests), new string[] { "--int", "8" });
			}
			catch(LoaderException ex)
			{
				var m = ex.Message;
				Assert.IsTrue(m.Contains("unrecognized"));
				Assert.IsTrue(m.Contains("--int"));
				return;
			}
			Assert.Fail();
		}

		[Test]
		public void TestUnrecognizedShort()
		{
			try
			{
				Loader.Main(typeof(OptionTests), new string[] { "-i", "8" });
			}
			catch (LoaderException ex)
			{
				var m = ex.Message;
				Assert.IsTrue(m.Contains("unrecognized"));
				Assert.IsTrue(m.Contains("-i"));
				return;
			}
			Assert.Fail();
		}
	}
}
