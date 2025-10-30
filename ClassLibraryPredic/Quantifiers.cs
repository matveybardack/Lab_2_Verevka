using ClassLibraryPredic.Interface;
using ClassLibraryPredic.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryPredic
{
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
