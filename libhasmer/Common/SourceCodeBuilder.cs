using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hasmer {
    /// <summary>
    /// Represents source code. Used for maintaining indentation and other source code syntax.
    /// </summary>
    public class SourceCodeBuilder {
        /// <summary>
        /// The StringBuilder used internally.
        /// </summary>
        public StringBuilder Builder { get; }

        /// <summary>
        /// The amount of indentation currently.
        /// </summary>
        private int IndentationLevel = 0;

        /// <summary>
        /// The character used for indentation.
        /// </summary>
        private string IndentationCharacter;

        /// <summary>
        /// Creates a new SourceCodeBuilder object given the indentation character.
        /// </summary>
        /// <param name="indentationCharacter">The character (actually a string, not a single character) to use for indentation. This could be a tab or four spaces for example.</param>
        public SourceCodeBuilder(string indentationCharacter) {
            IndentationCharacter = indentationCharacter;
            Builder = new StringBuilder();
        }

        /// <summary>
        /// Appends a new line and puts the proper indentation on that line.
        /// </summary>
        public void NewLine() {
            Builder.AppendLine();
            if (IndentationLevel > 0) {
                Builder.Append(string.Concat(Enumerable.Repeat(IndentationCharacter, IndentationLevel)));
            }
        }

        /// <summary>
        /// Writes the given string to the stream. Does not append a new line.
        /// </summary>
        public void Write(string code) {
            Builder.Append(code);
        }

        /// <summary>
        /// Modifies the indentation amount by the given level.
        /// </summary>
        public void AddIndent(int amount) {
            IndentationLevel += amount;
        }

        /// <summary>
        /// Deletes 1 level of indentation from the current line.
        /// </summary>
        public void RemoveLastIndent() {
            Builder.Remove(Builder.Length - IndentationCharacter.Length, IndentationCharacter.Length);
        }

        public override string ToString() {
            return Builder.ToString();
        }
    }
}
