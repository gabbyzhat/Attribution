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
				tw.WriteLine("{0} - {1}", name, typeHelp.Text);
			}
			else
			{
				tw.WriteLine(name);
			}

			foreach(var x in type.GetMethods())
			{
				var dcAttr = x.GetCustomAttribute<DefaultCommandAttribute>();
				if (dcAttr == null)
					continue;

				tw.Write("USAGE: {0} ", name);
				
				foreach(var y in x.GetParameters())
				{
					if (y.GetCustomAttribute<ParamArrayAttribute>() != null)
					{
						tw.Write("[{0}...]", y.Name);
					}
					else
					{
						tw.Write("{0} ", y.Name);
					}
				}

				tw.WriteLine();

				var helpAttr = x.GetCustomAttribute<HelpAttribute>();

				if (helpAttr != null)
					tw.WriteLine(helpAttr.Text);
			}

			tw.WriteLine("Commands:");

			foreach(var x in type.GetMethods())
			{
				var commandAttr = x.GetCustomAttribute<CommandAttribute>();

				if (commandAttr == null)
					continue;

				var helpAttr = x.GetCustomAttribute<HelpAttribute>();

				if (helpAttr != null)
					tw.WriteLine("{0} - {1}", commandAttr.Name, helpAttr.Text);
				else
					tw.WriteLine(commandAttr.Name);

				tw.Write("USAGE: {0} {1} ", name, commandAttr.Name);

				foreach (var y in x.GetParameters())
				{
					tw.Write("{0} ", y.Name);
				}

				tw.WriteLine();

				
			}

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

			foreach (var x in type.GetMethods())
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

		static string ParseString(string s)
		{
			return s;
		}

		static bool ParseInvoke(MethodInfo method, List<string> values, Dictionary<Type, MethodInfo> parsers)
		{
			var parameters = method.GetParameters();

			if (parameters.Length == 0)
			{
				method.Invoke(null, new object[0]);
				return true;
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
						Console.Error.WriteLine("Error: Missing parameter: {0}", parameters[i].Name);
						return false;
					}
				}
				catch(Exception ex)
				{
					Console.Error.WriteLine("Error: Parsing parameter: {0}: {1}", parameters[i].Name, ex);
					return false;
				}
			}

			method.Invoke(null, args);

			return true;
		}
		static bool ParseInvokeParams(MethodInfo method, List<string> values, Dictionary<Type, MethodInfo> parsers)
		{
			var parameters = method.GetParameters();

			if (parameters.Length == 0)
			{
				method.Invoke(null, new object[0]);
				return true;
			}

			var last = parameters.Last();

			if (last.GetCustomAttribute<ParamArrayAttribute>() == null)
				return ParseInvoke(method, values, parsers);

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
						Console.Error.WriteLine("Error: Missing parameter: {0}", parameters[i].Name);
						return false;
					}
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine("Error: Parsing parameter: {0}: {1}", parameters[i].Name, ex);
					return false;
				}
			}

			var elem = last.ParameterType.GetElementType();
			var objs = new List<object>();
			for (var i = parameters.Length; i < values.Count; i++)
			{
				try
				{
					objs.Add(parsers[elem].Invoke(null, new[] { values[i] }));
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine("Error: Parsing parameter: {0}: {1}", last.Name, ex);
					return false;
				}
			}

			Array destinationArray = Array.CreateInstance(elem, objs.Count);
			Array.Copy(objs.ToArray(), destinationArray, destinationArray.Length);

			args[args.Length - 1] = destinationArray;

			method.Invoke(null, args);

			return true;
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

			parsers.Add(typeof(string), typeof(Loader).GetMethod("ParseString"));
			parsers.Add(typeof(byte), typeof(byte).GetMethod("Parse"));
			parsers.Add(typeof(sbyte), typeof(sbyte).GetMethod("Parse"));
			parsers.Add(typeof(ushort), typeof(ushort).GetMethod("Parse"));
			parsers.Add(typeof(short), typeof(short).GetMethod("Parse"));
			parsers.Add(typeof(uint), typeof(uint).GetMethod("Parse"));
			parsers.Add(typeof(int), typeof(int).GetMethod("Parse"));
			parsers.Add(typeof(float), typeof(float).GetMethod("Parse"));
			parsers.Add(typeof(double), typeof(double).GetMethod("Parse"));


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
						if (!ParseInvoke(option, optionArgs, parsers))
							return;
					}
					if (optionArgs.Count > optionParams.Length)
					{
						throw new Exception();
					}
				}
				else if (arg == "--")
				{
					commandLock = true;
				}
				else if (arg == "--help")
				{
					Help(type, Console.Out);
					break;
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
						Console.Error.WriteLine("Error: Unrecognized long option: {0}", optionName);
						return;
					}

					if (optionValue != null)
					{
						var parameters = option.GetParameters();
						if (parameters.Length == 0)
						{
							Console.Error.WriteLine("Error: Option does not take parameters: {0}", optionName);
							return;
						}
						else if (parameters.Length == 1)
						{
							optionArgs.Clear();
							optionArgs.Add(optionValue);
							if (!ParseInvoke(option, optionArgs, parsers))
								return;
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
							Console.Error.WriteLine("Error: Unrecognized short option: {0}", optionNames);
							return;
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

							if (!flag)
							{
								Console.Error.WriteLine("Error: Only flag options allowed in a multi-set");
								return;
							}

							opt.Invoke(null, new object[] { true });
						}
					}
				}
				else
				{
					commandArgs.Add(arg);
				}

			}

			MethodInfo realCommand;

			if (command == null || command == "")
			{
				realCommand = type.GetMethods().Where(x => x.GetCustomAttribute<DefaultCommandAttribute>() != null).FirstOrDefault();
				if (realCommand == null)
				{
					Console.Error.WriteLine("Error: No command given");
					return;
				}
			}
			else
			{
				realCommand = commands.GetValueOrDefault(command);
				if (realCommand == null)
				{
					Console.Error.WriteLine("Error: Unrecognized command: {0}", command);
				}
			}

			ParseInvokeParams(realCommand, commandArgs, parsers);
		}
	}
}
