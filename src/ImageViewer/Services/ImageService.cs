using ImageViewer.Models;
using System.IO;

namespace ImageViewer.Services;

public class ImageService
{
    private static readonly string[] SupportedExtensions = 
    {
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".tif"
    };

    public List<ImageInfo> LoadImagesFromFolder(string folderPath)
    {
        var imageFiles = new List<ImageInfo>();

        if (!Directory.Exists(folderPath))
            return imageFiles;

        try
        {
            var files = Directory.GetFiles(folderPath)
                .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .OrderBy(file => Path.GetFileName(file));

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                imageFiles.Add(new ImageInfo
                {
                    FilePath = file,
                    FileName = fileInfo.Name,
                    FileSize = FormatFileSize(fileInfo.Length),
                    DateModified = fileInfo.LastWriteTime,
                    Extension = fileInfo.Extension.ToLowerInvariant()
                });
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading images from folder: {ex.Message}", ex);
        }

        return imageFiles;
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}