using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EngineeringTargets.Helpers;
using EngineeringTargets.Models;
using EngineeringTargets.Services;

namespace EngineeringTargets.ViewModels
{
    public class GraphViewModel : ObservableObject
    {
        private ProjectModel _project;
        private readonly IGoalsGraphCalculator? _calculator;
        private Action? _onProjectChanged;
        
        private const double LEVEL_SPACING = 150; // Расстояние между уровнями
        private const double NODE_SPACING = 150; // Расстояние между узлами одного уровня
        private const double NODE_WIDTH = 140;
        private const double NODE_HEIGHT = 70;
        private const double MARGIN_X = 100;
        private const double MARGIN_Y = 100;

        public GraphViewModel(ProjectModel project, Action? onProjectChanged = null)
        {
            _project = project;
            _onProjectChanged = onProjectChanged;
            _calculator = new Services.GoalsGraphCalculator();
            Nodes = new ObservableCollection<GraphNodeViewModel>();
            Edges = new ObservableCollection<GraphEdgeViewModel>();
            
            BuildGraph();
        }

        public ObservableCollection<GraphNodeViewModel> Nodes { get; }
        public ObservableCollection<GraphEdgeViewModel> Edges { get; }

        public double CanvasWidth { get; private set; }
        public double CanvasHeight { get; private set; }

        public void UpdateProject(ProjectModel project)
        {
            _project = project;
            BuildGraph();
        }

        private void BuildGraph()
        {
            Nodes.Clear();
            Edges.Clear();

            if (_project == null || _project.Goals == null || _project.Goals.Count == 0)
            {
                CanvasWidth = 800;
                CanvasHeight = 600;
                OnPropertyChanged(nameof(CanvasWidth));
                OnPropertyChanged(nameof(CanvasHeight));
                OnPropertyChanged(nameof(Nodes));
                OnPropertyChanged(nameof(Edges));
                return;
            }

            // Получаем абсолютные веса, если возможно
            var calculationResult = _calculator?.Calculate(_project);
            var absoluteWeights = new Dictionary<string, double>();
            if (calculationResult?.IsValid == true)
            {
                foreach (var result in calculationResult.GoalResults)
                {
                    absoluteWeights[result.Code] = result.AbsoluteWeight;
                }
            }

            // Группируем цели по уровням
            var levels = _project.Levels.OrderBy(l => l.Index).ToList();
            var goalsByLevel = _project.Goals
                .GroupBy(g => g.LevelIndex)
                .OrderBy(g => g.Key)
                .ToList();

            // Размещаем узлы по уровням
            var nodePositions = new Dictionary<string, GraphNodeViewModel>();
            double currentY = MARGIN_Y;

            foreach (var level in levels)
            {
                var levelGoals = goalsByLevel.FirstOrDefault(g => g.Key == level.Index);
                if (levelGoals == null) continue;

                var sortedGoals = levelGoals.OrderBy(g => int.Parse(g.Code.Split('-')[1])).ToList();
                int goalCount = sortedGoals.Count;
                double totalWidth = goalCount > 1 ? (goalCount - 1) * NODE_SPACING + NODE_WIDTH : NODE_WIDTH;
                double minCanvasWidth = 1400;
                double startX = MARGIN_X + (Math.Max(minCanvasWidth, totalWidth + 2 * MARGIN_X) - totalWidth) / 2;

                for (int i = 0; i < sortedGoals.Count; i++)
                {
                    var goal = sortedGoals[i];
                    double x = startX + i * NODE_SPACING;
                    
                    var node = new GraphNodeViewModel
                    {
                        GoalCode = goal.Code,
                        GoalName = goal.Name,
                        LevelIndex = goal.LevelIndex,
                        RelativeWeight = goal.RelativeWeight,
                        AbsoluteWeight = absoluteWeights.GetValueOrDefault(goal.Code, 0),
                        Position = new Point(x, currentY),
                        Width = NODE_WIDTH,
                        Height = NODE_HEIGHT
                    };

                    Nodes.Add(node);
                    nodePositions[goal.Code] = node;
                }

                currentY += LEVEL_SPACING;
            }

            // Строим связи
            foreach (var link in _project.Links)
            {
                if (nodePositions.ContainsKey(link.FromGoalCode) && nodePositions.ContainsKey(link.ToGoalCode))
                {
                    var fromNode = nodePositions[link.FromGoalCode];
                    var toNode = nodePositions[link.ToGoalCode];

                    var edge = new GraphEdgeViewModel
                    {
                        FromGoalCode = link.FromGoalCode,
                        ToGoalCode = link.ToGoalCode,
                        StartPoint = new Point(fromNode.Position.X + NODE_WIDTH / 2, fromNode.Position.Y + NODE_HEIGHT),
                        EndPoint = new Point(toNode.Position.X + NODE_WIDTH / 2, toNode.Position.Y)
                    };

                    Edges.Add(edge);
                }
            }

            // Вычисляем размеры Canvas
            if (Nodes.Count > 0)
            {
                double maxX = Nodes.Max(n => n.Position.X + NODE_WIDTH);
                double maxY = Nodes.Max(n => n.Position.Y + NODE_HEIGHT);
                double maxLevelWidth = 0;
                
                // Находим максимальную ширину уровня для центрирования
                foreach (var level in levels)
                {
                    var levelGoals = goalsByLevel.FirstOrDefault(g => g.Key == level.Index);
                    if (levelGoals != null)
                    {
                        int count = levelGoals.Count();
                        double width = count > 1 ? (count - 1) * NODE_SPACING + NODE_WIDTH : NODE_WIDTH;
                        maxLevelWidth = Math.Max(maxLevelWidth, width);
                    }
                }
                
                CanvasWidth = Math.Max(1400, maxLevelWidth + 2 * MARGIN_X);
                CanvasHeight = maxY + MARGIN_Y;
            }
            else
            {
                CanvasWidth = 1400;
                CanvasHeight = 600;
            }

            OnPropertyChanged(nameof(CanvasWidth));
            OnPropertyChanged(nameof(CanvasHeight));
            OnPropertyChanged(nameof(Nodes));
            OnPropertyChanged(nameof(Edges));
        }
        
        public void RefreshGraph()
        {
            BuildGraph();
        }
    }
}

