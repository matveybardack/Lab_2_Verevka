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
    /// Animation for panels
    /// </summary>
    public partial class MainWindow : Window
    {
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
    }
}
