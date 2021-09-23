using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaObfsucator
{
    enum TokenType
    {
        Keyword,
        Identifier,
        Number,
        String,
        Punctuation,
        Whitespace,
        Comment,
        EOF
    }

    struct Token
    {
        public TokenType type;
        public string content;

        public static readonly Token EOF = new Token(TokenType.EOF, "");

        public Token(TokenType type, string content)
        {
            this.type = type;
            this.content = content;
        }

        public override string ToString()
        {
            return $"[{type}] '{content}'";
        }
    }
}
