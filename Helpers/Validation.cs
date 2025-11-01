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

            // Пустой проект
            if (project == null || (project.Levels.Count == 0 && project.Goals.Count == 0))
            {
                errors.Add("Проект пуст");
                return errors;
            }

            // Пустой список целей
            if (project.Goals.Count == 0)
            {
                errors.Add("Список целей пуст");
            }

            // Проверка уровней без целей
            foreach (var level in project.Levels)
            {
                if (!project.Goals.Any(g => g.LevelIndex == level.Index))
                {
                    errors.Add($"Уровень {level.Index} не содержит целей");
                }
            }

            // Проверка суммы весов по уровням
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

                    // Проверка целей без веса
                    foreach (var goal in levelGoals)
                    {
                        if (goal.RelativeWeight < 0 || goal.RelativeWeight > 1)
                        {
                            errors.Add($"Цель {goal.Code} имеет некорректный вес: {goal.RelativeWeight:F3}");
                        }
                    }
                }
            }

            // Проверка связей
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

                // Связь на тот же уровень или вверх
                if (fromGoal.LevelIndex >= toGoal.LevelIndex)
                {
                    errors.Add($"Недопустимая связь: {link.FromGoalCode} (уровень {fromGoal.LevelIndex}) -> {link.ToGoalCode} (уровень {toGoal.LevelIndex})");
                }

                // Петля
                if (link.FromGoalCode == link.ToGoalCode)
                {
                    errors.Add($"Петля: цель {link.FromGoalCode} связана сама с собой");
                }
            }

            return errors;
        }
    }
}

