using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox.natives
{
    class ReadLine : ILoxCallable
    {
        public int Arity => 0;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return Console.ReadLine();
        }
    }
}
