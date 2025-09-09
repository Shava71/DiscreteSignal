using DiscreteSignal.Models;
using DiscreteSignal.Service.Implementation;
using DiscreteSignal.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DiscreteSignal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAudioStorageService _audioStorageService;
    private readonly IDiscreteSpectrumService _spectrumService;
    private readonly IAmplitudeFrequencyResponseService _amplitudeFrequencyResponseService;
    private readonly IInverseDiscreteFourierTransformService _idftService;
    private readonly IWavReaderService _wavReaderService;

    // Передача интерфейсов в конструктор класса необходим для автоматической
    // подстановки соответствующих классов через ServiceProvider
    // это выполнение принципа D из SOLID
    public HomeController(ILogger<HomeController> logger, 
        IAudioStorageService audioStorageService, 
        IDiscreteSpectrumService spectrumService,
        IAmplitudeFrequencyResponseService amplitudeFrequencyResponseService,
        IInverseDiscreteFourierTransformService idftService,
        IWavReaderService wavReaderService)
    {
        _logger = logger;
        _audioStorageService = audioStorageService;
        _spectrumService = spectrumService;
        _amplitudeFrequencyResponseService = amplitudeFrequencyResponseService;
        _idftService = idftService;
        _wavReaderService = wavReaderService;
    }
    
    // Главная страница с возможность записи голоса / загрузки файла с голосом
    [HttpGet("/")] //атрибут маршрутизации
    public IActionResult Index()
    {
        return View();
    }
    
    // страница с выводом дискретнего спектра (жёстко нейронка насрала) и обратного
    // тут будут общие графики наверное
    [HttpGet("Analyze")]
    public IActionResult Analyze(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return RedirectToAction("Index");
        
        ViewData["FileName"] = fileName;
        return View();
    }

    // Загрузка аудио на сервер
    [HttpPost("Upload")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile audio)
    {
        if (audio == null || audio.Length == 0)
        {
            return BadRequest("Файл не получен");
        }
        try
        {
            var fileName = await _audioStorageService.SaveAsync(audio); // работу с IO bound лучше делать через async/await (асинхронность)
            // для CPU bound лучше синхронность или параллельность
            return Json(new { ok = true, fileName });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ok = false, message = ex.Message });
        }
    }
    
    // это гет-запрос, чтобы получить ДПФ, на него страница analyze делает fetch
    [HttpGet("GetDiscreteSpectrumWindow")]
    public IActionResult GetDiscreteSpectrumWindow(string fileName, int windowIndex = 0, int windowSize = 1024)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest("Не указано имя файла");

        if (!_audioStorageService.Exists(fileName))
            return NotFound("Файл не найден");

        var path = _audioStorageService.GetFullPath(fileName);
        var allSpectrums = _spectrumService.ComputeSpectrumMagnitude(path, windowSize);

        if (windowIndex < 0 || windowIndex >= allSpectrums.Count)
            return BadRequest("Некорректный индекс окна");

        return Json(new { ok = true, magnitudes = allSpectrums[windowIndex] });
    }
    
    [HttpGet("GetRawSamples")]
    public IActionResult GetRawSamples(string fileName, int maxCount = 10000)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Json(new { ok = false, message = "Не указано имя файла" });

        if (!_audioStorageService.Exists(fileName))
            return Json(new { ok = false, message = "Файл не найден" });

        var path = _audioStorageService.GetFullPath(fileName);
        var samples = _spectrumService.GetRawSamples(path, maxCount);

        return Json(new { ok = true, samples });
    }
    
    /// <summary>
    /// Получение временного сигнала после обратного ДПФ (проверка корректности восстановления)
    /// </summary>
    [HttpGet("GetInverseDFTWindow")]
    public IActionResult GetInverseDFTWindow(string fileName, int windowIndex = 0, int windowSize = 1024)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest("Не указано имя файла");

        if (!_audioStorageService.Exists(fileName))
            return NotFound("Файл не найден");

        var path = _audioStorageService.GetFullPath(fileName);
        var complexSpectrums = _spectrumService.ComputeSpectrumComplex(path, windowSize);

        if (windowIndex < 0 || windowIndex >= complexSpectrums.Count)
            return BadRequest("Некорректный индекс окна");

        var spectrum = complexSpectrums[windowIndex];
        var restored = _idftService.IDFT(spectrum);

        // Берем только действительную часть, т.к. результат должен быть вещественным
        var signal = restored.Select(c => c.Real).ToArray();

        return Json(new { ok = true, signal });
    }

    [HttpGet("GetWavSamples")]
    public IActionResult GetWavSamples(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest("Не указано имя файла");
        if (!_audioStorageService.Exists(fileName))
            return NotFound("Файл не найден");

        var path = _audioStorageService.GetFullPath(fileName);
        var samples = _wavReaderService.ReadWavSamples(path);
        return Json(new { ok = true, samples });
    }

    // [HttpGet("GetAmplitudeFrequencyResponse")]
    // public IActionResult GetAmplitudeFrequencyResponse(string fileName, int windowSize = 1024)
    // {
    //     if (string.IsNullOrWhiteSpace(fileName))
    //         return BadRequest("Не указано имя файла");
    //
    //     if (!_audioStorageService.Exists(fileName))
    //         return NotFound("Файл не найден");
    //
    //     var path = _audioStorageService.GetFullPath(fileName);
    //     var spectrum = _spectrumService.ComputeSpectrum(path, windowSize);
    //
    //     return Json(new
    //     {
    //         ok = true,
    //         magnitudes = spectrum,
    //         windowSize = windowSize
    //     });
    // }

    // Эту хуйню проект срёт сам, не обращай внимания, можно даже удалить
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}