using DiscreteSignal.Service.Interface;
using System.Numerics;

namespace DiscreteSignal.Service.Implementation;

public class InverseDiscreteFourierTransformService : IInverseDiscreteFourierTransformService
{
    public Complex[] IDFT(Complex[] X)
    {
        // создаем массив комплексных чисел для s(n)
        int N = X.Length;
        Complex[] x = new Complex[N];


        for (int n = 0; n < N; n++)
        {
            // инициализируем текущую сумму, как ноль
            Complex sum = Complex.Zero;

            // считаем сумму по k для текущего n
            for (int k = 0; k < N; k++)
            {
                // вычисляем экспоненту - множитель члена суммы
                double theta = 2 * Math.PI * k * n / N;
                Complex exp = new Complex(Math.Cos(theta), Math.Sin(theta));
                // добавляем сумме произведение Sd(k) и экспоненты 
                sum += X[k] * exp;
            }
            // вычисляем s(n)
            x[n] = sum / N;
        }
        return x;
    }
}
