using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    public class DiagnosticData : IDiagnosticData
    {
        public DateTime Timestamp { get; }
        public string EventType { get; }
        public string Description { get; }
        public Dictionary<string, object> Parameters { get; }
        public long ElapsedMilliseconds { get; }

        public DiagnosticData(string eventType, string description, Dictionary<string, object> parameters = null, long elapsedMs = 0)
        {
            Timestamp = DateTime.UtcNow;
            EventType = eventType;
            Description = description;
            Parameters = parameters ?? new Dictionary<string, object>();
            ElapsedMilliseconds = elapsedMs;
        }
    }

    public class DiagnosticDataCollector : IDiagnosticDataCollector
    {
        private readonly ConcurrentQueue<IDiagnosticData> _diagnosticData;
        private readonly Stopwatch _stopwatch;
        private readonly object _lockObject = new object();
        private long _lastCleanupTime;

        public DiagnosticDataCollector()
        {
            _diagnosticData = new ConcurrentQueue<IDiagnosticData>();
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _lastCleanupTime = _stopwatch.ElapsedMilliseconds;
        }

        public void RegisterEvent(string eventType, string description, Dictionary<string, object> parameters = null)
        {
            var elapsedMs = _stopwatch.ElapsedMilliseconds;
            var diagnosticData = new DiagnosticData(eventType, description, parameters, elapsedMs);
            _diagnosticData.Enqueue(diagnosticData);

            // Cleanup old data every 5 minutes
            if (elapsedMs - _lastCleanupTime > 300000) // 5 minutes in milliseconds
            {
                CleanupOldData();
                _lastCleanupTime = elapsedMs;
            }
        }

        public IEnumerable<IDiagnosticData> GetDiagnosticData()
        {
            return _diagnosticData.ToArray();
        }

        public void ClearDiagnosticData()
        {
            while (_diagnosticData.TryDequeue(out _)) { }
            _lastCleanupTime = _stopwatch.ElapsedMilliseconds;
        }

        private void CleanupOldData()
        {
            lock (_lockObject)
            {
                var currentTime = _stopwatch.ElapsedMilliseconds;
                var tempQueue = new ConcurrentQueue<IDiagnosticData>();

                while (_diagnosticData.TryDequeue(out var data))
                {
                    if (currentTime - data.ElapsedMilliseconds <= 3600000) // Keep last hour of data
                    {
                        tempQueue.Enqueue(data);
                    }
                }

                while (tempQueue.TryDequeue(out var data))
                {
                    _diagnosticData.Enqueue(data);
                }
            }
        }
    }
} 