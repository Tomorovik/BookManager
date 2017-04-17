using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using BookManager.Model;
using BookManager.Utility;
using MessageBox = System.Windows.MessageBox;

namespace BookManager.ViewModel
{
    public class BookSearchViewModel : BindableBase
    {
        public ObservableCollection<string> ExtensionTypes { get; set; } = new ObservableCollection<string>() { "*.pdf", "*.epub", "*.mobi" };
        public ObservableCollection<string> Drives { get; set; }

        private string _selectedExtension;
        public string SelectedExtension
        {
            get { return _selectedExtension; }
            set { SetProperty(ref _selectedExtension, value); }
        }

        private bool _canSearch = true;
        public bool CanSearch
        {
            get { return _canSearch; }
            set
            {
                SetProperty(ref _canSearch, value);
                if (!value)
                {
                    CanCancel = Visibility.Visible;
                    IsVisible = Visibility.Collapsed;
                }
                else
                {
                    CanCancel = Visibility.Collapsed;
                    IsVisible = Visibility.Visible;
                }
            }
        }

        private Visibility _canCancel = Visibility.Collapsed;
        public Visibility CanCancel
        {
            get { return _canCancel; }
            set { SetProperty(ref _canCancel, value); }
        }

        private Visibility _isVisible;
        public Visibility IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        private ObservableCollection<BookFile> _files;
        public ObservableCollection<BookFile> Files
        {
            get { return _files; }
            set { SetProperty(ref _files, value); }
        }

        private ObservableCollection<string> _pdfs;
        public ObservableCollection<string> Pdfs
        {
            get { return _pdfs; }
            set { SetProperty(ref _pdfs, value); }
        }

        private string _selectedLocation;
        public string SelectedLocation
        {
            get { return _selectedLocation; }
            set { SetProperty(ref _selectedLocation, value); }
        }

        private BookFile _selectedItem;
        public BookFile SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        private bool _searchForDuplicates;
        public bool SearchForDuplicates
        {
            get { return _searchForDuplicates; }
            set { SetProperty(ref _searchForDuplicates, value); }
        }

        private bool _deleteDuplicates;
        public bool DeleteDuplicates
        {
            get { return _deleteDuplicates; }
            set { SetProperty(ref _deleteDuplicates, value); }
        }

        private bool _skipBin;
        public bool SkipBin
        {
            get { return _skipBin; }
            set { SetProperty(ref _skipBin, value); }
        }

        public static bool StopSearch = false;

        public RelayCommand SearchCommand { get; set; }
        public RelayCommand SelectCommand { get; set; }
        public RelayCommand SettingsCommand { get; set; }
        public RelayCommand MoveCommand { get; set; }
        public RelayCommand DeleteDuplicatesCommand { get; set; }
        public RelayCommand CancelSearchCommand { get; set; }

        public BookSearchViewModel()
        {
            SelectedExtension = ExtensionTypes.FirstOrDefault();
            Drives = new ObservableCollection<string>(GetAllDrives());
            Drives.Insert(0, "Cały dysk");
            SelectedLocation = Drives.FirstOrDefault();
            SearchCommand = new RelayCommand(OnSearch);
            SelectCommand = new RelayCommand(OnSelect);
            SettingsCommand = new RelayCommand(OnSettings);
            MoveCommand = new RelayCommand(OnMove);
            DeleteDuplicatesCommand = new RelayCommand(OnDeleteDuplicates);
            CancelSearchCommand = new RelayCommand(OnCancelSearch);
        }

        private void OnMove()
        {
            MessageBox.Show("Nie zaimplementowane!");
        }

        private void OnCancelSearch()
        {
            StopSearch = true;
        }

        private void OnDeleteDuplicates()
        {
            var temp = Files.Where(file => file.PublicHash == SelectedItem.PublicHash).Except(new List<BookFile>() { SelectedItem }).ToList();
            foreach (BookFile pdfFile in temp)
            {
                Files.Remove(pdfFile);
                File.Delete(pdfFile.FileName);
            }
        }

        private void OnSettings()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath == "") return;
            Properties.Settings.Default.DefaultFolderLocation = fbd.SelectedPath;
            Properties.Settings.Default.DefaultFolderSet = true;
            Properties.Settings.Default.Save();
        }

        private void OnSelect()
        {
            using (var folder = new FolderBrowserDialog())
            {
                var result = folder.ShowDialog();
                if (result == DialogResult.OK)
                {
                    SelectedLocation = folder.SelectedPath;
                }
            }
        }

        private async void OnSearch()
        {
            Files = new ObservableCollection<BookFile>();
            if (SelectedLocation == null)
            {
                MessageBox.Show("Wybierz lokalizacje");
                return;
            }
            if ("Cały dysk".Equals(SelectedLocation))
            {
                for (int i = 1; i < Drives.Count; i++)
                {
                    if (StopSearch)
                    {
                        StopSearch = false;
                        return;
                    }
                    SelectedLocation = Drives[i];
                    Files = new ObservableCollection<BookFile>(Files.Union(await SearchLocationAsync()));
                }
            }
            else
                Files = new ObservableCollection<BookFile>(await SearchLocationAsync());
            MessageBox.Show("Przeszukiwanie zakończone");
        }

        private async Task<ObservableCollection<BookFile>> SearchLocationAsync()
        {
            CanSearch = false;
            if (StopSearch) StopSearch = false;
            var unsortedFiles = await FileSearch.GetPdfFiles(SelectedLocation, SelectedExtension, SkipBin);

            IOrderedEnumerable<BookFile> sortedFiles;
            if (SearchForDuplicates)
                sortedFiles =
                    unsortedFiles.GroupBy(file => file.PublicHash)
                        .Where(g => g.Count() > 1)
                        .SelectMany(files => files)
                        .OrderBy(file => file.FileName)
                        .ThenBy(file => file.PublicHash);

            else
                sortedFiles = unsortedFiles.OrderBy(file => file.FileName).ThenBy(file => file.PublicHash);
            CanSearch = true;
            return new ObservableCollection<BookFile>(sortedFiles);
        }

        private IEnumerable<string> GetAllDrives() => Environment.GetLogicalDrives();
    }
}
