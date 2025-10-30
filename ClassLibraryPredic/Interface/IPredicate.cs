using System.Collections.Generic;

namespace ClassLibraryPredic
{
    /// <summary>
    /// Базовый интерфейс предиката — может быть вычислен для конкретного назначения переменных.
    /// Назначение задаётся как словарь "имя переменной" -> значение объекта.
    /// </summary>
    public interface IPredicate
    {
        /// <summary>
        /// Оценить предикат при данном назначении переменных.
        /// Бросает исключение, если значение переменной отсутствует.
        /// </summary>
        bool Evaluate(IDictionary<string, object> assignment);

        /// <summary>
        /// Короткое текстовое представление предиката (имя/описание).
        /// </summary>
        string Description { get; }
    }
}
