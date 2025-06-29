namespace ImageViewer.Models;

public class ImageInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public DateTime DateModified { get; set; }
    public string Extension { get; set; } = string.Empty;
}