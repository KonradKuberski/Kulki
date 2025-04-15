//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Windows;
using System.Windows.Controls;
using TP.ConcurrentProgramming.Presentation.ViewModel;

namespace TP.ConcurrentProgramming.PresentationView
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? viewModel;
        private Canvas? canvas;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainWindow: MainWindow_Loaded - start");
                viewModel = DataContext as MainWindowViewModel;
                if (viewModel == null)
                {
                    System.Diagnostics.Debug.WriteLine("MainWindow: viewModel jest null");
                    throw new InvalidOperationException("DataContext nie jest typu MainWindowViewModel lub jest null.");
                }

                canvas = (Canvas)FindName("BallCanvas");
                if (canvas == null)
                {
                    System.Diagnostics.Debug.WriteLine("MainWindow: canvas jest null");
                    throw new InvalidOperationException("Nie znaleziono elementu 'BallCanvas' w XAML.");
                }

                System.Diagnostics.Debug.WriteLine("MainWindow: MainWindow_Loaded - przed SizeChanged");
                canvas.SizeChanged += Canvas_SizeChanged;
                System.Diagnostics.Debug.WriteLine("MainWindow: MainWindow_Loaded - przed UpdateBoundaries");
                UpdateBoundaries();
                System.Diagnostics.Debug.WriteLine("MainWindow: MainWindow_Loaded - koniec");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow: Wyjątek w MainWindow_Loaded: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Wystąpił błąd podczas inicjalizacji okna: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainWindow: Canvas_SizeChanged");
                UpdateBoundaries();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow: Wyjątek w Canvas_SizeChanged: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void UpdateBoundaries()
        {
            if (canvas != null)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow: UpdateBoundaries - canvas.ActualWidth={canvas.ActualWidth}, canvas.ActualHeight={canvas.ActualHeight}");
                viewModel?.UpdateBoundaries(canvas.ActualWidth, canvas.ActualHeight);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MainWindow: OnClosed");
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.Dispose();
            base.OnClosed(e);
        }
    }
}