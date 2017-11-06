using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class AstPrinter : Expr.IVisitor<String>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            return parenthesize(expr.name.Lexeme, expr.Value);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return parenthesize("group", expr.Expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null) return "nil";
            return expr.Value.ToString();
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return parenthesize(expr.Op.Lexeme, expr.Right);
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            return parenthesize("VAR", expr);
        }

        private string parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);

            foreach(var expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }

            builder.Append(")");

            return builder.ToString();
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            throw new NotImplementedException();
        }

        public string VisitCallExpr(Expr.Call expr)
        {
            throw new NotImplementedException();
        }

        public string VisitGetExpr(Expr.Get expr)
        {
            throw new NotImplementedException();
        }

        public string VisitSetExpr(Expr.Set expr)
        {
            throw new NotImplementedException();
        }

        public string VisitThisExpr(Expr.This expr)
        {
            throw new NotImplementedException();
        }
    }
}
