using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DiscreteSignal.Models;
using DiscreteSignal.Service.Interface;

namespace DiscreteSignal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAudioStorageService _audioStorageService;
    private readonly IDiscreteSpectrumService _spectrumService;

    // Передача интерфейсов в конструктор класса необходим для автоматической
    // подстановки соответствующих классов через ServiceProvider
    // это выполнение принципа D из SOLID
    public HomeController(ILogger<HomeController> logger, 
        IAudioStorageService audioStorageService, 
        IDiscreteSpectrumService spectrumService)
    {
        _logger = logger;
        _audioStorageService = audioStorageService;
        _spectrumService = spectrumService;
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
    [HttpGet("GetDiscreteSpectrum")]
    public IActionResult CreateDiscreteSpectrum(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest("Не указано имя файла");

        if (!_audioStorageService.Exists(fileName))
            return NotFound("Файл не найден");

        var path = _audioStorageService.GetFullPath(fileName);
        var spectrum = _spectrumService.ComputeSpectrum(path);

        return Json(new { ok = true, magnitudes = spectrum });
    }
    
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