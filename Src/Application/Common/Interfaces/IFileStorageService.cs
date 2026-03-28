namespace Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(
            Stream content,
            string fileName,
            string contentType,
            string folder,
            CancellationToken ct = default);

    Task DeleteFileAsync(
        string objectKey,
        CancellationToken ct = default);
}
