namespace RecruitmentAPI.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Save a file to the configured storage folder.
    /// Returns the relative path to the saved file.
    /// </summary>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string subFolder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file by its relative path.
    /// </summary>
    Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a file exists at the given relative path.
    /// </summary>
    bool FileExists(string relativePath);

    /// <summary>
    /// Returns the absolute physical path for a given relative path.
    /// </summary>
    string GetAbsolutePath(string relativePath);
}
