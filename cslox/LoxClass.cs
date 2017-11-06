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

        private readonly Dictionary<string, LoxFunction> _methods;

        public LoxClass(string name, Dictionary<string, LoxFunction> methods)
        {
            Name = name;
            _methods = methods;
        }

        public int Arity{
            get
            {
                var initializer = _methods.ContainsKey("init") ? _methods["init"] : null;

                return initializer?.Arity ?? 0;
            }
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new LoxInstance(this);

            var initializer = _methods.ContainsKey("init") ? _methods["init"] : null;

            initializer?.Bind(instance).Call(interpreter, arguments);

            return instance;
        }

        internal LoxFunction FindMethod(LoxInstance instance, string name)
        {
            if (_methods.ContainsKey(name))
            {
                return _methods[name].Bind(instance);
            }

            return null;
        }

        public override string ToString()
        {
            return $"<cls {Name}>";
        }
    }
}
