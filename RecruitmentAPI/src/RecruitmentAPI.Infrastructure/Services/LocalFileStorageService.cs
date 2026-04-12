using Microsoft.Extensions.Configuration;
using RecruitmentAPI.Application.Common.Interfaces;

namespace RecruitmentAPI.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _rootPath = configuration["FileStorage:RootPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        // Ensure root exists on startup
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subFolder, CancellationToken cancellationToken = default)
    {
        // Sanitize the original filename — keep extension only, generate a unique name
        var extension = Path.GetExtension(fileName);
        var safeFileName = $"{Guid.NewGuid()}{extension}";

        var folderPath = Path.Combine(_rootPath, subFolder);
        Directory.CreateDirectory(folderPath);

        var absolutePath = Path.Combine(folderPath, safeFileName);

        await using var fs = new FileStream(absolutePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(fs, cancellationToken);

        // Return relative path (subFolder/fileName) — portable across environments
        return Path.Combine(subFolder, safeFileName).Replace("\\", "/");
    }

    public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        return Task.CompletedTask;
    }

    public bool FileExists(string relativePath)
        => File.Exists(GetAbsolutePath(relativePath));

    public string GetAbsolutePath(string relativePath)
        => Path.Combine(_rootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
}
