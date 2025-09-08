using System.Diagnostics;
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

        var ext = Path.GetExtension(audioFile.FileName).ToLower();
        if (ext != ".wav")
            throw new ArgumentException("Только WAV файлы поддерживаются");

        var fileName = $"audio_{DateTime.UtcNow:yyyyMMdd_HHmmss}.wav";
        var path = Path.Combine(_uploadsPath, fileName);

        using var fs = new FileStream(path, FileMode.Create);
        await audioFile.CopyToAsync(fs, ct);

        return fileName;
    }

    
    // public async Task<string> SaveAsync(IFormFile audioFile, CancellationToken ct = default)
    // {
    //     if (audioFile == null || audioFile.Length == 0)
    //         throw new ArgumentException("Пустой файл");
    //
    //     var ext = Path.GetExtension(audioFile.FileName).ToLower();
    //
    //     // Принимаем только WAV, остальные конвертируем через FFmpeg
    //     var fileName = $"audio_{DateTime.UtcNow:yyyyMMdd_HHmmss}.wav";
    //     var path = Path.Combine(_uploadsPath, fileName);
    //
    //     if (ext != ".wav")
    //     {
    //         // Сохраняем временный файл
    //         var tempPath = Path.Combine(_uploadsPath, Path.GetRandomFileName() + ext);
    //         using (var fs = new FileStream(tempPath, FileMode.Create))
    //         {
    //             await audioFile.CopyToAsync(fs, ct);
    //         }
    //
    //         // Конвертируем через FFmpeg в WAV
    //         var ffmpeg = "ffmpeg"; // должен быть в PATH
    //         var args = $"-y -i \"{tempPath}\" -ac 1 -ar 44100 \"{path}\"";
    //
    //         var process = new Process
    //         {
    //             StartInfo = new ProcessStartInfo
    //             {
    //                 FileName = ffmpeg,
    //                 Arguments = args,
    //                 RedirectStandardOutput = true,
    //                 RedirectStandardError = true,
    //                 UseShellExecute = false,
    //                 CreateNoWindow = true
    //             }
    //         };
    //         process.Start();
    //         string stderr = await process.StandardError.ReadToEndAsync();
    //         await process.WaitForExitAsync(ct);
    //
    //         File.Delete(tempPath);
    //         if (process.ExitCode != 0)
    //             throw new Exception("Ошибка конвертации аудио: " + stderr);
    //     }
    //     else
    //     {
    //         // Просто сохраняем WAV
    //         using var fs = new FileStream(path, FileMode.Create);
    //         await audioFile.CopyToAsync(fs, ct);
    //     }
    //
    //     return fileName;
    // }

    public bool Exists(string fileName)
    {
        return File.Exists(GetFullPath(fileName));
    }

    public string GetFullPath(string fileName)
    {
        return Path.Combine(_uploadsPath, fileName);
    }
}