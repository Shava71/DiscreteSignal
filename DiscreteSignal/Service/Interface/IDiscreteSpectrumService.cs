using System.Numerics;

namespace DiscreteSignal.Service.Interface;

public interface IDiscreteSpectrumService
{
    // double[] ComputeSpectrum(string filePath, int fftSize = 2048);
    List<Complex[]> ComputeSpectrumComplex(string filePath, int windowSize = 1024);
    List<double[]> ComputeSpectrumMagnitude(string filePath, int windowSize = 1024);
    double[] GetRawSamples(string filePath, int maxCount);
}