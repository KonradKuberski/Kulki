using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DiagnosticDataCollectorTest
    {
        private IDiagnosticDataCollector _collector;

        [TestInitialize]
        public void Initialize()
        {
            _collector = new DiagnosticDataCollector();
        }

        [TestMethod]
        public void RegisterEvent_ShouldAddEventToCollection()
        {
            // Arrange
            string eventType = "TestEvent";
            string description = "Test Description";
            var parameters = new Dictionary<string, object> { { "key", "value" } };

            // Act
            _collector.RegisterEvent(eventType, description, parameters);

            // Assert
            var data = _collector.GetDiagnosticData().ToList();
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(eventType, data[0].EventType);
            Assert.AreEqual(description, data[0].Description);
            Assert.AreEqual(parameters["key"], data[0].Parameters["key"]);
        }

        [TestMethod]
        public void ClearDiagnosticData_ShouldRemoveAllEvents()
        {
            // Arrange
            _collector.RegisterEvent("Event1", "Description1");
            _collector.RegisterEvent("Event2", "Description2");

            // Act
            _collector.ClearDiagnosticData();

            // Assert
            Assert.AreEqual(0, _collector.GetDiagnosticData().Count());
        }

        [TestMethod]
        public void GetDiagnosticData_ShouldReturnEventsInOrder()
        {
            // Arrange
            _collector.RegisterEvent("Event1", "First");
            _collector.RegisterEvent("Event2", "Second");

            // Act
            var data = _collector.GetDiagnosticData().ToList();

            // Assert
            Assert.AreEqual(2, data.Count);
            Assert.AreEqual("Event1", data[0].EventType);
            Assert.AreEqual("Event2", data[1].EventType);
        }
    }
} 