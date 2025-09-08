namespace DiscreteSignal.Service.Interface;

public interface IAudioStorageService
{
    Task<string> SaveAsync(IFormFile file, CancellationToken ct = default);
    bool Exists(string fileName);
    string GetFullPath(string fileName);
}