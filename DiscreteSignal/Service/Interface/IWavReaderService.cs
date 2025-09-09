using System;
using System.IO;
using System.Linq;

namespace DiscreteSignal.Service.Interface
{
    public interface IWavReaderService
    {
        float[] ReadWavSamples(string filePath);
    }
}