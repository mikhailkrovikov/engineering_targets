using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using EngineeringTargets.ViewModels;

namespace EngineeringTargets.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.CalculationViewModel.PropertyChanged += CalculationViewModel_PropertyChanged;
            }
        }

        private void CalculationViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CalculationViewModel.MatrixA) || 
                e.PropertyName == nameof(CalculationViewModel.MatrixW) ||
                e.PropertyName == nameof(CalculationViewModel.HasResults))
            {
                // Используем Dispatcher для обновления UI после того, как данные добавлены
                Dispatcher.BeginInvoke(new Action(() => UpdateMatrixColumns()), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void UpdateMatrixColumns()
        {
            if (DataContext is not MainViewModel viewModel) return;

            var matrixARows = viewModel.CalculationViewModel.MatrixA;
            var matrixWRows = viewModel.CalculationViewModel.MatrixW;

            if (matrixARows.Count > 0)
            {
                UpdateDataGridColumns(MatrixADataGrid, matrixARows);
            }

            if (matrixWRows.Count > 0)
            {
                UpdateDataGridColumns(MatrixWDataGrid, matrixWRows);
            }
        }

        private void UpdateDataGridColumns(DataGrid dataGrid, System.Collections.ObjectModel.ObservableCollection<MatrixRow> rows)
        {
            if (rows.Count == 0) return;

            if (DataContext is not MainViewModel viewModel) return;

            // Очищаем старые колонки (кроме первой с кодом)
            var columnsToRemove = dataGrid.Columns.Where(c => c.Header?.ToString() != "Код").ToList();
            foreach (var col in columnsToRemove)
            {
                dataGrid.Columns.Remove(col);
            }

            // Добавляем новые колонки для значений матрицы
            // Заголовки колонок - это коды целей в отсортированном порядке
            var sortedGoalCodes = viewModel.CalculationViewModel.SortedGoalCodes;
            int columnCount = rows[0].Values.Count;

            for (int i = 0; i < columnCount && i < sortedGoalCodes.Count; i++)
            {
                int columnIndex = i; // Для замыкания
                var column = new DataGridTextColumn
                {
                    Header = sortedGoalCodes[i], // Используем код цели как заголовок
                    Binding = new Binding($"Values[{columnIndex}]")
                    {
                        StringFormat = "F3"
                    },
                    Width = 80,
                    IsReadOnly = true
                };
                dataGrid.Columns.Add(column);
            }
        }
    }
}

