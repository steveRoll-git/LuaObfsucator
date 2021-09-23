using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaObfsucator
{
    class LexerException : Exception
    {
        public LexerException(string sourceName, int line, string message)
            : base($"{sourceName}:{line}: {message}")
        {
        }
    }
}
