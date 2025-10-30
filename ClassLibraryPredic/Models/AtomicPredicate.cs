using ClassLibraryPredic.Interface;
using System;
using System.Collections.Generic;

namespace ClassLibraryPredic.Models
{
    /// <summary>
    /// Атомарный предикат, определяется функцией evaluator,
    /// использующей текущие значения переменных.
    /// </summary>
    public class AtomicPredicate : IPredicate
    {
        private readonly Func<IDictionary<string, object>, bool> _evaluator;

        public AtomicPredicate(Func<IDictionary<string, object>, bool> evaluator, string description = "Atomic")
        {
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            Description = description;
        }

        public string Description { get; }

        public bool Evaluate(IDictionary<string, object> assignment)
        {
            return _evaluator(assignment);
        }

        public override string ToString() => Description;
    }
}
