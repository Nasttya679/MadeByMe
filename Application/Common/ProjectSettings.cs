namespace MadeByMe.Application.Common;

public class ProjectSettings
{
    public PaginationSettings Pagination { get; set; } = new();

    public FileStorageSettings FileStorage { get; set; } = new();
}

public class PaginationSettings
{
    public int DefaultPageSize { get; set; }

    public int MinSearchLength { get; set; }
}

public class FileStorageSettings
{
    public string UploadFolder { get; set; } = string.Empty;

    public string DefaultImagePath { get; set; } = string.Empty;

    public int MaxImageSizeMB { get; set; }

    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
}