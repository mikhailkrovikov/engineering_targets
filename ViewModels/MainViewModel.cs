using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using EngineeringTargets.Helpers;
using EngineeringTargets.Models;
using EngineeringTargets.Services;
using Microsoft.Win32;

namespace EngineeringTargets.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IProjectStorage _projectStorage;
        private ProjectModel _project;

        public MainViewModel()
        {
            _projectStorage = new JsonProjectStorage();
            _project = new ProjectModel();

            InitializeProjectChanged();

            LevelsAndGoalsViewModel = new LevelsAndGoalsViewModel(_project, OnProjectChanged);
            LinksViewModel = new LinksViewModel(_project, OnProjectChanged);
            CalculationViewModel = new CalculationViewModel(_project, OnProjectChanged);

            NewProjectCommand = new RelayCommand(_ => NewProject());
            OpenProjectCommand = new RelayCommand(_ => OpenProject());
            SaveProjectCommand = new RelayCommand(_ => SaveProject());
        }

        public Action OnProjectChanged { get; private set; } = () => { };

        private void OnProjectChangedHandler()
        {
            // Обновляем все ViewModels при изменении проекта
            LevelsAndGoalsViewModel.UpdateProject(_project);
            LinksViewModel.UpdateProject(_project);
            CalculationViewModel.UpdateProject(_project);
        }

        private void InitializeProjectChanged()
        {
            OnProjectChanged = OnProjectChangedHandler;
        }

        public LevelsAndGoalsViewModel LevelsAndGoalsViewModel { get; }
        public LinksViewModel LinksViewModel { get; }
        public CalculationViewModel CalculationViewModel { get; }

        public ICommand NewProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }

        private void NewProject()
        {
            _project = new ProjectModel();
            LevelsAndGoalsViewModel.UpdateProject(_project);
            LinksViewModel.UpdateProject(_project);
            CalculationViewModel.UpdateProject(_project);
        }

        private void OpenProject()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Открыть проект"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _project = _projectStorage.Load(dialog.FileName);
                    LevelsAndGoalsViewModel.UpdateProject(_project);
                    LinksViewModel.UpdateProject(_project);
                    CalculationViewModel.UpdateProject(_project);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveProject()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Сохранить проект",
                FileName = _project.Title
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _projectStorage.Save(_project, dialog.FileName);
                    MessageBox.Show("Проект успешно сохранен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

