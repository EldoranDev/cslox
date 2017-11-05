using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static cslox.TokenType;

namespace cslox
{
    class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        internal readonly Environment globals = new Environment();
        private Environment environment;

        private Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

        public Interpreter()
        {
            environment = globals;

            globals.Define("clock", new natives.Clock());
            globals.Define("readLine", new natives.ReadLine());
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var stmt in statements)
                {
                    Execute(stmt);
                }
            } catch(RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(environment));
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new LoxFunction(stmt, environment);

            environment.Define(stmt.name.Lexeme, function);
            return null;
        }
        
        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if(stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            environment.Define(stmt.name.Lexeme, value);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if(stmt.Value != null)
            {
                value = Evaluate(stmt.Value);
            }

            throw new Return(value);
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while(IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }

            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.Value);

            if (locals.ContainsKey(expr))
            {
                environment.AssignAt(locals[expr], expr.name, value);
            }
            else
            {
                environment.Assign(expr.name, value);
            }
            return value;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.Left);
            object right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case BANG_EQUAL: return !IsEqual(left, right);
                case EQUAL_EQUAL: return IsEqual(left, right);
                case GREATER:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left >= (double)right;
                case LESS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left < (double)right;
                case LESS_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left <= (double)right;
                case MINUS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left - (double)right;
                case SLASH:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left / (double)right;
                case STAR:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left * (double)right;
                case PLUS:
                    if(left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }

                    if(left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.Op, "Operands must be two numbers or two strings.");
            }

            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            environment.Define(stmt.Name.Lexeme, null);
            var cls = new LoxClass(stmt.Name.Lexeme);
            environment.Assign(stmt.Name, cls);
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            var callee = Evaluate(expr.Callee);

            var arguments = new List<object>();

            expr.Arguments.ForEach((e) =>
            {
                arguments.Add(Evaluate(e));
            });

            if(!(callee is ILoxCallable))
            {
                throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
            }

            var function = callee as ILoxCallable;

            if (arguments.Count != function.Arity)
            {
                throw new RuntimeError(expr.Paren, $"Expected {function.Arity} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.Left);
            if(expr.Op.Type == OR)
            {
                if (IsTruthy(left)) return left;
            } else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.Right);
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch(expr.Op.Type)
            {
                case BANG:
                    return !IsTruthy(right);
                case MINUS:
                    CheckNumberOperand(expr.Op, right);
                    return -(double)right;
            }

            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.Name, expr);
        }

        /// <summary>
        /// Check if the given obj is Truthy
        /// 
        /// Everything that is not false or null is true
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;

            return true;
        }

        /// <summary>
        /// Check if two elements are equal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(op, "Operands must be numbers.");
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "nil";

            return obj.ToString();
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        internal void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            var previous = this.environment;

            try
            {
                this.environment = environment;

                statements.ForEach((e) =>
                {
                    Execute(e);
                });
            }
            finally
            {
                this.environment = previous;
            }
        }

        object LookUpVariable(Token name, Expr expr)
        {
            if(locals.ContainsKey(expr))
            {
                return environment.GetAt(locals[expr], name.Lexeme);
            } else
            {
                return globals.Get(name);
            }
        }

        internal void Resolve(Expr expr, int depth)
        {
            locals.Add(expr, depth);
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            var obj = Evaluate(expr.obj);

            if(obj is LoxInstance)
            {
                return ((LoxInstance)obj).Get(expr.name);
            }

            throw new RuntimeError(expr.name, "Only instances have properties.");
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            var obj = Evaluate(expr.Obj);

            if(!(obj is LoxInstance))
            {
                throw new RuntimeError(expr.Name, "Only instances have fields.");
            }

            var value = Evaluate(expr.Value);
            (obj as LoxInstance).Set(expr.Name, value);
            return value;
        }
    }
}
