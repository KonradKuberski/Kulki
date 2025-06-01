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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

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
                _ballTasks.Clear(); // Wyczyść zadania
            }

            // Lista wszystkich możliwych kombinacji prędkości
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

            for (int i = 0; i < numberOfBalls; i++)
            {
                double diameter = 20.0;
                double posX = random.Next(50, (int)(maxX - 50 - diameter));
                double posY = random.Next(50, (int)(maxY - 50 - diameter));
                Vector startingPosition = new(posX, posY);
                Vector initialVelocity = possibleVelocities[random.Next(possibleVelocities.Length)];
                double mass = 1.0;
                Ball newBall = new(startingPosition, initialVelocity, mass);
                newBall.SetBoundaries(maxX, maxY);
                upperLayerHandler(startingPosition, newBall);
                lock (_lockObject)
                {
                    BallsList.Add(newBall);
                }
            }

            // Uruchamiamy osobne zadania dla każdej kulki
            _cancellationTokenSource = new CancellationTokenSource();
            StartBallTasks(_cancellationTokenSource.Token);
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
                        _ballTasks.Clear();
                        // Zatrzymaj i wyczyść timery
                        foreach (var timer in _ballTimers)
                        {
                            timer.Stop();
                            timer.Dispose();
                        }
                        _ballTimers.Clear();
                        if (_collisionTimer != null)
                        {
                            _collisionTimer.Stop();
                            _collisionTimer.Dispose();
                            _collisionTimer = null;
                        }
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
        private List<Task> _ballTasks = []; // Lista zadań dla każdej kulki
        private CancellationTokenSource _cancellationTokenSource;
        private readonly object _lockObject = new object();
        private List<System.Timers.Timer> _ballTimers = new(); // Timery dla każdej kulki
        private System.Timers.Timer _collisionTimer; // Timer do obsługi kolizji

        private void StartBallTasks(CancellationToken cancellationToken)
        {
            lock (_lockObject)
            {
                foreach (Ball ball in BallsList)
                {
                    //Task ballTask = Task.Run(() => MoveBall(ball, cancellationToken), cancellationToken);
                    // _ballTasks.Add(ballTask);
                    StartBallTimer(ball);
                }

                //Task collisionTask = Task.Run(() => CollisionLoop(cancellationToken), cancellationToken);
                // _ballTasks.Add(collisionTask);
                StartCollisionTimer();
            }
        }

        private void StartBallTimer(Ball ball)
        {
            var timer = new System.Timers.Timer(16); // 16 ms ~ 60 FPS
            timer.Elapsed += (sender, e) =>
            {
                lock (ball)
                {
                    ball.Move((Vector)ball.Velocity);
                }
            };
            timer.AutoReset = true;
            timer.Start();
            _ballTimers.Add(timer);
        }

        private void StartCollisionTimer()
        {
            _collisionTimer = new System.Timers.Timer(8); // 8 ms
            _collisionTimer.Elapsed += (sender, e) =>
            {
                HandleCollisions();
            };
            _collisionTimer.AutoReset = true;
            _collisionTimer.Start();
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

                        lock (ball1)
                            lock (ball2)
                            {
                                double dx = ball2.Position.x - ball1.Position.x;
                                double dy = ball2.Position.y - ball1.Position.y;
                                double distance = Math.Sqrt(dx * dx + dy * dy);

                                if (distance <= (ball1.Radius + ball2.Radius))
                                {
                                    if (distance < 0.00001)
                                    {
                                        dx = 1.0;
                                        dy = 0.0;
                                        distance = 1.0;
                                    }
                                    double nx = dx / distance;
                                    double ny = dy / distance;

                                    double dvx = ball2.Velocity.x - ball1.Velocity.x;
                                    double dvy = ball2.Velocity.y - ball1.Velocity.y;
                                    double dotProduct = dvx * nx + dvy * ny;

                                    if (dotProduct < 0)
                                    {
                                        ball1.Velocity = new Vector(
                                            ball1.Velocity.x + dotProduct * nx,
                                            ball1.Velocity.y + dotProduct * ny
                                        );
                                        ball2.Velocity = new Vector(
                                            ball2.Velocity.x - dotProduct * nx,
                                            ball2.Velocity.y - dotProduct * ny
                                        );

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