namespace MicroJack.API.Models.Update;

public class UpdateRequest
{
    public string Version { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;
    public bool ForceRestart { get; set; } = true;
}
