using System;
using System.Windows;

namespace BookManager.View.CustomView
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog()
        {
            InitializeComponent();
        }
        public InputDialog(string defaultAnswer = "")
        {
            InitializeComponent();
            fileName.Text = defaultAnswer;
        }

        private void DialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            fileName.SelectAll();
            fileName.Focus();
        }

        public string Input
        {
            get { return fileName.Text; }
        }
    }
}

