//using https://github.com/drewnoakes/metadata-extractor-dotnet 
using MetadataExtractor;
//Alias due to conflict
using Directorio = System.IO.Directory;
using System.Globalization;

//used LR structure or Bridge
string Root = @"PATHTORoot";

List<string> Founds = GetFiles(Root);

//printfile doing a csv
using StreamWriter File = new("photoMetadata.csv");

await File.WriteLineAsync("Sequence,Model,CreatedDate");

for (int i = 0; i < Founds.Count; i++)
{
    string? PhotoPath = Founds[i];

    // EXIF Type {272 => Camera Model;306 => CreatedDate}
    await File.WriteLineAsync($"{i},{GetTag(PhotoPath, 272)},{FormatDate(GetTag(PhotoPath, 306))}");

}

//helper method to see all Metadata Showing Directory and Tag incase extra information is required
static void PrintExif(string imagePath)
{
    IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(imagePath);
    foreach (var directory in directories)
        foreach (var tag in directory.Tags)
            Console.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
}

//helper method to convert dates 
static string FormatDate(string input)
{
    DateTime PartialDate = DateTime.ParseExact(input, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
    return PartialDate.ToString("yyyy-MM-dd");

}

//Loads all the file paths you are looking for, this version is setup to look for NEF and RAF files, adjustments might be needed for other types
static List<string> GetFiles(string root)
{
    List<string> Files = new List<string>();

    var result = Directorio.EnumerateFiles(root, "*.NEF",
        SearchOption.AllDirectories).Union(Directorio.EnumerateFiles(root, "*.RAF",
        SearchOption.AllDirectories));

    foreach (var file in result)
    {
        Files.Add(file);
    }

    return Files;
}

//Looks for Exif Information only, to extend a navigation through all the directories in the structure
static string GetTag(string imagepath, int type)
{
    IEnumerable<MetadataExtractor.Directory> info = ImageMetadataReader.ReadMetadata(imagepath);

    MetadataExtractor.Directory exifInfo = info.Where(E => E.Name == "Exif IFD0")
                                            .First();

    if (!exifInfo.IsEmpty)
    {
        var exifTags = exifInfo.Tags;

        var value = exifTags.Where(T => T.Type == type)
                            .Select(T => T.Description)
                            .First();

        if (string.IsNullOrEmpty(value))
            return string.Empty;
        return value.ToString();
    }
    else
        return string.Empty;

}