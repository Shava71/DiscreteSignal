using DiscreteSignal.Service.Interface;
using NAudio.Dsp;
using NAudio.Wave;
using System.Numerics;
using Complex = System.Numerics.Complex;

namespace DiscreteSignal.Service.Implementation;

public class DiscreteSpectrumService : IDiscreteSpectrumService
{
    /// <summary>
    /// Вычисляет амплитудный спектр сигнала из WAV-файла с окном длиной N отсчетов
    /// </summary>
    /// <param name="filePath">Путь к WAV-файлу</param>
    /// <param name="windowSize">Длина окна N</param>
    /// <returns>Список амплитуд спектра для каждого окна</returns>
    public List<double[]> ComputeSpectrum(string filePath, int windowSize = 1024)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        // Считываем WAV-файл через NAudio
        List<double> samples = new();
        using (var reader = new AudioFileReader(filePath))
        {
            float[] buffer = new float[reader.WaveFormat.SampleRate]; // буфер на 1 сек
            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                    samples.Add(buffer[i]);
            }
        }

        // Разбиваем сигнал на окна длиной N и считаем ДПФ
        List<double[]> spectrums = new();
        int totalSamples = samples.Count;
        for (int start = 0; start < totalSamples; start += windowSize)
        {
            int len = Math.Min(windowSize, totalSamples - start);
            double[] window = new double[windowSize];
            for (int i = 0; i < len; i++)
                window[i] = samples[start + i];
            // Если окно меньше N, дополняем нулями
            for (int i = len; i < windowSize; i++)
                window[i] = 0;

            var spectrum = DFT(window);
            spectrums.Add(spectrum);
        }

        return spectrums;
    }

    /// <summary>
    /// Дискретное преобразование Фурье (ДПФ)
    /// </summary>
    /// <param name="x">Массив временных отсчетов</param>
    /// <returns>Массив амплитуд спектра (модуль комплексных чисел)</returns>
    private double[] DFT(double[] x)
    {
        int N = x.Length;
        double[] magnitudes = new double[N];
        for (int k = 0; k < N; k++)
        {
            Complex sum = Complex.Zero;
            for (int n = 0; n < N; n++)
            {
                double angle = -2.0 * Math.PI * k * n / N;
                sum += x[n] * Complex.Exp(new Complex(0, angle));
            }
            // magnitudes[k] = sum.Magnitude / N; // нормировка по 1/N
            magnitudes[k] = sum.Magnitude;
        }
        return magnitudes;
    }
}