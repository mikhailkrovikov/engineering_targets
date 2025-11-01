/// <summary>
/// Валидация проектов: проверка целей, уровней, связей, сумм весов
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using EngineeringTargets.Models;

namespace EngineeringTargets.Helpers
{
    public static class Validation
    {
        public static List<string> ValidateProject(ProjectModel project)
        {
            var errors = new List<string>();

            if (project == null || (project.Levels.Count == 0 && project.Goals.Count == 0))
            {
                errors.Add("Проект пуст");
                return errors;
            }

            if (project.Goals.Count == 0)
            {
                errors.Add("Список целей пуст");
            }

            foreach (var level in project.Levels)
            {
                if (!project.Goals.Any(g => g.LevelIndex == level.Index))
                {
                    errors.Add($"Уровень {level.Index} не содержит целей");
                }
            }

            foreach (var level in project.Levels)
            {
                var levelGoals = project.Goals.Where(g => g.LevelIndex == level.Index).ToList();
                if (levelGoals.Count > 0)
                {
                    var sum = levelGoals.Sum(g => g.RelativeWeight);
                    const double tolerance = 0.0001;
                    if (Math.Abs(sum - 1.0) > tolerance)
                    {
                        errors.Add($"Сумма весов уровня {level.Index} равна {sum:F3}, должна быть 1.0");
                    }

                    foreach (var goal in levelGoals)
                    {
                        if (goal.RelativeWeight < 0 || goal.RelativeWeight > 1)
                        {
                            errors.Add($"Цель {goal.Code} имеет некорректный вес: {goal.RelativeWeight:F3}");
                        }
                    }
                }
            }

            foreach (var link in project.Links)
            {
                var fromGoal = project.Goals.FirstOrDefault(g => g.Code == link.FromGoalCode);
                var toGoal = project.Goals.FirstOrDefault(g => g.Code == link.ToGoalCode);

                if (fromGoal == null)
                {
                    errors.Add($"Связь: цель {link.FromGoalCode} не найдена");
                    continue;
                }

                if (toGoal == null)
                {
                    errors.Add($"Связь: цель {link.ToGoalCode} не найдена");
                    continue;
                }

                if (fromGoal.LevelIndex >= toGoal.LevelIndex)
                {
                    errors.Add($"Недопустимая связь: {link.FromGoalCode} (уровень {fromGoal.LevelIndex}) -> {link.ToGoalCode} (уровень {toGoal.LevelIndex})");
                }

                if (link.FromGoalCode == link.ToGoalCode)
                {
                    errors.Add($"Петля: цель {link.FromGoalCode} связана сама с собой");
                }
            }

            return errors;
        }
    }
}
