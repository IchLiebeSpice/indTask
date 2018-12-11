using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace МНК
{
    class CurveFunctions
    {
        public static double F(List<double> coeffs, double x)
        {
            double total = 0;
            double x_factor = 1;
            for (int i = 0; i < coeffs.Count; i++)
            {
                total += x_factor * coeffs[i];
                x_factor *= x;
            }
            return total;
        }

        public static double ErrorSquared(List<PointF> points, List<double> coeffs)
        {
            double total = 0;
            foreach (PointF pt in points)
            {
                double dy = pt.Y - F(coeffs, pt.X);
                total += dy * dy;
            }
            return total;
        }

        public static List<double> FindPolynomialLeastSquaresFit(List<PointF> points, int degree)
        {
            double[,] coeffs = new double[degree + 1, degree + 2];

            for (int j = 0; j <= degree; j++)
            {

                coeffs[j, degree + 1] = 0;
                foreach (PointF pt in points)
                {
                    coeffs[j, degree + 1] -= Math.Pow(pt.X, j) * pt.Y;
                }

                for (int a_sub = 0; a_sub <= degree; a_sub++)
                {
                    coeffs[j, a_sub] = 0;
                    foreach (PointF pt in points)
                    {
                        coeffs[j, a_sub] -= Math.Pow(pt.X, a_sub + j);
                    }
                }
            }

            double[] answer = GaussianElimination(coeffs);

            return answer.ToList<double>();
        }

        private static double[] GaussianElimination(double[,] coeffs)
        {
            int max_equation = coeffs.GetUpperBound(0);
            int max_coeff = coeffs.GetUpperBound(1);
            for (int i = 0; i <= max_equation; i++)
            {

                if (coeffs[i, i] == 0)
                {
                    for (int j = i + 1; j <= max_equation; j++)
                    {
                        if (coeffs[j, i] != 0)
                        {

                            for (int k = i; k <= max_coeff; k++)
                            {
                                double temp = coeffs[i, k];
                                coeffs[i, k] = coeffs[j, k];
                                coeffs[j, k] = temp;
                            }
                            break;
                        }
                    }
                }

                double coeff_i_i = coeffs[i, i];
                if (coeff_i_i == 0)
                {
                    throw new ArithmeticException(String.Format(
                        "There is no unique solution for these points.",
                        coeffs.GetUpperBound(0) - 1));
                }

                for (int j = i; j <= max_coeff; j++)
                {
                    coeffs[i, j] /= coeff_i_i;
                }

                for (int j = 0; j <= max_equation; j++)
                {
                    if (j != i)
                    {
                        double coef_j_i = coeffs[j, i];
                        for (int d = 0; d <= max_coeff; d++)
                        {
                            coeffs[j, d] -= coeffs[i, d] * coef_j_i;
                        }
                    }
                }
            }

            double[] solution = new double[max_equation + 1];
            for (int i = 0; i <= max_equation; i++)
            {
                solution[i] = coeffs[i, max_coeff];
            }

            return solution;
        }
    }
}
