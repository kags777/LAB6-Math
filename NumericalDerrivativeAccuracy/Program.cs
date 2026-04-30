using System;
using System.Collections.Generic;

namespace DerivativeCalculation
{
    class Program
    {
        // Функция f(x) = v * x^2, где v = 1 (вариант 1)
        static float Function(float x)
        {
            return 1.0f * x * x; // f(x) = x^2
        }

        // Аналитическая производная f'(x) = 2 * v * x
        static float AnalyticalDerivative(float x)
        {
            return 2.0f * x; // f'(x) = 2x
        }

        // Приближенное вычисление производной по центральной формуле
        // f'(x) ≈ (f(x + h) - f(x - h)) / (2h)
        static float CentralDifference(float x, float h)
        {
            return (Function(x + h) - Function(x - h)) / (2.0f * h);
        }

        static void Main(string[] args)
        {
            // Точки, в которых вычисляем производную
            float[] points = { 1e-2f, 1e-1f, 1.0f, 10.0f, 1e2f };

            // Шаги для вычисления производной
            float[] steps = new float[8];
            for (int i = 0; i < 8; i++)
            {
                steps[i] = (float)Math.Pow(10, -(i + 1)); // 10^-1, 10^-2, ..., 10^-8
            }

            Console.WriteLine("ПРИБЛИЖЕННОЕ ВЫЧИСЛЕНИЕ ПЕРВОЙ ПРОИЗВОДНОЙ");
            Console.WriteLine($"Функция: f(x) = x^2");
            Console.WriteLine($"Аналитическая производная: f'(x) = 2x");
            Console.WriteLine("Метод: центральная разностная формула");


            foreach (float x in points)
            {
                float exactDerivative = AnalyticalDerivative(x);
                Console.WriteLine($"\nТочка x = {x}");
                Console.WriteLine($"Точное значение производной: {exactDerivative}");
                Console.WriteLine("\nШаг (h)     | Приближенное значение | Абсолютная ошибка | Относительная ошибка");
                Console.WriteLine("-".PadRight(70, '-'));

                float bestError = float.MaxValue;
                float bestStep = 0;
                float bestValue = 0;

                foreach (float h in steps)
                {
                    float approxDerivative = CentralDifference(x, h);
                    float absoluteError = Math.Abs(approxDerivative - exactDerivative);
                    float relativeError = absoluteError / Math.Abs(exactDerivative);

                    if (absoluteError < bestError)
                    {
                        bestError = absoluteError;
                        bestStep = h;
                        bestValue = approxDerivative;
                    }

                    Console.WriteLine($"{h,-12} | {approxDerivative,-20} | {absoluteError,-16} | {relativeError,-20:E3}");
                }

                Console.WriteLine("-".PadRight(70, '-'));
                Console.WriteLine($"  Оптимальный шаг для x = {x}: h = {bestStep}");
                Console.WriteLine($"  Значение производной: {bestValue}");
                Console.WriteLine($"  Абсолютная ошибка: {bestError}");
            }
            Console.ReadKey();
        }
    }
}
