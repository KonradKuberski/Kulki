//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            Ball newInstance = new(dataBallFixture);
            int numberOfCallBackCalled = 0;
            newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); Assert.IsNotNull(position); numberOfCallBackCalled++; };
            dataBallFixture.Move();
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
        }

        #region testing instrumentation

        private class DataBallFixture : Data.IBall
        {
            private VectorFixture position = new VectorFixture(0.0, 0.0);
            private VectorFixture velocity = new VectorFixture(0.0, 0.0);

            public Data.IVector Velocity
            {
                get => velocity;
                set => velocity = (VectorFixture)value;
            }

            public Data.IVector Position => position;
            public double Radius => 10.0;

            public event EventHandler<Data.IVector>? NewPositionNotification;

            internal void Move()
            {
                position = new VectorFixture(0.0, 0.0);
                NewPositionNotification?.Invoke(this, position);
            }

            public void SetBoundaries(double maxX, double maxY) // Dodana metoda
            {
                // Pusta implementacja, ponieważ nie jest używana w teście
            }
        }

        private class VectorFixture : Data.IVector
        {
            internal VectorFixture(double X, double Y)
            {
                x = X; y = Y;
            }

            public double x { get; init; }
            public double y { get; init; }
        }

        #endregion testing instrumentation
    }
}