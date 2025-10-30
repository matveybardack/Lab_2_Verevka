//using ClassLibraryPredic;
//using ClassLibraryPredic.Service;
//using System;
//using System.Collections.Generic;
//using System.Linq;

namespace ConsoleTest
{
    /// <summary>
    /// Консольный тест все библиотеки
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
//            Console.WriteLine("🔬 ТЕСТИРОВАНИЕ БИБЛИОТЕКИ ЛОГИЧЕСКИХ ПРЕДИКАТОВ");
//            Console.WriteLine("=================================================\n");

//            try
//            {
//                TestBasicPredicates();
//                TestLogicalOperations();
//                TestQuantifiers();
//                TestParser();
//                TestAdvancedScenarios();
//                TestErrorHandling();

//                Console.WriteLine("\n🎉 ВСЕ ТЕСТЫ ПРОЙДЕНЫ УСПЕШНО!");
            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"\n❌ ОШИБКА: {ex.Message}");
        //            }
        //        }

        //        static void TestBasicPredicates()
        //        {
        //            Console.WriteLine("1. ТЕСТ БАЗОВЫХ ПРЕДИКАТОВ");
        //            Console.WriteLine("--------------------------");

        //            // Создаем простой предикат вручную
        //            var predicate = new AtomicPredicate(
        //                vars => (int)vars["x"] > 5,
        //                "x > 5"
        //            );

        //            var domain = new Domain();
        //            domain.SetDomain("x", new object[] { 1, 3, 5, 7, 9 });

        //            Console.WriteLine($"Предикат: {predicate.Description}");
        //            Console.WriteLine($"Домен x: {{{string.Join(", ", domain.GetDomain("x"))}}}");

        //            // Тестируем вычисление
        //            var testAssignment = new Dictionary<string, object> { ["x"] = 7 };
        //            bool result = predicate.Evaluate(testAssignment);
        //            Console.WriteLine($"При x=7: {result} (ожидается: True)");

        //            testAssignment["x"] = 3;
        //            result = predicate.Evaluate(testAssignment);
        //            Console.WriteLine($"При x=3: {result} (ожидается: False)");

        //            // Анализ предиката
        //            var type = PredicateAnalyzer.DetermineType(predicate, domain);
        //            var truthSet = PredicateAnalyzer.ComputeTruthSet(predicate, domain);

        //            Console.WriteLine($"Тип предиката: {type}");
        //            Console.WriteLine($"Область истинности: {truthSet.Count} комбинаций");
        //            foreach (var assignment in truthSet)
        //            {
        //                Console.WriteLine($"  x = {assignment["x"]}");
        //            }

        //            Console.WriteLine("✅ Базовые предикаты работают\n");
        //        }

        //        static void TestLogicalOperations()
        //        {
        //            Console.WriteLine("2. ТЕСТ ЛОГИЧЕСКИХ ОПЕРАЦИЙ");
        //            Console.WriteLine("----------------------------");

        //            var domain = new Domain();
        //            domain.SetDomain("x", new object[] { 1, 2, 3, 4, 5 });
        //            domain.SetDomain("y", new object[] { 1, 2, 3 });

        //            // Создаем предикаты
        //            var p1 = new AtomicPredicate(vars => (int)vars["x"] > 2, "x > 2");
        //            var p2 = new AtomicPredicate(vars => (int)vars["y"] < 3, "y < 3");

        //            // Тестируем логические операции
        //            var andPredicate = new AndPredicate(p1, p2);
        //            var orPredicate = new OrPredicate(p1, p2);
        //            var notPredicate = new NotPredicate(p1);
        //            var impPredicate = new ImplicationPredicate(p1, p2);
        //            var eqvPredicate = new EquivalencePredicate(p1, p2);

        //            Console.WriteLine($"AND: {andPredicate.Description}");
        //            Console.WriteLine($"OR: {orPredicate.Description}");
        //            Console.WriteLine($"NOT: {notPredicate.Description}");
        //            Console.WriteLine($"IMPL: {impPredicate.Description}");
        //            Console.WriteLine($"EQV: {eqvPredicate.Description}");

        //            // Тестовое вычисление
        //            var testAssign = new Dictionary<string, object> { ["x"] = 3, ["y"] = 2 };
        //            Console.WriteLine($"\nПри x=3, y=2:");
        //            Console.WriteLine($"  AND: {andPredicate.Evaluate(testAssign)}");
        //            Console.WriteLine($"  OR: {orPredicate.Evaluate(testAssign)}");
        //            Console.WriteLine($"  NOT: {notPredicate.Evaluate(testAssign)}");
        //            Console.WriteLine($"  IMPL: {impPredicate.Evaluate(testAssign)}");
        //            Console.WriteLine($"  EQV: {eqvPredicate.Evaluate(testAssign)}");

        //            // Анализ составного предиката
        //            var complexPredicate = new AndPredicate(
        //                new OrPredicate(p1, p2),
        //                new NotPredicate(new AtomicPredicate(vars => (int)vars["x"] == (int)vars["y"], "x = y"))
        //            );

        //            Console.WriteLine($"\nСложный предикат: {complexPredicate.Description}");
        //            var truthSet = PredicateAnalyzer.ComputeTruthSet(complexPredicate, domain);
        //            Console.WriteLine($"Область истинности: {truthSet.Count} комбинаций");

        //            Console.WriteLine("✅ Логические операции работают\n");
        //        }

        //        static void TestQuantifiers()
        //        {
        //            Console.WriteLine("3. ТЕСТ КВАНТОРОВ");
        //            Console.WriteLine("------------------");

        //            var domain = new Domain();
        //            domain.SetDomain("x", new object[] { 1, 2, 3, 4, 5 });

        //            // Базовый предикат
        //            var innerPredicate = new AtomicPredicate(
        //                vars => (int)vars["x"] > 0,
        //                "x > 0"
        //            );

        //            // Квантор всеобщности
        //            var forAllPredicate = new ForAllPredicate("x", domain.GetDomain("x"), innerPredicate);
        //            Console.WriteLine($"∀x (x > 0): {forAllPredicate.Description}");

        //            var testAssignment = new Dictionary<string, object>();
        //            bool forAllResult = forAllPredicate.Evaluate(testAssignment);
        //            Console.WriteLine($"Результат: {forAllResult} (ожидается: True)");

        //            // Квантор существования с другим предикатом
        //            var existsPredicate = new ExistsPredicate("x", domain.GetDomain("x"),
        //                new AtomicPredicate(vars => (int)vars["x"] > 10, "x > 10"));
        //            Console.WriteLine($"\n∃x (x > 10): {existsPredicate.Description}");

        //            bool existsResult = existsPredicate.Evaluate(testAssignment);
        //            Console.WriteLine($"Результат: {existsResult} (ожидается: False)");

        //            // Проверяем, что исходный assignment не изменился
        //            Console.WriteLine($"Исходный assignment остался пустым: {testAssignment.Count == 0}");

        //            Console.WriteLine("✅ Кванторы работают\n");
        //        }

        //        static void TestParser()
        //        {
        //            Console.WriteLine("4. ТЕСТ ПАРСЕРА");
        //            Console.WriteLine("----------------");

        //            var domain = new Domain();
        //            domain.SetDomain("x", new object[] { 1, 2, 3, 4, 5 });
        //            domain.SetDomain("y", new object[] { 10, 20, 30 });
        //            domain.SetDomain("age", new object[] { 15, 18, 25, 30 });
        //            domain.SetDomain("score", new object[] { 50, 75, 90, 100 });

        //            // Тестовые выражения
        //            var expressions = new[]
        //            {
        //                "x > 3",
        //                "x + y = 25",
        //                "x > 1 ∧ y < 25",
        //                "¬(x = 5) ∨ y = 20",
        //                "age >= 18 ∧ score > 75",
        //                "x * 2 < y",
        //                "forall x: x > 0",
        //                "exists y: y = 20",
        //                "forall x in {1,2,3}: x < 5",
        //                "∃y: y > 15 ∧ y < 25"
        //            };

        //            foreach (var expr in expressions)
        //            {
        //                try
        //                {
        //                    Console.WriteLine($"\nВыражение: {expr}");
        //                    var factory = PredicateParser.Parse(expr);
        //                    var predicate = factory(domain);

        //                    Console.WriteLine($"  Парсинг: ✅ УСПЕХ");
        //                    Console.WriteLine($"  Описание: {predicate.Description}");

        //                    // Тестовое вычисление
        //                    var testAssign = new Dictionary<string, object> { ["x"] = 3, ["y"] = 20, ["age"] = 25, ["score"] = 90 };
        //                    try
        //                    {
        //                        bool result = predicate.Evaluate(testAssign);
        //                        Console.WriteLine($"  Тестовое вычисление: {result}");
        //                    }
        //                    catch (KeyNotFoundException)
        //                    {
        //                        Console.WriteLine($"  Тестовое вычисление: ⚠️ Не все переменные использованы");
        //                    }

        //                    // Анализ
        //                    var type = PredicateAnalyzer.DetermineType(predicate, domain);
        //                    Console.WriteLine($"  Тип предиката: {type}");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"  Парсинг: ❌ ОШИБКА: {ex.Message}");
        //                }
        //            }

        //            Console.WriteLine("\n✅ Парсер работает\n");
        //        }

        //        static void TestAdvancedScenarios()
        //        {
        //            Console.WriteLine("5. ТЕСТ СЛОЖНЫХ СЦЕНАРИЕВ");
        //            Console.WriteLine("--------------------------");

        //            var domain = new Domain();
        //            domain.SetDomain("a", new object[] { 1, 2, 3 });
        //            domain.SetDomain("b", new object[] { 1, 2, 3 });

        //            // Сложное выражение с кванторами
        //            string complexExpr = "forall a: exists b: a + b = 4";
        //            Console.WriteLine($"Выражение: {complexExpr}");

        //            try
        //            {
        //                var factory = PredicateParser.Parse(complexExpr);
        //                var predicate = factory(domain);

        //                Console.WriteLine($"Парсинг: ✅ УСПЕХ");
        //                Console.WriteLine($"Описание: {predicate.Description}");

        //                var result = predicate.Evaluate(new Dictionary<string, object>());
        //                Console.WriteLine($"Результат: {result}");

        //                // Таблица истинности для простого предиката
        //                var simplePredicate = new AtomicPredicate(
        //                    vars => (int)vars["a"] + (int)vars["b"] > 3,
        //                    "a + b > 3"
        //                );

        //                Console.WriteLine($"\nТаблица истинности для 'a + b > 3':");
        //                var truthTable = PredicateAnalyzer.TruthTable(simplePredicate, domain);

        //                foreach (var row in truthTable)
        //                {
        //                    Console.WriteLine($"  a={row.Assignment["a"]}, b={row.Assignment["b"]} -> {row.Value}");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"ОШИБКА: {ex.Message}");
        //            }

        //            Console.WriteLine("✅ Сложные сценарии работают\n");
        //        }

        //        static void TestErrorHandling()
        //        {
        //            Console.WriteLine("6. ТЕСТ ОБРАБОТКИ ОШИБОК");
        //            Console.WriteLine("-------------------------");

        //            var domain = new Domain();
        //            domain.SetDomain("x", new object[] { 1, 2, 3 });

        //            // Тест 1: Несуществующая переменная в assignment
        //            Console.WriteLine("1. Несуществующая переменная:");
        //            var predicate = new AtomicPredicate(vars => (int)vars["y"] > 0, "y > 0");
        //            var testAssign = new Dictionary<string, object> { ["x"] = 1 }; // y отсутствует

        //            try
        //            {
        //                bool result = predicate.Evaluate(testAssign);
        //                Console.WriteLine("   ❌ Ожидалось исключение!");
        //            }
        //            catch (KeyNotFoundException)
        //            {
        //                Console.WriteLine("   ✅ Корректно обработано KeyNotFoundException");
        //            }

        //            // Тест 2: Некорректный синтаксис парсера
        //            Console.WriteLine("\n2. Некорректный синтаксис:");
        //            try
        //            {
        //                var factory = PredicateParser.Parse("x > & 5"); // Некорректный оператор
        //                Console.WriteLine("   ❌ Ожидалось исключение парсера!");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"   ✅ Корректно обработано: {ex.GetType().Name}");
        //            }

        //            // Тест 3: Анализ предиката с отсутствующими переменными
        //            Console.WriteLine("\n3. Анализ предиката с отсутствующими переменными:");
        //            var predWithMissingVars = new AtomicPredicate(
        //                vars => (int)vars["z"] > 0, // z нет в domain
        //                "z > 0"
        //            );

        //            var type = PredicateAnalyzer.DetermineType(predWithMissingVars, domain);
        //            var truthSet = PredicateAnalyzer.ComputeTruthSet(predWithMissingVars, domain);

        //            Console.WriteLine($"   Тип предиката: {type}");
        //            Console.WriteLine($"   Область истинности: {truthSet.Count} комбинаций");

        //            Console.WriteLine("✅ Обработка ошибок работает\n");
        //        }
    }
}