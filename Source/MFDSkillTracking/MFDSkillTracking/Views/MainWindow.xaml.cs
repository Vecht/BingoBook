using System;
using System.Windows;
using System.Windows.Input;
using MFDSkillTracking.ViewModels;

namespace MFDSkillTracking.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm?.SaveExistingData();
            Application.Current.Shutdown();
        }

        private void NewCharacterTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var vm = DataContext as MainViewModel;
            if (vm == null) return;
            if (vm.AddNewCharacterCommand.CanExecute(null))
                vm.AddNewCharacterCommand.Execute(null);
        }

        private void CharacterListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            TryShowCharacter();
        }

        private void CharacterListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TryShowCharacter();
        }

        private void TryShowCharacter()
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;
            if (vm.ShowCharacterCommand.CanExecute(null))
                vm.ShowCharacterCommand.Execute(null);
        }

        private void CharacterSearch_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm == null) return;
            vm.CharacterSearchString = "";
        }
    }
}
