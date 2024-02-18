using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theta.SDK.Utils
{
    /// <summary>
    /// Represents a printable manual.
    /// </summary>
    public interface IManual
    {
        /// <summary>
        /// Get the manual as a human-readable string; Useful for printing, etc.
        /// </summary>
        /// <returns>String representation of the manual</returns>
        string ToString();
    }

    /// <summary>
    /// Provides a mechanism to construct and present a formatted
    /// man-page for a command.
    /// </summary>
    public class Manual : IManual
    {
        /// <summary>
        /// Describes the primary consideration for the manual.
        /// </summary>
        public string Heading;

        /// <summary>
        /// Introduces a reader to the concepts contained within.
        /// </summary>
        public string Summary;

        /// <summary>
        /// Usage information for the resource.
        /// </summary>
        public string Usage;

        /// <summary>
        /// Page sections (optional)
        /// </summary>
        public Section[] Sections = Array.Empty<Section>();

        /// <summary>
        /// Default CTOR.
        /// </summary>
        public Manual(string heading, string summary, string usage)
        {
            Heading = heading;
            Summary = summary;
            Usage = usage;
        }

        /// <summary>
        /// Construct manual for a Command.
        /// </summary>
        public Manual(Command command)
        {
            Heading = $"Command: {command.Name}";
            Summary = command.Summary;
            Usage = command.Usage;
        }

        //---
        /// <summary>
        /// Get the manual as a human-readable string; Useful for printing, etc.
        /// </summary>
        /// <returns>String representation of the manual</returns>
        override public string ToString()
        {
            StringBuilder page = new StringBuilder()
                .AppendLine($"{Heading}:\n\t{Summary}")
                .AppendLine($"\nUSAGE:\n\t{Usage}");

            foreach (Section section in Sections)
            {
                page.AppendLine($"\n{section}");
            }

            return page.ToString();
        }

        //---
        /// <summary>
        /// Represents a structured man-page section.
        /// </summary>
        public class Section : Manual
        {
            public Section(string heading, string summary, string usage) : base(heading, summary, usage)
            { }

            public Section(Command command) : base(command)
            { }
        }
    }
}
