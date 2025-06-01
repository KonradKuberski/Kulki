using System.Windows;
using TP.ConcurrentProgramming.Presentation.ViewModel;

namespace TP.ConcurrentProgramming.PresentationView
{
    public partial class DiagnosticDataWindow : Window
    {
        public DiagnosticDataWindow()
        {
            InitializeComponent();
            DataContext = new TP.ConcurrentProgramming.Presentation.ViewModel.DiagnosticDataViewModel();
        }
    }
} 