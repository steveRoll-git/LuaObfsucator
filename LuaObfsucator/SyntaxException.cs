using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaObfsucator
{
    class SyntaxException : Exception
    {
        public SyntaxException(string sourceName, int line, string message)
            : base($"{sourceName}:{line}: {message}")
        {
        }
    }
}
