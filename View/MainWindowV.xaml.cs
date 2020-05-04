using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZMuse.ViewModel;

namespace ZMuse.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowV : Window
    {
        private MainWindowVM _vm;

        public MainWindowV()
        {
            InitializeComponent();
            DataContext = 
                _vm     = new MainWindowVM();
        }

        // Not MVVM but currently no clean way to forward these to the VM.
        private void LibraryListSelChange(Object sender, EventArgs e)
        {
            _vm.LibraryListSelChange(sender, e);
        }
        
        private void PlayListSelChange(Object sender, EventArgs e)
        {
            _vm.PlayListSelChange(sender, e);
        }
    }
}
