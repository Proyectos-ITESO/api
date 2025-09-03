namespace MicroJack.API.Models.Update;

public class UpdateInfo
{
    public string Version { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;
    public List<string> Changelog { get; set; } = new();
    public bool IsRequired { get; set; }
    public string MinimumVersion { get; set; } = string.Empty;
}
