using System;
using System.Collections.Generic;
using System.Text;

namespace Gabbyz.Attribution
{
	/// <summary>
	/// The default command.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class DefaultCommandAttribute : Attribute
	{
	}
}
