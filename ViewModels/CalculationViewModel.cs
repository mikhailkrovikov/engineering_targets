using System;
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
        private Action? _onProjectChanged;

        public List<string> SortedGoalCodes
        {
            get => _sortedGoalCodes;
            private set
            {
                _sortedGoalCodes = value;
                OnPropertyChanged();
            }
        }

        public CalculationViewModel(ProjectModel project, Action? onProjectChanged = null)
        {
            _project = project;
            _onProjectChanged = onProjectChanged;
            _calculator = new GoalsGraphCalculator();
            GoalResults = new ObservableCollection<GoalResult>();
            CalculationRows = new ObservableCollection<CalculationResultRow>();
            FilteredCalculationRows = new ObservableCollection<CalculationResultRow>();
            Top10Results = new ObservableCollection<CalculationResultRow>();
            Bottom10Results = new ObservableCollection<CalculationResultRow>();
            ValidationErrors = new ObservableCollection<string>();
            MatrixA = new ObservableCollection<MatrixRow>();
            MatrixW = new ObservableCollection<MatrixRow>();

            CalculateCommand = new RelayCommand(_ => Calculate());
            PruneGraphCommand = new RelayCommand(_ => PruneGraph(), _ => HasResults && MinWeightThreshold > 0);
        }

        public ObservableCollection<GoalResult> GoalResults { get; }
        public ObservableCollection<CalculationResultRow> CalculationRows { get; } = new ObservableCollection<CalculationResultRow>();
        public ObservableCollection<CalculationResultRow> Top10Results { get; } = new ObservableCollection<CalculationResultRow>();
        public ObservableCollection<CalculationResultRow> Bottom10Results { get; } = new ObservableCollection<CalculationResultRow>();
        public ObservableCollection<string> ValidationErrors { get; }
        public ObservableCollection<MatrixRow> MatrixA { get; }
        public ObservableCollection<MatrixRow> MatrixW { get; }

        public bool HasResults => _calculationResult != null && _calculationResult.IsValid;
        public bool HasErrors => _calculationResult != null && !_calculationResult.IsValid;

        private string _sortColumn = "AbsoluteWeight";
        private bool _sortDescending = true;
        private double _minWeightThreshold = 0;

        public string SortColumn
        {
            get => _sortColumn;
            set
            {
                if (value != _sortColumn)
                {
                    _sortColumn = value;
                    OnPropertyChanged();
                    SortResults();
                }
            }
        }

        public bool SortDescending
        {
            get => _sortDescending;
            set
            {
                if (SetProperty(ref _sortDescending, value))
                {
                    SortResults();
                }
            }
        }

        public double MinWeightThreshold
        {
            get => _minWeightThreshold;
            set
            {
                if (SetProperty(ref _minWeightThreshold, value))
                {
                    FilterByThreshold();
                }
            }
        }

        public ObservableCollection<CalculationResultRow> FilteredCalculationRows { get; } = new ObservableCollection<CalculationResultRow>();

        public ICommand CalculateCommand { get; }
        public ICommand PruneGraphCommand { get; }

        public void UpdateProject(ProjectModel project)
        {
            _project = project;
            _calculationResult = null;
            GoalResults.Clear();
            CalculationRows.Clear();
            FilteredCalculationRows.Clear();
            Top10Results.Clear();
            Bottom10Results.Clear();
            ValidationErrors.Clear();
            MatrixA.Clear();
            MatrixW.Clear();
            MinWeightThreshold = 0;
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(HasErrors));
        }

        private void Calculate()
        {
            _calculationResult = _calculator.Calculate(_project);

            GoalResults.Clear();
            CalculationRows.Clear();
            FilteredCalculationRows.Clear();
            Top10Results.Clear();
            Bottom10Results.Clear();
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

                // Создаем строки с информацией о связях
                BuildCalculationRows();
                SortResults(); // SortResults уже вызывает FilterByThreshold внутри
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
            OnPropertyChanged(nameof(CalculationRows));
            OnPropertyChanged(nameof(FilteredCalculationRows));
            OnPropertyChanged(nameof(Top10Results));
            OnPropertyChanged(nameof(Bottom10Results));

            if (!_calculationResult.IsValid)
            {
                MessageBox.Show(
                    "Обнаружены ошибки валидации:\n\n" + string.Join("\n", _calculationResult.ValidationErrors),
                    "Ошибки валидации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void BuildCalculationRows()
        {
            CalculationRows.Clear();

            foreach (var goalResult in GoalResults)
            {
                var row = new CalculationResultRow
                {
                    GoalCode = goalResult.Code,
                    GoalName = goalResult.Name,
                    Level = goalResult.Level,
                    AbsoluteWeight = goalResult.AbsoluteWeight,
                    RelativeWeight = goalResult.RelativeWeight,
                    Rank = goalResult.Rank
                };

                // Находим входящие и исходящие связи
                var incomingLinks = _project.Links
                    .Where(l => l.ToGoalCode == goalResult.Code)
                    .Select(l => l.FromGoalCode)
                    .ToList();

                var outgoingLinks = _project.Links
                    .Where(l => l.FromGoalCode == goalResult.Code)
                    .Select(l => l.ToGoalCode)
                    .ToList();

                row.IncomingLinks = incomingLinks;
                row.OutgoingLinks = outgoingLinks;

                // Форматируем отображение связей
                var linksParts = new List<string>();
                if (incomingLinks.Any())
                {
                    linksParts.Add($"← {string.Join(", ", incomingLinks)}");
                }
                if (outgoingLinks.Any())
                {
                    linksParts.Add($"→ {string.Join(", ", outgoingLinks)}");
                }
                row.LinksDisplay = string.Join(" | ", linksParts);
                if (string.IsNullOrEmpty(row.LinksDisplay))
                {
                    row.LinksDisplay = "-";
                }

                CalculationRows.Add(row);
            }
            
            OnPropertyChanged(nameof(CalculationRows));
        }

        private void SortResults()
        {
            if (CalculationRows.Count == 0) return;

            // Сохраняем данные во временный список для сортировки
            var tempList = CalculationRows.ToList();

            IEnumerable<CalculationResultRow> sorted = SortColumn switch
            {
                "GoalCode" => SortDescending 
                    ? tempList.OrderByDescending(r => r.GoalCode)
                    : tempList.OrderBy(r => r.GoalCode),
                "GoalName" => SortDescending 
                    ? tempList.OrderByDescending(r => r.GoalName)
                    : tempList.OrderBy(r => r.GoalName),
                "Level" => SortDescending 
                    ? tempList.OrderByDescending(r => r.Level)
                    : tempList.OrderBy(r => r.Level),
                "AbsoluteWeight" => SortDescending 
                    ? tempList.OrderByDescending(r => r.AbsoluteWeight)
                    : tempList.OrderBy(r => r.AbsoluteWeight),
                "Rank" => SortDescending 
                    ? tempList.OrderByDescending(r => r.Rank)
                    : tempList.OrderBy(r => r.Rank),
                _ => tempList.OrderByDescending(r => r.AbsoluteWeight)
            };

            var sortedList = sorted.ToList();

            CalculationRows.Clear();
            foreach (var row in sortedList)
            {
                CalculationRows.Add(row);
            }

            // Уведомляем об изменении CalculationRows
            OnPropertyChanged(nameof(CalculationRows));

            // Обновляем топ-10 лучших и худших
            UpdateTopBottom10();
            FilterByThreshold();
        }

        private void UpdateTopBottom10()
        {
            Top10Results.Clear();
            Bottom10Results.Clear();

            if (CalculationRows.Count == 0)
            {
                OnPropertyChanged(nameof(Top10Results));
                OnPropertyChanged(nameof(Bottom10Results));
                return;
            }

            var sortedByWeight = CalculationRows.OrderByDescending(r => r.AbsoluteWeight).ToList();
            
            foreach (var row in sortedByWeight.Take(10))
            {
                Top10Results.Add(row);
            }

            foreach (var row in sortedByWeight.TakeLast(10).Reverse())
            {
                Bottom10Results.Add(row);
            }
            
            OnPropertyChanged(nameof(Top10Results));
            OnPropertyChanged(nameof(Bottom10Results));
        }

        private void FilterByThreshold()
        {
            FilteredCalculationRows.Clear();
            
            if (CalculationRows.Count == 0)
            {
                OnPropertyChanged(nameof(FilteredCalculationRows));
                return;
            }
            
            if (MinWeightThreshold > 0)
            {
                foreach (var row in CalculationRows)
                {
                    if (row.AbsoluteWeight >= MinWeightThreshold)
                    {
                        FilteredCalculationRows.Add(row);
                    }
                }
            }
            else
            {
                // Показываем все результаты если порог = 0
                foreach (var row in CalculationRows)
                {
                    FilteredCalculationRows.Add(row);
                }
            }
            
            OnPropertyChanged(nameof(FilteredCalculationRows));
        }

        private void PruneGraph()
        {
            if (!HasResults || MinWeightThreshold <= 0) return;

            var goalsToRemove = CalculationRows
                .Where(r => r.AbsoluteWeight < MinWeightThreshold)
                .Select(r => r.GoalCode)
                .ToList();

            if (goalsToRemove.Count == 0)
            {
                MessageBox.Show("Нет целей для удаления при заданном пороге.", "Информация", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Будет удалено целей: {goalsToRemove.Count}\n\n" +
                $"Удалить цели с абсолютным весом менее {MinWeightThreshold:F3}?",
                "Подтверждение обрезки графа",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Удаляем цели и связанные связи
                foreach (var goalCode in goalsToRemove)
                {
                    var goal = _project.Goals.FirstOrDefault(g => g.Code == goalCode);
                    if (goal != null)
                    {
                        _project.Links.RemoveAll(l => l.FromGoalCode == goalCode || l.ToGoalCode == goalCode);
                        _project.Goals.Remove(goal);
                    }
                }

                // Пересчитываем
                _onProjectChanged?.Invoke();
                Calculate();
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

