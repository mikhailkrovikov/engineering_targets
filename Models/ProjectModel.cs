/// <summary>
/// Модель проекта: заголовок, уровни, цели, связи
/// </summary>
using System.Collections.Generic;

namespace EngineeringTargets.Models
{
    public class ProjectModel
    {
        public string Title { get; set; } = "Новый проект";
        public List<LevelModel> Levels { get; set; } = new List<LevelModel>();
        public List<GoalModel> Goals { get; set; } = new List<GoalModel>();
        public List<LinkModel> Links { get; set; } = new List<LinkModel>();
    }
}
