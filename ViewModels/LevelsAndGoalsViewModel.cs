/// <summary>
/// ViewModel для управления уровнями и целями в единой иерархической таблице
/// </summary>
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EngineeringTargets.Helpers;
using EngineeringTargets.Models;
using EngineeringTargets.Services;

namespace EngineeringTargets.ViewModels
{
    public class LevelsAndGoalsViewModel : ObservableObject
    {
        private ProjectModel _project;
        private LevelGoalRowModel? _selectedRow;
        private string _newLevelName = string.Empty;
        private string _newGoalName = string.Empty;
        private double _newGoalWeight;
        private LevelModel? _selectedLevelForNewGoal;
        private string? _selectedTemplate;
        private Action? _onProjectChanged;

        public LevelsAndGoalsViewModel(ProjectModel project, Action? onProjectChanged = null)
        {
            _project = project;
            _onProjectChanged = onProjectChanged;
            TableRows = new ObservableCollection<LevelGoalRowModel>();
            Levels = new ObservableCollection<LevelModel>(_project.Levels);

            RefreshTable();

            AddLevelCommand = new RelayCommand(_ => AddLevel(), _ => !string.IsNullOrWhiteSpace(NewLevelName));
            AddGoalCommand = new RelayCommand(_ => AddGoal(), _ => CanAddGoal());
            DeleteRowCommand = new RelayCommand(_ => DeleteRow(), _ => SelectedRow != null && !SelectedRow.IsEmptyRow);
            UpdateWeightCommand = new RelayCommand(_ => UpdateWeight(), _ => SelectedRow != null && SelectedRow.IsLevel == false && !SelectedRow.IsEmptyRow);
            AddLevelFromTemplateCommand = new RelayCommand(_ => AddLevelFromTemplate(SelectedTemplate), _ => !string.IsNullOrWhiteSpace(SelectedTemplate));
        }

        public ObservableCollection<LevelGoalRowModel> TableRows { get; }
        public ObservableCollection<LevelModel> Levels { get; }
        public ObservableCollection<string> LevelTemplates { get; } = new ObservableCollection<string>(EngineeringTargets.Services.LevelTemplates.GetStandardLevelNames());

        public LevelGoalRowModel? SelectedRow
        {
            get => _selectedRow;
            set
            {
                SetProperty(ref _selectedRow, value);
                if (value != null && !value.IsLevel && !value.IsEmptyRow && value.Goal != null)
                {
                    NewGoalWeight = value.Goal.RelativeWeight;
                    SelectedLevelForNewGoal = Levels.FirstOrDefault(l => l.Index == value.Goal.LevelIndex);
                }
            }
        }

