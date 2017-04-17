using BookManager.ViewModel;
using System.Windows.Controls;

namespace BookManager.View
{
    /// <summary>
    /// Interaction logic for FolderListView.xaml
    /// </summary>
    public partial class FolderListView : UserControl
    {
        public FolderListView()
        {
            this.DataContext = new FolderListViewModel();
            InitializeComponent();
        }
    }
}
