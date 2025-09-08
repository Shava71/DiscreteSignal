using System.Numerics;
using DiscreteSignal.Service.Interface;
using NAudio.Wave;

namespace DiscreteSignal.Service.Implementation;

public class AmplitudeFrequencyResponseService : IAmplitudeFrequencyResponseService
{
    /// <summary>
    /// Вычисляет амплитудно-частотную характеристику (АЧХ) для WAV-файла
    /// </summary>
    /// <param name="filePath">Путь к WAV-файлу</param>
    /// <param name="windowSize">Длина окна N</param>
    /// <returns>Массив амплитуд нормированного спектра</returns>
    public double[] ComputeSpectrum(string filePath, int windowSize = 1024)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        // Считываем все отсчеты WAV в память
        List<double> samples = new();
        using (var reader = new AudioFileReader(filePath))
        {
            float[] buffer = new float[windowSize * 10]; // читаем пачками
            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                for (int i = 0; i < read; i++)
                    samples.Add(buffer[i]);
        }

        // Берем первые windowSize отсчетов (N) или дополняем нулями
        double[] window = new double[windowSize];
        int count = Math.Min(windowSize, samples.Count);
        for (int i = 0; i < count; i++)
            window[i] = samples[i];

        // Вычисляем ДПФ и нормируем
        var magnitudes = DFT(window);

        // Нормировка: приводим к диапазону [0..1] для визуализации
        double max = magnitudes.Max();
        if (max > 0)
            for (int i = 0; i < magnitudes.Length; i++)
                magnitudes[i] /= max;

        return magnitudes;
    }

    /// <summary>
    /// Дискретное преобразование Фурье (берем только первые N/2 гармоник)
    /// </summary>
    private double[] DFT(double[] x)
    {
        int N = x.Length;
        double[] magnitudes = new double[N / 2];
        for (int k = 0; k < N / 2; k++)
        {
            Complex sum = Complex.Zero;
            for (int n = 0; n < N; n++)
            {
                double angle = -2.0 * Math.PI * k * n / N;
                sum += x[n] * Complex.Exp(new Complex(0, angle));
            }
            magnitudes[k] = sum.Magnitude;
        }
        return magnitudes;
    }
}