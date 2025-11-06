using ClassLibraryPredic;
using OxyPlot;
using ClassLibraryPredic.Models;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
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
    /// Graphics for panels
    /// </summary>
    public partial class MainWindow : Window
    {
        public PlotModel MyModel { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            // Тест графика
            var factory = PredicateParser.Parse("∀ x : x < 2*y");

            // Создаём область значений
            var domain = new Domain();
            domain.SetDomain("x", Enumerable.Range(0, 10).Cast<object>());
            domain.SetDomain("y", Enumerable.Range(0, 20).Cast<object>());

            // Получаем IPredicate
            var predicate = factory(domain);

            // Создаём PlotModel
            var model = PredicatePlotter.CreatePlotModel(predicate, domain, "y < 2*x && x > 0");

            // Передаём model в OxyPlot WPF PlotView
            TestModel.Model = model;
        }
    }
}
