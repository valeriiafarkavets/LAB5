using System.Windows;
using LAB5.ViewModels;
namespace LAB5{
    public partial class MainWindow : Window{
        public MainWindow(){
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
