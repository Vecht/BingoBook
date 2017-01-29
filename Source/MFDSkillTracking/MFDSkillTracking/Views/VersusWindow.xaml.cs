using System.Windows;
using MFDSkillTracking.ViewModels;

namespace MFDSkillTracking.Views
{
    /// <summary>
    /// Interaction logic for VersusWindow.xaml
    /// </summary>
    public partial class VersusWindow : Window
    {
        public VersusWindow(VersusViewModel versusViewModel)
        {
            DataContext = versusViewModel;
            InitializeComponent();
        }
    }
}
