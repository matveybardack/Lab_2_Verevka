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

namespace WpfAppPredic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool AddingEq = false;
        private bool IsQuantifierPanelOpen = false;
        private string SelectedQuantifier;
        private string? selectedLogicalOperator = null;

        private List<string> originalEquationsList = new List<string>();
        private List<string> equationsWithQuantifiersList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        // Обработчик для добавления символов в уравнение
        private void AddSymbolToEquation(string symbol)
        {
            if (EquationTextBox != null)
            {
                EquationTextBox.Text += symbol;
            }
        }

        private void UpdateComboBox()
        {
            EquationsComboBox.ItemsSource = equationsWithQuantifiersList;
            LogicalEquationsComboBox.ItemsSource = originalEquationsList;
        }

        // Обработчик для добавления символов в уравнение
        private void AddSymbolToPredicate(string symbol)
        {
            if (PredicateTextBox != null)
            {
                PredicateTextBox.Text += symbol;
            }
        }

        // Обработчик для добавления уравнения в предикат
        private void AddEquationToPredicate()
        {
            string equation = EquationTextBox.Text;
            if (!string.IsNullOrEmpty(equation))
            {
                List<string> equationsList = new List<string>(EquationsComboBox.Items.Cast<string>());
                equationsList.Add(equation);
                equationsWithQuantifiersList = equationsList;
                originalEquationsList = equationsList;
                UpdateComboBox();
                PredicateTextBox.Text = string.Join(Environment.NewLine, equationsList);
                EquationTextBox.Text = "";
            }
        }

        private void AddLogicalOperatorButton_Click(object sender, RoutedEventArgs e)
        {

            bool operatorIsNot = false;

            if (LogicalEquationsComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите уравнение!");
                return;
            }

            string logicalOperator;
            string selectedEquation = LogicalEquationsComboBox.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(selectedEquation))
            {
                logicalOperator = selectedLogicalOperator;
                if (logicalOperator == "!") operatorIsNot = true;
            }
            else
            {
                MessageBox.Show("Не выбрано ни одного логического оператора");
                return;
            }

            string newEquation;
            // Формируем новое уравнение с логическим оператором
            if (operatorIsNot)
            {
                newEquation = $"{logicalOperator} {selectedEquation}";
            } else
            {
                newEquation = $"{selectedEquation} {logicalOperator}";
            }

                // Обновляем список уравнений с кванторами
                int index = equationsWithQuantifiersList.IndexOf(selectedEquation);
            if (index != -1)
            {
                equationsWithQuantifiersList[index] = newEquation;
                EquationsComboBox.ItemsSource = null;
                EquationsComboBox.ItemsSource = equationsWithQuantifiersList;
            }

            // Обновляем текстовое поле
            PredicateTextBox.Text = string.Join(Environment.NewLine, equationsWithQuantifiersList);

            EnableLogicalButtons(true);
            // Закрываем панель с логическими операторами
            AnimationClosePanel(LogicalOperatorsGrid);
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

            // Обновляем список уравнений в ComboBox
            List<string> equationsList = new List<string>(EquationsComboBox.Items.Cast<string>());
            equationsList[EquationsComboBox.SelectedIndex] = newEquation;
            EquationsComboBox.ItemsSource = null;
            EquationsComboBox.ItemsSource = equationsList;

            // Обновляем текстовое поле
            PredicateTextBox.Text = string.Join(Environment.NewLine, equationsList);

            EnableQuantifierButtons(true);
            // Закрываем панель с разблокировкой кнопок
            AnimationClosePanel(QuantifierGrid);
        }

        // Обработчики для математических операторов
        private void Button_Add_Plus_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("+");
        private void Button_Add_Minus_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("-");
        private void Button_Add_Multi_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("*");
        private void Button_Add_Div_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("/");
        private void Button_Add_IntDiv_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("//");
        private void Button_Add_Deg_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("^2");
        private void Button_Add_Mod_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("%");
        private void Button_Add_Abs_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("ABS(");

        // Обработчики для переменных
        private void Button_Add_X_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("x");
        private void Button_Add_Y_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("y");

        // Обработчики для логических операторов
        private void Button_Add_Not_Click(object sender, RoutedEventArgs e)
        {
            AnimationOpenPanel(LogicalOperatorsGrid);
            selectedLogicalOperator = "!";
            EnableLogicalButtons(false);
        }
        private void Button_Add_And_Click(object sender, RoutedEventArgs e)
        {
            AnimationOpenPanel(LogicalOperatorsGrid);
            selectedLogicalOperator = "&&";
            EnableLogicalButtons(false);
        }
        private void Button_Add_Or_Click(object sender, RoutedEventArgs e)
        {
            AnimationOpenPanel(LogicalOperatorsGrid);
            selectedLogicalOperator = "||";
            EnableLogicalButtons(false);
        }
        private void Button_Add_Imp_Click(object sender, RoutedEventArgs e)
        {
            AnimationOpenPanel(LogicalOperatorsGrid);
            selectedLogicalOperator = "->";
            EnableLogicalButtons(false);
        }
        private void Button_Add_Equiv_Click(object sender, RoutedEventArgs e)
        {
            AnimationOpenPanel(LogicalOperatorsGrid);
            selectedLogicalOperator = "<->";
            EnableLogicalButtons(false);
        }

        // Обработчики для операторов сравнения
        private void Button_Add_LowerThan_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("<");
        private void Button_Add_LowerOrEqual_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("<=");
        private void Button_Add_Equal_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("==");
        private void Button_Add_BiggerOrEqual_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation(">=");
        private void Button_Add_Bigger_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation(">");

        // Обработчики для кванторов
        private void Button_Add_Forall_Click(object sender, RoutedEventArgs e)
        {
            if (!IsQuantifierPanelOpen)
            {
                SelectedQuantifier = "∀";
                EnableQuantifierButtons(false);
                AnimationOpenPanel(QuantifierGrid);
            }
        }

        private void Button_Add_Exists_Click(object sender, RoutedEventArgs e)
        {
            if (!IsQuantifierPanelOpen)
            {
                SelectedQuantifier = "∃";
                EnableQuantifierButtons(false);
                AnimationOpenPanel(QuantifierGrid);
            }
        }

        private void EnableQuantifierButtons(bool enable)
        {
            Button_Add_Forall.IsEnabled = enable;
            Button_Add_Exists.IsEnabled = enable;
        }

        private void EnableAddEqButton(bool enable)
        {
            Button_AddEq.IsEnabled = enable;
        }

        private void EnableLogicalButtons(bool enable)
        {
            Button_Logical_And.IsEnabled = enable;
            Button_Logical_Imp.IsEnabled = enable;
            Button_Logical_Not.IsEnabled = enable;
            Button_Logical_Or.IsEnabled = enable;
            Button_Logical_Equiv.IsEnabled = enable;
        }

        // Модифицируем обработчик кнопки "Добавить"
        private void Button_Add_Eq_Click(object sender, RoutedEventArgs e)
        {
            // Добавить проверку корректности
            EnableAddEqButton(true);
            AddEquationToPredicate();
            AnimationClosePanel(GridAddEq);
        }

        private void AnimationOpenPanel(Grid grid1)
        {
            grid1.Visibility = Visibility.Visible;
            grid1.UpdateLayout();
            ParentGrid.UpdateLayout();

            double containerHeight = grid1.ActualHeight;
            var heightAnim = new DoubleAnimation
            {
                From = 0,
                To = containerHeight, // 100% от размера родительского контейнера
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var opacityAnim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            
            grid1.Height = 0; // Начальная высота
            grid1.Opacity = 0; // Начальная прозрачность
            
            grid1.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
            grid1.BeginAnimation(Grid.HeightProperty, heightAnim);
        }

        private void AnimationClosePanel(Grid grid1)
        {
            double containerHeight = grid1.ActualHeight;
            var heightAnim = new DoubleAnimation
            {
                From = containerHeight,
                To = 0, // 100% от размера родительского контейнера
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var opacityAnim = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5)
            };

            grid1.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
            grid1.BeginAnimation(Grid.HeightProperty, heightAnim);

            heightAnim.Completed += (s, ev) =>
            {
                grid1.Visibility = Visibility.Collapsed;
                EnableQuantifierButtons(true); // Разблокируем кнопки кванторов
            };
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
            } else {
                AddingEq = true;
                EnableAddEqButton(false);
                AnimationOpenPanel(GridAddEq);
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