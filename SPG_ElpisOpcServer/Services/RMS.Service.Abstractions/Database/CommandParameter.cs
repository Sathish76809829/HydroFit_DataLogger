using System;
using System.Data;

namespace RMS.Service.Abstractions.Database
{
    /// <summary>
    /// Command parameters for Database
    /// </summary>
    public struct CommandParameter
    {
        public string Name { get; set; }

        public DbType? Type { get; set; }

        public object Value { get; set; }

        public ParameterDirection? Direction { get; set; }

        /// <summary>
        /// Parameter having input
        /// </summary>
        /// <param name="name">Name of the input parameter</param>
        /// <param name="type">Type of parameter</param>
        /// <param name="value">Value of the input parameter</param>
        /// <returns>CommandParameter instance for input</returns>
        public static CommandParameter WithInput(string name, DbType type, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return new CommandParameter
            {
                Direction = ParameterDirection.Input,
                Name = name,
                Type = type,
                Value = value
            };
        }

        /// <summary>
        /// Paramter with output
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        /// <returns>CommandParameter instance</returns>
        public static CommandParameter With(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return new CommandParameter
            {
                Name = name,
                Value = value
            };
        }
    }
}