        public string NewLevelName
        {
            get => _newLevelName;
            set
            {
                SetProperty(ref _newLevelName, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string NewGoalName
        {
            get => _newGoalName;
            set
            {
                SetProperty(ref _newGoalName, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public double NewGoalWeight
        {
            get => _newGoalWeight;
            set => SetProperty(ref _newGoalWeight, value);
        }

        public LevelModel? SelectedLevelForNewGoal
        {
            get => _selectedLevelForNewGoal;
            set => SetProperty(ref _selectedLevelForNewGoal, value);
        }

        public string? SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                if (SetProperty(ref _selectedTemplate, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ICommand AddLevelCommand { get; }
        public ICommand AddGoalCommand { get; }
        public ICommand DeleteRowCommand { get; }
        public ICommand UpdateWeightCommand { get; }
        public ICommand AddLevelFromTemplateCommand { get; }

        public void UpdateProject(ProjectModel project)
        {
            _project = project;
            Levels.Clear();
            foreach (var level in _project.Levels)
            {
                Levels.Add(level);
            }
            SelectedLevelForNewGoal = Levels.FirstOrDefault();
            RefreshTable();
        }

        private void RefreshTable()
        {
            TableRows.Clear();
            var sortedLevels = _project.Levels.OrderBy(l => l.Index).ToList();

            foreach (var level in sortedLevels)
            {
                TableRows.Add(new LevelGoalRowModel
                {
                    Name = level.Name,
                    Weight = null,
                    IsLevel = true,
                    IsEmptyRow = false,
                    LevelIndex = level.Index,
                    GoalCode = $"Уровень {level.Index}",
                    Level = level
                });

                var levelGoals = _project.Goals
                    .Where(g => g.LevelIndex == level.Index)
                    .OrderBy(g => int.Parse(g.Code.Split('-')[1]))
                    .ToList();

                foreach (var goal in levelGoals)
                {
                    var goalRow = new LevelGoalRowModel
                    {
                        Name = goal.Name,
                        Weight = goal.RelativeWeight,
                        IsLevel = false,
                        IsEmptyRow = false,
                        LevelIndex = level.Index,
                        GoalCode = goal.Code,
                        Goal = goal
                    };
                    TableRows.Add(goalRow);
                }

                if (level != sortedLevels.Last())
                {
                    TableRows.Add(new LevelGoalRowModel 
                    { 
                        IsEmptyRow = true,
                        GoalCode = ""
                    });
                }
            }
        }

        private void AddLevel()
        {
            int nextIndex = Levels.Count == 0 ? 1 : Levels.Max(l => l.Index) + 1;
            var level = new LevelModel
            {
                Index = nextIndex,
                Name = NewLevelName
            };

            _project.Levels.Add(level);
            Levels.Add(level);
            NewLevelName = string.Empty;
            RefreshTable();
            _onProjectChanged?.Invoke();
        }

        private void AddLevelFromTemplate(string? templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName)) return;

            int nextIndex = Levels.Count == 0 ? 1 : Levels.Max(l => l.Index) + 1;
            var level = new LevelModel
            {
                Index = nextIndex,
                Name = templateName
            };

            _project.Levels.Add(level);
            Levels.Add(level);
            SelectedTemplate = null;
            RefreshTable();
            _onProjectChanged?.Invoke();
        }

        private bool CanAddGoal()
        {
            return !string.IsNullOrWhiteSpace(NewGoalName) && 
                   SelectedLevelForNewGoal != null && 
                   NewGoalWeight >= 0 && 
                   NewGoalWeight <= 1;
        }

        private void AddGoal()
        {
            if (!CanAddGoal() || SelectedLevelForNewGoal == null) return;

            var levelGoals = _project.Goals.Where(g => g.LevelIndex == SelectedLevelForNewGoal.Index).ToList();
            int nextGoalNumber = levelGoals.Count == 0 ? 1 : levelGoals.Max(g => int.Parse(g.Code.Split('-')[1])) + 1;

            var goal = new GoalModel
            {
                Code = $"{SelectedLevelForNewGoal.Index}-{nextGoalNumber}",
                LevelIndex = SelectedLevelForNewGoal.Index,
                Name = NewGoalName,
                RelativeWeight = NewGoalWeight
            };

            _project.Goals.Add(goal);
            NewGoalName = string.Empty;
            NewGoalWeight = 0;
            RefreshTable();
            _onProjectChanged?.Invoke();
        }

        private void DeleteRow()
        {
            if (SelectedRow == null || SelectedRow.IsEmptyRow) return;

            if (SelectedRow.IsLevel && SelectedRow.Level != null)
            {
                var level = SelectedRow.Level;
                var goalsToRemove = _project.Goals.Where(g => g.LevelIndex == level.Index).ToList();
                foreach (var goal in goalsToRemove)
                {
                    _project.Links.RemoveAll(l => l.FromGoalCode == goal.Code || l.ToGoalCode == goal.Code);
                    _project.Goals.Remove(goal);
                }
                _project.Levels.Remove(level);
                Levels.Remove(level);
            }
            else if (SelectedRow.Goal != null)
            {
                var goal = SelectedRow.Goal;
                _project.Links.RemoveAll(l => l.FromGoalCode == goal.Code || l.ToGoalCode == goal.Code);
                _project.Goals.Remove(goal);
            }

            SelectedRow = null;
            RefreshTable();
            _onProjectChanged?.Invoke();
        }

        private void UpdateWeight()
        {
            if (SelectedRow == null || SelectedRow.IsLevel || SelectedRow.IsEmptyRow || SelectedRow.Goal == null) return;

            SelectedRow.Goal.RelativeWeight = NewGoalWeight;
            var projectGoal = _project.Goals.FirstOrDefault(g => g.Code == SelectedRow.Goal.Code);
            if (projectGoal != null)
            {
                projectGoal.RelativeWeight = NewGoalWeight;
            }
            SelectedRow.Weight = NewGoalWeight;
            RefreshTable();
            _onProjectChanged?.Invoke();
        }

        public void UpdateWeightFromTable(LevelGoalRowModel row, double newWeight)
        {
            if (row == null || row.IsLevel || row.IsEmptyRow || row.Goal == null) return;

            row.Goal.RelativeWeight = newWeight;
            var projectIndex = _project.Goals.FindIndex(g => g.Code == row.Goal.Code);
            if (projectIndex >= 0)
            {
                _project.Goals[projectIndex].RelativeWeight = newWeight;
            }
            row.Weight = newWeight;
            _onProjectChanged?.Invoke();
        }
    }
}
