namespace cslox {
	abstract class Expr {
		public interface IVisitor<T> {
			 T VisitBinaryExpr(Binary expr);
			 T VisitGroupingExpr(Grouping expr);
			 T VisitLiteralExpr(Literal expr);
			 T VisitUnaryExpr(Unary expr);
		}

		 public class Binary : Expr {
			public Binary( Expr Left, Token Op, Expr Right ) {
				this.Left = Left;
				this.Op = Op;
				this.Right = Right;
			}

			public override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitBinaryExpr(this);
			}

			 public readonly Expr Left;
			 public readonly Token Op;
			 public readonly Expr Right;
		}

		 public class Grouping : Expr {
			public Grouping( Expr Expression ) {
				this.Expression = Expression;
			}

			public override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitGroupingExpr(this);
			}

			 public readonly Expr Expression;
		}

		 public class Literal : Expr {
			public Literal( object Value ) {
				this.Value = Value;
			}

			public override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitLiteralExpr(this);
			}

			 public readonly object Value;
		}

		 public class Unary : Expr {
			public Unary( Token Op, Expr Right ) {
				this.Op = Op;
				this.Right = Right;
			}

			public override T Accept<T>(IVisitor<T> visitor) {
				return visitor.VisitUnaryExpr(this);
			}

			 public readonly Token Op;
			 public readonly Expr Right;
		}

		 public abstract T Accept<T>(IVisitor<T> visitor);
	}
}
