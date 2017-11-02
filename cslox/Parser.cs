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

        public Expr Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseError error)
            {
                return null;
            }
        }

        private Expr Expression()
        {
            return Equality();
        }

        private Expr Equality()
        {
            var expr = Comparison();

            while (match(BANG_EQUAL, EQUAL_EQUAL))
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

            while (match(GREATER, GREATER_EQUAL, LESS, LESS, EQUAL))
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

            while (match(MINUS, PLUS))
            {
                Token op = Previous();
                Expr right = Multiplication();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Multiplication()
        {
            Expr expr = Unary();
            
            while(match(SLASH, STAR))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if(match(BANG, MINUS))
            {
                Token op = Previous();
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            if (match(FALSE)) return new Expr.Literal(false);
            if (match(TRUE)) return new Expr.Literal(true);
            if (match(NIL)) return new Expr.Literal(null);

            if(match(NUMBER, STRING))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if(match(LEFT_PAREN))
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

        private bool match(params TokenType[] types)
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
