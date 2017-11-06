﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Environment
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        private readonly Environment enclosing;

        internal void Assign(Token name, object value)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        internal void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance).values[name.Lexeme] = value;
        }

        internal void Define(string name, object value)
        {
            values[name] = value;
        }

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        internal object Get(Token name)
        {
            if(values.ContainsKey(name.Lexeme))
            {
                return values[name.Lexeme];
            }

            if (enclosing != null) return enclosing.Get(name);

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        internal object GetAt(int dist, string name)
        {
            var vals = Ancestor(dist).values;
        
            return vals.ContainsKey(name) ? vals[name] : null;
        }

        internal Environment Ancestor(int dist)
        {
            var environment = this;
            for(var i = 0; i < dist; i++)
            {
                environment = environment.enclosing;
            }

            return environment;
        }


    }
}
