using ClassLibraryPredic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryPredic
{
    /// <summary>
    /// Базовый класс для кванторных предикатов, использует конечную область определения для переменной.
    /// </summary>
    public abstract class QuantifierPredicate : IPredicate
    {
        protected QuantifierPredicate(string variableName, IEnumerable<object> domain, IPredicate inner, string symbol)
        {
            if (string.IsNullOrWhiteSpace(variableName)) throw new ArgumentNullException(nameof(variableName));
            VariableName = variableName;
            Domain = domain?.ToArray() ?? throw new ArgumentNullException(nameof(domain));
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
            Description = $"{symbol}{variableName}.({Inner.Description})";
        }

        public string VariableName { get; }
        public object[] Domain { get; }
        public IPredicate Inner { get; }
        public string Description { get; }

        public abstract bool Evaluate(IDictionary<string, object> assignment);
    }

    public class ForAllPredicate : QuantifierPredicate
    {
        public ForAllPredicate(string variableName, IEnumerable<object> domain, IPredicate inner)
            : base(variableName, domain, inner, "∀")
        { }

        public override bool Evaluate(IDictionary<string, object> assignment)
        {
            // Перебираем все значения переменной в области; при каждом временно подставляем
            foreach (var val in Domain)
            {
                assignment[VariableName] = val;
                if (!Inner.Evaluate(assignment))
                {
                    return false; // найдена ложная подстановка -> квантор ложен
                }
            }
            return true;
        }
    }

    public class ExistsPredicate : QuantifierPredicate
    {
        public ExistsPredicate(string variableName, IEnumerable<object> domain, IPredicate inner)
            : base(variableName, domain, inner, "∃")
        { }

        public override bool Evaluate(IDictionary<string, object> assignment)
        {
            foreach (var val in Domain)
            {
                assignment[VariableName] = val;
                if (Inner.Evaluate(assignment))
                {
                    return true; // найдена истинная подстановка -> квантор истиннен
                }
            }
            return false;
        }
    }
}
