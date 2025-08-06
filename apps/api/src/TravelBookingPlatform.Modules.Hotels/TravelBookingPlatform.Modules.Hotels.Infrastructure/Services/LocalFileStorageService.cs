using Microsoft.AspNetCore.Hosting;
using TravelBookingPlatform.Modules.Hotels.Application.Interfaces;

namespace TravelBookingPlatform.Modules.Hotels.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> UploadFileAsync(string fileName, byte[] content, string contentType)
    {
        var folder = Path.Combine(_env.WebRootPath, "confirmations");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var filePath = Path.Combine(folder, fileName);
        await File.WriteAllBytesAsync(filePath, content);

        return $"/confirmations/{fileName}"; // Return relative URL
    }

    public async Task<byte[]?> GetFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            return await File.ReadAllBytesAsync(fullPath);
        }
        return null;
    }
}