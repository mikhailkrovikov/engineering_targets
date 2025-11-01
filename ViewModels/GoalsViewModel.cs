using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EngineeringTargets.Helpers;
using EngineeringTargets.Models;

namespace EngineeringTargets.ViewModels
{
    public class GoalsViewModel : ObservableObject
    {
        private ProjectModel _project;
        private GoalModel? _selectedGoal;
        private LevelModel? _selectedLevelForNewGoal;
        private string _newGoalName = string.Empty;
        private double _newGoalWeight;

        private Action? _onProjectChanged;

        public GoalsViewModel(ProjectModel project, Action? onProjectChanged = null)
        {
            _project = project;
            _onProjectChanged = onProjectChanged;
            Goals = new ObservableCollection<GoalModel>(_project.Goals);
            Levels = new ObservableCollection<LevelModel>(_project.Levels);

            AddGoalCommand = new RelayCommand(_ => AddGoal(), _ => CanAddGoal());
            DeleteGoalCommand = new RelayCommand(_ => DeleteGoal(), _ => SelectedGoal != null);
            UpdateWeightCommand = new RelayCommand(_ => UpdateWeight());
        }

        public ObservableCollection<GoalModel> Goals { get; }
        public ObservableCollection<LevelModel> Levels { get; }

        public GoalModel? SelectedGoal
        {
            get => _selectedGoal;
            set => SetProperty(ref _selectedGoal, value);
        }

        public LevelModel? SelectedLevelForNewGoal
        {
            get => _selectedLevelForNewGoal;
            set => SetProperty(ref _selectedLevelForNewGoal, value);
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

        public ICommand AddGoalCommand { get; }
        public ICommand DeleteGoalCommand { get; }
        public ICommand UpdateWeightCommand { get; }

        public void UpdateProject(ProjectModel project)
        {
            _project = project;
            Goals.Clear();
            foreach (var goal in _project.Goals)
            {
                Goals.Add(goal);
            }
            Levels.Clear();
            foreach (var level in _project.Levels)
            {
                Levels.Add(level);
            }
            SelectedGoal = null;
            SelectedLevelForNewGoal = Levels.FirstOrDefault();
        }

        private bool CanAddGoal()
        {
            return !string.IsNullOrWhiteSpace(NewGoalName) && SelectedLevelForNewGoal != null;
        }

        private void AddGoal()
        {
            if (SelectedLevelForNewGoal == null) return;

            int nextNumber = 1;
            var existingGoalsInLevel = Goals.Where(g => g.LevelIndex == SelectedLevelForNewGoal.Index).ToList();
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
            Goals.Add(goal);
            NewGoalName = string.Empty;
            NewGoalWeight = 0;
            _onProjectChanged?.Invoke();
        }

        private void DeleteGoal()
        {
            if (SelectedGoal != null)
            {
                // Удаляем связи с этой целью
                _project.Links.RemoveAll(l => l.FromGoalCode == SelectedGoal.Code || l.ToGoalCode == SelectedGoal.Code);
                _project.Goals.Remove(SelectedGoal);
                Goals.Remove(SelectedGoal);
                SelectedGoal = null;
                _onProjectChanged?.Invoke();
            }
        }

        private void UpdateWeight()
        {
            if (SelectedGoal != null)
            {
                SelectedGoal.RelativeWeight = NewGoalWeight;
                var projectIndex = _project.Goals.FindIndex(g => g.Code == SelectedGoal.Code);
                if (projectIndex >= 0)
                {
                    _project.Goals[projectIndex].RelativeWeight = NewGoalWeight;
                }
                // Обновляем коллекцию для уведомления UI
                var index = Goals.IndexOf(SelectedGoal);
                if (index >= 0)
                {
                    Goals.RemoveAt(index);
                    Goals.Insert(index, SelectedGoal);
                }
                _onProjectChanged?.Invoke();
            }
        }
    }
}

