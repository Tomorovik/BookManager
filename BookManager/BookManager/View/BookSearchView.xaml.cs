using BookManager.ViewModel;
using System.Windows.Controls;

namespace BookManager.View
{
    /// <summary>
    /// Interaction logic for BookSearchView.xaml
    /// </summary>
    public partial class BookSearchView : UserControl
    {
        public BookSearchView()
        {
            this.DataContext = new BookSearchViewModel();
            InitializeComponent();
        }
    }
}
