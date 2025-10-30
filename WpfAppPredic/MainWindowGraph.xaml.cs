using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
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

            //Тестовый график
            this.MyModel = new PlotModel { Title = "Example 1" };

            // --- Граница области ---
            var line = new FunctionSeries(x => 2 * x, -5, 5, 0.1, "y = 2x");
            MyModel.Series.Add(line);

            var region = new PolygonAnnotation
            {
                Fill = OxyColor.FromAColor(120, OxyColors.SkyBlue), // прозрачный цвет
                StrokeThickness = 0
            };

            // Определяем многоугольник под прямой
            double xMin = -5;
            double xMax = 5;
            double yMin = -10; // нижняя граница области
            double yOnMinX = 2 * xMin;
            double yOnMaxX = 2 * xMax;

            region.Points.Add(new DataPoint(xMin, yMin));   // нижний левый
            region.Points.Add(new DataPoint(xMax, yMin));   // нижний правый
            region.Points.Add(new DataPoint(xMax, yOnMaxX)); // точка на прямой
            region.Points.Add(new DataPoint(xMin, yOnMinX)); // точка на прямой

            MyModel.Annotations.Add(region);

            // --- Настройка осей ---
            MyModel.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Title = "X",
                Minimum = -5,
                Maximum = 5
            });

            MyModel.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                Title = "Y",
                Minimum = -10,
                Maximum = 10
            });

            //this.MyModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));
            TestModel.Model = MyModel;
        }
    }
}
