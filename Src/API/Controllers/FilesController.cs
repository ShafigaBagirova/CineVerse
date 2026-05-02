using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IMinioClient minioClient,
        ILogger<FilesController> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
    }

    [HttpGet("{bucket}/{**objectName}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFile(
        [FromRoute] string bucket,
        [FromRoute] string objectName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(bucket) || string.IsNullOrWhiteSpace(objectName))
            return BadRequest("Bucket and object name are required.");

        var normalizedBucket = bucket.Trim().Trim('/');
        var normalizedObject = objectName.Trim().TrimStart('/');

        try
        {
            byte[] fileBytes;

            await using (var memoryStream = new MemoryStream())
            {
                await _minioClient.GetObjectAsync(
                    new GetObjectArgs()
                        .WithBucket(normalizedBucket)
                        .WithObject(normalizedObject)
                        .WithCallbackStream(async stream =>
                        {
                            await stream.CopyToAsync(memoryStream, cancellationToken);
                        }),
                    cancellationToken);

                fileBytes = memoryStream.ToArray();
            }

            if (fileBytes.Length == 0)
                return NotFound();

            var contentType = GetContentType(normalizedObject);
            return File(fileBytes, contentType);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "File request cancelled. Bucket: {Bucket}, ObjectName: {ObjectName}",
                normalizedBucket,
                normalizedObject);
            return new EmptyResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to get file. Bucket: {Bucket}, ObjectName: {ObjectName}",
                normalizedBucket,
                normalizedObject);
            return NotFound();
        }
    }

    private static string GetContentType(string objectName)
    {
        var ext = Path.GetExtension(objectName)?.ToLowerInvariant();
        return ext switch
        {
            ".webp" => "image/webp",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
