/// <summary>
/// ViewModel для управления уровнями (устаревший, используется LevelsAndGoalsViewModel)
/// </summary>
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EngineeringTargets.Helpers;
using EngineeringTargets.Models;

namespace EngineeringTargets.ViewModels
{
    public class LevelsViewModel : ObservableObject
    {
        private ProjectModel _project;
        private LevelModel? _selectedLevel;
        private string _newLevelName = string.Empty;

        private Action? _onProjectChanged;

        public LevelsViewModel(ProjectModel project, Action? onProjectChanged = null)
        {
            _project = project;
            _onProjectChanged = onProjectChanged;
            Levels = new ObservableCollection<LevelModel>(_project.Levels);

            AddLevelCommand = new RelayCommand(_ => AddLevel(), _ => !string.IsNullOrWhiteSpace(NewLevelName));
            DeleteLevelCommand = new RelayCommand(_ => DeleteLevel(), _ => SelectedLevel != null);
        }

        public ObservableCollection<LevelModel> Levels { get; }

        public LevelModel? SelectedLevel
        {
            get => _selectedLevel;
            set => SetProperty(ref _selectedLevel, value);
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

        public ICommand AddLevelCommand { get; }
        public ICommand DeleteLevelCommand { get; }

        public void UpdateProject(ProjectModel project)
        {
            _project = project;
            Levels.Clear();
            foreach (var level in _project.Levels)
            {
                Levels.Add(level);
            }
            SelectedLevel = null;
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
            _onProjectChanged?.Invoke();
        }

        private void DeleteLevel()
        {
            if (SelectedLevel != null)
            {
                int levelIndex = SelectedLevel.Index;
                
                var goalsToDelete = _project.Goals.Where(g => g.LevelIndex == levelIndex).ToList();
                foreach (var goal in goalsToDelete)
                {
                    _project.Links.RemoveAll(l => l.FromGoalCode == goal.Code || l.ToGoalCode == goal.Code);
                    _project.Goals.Remove(goal);
                }

                _project.Levels.Remove(SelectedLevel);
                Levels.Remove(SelectedLevel);
                SelectedLevel = null;
                _onProjectChanged?.Invoke();
            }
        }
    }
}

