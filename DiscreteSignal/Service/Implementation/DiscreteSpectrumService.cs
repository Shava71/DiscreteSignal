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

        // ✅ NAudio.Wave — AudioFileReader умеет читать WAV/MP3/OGG/M4A и конвертировать в float
        using var reader = new AudioFileReader(filePath);

        var buffer = new float[fftSize];
        int read = reader.Read(buffer, 0, fftSize);

        // ✅ NAudio.Dsp — FastFourierTransform выполняет FFT
        var complex = new Complex[fftSize];
        for (int i = 0; i < fftSize; i++)
        {
            complex[i].X = buffer[i];
            complex[i].Y = 0;
        }

        // Hamming window (сам код)
        for (int i = 0; i < fftSize; i++)
            complex[i].X *= (float)(0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (fftSize - 1)));

        // ✅ NAudio.Dsp.FastFourierTransform
        FastFourierTransform.FFT(true, (int)Math.Log2(fftSize), complex);

        var magnitudes = new double[fftSize / 2];
        for (int i = 0; i < fftSize / 2; i++)
            magnitudes[i] = Math.Sqrt(complex[i].X * complex[i].X + complex[i].Y * complex[i].Y);

        return magnitudes;
    }
}