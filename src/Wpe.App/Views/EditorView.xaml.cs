using System.Windows;
using System.Windows.Controls;
using Wpe.App.ViewModels;
using WPELibrary.Lib;

namespace Wpe.App.Views
{
    public partial class EditorView : UserControl
    {
        private EditorViewModel _vm;

        public EditorView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_vm != null)
                _vm.CompareRequested -= OnCompareRequested;

            _vm = e.NewValue as EditorViewModel;

            if (_vm != null)
                _vm.CompareRequested += OnCompareRequested;
        }

        private void OnCompareRequested(Socket_PacketInfo spi)
        {
            if (spi == null) return;

            var win = new CompareWindow(spi) { Owner = Window.GetWindow(this) };
            win.ShowDialog();
        }
    }
}
