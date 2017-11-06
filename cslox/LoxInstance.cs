using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class LoxInstance
    {
        private LoxClass klass;

        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            this.klass = klass;
            fields = new Dictionary<string, object>();
        }

        public override string ToString()
        {
            return $"<instance {klass.Name}>";
        }

        public object Get(Token name)
        {
            if (fields.ContainsKey(name.Lexeme))
            {
                return fields[name.Lexeme];
            }

            var method = klass.FindMethod(this, name.Lexeme);
            if (method != null) return method;

            throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, Object value)
        {
            fields[name.Lexeme] = value;
        }
    }
}
