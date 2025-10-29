using ClassLibraryPredic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryPredic
{
    public enum PredicateType
    {
        Tautology,
        Contradiction,
        Satisfiable
    }

    public static class PredicateAnalyzer
    {
        /// <summary>
        /// Возвращает все назначения переменных (комбинации) из domain,
        /// для которых predicate даёт true.
        /// </summary>
        public static List<Dictionary<string, object>> ComputeTruthSet(IPredicate predicate, Domain domain)
        {
            var vars = domain.Variables.ToArray();
            var domains = vars.Select(v => domain.GetDomain(v)).ToArray();

            var truthAssignments = new List<Dictionary<string, object>>();

            // рекурсивная генерация комбинаций
            void Recur(int idx, Dictionary<string, object> current)
            {
                if (idx == vars.Length)
                {
                    // копия словаря
                    var copy = new Dictionary<string, object>(current);
                    try
                    {
                        if (predicate.Evaluate(copy))
                        {
                            truthAssignments.Add(copy);
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки при отсутствии переменных внутри предиката
                    }
                    return;
                }

                var varName = vars[idx];
                foreach (var val in domains[idx])
                {
                    current[varName] = val;
                    Recur(idx + 1, current);
                }
                current.Remove(varName);
            }

            Recur(0, new Dictionary<string, object>());

            return truthAssignments;
        }

        /// <summary>
        /// Определяет тип предиката над заданной конечной областью.
        /// </summary>
        public static PredicateType DetermineType(IPredicate predicate, Domain domain)
        {
            var vars = domain.Variables.ToArray();
            var domains = vars.Select(v => domain.GetDomain(v)).ToArray();

            bool hasTrue = false, hasFalse = false;

            void Recur(int idx, Dictionary<string, object> current)
            {
                if (idx == vars.Length)
                {
                    try
                    {
                        var res = predicate.Evaluate(current);
                        if (res) hasTrue = true; else hasFalse = true;
                    }
                    catch
                    {
                        // если предикат обращается к неизвестной переменной, считаем, что это false для этой комбинации
                        hasFalse = true;
                    }
                    return;
                }

                var varName = vars[idx];
                foreach (var val in domains[idx])
                {
                    if (hasTrue && hasFalse) return;
                    current[varName] = val;
                    Recur(idx + 1, current);
                }
                current.Remove(varName);
            }

            Recur(0, new Dictionary<string, object>());

            if (hasTrue && !hasFalse) return PredicateType.Tautology;
            if (!hasTrue && hasFalse) return PredicateType.Contradiction;
            return PredicateType.Satisfiable;
        }

        /// <summary>
        /// Генератор таблицы истинности: возвращает каждую комбинацию и значение предиката.
        /// </summary>
        public static IEnumerable<(Dictionary<string, object> Assignment, bool Value)> TruthTable(IPredicate predicate, Domain domain)
        {
            var vars = domain.Variables.ToArray();
            var domains = vars.Select(v => domain.GetDomain(v)).ToArray();

            void Recur(int idx, Dictionary<string, object> current, Action<Dictionary<string, object>> action)
            {
                if (idx == vars.Length)
                {
                    action(new Dictionary<string, object>(current));
                    return;
                }

                var varName = vars[idx];
                foreach (var val in domains[idx])
                {
                    current[varName] = val;
                    Recur(idx + 1, current, action);
                }
                current.Remove(varName);
            }

            var list = new List<(Dictionary<string, object>, bool)>();
            Recur(0, new Dictionary<string, object>(), (assignment) =>
            {
                bool res = false;
                try
                {
                    res = predicate.Evaluate(assignment);
                }
                catch
                {
                    res = false;
                }
                list.Add((assignment, res));
            });

            return list;
        }
    }
}
