using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

namespace InterpolationPolynom
{
    public class Form1 : Form
    {
        // Элементы управления для численного дифференцирования
        private TextBox txtH;
        private CheckBox chkFirstDeriv, chkSecondDeriv;
        private TextBox txtErrors;
        private Chart myChart;

        public Form1()
        {
            this.Width = 1200;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Интерполяция функции (Ньютон) с численным дифференцированием";

            // Панель управления сверху
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(10),
                BackColor = Color.WhiteSmoke
            };

            Label lblH = new Label { Text = "Шаг h:", Left = 10, Top = 15, Width = 50, Font = new Font("Arial", 9) };
            txtH = new TextBox { Left = 65, Top = 12, Width = 60, Text = "0.1", Font = new Font("Arial", 9) };
            chkFirstDeriv = new CheckBox { Text = "f'(x)", Left = 140, Top = 12, Width = 60, Checked = false, Font = new Font("Arial", 9) };
            chkSecondDeriv = new CheckBox { Text = "f''(x)", Left = 210, Top = 12, Width = 70, Checked = false, Font = new Font("Arial", 9) };
            txtErrors = new TextBox
            {
                Left = 10,
                Top = 42,
                Width = 700,
                Height = 25,
                ReadOnly = true,
                BackColor = Color.LightYellow,
                Font = new Font("Consolas", 8.5f)
            };

            // События для перерисовки графика при изменении параметров
            txtH.TextChanged += (s, e) => UpdateChart();
            chkFirstDeriv.CheckedChanged += (s, e) => UpdateChart();
            chkSecondDeriv.CheckedChanged += (s, e) => UpdateChart();

            topPanel.Controls.Add(lblH);
            topPanel.Controls.Add(txtH);
            topPanel.Controls.Add(chkFirstDeriv);
            topPanel.Controls.Add(chkSecondDeriv);
            topPanel.Controls.Add(txtErrors);

            // Область для графика
            string areaName = "MainArea";
            ChartArea area = new ChartArea(areaName);
            SetupArea(area);

            myChart = new Chart();
            myChart.Dock = DockStyle.Fill;
            myChart.Name = "chart1";
            myChart.ChartAreas.Add(area);

            // Легенда
            Legend legend = new Legend("Legend");
            legend.DockedToChartArea = areaName;
            legend.Docking = Docking.Right;
            legend.Alignment = StringAlignment.Center;
            legend.Font = new Font("Arial", 10);
            legend.BackColor = Color.WhiteSmoke;
            legend.BorderColor = Color.Gray;
            legend.BorderWidth = 1;
            legend.BorderDashStyle = ChartDashStyle.Solid;
            myChart.Legends.Add(legend);

            // Заголовок графика
            Title chartTitle = new Title("Интерполяционный многочлен Ньютона и численные производные");
            chartTitle.Font = new Font("Arial", 14, FontStyle.Bold);
            chartTitle.Docking = Docking.Top;
            myChart.Titles.Add(chartTitle);

            // Добавляем на форму панель и график
            this.Controls.Add(myChart);
            this.Controls.Add(topPanel); // панель сверху графика

            // Первоначальная отрисовка
            UpdateChart();
        }

        // Исходные данные для интерполяции
        private static readonly double[] XPoints = { 2, 4, 5, 6, 7 };
        private static readonly double[] YPoints = { 6, 6, 1, -1, 11 };

        // Интерполяционный многочлен Ньютона (степень 4)
        private static double NewtonValue(double x)
        {
            return 6
                   - (5.0 / 3) * (x - 2) * (x - 4)
                   + (19.0 / 24) * (x - 2) * (x - 4) * (x - 5)
                   + (5.0 / 24) * (x - 2) * (x - 4) * (x - 5) * (x - 6);
        }

        // Аналитическая первая производная
        private static double ExactFirstDerivative(double x)
        {
            double u = x - 2;
            return 37.0 / 12 - (5.0 / 12) * u - (13.0 / 4) * u * u + (5.0 / 6) * u * u * u;
        }

        // Аналитическая вторая производная
        private static double ExactSecondDerivative(double x)
        {
            double u = x - 2;
            return -5.0 / 12 - (13.0 / 2) * u + (5.0 / 2) * u * u;
        }

        // Численная первая производная (центральная разность)
        private static double NumericalFirstDeriv(double x, double h)
        {
            double fp = NewtonValue(x + h);
            double fm = NewtonValue(x - h);
            return (fp - fm) / (2.0 * h);
        }

        // Численная вторая производная (центральная разность)
        private static double NumericalSecondDeriv(double x, double h)
        {
            double f0 = NewtonValue(x);
            double fp = NewtonValue(x + h);
            double fm = NewtonValue(x - h);
            return (fp - 2.0 * f0 + fm) / (h * h);
        }

