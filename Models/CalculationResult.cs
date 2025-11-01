using System.Collections.Generic;

namespace EngineeringTargets.Models
{
    public class CalculationResult
    {
        public List<GoalResult> GoalResults { get; set; } = new List<GoalResult>();
        public double[,]? MatrixA { get; set; }
        public double[,]? MatrixW { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public bool IsValid { get; set; }
    }

    public class GoalResult
    {
        public string Code { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Name { get; set; } = string.Empty;
        public double RelativeWeight { get; set; }
        public double AbsoluteWeight { get; set; }
        public int Rank { get; set; }
    }
}

