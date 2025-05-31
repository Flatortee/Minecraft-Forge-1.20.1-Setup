using System.Windows;

namespace MCjava
{
    public partial class Connect : Window
    {
        public string EnteredPath { get; private set; }

        public PathPromptWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            EnteredPath = PathTextBox.Text;
            DialogResult = true;
            Close();
        }
    }
}

}