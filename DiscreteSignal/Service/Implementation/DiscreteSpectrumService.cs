using DiscreteSignal.Service.Interface;
using NAudio.Dsp;
using NAudio.Wave;

namespace DiscreteSignal.Service.Implementation;

public class DiscreteSpectrumService : IDiscreteSpectrumService
{
    // тут нейронка насрала свой код (пусть пока будет заглушкой)
    public double[] ComputeSpectrum(string filePath, int fftSize = 2048)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Файл не найден", filePath);

        // Используем только WaveFileReader (кроссплатформенный)
        using var reader = new WaveFileReader(filePath);

        var buffer = new float[fftSize];
        int read = 0;
        for (int i = 0; i < fftSize;)
        {
            var sample = reader.ReadNextSampleFrame();
            if (sample == null) break;
            buffer[i++] = sample[0]; // берём первый канал
            read++;
        }

        // Преобразуем в комплексные числа
        var complex = new Complex[fftSize];
        for (int i = 0; i < fftSize; i++)
        {
            complex[i].X = i < read ? buffer[i] : 0;
            complex[i].Y = 0;
        }

        // Hamming window
        for (int i = 0; i < fftSize; i++)
            complex[i].X *= (float)(0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (fftSize - 1)));

        // FFT
        FastFourierTransform.FFT(true, (int)Math.Log2(fftSize), complex);

        var magnitudes = new double[fftSize / 2];
        for (int i = 0; i < fftSize / 2; i++)
            magnitudes[i] = Math.Sqrt(complex[i].X * complex[i].X + complex[i].Y * complex[i].Y);

        return magnitudes;
    }
}