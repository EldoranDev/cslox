using System;
using System.Collections.Generic;

using static cslox.TokenType;

namespace cslox
{
    class Parser
    {
        private readonly List<Token> _tokens;
        private int current = 0;

        public Parser(IEnumerable<Token> tokens)
        {
            _tokens = new List<Token>(tokens);
        }

        public List<Stmt> Parse()
        {
            var statments = new List<Stmt>();

            while (!IsAtEnd())
            {
                statments.Add(Declaration());
            }

            return statments;
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(FUN)) return Function("function");
                if (Match(VAR)) return VarDeclaration();

                return Statement();
            } catch(ParseError error)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt Function(string kind)
        {
            var name = Consume(IDENTIFIER, $"Expect {kind} name.");

            Consume(LEFT_PAREN, $"Expect '(' after {kind} name.");

            var parameters = new List<Token>();

            if(!Check(RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 8)
                    {
                        Error(Peek(), "Cannot have more than 8 Parameters.");
                    }

                    parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
                } while (Match(COMMA));
            }
            Consume(RIGHT_PAREN, $"Expect ')' after {kind} parameters.");

            Consume(LEFT_BRACE, $"Expect '{{' before {kind} body.");
            var body = Block();
            return new Stmt.Function(name, parameters, body);
        }

        private Stmt VarDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(EQUAL))
            {
                initializer = Expression();
            }

            Consume(SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt Statement()
        {
            if (Match(PRINT)) return PrintStatement();
            if (Match(RETURN)) return ReturnStatement();
            if (Match(WHILE)) return WhileStatement();
            if (Match(LEFT_BRACE)) return new Stmt.Block(Block());
            if (Match(IF)) return IfStatement();
            if (Match(FOR)) return ForStatement();

            return ExpressionStatement();
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while(!Check(RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(RIGHT_BRACE, "Expect '}' after block.");

            return statements;
        }

        private Stmt IfStatement()
        {
            Consume(LEFT_PAREN, "Exepct '(' after 'if'.");
            var condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;

            if (Match(ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt ForStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'while'");

            Stmt initializer;
            if (Match(SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!Check(SEMICOLON))
            {
                condition = Expression();
                Consume(SEMICOLON, "Expect ';' after loop condition.");
            }

            Expr increment = null;
            if (!Check(RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(RIGHT_PAREN, "Expect ')' after for clauses.");

            var body = Statement();

            if (increment != null)
            {
                body = new Stmt.Block(new List<Stmt>
                {
                    body,
                    new Stmt.Expression(increment)
                });
            }

            if (condition == null) condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);

            if(initializer != null)
            {
                body = new Stmt.Block(new List<Stmt>
                {
                    initializer, body
                });
            }

            return body;
        }
        
        private Stmt ReturnStatement()
        {
            var keyword = Previous();
            Expr value = null;
            if(!Check(SEMICOLON))
            {
                value = Expression();
            }

            Consume(SEMICOLON, "Expect ';' after return value.");

            return new Stmt.Return(keyword, value);
        }

        private Stmt WhileStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'while'.");
            var condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after 'while' condition.");

            var body = Statement();

            return new Stmt.While(condition, body);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Expr Expression()
        {
            return Assignement();
        }

        private Expr Assignement()
        {
            var expr = Or();

            if(Match(EQUAL))
            {
                var equals = Previous();

                var value = Assignement();

                if(expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).Name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Or()
        {
            var expr = And();

            while(Match(OR))
            {
                var op = Previous();
                var right = And();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            var expr = Equality();

            while(Match(AND))
            {
                var op = Previous();
                var right = Equality();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr Equality()
        {
            var expr = Comparison();

            while (Match(BANG_EQUAL, EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Addition();

            while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = Addition();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Addition()
        {
            Expr expr = Multiplication();

            while (Match(MINUS, PLUS))
            {
                Token op = Previous();
                Expr right = Multiplication();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();

            if(!Check(RIGHT_PAREN))
            {
                do
                {
                    if(arguments.Count >= 8)
                    {
                        Error(Peek(), "Cannot have more than 8 arguments.");
                    }
                    arguments.Add(Expression());
                } while (Match(COMMA));
            }

            var paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
        }

        private Expr Multiplication()
        {
            Expr expr = Unary();
            
            while(Match(SLASH, STAR))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if(Match(BANG, MINUS))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }

            return Call();
        }

        private Expr Primary()
        {
            if (Match(FALSE)) return new Expr.Literal(false);
            if (Match(TRUE)) return new Expr.Literal(true);
            if (Match(NIL)) return new Expr.Literal(null);

            if(Match(NUMBER, STRING))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if(Match(IDENTIFIER))
            {
                return new Expr.Variable(Previous());
            }

            if(Match(LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(RIGHT_PAREN, "Expect ')' after expression");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private void Synchronize()
        {
            Advance();

            while(!IsAtEnd())
            {
                if (Previous().Type == SEMICOLON) return;

                switch(Peek().Type)
                {
                    case CLASS:
                    case FUN:
                    case VAR:
                    case FOR:
                    case IF:
                    case WHILE:
                    case PRINT:
                    case RETURN:
                        return;
                }

                Advance();
            }
        }

        private Token Consume(TokenType type, string Message)
        {
            if (Check(type)) return Advance();
            throw Error(Peek(), Message);
        }

        private ParseError Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParseError();
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if(Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }


        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        /// <summary>
        /// Check current token for given type
        /// </summary>
        /// <param name="type">type to check for</param>
        /// <returns>true if matches</returns>
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        /// <summary>
        /// Check if we are at the end of the File
        /// </summary>
        /// <returns></returns>
        private bool IsAtEnd()
        {
            return Peek().Type == EOF;
        }

        /// <summary>
        /// Get current Token without consuming it
        /// </summary>
        /// <returns></returns>
        private Token Peek()
        {
            return _tokens[current];
        }

        private Token Previous()
        {
            return _tokens[current - 1];
        }

        class ParseError : Exception
        {
        }
    }
}
