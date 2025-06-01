//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;
using TP.ConcurrentProgramming.Data;
using System.Collections.Generic;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
  {
    #region ctor

    public BusinessLogicImplementation() : this(null)
    { }

    internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
    {
      layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
      _diagnosticDataCollector = new DiagnosticDataCollector();
    }

    #endregion ctor

    #region BusinessLogicAbstractAPI

    public override void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      layerBellow.Dispose();
      Disposed = true;
    }

    public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler, double maxX, double maxY)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));

      _diagnosticDataCollector.RegisterEvent("GameStart", "Starting game with balls", 
        new Dictionary<string, object> { { "numberOfBalls", numberOfBalls }, { "maxX", maxX }, { "maxY", maxY } });

      layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
      {
        var ball = new Ball(databall);
        var position = new Position(startingPosition.x, startingPosition.y);
        
        _diagnosticDataCollector.RegisterEvent("BallCreated", "New ball created", 
          new Dictionary<string, object> { { "x", position.x }, { "y", position.y } });
        
        upperLayerHandler(position, ball);
      }, maxX, maxY);
    }

    public override IDiagnosticDataCollector GetDiagnosticDataCollector()
    {
      return _diagnosticDataCollector;
    }

    #endregion BusinessLogicAbstractAPI

    #region private

    private bool Disposed = false;
    private readonly UnderneathLayerAPI layerBellow;
    private readonly IDiagnosticDataCollector _diagnosticDataCollector;

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}