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
    /// Панель добавления уравнения
    /// </summary>
    public partial class MainWindow : Window
    {
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

        // Обработчики для операторов сравнения
        private void Button_Add_LowerThan_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("<");
        private void Button_Add_LowerOrEqual_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("<=");
        private void Button_Add_Equal_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation("==");
        private void Button_Add_BiggerOrEqual_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation(">=");
        private void Button_Add_Bigger_Click(object sender, RoutedEventArgs e) => AddSymbolToEquation(">");

        // Обработчик для добавления символов в уравнение
        private void AddSymbolToEquation(string symbol)
        {
            if (EquationTextBox != null)
            {
                EquationTextBox.Text += symbol;
            }
        }
    }
}
