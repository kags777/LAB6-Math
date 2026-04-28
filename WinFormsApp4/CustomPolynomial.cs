using System;

namespace WinFormsApp4
{
    public class CustomPolynomial
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double D { get; set; }
        public double Xmin { get; set; }
        public double Xmax { get; set; }

        public CustomPolynomial(double a, double b, double c, double d, double xmin, double xmax)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            Xmin = xmin;
            Xmax = xmax;
        }

        // Значение функции f(x)
        public double Value(double x)
        {
            double dx = x - Xmin;
            return A + B * dx + C * dx * dx + D * dx * dx * dx;
        }

        // Первая производная: f'(x) = B + 2*C*dx + 3*D*dx^2
        public double FirstDerivative(double x)
        {
            double dx = x - Xmin;
            return B + 2 * C * dx + 3 * D * dx * dx;
        }

        // Вторая производная: f''(x) = 2*C + 6*D*dx
        public double SecondDerivative(double x)
        {
            double dx = x - Xmin;
            return 2 * C + 6 * D * dx;
        }

        // Третья производная: f'''(x) = 6*D (константа для кубического полинома)
        public double ThirdDerivative()
        {
            return 6 * D;
        }

        public override string ToString()
        {
            return $"f(x) = {A} + {B}·(x-{Xmin}) + {C}·(x-{Xmin})² + {D}·(x-{Xmin})³, x∈[{Xmin};{Xmax}]";
        }
    }
}