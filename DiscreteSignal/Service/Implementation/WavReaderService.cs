using DiscreteSignal.Service.Interface;

namespace DiscreteSignal.Service.Implementation
{
    public class WavReaderService : IWavReaderService
    {
        public float[] ReadWavSamples(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new BinaryReader(stream);

            // Пропускаем заголовок WAV (44 байта для стандартного формата)
            stream.Seek(44, SeekOrigin.Begin);

            // Читаем данные
            var samples = new float[(stream.Length - 44) / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                // Читаем 16-битные PCM данные и конвертируем в float (-1.0 до 1.0)
                short sample = reader.ReadInt16();
                samples[i] = sample / 32768.0f; // Нормализация
            }

            return samples;
        }
    }
}