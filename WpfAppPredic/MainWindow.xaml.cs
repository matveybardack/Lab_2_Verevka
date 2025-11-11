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
        private string SelectedQuantifier;
        private string? selectedLogicalOperator = null;

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
                PredicateTextBox.Text = string.Join(Environment.NewLine, originalEquationsList);
                EquationTextBox.Text = "";
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

        private void Button_Calculate_Click(object sender, RoutedEventArgs e)
        {
            // Расчёт ;)
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
                    Button_Add_Forall.IsEnabled = enable;
                    Button_Add_Exists.IsEnabled = enable;
                    Button_Logical_And.IsEnabled = enable;
                    Button_Logical_Imp.IsEnabled = enable;
                    Button_Logical_Not.IsEnabled = enable;
                    Button_Logical_Or.IsEnabled = enable;
                    Button_Logical_Equiv.IsEnabled = enable;
                    Button_AddEq.IsEnabled = enable;
                    if (enable) PredicateTextBox.IsEnabled = true;
                    break;
                case LockButtonTypes.Logical:
                    Button_Add_Forall.IsEnabled = enable;
                    Button_Add_Exists.IsEnabled = enable;
                    Button_AddEq.IsEnabled = enable;
                    if (enable) PredicateTextBox.IsEnabled = true;
                    break;
                case LockButtonTypes.Quantifier:
                    Button_Add_Forall.IsEnabled = enable;
                    Button_Add_Exists.IsEnabled = enable;
                    Button_Logical_And.IsEnabled = enable;
                    Button_Logical_Imp.IsEnabled = enable;
                    Button_Logical_Not.IsEnabled = enable;
                    Button_Logical_Or.IsEnabled = enable;
                    Button_Logical_Equiv.IsEnabled = enable;
                    Button_AddEq.IsEnabled = enable;
                    if (enable) PredicateTextBox.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PredicateTextBox.TextChanged += PredicateTextBox_TextChanged;
            UpdateQuantifierButtonsState();
        }

        private void PredicateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEquationsFromPredicate();
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