using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfAppPredic
{
    /// <summary>
    /// Логика взаимодействия для SelectSecondEquationDialog.xaml
    /// </summary>
    public partial class SelectSecondEquationDialog : Window
    {
        public string SelectedEquation { get; private set; }

        public SelectSecondEquationDialog(List<string> equations, string excludedEquation)
        {
            InitializeComponent();

            // Фильтруем уравнения, исключая уже выбранное
            var availableEquations = equations.Where(eq => eq != excludedEquation).ToList();
            EquationsComboBox.ItemsSource = availableEquations;

            if (availableEquations.Count > 0)
            {
                EquationsComboBox.SelectedIndex = 0;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (EquationsComboBox.SelectedItem != null)
            {
                SelectedEquation = EquationsComboBox.SelectedItem.ToString();
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите уравнение");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
