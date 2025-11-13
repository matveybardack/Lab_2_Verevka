using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WpfAppPredic.EquationParser;

namespace WpfAppPredic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    enum LockButtonTypes
    {
        None,
        Quantifier,
        Logical,
        Equation
    }
    public partial class MainWindow : Window
    {
        private bool AddingEq = false;
        private bool IsQuantifierPanelOpen = false;
        private string? SelectedQuantifier;
        private string? selectedLogicalOperator = null;
        // Свойства для хранения ограничений
        public double XMin { get; set; } = -10.0;
        public double XMax { get; set; } = 10.0;
        public double YMin { get; set; } = -10.0;
        public double YMax { get; set; } = 10.0;

        private List<string> originalEquationsList = new List<string>();
        private List<string> equationsWithQuantifiersList = new List<string>();

        private void UpdateEquationsFromPredicate()
        {
            var equations = EquationParser.ParseEquations(PredicateTextBox.Text);
            originalEquationsList = equations;
            equationsWithQuantifiersList = new List<string>(equations);
            UpdateComboBox();
        }

        private void UpdateComboBox()
        {
            // Для LogicalEquationsComboBox показываем базовые уравнения (без кванторов)
            var baseEquations = originalEquationsList.Select(EquationParser.ExtractBaseEquation).ToList();
            LogicalEquationsComboBox.ItemsSource = baseEquations;

            // Для EquationsComboBox показываем уравнения с кванторами
            EquationsComboBox.ItemsSource = equationsWithQuantifiersList;

            UpdateQuantifierButtonsState();
        }

        // Обработчик для добавления уравнения в предикат
        private void AddEquationToPredicate()
        {
            string equation = EquationTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(equation))
            {
                // Добавляем как одно уравнение
                originalEquationsList.Add(equation);
                equationsWithQuantifiersList.Add(equation);

                UpdateComboBox();

                // Собираем все уравнения и добавляем ограничения области
                string predicate = string.Join(Environment.NewLine, originalEquationsList);
                //predicate += GetRangeConstraintsString(); // Добавляем ограничения

                PredicateTextBox.Text = predicate;
                EquationTextBox.Text = "";

                // Закрываем панель после успешного добавления
                AddingEq = false;
                EnableAllButtons(true, LockButtonTypes.Equation);
                AnimationClosePanel(GridAddEq);
            }
        }

        private void AddLogicalOperatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (LogicalEquationsComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите уравнение!");
                return;
            }

            string? selectedBaseEquation = LogicalEquationsComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedBaseEquation))
            {
                MessageBox.Show("Ошибка: выбранное уравнение пустое");
                return;
            }

            string? logicalOperator = selectedLogicalOperator;
            if (string.IsNullOrEmpty(logicalOperator))
            {
                MessageBox.Show("Не выбрано ни одного логического оператора");
                return;
            }

            // Нахождение полного уравнения по базовому
            string? selectedEquation = originalEquationsList.FirstOrDefault(eq =>
        EquationParser.ExtractBaseEquation(eq) == selectedBaseEquation);

            if (string.IsNullOrEmpty(selectedEquation))
            {
                MessageBox.Show("Ошибка при поиске уравнения");
                return;
            }

            string newEquation;

            // Обработка унарного оператора NOT
            if (logicalOperator == "!")
            {
                newEquation = $"{logicalOperator}({selectedEquation})";

                // Обновление списков - заменяем только выбранное уравнение
                int index1 = originalEquationsList.IndexOf(selectedEquation);
                if (index1 != -1)
                {
                    originalEquationsList[index1] = newEquation;
                    equationsWithQuantifiersList[index1] = newEquation;
                }

                UpdateComboBox();
                PredicateTextBox.Text = string.Join(Environment.NewLine, originalEquationsList);

                EnableAllButtons(true, LockButtonTypes.Logical);
                AnimationClosePanel(LogicalOperatorsGrid);
            }
            else
            {
                // Для бинарных операторов нужно второе уравнение
                if (LogicalEquationsComboBox.Items.Count < 2)
                {
                    MessageBox.Show("Для бинарного оператора нужно как минимум два уравнения");

                    // Даем пользователю возможность прервать операцию
                    var result = MessageBox.Show("Недостаточно уравнений для бинарного оператора. Хотите выбрать другой оператор?",
                                               "Недостаточно уравнений",
                                               MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Оставляем панель открытой для выбора другого оператора
                        return;
                    }
                    else
                    {
                        // Закрываем панель
                        EnableAllButtons(true, LockButtonTypes.Logical);
                        AnimationClosePanel(LogicalOperatorsGrid);
                        return;
                    }
                }

                // Создаем диалог для выбора второго уравнения
                var selectDialog = new SelectSecondEquationDialog(LogicalEquationsComboBox.Items.Cast<string>().ToList(), selectedBaseEquation);
                if (selectDialog.ShowDialog() == true)
                {
                    string secondBaseEquation = selectDialog.SelectedEquation;
                    string? secondEquation = originalEquationsList.FirstOrDefault(eq =>
                        EquationParser.ExtractBaseEquation(eq) == secondBaseEquation);

                    if (!string.IsNullOrEmpty(secondEquation))
                    {
                        newEquation = $"({selectedEquation}) {logicalOperator} ({secondEquation})";

                        // НАЙДЕМ ИНДЕКСЫ ОБОИХ УРАВНЕНИЙ
                        int firstIndex = originalEquationsList.IndexOf(selectedEquation);
                        int secondIndex = originalEquationsList.IndexOf(secondEquation);

                        if (firstIndex != -1 && secondIndex != -1)
                        {
                            // УДАЛЯЕМ ВТОРОЕ УРАВНЕНИЕ ИЗ ОБОИХ СПИСКОВ
                            originalEquationsList.RemoveAt(secondIndex);
                            equationsWithQuantifiersList.RemoveAt(secondIndex);

                            // ЗАМЕНЯЕМ ПЕРВОЕ УРАВНЕНИЕ НА НОВОЕ ОБЪЕДИНЕННОЕ
                            originalEquationsList[firstIndex] = newEquation;
                            equationsWithQuantifiersList[firstIndex] = newEquation;

                            UpdateComboBox();
                            PredicateTextBox.Text = string.Join(Environment.NewLine, originalEquationsList);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при поиске уравнений в списке");
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при поиске второго уравнения");
                        return;
                    }
                }
                else
                {
                    // Пользователь отменил выбор
                    return;
                }

                EnableAllButtons(true, LockButtonTypes.Logical);
                AnimationClosePanel(LogicalOperatorsGrid);
            }

            //// Обновление списков
            //int index = originalEquationsList.IndexOf(selectedEquation);
            //if (index != -1)
            //{
            //    originalEquationsList[index] = newEquation;
            //    equationsWithQuantifiersList[index] = newEquation;
            //}

            //UpdateComboBox();
            //PredicateTextBox.Text = string.Join(Environment.NewLine, originalEquationsList);

            //EnableAllButtons(true, LockButtonTypes.Logical);
            //AnimationClosePanel(LogicalOperatorsGrid);
            //PredicateTextBox.IsEnabled = true;
        }

        // Обработчики отмены операций
        private void CancelEquation_Click(object sender, RoutedEventArgs e)
        {
            AddingEq = false;
            EnableMainButtons(true);
            //EnableAllButtons(true, LockButtonTypes.Equation);
            AnimationClosePanel(GridAddEq);
            PredicateTextBox.IsEnabled = true;
            EquationTextBox.Text = ""; // Очищаем поле ввода

            UpdateQuantifierButtonsState();
        }

        private void CancelLogical_Click(object sender, RoutedEventArgs e)
        {
            EnableMainButtons(true);
            //EnableAllButtons(true, LockButtonTypes.Logical);
            AnimationClosePanel(LogicalOperatorsGrid);
            PredicateTextBox.IsEnabled = true;
            selectedLogicalOperator = null; // Сбрасываем выбранный оператор

            UpdateQuantifierButtonsState();
        }

        private void CancelQuantifier_Click(object sender, RoutedEventArgs e)
        {
            EnableMainButtons(true);
            //EnableAllButtons(true, LockButtonTypes.Quantifier);
            AnimationClosePanel(QuantifierGrid);
            PredicateTextBox.IsEnabled = true;
            IsQuantifierPanelOpen = false;
            SelectedQuantifier = null; // Сбрасываем выбранный квантор

            UpdateQuantifierButtonsState();
        }

        private void EnableMainButtons(bool enable)
        {
            Button_Logical_And.IsEnabled = enable;
            Button_Logical_Imp.IsEnabled = enable;
            Button_Logical_Not.IsEnabled = enable;
            Button_Logical_Or.IsEnabled = enable;
            Button_Logical_Equiv.IsEnabled = enable;
            Button_AddEq.IsEnabled = enable;

            // Кванторы НЕ включаем здесь - их состояние определяется отдельно
            // Button_Add_Forall.IsEnabled = enable;
            // Button_Add_Exists.IsEnabled = enable;

            if (enable) PredicateTextBox.IsEnabled = true;
        }

        private void AddQuantifier(object sender, RoutedEventArgs e)
        {
            string variable = RadioButtonX.IsChecked == true ? "x" : "y";
            string quantifierExpression = $"{SelectedQuantifier}{variable}";

            if (EquationsComboBox.Items.Count == 0)
            {
                // Если уравнений нет, просто добавляем квантор в предикат
                PredicateTextBox.Text += quantifierExpression;
            }
            else
            {
                int selectedIndex = EquationsComboBox.SelectedIndex;
                if (selectedIndex == -1)
                {
                    // Если ничего не выбрано, добавляем в конец
                    PredicateTextBox.Text += Environment.NewLine + quantifierExpression;
                }
                else
                {
                    // Добавляем квантор перед выбранным уравнением
                    string[] equations = PredicateTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    equations[selectedIndex] = quantifierExpression + " " + equations[selectedIndex];
                    PredicateTextBox.Text = string.Join(Environment.NewLine, equations);
                }
            }
            AnimationClosePanel(QuantifierGrid);
        }

        private void AddQuantifierButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем выбор переменной
            if (!RadioButtonX.IsChecked.HasValue && !RadioButtonY.IsChecked.HasValue)
            {
                MessageBox.Show("Выберите переменную!");
                return;
            }

            // Проверяем выбор уравнения
            if (EquationsComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите уравнение!");
                return;
            }

            string variable = RadioButtonX.IsChecked.GetValueOrDefault() ? "x" : "y";
            string selectedEquation = EquationsComboBox.SelectedItem.ToString();

            // Формируем новое уравнение с квантором
            string newEquation = $"{SelectedQuantifier}{variable} ({selectedEquation})";

            // Обновляем список уравнений с кванторами
            int index = equationsWithQuantifiersList.IndexOf(selectedEquation);
            if (index != -1)
            {
                equationsWithQuantifiersList[index] = newEquation;

                // Также обновляем оригинальный список, если нужно
                if (originalEquationsList.Count > index)
                {
                    originalEquationsList[index] = newEquation;
                }
            }

            UpdateComboBox();
            PredicateTextBox.Text = string.Join(Environment.NewLine, equationsWithQuantifiersList);

            EnableAllButtons(true, LockButtonTypes.Quantifier);
            AnimationClosePanel(QuantifierGrid);
            PredicateTextBox.IsEnabled = true; // Разблокируем поле
            IsQuantifierPanelOpen = false;
        }

        /// <summary>
        /// Проверка на наличие уравнений в строке предиката
        /// </summary>
        private void UpdateQuantifierButtonsState()
        {
            bool hasEquations = originalEquationsList != null && originalEquationsList.Count > 0;
            Button_Add_Forall.IsEnabled = hasEquations;
            Button_Add_Exists.IsEnabled = hasEquations;
        }

        // Обработчики для логических операторов
        private void Button_Add_Not_Click(object sender, RoutedEventArgs e)
        {
            AnimationOpenPanel(LogicalOperatorsGrid);
            selectedLogicalOperator = "!";
            EnableAllButtons(false, LockButtonTypes.Logical);
            PredicateTextBox.IsEnabled = false;
        }
        private void Button_Add_And_Click(object sender, RoutedEventArgs e)
        {
            AnimationOpenPanel(LogicalOperatorsGrid);
            selectedLogicalOperator = "&&";
            EnableAllButtons(false, LockButtonTypes.Logical);
            PredicateTextBox.IsEnabled = false;
        }
        private void Button_Add_Or_Click(object sender, RoutedEventArgs e)
        {
            AnimationOpenPanel(LogicalOperatorsGrid);
            selectedLogicalOperator = "||";
            EnableAllButtons(false, LockButtonTypes.Logical);
            PredicateTextBox.IsEnabled = false;
        }
        private void Button_Add_Imp_Click(object sender, RoutedEventArgs e)
        {
            AnimationOpenPanel(LogicalOperatorsGrid);
            selectedLogicalOperator = "->";
            EnableAllButtons(false, LockButtonTypes.Logical);
            PredicateTextBox.IsEnabled = false;
        }
        private void Button_Add_Equiv_Click(object sender, RoutedEventArgs e)
        {
            AnimationOpenPanel(LogicalOperatorsGrid);
            selectedLogicalOperator = "<->";
            EnableAllButtons(false, LockButtonTypes.Logical);
            PredicateTextBox.IsEnabled = false;
        }


        // Обработчики для кванторов
        private void Button_Add_Forall_Click(object sender, RoutedEventArgs e)
        {
            if (!IsQuantifierPanelOpen)
            {
                SelectedQuantifier = "∀";
                EnableAllButtons(false, LockButtonTypes.Equation);
                AnimationOpenPanel(QuantifierGrid);
                PredicateTextBox.IsEnabled = false;
                IsQuantifierPanelOpen = true;
            }
        }

        private void Button_Add_Exists_Click(object sender, RoutedEventArgs e)
        {
            if (!IsQuantifierPanelOpen)
            {
                SelectedQuantifier = "∃";
                EnableAllButtons(false, LockButtonTypes.Equation);
                AnimationOpenPanel(QuantifierGrid);
                PredicateTextBox.IsEnabled = false; // Блокируем поле
                IsQuantifierPanelOpen = true;
            }
        }


        // Модифицируем обработчик кнопки "Добавить"
        private void Button_Add_Eq_Click(object sender, RoutedEventArgs e)
        {
            // Добавить проверку корректности
            EnableAllButtons(true, LockButtonTypes.Equation);
            AddEquationToPredicate();
            AnimationClosePanel(GridAddEq);
            PredicateTextBox.IsEnabled = true;
        }

        private void EquationsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Здесь можно добавить дополнительную логику, если необходимо
        }


        private void Button_AddEq_Click(object sender, RoutedEventArgs e)
        {
            if (AddingEq) {
                AddingEq = false;
                AnimationClosePanel(GridAddEq);
                PredicateTextBox.IsEnabled = true;
            } else {
                AddingEq = true;
                EnableAllButtons(false, LockButtonTypes.Equation);
                AnimationOpenPanel(GridAddEq);
                PredicateTextBox.IsEnabled = false;
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow();
            helpWindow.Owner = this;
            helpWindow.ShowDialog();
        }

        private void EnableAllButtons(bool enable, LockButtonTypes buttonType)
        {
            switch (buttonType)
            {
                case LockButtonTypes.Equation:
                    EnableMainButtons(enable);
                    // Кванторы включаем только если enable=true И есть уравнения
                    if (enable)
                    {
                        UpdateQuantifierButtonsState(); // Проверяем состояние кванторов
                    }
                    else
                    {
                        Button_Add_Forall.IsEnabled = false;
                        Button_Add_Exists.IsEnabled = false;
                    }
                    break;
                case LockButtonTypes.Logical:
                    Button_Add_Forall.IsEnabled = enable;
                    Button_Add_Exists.IsEnabled = enable;
                    Button_AddEq.IsEnabled = enable;
                    // Логические кнопки остаются активными для возможности выбора другого оператора
                    break;
                case LockButtonTypes.Quantifier:
                    EnableMainButtons(enable);
                    // Кванторы включаем только если enable=true И есть уравнения
                    if (enable)
                    {
                        UpdateQuantifierButtonsState(); // Проверяем состояние кванторов
                    }
                    else
                    {
                        Button_Add_Forall.IsEnabled = false;
                        Button_Add_Exists.IsEnabled = false;
                    }
                    break;
                default:
                    break;
            }

            // Всегда разблокируем поле предиката при enable
            if (enable)
            {
                PredicateTextBox.IsEnabled = true;
            }
        }

        // Обновим метод для кнопки "Рассчитать"
        private void Button_Calculate_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, есть ли уравнения для расчета
            if (string.IsNullOrWhiteSpace(PredicateTextBox.Text) ||
                (!PredicateTextBox.Text.Contains("x") && !PredicateTextBox.Text.Contains("y")))
            {
                MessageBox.Show("Добавьте уравнения с переменными x и y перед расчетом.",
                               "Нет уравнений",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // Получаем текущий предикат (без предыдущих ограничений если они есть)
            string predicate = PredicateTextBox.Text.Trim();

            // Удаляем предыдущие ограничения если они есть
            predicate = RemoveExistingRangeConstraints(predicate);

            // Добавляем новые ограничения только при нажатии кнопки
            string rangeConstraints = GetRangeConstraintsString();
            string finalPredicate = predicate + rangeConstraints;

            // Обновляем текстовое поле
            PredicateTextBox.Text = finalPredicate;

            // Здесь будет логика построения графика
            // CalculateAndPlot(); - этот метод вы реализуете позже

            MessageBox.Show($"Предикат с ограничениями области:\n{finalPredicate}\n\n" +
                           $"Область построения:\n" +
                           $"X: от {XMin:F1} до {XMax:F1}\n" +
                           $"Y: от {YMin:F1} до {YMax:F1}",
                           "Готово к расчету",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
        }

        // Метод для удаления существующих ограничений из предиката
        private string RemoveExistingRangeConstraints(string predicate)
        {
            if (predicate.Contains(" x in [") && predicate.Contains("]; y in ["))
            {
                int rangeIndex = predicate.IndexOf(" x in [");
                if (rangeIndex > 0)
                {
                    return predicate.Substring(0, rangeIndex).Trim();
                }
            }
            return predicate;
        }

        // Обновленный обработчик изменения ползунков
        private void RangeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (XMinSlider != null && XMaxSlider != null && YMinSlider != null && YMaxSlider != null)
            {
                // Обновляем значения
                XMin = XMinSlider.Value;
                XMax = XMaxSlider.Value;
                YMin = YMinSlider.Value;
                YMax = YMaxSlider.Value;

                // Проверяем корректность диапазонов
                ValidateRanges();

                // Обновляем текстовую информацию
                UpdateRangeInfoText();
            }
        }

        // Метод проверки корректности диапазонов
        private void ValidateRanges()
        {
            // Гарантируем, что min <= max
            if (XMin > XMax)
            {
                XMin = XMax;
                XMinSlider.Value = XMax;
            }

            if (YMin > YMax)
            {
                YMin = YMax;
                YMinSlider.Value = YMax;
            }
        }

        // Метод для получения строки с ограничениями области
        private string GetRangeConstraintsString()
        {
            return $" x in [{XMin:F1}, {XMax:F1}]; y in [{YMin:F1}, {YMax:F1}]";
        }

        // Метод для получения кортежа с ограничениями (удобно для построения графика)
        public (double xMin, double xMax, double yMin, double yMax) GetPlotRange()
        {
            return (XMin, XMax, YMin, YMax);
        }

        // Метод обновления текстовой информации
        private void UpdateRangeInfoText()
        {
            if (RangeInfoText != null)
            {
                RangeInfoText.Content = $"Область построения: X ∈ [{XMin:F1}, {XMax:F1}], Y ∈ [{YMin:F1}, {YMax:F1}]";
            }
        }

        private void RangeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (double.TryParse(textBox.Text, out double value))
                {
                    // Ограничиваем значение в допустимых пределах
                    value = Math.Max(-20, Math.Min(20, value));

                    // Обновляем соответствующий слайдер
                    switch (textBox.Name)
                    {
                        case "XMinTextBox":
                            XMinSlider.Value = value;
                            break;
                        case "XMaxTextBox":
                            XMaxSlider.Value = value;
                            break;
                        case "YMinTextBox":
                            YMinSlider.Value = value;
                            break;
                        case "YMaxTextBox":
                            YMaxSlider.Value = value;
                            break;
                    }
                }
                else
                {
                    // Восстанавливаем предыдущее значение при ошибке
                    switch (textBox.Name)
                    {
                        case "XMinTextBox":
                            textBox.Text = XMinSlider.Value.ToString("F1");
                            break;
                        case "XMaxTextBox":
                            textBox.Text = XMaxSlider.Value.ToString("F1");
                            break;
                        case "YMinTextBox":
                            textBox.Text = YMinSlider.Value.ToString("F1");
                            break;
                        case "YMaxTextBox":
                            textBox.Text = YMaxSlider.Value.ToString("F1");
                            break;
                    }
                }

                ValidateRanges();
                UpdateRangeInfoText();
            }
        }

        // Обработчики для текстовых полей
        private void InitializeRangeControls()
        {
            XMinTextBox.LostFocus += RangeTextBox_LostFocus;
            XMaxTextBox.LostFocus += RangeTextBox_LostFocus;
            YMinTextBox.LostFocus += RangeTextBox_LostFocus;
            YMaxTextBox.LostFocus += RangeTextBox_LostFocus;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PredicateTextBox.TextChanged += PredicateTextBox_TextChanged;
            UpdateQuantifierButtonsState();
            InitializeRangeControls();
            
        }

        private void PredicateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEquationsFromPredicate();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PredicateTextBox.Text != String.Empty && Graph.IsSelected == true) { 
                try
                {
                    ShowGraph();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Введен неверный предикат");
                }
            }

        }
        //private void HelpButton_Click(object sender, RoutedEventArgs e)
        //{
        //    HelpPopup.IsOpen = !HelpPopup.IsOpen;
        //}

        //private void CloseHelp_Click(object sender, RoutedEventArgs e)
        //{
        //    HelpPopup.IsOpen = false;
        //}
    }
}