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

        const float StepSize = 0.5f;
        IEnumerable<object> CreateDoubleSequence(float start, float end, float step)
        {
            for (float current = start; current <= end; current += step)
            {
                yield return current; // double неявно преобразуется в object
            }
        }

        public PlotModel MyModel { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void ShowGraph()
        {
            // Тест графика
            var factory = PredicateParser.Parse(PredicateTextBox.Text);

            //var x = Enumerable<double>.Range(0, 100);

            // Создаём область значений
            var domain = new Domain();
            //Enumerable.Range((int)XMin, (int)XMax)
            domain.SetDomain("x", CreateDoubleSequence((float)XMin, (float)XMax, StepSize).Cast<object>());
            domain.SetDomain("y", CreateDoubleSequence((float)YMin, (float)YMax, StepSize).Cast<object>());

            // Получаем IPredicate
            var predicate = factory(domain);

            // Создаём PlotModel
            var model = PredicatePlotter.CreatePlotModel(predicate, domain, PredicateTextBox.Text);

            // Передаём model в OxyPlot WPF PlotView
            TestModel.Model = model;
        }
    }
}
