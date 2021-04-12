using System;
using System.Collections.Generic;
using System.Text;

namespace Gabbyz.Attribution
{
	/// <summary>
	/// A command line dash option.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public class OptionAttribute : Attribute
	{
		/// <summary>
		/// The long double-dash name.
		/// </summary>
		public string Long { get; set; } = null;

		/// <summary>
		/// The short single-dash name.
		/// </summary>
		public char Short { get; set; } = '\0';

		/// <summary>
		/// The default display value. Uses ToString by default for property targets.
		/// </summary>
		public string Default { get; set; } = null;

		/// <summary>
		/// This is a flag boolean attribute.
		/// </summary>
		public bool Flag { get; set; } = false;
	}
}
