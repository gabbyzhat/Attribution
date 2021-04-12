using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace Gabbyz.Attribution
{
	/// <summary>
	/// The command line loader.
	/// </summary>
	public static class Loader
	{
		/// <summary>
		/// Displays help.
		/// </summary>
		/// <param name="type">The type to manage.</param>
		/// <param name="tw">The text writer to output to.</param>
		public static void Help(Type type, TextWriter tw)
		{
			var name = Process.GetCurrentProcess().ProcessName;
			

			var typeHelp = type.GetCustomAttribute<HelpAttribute>();
			if (typeHelp != null)
			{
				tw.WriteLine(name);
				tw.WriteLine($"\t{typeHelp.Text}");
			}
			else
			{
				tw.WriteLine(name);
			}

			tw.WriteLine();

			foreach(var x in type.GetMethods().Where(x => x.IsStatic))
			{
				var dcAttr = x.GetCustomAttribute<DefaultCommandAttribute>();
				if (dcAttr == null)
					continue;

				tw.Write("USAGE: {0} ", name);
				
				foreach(var y in x.GetParameters())
				{
					if (y.GetCustomAttribute<ParamArrayAttribute>() != null)
						tw.Write($"[{y.Name}...]");
					else
						tw.Write($"{y.Name} ");
				}

				tw.WriteLine();

				var helpAttr = x.GetCustomAttribute<HelpAttribute>();

				if (helpAttr != null)
					tw.WriteLine(helpAttr.Text);
			}

			tw.WriteLine("Commands:");

			foreach(var x in type.GetMethods().Where(x => x.IsStatic))
			{
				var commandAttr = x.GetCustomAttribute<CommandAttribute>();

				if (commandAttr == null)
					continue;

				var helpAttr = x.GetCustomAttribute<HelpAttribute>();

				tw.Write($"{commandAttr.Name} ");

				foreach (var y in x.GetParameters())
				{
					if (y.GetCustomAttribute<ParamArrayAttribute>() != null)
						tw.Write($"[{y.Name}...]");
					else
						tw.Write($"{y.Name} ");
				}

				tw.WriteLine();

				if (helpAttr != null)
					tw.WriteLine($"\t{helpAttr.Text}");


			}

			tw.WriteLine();
			tw.WriteLine("Options:");
			
			foreach(var x in type.GetProperties())
			{

				var optionAttr = x.GetCustomAttribute<OptionAttribute>();

				if (optionAttr == null)
					continue;

				if (optionAttr.Long != null)
				{
					tw.Write("--{0} ", optionAttr.Long);
				}
				if (optionAttr.Short != '\0')
				{
					tw.Write("-{0} ", optionAttr.Short);
				}

				if (optionAttr.Flag)
				{
					tw.Write("(flag)");
				}

				tw.WriteLine();

				var helpAttr = x.GetCustomAttribute<HelpAttribute>();

				if (helpAttr != null)
				{
					tw.WriteLine("\t{0}", helpAttr.Text);
				}
			}

			foreach (var x in type.GetMethods().Where(x => x.IsStatic))
			{
				var optionAttr = x.GetCustomAttribute<OptionAttribute>();

				if (optionAttr == null)
					continue;

				if (optionAttr.Long != null)
				{
					tw.Write("--{0} ", optionAttr.Long);
				}
				if (optionAttr.Short != '\0')
				{
					tw.Write("-{0} ", optionAttr.Short);
				}

				var par = x.GetParameters();

				if (optionAttr.Flag)
				{
					tw.Write("(flag)");
				}
				else if (par.Length > 1)
				{
					foreach(var parameter in par)
					{
						tw.Write("{0} ", parameter.Name);
					}
				}

				tw.WriteLine();

				var helpAttr = x.GetCustomAttribute<HelpAttribute>();

				if (helpAttr != null)
				{
					tw.WriteLine("\t{0}", helpAttr.Text);
				}
			}
		}

		class Subloader
		{
			public static string ParseString(string s) => s;
			public static byte ParseByte(string s) => byte.Parse(s);
			public static sbyte ParseSByte(string s) => sbyte.Parse(s);
			public static ushort ParseUShort(string s) => ushort.Parse(s);
			public static short ParseShort(string s) => short.Parse(s);
			public static uint ParseUInt(string s) => uint.Parse(s);
			public static int ParseInt(string s) => int.Parse(s);
			public static ulong ParseULong(string s) => ulong.Parse(s);
			public static long ParseLong(string s) => long.Parse(s);
			public static float ParseFloat(string s) => float.Parse(s);
			public static double ParseDouble(string s) => double.Parse(s);
		}




		static void ParseInvoke(MethodInfo method, List<string> values, Dictionary<Type, MethodInfo> parsers)
		{
			var parameters = method.GetParameters();

			if (parameters.Length == 0)
			{
				method.Invoke(null, new object[0]);
				return;
			}

			var args = new object[parameters.Length];

			for(var i = 0; i < parameters.Length; i++)
			{
				try
				{
					if (i < values.Count)
					{
						args[i] = parsers[parameters[i].ParameterType].Invoke(null, new[] { values[i] });
					}
					else if (parameters[i].IsOptional)
					{
						args[i] = parameters[i].DefaultValue;
					}
					else
					{
						throw new LoaderException($"missing parameter {i+1}: {parameters[i].Name}");
					}
				}
				catch(LoaderException ex)
				{
					throw ex;
				}
				catch(Exception ex)
				{
					throw new LoaderException($"parsing parameter #{i+1}: {parameters[i].Name}: {ex}", ex);
				}
			}

			method.Invoke(null, args);
		}
		static void ParseInvokeParams(MethodInfo method, List<string> values, Dictionary<Type, MethodInfo> parsers)
		{
			var parameters = method.GetParameters();

			if (parameters.Length == 0)
			{
				method.Invoke(null, new object[0]);
				return;
			}

			var last = parameters.Last();

			if (last.GetCustomAttribute<ParamArrayAttribute>() == null)
			{
				ParseInvoke(method, values, parsers);
				return;
			}

			var args = new object[parameters.Length];

			for (var i = 0; i < parameters.Length - 1; i++)
			{
				try
				{
					if (i < values.Count)
					{
						args[i] = parsers[parameters[i].ParameterType].Invoke(null, new[] { values[i] });
					}
					else if (parameters[i].IsOptional)
					{
						args[i] = parameters[i].DefaultValue;
					}
					else
					{
						throw new LoaderException($"missing parameter {i + 1}: {parameters[i].Name}");
					}
				}
				catch (LoaderException ex)
				{
					throw ex;
				}
				catch (Exception ex)
				{
					throw new LoaderException($"parsing parameter #{i + 1}: {parameters[i].Name}: {ex}", ex);
				}
			}

			var elem = last.ParameterType.GetElementType();
			var objs = new List<object>();
			for (var i = parameters.Length - 1; i < values.Count; i++)
			{
				try
				{
					objs.Add(parsers[elem].Invoke(null, new[] { values[i] }));
				}
				catch (Exception ex)
				{
					throw new LoaderException($"parsing variadic parameter #{i - parameters.Length + 2}: {last.Name}: {ex}", ex);
				}
			}

			Array destinationArray = Array.CreateInstance(elem, objs.Count);
			Array.Copy(objs.ToArray(), destinationArray, destinationArray.Length);

			args[args.Length - 1] = destinationArray;

			method.Invoke(null, args);
		}

		/// <summary>
		/// The main method.
		/// </summary>
		/// <param name="type">The type to manage.</param>
		/// <param name="args">The arguments.</param>
		public static void Main(Type type, string[] args)
		{
			var parsers = new Dictionary<Type, MethodInfo>();
			var longs = new Dictionary<string, (bool, MethodInfo)>();
			var shorts = new Dictionary<char, (bool, MethodInfo)>();


			var commands = new Dictionary<string, MethodInfo>();

			parsers.Add(typeof(string), typeof(Subloader).GetMethod("ParseString"));
			parsers.Add(typeof(byte), typeof(Subloader).GetMethod("ParseByte"));
			parsers.Add(typeof(sbyte), typeof(Subloader).GetMethod("ParseSByte"));
			parsers.Add(typeof(ushort), typeof(Subloader).GetMethod("ParseUShort"));
			parsers.Add(typeof(short), typeof(Subloader).GetMethod("ParseShort"));
			parsers.Add(typeof(uint), typeof(Subloader).GetMethod("ParseUInt"));
			parsers.Add(typeof(int), typeof(Subloader).GetMethod("ParseInt"));
			parsers.Add(typeof(float), typeof(Subloader).GetMethod("ParseFloat"));
			parsers.Add(typeof(double), typeof(Subloader).GetMethod("ParseDouble"));


			foreach (var x in type.GetMethods().Where(x => x.IsStatic))
			{
				var commandAttr = x.GetCustomAttribute<CommandAttribute>();
				if (commandAttr != null)
				{
					commands.Add(commandAttr.Name, x);
				}
				var optionAttr = x.GetCustomAttribute<OptionAttribute>();
				if (optionAttr != null)
				{
					if (optionAttr.Long != null)
					{
						longs.Add(optionAttr.Long, (optionAttr.Flag, x));
					}
					if (optionAttr.Short != '\0')
					{
						shorts.Add(optionAttr.Short, (optionAttr.Flag, x));
					}
				}
				var parserAttr = x.GetCustomAttribute<ParserAttribute>();
				if (parserAttr != null)
				{
					parsers.Add(x.ReturnType, x);
				}
			}
			foreach (var x in type.GetProperties().Where(x => x.SetMethod != null && x.SetMethod.IsStatic))
			{
				var optionAttr = x.GetCustomAttribute<OptionAttribute>();
				if (optionAttr != null)
				{
					if (optionAttr.Long != null)
					{
						longs.Add(optionAttr.Long, (optionAttr.Flag, x.SetMethod));
					}
					if (optionAttr.Short != '\0')
					{
						shorts.Add(optionAttr.Short, (optionAttr.Flag, x.SetMethod));
					}
				}
			}

			string command = null;
			MethodInfo option = null;
			bool commandLock = false;
			var optionArgs = new List<string>();
			var commandArgs = new List<string>();
			ParameterInfo[] optionParams = null;

			// process options and collect command args
			foreach (var arg in args)
			{
				if (commandLock)
				{
					commandArgs.Add(arg);
					continue;
				}


				if (option != null)
				{
					optionArgs.Add(arg);
					if (optionArgs.Count == optionParams.Length)
					{
						ParseInvoke(option, optionArgs, parsers);
						option = null;
					}
					if (optionArgs.Count > optionParams.Length)
					{
						throw new LoaderException("this should never happen");
					}
				}
				else if (arg == "--")
				{
					commandLock = true;
				}
				else if (arg == "--help")
				{
					Help(type, Console.Out);
					return;
				}
				else if (arg.StartsWith("--"))
				{
					string optionName;
					string optionValue = null;
					var eq = arg.IndexOf('=');
					
					if (eq != -1)
					{
						optionName = arg.Substring(2, eq - 2);
						optionValue = arg.Substring(eq + 1);
					}
					else
					{
						optionName = arg.Substring(2);
					}

					var (flag, opt) = longs.GetValueOrDefault(optionName);
					option = opt;

					if (option == null)
					{
						throw new LoaderException($"unrecognized option: --{optionName}");
					}

					if (optionValue != null)
					{
						var parameters = option.GetParameters();
						if (parameters.Length == 0)
						{
							throw new LoaderException($"option does not take parameters: --{optionName}");
						}
						else if (parameters.Length == 1)
						{
							optionArgs.Clear();
							optionArgs.Add(optionValue);
							ParseInvoke(option, optionArgs, parsers);
							option = null;
						}
						else
						{
							optionArgs.Clear();
							optionArgs.Add(optionValue);
							optionParams = option.GetParameters();
						}
					}
					else
					{
						if (option.GetParameters().Length == 0)
						{
							option.Invoke(null, new object[0]);
							option = null;
						}
						else if (flag)
						{
							option.Invoke(null, new object[] { true });
							option = null;
						}
						else
						{
							optionArgs.Clear();
							optionParams = option.GetParameters();
						}
					}
				}
				else if (arg.StartsWith("-"))
				{
					var optionNames = arg.Substring(1);

					if (optionNames.Length == 1)
					{
						var (flag, opt) = shorts.GetValueOrDefault(optionNames[0]);
						option = opt;

						if (option == null)
						{
							throw new LoaderException($"unrecognized option: -{optionNames}");
						}
						else if (flag)
						{
							option.Invoke(null, new object[] { true });
							option = null;
						}
						else
						{
							optionArgs.Clear();
							optionParams = option.GetParameters();

							if (optionParams.Length == 0)
							{
								option.Invoke(null, new object[0]);
								option = null;
							}
						}
					}
					else
					{
						foreach(var name in optionNames)
						{
							var (flag, opt) = shorts.GetValueOrDefault(optionNames[0]);

							if (opt == null)
								throw new LoaderException($"unrecognized option: -{optionNames[0]}");

							if (!flag)
								throw new LoaderException($"only flag options allowed in a multiple set");

							opt.Invoke(null, new object[] { true });
						}
					}
				}
				else
				{
					if (command == null)
					{
						command = arg;
					}
					else
						commandArgs.Add(arg);
				}

			}

			MethodInfo realCommand;

			if (command == null || command == "")
			{
				realCommand = type.GetMethods().Where(x => x.GetCustomAttribute<DefaultCommandAttribute>() != null).FirstOrDefault();
				if (realCommand == null)
					throw new LoaderException("no command given");
				commandArgs.Insert(0, command);
			}
			else
			{
				realCommand = commands.GetValueOrDefault(command);
				if (realCommand == null)
				{
					realCommand = type.GetMethods().Where(x => x.GetCustomAttribute<DefaultCommandAttribute>() != null).FirstOrDefault();
					commandArgs.Insert(0, command);
				}
				if (realCommand == null)
					throw new LoaderException($"unrecognized command: {command}");
			}

			ParseInvokeParams(realCommand, commandArgs, parsers);
		}
	}
}
