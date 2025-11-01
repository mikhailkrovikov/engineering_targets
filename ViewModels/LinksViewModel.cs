using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EngineeringTargets.Helpers;
using EngineeringTargets.Models;

namespace EngineeringTargets.ViewModels
{
    public class LinksViewModel : ObservableObject
    {
        private ProjectModel _project;
        private LinkModel? _selectedLink;
        private string? _selectedFromGoal;
        private string? _selectedToGoal;

        private Action? _onProjectChanged;

        public LinksViewModel(ProjectModel project, Action? onProjectChanged = null)
        {
            _project = project;
            _onProjectChanged = onProjectChanged;
            Links = new ObservableCollection<LinkModel>(_project.Links);
            UpdateGoalCodes();

            AddLinkCommand = new RelayCommand(_ => AddLink(), _ => CanAddLink());
            DeleteLinkCommand = new RelayCommand(_ => DeleteLink(), _ => SelectedLink != null);
        }

        public ObservableCollection<LinkModel> Links { get; }
        public ObservableCollection<string> FromGoalCodes { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ToGoalCodes { get; } = new ObservableCollection<string>();

        public LinkModel? SelectedLink
        {
            get => _selectedLink;
            set => SetProperty(ref _selectedLink, value);
        }

        public string? SelectedFromGoal
        {
            get => _selectedFromGoal;
            set => SetProperty(ref _selectedFromGoal, value);
        }

        public string? SelectedToGoal
        {
            get => _selectedToGoal;
            set => SetProperty(ref _selectedToGoal, value);
        }

        public ICommand AddLinkCommand { get; }
        public ICommand DeleteLinkCommand { get; }

        public void UpdateProject(ProjectModel project)
        {
            _project = project;
            Links.Clear();
            foreach (var link in _project.Links)
            {
                Links.Add(link);
            }
            UpdateGoalCodes();
            SelectedLink = null;
        }

        private void UpdateGoalCodes()
        {
            FromGoalCodes.Clear();
            ToGoalCodes.Clear();

            var goals = _project.Goals.OrderBy(g => g.LevelIndex).ThenBy(g => g.Code).ToList();
            foreach (var goal in goals)
            {
                FromGoalCodes.Add(goal.Code);
                ToGoalCodes.Add(goal.Code);
            }
        }

        private bool CanAddLink()
        {
            if (string.IsNullOrEmpty(SelectedFromGoal) || string.IsNullOrEmpty(SelectedToGoal))
                return false;

            if (SelectedFromGoal == SelectedToGoal)
                return false;

            var fromGoal = _project.Goals.FirstOrDefault(g => g.Code == SelectedFromGoal);
            var toGoal = _project.Goals.FirstOrDefault(g => g.Code == SelectedToGoal);

            if (fromGoal == null || toGoal == null)
                return false;

            // Связь только вниз
            if (fromGoal.LevelIndex >= toGoal.LevelIndex)
                return false;

            // Проверка на дубликат
            if (_project.Links.Any(l => l.FromGoalCode == SelectedFromGoal && l.ToGoalCode == SelectedToGoal))
                return false;

            return true;
        }

        private void AddLink()
        {
            if (!CanAddLink()) return;

            var link = new LinkModel
            {
                FromGoalCode = SelectedFromGoal!,
                ToGoalCode = SelectedToGoal!
            };

            _project.Links.Add(link);
            Links.Add(link);
            SelectedFromGoal = null;
            SelectedToGoal = null;
            _onProjectChanged?.Invoke();
        }

        private void DeleteLink()
        {
            if (SelectedLink != null)
            {
                _project.Links.Remove(SelectedLink);
                Links.Remove(SelectedLink);
                SelectedLink = null;
                _onProjectChanged?.Invoke();
            }
        }
    }
}

