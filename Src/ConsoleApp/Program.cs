// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
checkDictionary();

static void checkDictionary() {
Dictionary<string, Dictionary<string, List<string>>> arTypeExtensions = new() {
    {"Tar", new() { {"GZip" , new () {".tar", ".tgz"}}}},
    {"GZip", new() { { "GZip", new() { ".gzip", ".gz" } } }},
    //{ArchiveType.SevenZip, new() { { CompressionType.GZip, new() { ".7z", ".7zip" } } } },    // not supported yet
    {"Zip", new() {
        { "Deflate", new() { ".zip" } },
        { "BZip2", new() { ".bz2", "bzip2"} },
        { "LZMA", new() { ".lzm", ".lz" } },
        { "PPMd", new() { ".pzip", ".pz" } }
    } }
};
string ext = "gz";
var foundType = arTypeExtensions.FirstOrDefault(e => e.Value.Any(c => c.Value.Contains(ext)));
string? fndArchiveType = foundType.Key;
Console.Write(fndArchiveType);
}