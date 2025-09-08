using DiscreteSignal.Service.Interface;
using NAudio.Dsp;
using NAudio.Wave;
using System.Numerics;
using Complex = System.Numerics.Complex;

namespace DiscreteSignal.Service.Implementation;

public class DiscreteSpectrumService : IDiscreteSpectrumService
{   
    /// <summary>
    /// Вычисляет спектр сигнала (комплексный), чтобы можно было потом сделать ОДПФ.
    /// </summary>
    /// <param name="filePath">Путь к WAV-файлу</param>
    /// <param name="windowSize">Длина окна N</param>
    /// <returns>Список массивов комплексных чисел для каждого окна</returns>
    public List<Complex[]> ComputeSpectrumComplex(string filePath, int windowSize = 1024)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException(filePath);

        List<double> samples = new();
        using (var reader = new WaveFileReader(filePath))
        {
            var buffer = new float[reader.SampleCount];
            int index = 0;
            var sampleProvider = reader.ToSampleProvider();
            int read;
            while ((read = sampleProvider.Read(buffer, index, buffer.Length - index)) > 0)
                index += read;

            samples.AddRange(buffer.Take(index).Select(f=>(double)f));
        }
        
        Console.WriteLine($"Прочитано {samples.Count} сэмплов, макс={samples.Max()}, мин={samples.Min()}");
        List<Complex[]> spectrums = new();
        int totalSamples = samples.Count;
        for (int start = 0; start < totalSamples; start += windowSize)
        {
            int len = Math.Min(windowSize, totalSamples - start);
            double[] window = new double[windowSize];
            for (int i = 0; i < len; i++)
                window[i] = samples[start + i];
            for (int i = len; i < windowSize; i++)
                window[i] = 0;

            spectrums.Add(DFT(window));
        }

        return spectrums;
    }

    /// <summary>
    /// Вычисляет амплитудный спектр (для визуализации).
    /// </summary>
    public List<double[]> ComputeSpectrumMagnitude(string filePath, int windowSize = 1024)
    {
        var complexSpectrums = ComputeSpectrumComplex(filePath, windowSize);
        return complexSpectrums
            .Select(spec => spec.Select(c => c.Magnitude).ToArray())
            .ToList();
    }

    /// <summary>
    /// ДПФ: возвращает комплексный спектр (без нормировки по 1/N)
    /// </summary>
    private Complex[] DFT(double[] x)
    {
        int N = x.Length;
        Complex[] X = new Complex[N];
        for (int k = 0; k < N; k++)
        {
            Complex sum = Complex.Zero;
            for (int n = 0; n < N; n++)
            {
                double angle = -2.0 * Math.PI * k * n / N;
                sum += x[n] * Complex.Exp(new Complex(0, angle));
            }
            X[k] = sum;
        }
        return X;
    }
    
    public double[] GetRawSamples(string filePath, int maxCount)
    {
        List<double> samples = new();

        using (var reader = new WaveFileReader(filePath))
        {
            int channels = reader.WaveFormat.Channels;
            int bytesPerSample = reader.WaveFormat.BitsPerSample / 8;

            byte[] buffer = new byte[reader.WaveFormat.BlockAlign * 1024]; // блок

            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i += bytesPerSample * channels)
                {
                    double sample = 0;
                    if (bytesPerSample == 2) // 16-bit PCM
                    {
                        short s = BitConverter.ToInt16(buffer, i);
                        sample = s / 32768.0;
                    }
                    else if (bytesPerSample == 4)
                    {
                        if (reader.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                        {
                            float f = BitConverter.ToSingle(buffer, i);
                            sample = f;
                        }
                        else
                        {
                            int s = BitConverter.ToInt32(buffer, i);
                            sample = s / (double)Int32.MaxValue;
                        }
                    }

                    samples.Add(sample);

                    if (samples.Count >= maxCount)
                        break;
                }

                if (samples.Count >= maxCount)
                    break;
            }
        }

        return samples.Take(maxCount).ToArray();
    }
    // public double[] GetRawSamples(string filePath, int maxCount)
    // {
    //     List<double> samples = new();
    //
    //     using (var reader = new WaveFileReader(filePath))
    //     {
    //         if (reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm &&
    //             reader.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
    //         {
    //             throw new InvalidOperationException("Требуется WAV в PCM или IEEE float формате");
    //         }
    //
    //         var bytesPerSample = reader.WaveFormat.BitsPerSample / 8;
    //         var buffer = new byte[reader.WaveFormat.BlockAlign * 1024];
    //         int read;
    //
    //         while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
    //         {
    //             for (int i = 0; i < read; i += bytesPerSample)
    //             {
    //                 double sampleValue;
    //
    //                 if (reader.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
    //                 {
    //                     float sample = BitConverter.ToSingle(buffer, i);
    //                     sampleValue = sample; // уже [-1;1]
    //                 }
    //                 else if (reader.WaveFormat.BitsPerSample == 16)
    //                 {
    //                     short sample = BitConverter.ToInt16(buffer, i);
    //                     sampleValue = sample / 32768.0;
    //                 }
    //                 else if (reader.WaveFormat.BitsPerSample == 24)
    //                 {
    //                     // 24-bit PCM: собрать вручную
    //                     int sample = (buffer[i + 2] << 16) | (buffer[i + 1] << 8) | buffer[i];
    //                     if ((sample & 0x800000) != 0) // знак
    //                         sample |= unchecked((int)0xFF000000);
    //                     sampleValue = sample / 8388608.0;
    //                 }
    //                 else if (reader.WaveFormat.BitsPerSample == 32)
    //                 {
    //                     int sample = BitConverter.ToInt32(buffer, i);
    //                     sampleValue = sample / (double)int.MaxValue;
    //                 }
    //                 else
    //                 {
    //                     throw new NotSupportedException($"Неподдерживаемая разрядность {reader.WaveFormat.BitsPerSample}");
    //                 }
    //
    //                 samples.Add(sampleValue);
    //                 if (samples.Count >= maxCount)
    //                     return samples.ToArray();
    //             }
    //         }
    //     }
    //
    //     return samples.ToArray();
    // }
    
}