using ClassLibraryPredic.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryPredic.Models
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
}
