namespace DiscreteSignal.Service.Interface;

public interface IAmplitudeFrequencyResponseService
{
    double[] ComputeSpectrum(string filePath, int windowSize = 1024);
}