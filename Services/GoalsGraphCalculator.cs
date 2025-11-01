using System;
using System.Collections.Generic;
using System.Linq;
using EngineeringTargets.Models;
using EngineeringTargets.Helpers;

namespace EngineeringTargets.Services
{
    public class GoalsGraphCalculator : IGoalsGraphCalculator
    {
        public CalculationResult Calculate(ProjectModel project)
        {
            var result = new CalculationResult();

            // Валидация
            result.ValidationErrors = Validation.ValidateProject(project);
            result.IsValid = result.ValidationErrors.Count == 0;

            if (!result.IsValid)
            {
                return result;
            }

            // Сортировка целей по уровню и номеру
            var sortedGoals = project.Goals
                .OrderBy(g => g.LevelIndex)
                .ThenBy(g => int.Parse(g.Code.Split('-')[1]))
                .ToList();

            int N = sortedGoals.Count;

            // Создаем индекс по коду
            var goalIndex = new Dictionary<string, int>();
            for (int i = 0; i < N; i++)
            {
                goalIndex[sortedGoals[i].Code] = i;
            }

            // Матрица смежности A (N × N)
            var matrixA = new double[N, N];
            foreach (var link in project.Links)
            {
                if (goalIndex.ContainsKey(link.FromGoalCode) && goalIndex.ContainsKey(link.ToGoalCode))
                {
                    int p = goalIndex[link.FromGoalCode];
                    int q = goalIndex[link.ToGoalCode];
                    matrixA[p, q] = 1;
                }
            }

            // Матрица весов W (N × N)
            var matrixW = new double[N, N];
            for (int p = 0; p < N; p++)
            {
                for (int q = 0; q < N; q++)
                {
                    if (matrixA[p, q] == 1)
                    {
                        matrixW[p, q] = sortedGoals[p].RelativeWeight * sortedGoals[q].RelativeWeight;
                    }
                }
            }

            // Расчет абсолютных весов
            var goalResults = new List<GoalResult>();
            for (int q = 0; q < N; q++)
            {
                double sumW = 0;
                for (int p = 0; p < N; p++)
                {
                    sumW += matrixW[p, q];
                }

                double R_q = sortedGoals[q].RelativeWeight + sumW;

                goalResults.Add(new GoalResult
                {
                    Code = sortedGoals[q].Code,
                    Level = sortedGoals[q].LevelIndex,
                    Name = sortedGoals[q].Name,
                    RelativeWeight = sortedGoals[q].RelativeWeight,
                    AbsoluteWeight = R_q
                });
            }

            // Сортировка по убыванию абсолютного веса и присвоение рангов
            var sortedResults = goalResults.OrderByDescending(r => r.AbsoluteWeight).ToList();
            for (int i = 0; i < sortedResults.Count; i++)
            {
                sortedResults[i].Rank = i + 1;
            }

            result.MatrixA = matrixA;
            result.MatrixW = matrixW;
            result.GoalResults = sortedResults;

            return result;
        }
    }
}

