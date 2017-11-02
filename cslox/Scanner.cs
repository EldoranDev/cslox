using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static cslox.TokenType;

namespace cslox
{
    class Scanner
    {
        /// <summary>
        /// Source code to analyze
        /// </summary>
        string _source;

        /// <summary>
        /// List of found tokens
        /// </summary>
        List<Token> _tokens = new List<Token>();

        private int _start = 0;
        private int _current = 0;
        private int _line = 0;

        private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
            {
                { "and", AND },
                { "class", CLASS },
                { "else", ELSE },
                { "false", FALSE },
                { "for", FOR },
                { "fun", FUN },
                { "if", IF },
                { "nil", NIL },
                { "or", OR },
                { "print", PRINT },
                { "return", RETURN },
                { "super", SUPER },
                { "this", THIS },
                { "true", TRUE },
                { "var", VAR },
                { "while", WHILE }
            };

        /// <summary>
        /// csLox Scanner/Lexer
        /// </summary>
        /// <param name="source">Source Code to scan</param>
        public Scanner(string source)
        {
            _source = source;
        }

        /// <summary>
        /// Get the tokens based on the Scanners Source
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Token> GetTokens()
        {
            while(!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(EOF, "", null, _line));
            return _tokens;
        }

        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case '(': AddToken(LEFT_PAREN); break;
                case ')': AddToken(RIGHT_PAREN); break;
                case '{': AddToken(LEFT_BRACE); break;
                case '}': AddToken(RIGHT_BRACE); break;
                case ',': AddToken(COMMA); break;
                case '.': AddToken(DOT); break;
                case '-': AddToken(MINUS); break;
                case '+': AddToken(PLUS); break;
                case ';': AddToken(SEMICOLON); break;
                case '*': AddToken(STAR); break;
                case '!': AddToken(Match('=') ? BANG_EQUAL : BANG); break;
                case '=': AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
                case '<': AddToken(Match('=') ? LESS_EQUAL : LESS); break;
                case '>': AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;
                case '/':
                    if (Match('/')) //We are inside of a comment discard the whole line
                    {
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(SLASH);
                    }
                    break;
                case '"': ScanString(); break;

                case '\n':
                    _line++;
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore Whitespaces
                    break;
                default:
                    if (IsDigit(c))
                    {
                        ScanNumber();
                    }
                    else if(IsAlpha(c))
                    {
                        ScanIdentifier();
                    }
                    else
                    {
                        Program.error(_line, "Unexpected character.");
                    }

                    break;
            }
        }

        /// <summary>
        /// Advance the scanner by a single character.
        /// </summary>
        /// <returns>the Character the scanner is not pointing at.</returns>
        private char Advance()
        {
            return _source[_current++];
        }

        /// <summary>
        /// Returns the current character without consuming it.
        /// </summary>
        /// <returns>Current character</returns>
        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[_current];
        }

        /// <summary>
        /// Returns the next character without consuming it
        /// </summary>
        /// <returns>Next character</returns>
        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';

            return _source[_current + 1];
        }

        /// <summary>
        /// Checks if the current character matches the expected.
        /// </summary>
        /// <param name="expected">Expected char</param>
        /// <returns>true if they match</returns>
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;

            if (_source[_current] != expected) return false;

            _current++;

            return true;
        }

        /// <summary>
        /// Scans a string literal
        /// </summary>
        private void ScanString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if(IsAtEnd())
            {
                Program.error(_line, "Unterminated string.");
            }
            Advance();

            var value = _source.Substring(_start + 1, (_current - _start) - 2);
            AddToken(STRING, value);
        }

        /// <summary>
        /// Scans a number literal
        /// </summary>
        private void ScanNumber()
        {
            while (IsDigit(Peek())) Advance();

            if(Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek())) Advance();
            }

            AddToken(NUMBER, Double.Parse(_source.Substring(_start, _current-_start), CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Scans a identifier
        /// </summary>
        private void ScanIdentifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            var text = _source.Substring(_start, _current - _start);

            TokenType type = IDENTIFIER;

            if (keywords.ContainsKey(text))
            {
                type = keywords[text];
            }

            AddToken(type);
        }

        /// <summary>
        /// Add token to the list of tokens
        /// </summary>
        /// <param name="type">Type of the token</param>
        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        /// <summary>
        /// Add token to the list of tokens
        /// </summary>
        /// <param name="type">Type of the token</param>
        /// <param name="literal">literal belonging to the Token</param>
        private void AddToken(TokenType type, object literal)
        {
            var text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }

        /// <summary>
        /// Checks if given character is a Digit (0-9)
        /// </summary>
        /// <param name="c">Character to check</param>
        /// <returns>true if digit</returns>
        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        /// Checks if the given character is a-zA-Z or _
        /// </summary>
        /// <param name="c">character to check</param>
        /// <returns>true if matches</returns>
        private bool IsAlpha(char c)
        {
            return (
                 (c >= 'a' && c <= 'z') ||
                 (c >= 'A' && c <= 'Z') ||
                 c == '_'
            );
        }

        /// <summary>
        /// Checks if the Character is Digit or a-zA-Z or _
        /// </summary>
        /// <param name="c">Character to check</param>
        /// <returns>true if matches</returns>
        private bool IsAlphaNumeric(char c)
        {
            return IsDigit(c) || IsAlpha(c);
        }

        /// <summary>
        /// Check if the Scanner reached the end of the source
        /// </summary>
        /// <returns>true if at end</returns>
        private bool IsAtEnd()
        {
            return _current >= _source.Length; ;
        }
    }
}
