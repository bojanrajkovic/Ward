using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ward.Dns
{
    /// <summary>
    /// A DNS question.
    /// </summary>
    public class Question
    {
        /// <summary>The name being queried.</summary>
        /// <returns>The name being queried.</returns>
        public string Name { get; }

        /// <summary>The record type being queried.</summary>
        /// <returns>The record type being queried.</returns>
        public Type Type { get; }

        /// <summary>The record class being queried.</summary>
        /// <returns>The record class being queried.</returns>
        public Class Class { get; }

        /// <summary>
        /// Creates a <see cref="Question"/>.
        /// </summary>
        /// <param name="name">The name being queried.</param>
        /// <param name="type">The record type being queried.</param>
        /// <param name="class">The record class being queried.</param>
        public Question(string name, Type type, Class @class) {
            Name = name;
            Type = type;
            Class = @class;
        }
    }
}
