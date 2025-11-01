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
                // Добавляем строку уровня
                TableRows.Add(new LevelGoalRowModel
                {
                    Name = level.Name,
                    Weight = null,
                    IsLevel = true,
                    IsEmptyRow = false,
                    LevelIndex = level.Index,
                    Level = level
                });

                // Добавляем цели этого уровня
                var levelGoals = _project.Goals
                    .Where(g => g.LevelIndex == level.Index)
                    .OrderBy(g => int.Parse(g.Code.Split('-')[1]))
                    .ToList();

                foreach (var goal in levelGoals)
                {
                    var goalRow = new LevelGoalRowModel
                    {
                        Name = $"  {goal.Code} - {goal.Name}",
                        Weight = goal.RelativeWeight,
                        IsLevel = false,
                        IsEmptyRow = false,
                        LevelIndex = level.Index,
                        GoalCode = goal.Code,
                        Goal = goal
                    };
                    TableRows.Add(goalRow);
                }

                // Добавляем пустую строку перед следующим уровнем (кроме последнего)
                if (level != sortedLevels.Last())
                {
                    TableRows.Add(new LevelGoalRowModel
                    {
                        Name = string.Empty,
                        Weight = null,
                        IsLevel = false,
                        IsEmptyRow = true,
                        LevelIndex = 0
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
            SelectedTemplate = null; // Очищаем выбор после добавления
            RefreshTable();
            _onProjectChanged?.Invoke();
        }

        private bool CanAddGoal()
        {
            return !string.IsNullOrWhiteSpace(NewGoalName) && SelectedLevelForNewGoal != null;
        }

        private void AddGoal()
        {
            if (SelectedLevelForNewGoal == null) return;

            int nextNumber = 1;
            var existingGoalsInLevel = _project.Goals.Where(g => g.LevelIndex == SelectedLevelForNewGoal.Index).ToList();
            if (existingGoalsInLevel.Any())
            {
                var numbers = existingGoalsInLevel.Select(g => int.Parse(g.Code.Split('-')[1])).ToList();
                nextNumber = numbers.Max() + 1;
            }

            string code = $"{SelectedLevelForNewGoal.Index}-{nextNumber}";

            var goal = new GoalModel
            {
                Code = code,
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
                int levelIndex = SelectedRow.Level.Index;
                var goalsToDelete = _project.Goals.Where(g => g.LevelIndex == levelIndex).ToList();
                foreach (var goal in goalsToDelete)
                {
                    _project.Links.RemoveAll(l => l.FromGoalCode == goal.Code || l.ToGoalCode == goal.Code);
                    _project.Goals.Remove(goal);
                }
                _project.Levels.Remove(SelectedRow.Level);
                Levels.Remove(SelectedRow.Level);
            }
            else if (!SelectedRow.IsLevel && SelectedRow.Goal != null)
            {
                _project.Links.RemoveAll(l => l.FromGoalCode == SelectedRow.Goal.Code || l.ToGoalCode == SelectedRow.Goal.Code);
                _project.Goals.Remove(SelectedRow.Goal);
            }

            SelectedRow = null;
            RefreshTable();
            _onProjectChanged?.Invoke();
        }

        private void UpdateWeight()
        {
            if (SelectedRow == null || SelectedRow.IsLevel || SelectedRow.IsEmptyRow || SelectedRow.Goal == null) return;

            SelectedRow.Goal.RelativeWeight = NewGoalWeight;
            var projectIndex = _project.Goals.FindIndex(g => g.Code == SelectedRow.Goal.Code);
            if (projectIndex >= 0)
            {
                _project.Goals[projectIndex].RelativeWeight = NewGoalWeight;
            }
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

