using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibraryPredic.Models
{
    /// <summary>
    /// Простая обёртка для областей определения переменных.
    /// Позволяет задать конечную область по имени переменной.
    /// </summary>
    public class Domain
    {
        private readonly Dictionary<string, object[]> _map = new();

        public void SetDomain(string variable, IEnumerable<object> values)
        {
            if (string.IsNullOrWhiteSpace(variable)) throw new ArgumentNullException(nameof(variable));
            _map[variable] = values?.ToArray() ?? throw new ArgumentNullException(nameof(values));
        }

        public IReadOnlyDictionary<string, object[]> Map => _map;

        public IEnumerable<string> Variables => _map.Keys;

        public object[] GetDomain(string variable)
        {
            if (!_map.TryGetValue(variable, out var arr)) throw new KeyNotFoundException(variable);
            return arr;
        }
    }
}
