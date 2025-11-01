/// <summary>
/// Модель связи между целями
/// </summary>
namespace EngineeringTargets.Models
{
    public class LinkModel
    {
        public string FromGoalCode { get; set; } = string.Empty;
        public string ToGoalCode { get; set; } = string.Empty;
    }
}
