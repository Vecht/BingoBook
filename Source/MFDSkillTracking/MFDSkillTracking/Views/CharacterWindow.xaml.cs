using System.Windows;
using System.Windows.Input;
using MFDSkillTracking.ViewModels;

namespace MFDSkillTracking.Views
{
    public partial class CharacterWindow : Window
    {
        public CharacterWindow(CharacterViewModel dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }

        private void NewKnownSkillTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var vm = DataContext as CharacterViewModel;
            if (vm == null) return;
            if (vm.AddKnownSkillCommand.CanExecute(null))
                vm.AddKnownSkillCommand.Execute(null);
        }

        private void NewUnknownSkillTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var vm = DataContext as CharacterViewModel;
            if (vm == null) return;
            if (vm.AddUnknownSkillCommand.CanExecute(null))
                vm.AddUnknownSkillCommand.Execute(null);
        }

        private void KnownSkillSearch_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as CharacterViewModel;
            if (vm == null) return;
            vm.KnownSkillSearchString = "";
        }

        private void UnknownSkillSearch_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as CharacterViewModel;
            if (vm == null) return;
            vm.UnknownSkillSearchString = "";
        }
    }
}
