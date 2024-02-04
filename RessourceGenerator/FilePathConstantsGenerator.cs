using System;
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
        var classBuilder = new StringBuilder(@"
using System;

namespace Generated
{
    public static class FilePathConstants
    {
");

        foreach (var file in context.AdditionalFiles)
        {
            var filePath = file.Path.Replace("\\", "\\\\"); // Échappe les backslashes pour le code généré
            var relativePath = GetRelativePath(filePath, "Assets");
            var fieldName = SanitizeIdentifier(relativePath);

            classBuilder.AppendLine($"        public const string {fieldName} = @\"{filePath}\";");
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
            var relativeSegments = segments.Skip(afterFolderIndex + 1).ToArray();
            return Path.Combine(relativeSegments).Replace(Path.DirectorySeparatorChar, '_');
        }
        return Path.GetFileNameWithoutExtension(fullPath);
    }

    private string SanitizeIdentifier(string path)
    {
        // Remplace les caractères non valides pour un identifiant C# par des underscores et assure que chaque segment est capitalisé
        return string.Concat(path.Split(new[] { '_', Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)))
                      .Replace("-", "_")
                      .Replace(" ", "_")
                      .Replace(".", "_");
    }
    }
}