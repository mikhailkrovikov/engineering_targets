using System.Collections.Generic;

namespace EngineeringTargets.Models
{
    public class CalculationResultRow
    {
        public string GoalCode { get; set; } = string.Empty;
        public string GoalName { get; set; } = string.Empty;
        public int Level { get; set; }
        public double AbsoluteWeight { get; set; }
        public double RelativeWeight { get; set; }
        public int Rank { get; set; }
        public List<string> IncomingLinks { get; set; } = new List<string>(); // Цели-предшественники
        public List<string> OutgoingLinks { get; set; } = new List<string>(); // Цели-последователи
        public string LinksDisplay { get; set; } = string.Empty; // Отформатированное отображение связей
    }
}

