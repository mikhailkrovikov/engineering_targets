using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EngineeringTargets.Helpers;
using EngineeringTargets.Models;
using EngineeringTargets.Services;

namespace EngineeringTargets.ViewModels
{
    public class CalculationViewModel : ObservableObject
    {
        private ProjectModel _project;
        private readonly IGoalsGraphCalculator _calculator;
        private CalculationResult? _calculationResult;
        private List<string> _sortedGoalCodes = new List<string>();

        public List<string> SortedGoalCodes
        {
            get => _sortedGoalCodes;
            private set
            {
                _sortedGoalCodes = value;
                OnPropertyChanged();
            }
        }

        public CalculationViewModel(ProjectModel project)
        {
            _project = project;
            _calculator = new GoalsGraphCalculator();
            GoalResults = new ObservableCollection<GoalResult>();
            ValidationErrors = new ObservableCollection<string>();
            MatrixA = new ObservableCollection<MatrixRow>();
            MatrixW = new ObservableCollection<MatrixRow>();

            CalculateCommand = new RelayCommand(_ => Calculate());
        }

        public ObservableCollection<GoalResult> GoalResults { get; }
        public ObservableCollection<string> ValidationErrors { get; }
        public ObservableCollection<MatrixRow> MatrixA { get; }
        public ObservableCollection<MatrixRow> MatrixW { get; }

        public bool HasResults => _calculationResult != null && _calculationResult.IsValid;
        public bool HasErrors => _calculationResult != null && !_calculationResult.IsValid;

        public ICommand CalculateCommand { get; }

        public void UpdateProject(ProjectModel project)
        {
            _project = project;
            _calculationResult = null;
            GoalResults.Clear();
            ValidationErrors.Clear();
            MatrixA.Clear();
            MatrixW.Clear();
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(HasErrors));
        }

        private void Calculate()
        {
            _calculationResult = _calculator.Calculate(_project);

            GoalResults.Clear();
            ValidationErrors.Clear();
            MatrixA.Clear();
            MatrixW.Clear();

            if (_calculationResult.IsValid && _calculationResult.MatrixA != null && _calculationResult.MatrixW != null)
            {
                foreach (var result in _calculationResult.GoalResults)
                {
                    GoalResults.Add(result);
                }

                // Заполнение матриц
                int N = _calculationResult.MatrixA.GetLength(0);
                var sortedGoals = _project.Goals
                    .OrderBy(g => g.LevelIndex)
                    .ThenBy(g => int.Parse(g.Code.Split('-')[1]))
                    .ToList();

                // Сохраняем коды целей в порядке сортировки для заголовков колонок
                SortedGoalCodes = sortedGoals.Select(g => g.Code).ToList();

                for (int i = 0; i < N; i++)
                {
                    var rowA = new MatrixRow { RowNumber = i, GoalCode = sortedGoals[i].Code, Values = new List<double>() };
                    var rowW = new MatrixRow { RowNumber = i, GoalCode = sortedGoals[i].Code, Values = new List<double>() };

                    for (int j = 0; j < N; j++)
                    {
                        rowA.Values.Add(_calculationResult.MatrixA[i, j]);
                        rowW.Values.Add(_calculationResult.MatrixW[i, j]);
                    }

                    MatrixA.Add(rowA);
                    MatrixW.Add(rowW);
                }

                OnPropertyChanged(nameof(SortedGoalCodes));
            }
            else
            {
                foreach (var error in _calculationResult.ValidationErrors)
                {
                    ValidationErrors.Add(error);
                }
            }

            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(HasErrors));
            OnPropertyChanged(nameof(MatrixA));
            OnPropertyChanged(nameof(MatrixW));

            if (!_calculationResult.IsValid)
            {
                MessageBox.Show(
                    "Обнаружены ошибки валидации:\n\n" + string.Join("\n", _calculationResult.ValidationErrors),
                    "Ошибки валидации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }

    public class MatrixRow
    {
        public int RowNumber { get; set; }
        public string GoalCode { get; set; } = string.Empty;
        public List<double> Values { get; set; } = new List<double>();
    }
}

