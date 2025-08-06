namespace TravelBookingPlatform.Modules.Hotels.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(string fileName, byte[] content, string contentType);
    Task<byte[]?> GetFileAsync(string filePath);
}