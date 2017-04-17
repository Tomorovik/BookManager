using BookManager.Model;
using BookManager.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace BookManager.ViewModel
{
    public class FolderListViewModel : BindableBase
    {
        public RelayCommand MoveUpCommand { get; set; }
        public RelayCommand NewFolderCommand { get; set; }
        public RelayCommand DesktopCommand { get; set; }
        public RelayCommand FavoriteCommand { get; set; }
        public RelayCommand RenameItemCommand { get; set; }
        public RelayCommand DeleteFolderCommand { get; set; }
        public RelayCommand MouseDoubleClickCommand { get; set; }
        public ObservableCollection<string> DrivesList { get; set; }

        private bool _canGoUp;
        public bool CanGoUp
        {
            get { return _canGoUp; }
            set { SetProperty(ref _canGoUp, value); }
        }

        private bool _canGoDown;
        public bool CanGoDown
        {
            get { return _canGoDown; }
            set { SetProperty(ref _canGoDown, value); }
        }


        private string _selectedDrive;
        public string SelectedDrive
        {
            get { return _selectedDrive; }
            set
            {
                if (value.Equals("Biblioteka książek"))
                    if (!Properties.Settings.Default.DefaultFolderSet)
                        MessageBox.Show("Nie ustalono domyślnej lokalizacji!");
                    else
                        SetProperty(ref _selectedDrive, Properties.Settings.Default.DefaultFolderLocation);
                else
                    SetProperty(ref _selectedDrive, value);
                GetDirectoriesAndFiles(ref _selectedLocationItems, _selectedDrive);
            }
        }

        public static string _selectedLocation;
        public string SelectedLocation
        {
            get { return _selectedLocation; }
            set { SetProperty(ref _selectedLocation, value); }
        }

        private DirectoryItem _selectedItem;
        public DirectoryItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                Type = value.Type;
                SetProperty(ref _selectedItem, value);
            }
        }

        private int _selectedItemPosition;
        public int SelectedItemPosition
        {
            get { return _selectedItemPosition; }
            set
            {
                SetProperty(ref _selectedItemPosition, value);
            }
        }

        public static ObservableCollection<DirectoryItem> _selectedLocationItems;
        public ObservableCollection<DirectoryItem> SelectedLocationItems
        {
            get { return _selectedLocationItems; }
            set
            {
                SetProperty(ref _selectedLocationItems, value);
            }
        }

        private DirectoryItemType _type;
        public DirectoryItemType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        public FolderListViewModel()
        {
            DrivesList = new ObservableCollection<string>() { "Biblioteka książek" };
            DrivesList = new ObservableCollection<string>(DrivesList.Union(GetAllDrives()));
            SelectedLocationItems = new ObservableCollection<DirectoryItem>();
            SelectedLocation = DrivesList[1];
            SelectedDrive = DrivesList[1];
            MoveUpCommand = new RelayCommand(OnMoveUp);
            NewFolderCommand = new RelayCommand(OnNewFolder);
            DesktopCommand = new RelayCommand(OnDesktop);
            FavoriteCommand = new RelayCommand(OnFavorite);
            RenameItemCommand = new RelayCommand(OnRenameItem);
            DeleteFolderCommand = new RelayCommand(OnDeleteFolder);
            MouseDoubleClickCommand = new RelayCommand(OnMouseDoubleClick);
        }

        private void OnMouseDoubleClick()
        {
            try
            {
                if (Type == DirectoryItemType.File) return;
                if (CanGoDown)
                {
                    SelectedLocation = SelectedItem.Name;
                    GetDirectoriesAndFiles(ref _selectedLocationItems, SelectedLocation);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show("Brak dostepu do folderu");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void OnRenameItem()
        {
            View.CustomView.InputDialog id = new View.CustomView.InputDialog();
            if (id.ShowDialog() == true)
            {
                if (id.DialogResult == true)
                {
                    var temp = Path.Combine(SelectedLocation, id.Input);
                    if (Type == DirectoryItemType.File)
                        File.Move(SelectedItem.Name, temp);
                    else
                        Directory.Move(SelectedItem.Name, temp);
                    GetDirectoriesAndFiles(ref _selectedLocationItems, SelectedLocation);
                }
            }
        }

        private void OnDeleteFolder()
        {
            if (Directory.Exists(SelectedItem.Name))
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this?", "Confirm deletion", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Directory.Delete(SelectedItem.Name);
                }
            }
            GetDirectoriesAndFiles(ref _selectedLocationItems, SelectedLocation);
        }

        private void OnFavorite()
        {
            if (!Properties.Settings.Default.DefaultFolderSet) return;
            SelectedLocation = Properties.Settings.Default.DefaultFolderLocation;
            GetDirectoriesAndFiles(ref _selectedLocationItems, SelectedLocation);
        }

        private void GetDirectoriesAndFiles(ref ObservableCollection<DirectoryItem> observableCollection, string location)
        {
            var dirs = Directory
                .GetDirectories(location)
                .Select(s => new DirectoryItem()
                {
                    Type = DirectoryItemType.Directory,
                    Name = s,
                }).ToList();
            var files = Directory.GetFiles(location).Select(s => new DirectoryItem()
            {
                Type = DirectoryItemType.File,
                Name = s
            });
            observableCollection = new ObservableCollection<DirectoryItem>(dirs.Union(files));
            OnPropertyChanged(nameof(SelectedLocationItems));
            CanGoUp = Directory.GetParent(SelectedLocation) != null;
            CanGoDown = Directory.GetDirectories(SelectedLocation).Length != 0;
        }

        private void OnDesktop()
        {
            SelectedLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            GetDirectoriesAndFiles(ref _selectedLocationItems, SelectedLocation);
            OnPropertyChanged(nameof(SelectedLocationItems));
        }

        private void OnNewFolder()
        {
            string newFolderLocation = Path.Combine(SelectedLocation, "New Folder");
            if (Directory.Exists(newFolderLocation))
            {
                Regex regex = new Regex("^New Folder[(\\d)]*$");
                int folderCount = Directory.GetDirectories(SelectedLocation).Count(s => regex.IsMatch(Path.GetFileName(s)));
                newFolderLocation = String.Format(newFolderLocation + $"({folderCount})");
            }
            Directory.CreateDirectory(newFolderLocation);
            GetDirectoriesAndFiles(ref _selectedLocationItems, SelectedLocation);
        }

        private void OnMoveUp()
        {
            if (!CanGoUp) return;
            SelectedLocation = Directory.GetParent(SelectedLocation).FullName;
            GetDirectoriesAndFiles(ref _selectedLocationItems, SelectedLocation);
        }




        private IEnumerable<string> GetAllDrives() => Environment.GetLogicalDrives();

    }
}
