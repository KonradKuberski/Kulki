//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IDisposable, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        #region ctor

        public MainWindowViewModel() : this(null)
        { }

        internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
            Observer?.Dispose();
            Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
            StartGameCommand = new StartGameCommandImplementation(this);
        }

        #endregion ctor

        #region public API

        private int _initialBallCount = 5;
        public int InitialBallCount
        {
            get => _initialBallCount;
            set
            {
                if (_initialBallCount != value)
                {
                    _initialBallCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand StartGameCommand { get; }

        private class StartGameCommandImplementation : ICommand
        {
            private readonly MainWindowViewModel _viewModel;

            public StartGameCommandImplementation(MainWindowViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                if (_viewModel.Disposed)
                    throw new ObjectDisposedException(nameof(MainWindowViewModel));

                // Uruchom symulację tylko, jeśli liczba kulek jest poprawna
                if (_viewModel.InitialBallCount > 0)
                {
                    _viewModel.Balls.Clear();
                    _viewModel.ModelLayer.Start(_viewModel.InitialBallCount, _viewModel.maxX, _viewModel.maxY);
                    _viewModel.Observer?.Dispose();
                    _viewModel.Observer = _viewModel.ModelLayer.Subscribe<ModelIBall>(x => _viewModel.Balls.Add(x));
                }
            }
        }

        public void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Balls.Clear();
            ModelLayer.Start(numberOfBalls, maxX, maxY);
        }

        public void UpdateBoundaries(double maxX, double maxY)
        {
            this.maxX = maxX;
            this.maxY = maxY;
            if (Balls.Count > 0)
            {
                Balls.Clear();
                ModelLayer.Start(InitialBallCount, maxX, maxY);
                Observer?.Dispose();
                Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
            }
        }

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        #endregion public API

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Balls.Clear();
                    Observer?.Dispose();
                    ModelLayer.Dispose();
                }
                Disposed = true;
            }
        }

        public void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private IDisposable Observer = null;
        private ModelAbstractApi ModelLayer;
        private bool Disposed = false;
        private double maxX = 400.0;
        private double maxY = 420.0;

        #endregion private
    }
}