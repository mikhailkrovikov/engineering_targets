using System.Windows;

namespace EngineeringTargets.Models
{
    public class GraphNodeViewModel
    {
        public string GoalCode { get; set; } = string.Empty;
        public string GoalName { get; set; } = string.Empty;
        public int LevelIndex { get; set; }
        public double RelativeWeight { get; set; }
        public double AbsoluteWeight { get; set; }
        public Point Position { get; set; }
        public double Width { get; set; } = 120;
        public double Height { get; set; } = 60;
    }
}

