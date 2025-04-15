//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicImplementationUnitTest
    {
        private const double DefaultMaxX = 400.0; // Domyślne granice dla testów
        private const double DefaultMaxY = 420.0;

        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (BusinessLogicImplementation newInstance = new(new DataLayerConstructorFixcure()))
            {
                bool newInstanceDisposed = true;
                newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
                Assert.IsFalse(newInstanceDisposed);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataLayerDisposeFixcure dataLayerFixcure = new DataLayerDisposeFixcure();
            BusinessLogicImplementation newInstance = new(dataLayerFixcure);
            Assert.IsFalse(dataLayerFixcure.Disposed);
            bool newInstanceDisposed = true;
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsFalse(newInstanceDisposed);
            newInstance.Dispose();
            newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
            Assert.IsTrue(newInstanceDisposed);
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
            Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }, DefaultMaxX, DefaultMaxY));
            Assert.IsTrue(dataLayerFixcure.Disposed);
        }

        [TestMethod]
        public void StartTestMethod()
        {
            DataLayerStartFixcure dataLayerFixcure = new();
            using (BusinessLogicImplementation newInstance = new(dataLayerFixcure))
            {
                int called = 0;
                int numberOfBalls2Create = 10;
                newInstance.Start(
                    numberOfBalls2Create,
                    (startingPosition, ball) =>
                    {
                        called++;
                        Assert.IsNotNull(startingPosition);
                        Assert.IsNotNull(ball);
                    },
                    DefaultMaxX,
                    DefaultMaxY);
                Assert.AreEqual<int>(1, called); // Oczekujemy 1 wywołania, bo DataLayerStartFixcure wywołuje handler tylko raz
                Assert.IsTrue(dataLayerFixcure.StartCalled);
                Assert.AreEqual<int>(numberOfBalls2Create, dataLayerFixcure.NumberOfBallseCreated);
            }
        }

        #region testing instrumentation

        private class DataLayerConstructorFixcure : Data.DataAbstractAPI
        {
            public override void Dispose()
            { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler, double maxX, double maxY)
            {
                throw new NotImplementedException();
            }

           // public override void UpdateBallBoundaries(double maxX, double maxY)
           // {
           //     throw new NotImplementedException();
           // }
        }

        private class DataLayerDisposeFixcure : Data.DataAbstractAPI
        {
            internal bool Disposed = false;

            public override void Dispose()
            {
                Disposed = true;
            }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler, double maxX, double maxY)
            {
                throw new NotImplementedException();
            }

           // public override void UpdateBallBoundaries(double maxX, double maxY)
           //{
           //     throw new NotImplementedException();
           // }
        }

        private class DataLayerStartFixcure : Data.DataAbstractAPI
        {
            internal bool StartCalled = false;
            internal int NumberOfBallseCreated = -1;

            public override void Dispose()
            { }

            public override void Start(int numberOfBalls, Action<IVector, Data.IBall> upperLayerHandler, double maxX, double maxY)
            {
                StartCalled = true;
                NumberOfBallseCreated = numberOfBalls;
                upperLayerHandler(new DataVectorFixture(), new DataBallFixture());
            }

            //public override void UpdateBallBoundaries(double maxX, double maxY)
            //{
                // Nie używane w tym teście, więc pusta implementacja
            //}

            private record DataVectorFixture : Data.IVector
            {
                public double x { get; init; }
                public double y { get; init; }
            }

            private class DataBallFixture : Data.IBall
            {
                private DataVectorFixture position = new DataVectorFixture { x = 0.0, y = 0.0 };
                private DataVectorFixture velocity = new DataVectorFixture { x = 0.0, y = 0.0 };

                public IVector Velocity
                {
                    get => velocity;
                    set => velocity = (DataVectorFixture)value;
                }

                public IVector Position => position;

                public double Radius => 10.0;

                public event EventHandler<IVector>? NewPositionNotification = null;

                public void SetBoundaries(double maxX, double maxY)
                {
                    // Pusta implementacja, ponieważ nie jest używana w teście
                }
            }
        }

        #endregion testing instrumentation
    }
}