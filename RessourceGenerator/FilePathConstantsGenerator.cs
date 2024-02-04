using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RessourceGenerator
{
    [Generator]
    public class FilePathConstantsGenerator  : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Pas nécessaire pour cet exemple
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var filesByDirectory = new Dictionary<string, List<string>>();

            // Organiser les fichiers par dossier
            foreach (var file in context.AdditionalFiles)
            {
                var directoryPath = Path.GetDirectoryName(file.Path);
                if (directoryPath != null)
                {
                    if (!filesByDirectory.ContainsKey(directoryPath))
                    {
                        filesByDirectory[directoryPath] = new List<string>();
                    }
                    filesByDirectory[directoryPath].Add(file.Path);
                }
            }

            var classBuilder = new StringBuilder(@"
using System;

namespace Generated
{
    public static class FilePathConstants
    {
");
            // Générer des sous-classes pour chaque dossier
            foreach (var directoryEntry in filesByDirectory)
            {
                var directoryName = Path.GetFileName(directoryEntry.Key);
                var className = SanitizeIdentifier(directoryName);
                var directoryPath = GetRelativePath(directoryEntry.Key, "Assets").Replace("\\", "\\\\"); // Échappe les backslashes pour le code généré
                classBuilder.AppendLine($"        public static class {className}");
                classBuilder.AppendLine("        {");
                // Ajouter un champ pour le chemin du dossier
                classBuilder.AppendLine($"            public const string DirectoryPath = @\"{directoryPath}\";");


                foreach (var file in directoryEntry.Value)
                {
                    var fileName = Path.GetFileName(file);
                    var fieldName = SanitizeIdentifier(fileName);
                    var relativePath = GetRelativePath(file, "Assets");
                    var filePath = relativePath.Replace("\\", "\\\\"); // Échappe les backslashes pour le code généré
                    classBuilder.AppendLine($"            public const string {fieldName} = @\"{filePath}\";");
                }

                classBuilder.AppendLine("        }");
            }

            classBuilder.Append(@"
    }
}
");

            context.AddSource("FilePathConstants", SourceText.From(classBuilder.ToString(), Encoding.UTF8));
        }
        
        private string GetRelativePath(string fullPath, string afterFolder)
        {
            var segments = fullPath.Split(Path.DirectorySeparatorChar);
            var afterFolderIndex = Array.IndexOf(segments, afterFolder);
            if (afterFolderIndex >= 0 && afterFolderIndex < segments.Length - 1)
            {
                // Construit un chemin relatif à partir des segments après le dossier spécifié
                var relativeSegments = segments.Skip(afterFolderIndex).ToList();
                relativeSegments.Insert(0,".\\");
                return Path.Combine(relativeSegments.ToArray());
            }
            return Path.GetFileNameWithoutExtension(fullPath);
        }

        private string SanitizeIdentifier(string identifier)
        {
            // Assurez-vous que l'identifiant est valide en C#
            return identifier.Replace("-", "_")
                .Replace(" ", "_")
                .Replace(".", "_")
                .Replace("\\", "_")
                .Replace("/", "_");
        }
    }
}