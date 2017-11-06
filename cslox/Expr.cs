using System.Collections.Generic;

namespace cslox
{
    abstract class Expr
    {
        public interface IVisitor<T>
        {
            T VisitAssignExpr(Assign expr);
            T VisitBinaryExpr(Binary expr);
            T VisitCallExpr(Call expr);
            T VisitGetExpr(Get expr);
            T VisitGroupingExpr(Grouping expr);
            T VisitLiteralExpr(Literal expr);
            T VisitLogicalExpr(Logical expr);
            T VisitSetExpr(Set expr);
            T VisitThisExpr(This expr);
            T VisitUnaryExpr(Unary expr);
            T VisitVariableExpr(Variable expr);
        }

        public class Assign : Expr
        {
            public Assign(Token name, Expr Value)
            {
                this.name = name;
                this.Value = Value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }

            public readonly Token name;
            public readonly Expr Value;
        }

        public class Binary : Expr
        {
            public Binary(Expr Left, Token Op, Expr Right)
            {
                this.Left = Left;
                this.Op = Op;
                this.Right = Right;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

            public readonly Expr Left;
            public readonly Token Op;
            public readonly Expr Right;
        }

        public class Call : Expr
        {
            public Call(Expr Callee, Token Paren, List<Expr> Arguments)
            {
                this.Callee = Callee;
                this.Paren = Paren;
                this.Arguments = Arguments;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitCallExpr(this);
            }

            public readonly Expr Callee;
            public readonly Token Paren;
            public readonly List<Expr> Arguments;
        }

        public class Get : Expr
        {
            public Get(Expr obj, Token name)
            {
                this.obj = obj;
                this.name = name;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitGetExpr(this);
            }

            public readonly Expr obj;
            public readonly Token name;
        }

        public class Grouping : Expr
        {
            public Grouping(Expr Expression)
            {
                this.Expression = Expression;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

            public readonly Expr Expression;
        }

        public class Literal : Expr
        {
            public Literal(object Value)
            {
                this.Value = Value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public readonly object Value;
        }

        public class Logical : Expr
        {
            public Logical(Expr Left, Token Op, Expr Right)
            {
                this.Left = Left;
                this.Op = Op;
                this.Right = Right;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }

            public readonly Expr Left;
            public readonly Token Op;
            public readonly Expr Right;
        }

        public class Set : Expr
        {
            public Set(Expr Obj, Token Name, Expr Value)
            {
                this.Obj = Obj;
                this.Name = Name;
                this.Value = Value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitSetExpr(this);
            }

            public readonly Expr Obj;
            public readonly Token Name;
            public readonly Expr Value;
        }

        public class This : Expr
        {
            public This(Token Keyword)
            {
                this.Keyword = Keyword;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitThisExpr(this);
            }

            public readonly Token Keyword;
        }

        public class Unary : Expr
        {
            public Unary(Token Op, Expr Right)
            {
                this.Op = Op;
                this.Right = Right;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

            public readonly Token Op;
            public readonly Expr Right;
        }

        public class Variable : Expr
        {
            public Variable(Token Name)
            {
                this.Name = Name;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }

            public readonly Token Name;
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}
