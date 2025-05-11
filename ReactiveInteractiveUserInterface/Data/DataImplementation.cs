//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor
        public DataImplementation()
        {
            // Inicjalizacja przeniesiona do Start
        }
        #endregion ctor

        #region DataAbstractAPI
        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler, double maxX, double maxY)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            Random random = new Random();

            lock (_lockObject)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }
                BallsList.Clear(); // Wyczyść poprzednie kulki
            }

            // Lista wszystkich możliwych kombinacji prędkości (do przemyślenia)
            Vector[] possibleVelocities = new Vector[]
            {
                new Vector(90.0, 45.0),
                new Vector(-90.0, -45.0),
                new Vector(90.0, -45.0),
                new Vector(-90.0, 45.0),
                new Vector(90.0, 45.0),
                new Vector(-45.0, -90.0),
                new Vector(45.0, -90.0),
                new Vector(-45.0, 90.0)
            };

            for (int i = 0; i < numberOfBalls; i++) // Tworzenie kulek, losowanie pozycji, prędkości startowej i masy
            {
                double diameter = 20.0; // Średnica kulki
                double posX = random.Next(50, (int)(maxX - 50 - diameter));
                double posY = random.Next(50, (int)(maxY - 50 - diameter));
                Vector startingPosition = new(posX, posY);
                Vector initialVelocity = possibleVelocities[random.Next(possibleVelocities.Length)];
                double mass = 1.0; // Masa kulki
                Ball newBall = new(startingPosition, initialVelocity, mass);
                newBall.SetBoundaries(maxX, maxY); // Ustawiamy granice dla każdej kulki
                upperLayerHandler(startingPosition, newBall);
                lock (_lockObject) // Synchronizacja przy dodawaniu do listy
                {
                    BallsList.Add(newBall);
                }
            }

            // Uruchamiamy asynchroniczną pętlę ruchu
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => MoveLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }
        #endregion DataAbstractAPI

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                    lock (_lockObject)
                    {
                        BallsList.Clear();
                    }
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable

        #region private
        private bool Disposed = false;
        private Random RandomGenerator = new();
        private List<Ball> BallsList = [];
        private CancellationTokenSource _cancellationTokenSource;
        private readonly object _lockObject = new object();

        private async Task MoveLoop(CancellationToken cancellationToken)
        {
            try
            {
                const int frameDelayMs = 15; // Możliwość konfiguracji opóźnienia
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Move();
                    await Task.Delay(frameDelayMs, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Logowanie anulowania (opcjonalne)
                Debug.WriteLine("MoveLoop cancelled.");
            }
            catch (Exception ex)
            {
                // Obsługa nieoczekiwanych błędów
                Debug.WriteLine($"MoveLoop error: {ex.Message}");
            }
        }

        private async Task Move()
        {
            lock (_lockObject)
            {
                HandleCollisions();

                foreach (Ball item in BallsList.ToList()) // Używamy ToList, aby uniknąć modyfikacji kolekcji podczas iteracji
                {
                    item.Move((Vector)item.Velocity);
                }
            }
        }
        private void HandleCollisions()
        {
            lock (_lockObject)
            {
                for (int i = 0; i < BallsList.Count; i++)
                {
                    for (int j = i + 1; j < BallsList.Count; j++)
                    {
                        Ball ball1 = BallsList[i];
                        Ball ball2 = BallsList[j];

                        lock (ball1) // Synchronizacja na obu kulkach
                            lock (ball2)
                            {
                                double dx = ball2.Position.x - ball1.Position.x;
                                double dy = ball2.Position.y - ball1.Position.y;
                                double distance = Math.Sqrt(dx * dx + dy * dy);

                                // Sprawdzamy, czy kulki się stykają (odległość mniejsza lub równa sumie promieni)
                                if (distance <= (ball1.Radius + ball2.Radius))
                                {
                                    if (distance < 0.00001) // Unikamy dzielenia przez zero
                                    {
                                        dx = 1.0;
                                        dy = 0.0;
                                        distance = 1.0;
                                    }
                                    double nx = dx / distance;
                                    double ny = dy / distance;

                                    // Obliczamy względną prędkość
                                    double dvx = ball2.Velocity.x - ball1.Velocity.x;
                                    double dvy = ball2.Velocity.y - ball1.Velocity.y;

                                    // Obliczamy składową prędkości wzdłuż wektora kolizji
                                    double dotProduct = dvx * nx + dvy * ny;

                                    // Jeśli kulki zbliżają się do siebie (dotProduct < 0), obliczamy nowe prędkości
                                    if (dotProduct < 0)
                                    {
                                        // Upraszczamy, bo masa jest taka sama (m1 = m2 = 1.0)
                                        ball1.Velocity = new Vector(
                                            ball1.Velocity.x + dotProduct * nx,
                                            ball1.Velocity.y + dotProduct * ny
                                        );
                                        ball2.Velocity = new Vector(
                                            ball2.Velocity.x - dotProduct * nx,
                                            ball2.Velocity.y - dotProduct * ny
                                        );

                                        // Rozdzielamy kulki, aby się nie skleiły
                                        double overlap = (ball1.Radius + ball2.Radius - distance) / 2;
                                        ball1.UpdatePosition(new Vector(
                                            ball1.Position.x - overlap * nx,
                                            ball1.Position.y - overlap * ny
                                        ));
                                        ball2.UpdatePosition(new Vector(
                                            ball2.Position.x + overlap * nx,
                                            ball2.Position.y + overlap * ny
                                        ));
                                    }
                                }
                            }
                    }
                }
            }
        }
        #endregion private

        #region TestingInfrastructure
        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            lock (_lockObject)
            {
                returnBallsList(BallsList);
            }
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            lock (_lockObject)
            {
                returnNumberOfBalls(BallsList.Count);
            }
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }
        #endregion TestingInfrastructure
    }
}