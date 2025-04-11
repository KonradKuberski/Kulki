//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor
        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            PositionBackingField = initialPosition;
            Velocity = initialVelocity;
            Radius = 10.0; // Promień = 10, bo srednica to 20
        }
        #endregion ctor

        #region IBall
        public event EventHandler<IVector>? NewPositionNotification;
        public IVector Velocity { get; set; }
        public IVector Position => PositionBackingField; 
        public double Radius { get; } 
        #endregion IBall

        #region private
        private Vector PositionBackingField;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, PositionBackingField);
        }

        internal void Move(Vector delta)
        {
            double scale = 0.016;
            double newX = PositionBackingField.x + delta.x * scale;
            double newY = PositionBackingField.y + delta.y * scale;
            double diameter = 2 * Radius;
            double maxX = 392.0 - diameter; // Ramka ma 400 na 420
            double maxY = 412.0 - diameter;
            Vector newVelocity = (Vector)Velocity;

            if (newX < 0) // Jeśli nowa pozycja kulki wychodziłaby za lewą ściankę, zmieniamy kierunek prędkości, żeby kulka się odbiła
            {
                newX = 0; // I ustawiamy pozycje tu na 0 lub na max w zależności od ścianki
                newVelocity = new Vector(-newVelocity.x, newVelocity.y);
            }
            else if (newX > maxX) // Tak samo, tylko dla prawej ścianki
            {
                newX = maxX;
                newVelocity = new Vector(-newVelocity.x, newVelocity.y);
            }

            if (newY < 0) // Dla dolnej ścianki
            {
                newY = 0;
                newVelocity = new Vector(newVelocity.x, -newVelocity.y);
            }
            else if (newY > maxY) // Dla górnej ścianki
            {
                newY = maxY;
                newVelocity = new Vector(newVelocity.x, -newVelocity.y);
            }

            UpdatePosition(new Vector(newX, newY)); // Aktualizacja pozycji i prędkości
            Velocity = newVelocity;
        }

        internal void UpdatePosition(Vector newPosition)
        {
            PositionBackingField = newPosition;
            RaiseNewPositionChangeNotification();
        }
        #endregion private
    }
}