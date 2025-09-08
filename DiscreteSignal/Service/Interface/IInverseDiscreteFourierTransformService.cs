using System.Numerics;
namespace DiscreteSignal.Service.Interface;


public interface IInverseDiscreteFourierTransformService
{
    Complex[] IDFT(Complex[] X);
}
