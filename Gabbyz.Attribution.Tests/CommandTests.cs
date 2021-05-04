using System;
using System.Collections.Generic;
using System.Text;
using Gabbyz.Attribution;
using NUnit.Framework;

namespace Gabbyz.Attribution.Tests
{
	public class CommandTests
	{
		[Command("copy")]
		public static void Copy(string source, string destination, int make5)
		{
			Assert.AreEqual("source123", source);
			Assert.AreEqual("dest123", destination);
			Assert.AreEqual(5, make5);
		}

		[Command("delete")]
		public static void Delete(string target)
		{
			Assert.AreEqual("targ", target);
		}

		[Test]
		public void TestCopy()
		{
			Loader.Main(typeof(CommandTests), new string[] {"copy", "source123", "dest123", "5"});
		}
		
		[Test]
		public void TestDelete()
		{
			Loader.Main(typeof(CommandTests), new string[] {"delete", "targ"});
		}
	}
}
