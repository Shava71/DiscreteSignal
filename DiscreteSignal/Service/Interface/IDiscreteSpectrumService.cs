namespace DiscreteSignal.Service.Interface;

public interface IDiscreteSpectrumService
{
    double[] ComputeSpectrum(string filePath, int fftSize = 2048);
}