/// <summary>
/// Модель строки для таблицы уровней и целей
/// </summary>
namespace EngineeringTargets.Models
{
    public class LevelGoalRowModel
    {
        public string Name { get; set; } = string.Empty;
        public double? Weight { get; set; }
        public bool IsLevel { get; set; }
        public bool IsEmptyRow { get; set; }
        public int LevelIndex { get; set; }
        public string? GoalCode { get; set; }
        public GoalModel? Goal { get; set; }
        public LevelModel? Level { get; set; }
    }
}
