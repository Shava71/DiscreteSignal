namespace DiscreteSignal.Service.Interface;

public interface IDiscreteSpectrumService
{
    // double[] ComputeSpectrum(string filePath, int fftSize = 2048);
    List<double[]> ComputeSpectrum(string filePath, int windowSize = 1024);
}