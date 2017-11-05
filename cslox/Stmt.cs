using System.Collections.Generic;

namespace cslox
{
    abstract class Stmt
    {
        public interface IVisitor<T>
        {
            T VisitBlockStmt(Block stmt);
            T VisitClassStmt(Class stmt);
            T VisitExpressionStmt(Expression stmt);
            T VisitFunctionStmt(Function stmt);
            T VisitIfStmt(If stmt);
            T VisitPrintStmt(Print stmt);
            T VisitReturnStmt(Return stmt);
            T VisitVarStmt(Var stmt);
            T VisitWhileStmt(While stmt);
        }

        public class Block : Stmt
        {
            public Block(List<Stmt> Statements)
            {
                this.Statements = Statements;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }

            public readonly List<Stmt> Statements;
        }

        public class Class : Stmt
        {
            public Class(Token Name, List<Stmt.Function> Methods)
            {
                this.Name = Name;
                this.Methods = Methods;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitClassStmt(this);
            }

            public readonly Token Name;
            public readonly List<Stmt.Function> Methods;
        }

        public class Expression : Stmt
        {
            public Expression(Expr expression)
            {
                this.expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }

            public readonly Expr expression;
        }

        public class Function : Stmt
        {
            public Function(Token name, List<Token> parameters, List<Stmt> Body)
            {
                this.name = name;
                this.parameters = parameters;
                this.Body = Body;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }

            public readonly Token name;
            public readonly List<Token> parameters;
            public readonly List<Stmt> Body;
        }

        public class If : Stmt
        {
            public If(Expr Condition, Stmt ThenBranch, Stmt ElseBranch)
            {
                this.Condition = Condition;
                this.ThenBranch = ThenBranch;
                this.ElseBranch = ElseBranch;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitIfStmt(this);
            }

            public readonly Expr Condition;
            public readonly Stmt ThenBranch;
            public readonly Stmt ElseBranch;
        }

        public class Print : Stmt
        {
            public Print(Expr expression)
            {
                this.expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }

            public readonly Expr expression;
        }

        public class Return : Stmt
        {
            public Return(Token Keyword, Expr Value)
            {
                this.Keyword = Keyword;
                this.Value = Value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitReturnStmt(this);
            }

            public readonly Token Keyword;
            public readonly Expr Value;
        }

        public class Var : Stmt
        {
            public Var(Token name, Expr initializer)
            {
                this.name = name;
                this.initializer = initializer;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitVarStmt(this);
            }

            public readonly Token name;
            public readonly Expr initializer;
        }

        public class While : Stmt
        {
            public While(Expr Condition, Stmt Body)
            {
                this.Condition = Condition;
                this.Body = Body;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }

            public readonly Expr Condition;
            public readonly Stmt Body;
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}
