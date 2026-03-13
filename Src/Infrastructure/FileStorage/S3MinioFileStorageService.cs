using Application.Common.Interfaces;
using Application.Common.Options;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Infrastructure.FileStorage;

public sealed class S3MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioOptions _options;

    public S3MinioFileStorageService(
        IMinioClient minioClient,
        IOptions<MinioOptions> options)
    {
        _minioClient = minioClient;
        _options = options.Value;
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
         int mediaId,
        CancellationToken ct = default)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty.", nameof(fileName));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type cannot be empty.", nameof(contentType));

        var bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_options.Bucket),
            ct);

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_options.Bucket),
                ct);
        }

        if (content.CanSeek)
            content.Position = 0;

        var extension = Path.GetExtension(fileName);
        var objectKey = $"{Guid.NewGuid()}{extension}";

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_options.Bucket)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs, ct);

        return objectKey;
    }

    public async Task DeleteFileAsync(string objectKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new ArgumentException("Object key cannot be empty.", nameof(objectKey));

        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(_options.Bucket)
            .WithObject(objectKey);

        await _minioClient.RemoveObjectAsync(removeObjectArgs, ct);
    }
}