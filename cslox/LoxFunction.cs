using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class LoxFunction : ILoxCallable
    {
        private readonly Stmt.Function _declaration;
        private readonly Environment _closure;
        private readonly bool _isInitializer;

        public int Arity => _declaration.parameters.Count;

        public LoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
        {
            _declaration = declaration;
            _closure = closure;
            _isInitializer = isInitializer;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            var environment = new Environment(_closure);

            environment.Define("this", instance);
            return new LoxFunction(_declaration, environment, _isInitializer);
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new Environment(_closure);
            
            for(var i = 0; i < _declaration.parameters.Count; i++)
            {
                environment.Define(_declaration.parameters[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.Value;
            }

            if (_isInitializer)
            {
                return _closure.GetAt(0, "this");
            }

            return null;
        }

        public override string ToString()
        {
            return $"<fn {_declaration.name.Lexeme}>";
        }
    }
}
