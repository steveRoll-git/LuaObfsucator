using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaObfsucator
{
    /// <summary>
    /// A lexer made specifically for Lua code
    /// </summary>
    class Lexer
    {

        private static readonly HashSet<string> keywords = new HashSet<string>
        {
            "and", "break", "do", "else", "elseif", "end", "false",
            "for", "function", "if", "in", "local", "nil", "not",
            "or", "repeat", "return", "then", "true", "until", "while"
        };

        private static readonly HashSet<string> groupPunctuation = new HashSet<string>
        {
            "==", "~=", "<=", ">=", "..", "..."
        };

        /// <summary>
        /// The name of the source file, used in error messages.
        /// </summary>
        private string sourceName;

        /// <summary>
        /// The index of the last character where the lexer stopped.
        /// </summary>
        private int currentIndex = 0;

        /// <summary>
        /// Reader for the source code
        /// </summary>
        private string source;

        /// <summary>
        /// The current line in the source code, used in error messages.
        /// </summary>
        private int currentLine = 1;

        private char currentChar => source[currentIndex];

        /// <summary>
        /// Whether the lexer has reached the end of the file.
        /// </summary>
        public bool reachedEnd
        {
            get;
            private set;
        } = false;

        public Lexer(string source, string sourceName = "code")
        {
            this.source = source;
            this.sourceName = sourceName;
        }


        /// <summary>
        /// Returns whether the character <paramref name="c"/> is a
        /// character that can be part of an identifier - an underscore,
        /// letter or digit.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsNameChar(char c)
        {
            return c == '_' || char.IsLetterOrDigit(c);
        }

        /// <summary>
        /// Increments currentIndex, and updates the currentLine and reachedEnd properties if needed.
        /// </summary>
        private void Advance()
        {
            if (reachedEnd)
            {
                return;
            }

            currentIndex++;

            if (currentIndex == source.Length)
            {
                reachedEnd = true;
                return;
            }
            else
            {
                if (currentChar == '\n')
                {
                    currentLine++;
                }
            }
        }

        /// <summary>
        /// Advances like the Advance method, but up to a specified index.
        /// </summary>
        /// <param name="index"></param>
        private string ConsumeUntilIndex(int index)
        {
            int start = currentIndex;
            while (currentIndex < index)
            {
                Advance();
            }
            return source[start..index];
        }

        /// <summary>
        /// Returns a substring of the source until the first character that fulfills <paramref name="condition"/>,
        /// and advances currentIndex to the end of that substring.
        /// </summary>
        /// <param name="condition">A function to check the current character. The method stops when this function returns true.</param>
        /// <param name="includeLast">Whether to include the last character (which fulfilled <paramref name="condition"/>)</param>
        /// <returns></returns>
        private string ConsumeUntilCondition(Func<char, bool> condition, bool includeLast = false)
        {
            int start = currentIndex;

            while (!reachedEnd && !condition(source[currentIndex]))
            {
                Advance();
            }

            if (includeLast)
            {
                Advance();
            }

            return source[start..currentIndex];
        }

        public Token NextToken()
        {
            if (reachedEnd)
            {
                return Token.EOF;
            }

            if (char.IsWhiteSpace(currentChar))
            {
                // whitespace

                string content = ConsumeUntilCondition(c => !char.IsWhiteSpace(c));

                return new Token(TokenType.Whitespace, content);
            }
            else if (currentChar == '_' || char.IsLetter(currentChar))
            {
                // identifier or keyword

                string content = ConsumeUntilCondition(c => !IsNameChar(c));

                return new Token(keywords.Contains(content) ? TokenType.Keyword : TokenType.Identifier, content);
            }
            else if (char.IsDigit(currentChar))
            {
                // number

                string content = ConsumeUntilCondition(c => !(char.IsDigit(c) || (c is '.' or 'x' or (>= 'a' and <= 'f') or (>= 'A' and <= 'F'))));

                return new Token(TokenType.Number, content);
            }
            else if (currentChar is '"' or '\'')
            {
                char delimiter = currentChar;
                int start = currentIndex;

                Advance();

                while (currentChar != delimiter)
                {
                    Advance();
                    if (currentIndex < source.Length && currentChar == '\\')
                    {
                        Advance();
                        Advance();
                    }
                    if (reachedEnd)
                    {
                        throw new LexerException(sourceName, currentLine, "unfinished string");
                    }
                }
                Advance();

                string content = source[start..currentIndex];

                return new Token(TokenType.String, content);
            }
            else if (currentIndex < source.Length - 1 && source.Substring(currentIndex, 2) == "[[")
            {
                // multiline string

                int end = source.IndexOf("]]", currentIndex);
                if (end == -1)
                {
                    throw new LexerException(sourceName, currentLine, "unfinished multiline string");
                }
                end += 2;

                string content = ConsumeUntilIndex(end);

                return new Token(TokenType.String, content);
            }
            else if (currentIndex < source.Length - 3 && source.Substring(currentIndex, 4) == "--[[")
            {
                // multiline comment

                int end = source.IndexOf("]]", currentIndex);
                if (end == -1)
                {
                    throw new LexerException(sourceName, currentLine, "unfinished multiline comment");
                }
                end += 2;

                string content = ConsumeUntilIndex(end);

                return new Token(TokenType.Comment, content);
            }
            else if (currentIndex < source.Length - 1 && source.Substring(currentIndex, 2) == "--")
            {
                // single line comment

                string content = ConsumeUntilCondition(c => c is '\n' or '\r');

                return new Token(TokenType.Comment, content);
            }
            else
            {
                // punctuation
                int end = currentIndex + 1;
                while (end < source.Length && groupPunctuation.Contains(source[currentIndex..(end + 1)]))
                {
                    end++;
                }

                string content = ConsumeUntilIndex(end);

                return new Token(TokenType.Punctuation, content);
            }
        }
    }
}
