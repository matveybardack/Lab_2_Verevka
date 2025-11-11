using System.Text.RegularExpressions;

namespace WpfAppPredic
{
    public static class EquationParser
    {
        private static readonly string[] LogicalOperators = { "&&", "||", "->", "<->" };
        private static readonly string[] ComparisonOperators = { "<=", ">=", "==", "!=", "<", ">" };

        public static List<string> ParseEquations(string predicateText)
        {
            var equations = new List<string>();

            if (string.IsNullOrWhiteSpace(predicateText))
                return equations;

            // Разделяем по переводам строк
            var lines = predicateText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (!string.IsNullOrEmpty(trimmedLine))
                {
                    // Проверяем, содержит ли строка логические операторы
                    if (ContainsLogicalOperators(trimmedLine))
                    {
                        equations.Add(trimmedLine);
                    }
                    else
                    {
                        equations.Add(trimmedLine);
                    }
                }
            }

            return equations;
        }

        private static bool ContainsLogicalOperators(string text)
        {
            foreach (var op in LogicalOperators)
            {
                if (text.Contains(op))
                    return true;
            }
            return false;
        }

        public static string ExtractBaseEquation(string equation)
        {
            // Удаляем кванторы и скобки для получения базового уравнения
            var baseEq = Regex.Replace(equation, @"[∀∃][xy]\s*", ""); // Удаляем кванторы
            baseEq = Regex.Replace(baseEq, @"^\(|\)$", ""); // Удаляем внешние скобки
            return baseEq.Trim();
        }
    }
}
