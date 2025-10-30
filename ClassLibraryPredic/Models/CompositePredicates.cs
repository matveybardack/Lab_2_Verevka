using ClassLibraryPredic.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryPredic.Models
{
    public class NotPredicate : IPredicate
    {
        public NotPredicate(IPredicate inner)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
            Description = $"¬({Inner.Description})";
        }

        public IPredicate Inner { get; }
        public string Description { get; }

        public bool Evaluate(IDictionary<string, object> assignment) => !Inner.Evaluate(assignment);
    }

    public class AndPredicate : IPredicate
    {
        public AndPredicate(params IPredicate[] operands)
        {
            Operands = operands ?? throw new ArgumentNullException(nameof(operands));
            Description = $"({string.Join(" ∧ ", Operands.Select(o => o.Description))})";
        }

        public IPredicate[] Operands { get; }
        public string Description { get; }

        public bool Evaluate(IDictionary<string, object> assignment) => Operands.All(p => p.Evaluate(assignment));
    }

    public class OrPredicate : IPredicate
    {
        public OrPredicate(params IPredicate[] operands)
        {
            Operands = operands ?? throw new ArgumentNullException(nameof(operands));
            Description = $"({string.Join(" ∨ ", Operands.Select(o => o.Description))})";
        }

        public IPredicate[] Operands { get; }
        public string Description { get; }

        public bool Evaluate(IDictionary<string, object> assignment) => Operands.Any(p => p.Evaluate(assignment));
    }

    public class ImplicationPredicate : IPredicate
    {
        public ImplicationPredicate(IPredicate left, IPredicate right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
            Description = $"({Left.Description} → {Right.Description})";
        }

        public IPredicate Left { get; }
        public IPredicate Right { get; }
        public string Description { get; }

        public bool Evaluate(IDictionary<string, object> assignment)
        {
            var a = Left.Evaluate(assignment);
            var b = Right.Evaluate(assignment);
            // импликация: ложна только когда A=true и B=false
            return !a || b;
        }
    }

    public class EquivalencePredicate : IPredicate
    {
        public EquivalencePredicate(IPredicate left, IPredicate right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
            Description = $"({Left.Description} ↔ {Right.Description})";
        }

        public IPredicate Left { get; }
        public IPredicate Right { get; }
        public string Description { get; }

        public bool Evaluate(IDictionary<string, object> assignment)
        {
            return Left.Evaluate(assignment) == Right.Evaluate(assignment);
        }
    }
}
