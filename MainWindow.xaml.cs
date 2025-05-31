using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using MCjava.Windows;
using MCjava.Models;

namespace MCjava
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Constructeur principal de la fenêtre.
        /// Gère la récupération ou la demande du chemin d'installation au lancement.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            string configPath = "config.json";
            string savedPath = null;

            // Vérifie si le chemin est déjà enregistré dans le fichier de configuration
            if (File.Exists(configPath))
            {
                // Lire le chemin depuis le JSON
                var json = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                config.TryGetValue("path", out savedPath);
            }

            // Si aucun chemin n'est enregistré, demande à l'utilisateur de le renseigner
            if (string.IsNullOrEmpty(savedPath))
            {
                // Première connexion : demander le chemin
                var prompt = new Connect();
                if (prompt.ShowDialog() == true)
                {
                    savedPath = prompt.EnteredPath;
                    var config = new Dictionary<string, string> { { "path", savedPath } };
                    File.WriteAllText(configPath, JsonConvert.SerializeObject(config));
                }
                else
                {
                    // L'utilisateur a annulé, fermer l'application
                    Application.Current.Shutdown();
                }
            }

            // Initialise le FileInstaller avec le chemin choisi
            FileInstaller.Init(savedPath);
        }

        /// <summary>
        /// Permet à l'utilisateur de réinitialiser le chemin d'installation.
        /// Supprime l'ancien chemin et enregistre le nouveau.
        /// </summary>
        private void buttonResetPath_Click(object sender, RoutedEventArgs e)
        {
            string configPath = "config.json";
            if (File.Exists(configPath))
            {
                File.Delete(configPath);
            }
            // Ouvre la fenêtre de sélection de chemin
            var prompt = new Connect();
            if (prompt.ShowDialog() == true)
            {
                string savedPath = prompt.EnteredPath;
                var config = new Dictionary<string, string> { { "path", savedPath } };
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config));
                MessageBox.Show("Chemin réinitialisé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Lance la création et la personnalisation du mod à partir des informations saisies.
        /// </summary>
        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            string configPath = "config.json";
            string savedPath = null;

            // Récupère le chemin d'installation depuis le fichier de configuration
            var json = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            config.TryGetValue("path", out savedPath);

            if (string.IsNullOrEmpty(savedPath))
            {
                MessageBox.Show("Le chemin n'est pas défini. Veuillez le configurer d'abord.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Récupère les informations saisies dans les TextBox et les transmet à FileInstaller
            FileInstaller.GetTextBoxInfo(
                modidTextBox.Text.Trim(),
                nameTextBox.Text.Trim(),
                licenseTextBox.Text.Trim(),
                authorsTextBox.Text.Trim(),
                descriptionTextBox.Text.Trim()
            );

            try
            {
                // Exécute les différentes étapes d'installation et de personnalisation du mod
                FileInstaller.InstallFiles();
                FileInstaller.NewPackage();
                FileInstaller.ModifyGradleProperties();
                FileInstaller.ModifyConfigJava();
                FileInstaller.ModifyMainJava();

                // Affiche un message de succès à la fin
                MessageBox.Show("Fichiers installés avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Affiche un message d'erreur en cas de problème
                MessageBox.Show($"Erreur lors de l'installation des fichiers : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
