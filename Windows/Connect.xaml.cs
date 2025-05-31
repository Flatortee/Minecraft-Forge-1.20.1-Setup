using System.Windows;
using System.Windows.Forms;

namespace MCjava.Windows
{
    /// <summary>
    /// Fenêtre permettant à l'utilisateur de sélectionner un dossier de destination.
    /// </summary>
    public partial class Connect : Window
    {
        /// <summary>
        /// Chemin sélectionné par l'utilisateur, accessible après validation.
        /// </summary>
        public string EnteredPath { get; private set; }

        /// <summary>
        /// Constructeur de la fenêtre Connect.
        /// </summary>
        public Connect()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Ouvre une boîte de dialogue pour parcourir et sélectionner un dossier.
        /// Le chemin choisi est affiché dans la TextBox PathTextBox.
        /// </summary>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Sélectionnez un dossier de destination";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    PathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        /// <summary>
        /// Valide le chemin saisi ou sélectionné, ferme la fenêtre et retourne le résultat.
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            EnteredPath = PathTextBox.Text;
            DialogResult = true;
            Close();
        }
    }
}
