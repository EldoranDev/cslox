using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        Interpreter _interpreter;
        Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();

        private FunctionType currentFunction = FunctionType.NONE;

        private ClassType currentClass = ClassType.NONE;

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Right);
            Resolve(expr.Left);

            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();

            return null;
        }

        public object VisitClassStmt (Stmt.Class stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            var enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            BeginScope();
            scopes.Peek().Add("this", true);

            stmt.Methods.ForEach(m =>
            {
                var declaration = FunctionType.METHOD;

                if(m.name.Lexeme == "init")
                {
                    declaration = FunctionType.INITIALIZER;
                }
                ResolveFunction(m, declaration);
            });

            currentClass = enclosingClass;

            EndScope();
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.Callee);

            expr.Arguments.ForEach(e => Resolve(e));

            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {

            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);

            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expression);return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);

            if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);

            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            if(stmt.expression != null)
            {
                Resolve(stmt.expression);
            }

            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            if(currentFunction == FunctionType.NONE)
            {
                Lox.Error(stmt.Keyword, "Cannot return from top-level code.");
            }
            else if (currentFunction == FunctionType.INITIALIZER)
            {
                Lox.Error(stmt.Keyword, "cannot return a value from an initializer.");
            }
            if(stmt.Value != null)
            {
                Resolve(stmt.Value);
            }

            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            if(scopes.Any() && scopes.Peek()[expr.Name.Lexeme] == false)
            {
                Lox.Error(expr.Name, "Cannot read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if(stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);

            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);

            return null;
        }

        void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void Declare(Token name)
        {
            if (scopes.Count == 0) return;

            var scope = scopes.Peek();
            if(scope.ContainsKey(name.Lexeme))
            {
                Lox.Error(name, "Variable with this name already declared in this scope.");
            }
            scope.Add(name.Lexeme, false);
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0) return;

            scopes.Peek()[name.Lexeme] = true;
        }

        void EndScope()
        {
            scopes.Pop();
        }

        internal void Resolve(List<Stmt> statements)
        {
            statements.ForEach((e) => Resolve(e));
        }

        void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            var enclosingFunction = currentFunction;
            currentFunction = type;
            BeginScope();
            function.parameters.ForEach(e =>
            {
                Declare(e);
                Define(e);
            });

            Resolve(function.Body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        void ResolveLocal(Expr expr, Token name)
        {
            for (var i = scopes.Count - 1; i >= 0; i--)
            {
                if(scopes.Reverse().ElementAt(i).ContainsKey(name.Lexeme))
                {
                    _interpreter.Resolve(expr, scopes.Count - 1 - i);
                    return;
                }
            }
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.obj);
            return null;
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Obj);

            return null;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            if(currentClass == ClassType.NONE)
            {
                Lox.Error(expr.Keyword, "Cannot use 'this' outside of a class");
                return null;
            }

            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        enum FunctionType
        {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
        }

        private enum ClassType
        {
            NONE,
            CLASS
        }
    }
}
