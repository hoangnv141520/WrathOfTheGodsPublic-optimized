using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable
namespace WoTGAssetRebuilder;

internal partial class Program
{
    /// <summary>
    /// A regular expression used to identify numbers at the end of text.
    /// </summary>
    public static readonly Regex EndNumberFinder = new Regex(@"([A-Za-z]+)([\d-]+)$");

    /// <summary>
    /// The set of all number characters.
    /// </summary>
    public static readonly char[] NumberCharacters = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];

    /// <summary>
    /// The text indentation used for lines in the auto-generated file.
    /// </summary>
    public const string Indent = "    ";

    public static void Main(string[] args)
    {
        string assetContentsDirectory = args[0];
        string outputFileName = args[1];
        string relativePath = args[2];

        if (!Directory.Exists(assetContentsDirectory))
        {
            Console.WriteLine($"Directory '{assetContentsDirectory}' does not exist.");
            return;
        }

        List<string> pngPaths = Directory.GetFiles(assetContentsDirectory, "*.png", SearchOption.AllDirectories).ToList();
        List<string> oggPaths = Directory.GetFiles(assetContentsDirectory, "*.ogg", SearchOption.AllDirectories).ToList();
        string assetRoot = Path.GetFullPath(assetContentsDirectory);
        File.WriteAllText(outputFileName, ProcessAssetContents(pngPaths, oggPaths, Path.GetFileNameWithoutExtension(outputFileName), relativePath, assetRoot));

        Console.WriteLine("Asset file generated successfully.");
    }

    /// <summary>
    /// Builds the tModLoader content path: {modInternalName}/Assets/{path relative to the Assets folder on disk}.
    /// </summary>
    private static string GetModContentPath(string fullPath, string assetsDirectoryFullPath, string modInternalName)
    {
        string relative = Path.GetRelativePath(assetsDirectoryFullPath, Path.GetFullPath(fullPath));
        return $"{modInternalName}/Assets/{relative}".Replace('\\', '/');
    }

    /// <summary>
    /// Sanitizes a given asset name for use as a member name, ensuring compliance with C# compilation rules.
    /// </summary>
    /// <param name="assetName">The raw asset name.</param>
    private static string SanitizeMemberName(string assetName) => assetName.Replace('-', '_').Replace(' ', '_').Replace("_ImmediateLoad", string.Empty);

    /// <summary>
    /// Processes asset contents, generating a .cs file that contains code referencing said assets.
    /// </summary>
    /// <param name="pngPaths">The set of all found .PNG files to convert to texture asset references.</param>
    /// <param name="oggPaths">The set of all found .OGG files to convert to sound asset references.</param>
    /// <param name="className">The name of the class that should be generated.</param>
    /// <param name="relativePath">The relative path for the mod.</param>
    private static string ProcessAssetContents(List<string> pngPaths, List<string> oggPaths, string className, string modInternalName, string assetsDirectoryFullPath)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("using Microsoft.Xna.Framework.Graphics;");
        sb.AppendLine("using System.CodeDom.Compiler;");
        sb.AppendLine("using ReLogic.Content;");
        sb.AppendLine("using Terraria.Audio;");
        sb.AppendLine();
        sb.AppendLine("namespace NoxusBoss.Assets;");
        sb.AppendLine();

        AssemblyName assemblyName = Assembly.GetCallingAssembly().GetName();
        string toolHeader = assemblyName.Name ?? "Unknown";
        string versionHeader = assemblyName.Version?.ToString() ?? "1.0.0.0";
        sb.AppendLine($"[GeneratedCode(\"{toolHeader}\", \"{versionHeader}\")]");
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");

        ProcessTextureContents(sb, pngPaths, modInternalName, assetsDirectoryFullPath);
        ProcessSoundContents(sb, oggPaths, modInternalName, assetsDirectoryFullPath);

        sb.AppendLine("}");

        return sb.ToString();
    }

    [GeneratedCode("WoTGAssetBuilder", "a")]
    private static void ProcessTextureContents(StringBuilder sb, List<string> pngPaths, string modInternalName, string assetsDirectoryFullPath)
    {
        sb.AppendLine($"{Indent}public class Textures");
        sb.AppendLine($"{Indent}{{");

        var pathGroups = pngPaths.GroupBy(p => Directory.GetParent(p)!.Name).ToList();
        foreach (var group in pathGroups.OrderBy(g => g.Key))
        {
            sb.AppendLine($"{Indent}{Indent}public class {group.Key}");
            sb.AppendLine($"{Indent}{Indent}{{");

            foreach (string path in group.OrderBy(p => p))
            {
                string assetName = Path.GetFileNameWithoutExtension(path);
                string pathRelativeToSource = GetModContentPath(path, assetsDirectoryFullPath, modInternalName).Replace(".png", string.Empty);

                bool immediateLoad = path.Contains("_ImmediateLoad");
                string loadContext = immediateLoad ? ", AssetRequestMode.ImmediateLoad" : string.Empty;

                sb.AppendLine($"{Indent}{Indent}{Indent}public static readonly LazyAsset<Texture2D> {SanitizeMemberName(assetName)} = LazyAsset<Texture2D>.FromPath(\"{pathRelativeToSource}\"{loadContext});");
            }

            sb.AppendLine($"{Indent}{Indent}}}");
            sb.AppendLine();
        }
        sb.AppendLine($"{Indent}}}");
    }

    private static void ProcessSoundContents(StringBuilder sb, List<string> oggPaths, string modInternalName, string assetsDirectoryFullPath)
    {
        sb.AppendLine($"{Indent}public class Sounds");
        sb.AppendLine($"{Indent}{{");

        HashSet<string> createdAssets = [];

        var pathGroups = oggPaths.GroupBy(p => Directory.GetParent(p)!.Name).ToList();
        foreach (var group in pathGroups.OrderBy(g => g.Key))
        {
            // Ignore music files.
            if (group.Key == "Music")
                continue;

            sb.AppendLine($"{Indent}{Indent}public class {group.Key}");
            sb.AppendLine($"{Indent}{Indent}{{");

            foreach (string path in group.OrderBy(p => p))
            {
                string assetName = Path.GetFileNameWithoutExtension(path);
                string pathRelativeToSource = GetModContentPath(path, assetsDirectoryFullPath, modInternalName).Replace(".ogg", string.Empty);

                // If the name contains a number suffix, use syntax to indicate that multiple choosable sounds can play.
                string variantSyntax = string.Empty;
                Match numberMatch = EndNumberFinder.Match(assetName);
                if (numberMatch.Success)
                {
                    assetName = numberMatch.Groups[1].Value;

                    pathRelativeToSource = pathRelativeToSource.TrimEnd(NumberCharacters);
                    variantSyntax = GenerateSoundVariantSuffix(oggPaths, assetName);
                }

                // Duplicates can appear as a result of processing numerically suffixed sounds.
                // Ignore them.
                if (createdAssets.Contains(assetName))
                    continue;

                sb.AppendLine($"{Indent}{Indent}{Indent}public static readonly SoundStyle {SanitizeMemberName(assetName)} = new SoundStyle(\"{pathRelativeToSource}\"{variantSyntax});");
                createdAssets.Add(assetName);
            }

            sb.AppendLine($"{Indent}{Indent}}}");
            sb.AppendLine();
        }
        sb.AppendLine($"{Indent}}}");
    }

    /// <summary>
    /// Generates an optional ", N" suffix for a given sound, thus specifying the amount of variants that exist, by examining other paths with the same sound prefix.
    /// </summary>
    /// <param name="oggPaths">The set of .OGG file paths to examine.</param>
    /// <param name="assetName">The name of the asset that's being suffixed.</param>
    private static string GenerateSoundVariantSuffix(List<string> oggPaths, string assetName)
    {
        int totalVariants = 1;
        foreach (string path in oggPaths)
        {
            if (!path.Contains(assetName))
                continue;

            Match numberResult = EndNumberFinder.Match(Path.GetFileNameWithoutExtension(path));
            if (numberResult.Success)
                totalVariants = Math.Max(totalVariants, int.Parse(numberResult.Groups[2].Value));
        }

        return $", {totalVariants}";
    }
}

#pragma warning restore
