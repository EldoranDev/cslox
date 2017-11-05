using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class LoxClass : ILoxCallable
    {
        public readonly string Name;

        public LoxClass(string name)
        {
            Name = name;
        }

        public int Arity => 0;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new LoxInstance(this);
            return instance;
        }

        public override string ToString()
        {
            return $"<cls {Name}>";
        }
    }
}
