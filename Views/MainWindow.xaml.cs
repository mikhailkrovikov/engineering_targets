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

        private void WeightTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Разрешаем только цифры и точку/запятую
            if (!char.IsDigit(e.Text, 0) && e.Text != "." && e.Text != ",")
            {
                e.Handled = true;
            }
        }

        private void MinWeightTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            // Проверяем, является ли символ допустимым (цифра, точка, запятая, минус только в начале)
            if (char.IsDigit(e.Text, 0))
            {
                return; // Цифры разрешены
            }

            if (e.Text == "." || e.Text == ",")
            {
                // Проверяем, нет ли уже точки или запятой
                string currentText = textBox.Text ?? "";
                if (currentText.Contains('.') || currentText.Contains(','))
                {
                    e.Handled = true;
                }
                return;
            }

            // Все остальное запрещено
            e.Handled = true;
        }

        private void MinWeightTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Разрешаем Delete, Backspace, Tab и другие служебные клавиши
            if (e.Key == System.Windows.Input.Key.Back || 
                e.Key == System.Windows.Input.Key.Delete ||
                e.Key == System.Windows.Input.Key.Tab ||
                e.Key == System.Windows.Input.Key.Enter ||
                (e.Key >= System.Windows.Input.Key.Left && e.Key <= System.Windows.Input.Key.Down))
            {
                return;
            }

            // Разрешаем Ctrl+C, Ctrl+V, Ctrl+X, Ctrl+A
            if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                if (e.Key == System.Windows.Input.Key.C ||
                    e.Key == System.Windows.Input.Key.V ||
                    e.Key == System.Windows.Input.Key.X ||
                    e.Key == System.Windows.Input.Key.A)
                {
                    return;
                }
            }
        }

        private void MinWeightThresholdTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && DataContext is MainViewModel viewModel)
            {
                string text = textBox.Text?.Replace(",", ".") ?? "0";
                if (double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value))
                {
                    viewModel.CalculationViewModel.MinWeightThreshold = value;
                    textBox.Text = value.ToString("F3");
                }
                else
                {
                    viewModel.CalculationViewModel.MinWeightThreshold = 0;
                    textBox.Text = "0";
                }
            }
        }

        private void TableDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && 
                e.Column.Header?.ToString() == "Вес (r)" &&
                DataContext is MainViewModel viewModel &&
                e.Row.Item is Models.LevelGoalRowModel row &&
                e.EditingElement is TextBox textBox)
            {
                if (double.TryParse(textBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double newWeight))
                {
                    viewModel.LevelsAndGoalsViewModel.UpdateWeightFromTable(row, newWeight);
                }
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

            // Определяем формат в зависимости от типа матрицы
            bool isMatrixA = dataGrid == MatrixADataGrid;
            string format = isMatrixA ? "F0" : "F3";

            for (int i = 0; i < columnCount && i < sortedGoalCodes.Count; i++)
            {
                int columnIndex = i; // Для замыкания
                var column = new DataGridTextColumn
                {
                    Header = sortedGoalCodes[i], // Используем код цели как заголовок
                    Binding = new Binding($"Values[{columnIndex}]")
                    {
                        StringFormat = format
                    },
                    Width = 80,
                    IsReadOnly = true
                };
                dataGrid.Columns.Add(column);
            }
        }
    }
}

