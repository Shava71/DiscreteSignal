using DiscreteSignal.Service.Interface;

namespace DiscreteSignal.Service.Implementation;

// данный сервис отвечает за загрузку файлов записи в хранилище сервера (будет срать в папку wwwroot)
public class AudioStorageService : IAudioStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly string _uploadsPath;

    public AudioStorageService(IWebHostEnvironment env)
    {
        _env = env;
        _uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(_uploadsPath))
            Directory.CreateDirectory(_uploadsPath);
    }

    public async Task<string> SaveAsync(IFormFile audioFile, CancellationToken ct = default)
    {
        if (audioFile == null || audioFile.Length == 0)
            throw new ArgumentException("Пустой файл");

        var ext = Path.GetExtension(audioFile.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".wav";

        var fileName = $"audio_{DateTime.UtcNow:yyyyMMdd_HHmmss}{ext}";
        var path = Path.Combine(_uploadsPath, fileName);

        using var fs = new FileStream(path, FileMode.Create);
        await audioFile.CopyToAsync(fs, ct);

        return fileName;
    }

    public bool Exists(string fileName)
    {
        return File.Exists(GetFullPath(fileName));
    }

    public string GetFullPath(string fileName)
    {
        return Path.Combine(_uploadsPath, fileName);
    }
}