/// <summary>
/// Настройка приложения: улучшение качества рендеринга для высоких DPI
/// </summary>
using System.Windows;
using System.Windows.Media;

namespace EngineeringTargets
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;
            
            TextOptions.TextFormattingModeProperty.OverrideMetadata(
                typeof(Window),
                new System.Windows.FrameworkPropertyMetadata(
                    System.Windows.Media.TextFormattingMode.Display));
            
            TextOptions.TextRenderingModeProperty.OverrideMetadata(
                typeof(Window),
                new System.Windows.FrameworkPropertyMetadata(
                    System.Windows.Media.TextRenderingMode.ClearType));
            
            base.OnStartup(e);
        }
    }
}

