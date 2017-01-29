using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using MFDSkillTracking.Common;
using MFDSkillTracking.Models;
using MFDSkillTracking.Views;
using Microsoft.Win32;

namespace MFDSkillTracking.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        #region Fields

        private const string DATA_FILE = "data.bingobook";

        private readonly ObservableCollection<Character> _characters = new ObservableCollection<Character>();
        private readonly CollectionViewSource _charactersCVS = new CollectionViewSource();
        private readonly List<Window> _openWindows = new List<Window>();

        private string _characterSearchString;
        private Character _selectedCharacter;
        private string _newCharacterName;
        private Window _versusWindow;

        #endregion

        #region Properties

        public ICollectionView Characters => _charactersCVS.View;

        public string CharacterSearchString
        {
            get { return _characterSearchString; }
            set
            {
                _characterSearchString = value;
                OnPropertyChanged();
                Characters.Refresh();
            }
        }

        public Character SelectedCharacter
        {
            get { return _selectedCharacter; }
            set
            {
                _selectedCharacter = value;
                OnPropertyChanged();
                _removeCharacterCommand?.Update();
            }
        }

        public string NewCharacterName
        {
            get { return _newCharacterName; }
            set
            {
                _newCharacterName = value;
                OnPropertyChanged();
                _addNewCharacterCommand?.Update();
            }
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            _charactersCVS.Source = _characters;
            _charactersCVS.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            _charactersCVS.Filter += (sender, e) =>
            {
                var character = e.Item as Character;
                if (character == null || string.IsNullOrWhiteSpace(CharacterSearchString)) return;

                e.Accepted = character.Name.ToUpper().Contains(CharacterSearchString.ToUpper());
            };

            LoadExistingData();
        }

        #endregion

        #region Methods

        private void LoadExistingData()
        {
            if (!File.Exists(DATA_FILE)) return;

            var xml = File.ReadAllText(DATA_FILE);
            var importedCharacters = DeserializeData(xml);
            _characters.Clear();
            importedCharacters.ForEach(c => _characters.Add(c));
        }

        public void SaveExistingData()
        {
            File.WriteAllText(DATA_FILE, GetSerializedData());
        }

        #endregion

        #region Commands

        #region Add New Character Command

        private Command _addNewCharacterCommand;

        public ICommand AddNewCharacterCommand
        {
            get
            {
                if (_addNewCharacterCommand == null)
                {
                    _addNewCharacterCommand = new Command(AddNewCharacter, CanAddNewCharacter);
                }
                return _addNewCharacterCommand;
            }
        }

        private void AddNewCharacter()
        {
            _characters.Add(new Character(NewCharacterName));
            NewCharacterName = null;
            _openWindows.Select(x => (CharacterViewModel)x.DataContext).ToList().ForEach(x => x.UpdateProperty("PossibleOpponents"));
        }

        private bool CanAddNewCharacter()
        {
            return !string.IsNullOrWhiteSpace(NewCharacterName)
                   && _characters.All(c => c.Name.ToUpper() != NewCharacterName.ToUpper());
        }

        #endregion

        #region Remove Character Command

        private Command _removeCharacterCommand;

        public ICommand RemoveCharacterCommand
        {
            get
            {
                if (_removeCharacterCommand == null)
                {
                    _removeCharacterCommand = new Command(RemoveCharacter, CanRemoveCharacter);
                }
                return _removeCharacterCommand;
            }
        }

        private void RemoveCharacter()
        {
            var result = MessageBox.Show($"Are you sure you want to remove character '{SelectedCharacter.Name}'?\nThis cannot be undone.",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result != MessageBoxResult.Yes) return;

            _characters.Remove(SelectedCharacter);
            _openWindows.Select(x => (CharacterViewModel)x.DataContext).ToList().ForEach(x => x.UpdateProperty("PossibleOpponents"));
        }

        private bool CanRemoveCharacter()
        {
            return SelectedCharacter != null && _characters.Contains(SelectedCharacter);
        }

        #endregion

        #region Show Character Command

        private Command _showCharacterCommand;

        public ICommand ShowCharacterCommand
        {
            get
            {
                if (_showCharacterCommand == null)
                {
                    _showCharacterCommand = new Command(ShowCharacter, CanShowCharacter);
                }
                return _showCharacterCommand;
            }
        }

        private void ShowCharacter()
        {
            var openWindow =
                _openWindows.FirstOrDefault(x => ((CharacterViewModel) x.DataContext).Character == SelectedCharacter);

            if (openWindow != null)
            {
                openWindow.Focus();
                return;
            }

            var characterViewModel = new CharacterViewModel(SelectedCharacter, _characters);
            var characterWindow = new CharacterWindow(characterViewModel);
            characterWindow.Closed += (sender, e) => _openWindows.Remove((CharacterWindow) sender);
            _openWindows.Add(characterWindow);
            characterWindow.Show();
        }

        private bool CanShowCharacter()
        {
            return SelectedCharacter != null;
        }

        #endregion

        #region Save Data Command

        private ICommand _saveDataCommand;

        public ICommand SaveDataCommand
        {
            get
            {
                if (_saveDataCommand == null)
                {
                    _saveDataCommand = new Command(SaveData, CanSaveData);
                }
                return _saveDataCommand;
            }
        }

        private void SaveData()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Bingo Book Data|*.bingobook",
                Title = "Save Data"
            };
            saveFileDialog.ShowDialog();
            if (string.IsNullOrWhiteSpace(saveFileDialog.FileName)) return;
            File.WriteAllText(saveFileDialog.FileName, GetSerializedData());
        }

        private bool CanSaveData()
        {
            return _characters.SelectMany(x => x.UnknownSkills).All(x => !x.IsBusy);
        }

        private string GetSerializedData()
        {
            using (var memStm = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(Character[]));
                serializer.WriteObject(memStm, _characters.ToArray());
                memStm.Seek(0, SeekOrigin.Begin);
                using (var streamReader = new StreamReader(memStm))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        #endregion

        #region Load Data Command

        private ICommand _loadDataCommand;

        public ICommand LoadDataCommand
        {
            get
            {
                if (_loadDataCommand == null)
                {
                    _loadDataCommand = new Command(LoadData, CanLoadData);
                }
                return _loadDataCommand;
            }
        }

        private void LoadData()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Bingo Book Data|*.bingobook",
                Title = "Save Data"
            };
            openFileDialog.ShowDialog();
            if (string.IsNullOrWhiteSpace(openFileDialog.FileName)) return;
            var serializedData = File.ReadAllText(openFileDialog.FileName);
            var importedCharacters = DeserializeData(serializedData);

            _characters.Clear();
            importedCharacters.ForEach(c => _characters.Add(c));
        }

        private List<Character> DeserializeData(string xml)
        {
            using (Stream stream = new MemoryStream())
            {
                var data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(typeof(Character[]));
                return ((Character[]) deserializer.ReadObject(stream)).ToList();
            }
        }

        private bool CanLoadData()
        {
            return _characters.SelectMany(x => x.UnknownSkills).All(x => !x.IsBusy);
        }

        #endregion

        #region Open Versus Window Command

        private Command _openVersusWindowCommand;

        public ICommand OpenVersusWindowCommand
        {
            get
            {
                if (_openVersusWindowCommand == null)
                {
                    _openVersusWindowCommand = new Command(OpenVersusWindow);
                }
                return _openVersusWindowCommand;
            }
        }

        private void OpenVersusWindow()
        {
            if (_versusWindow != null)
            {
                _versusWindow.Focus();
                return;
            }

            var versusViewModel = new VersusViewModel(_characters);
            _versusWindow = new VersusWindow(versusViewModel);
            _versusWindow.Show();
            _versusWindow.Closed += (sender, args) => _versusWindow = null;
        }

        #endregion

        #endregion
    }
}
