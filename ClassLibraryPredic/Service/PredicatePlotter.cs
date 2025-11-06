using ClassLibraryPredic;
using ClassLibraryPredic.Interface;
using ClassLibraryPredic.Models;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;

public static class PredicatePlotter
{
    /// <summary>
    /// Создаёт PlotModel для области истинности предиката (максимум 2 переменные).
    /// Каждая точка, где предикат истиннен, отображается на графике.
    /// </summary>
    public static PlotModel CreatePlotModel(IPredicate predicate, Domain domain, string title = "Область истинности предиката")
    {
        var vars = domain.Variables.ToArray();
        if (vars.Length != 2)
            throw new System.ArgumentException("Метод поддерживает ровно 2 переменные для графика.");

        string xVar = vars[0];
        string yVar = vars[1];

        // Вычисляем все комбинации, где предикат истиннен
        var truthAssignments = PredicateAnalyzer.ComputeTruthSet(predicate, domain);

        // Создаём PlotModel
        var plotModel = new PlotModel { Title = title };

        var scatterSeries = new ScatterSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 4,
            MarkerFill = OxyColors.SkyBlue
        };

        // Добавляем точки
        foreach (var assignment in truthAssignments)
        {
            double x = Convert.ToDouble(assignment[xVar]);
            double y = Convert.ToDouble(assignment[yVar]);
            scatterSeries.Points.Add(new ScatterPoint(x, y));
        }

        plotModel.Series.Add(scatterSeries);

        // Настройка осей
        plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis
        {
            Position = OxyPlot.Axes.AxisPosition.Bottom,
            Title = xVar
        });
        plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis
        {
            Position = OxyPlot.Axes.AxisPosition.Left,
            Title = yVar
        });

        return plotModel;
    }
}
