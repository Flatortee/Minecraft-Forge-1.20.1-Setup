using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;

namespace MCjava.Models
{
    internal static class FileInstaller
    {
        // Chemins des fichiers de configuration et d'archive ZIP
        private static string configPath = "config.json";
        private static string zipFilePath = "mdk.zip";
        private static string newPackagePath;
        private static string savedPath;

        // Informations du mod à injecter dans les fichiers
        private static string modid;
        private static string modname;
        private static string license;
        private static string authors;
        private static string description;

        /// <summary>
        /// Vérifie si le chemin de sauvegarde est défini dans le fichier de configuration.
        /// Affiche un message d'erreur si ce n'est pas le cas.
        /// </summary>
        public static void CheckSavedPath()
        {
            string configPath = "config.json";
            string savedPath = null;

            var json = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            config.TryGetValue("path", out savedPath);

            if (string.IsNullOrEmpty(savedPath))
            {
                MessageBox.Show("Le chemin n'est pas défini. Veuillez le configurer d'abord.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        /// <summary>
        /// Initialise les chemins principaux et vérifie l'existence du dossier Java.
        /// </summary>
        public static void Init(string _savedPath)
        {
            string javaRoot = Path.Combine(_savedPath, "src", "main", "java");
            if (!Directory.Exists(javaRoot))
            {
                MessageBox.Show("Le chemin Java n'existe pas.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            configPath = "config.json";
            zipFilePath = "mdk.zip";
            savedPath = _savedPath;
        }

        /// <summary>
        /// Récupère les informations saisies par l'utilisateur pour le mod.
        /// </summary>
        public static void GetTextBoxInfo(string _modid, string _modname, string _license, string _authors, string _description)
        {
            modid = _modid;
            modname = _modname;
            license = _license;
            authors = _authors;
            description = _description;
        }

        /// <summary>
        /// Installe les fichiers nécessaires à partir de l'archive ZIP et copie les fichiers supplémentaires.
        /// </summary>
        public static void InstallFiles()
        {
            CheckSavedPath();

            // Récupère le chemin de sauvegarde depuis la config
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                config.TryGetValue("path", out savedPath);
            }

            // Vérifie la validité des chemins et des informations du mod
            if (string.IsNullOrEmpty(savedPath))
            {
                MessageBox.Show("Aucun chemin de destination n'est défini.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(zipFilePath))
            {
                MessageBox.Show("Le fichier ZIP n'existe pas.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(modid) || string.IsNullOrEmpty(modname))
            {
                MessageBox.Show("Veuillez remplir au moins le mod id et le mod name.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Extraction de l'archive ZIP dans le dossier de destination
            ZipFile.ExtractToDirectory(zipFilePath, savedPath);

            // Liste des fichiers à copier après extraction
            List<string> filesToCopy = new List<string>
            {
                "build.bat"
                // Ajoute ici d'autres chemins relatifs si besoin
            };

            foreach (var relativeFile in filesToCopy)
            {
                string destFile = Path.Combine(savedPath, relativeFile);

                if (!File.Exists(relativeFile))
                {
                    MessageBox.Show($"Le fichier '{relativeFile}' est introuvable après extraction.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }

                File.Copy(relativeFile, destFile, true);
            }
        }

        /// <summary>
        /// Crée un nouveau package Java pour le mod et déplace les fichiers .java dans ce package.
        /// </summary>
        public static void NewPackage()
        {
            string javaRoot = Path.Combine(savedPath, "src", "main", "java");
            newPackagePath = Path.Combine(javaRoot, "com", modid.ToLower(), modname.ToLower());

            Directory.CreateDirectory(newPackagePath);

            // Recherche l'ancien package Java (exemple : com/example/examplemod)
            var oldPackagePath = Directory.GetDirectories(javaRoot, "*", SearchOption.AllDirectories)
                .FirstOrDefault(d => d.EndsWith("com") || d.Contains(@"\com\"));
            if (oldPackagePath == null)
            {
                MessageBox.Show("Dossier de package Java introuvable.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var javaFiles = Directory.GetFiles(oldPackagePath, "*.java", SearchOption.AllDirectories);

            foreach (var javaFile in javaFiles)
            {
                // Met à jour la déclaration du package dans chaque fichier .java
                var javaLines = File.ReadAllLines(javaFile);
                for (int i = 0; i < javaLines.Length; i++)
                {
                    if (javaLines[i].StartsWith("package "))
                    {
                        javaLines[i] = $"package com.{modid.ToLower()}.{modname.ToLower()};";
                    }
                }
                File.WriteAllLines(javaFile, javaLines);

                // Déplace le fichier dans le nouveau dossier de package
                string newFilePath = Path.Combine(newPackagePath, Path.GetFileName(javaFile));
                File.Move(javaFile, newFilePath);
            }

            // Supprime l'ancien dossier de package si nécessaire
            Directory.Delete(oldPackagePath + "/example", true);
        }

        /// <summary>
        /// Modifie le fichier gradle.properties avec les informations du mod.
        /// </summary>
        public static void ModifyGradleProperties()
        {
            string gradlePropsPath = Path.Combine(savedPath, "gradle.properties");
            if (File.Exists(gradlePropsPath))
            {
                var lines = File.ReadAllLines(gradlePropsPath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("mod_id="))
                        lines[i] = $"mod_id={modid}";
                    else if (lines[i].StartsWith("mod_name="))
                        lines[i] = $"mod_name={modname}";
                    else if (lines[i].StartsWith("mod_license="))
                        lines[i] = $"mod_license={license}";
                    else if (lines[i].StartsWith("mod_group_id="))
                        lines[i] = $"mod_group_id=com.{modid}.{modname}";
                    else if (lines[i].StartsWith("mod_authors="))
                        lines[i] = $"mod_authors={authors}";
                    else if (lines[i].StartsWith("mod_description="))
                        lines[i] = $"mod_description={description}";
                }
                File.WriteAllLines(gradlePropsPath, lines);
            }
        }

        /// <summary>
        /// Modifie le fichier Config.java pour remplacer le nom du mod dans l'annotation EventBusSubscriber.
        /// </summary>
        public static void ModifyConfigJava()
        {
            var configJava = Directory.GetFiles(newPackagePath, "Config.java", SearchOption.AllDirectories).FirstOrDefault();
            if (configJava != null)
            {
                var configLines = File.ReadAllLines(configJava);
                for (int i = 0; i < configLines.Length; i++)
                {
                    if (configLines[i].Contains("@Mod.EventBusSubscriber("))
                    {
                        // Remplace ExampleMod par le nom du mod
                        configLines[i] = configLines[i].Replace("ExampleMod", modname);
                    }
                }
                File.WriteAllLines(configJava, configLines);
            }
        }

        /// <summary>
        /// Modifie le fichier ExampleMod.java pour adapter le nom de la classe, le constructeur et le MODID.
        /// Renomme également le fichier selon le nom du mod.
        /// </summary>
        public static void ModifyMainJava()
        {
            var exampleModJava = Directory.GetFiles(newPackagePath, "ExampleMod.java", SearchOption.AllDirectories).FirstOrDefault();
            if (exampleModJava != null)
            {
                var exampleLines = File.ReadAllLines(exampleModJava);
                for (int i = 0; i < exampleLines.Length; i++)
                {
                    if (exampleLines[i].Contains("@Mod(ExampleMod.MODID)"))
                    {
                        exampleLines[i] = exampleLines[i].Replace("@Mod(ExampleMod.MODID)", $"@Mod({modname}.MODID)");
                    }
                    if (exampleLines[i].Contains("public class ExampleMod"))
                    {
                        exampleLines[i] = exampleLines[i].Replace("public class ExampleMod", $"public class {modname}");
                    }
                    if (exampleLines[i].Contains("public ExampleMod(FMLJavaModLoadingContext context)"))
                    {
                        exampleLines[i] = exampleLines[i].Replace("public ExampleMod(FMLJavaModLoadingContext context)", $"public {modname}(FMLJavaModLoadingContext context)");
                    }
                    if (exampleLines[i].Contains("public static final String MODID"))
                    {
                        exampleLines[i] = $"    public static final String MODID = \"{modid}\";";
                    }
                }
                File.WriteAllLines(exampleModJava, exampleLines);

                // Renomme le fichier ExampleMod.java en <modname>.java
                string newExampleModPath = Path.Combine(Path.GetDirectoryName(exampleModJava), $"{modname}.java");
                if (!File.Exists(newExampleModPath))
                {
                    File.Move(exampleModJava, newExampleModPath);
                }
            }
        }
    }
}
