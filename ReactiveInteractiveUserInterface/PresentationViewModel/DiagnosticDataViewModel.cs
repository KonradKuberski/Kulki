using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TP.ConcurrentProgramming.BusinessLogic;
using TP.ConcurrentProgramming.Data;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class DiagnosticDataViewModel : ViewModelBase
    {
        private readonly BusinessLogicAbstractAPI _businessLogic;
        private readonly ObservableCollection<IDiagnosticData> _diagnosticData;

        public DiagnosticDataViewModel()
        {
            _businessLogic = BusinessLogicAbstractAPI.GetBusinessLogicLayer();
            _diagnosticData = new ObservableCollection<IDiagnosticData>();
            RefreshCommand = new RelayCommand(Refresh);
            ClearCommand = new RelayCommand(Clear);
            Refresh();
        }

        public ObservableCollection<IDiagnosticData> DiagnosticData => _diagnosticData;

        public ICommand RefreshCommand { get; }
        public ICommand ClearCommand { get; }

        private void Refresh()
        {
            _diagnosticData.Clear();
            foreach (var data in _businessLogic.GetDiagnosticDataCollector().GetDiagnosticData())
            {
                _diagnosticData.Add(data);
            }
        }

        private void Clear()
        {
            _businessLogic.GetDiagnosticDataCollector().ClearDiagnosticData();
            _diagnosticData.Clear();
        }
    }
} 