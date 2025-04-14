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

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor
        public DataImplementation()
        {
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));
        }
        #endregion ctor

        #region DataAbstractAPI
        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            Random random = new Random();

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

            for (int i = 0; i < numberOfBalls; i++) // Tworzenie kulek, losowanie pozycji i prędkości startowej 
            {
                Vector startingPosition = new(random.Next(100, 300), random.Next(100, 320)); 
                Vector initialVelocity = possibleVelocities[random.Next(possibleVelocities.Length)];
                Ball newBall = new(startingPosition, initialVelocity);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
            }
        }
        #endregion DataAbstractAPI

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    MoveTimer.Dispose();
                    BallsList.Clear();
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
        private readonly Timer MoveTimer;
        private Random RandomGenerator = new();
        private List<Ball> BallsList = [];

        private void Move(object? x)
        {
            HandleCollisions();

            foreach (Ball item in BallsList.ToList()) // Iterujemy po kopii listy
            {
                item.Move((Vector)item.Velocity);
            }
        }

        private void HandleCollisions()
        {
            for (int i = 0; i < BallsList.Count; i++)
            {
                for (int j = i + 1; j < BallsList.Count; j++)
                {
                    Ball ball1 = BallsList[i] as Ball;
                    Ball ball2 = BallsList[j] as Ball;

                    if (ball1 == null || ball2 == null) continue;

                    double dx = ball2.Position.x - ball1.Position.x;
                    double dy = ball2.Position.y - ball1.Position.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    // Sprawdzamy, czy kulki się stykają (odległość mniejsza lub równa sumie promieni)
                    if (distance <= (ball1.Radius + ball2.Radius))
                    {
                        if (distance == 0) // Unikamy dzielenia przez zero
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
                            // Zakładamy, że kulki mają tę samą masę (sprężyste zderzenie)
                            double impulse = 2 * dotProduct / 2.0; // Dzielimy przez 2, bo masa = 1 dla obu kulek

                            // Aktualizujemy prędkości
                            ball1.Velocity = new Vector(
                              ball1.Velocity.x + impulse * nx,
                              ball1.Velocity.y + impulse * ny
                            );
                            ball2.Velocity = new Vector(
                              ball2.Velocity.x - impulse * nx,
                              ball2.Velocity.y - impulse * ny
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
        #endregion private

        #region TestingInfrastructure
        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }
        #endregion TestingInfrastructure
    }
}