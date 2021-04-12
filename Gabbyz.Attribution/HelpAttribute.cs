using System;
using System.Collections.Generic;
using System.Text;

namespace Gabbyz.Attribution
{
	/// <summary>
	/// Help text for an item.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property)]
	public class HelpAttribute : Attribute
	{
		/// <summary>
		/// The help text.
		/// </summary>
		public string Text { get; set; } = "";
		/// <summary>
		/// Assigns help text to this item.
		/// </summary>
		/// <param name="Text">The help text.</param>
		public HelpAttribute(string Text)
		{
			this.Text = Text;
		}
	}
}
