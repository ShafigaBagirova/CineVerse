using Application.Common.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IMinioClient _minioClient;
    private readonly MinioOptions _minioOptions;

    public FilesController(IMinioClient minioClient, IOptions<MinioOptions> minioOptions)
    {
        _minioClient = minioClient;
        _minioOptions = minioOptions.Value;
    }

    [HttpGet("{bucket}/{**objectName}")]
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
            var statArgs = new StatObjectArgs()
                .WithBucket(normalizedBucket)
                .WithObject(normalizedObject);

            var stat = await _minioClient.StatObjectAsync(statArgs, cancellationToken);
            var contentType = string.IsNullOrWhiteSpace(stat.ContentType)
                ? "application/octet-stream"
                : stat.ContentType;

            await using var memory = new MemoryStream();
            var getArgs = new GetObjectArgs()
                .WithBucket(normalizedBucket)
                .WithObject(normalizedObject)
                .WithCallbackStream(async stream =>
                {
                    await stream.CopyToAsync(memory, cancellationToken);
                });

            await _minioClient.GetObjectAsync(getArgs, cancellationToken);
            memory.Position = 0;

            return File(memory.ToArray(), contentType);
        }
        catch
        {
            return NotFound();
        }
    }
}
