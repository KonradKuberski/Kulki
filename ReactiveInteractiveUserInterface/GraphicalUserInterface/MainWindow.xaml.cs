//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Threading;
using TP.ConcurrentProgramming.Presentation.ViewModel;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.PresentationView
{
    /// <summary>
    /// View implementation
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.Balls.CollectionChanged += (sender, e) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            foreach (ModelIBall ball in e.NewItems)
                            {
                                Console.WriteLine($"Kulka w UI: Top={ball.Top}, Left={ball.Left}, Diameter={ball.Diameter}");
                            }
                            Console.WriteLine($"Dodano {e.NewItems?.Count ?? 0} kulek");
                        }
                    }, DispatcherPriority.Background);
                };
            }
        }

        private bool _isStarted = false;

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isStarted)
            {
                MessageBox.Show("Symulacja już uruchomiona. Zamknij okno, aby zresetować.");
                return;
            }

            if (DataContext is MainWindowViewModel viewModel)
            {
                try
                {
                    if (int.TryParse(BallsCountTextBox.Text, out int ballsCount))
                    {
                        if (ballsCount > 0 && ballsCount <= 50)
                        {
                            viewModel.Start(ballsCount);
                            _isStarted = true;
                            StartButton.IsEnabled = false; // Wyłącz przycisk po starcie
                        }
                        else
                        {
                            MessageBox.Show("Wprowadź liczbę kulek od 1 do 50.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Wprowadź poprawną liczbę całkowitą.");
                    }
                }
                catch (ObjectDisposedException)
                {
                    MessageBox.Show("Symulacja została zwolniona. Zamknij okno i spróbuj ponownie.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Raises the <seealso cref="System.Windows.Window.Closed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.Dispose();
            base.OnClosed(e);
        }
    }
}