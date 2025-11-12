using ClassLibraryPredic;
using ClassLibraryPredic.Interface;
using ClassLibraryPredic.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace ClassLibraryPredic.Tests
{
    /// <summary>
    /// Модульные тесты для библиотеки логических предикатов и кванторов
    /// Тестирование основных компонентов: атомарных предикатов, композитных предикатов,
    /// операций с доменами, анализа предикатов и парсера выражений
    /// </summary>

    #region ТЕСТЫ АТОМАРНЫХ ПРЕДИКАТОВ
    /// <summary>
    /// Тестирование класса AtomicPredicate - базового элемента системы предикатов
    /// </summary>
    public class AtomicPredicateTests
    {
        #region Положительные тесты
        /// <summary>
        /// Проверка корректного вычисления атомарного предиката с истинным условием
        /// Ожидается возврат true при выполнении условия
        /// </summary>
        [Fact]
        public void AtomicPredicate_Evaluate_ReturnsCorrectValue()
        {
            // Arrange - подготовка тестовых данных
            var predicate = new AtomicPredicate(
                assignment => (int)assignment["x"] > 5,
                "x > 5"
            );
            var assignment = new Dictionary<string, object> { ["x"] = 10 };

            // Act - выполнение тестируемого действия
            var result = predicate.Evaluate(assignment);

            // Assert - проверка результата
            Assert.True(result);
        }

        /// <summary>
        /// Проверка вычисления атомарного предиката с ложным условием
        /// Ожидается возврат false при невыполнении условия
        /// </summary>
        [Fact]
        public void AtomicPredicate_Evaluate_WithFalseCondition_ReturnsFalse()
        {
            // Arrange
            var predicate = new AtomicPredicate(
                assignment => (int)assignment["x"] > 5,
                "x > 5"
            );
            var assignment = new Dictionary<string, object> { ["x"] = 3 };

            // Act
            var result = predicate.Evaluate(assignment);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region Отрицательные тесты
        /// <summary>
        /// Проверка обработки отсутствующей переменной при вычислении предиката
        /// Ожидается исключение KeyNotFoundException
        /// </summary>
        [Fact]
        public void AtomicPredicate_Evaluate_WithMissingVariable_ThrowsException()
        {
            // Arrange
            var predicate = new AtomicPredicate(
                assignment => (int)assignment["x"] > 5,
                "x > 5"
            );
            var assignment = new Dictionary<string, object> { ["y"] = 10 }; // x is missing

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => predicate.Evaluate(assignment));
        }
        #endregion
    }
    #endregion

    #region ТЕСТЫ КОМПОЗИТНЫХ ПРЕДИКАТОВ (ЛОГИЧЕСКИЕ ОПЕРАЦИИ)
    /// <summary>
    /// Тестирование композитных предикатов - логических операций над предикатами
    /// </summary>
    public class CompositePredicateTests
    {
        #region Тесты отрицания (NOT)
        /// <summary>
        /// Тестирование операции отрицания (NOT)
        /// Ожидается инвертирование результата внутреннего предиката
        /// </summary>
        [Fact]
        public void NotPredicate_Evaluate_ReturnsNegatedValue()
        {
            // Arrange
            var inner = new AtomicPredicate(
                assignment => (int)assignment["x"] > 5,
                "x > 5"
            );
            var notPredicate = new NotPredicate(inner);
            var assignment = new Dictionary<string, object> { ["x"] = 10 };

            // Act
            var result = notPredicate.Evaluate(assignment);

            // Assert
            Assert.False(result); // 10 > 5 is true, so NOT should be false
        }
        #endregion

        #region Тесты конъюнкции (AND)
        /// <summary>
        /// Тестирование операции конъюнкции (AND) при истинности всех операндов
        /// Ожидается возврат true только когда все предикаты истинны
        /// </summary>
        [Fact]
        public void AndPredicate_Evaluate_WithAllTrue_ReturnsTrue()
        {
            // Arrange
            var pred1 = new AtomicPredicate(assignment => (int)assignment["x"] > 0, "x > 0");
            var pred2 = new AtomicPredicate(assignment => (int)assignment["x"] < 10, "x < 10");
            var andPredicate = new AndPredicate(pred1, pred2);
            var assignment = new Dictionary<string, object> { ["x"] = 5 };

            // Act
            var result = andPredicate.Evaluate(assignment);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Тестирование операции конъюнкции (AND) при наличии ложного операнда
        /// Ожидается возврат false если хотя бы один предикат ложен
        /// </summary>
        [Fact]
        public void AndPredicate_Evaluate_WithOneFalse_ReturnsFalse()
        {
            // Arrange
            var pred1 = new AtomicPredicate(assignment => (int)assignment["x"] > 0, "x > 0");
            var pred2 = new AtomicPredicate(assignment => (int)assignment["x"] < 3, "x < 3");
            var andPredicate = new AndPredicate(pred1, pred2);
            var assignment = new Dictionary<string, object> { ["x"] = 5 };

            // Act
            var result = andPredicate.Evaluate(assignment);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region Тесты дизъюнкции (OR)
        /// <summary>
        /// Тестирование операции дизъюнкции (OR) при истинности одного операнда
        /// Ожидается возврат true если хотя бы один предикат истинен
        /// </summary>
        [Fact]
        public void OrPredicate_Evaluate_WithOneTrue_ReturnsTrue()
        {
            // Arrange
            var pred1 = new AtomicPredicate(assignment => (int)assignment["x"] > 10, "x > 10");
            var pred2 = new AtomicPredicate(assignment => (int)assignment["x"] < 5, "x < 5");
            var orPredicate = new OrPredicate(pred1, pred2);
            var assignment = new Dictionary<string, object> { ["x"] = 3 };

            // Act
            var result = orPredicate.Evaluate(assignment);

            // Assert
            Assert.True(result);
        }
        #endregion

        #region Тесты импликации (->)
        /// <summary>
        /// Тестирование операции импликации (->) - проверка таблицы истинности
        /// Импликация ложна только когда A=true и B=false
        /// </summary>
        [Fact]
        public void ImplicationPredicate_Evaluate_ReturnsCorrectTruthTable()
        {
            // Arrange
            var left = new AtomicPredicate(assignment => (bool)assignment["A"], "A");
            var right = new AtomicPredicate(assignment => (bool)assignment["B"], "B");
            var implication = new ImplicationPredicate(left, right);

            // Test cases: A -> B is false only when A=true and B=false
            var testCases = new[]
            {
                (A: true, B: true, Expected: true),
                (A: true, B: false, Expected: false),
                (A: false, B: true, Expected: true),
                (A: false, B: false, Expected: true)
            };

            foreach (var (a, b, expected) in testCases)
            {
                // Act
                var assignment = new Dictionary<string, object> { ["A"] = a, ["B"] = b };
                var result = implication.Evaluate(assignment);

                // Assert
                Assert.Equal(expected, result);
            }
        }
        #endregion

        #region Тесты эквивалентности (<->)
        /// <summary>
        /// Тестирование операции эквивалентности (<->) - проверка таблицы истинности
        /// Эквивалентность истинна когда оба операнда равны
        /// </summary>
        [Fact]
        public void EquivalencePredicate_Evaluate_ReturnsCorrectTruthTable()
        {
            // Arrange
            var left = new AtomicPredicate(assignment => (bool)assignment["A"], "A");
            var right = new AtomicPredicate(assignment => (bool)assignment["B"], "B");
            var equivalence = new EquivalencePredicate(left, right);

            // Test cases: A <-> B is true when both are equal
            var testCases = new[]
            {
                (A: true, B: true, Expected: true),
                (A: true, B: false, Expected: false),
                (A: false, B: true, Expected: false),
                (A: false, B: false, Expected: true)
            };

            foreach (var (a, b, expected) in testCases)
            {
                // Act
                var assignment = new Dictionary<string, object> { ["A"] = a, ["B"] = b };
                var result = equivalence.Evaluate(assignment);

                // Assert
                Assert.Equal(expected, result);
            }
        }
        #endregion

        #region Отрицательные тесты
        /// <summary>
        /// Проверка валидации конструктора NotPredicate с null-аргументом
        /// Ожидается исключение ArgumentNullException
        /// </summary>
        [Fact]
        public void NotPredicate_Constructor_WithNullInner_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new NotPredicate(null));
        }
        #endregion
    }
    #endregion

    #region ТЕСТЫ РАБОТЫ С ДОМЕНАМИ (ОБЛАСТЯМИ ОПРЕДЕЛЕНИЯ)
    /// <summary>
    /// Тестирование класса Domain - управления областями определения переменных
    /// </summary>
    public class DomainTests
    {
        #region Положительные тесты
        /// <summary>
        /// Проверка корректного сохранения и извлечения значений домена
        /// Ожидается точное соответствие сохраненных и полученных значений
        /// </summary>
        [Fact]
        public void Domain_SetDomain_StoresValuesCorrectly()
        {
            // Arrange
            var domain = new Domain();
            var values = new object[] { 1, 2, 3, 4, 5 };

            // Act
            domain.SetDomain("x", values);

            // Assert
            var storedValues = domain.GetDomain("x");
            Assert.Equal(values, storedValues);
        }

        /// <summary>
        /// Проверка получения списка всех переменных домена
        /// Ожидается корректное отображение всех зарегистрированных переменных
        /// </summary>
        [Fact]
        public void Domain_Variables_ReturnsAllVariableNames()
        {
            // Arrange
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2 });
            domain.SetDomain("y", new object[] { "a", "b" });

            // Act
            var variables = domain.Variables;

            // Assert
            Assert.Contains("x", variables);
            Assert.Contains("y", variables);
            Assert.Equal(2, new List<string>(variables).Count);
        }
        #endregion

        #region Отрицательные тесты
        /// <summary>
        /// Проверка обработки попытки получения домена для несуществующей переменной
        /// Ожидается исключение KeyNotFoundException
        /// </summary>
        [Fact]
        public void Domain_GetDomain_ForNonExistentVariable_ThrowsException()
        {
            // Arrange
            var domain = new Domain();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => domain.GetDomain("nonexistent"));
        }

        /// <summary>
        /// Проверка валидации установки домена с null-именем переменной
        /// Ожидается исключение ArgumentNullException
        /// </summary>
        [Fact]
        public void Domain_SetDomain_WithNullVariable_ThrowsException()
        {
            // Arrange
            var domain = new Domain();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => domain.SetDomain(null, new object[] { 1, 2, 3 }));
        }
        #endregion
    }
    #endregion

    #region ТЕСТЫ АНАЛИЗАТОРА ПРЕДИКАТОВ
    /// <summary>
    /// Тестирование класса PredicateAnalyzer - анализа свойств предикатов
    /// </summary>
    public class PredicateAnalyzerTests
    {
        #region Тесты определения типа предиката
        /// <summary>
        /// Проверка определения тавтологии - предиката истинного при всех назначениях
        /// Ожидается возврат PredicateType.Tautology
        /// </summary>
        [Fact]
        public void DetermineType_WithTautology_ReturnsTautology()
        {
            // Arrange
            var predicate = new AtomicPredicate(assignment => true, "true");
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2, 3 });

            // Act
            var result = PredicateAnalyzer.DetermineType(predicate, domain);

            // Assert
            Assert.Equal(PredicateType.Tautology, result);
        }

        /// <summary>
        /// Проверка определения противоречия - предиката ложного при всех назначениях
        /// Ожидается возврат PredicateType.Contradiction
        /// </summary>
        [Fact]
        public void DetermineType_WithContradiction_ReturnsContradiction()
        {
            // Arrange
            var predicate = new AtomicPredicate(assignment => false, "false");
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2, 3 });

            // Act
            var result = PredicateAnalyzer.DetermineType(predicate, domain);

            // Assert
            Assert.Equal(PredicateType.Contradiction, result);
        }

        /// <summary>
        /// Проверка определения выполнимого предиката - истинного при некоторых назначениях
        /// Ожидается возврат PredicateType.Satisfiable
        /// </summary>
        [Fact]
        public void DetermineType_WithSatisfiable_ReturnsSatisfiable()
        {
            // Arrange
            var predicate = new AtomicPredicate(
                assignment => (int)assignment["x"] > 2,
                "x > 2"
            );
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2, 3, 4 });

            // Act
            var result = PredicateAnalyzer.DetermineType(predicate, domain);

            // Assert
            Assert.Equal(PredicateType.Satisfiable, result);
        }
        #endregion

        #region Тесты вычисления области истинности
        /// <summary>
        /// Проверка вычисления области истинности предиката
        /// Ожидается возврат только тех назначений, где предикат истинен
        /// </summary>
        [Fact]
        public void ComputeTruthSet_ReturnsCorrectAssignments()
        {
            // Arrange
            var predicate = new AtomicPredicate(
                assignment => (int)assignment["x"] > 2,
                "x > 2"
            );
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2, 3, 4, 5 });

            // Act
            var truthSet = PredicateAnalyzer.ComputeTruthSet(predicate, domain);

            // Assert
            Assert.Equal(3, truthSet.Count); // Only 3,4,5 should be in truth set
            Assert.All(truthSet, assignment => Assert.True((int)assignment["x"] > 2));
        }
        #endregion

        #region Тесты таблицы истинности
        /// <summary>
        /// Проверка генерации таблицы истинности
        /// Ожидается корректное сопоставление всех назначений и значений предиката
        /// </summary>
        [Fact]
        public void TruthTable_ReturnsAllCombinationsWithValues()
        {
            // Arrange
            var predicate = new AtomicPredicate(
                assignment => (int)assignment["x"] % 2 == 0, // true for even numbers
                "x is even"
            );
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2, 3, 4 });

            // Act
            var truthTable = PredicateAnalyzer.TruthTable(predicate, domain);

            // Assert
            var tableList = new List<(Dictionary<string, object>, bool)>(truthTable);
            Assert.Equal(4, tableList.Count); // All combinations

            // Check specific values
            Assert.Contains(tableList, item => (int)item.Item1["x"] == 2 && item.Item2 == true);
            Assert.Contains(tableList, item => (int)item.Item1["x"] == 3 && item.Item2 == false);
        }
        #endregion
    }
    #endregion

    #region ТЕСТЫ ПАРСЕРА ВЫРАЖЕНИЙ
    /// <summary>
    /// Тестирование парсера логических выражений - преобразования строк в предикаты
    /// </summary>
    public class ParserTests
    {
        #region Положительные тесты
        /// <summary>
        /// Проверка парсинга простого выражения сравнения
        /// Ожидается корректное создание предиката из строки
        /// </summary>
        [Fact]
        public void Parse_SimpleComparison_ReturnsCorrectPredicate()
        {
            // Arrange
            var expression = "x > 5";

            // Act
            var predicateFactory = PredicateParser.Parse(expression);
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 3, 6, 8 });
            var predicate = predicateFactory(domain);

            // Assert
            Assert.NotNull(predicate);
            Assert.Contains("x > 5", predicate.Description);
        }

        /// <summary>
        /// Проверка парсинга сложного логического выражения с конъюнкцией
        /// Ожидается корректное создание составного предиката
        /// </summary>
        [Fact]
        public void Parse_ComplexLogicalExpression_ReturnsCorrectPredicate()
        {
            // Arrange
            var expression = "(x > 2) ∧ (x < 8)";

            // Act
            var predicateFactory = PredicateParser.Parse(expression);
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 3, 6, 8, 10 });
            var predicate = predicateFactory(domain);

            // Assert
            Assert.NotNull(predicate);
        }

        /// <summary>
        /// Проверка парсинга выражения с арифметическими операциями
        /// Ожидается корректная обработка математических выражений
        /// </summary>
        [Fact]
        public void Parse_ArithmeticExpression_ReturnsCorrectPredicate()
        {
            // Arrange
            var expression = "x + y * 2 > 10";

            // Act
            var predicateFactory = PredicateParser.Parse(expression);
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 5, 10 });
            domain.SetDomain("y", new object[] { 2, 4, 6 });
            var predicate = predicateFactory(domain);

            // Assert
            Assert.NotNull(predicate);
        }
        #endregion

        #region Тесты кванторов
        /// <summary>
        /// Проверка парсинга выражения с квантором общности
        /// Ожидается создание предиката ForAllPredicate
        /// </summary>
        [Fact]
        public void Parse_WithUniversalQuantifier_ReturnsCorrectPredicate()
        {
            // Arrange
            var expression = "forall x: x > 0";

            // Act
            var predicateFactory = PredicateParser.Parse(expression);
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2, 3 });
            var predicate = predicateFactory(domain);

            // Assert
            Assert.NotNull(predicate);
            Assert.IsType<ForAllPredicate>(predicate);
        }

        /// <summary>
        /// Проверка парсинга выражения с квантором существования
        /// Ожидается создание предиката ExistsPredicate
        /// </summary>
        [Fact]
        public void Parse_WithExistentialQuantifier_ReturnsCorrectPredicate()
        {
            // Arrange
            var expression = "exists x: x < 0";

            // Act
            var predicateFactory = PredicateParser.Parse(expression);
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2, 3 });
            var predicate = predicateFactory(domain);

            // Assert
            Assert.NotNull(predicate);
            Assert.IsType<ExistsPredicate>(predicate);
        }

        /// <summary>
        /// Проверка парсинга выражения с Unicode символами логических операторов
        /// Ожидается корректная обработка математических символов
        /// </summary>
        [Fact]
        public void Parse_WithUnicodeOperators_ReturnsCorrectPredicate()
        {
            // Arrange
            var expression = "∀ x: x > 0"; // Unicode forall

            // Act
            var predicateFactory = PredicateParser.Parse(expression);
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2, 3 });
            var predicate = predicateFactory(domain);

            // Assert
            Assert.NotNull(predicate);
            Assert.IsType<ForAllPredicate>(predicate);
        }
        #endregion

        #region Отрицательные тесты
        /// <summary>
        /// Проверка валидации парсера при пустой входной строке
        /// Ожидается исключение ArgumentException
        /// </summary>
        [Fact]
        public void Parse_EmptyString_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => PredicateParser.Parse(""));
        }

        /// <summary>
        /// Проверка обработки синтаксически некорректного выражения
        /// Ожидается исключение с описанием ошибки парсинга
        /// </summary>
        [Fact]
        public void Parse_InvalidExpression_ThrowsException()
        {
            // Arrange
            var expression = "x > "; // incomplete expression

            // Act & Assert
            Assert.Throws<Exception>(() => PredicateParser.Parse(expression));
        }

        /// <summary>
        /// Проверка обработки выражения с недопустимыми символами
        /// Ожидается исключение с указанием позиции ошибки
        /// </summary>
        [Fact]
        public void Parse_WithInvalidCharacters_ThrowsException()
        {
            // Arrange
            var expression = "x @ 5"; // @ is not a valid operator

            // Act & Assert
            Assert.Throws<Exception>(() => PredicateParser.Parse(expression));
        }

        /// <summary>
        /// Проверка обработки неверного использования квантора
        /// Ожидается исключение при отсутствии имени переменной после квантора
        /// </summary>
        [Fact]
        public void Parse_QuantifierWithoutVariable_ThrowsException()
        {
            // Arrange
            var expression = "forall : x > 0"; // missing variable name

            // Act & Assert
            Assert.Throws<Exception>(() => PredicateParser.Parse(expression));
        }
        #endregion
    }
    #endregion

    #region ТЕСТЫ КВАНТОРОВ
    /// <summary>
    /// Тестирование кванторных предикатов - операций над множествами значений
    /// </summary>
    public class QuantifierTests
    {
        #region Тесты квантора общности (∀)
        /// <summary>
        /// Проверка квантора общности (∀) при истинности для всех значений домена
        /// Ожидается возврат true
        /// </summary>
        [Fact]
        public void ForAllPredicate_WithAllTrue_ReturnsTrue()
        {
            // Arrange
            var inner = new AtomicPredicate(
                assignment => (int)assignment["x"] > 0,
                "x > 0"
            );
            var forAll = new ForAllPredicate("x", new object[] { 1, 2, 3, 4, 5 }, inner);
            var assignment = new Dictionary<string, object>(); // empty, since variable is bound by quantifier

            // Act
            var result = forAll.Evaluate(assignment);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Проверка квантора общности (∀) при наличии хотя бы одного ложного значения
        /// Ожидается возврат false
        /// </summary>
        [Fact]
        public void ForAllPredicate_WithOneFalse_ReturnsFalse()
        {
            // Arrange
            var inner = new AtomicPredicate(
                assignment => (int)assignment["x"] > 0,
                "x > 0"
            );
            var forAll = new ForAllPredicate("x", new object[] { -1, 2, 3, 4, 5 }, inner); // -1 makes it false
            var assignment = new Dictionary<string, object>();

            // Act
            var result = forAll.Evaluate(assignment);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region Тесты квантора существования (∃)
        /// <summary>
        /// Проверка квантора существования (∃) при наличии хотя бы одного истинного значения
        /// Ожидается возврат true
        /// </summary>
        [Fact]
        public void ExistsPredicate_WithOneTrue_ReturnsTrue()
        {
            // Arrange
            var inner = new AtomicPredicate(
                assignment => (int)assignment["x"] > 4,
                "x > 4"
            );
            var exists = new ExistsPredicate("x", new object[] { 1, 2, 3, 5 }, inner); // 5 makes it true
            var assignment = new Dictionary<string, object>();

            // Act
            var result = exists.Evaluate(assignment);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Проверка квантора существования (∃) при отсутствии истинных значений
        /// Ожидается возврат false
        /// </summary>
        [Fact]
        public void ExistsPredicate_WithAllFalse_ReturnsFalse()
        {
            // Arrange
            var inner = new AtomicPredicate(
                assignment => (int)assignment["x"] > 10,
                "x > 10"
            );
            var exists = new ExistsPredicate("x", new object[] { 1, 2, 3, 4, 5 }, inner); // all less than 10
            var assignment = new Dictionary<string, object>();

            // Act
            var result = exists.Evaluate(assignment);

            // Assert
            Assert.False(result);
        }
        #endregion
    }
    #endregion

    #region ИНТЕГРАЦИОННЫЕ ТЕСТЫ
    /// <summary>
    /// Комплексное тестирование взаимодействия компонентов системы
    /// </summary>
    public class IntegrationTests
    {
        #region Полные сценарии работы
        /// <summary>
        /// Комплексный тест: парсинг выражения -> создание предиката -> анализ типа
        /// Проверка полного цикла обработки логического выражения
        /// </summary>
        [Fact]
        public void FullPipeline_ParseAndAnalyze_WorksCorrectly()
        {
            // Arrange
            var expression = "forall x: x > 0";
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2, 3 });

            // Act
            var predicateFactory = PredicateParser.Parse(expression);
            var predicate = predicateFactory(domain);
            var type = PredicateAnalyzer.DetermineType(predicate, domain);

            // Assert
            Assert.IsType<ForAllPredicate>(predicate);
            Assert.Equal(PredicateType.Tautology, type);
        }

        /// <summary>
        /// Комплексный тест с вычислением области истинности для сложного выражения
        /// Проверка корректности вычислений через всю цепочку компонентов
        /// </summary>
        [Fact]
        public void FullPipeline_WithComplexExpression_ComputesCorrectTruthSet()
        {
            // Arrange
            var expression = "(x > 1) ∧ (x < 4)";
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2, 3, 4, 5 });

            // Act
            var predicateFactory = PredicateParser.Parse(expression);
            var predicate = predicateFactory(domain);
            var truthSet = PredicateAnalyzer.ComputeTruthSet(predicate, domain);

            // Assert
            Assert.Equal(2, truthSet.Count); // Only x=2 and x=3 should satisfy
            Assert.Contains(truthSet, assignment => (int)assignment["x"] == 2);
            Assert.Contains(truthSet, assignment => (int)assignment["x"] == 3);
        }
        #endregion

        #region Сценарии с несколькими переменными
        /// <summary>
        /// Комплексный тест с несколькими переменными и сложным логическим выражением
        /// Проверка корректности работы системы в многомерном случае
        /// </summary>
        [Fact]
        public void FullPipeline_WithMultipleVariables_WorksCorrectly()
        {
            // Arrange
            var expression = "(x > 1) ∧ (y < 5)";
            var domain = new Domain();
            domain.SetDomain("x", new object[] { 1, 2, 3 });
            domain.SetDomain("y", new object[] { 3, 4, 5 });

            // Act
            var predicateFactory = PredicateParser.Parse(expression);
            var predicate = predicateFactory(domain);
            var truthSet = PredicateAnalyzer.ComputeTruthSet(predicate, domain);

            // Assert
            Assert.NotNull(truthSet);
            // Should include combinations where x>1 and y<5
            Assert.Contains(truthSet, assignment =>
                (int)assignment["x"] > 1 && (int)assignment["y"] < 5);
        }
        #endregion
    }
    #endregion
}