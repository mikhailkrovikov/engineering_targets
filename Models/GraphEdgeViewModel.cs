/// <summary>
/// Модель связи графа для визуализации стрелки между целями
/// </summary>
using System.Windows;
using System.Windows.Media;

namespace EngineeringTargets.Models
{
    public class GraphEdgeViewModel
    {
        public string FromGoalCode { get; set; } = string.Empty;
        public string ToGoalCode { get; set; } = string.Empty;
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        
        public Geometry PathData
        {
            get
            {
                var geometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = StartPoint };
                
                figure.Segments.Add(new LineSegment(EndPoint, true));
                
                var arrowSize = 8.0;
                var angle = System.Math.Atan2(EndPoint.Y - StartPoint.Y, EndPoint.X - StartPoint.X);
                var arrowPoint1 = new Point(
                    EndPoint.X - arrowSize * System.Math.Cos(angle - System.Math.PI / 6),
                    EndPoint.Y - arrowSize * System.Math.Sin(angle - System.Math.PI / 6));
                var arrowPoint2 = new Point(
                    EndPoint.X - arrowSize * System.Math.Cos(angle + System.Math.PI / 6),
                    EndPoint.Y - arrowSize * System.Math.Sin(angle + System.Math.PI / 6));
                
                figure.Segments.Add(new LineSegment(arrowPoint1, false));
                figure.Segments.Add(new LineSegment(EndPoint, false));
                figure.Segments.Add(new LineSegment(arrowPoint2, false));
                
                geometry.Figures.Add(figure);
                return geometry;
            }
        }
    }
}
