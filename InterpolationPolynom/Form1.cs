using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace InterpolationPolynom
{
    public class Form1 : Form
    {
        public Form1()
        {
            this.Width = 1200;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Интерполяция функций";

            string areaName = "MainArea";
            ChartArea area = new ChartArea(areaName);
            SetupArea(area);

            Chart myChart = new Chart();
            myChart.Dock = DockStyle.Fill;
            myChart.Name = "chart1";
            myChart.ChartAreas.Add(area);

            // Добавляем легенду
            Legend legend = new Legend("Legend");
            legend.DockedToChartArea = areaName;
            legend.Docking = Docking.Right;
            legend.Alignment = StringAlignment.Center;
            legend.Font = new System.Drawing.Font("Arial", 10);
            legend.BackColor = System.Drawing.Color.WhiteSmoke;
            legend.BorderColor = System.Drawing.Color.Gray;
            legend.BorderWidth = 1;
            legend.BorderDashStyle = ChartDashStyle.Solid;
            myChart.Legends.Add(legend);

            //  НЬЮТОН 
            Series seriesNewton = new Series("График 1: Ньютон");
            SetupSeries(seriesNewton, areaName, 10, System.Drawing.Color.Green);
            for (double x = 0; x <= 10; x += 0.1)
            {
                double y = newton(x);
                seriesNewton.Points.AddXY(x, y);
            }
            myChart.Series.Add(seriesNewton);

            //ПОЛИНОМ (коэффициенты из СЛАУ) 
            Series seriesPolinomial = new Series("График 2: Полином");
            SetupSeries(seriesPolinomial, areaName, 6, System.Drawing.Color.Orange);
            for (double x = 0; x <= 10; x += 0.1)
            {
                double y = polynomial(x);
                seriesPolinomial.Points.AddXY(x, y);
            }
            myChart.Series.Add(seriesPolinomial);

            //ЛАГРАНЖ 
            Series seriesLagrange = new Series("График 3: Лагранж");
            SetupSeries(seriesLagrange, areaName, 2, System.Drawing.Color.Red);
            for (double x = 0; x <= 10; x += 0.1)
            {
                double y = lagrange(x);
                seriesLagrange.Points.AddXY(x, y);
            }
            myChart.Series.Add(seriesLagrange);

            //ИСХОДНЫЕ ТОЧКИ (МАРКЕРЫ) 
            Series seriesPoints = new Series("Исходные точки");
            seriesPoints.ChartType = SeriesChartType.Point;
            seriesPoints.ChartArea = areaName;
            seriesPoints.MarkerStyle = MarkerStyle.Circle;
            seriesPoints.MarkerSize = 10;
            seriesPoints.MarkerColor = System.Drawing.Color.Blue;
            seriesPoints.Color = System.Drawing.Color.Blue;
            seriesPoints.Legend = "Legend";

            seriesPoints.Points.AddXY(2, 6);
            seriesPoints.Points.AddXY(4, 6);
            seriesPoints.Points.AddXY(5, 1);
            seriesPoints.Points.AddXY(6, -1);
            seriesPoints.Points.AddXY(7, 11);

            myChart.Series.Add(seriesPoints);

            // Добавляем заголовок графика
            Title chartTitle = new Title("Сравнение методов интерполяции (Ньютон, Лагранж, Полином)");
            chartTitle.Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold);
            chartTitle.Docking = Docking.Top;
            myChart.Titles.Add(chartTitle);

            this.Controls.Add(myChart);
        }

        static void SetupArea(ChartArea area)
        {
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 10;
            area.AxisY.Minimum = -5;
            area.AxisY.Maximum = 15;

            area.AxisX.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisX.MajorGrid.Interval = 1;
            area.AxisY.MajorGrid.Interval = 1;
            area.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            area.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            area.AxisX.LabelStyle.Interval = 1;
            area.AxisX.LabelStyle.Format = "{0:F0}";
            area.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 9);

            area.AxisY.LabelStyle.Interval = 5;
            area.AxisY.LabelStyle.Format = "{0:F0}";
            area.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 9);

            // Подписываем оси
            area.AxisX.Title = "Ось X";
            area.AxisX.TitleFont = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold);
            area.AxisY.Title = "Ось Y";
            area.AxisY.TitleFont = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold);

            // Добавляем рамку
            area.BorderColor = System.Drawing.Color.Black;
            area.BorderWidth = 1;
            area.BorderDashStyle = ChartDashStyle.Solid;
        }

        static void SetupSeries(Series series, string areaName, int width, System.Drawing.Color color)
        {
            series.ChartType = SeriesChartType.Line;
            series.ChartArea = areaName;
            series.BorderWidth = width;
            series.Color = color;
            series.Legend = "Legend";
            series.LegendText = series.Name;
        }

        static double polynomial(double x)
        {
            return 0.2083 * Math.Pow(x, 4) - 2.75 * Math.Pow(x, 3) + 11.292 * Math.Pow(x, 2) - 15.75 * x + 11;
        }

        static double lagrange(double x)
        {
            return
                (1f / 20) * (x - 4) * (x - 5) * (x - 6) * (x - 7)
                - (1f / 2) * (x - 2) * (x - 5) * (x - 6) * (x - 7)
                + (1f / 6) * (x - 2) * (x - 4) * (x - 6) * (x - 7)
                + (1f / 8) * (x - 2) * (x - 4) * (x - 5) * (x - 7)
                + (11f / 30) * (x - 2) * (x - 4) * (x - 5) * (x - 6);
        }

        static double newton(double x)
        {
            return
                6 - (5f / 3) * (x - 2) * (x - 4)
                + (19f / 24) * (x - 2) * (x - 4) * (x - 5)
                + (5f / 24) * (x - 2) * (x - 4) * (x - 5) * (x - 6);
        }
    }
}