        private void UpdateChart()
        {
            myChart.Series.Clear();
            txtErrors.Text = "";

            // Парсим шаг h
            double h;
            if (!double.TryParse(txtH.Text,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out h) || h <= 0)
            {
                h = 0.1;
                txtH.Text = "0.1";
            }

            string areaName = "MainArea";

            // 1. График самого многочлена Ньютона
            Series seriesNewton = new Series("P(x) — Ньютон");
            seriesNewton.ChartType = SeriesChartType.Line;
            seriesNewton.ChartArea = areaName;
            seriesNewton.BorderWidth = 4;
            seriesNewton.Color = Color.Green;
            seriesNewton.Legend = "Legend";

            for (double x = 0; x <= 10; x += 0.1)
                seriesNewton.Points.AddXY(x, NewtonValue(x));
            myChart.Series.Add(seriesNewton);

            // 2. Исходные точки (маркеры)
            Series seriesPoints = new Series("Исходные точки");
            seriesPoints.ChartType = SeriesChartType.Point;
            seriesPoints.ChartArea = areaName;
            seriesPoints.MarkerStyle = MarkerStyle.Circle;
            seriesPoints.MarkerSize = 10;
            seriesPoints.MarkerColor = Color.Blue;
            seriesPoints.Color = Color.Blue;
            seriesPoints.Legend = "Legend";

            for (int i = 0; i < XPoints.Length; i++)
                seriesPoints.Points.AddXY(XPoints[i], YPoints[i]);
            myChart.Series.Add(seriesPoints);

            // 3. Численные производные (если включены)
            bool showFirst = chkFirstDeriv.Checked;
            bool showSecond = chkSecondDeriv.Checked;
            bool showAny = showFirst || showSecond;

            if (showAny)
            {
                // Диапазон для отображения производных (весь интервал графика)
                double plotMin = 0.0;
                double plotMax = 10.0;
                int plotSteps = 500;

                // Для расчёта погрешностей используем интервал исходных точек [2, 7]
                double errorMin = 2.0 + h;
                double errorMax = 7.0 - h;
                bool canCalcError = errorMax > errorMin;

                List<double> errorsFirst = new List<double>();
                List<double> errorsSecond = new List<double>();

                if (showFirst)
                {
                    Series serDeriv1 = new Series("f'(x) числ.");
                    serDeriv1.ChartType = SeriesChartType.Line;
                    serDeriv1.ChartArea = areaName;
                    serDeriv1.BorderWidth = 2;
                    serDeriv1.Color = Color.DarkGreen;
                    serDeriv1.Legend = "Legend";

                    for (int i = 0; i <= plotSteps; i++)
                    {
                        double x = plotMin + (plotMax - plotMin) * i / plotSteps;
                        // Численная производная корректна, т.к. функция определена на всей прямой
                        double numDeriv = NumericalFirstDeriv(x, h);
                        serDeriv1.Points.AddXY(x, numDeriv);

                        // Если точка попадает в интервал погрешностей, считаем ошибку
                        if (canCalcError && x >= errorMin && x <= errorMax)
                        {
                            double exact = ExactFirstDerivative(x);
                            errorsFirst.Add(Math.Abs(numDeriv - exact));
                        }
                    }
                    myChart.Series.Add(serDeriv1);
                }

                if (showSecond)
                {
                    Series serDeriv2 = new Series("f''(x) числ.");
                    serDeriv2.ChartType = SeriesChartType.Line;
                    serDeriv2.ChartArea = areaName;
                    serDeriv2.BorderWidth = 2;
                    serDeriv2.Color = Color.Purple;
                    serDeriv2.Legend = "Legend";

                    for (int i = 0; i <= plotSteps; i++)
                    {
                        double x = plotMin + (plotMax - plotMin) * i / plotSteps;
                        double numDeriv2 = NumericalSecondDeriv(x, h);
                        serDeriv2.Points.AddXY(x, numDeriv2);

                        if (canCalcError && x >= errorMin && x <= errorMax)
                        {
                            double exact2 = ExactSecondDerivative(x);
                            errorsSecond.Add(Math.Abs(numDeriv2 - exact2));
                        }
                    }
                    myChart.Series.Add(serDeriv2);
                }

                // Вывод погрешностей
                string errorMsg = "";
                if (errorsFirst.Count > 0)
                    errorMsg += $"f'(x): макс. погр. = {errorsFirst.Max():E4}, средняя = {errorsFirst.Average():E4}  ";
                if (errorsSecond.Count > 0)
                    errorMsg += $"f''(x): макс. погр. = {errorsSecond.Max():E4}, средняя = {errorsSecond.Average():E4}";
                txtErrors.Text = errorMsg;
            }
        }

        private void SetupArea(ChartArea area)
        {
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 10;
            area.AxisY.Minimum = -5;
            area.AxisY.Maximum = 15;

            area.AxisX.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisX.MajorGrid.Interval = 1;
            area.AxisY.MajorGrid.Interval = 1;
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;

            area.AxisX.LabelStyle.Interval = 1;
            area.AxisX.LabelStyle.Format = "{0:F0}";
            area.AxisX.LabelStyle.Font = new Font("Arial", 9);

            area.AxisY.LabelStyle.Interval = 5;
            area.AxisY.LabelStyle.Format = "{0:F0}";
            area.AxisY.LabelStyle.Font = new Font("Arial", 9);

            area.AxisX.Title = "Ось X";
            area.AxisX.TitleFont = new Font("Arial", 11, FontStyle.Bold);
            area.AxisY.Title = "Ось Y";
            area.AxisY.TitleFont = new Font("Arial", 11, FontStyle.Bold);

            area.BorderColor = Color.Black;
            area.BorderWidth = 1;
            area.BorderDashStyle = ChartDashStyle.Solid;
        }
    }
}