using System;
using System.Collections.Generic;
using System.Text;

namespace Gabbyz.Attribution
{
    /// <summary>
    /// A command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// The name of the command.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Create a command from this method.
        /// </summary>
        /// <param name="Name">The name of the command.</param>
        public CommandAttribute(string Name)
		{
            this.Name = Name;
		}
    }
}
