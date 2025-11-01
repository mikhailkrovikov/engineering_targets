/// <summary>
/// Модель цели: код, уровень, название, относительный вес
/// </summary>
namespace EngineeringTargets.Models
{
    public class GoalModel
    {
        public string Code { get; set; } = string.Empty;
        public int LevelIndex { get; set; }
        public string Name { get; set; } = string.Empty;
        public double RelativeWeight { get; set; }
    }
}
