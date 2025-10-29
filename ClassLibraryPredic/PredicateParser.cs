using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClassLibraryPredic
{
    /// <summary>
    /// Расширённый парсер: арифметика (+ - * /), сравнения, логические операторы, кванторы:
    /// - forall x: expr
    /// - exists x: expr
    /// - поддерживаются unicode ∀ ∃ и ключевые слова forall/exists
    /// - опционально: "in {1,2,3}" после переменной в кванторе
    /// 
    /// Parse возвращает Func<Domain, IPredicate> — фабрику, чтобы подставить домены позже.
    /// </summary>
    public static class PredicateParser
    {
        #region Public API

        /// <summary>
        /// Парсит выражение и возвращает фабрику, принимающую Domain и дающую IPredicate.
        /// </summary>
        /// <param name="text">Строка выражения</param>
        /// <returns>Фабрика: Domain -> IPredicate</returns>
        public static Func<Domain, IPredicate> Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Пустое выражение");

            var tokenizer = new Tokenizer(text);
            var parser = new Parser(tokenizer);
            var node = parser.ParseExpression();

            // Возвращаем фабрику: при вызове даём Domain и получаем IPredicate
            return (domain) =>
            {
                // Если верхний узел — кванторный (Parser создаёт QuantifierNode), он уже хранит inlineDomain (если был).
                if (node is QuantifierNode qn)
                {
                    object[] domainValues;
                    if (qn.InlineDomain != null)
                    {
                        domainValues = qn.InlineDomain.ToArray();
                    }
                    else
                    {
                        // Берём домен из переданного Domain
                        if (domain == null) throw new ArgumentException($"Домены не заданы. Нужен домен для переменной '{qn.VariableName}'");
                        domainValues = domain.GetDomain(qn.VariableName);
                    }

                    // Внутренний IPredicate - это предикат, который вычисляет inner expression в контексте assignment
                    IPredicate innerPredicate = ExprToPredicate(qn.Inner);

                    if (qn.IsForAll)
                        return new ForAllPredicate(qn.VariableName, domainValues, innerPredicate);
                    else
                        return new ExistsPredicate(qn.VariableName, domainValues, innerPredicate);
                }
                else
                {
                    // Обычное выражение: создаём IPredicate, который при Evaluate вычисляет выражение и приводит к bool
                    var pred = ExprToPredicate(node);
                    return pred;
                }
            };
        }

        #endregion

        #region AST -> IPredicate

        // Преобразует ExprNode (возвращающий object) в IPredicate (boolean)
        private static IPredicate ExprToPredicate(ExprNode node)
        {
            return new AtomicPredicate(assign =>
            {
                var val = node.Evaluate(assign);
                // Обработка null и попытка приведения к bool
                if (val == null) return false;
                if (val is bool b) return b;
                // Числовые результаты: 0 -> false, остальные -> true
                if (val is IConvertible)
                {
                    try
                    {
                        var d = Convert.ToDouble(val);
                        return Math.Abs(d) > double.Epsilon;
                    }
                    catch
                    {
                        // не число
                    }
                }
                // Попытка привести строку "true"/"false"
                if (val is string s)
                {
                    if (bool.TryParse(s, out var rb)) return rb;
                    if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var rd)) return Math.Abs(rd) > double.Epsilon;
                }
                // По умолчанию - false
                return false;
            }, node.ToString());
        }

        #endregion

        #region Parser + Tokenizer + AST

        private enum TokenKind
        {
            Number,
            Identifier,
            Plus,
            Minus,
            Mul,
            Div,
            LParen,
            RParen,
            Eq,    // =
            Neq,   // !=
            Lt,
            Gt,
            Le,
            Ge,
            And,   // ∧ или &&
            Or,    // ∨ или ||
            Not,   // ¬ или !
            Imp,   // -> or => or →
            Eqv,   // <-> or <=> or ↔
            Comma,
            LBrace, // {
            RBrace, // }
            Colon,
            In,     // keyword 'in'
            ForAll, // 'forall' or '∀'
            Exists, // 'exists' or '∃'
            End
        }

        private record Token(TokenKind Kind, string Text, int Pos);

        private class Tokenizer
        {
            private readonly string _s;
            private int _i;
            public Tokenizer(string s) { _s = s; _i = 0; }
            private void SkipWs() { while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++; }
            public Token Next()
            {
                SkipWs();
                if (_i >= _s.Length) return new Token(TokenKind.End, "", _i);

                // Multi-char checks first
                // operators & punctuation
                if (_s[_i] == '{') { _i++; return new Token(TokenKind.LBrace, "{", _i - 1); }
                if (_s[_i] == '}') { _i++; return new Token(TokenKind.RBrace, "}", _i - 1); }
                if (_s[_i] == '(') { _i++; return new Token(TokenKind.LParen, "(", _i - 1); }
                if (_s[_i] == ')') { _i++; return new Token(TokenKind.RParen, ")", _i - 1); }
                if (_s[_i] == ',') { _i++; return new Token(TokenKind.Comma, ",", _i - 1); }
                if (_s[_i] == ':') { _i++; return new Token(TokenKind.Colon, ":", _i - 1); }
                if (_s[_i] == '+') { _i++; return new Token(TokenKind.Plus, "+", _i - 1); }
                if (_s[_i] == '*') { _i++; return new Token(TokenKind.Mul, "*", _i - 1); }
                if (_s[_i] == '/') { _i++; return new Token(TokenKind.Div, "/", _i - 1); }
                if (_s[_i] == '&') // allow && or single &
                {
                    int p = _i;
                    _i++;
                    if (_i < _s.Length && _s[_i] == '&') _i++;
                    return new Token(TokenKind.And, "∧", p);
                }
                if (_s[_i] == '|')
                {
                    int p = _i; _i++;
                    if (_i < _s.Length && _s[_i] == '|') _i++;
                    return new Token(TokenKind.Or, "∨", p);
                }

                // multi-char comparison tokens
                if (_s[_i] == '!' && _i + 1 < _s.Length && _s[_i + 1] == '=') { _i += 2; return new Token(TokenKind.Neq, "!=", _i - 2); }
                if (_s[_i] == '<')
                {
                    if (_i + 1 < _s.Length && _s[_i + 1] == '=') { _i += 2; return new Token(TokenKind.Le, "<=", _i - 2); }
                    if (_i + 2 < _s.Length && _s[_i + 1] == '-' && _s[_i + 2] == '>') { _i += 3; return new Token(TokenKind.Imp, "<->", _i - 3); }
                    _i++; return new Token(TokenKind.Lt, "<", _i - 1);
                }
                if (_s[_i] == '>')
                {
                    if (_i + 1 < _s.Length && _s[_i + 1] == '=') { _i += 2; return new Token(TokenKind.Ge, ">=", _i - 2); }
                    if (_i + 2 < _s.Length && _s[_i + 1] == '-' && _s[_i + 2] == '>') { _i += 3; return new Token(TokenKind.Imp, "->", _i - 3); }
                    _i++; return new Token(TokenKind.Gt, ">", _i - 1);
                }
                if (_s[_i] == '=')
                {
                    // allow => as implication if followed by '>'
                    if (_i + 1 < _s.Length && _s[_i + 1] == '>') { _i += 2; return new Token(TokenKind.Imp, "=>", _i - 2); }
                    _i++; return new Token(TokenKind.Eq, "=", _i - 1);
                }
                if (_s[_i] == '→' || _s[_i] == '⇒') { int p = _i; _i++; return new Token(TokenKind.Imp, "→", p); }
                if (_s[_i] == '↔') { int p = _i; _i++; return new Token(TokenKind.Eqv, "↔", p); }
                if (_s[_i] == '¬' || _s[_i] == '!') { int p = _i; _i++; return new Token(TokenKind.Not, "¬", p); }
                if (_s[_i] == '∧') { int p = _i; _i++; return new Token(TokenKind.And, "∧", p); }
                if (_s[_i] == '∨') { int p = _i; _i++; return new Token(TokenKind.Or, "∨", p); }
                if (_s[_i] == '∀') { int p = _i; _i++; return new Token(TokenKind.ForAll, "∀", p); }
                if (_s[_i] == '∃') { int p = _i; _i++; return new Token(TokenKind.Exists, "∃", p); }

                // numbers (int or double)
                if (char.IsDigit(_s[_i]) || (_s[_i] == '.' && _i + 1 < _s.Length && char.IsDigit(_s[_i + 1])))
                {
                    int start = _i;
                    while (_i < _s.Length && (char.IsDigit(_s[_i]) || _s[_i] == '.' || _s[_i] == 'e' || _s[_i] == 'E' || _s[_i] == '+' || _s[_i] == '-'))
                    {
                        // stop at letter following e sign if it's not part of exponent; simple approach: break on letter after digits
                        if (_s[_i] == 'e' || _s[_i] == 'E')
                        {
                            _i++;
                            if (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-')) _i++;
                            while (_i < _s.Length && char.IsDigit(_s[_i])) _i++;
                            break;
                        }
                        if (_s[_i] == '+' || _s[_i] == '-') // + and - inside number handled above; but avoid infinite loop
                        {
                            // allow leading +/-
                            _i++;
                            continue;
                        }
                        if (char.IsDigit(_s[_i]) || _s[_i] == '.') { _i++; continue; }
                        break;
                    }
                    var txt = _s.Substring(start, _i - start);
                    return new Token(TokenKind.Number, txt, start);
                }

                // identifiers / keywords
                if (char.IsLetter(_s[_i]) || _s[_i] == '_')
                {
                    int start = _i;
                    while (_i < _s.Length && (char.IsLetterOrDigit(_s[_i]) || _s[_i] == '_')) _i++;
                    var txt = _s.Substring(start, _i - start);
                    switch (txt.ToLowerInvariant())
                    {
                        case "and":
                        case "&&":
                            return new Token(TokenKind.And, txt, start);
                        case "or":
                        case "||":
                            return new Token(TokenKind.Or, txt, start);
                        case "not":
                            return new Token(TokenKind.Not, txt, start);
                        case "in":
                            return new Token(TokenKind.In, txt, start);
                        case "forall":
                            return new Token(TokenKind.ForAll, txt, start);
                        case "exists":
                            return new Token(TokenKind.Exists, txt, start);
                        default:
                            return new Token(TokenKind.Identifier, txt, start);
                    }
                }

                // Fallback: unknown char -> throw
                throw new Exception($"Неизвестный символ '{_s[_i]}' в позиции {_i}");
            }
        }

        // AST узлы: все ExprNode возвращают object (double для арифметики, bool для логики).
        private abstract class ExprNode
        {
            public abstract object Evaluate(IDictionary<string, object> assignment);
        }

        private class NumberNode : ExprNode
        {
            public double Value { get; }
            public NumberNode(double v) { Value = v; }
            public override object Evaluate(IDictionary<string, object> assignment) => Value;
            public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        }

        private class VariableNode : ExprNode
        {
            public string Name { get; }
            public VariableNode(string name) { Name = name; }
            public override object Evaluate(IDictionary<string, object> assignment)
            {
                if (!assignment.TryGetValue(Name, out var v))
                    throw new KeyNotFoundException($"Переменная '{Name}' не задана в назначении.");
                return v;
            }
            public override string ToString() => Name;
        }

        private class UnaryNode : ExprNode
        {
            public TokenKind Op { get; }
            public ExprNode Inner { get; }
            public UnaryNode(TokenKind op, ExprNode inner) { Op = op; Inner = inner; }
            public override object Evaluate(IDictionary<string, object> assignment)
            {
                var val = Inner.Evaluate(assignment);
                switch (Op)
                {
                    case TokenKind.Minus:
                        return -Convert.ToDouble(val);
                    case TokenKind.Plus:
                        return +Convert.ToDouble(val);
                    case TokenKind.Not:
                        if (val is bool b) return !b;
                        // numeric -> 0 -> false, else true
                        var d = Convert.ToDouble(val);
                        return Math.Abs(d) <= double.Epsilon ? false : true;
                    default:
                        throw new NotSupportedException($"Унарный оператор {Op} не поддерживается");
                }
            }
            public override string ToString() => Op == TokenKind.Not ? $"¬({Inner})" : $"({Op}{Inner})";
        }

        private class BinaryNode : ExprNode
        {
            public TokenKind Op { get; }
            public ExprNode Left { get; }
            public ExprNode Right { get; }
            public BinaryNode(TokenKind op, ExprNode left, ExprNode right) { Op = op; Left = left; Right = right; }

            public override object Evaluate(IDictionary<string, object> assignment)
            {
                var L = Left.Evaluate(assignment);
                var R = Right.Evaluate(assignment);

                switch (Op)
                {
                    // arithmetic
                    case TokenKind.Plus:
                        return Convert.ToDouble(L) + Convert.ToDouble(R);
                    case TokenKind.Minus:
                        return Convert.ToDouble(L) - Convert.ToDouble(R);
                    case TokenKind.Mul:
                        return Convert.ToDouble(L) * Convert.ToDouble(R);
                    case TokenKind.Div:
                        return Convert.ToDouble(L) / Convert.ToDouble(R);

                    // comparisons -> bool
                    case TokenKind.Eq:
                        return AreEqual(L, R);
                    case TokenKind.Neq:
                        return !AreEqual(L, R);
                    case TokenKind.Lt:
                        return Convert.ToDouble(L) < Convert.ToDouble(R);
                    case TokenKind.Gt:
                        return Convert.ToDouble(L) > Convert.ToDouble(R);
                    case TokenKind.Le:
                        return Convert.ToDouble(L) <= Convert.ToDouble(R);
                    case TokenKind.Ge:
                        return Convert.ToDouble(L) >= Convert.ToDouble(R);

                    // logical
                    case TokenKind.And:
                        return ToBool(L) && ToBool(R);
                    case TokenKind.Or:
                        return ToBool(L) || ToBool(R);
                    case TokenKind.Imp:
                        return !ToBool(L) || ToBool(R);
                    case TokenKind.Eqv:
                        return ToBool(L) == ToBool(R);

                    default:
                        throw new NotSupportedException($"Оператор {Op} не поддерживается");
                }
            }

            private static bool ToBool(object x)
            {
                if (x == null) return false;
                if (x is bool b) return b;
                if (x is IConvertible)
                {
                    try
                    {
                        var d = Convert.ToDouble(x);
                        return Math.Abs(d) > double.Epsilon;
                    }
                    catch { }
                }
                if (x is string s)
                {
                    if (bool.TryParse(s, out var rb)) return rb;
                    if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var rd)) return Math.Abs(rd) > double.Epsilon;
                }
                return false;
            }

            private static bool AreEqual(object a, object b)
            {
                if (a == null && b == null) return true;
                if (a == null || b == null) return false;
                // numeric compare
                if (a is IConvertible && b is IConvertible)
                {
                    try
                    {
                        var da = Convert.ToDouble(a);
                        var db = Convert.ToDouble(b);
                        return Math.Abs(da - db) <= 1e-9;
                    }
                    catch { }
                }
                return a.Equals(b);
            }

            public override string ToString() => $"({Left} {OpToText(Op)} {Right})";

            private static string OpToText(TokenKind op) => op switch
            {
                TokenKind.Plus => "+",
                TokenKind.Minus => "-",
                TokenKind.Mul => "*",
                TokenKind.Div => "/",
                TokenKind.Eq => "=",
                TokenKind.Neq => "!=",
                TokenKind.Lt => "<",
                TokenKind.Gt => ">",
                TokenKind.Le => "<=",
                TokenKind.Ge => ">=",
                TokenKind.And => "∧",
                TokenKind.Or => "∨",
                TokenKind.Imp => "→",
                TokenKind.Eqv => "↔",
                _ => op.ToString()
            };
        }

        // Узел квантора (хранит внутреннее выражение). InlineDomain может быть null.
        private class QuantifierNode : ExprNode
        {
            public bool IsForAll { get; }
            public string VariableName { get; }
            public List<object> InlineDomain { get; } // может быть null
            public ExprNode Inner { get; }
            public QuantifierNode(bool isForAll, string varName, List<object> inlineDomain, ExprNode inner)
            {
                IsForAll = isForAll; VariableName = varName; InlineDomain = inlineDomain; Inner = inner;
            }

            // Evaluate напрямую не используется — QuantifierNode обрабатывается при создании IPredicate.
            public override object Evaluate(IDictionary<string, object> assignment)
            {
                // Пробуем вычислить inner в текущем assignment
                return Inner.Evaluate(assignment);
            }

            public override string ToString()
            {
                var domainTxt = InlineDomain != null ? $" in {{{string.Join(",", InlineDomain)}}}" : "";
                return $"{(IsForAll ? "forall" : "exists")} {VariableName}{domainTxt}: ({Inner})";
            }
        }

        // Парсер (recursive descent)
        private class Parser
        {
            private readonly Tokenizer _tk;
            private Token _cur;

            public Parser(Tokenizer tk) { _tk = tk; _cur = _tk.Next(); }

            private void Next() => _cur = _tk.Next();
            private bool Eat(TokenKind k)
            {
                if (_cur.Kind == k) { Next(); return true; }
                return false;
            }
            private void Expect(TokenKind k)
            {
                if (_cur.Kind != k) throw new Exception($"Ожидался токен {k}, но найден {_cur.Kind} (pos {_cur.Pos})");
                Next();
            }

            // Top-level: поддерживаем возможный квантор в начале
            public ExprNode ParseExpression()
            {
                if (_cur.Kind == TokenKind.ForAll || _cur.Kind == TokenKind.Exists)
                {
                    bool isForAll = _cur.Kind == TokenKind.ForAll;
                    Next();

                    // variable name
                    if (_cur.Kind != TokenKind.Identifier) throw new Exception($"Ожидалось имя переменной после квантора (pos {_cur.Pos})");
                    var varName = _cur.Text;
                    Next();

                    // optional: in {1,2,3}
                    List<object> inlineDomain = null;
                    if (_cur.Kind == TokenKind.In)
                    {
                        Next();
                        // expect { number (, number)* }
                        Expect(TokenKind.LBrace);
                        inlineDomain = new List<object>();
                        while (true)
                        {
                            if (_cur.Kind != TokenKind.Number) throw new Exception($"Ожидалось число в inline-домене (pos {_cur.Pos})");
                            inlineDomain.Add(ParseNumberText(_cur.Text));
                            Next();
                            if (_cur.Kind == TokenKind.Comma) { Next(); continue; }
                            break;
                        }
                        Expect(TokenKind.RBrace);
                    }

                    // optional colon
                    if (_cur.Kind == TokenKind.Colon) Next();
                    // parse inner expression
                    var inner = ParseImplication();
                    return new QuantifierNode(isForAll, varName, inlineDomain, inner);
                }

                return ParseImplication();
            }

            // implication -> equivalence (->)
            private ExprNode ParseImplication()
            {
                var left = ParseEquivalence();
                while (_cur.Kind == TokenKind.Imp)
                {
                    var op = _cur.Kind;
                    Next();
                    var right = ParseEquivalence();
                    left = new BinaryNode(op, left, right);
                }
                return left;
            }

            private ExprNode ParseEquivalence()
            {
                var left = ParseOr();
                while (_cur.Kind == TokenKind.Eqv)
                {
                    var op = _cur.Kind;
                    Next();
                    var right = ParseOr();
                    left = new BinaryNode(op, left, right);
                }
                return left;
            }

            private ExprNode ParseOr()
            {
                var left = ParseAnd();
                while (_cur.Kind == TokenKind.Or)
                {
                    var op = _cur.Kind;
                    Next();
                    var right = ParseAnd();
                    left = new BinaryNode(op, left, right);
                }
                return left;
            }

            private ExprNode ParseAnd()
            {
                var left = ParseNot();
                while (_cur.Kind == TokenKind.And)
                {
                    var op = _cur.Kind;
                    Next();
                    var right = ParseNot();
                    left = new BinaryNode(op, left, right);
                }
                return left;
            }

            private ExprNode ParseNot()
            {
                if (_cur.Kind == TokenKind.Not)
                {
                    Next();
                    var inner = ParseNot();
                    return new UnaryNode(TokenKind.Not, inner);
                }
                return ParseComparison();
            }

            // Comparisons: left (==, !=, >, <, >=, <=) right
            private ExprNode ParseComparison()
            {
                var left = ParseAdd();
                while (_cur.Kind == TokenKind.Eq || _cur.Kind == TokenKind.Neq
                    || _cur.Kind == TokenKind.Lt || _cur.Kind == TokenKind.Gt
                    || _cur.Kind == TokenKind.Le || _cur.Kind == TokenKind.Ge)
                {
                    var op = _cur.Kind;
                    Next();
                    var right = ParseAdd();
                    left = new BinaryNode(op, left, right);
                }
                return left;
            }

            private ExprNode ParseAdd()
            {
                var left = ParseMul();
                while (_cur.Kind == TokenKind.Plus || _cur.Kind == TokenKind.Minus)
                {
                    var op = _cur.Kind;
                    Next();
                    var right = ParseMul();
                    left = new BinaryNode(op, left, right);
                }
                return left;
            }

            private ExprNode ParseMul()
            {
                var left = ParseUnary();
                while (_cur.Kind == TokenKind.Mul || _cur.Kind == TokenKind.Div)
                {
                    var op = _cur.Kind;
                    Next();
                    var right = ParseUnary();
                    left = new BinaryNode(op, left, right);
                }
                return left;
            }

            private ExprNode ParseUnary()
            {
                if (_cur.Kind == TokenKind.Plus)
                {
                    Next();
                    return new UnaryNode(TokenKind.Plus, ParseUnary());
                }
                if (_cur.Kind == TokenKind.Minus)
                {
                    Next();
                    return new UnaryNode(TokenKind.Minus, ParseUnary());
                }
                if (_cur.Kind == TokenKind.Not)
                {
                    Next();
                    return new UnaryNode(TokenKind.Not, ParseUnary());
                }
                return ParsePrimary();
            }

            private ExprNode ParsePrimary()
            {
                if (_cur.Kind == TokenKind.Number)
                {
                    var val = ParseNumberText(_cur.Text);
                    Next();
                    return new NumberNode(val);
                }
                if (_cur.Kind == TokenKind.Identifier)
                {
                    var name = _cur.Text;
                    Next();
                    return new VariableNode(name);
                }
                if (_cur.Kind == TokenKind.LParen)
                {
                    Next();
                    var inner = ParseExpression();
                    Expect(TokenKind.RParen);
                    return inner;
                }
                throw new Exception($"Неожиданный токен {_cur.Kind} (pos {_cur.Pos})");
            }

            private static double ParseNumberText(string txt)
            {
                if (double.TryParse(txt, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) return d;
                return double.Parse(txt, CultureInfo.InvariantCulture);
            }
        }

        #endregion
    }
}
