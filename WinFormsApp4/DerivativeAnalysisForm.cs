using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinFormsApp4
{
    public class DerivativeAnalysisForm : Form
    {
        private List<CustomPolynomial> splineSegments;
        private List<(double x, double y)> originalPoints;
        private Chart chartDerivative1;
        private Chart chartDerivative2;
        private ComboBox cmbAnalysisType;
        private NumericUpDown nudStep1, nudStep2, nudStep3;
        private Button btnAnalyze;
        private TextBox txtResults;
        private Label lblMaxError1, lblAvgError1, lblMaxError2, lblAvgError2;
        private List<double[]> allErrors = new List<double[]>();

        public DerivativeAnalysisForm(List<CustomPolynomial> segments, List<(double x, double y)> points)
        {
            this.splineSegments = segments;
            this.originalPoints = points;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Анализ производных кубического сплайна";
            this.Width = 1300;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Панель управления
            Panel controlPanel = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(10) };
            controlPanel.BackColor = Color.FromArgb(240, 240, 240);

            Label lblType = new Label { Text = "Тип анализа:", Left = 10, Top = 15, Width = 100, Font = new Font("Arial", 9, FontStyle.Bold) };
            cmbAnalysisType = new ComboBox { Left = 120, Top = 12, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAnalysisType.Items.AddRange(new[] { "Первая производная", "Вторая производная", "Обе производные" });
            cmbAnalysisType.SelectedIndex = 2;

            Label lblStep = new Label { Text = "Шаги для исследования:", Left = 300, Top = 15, Width = 150, Font = new Font("Arial", 9, FontStyle.Bold) };

            Label lblStep1 = new Label { Text = "h₁ =", Left = 460, Top = 15, Width = 30 };
            nudStep1 = new NumericUpDown { Left = 495, Top = 12, Width = 70, Minimum = 0.001m, Maximum = 1m, Value = 0.1m, DecimalPlaces = 3, Increment = 0.01m };

            Label lblStep2 = new Label { Text = "h₂ =", Left = 575, Top = 15, Width = 30 };
            nudStep2 = new NumericUpDown { Left = 610, Top = 12, Width = 70, Minimum = 0.001m, Maximum = 1m, Value = 0.05m, DecimalPlaces = 3, Increment = 0.01m };

            Label lblStep3 = new Label { Text = "h₃ =", Left = 690, Top = 15, Width = 30 };
            nudStep3 = new NumericUpDown { Left = 725, Top = 12, Width = 70, Minimum = 0.001m, Maximum = 1m, Value = 0.01m, DecimalPlaces = 3, Increment = 0.01m };

            btnAnalyze = new Button { Text = "▶ Выполнить анализ", Left = 820, Top = 10, Width = 140, Height = 30, BackColor = Color.LightBlue, Font = new Font("Arial", 9, FontStyle.Bold) };
            btnAnalyze.Click += BtnAnalyze_Click;

            // Панель результатов
            Panel resultPanel = new Panel { Left = 10, Top = 50, Width = 1000, Height = 55, BackColor = Color.FromArgb(255, 255, 224) };
            resultPanel.BorderStyle = BorderStyle.FixedSingle;

            Label lblMax1 = new Label { Text = "Макс. погрешность f'(x):", Left = 10, Top = 8, Width = 150, Font = new Font("Arial", 8, FontStyle.Bold) };
            lblMaxError1 = new Label { Text = "—", Left = 170, Top = 8, Width = 120, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8) };

            Label lblAvg1 = new Label { Text = "Сред. погрешность f'(x):", Left = 310, Top = 8, Width = 150, Font = new Font("Arial", 8, FontStyle.Bold) };
            lblAvgError1 = new Label { Text = "—", Left = 470, Top = 8, Width = 120, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8) };

            Label lblMax2 = new Label { Text = "Макс. погрешность f''(x):", Left = 610, Top = 8, Width = 150, Font = new Font("Arial", 8, FontStyle.Bold) };
            lblMaxError2 = new Label { Text = "—", Left = 770, Top = 8, Width = 120, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8) };

            Label lblAvg2 = new Label { Text = "Сред. погрешность f''(x):", Left = 10, Top = 32, Width = 150, Font = new Font("Arial", 8, FontStyle.Bold) };
            lblAvgError2 = new Label { Text = "—", Left = 170, Top = 32, Width = 120, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8) };

            resultPanel.Controls.AddRange(new Control[] { lblMax1, lblMaxError1, lblAvg1, lblAvgError1, lblMax2, lblMaxError2, lblAvg2, lblAvgError2 });

            controlPanel.Controls.AddRange(new Control[] { lblType, cmbAnalysisType, lblStep, lblStep1, nudStep1, lblStep2, nudStep2, lblStep3, nudStep3, btnAnalyze, resultPanel });

            // TabControl для графиков
            TabControl tabControl = new TabControl { Dock = DockStyle.Fill };

            TabPage pageDeriv1 = new TabPage("📈 Первая производная f'(x)");
            chartDerivative1 = CreateChart("Первая производная", "x", "f'(x)");
            pageDeriv1.Controls.Add(chartDerivative1);

            TabPage pageDeriv2 = new TabPage("📉 Вторая производная f''(x)");
            chartDerivative2 = CreateChart("Вторая производная", "x", "f''(x)");
            pageDeriv2.Controls.Add(chartDerivative2);

            tabControl.TabPages.Add(pageDeriv1);
            tabControl.TabPages.Add(pageDeriv2);

            // Текстовое поле для детальных результатов
            txtResults = new TextBox { Dock = DockStyle.Bottom, Height = 200, Multiline = true, ReadOnly = true, Font = new Font("Consolas", 8), ScrollBars = ScrollBars.Both, WordWrap = false, BackColor = Color.FromArgb(250, 250, 250) };

            this.Controls.Add(tabControl);
            this.Controls.Add(txtResults);
            this.Controls.Add(controlPanel);
        }

        private Chart CreateChart(string title, string xTitle, string yTitle)
        {
            Chart chart = new Chart { Dock = DockStyle.Fill };
            ChartArea area = new ChartArea();
            area.AxisX.Title = xTitle;
            area.AxisY.Title = yTitle;
            area.AxisX.TitleFont = new Font("Arial", 10, FontStyle.Bold);
            area.AxisY.TitleFont = new Font("Arial", 10, FontStyle.Bold);
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas.Add(area);
            chart.Legends.Add(new Legend() { Docking = Docking.Top, Font = new Font("Arial", 8) });
            return chart;
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                PerformAnalysis();
                Cursor = Cursors.Default;
                MessageBox.Show("Анализ завершен!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PerformAnalysis()
        {
            chartDerivative1.Series.Clear();
            chartDerivative2.Series.Clear();

            double[] steps = { (double)nudStep1.Value, (double)nudStep2.Value, (double)nudStep3.Value };
            string[] stepNames = { "Шаг h₁ = " + steps[0], "Шаг h₂ = " + steps[1], "Шаг h₃ = " + steps[2] };

            txtResults.Clear();
            txtResults.AppendText("═══════════════════════════════════════════════════════════════════════════════════\n");
            txtResults.AppendText("              АНАЛИЗ ПРОИЗВОДНЫХ КУБИЧЕСКОГО СПЛАЙНА (дефект 1)\n");
            txtResults.AppendText("═══════════════════════════════════════════════════════════════════════════════════\n\n");

            txtResults.AppendText("📌 ИСХОДНЫЕ ДАННЫЕ:\n");
            txtResults.AppendText("┌─────┬──────────┬──────────┐\n");
            txtResults.AppendText("│  i  │    x     │   f(x)   │\n");
            txtResults.AppendText("├─────┼──────────┼──────────┤\n");
            for (int i = 0; i < originalPoints.Count; i++)
            {
                txtResults.AppendText($"│ {i + 1,2} │ {originalPoints[i].x,8:F3} │ {originalPoints[i].y,8:F3} │\n");
            }
            txtResults.AppendText("└─────┴──────────┴──────────┘\n\n");

            txtResults.AppendText("📌 ИНТЕРВАЛЫ СПЛАЙНА:\n");
            foreach (var seg in splineSegments)
            {
                txtResults.AppendText($"   [{seg.Xmin:F3}; {seg.Xmax:F3}]  f(x) = {seg.A:F3} + {seg.B:F3}·dx + {seg.C:F3}·dx² + {seg.D:F3}·dx³\n");
                txtResults.AppendText($"          f'(x) = {seg.B:F3} + {2 * seg.C:F3}·dx + {3 * seg.D:F3}·dx²\n");
                txtResults.AppendText($"          f''(x) = {2 * seg.C:F3} + {6 * seg.D:F3}·dx\n\n");
            }

            // Вычисляем аналитические производные
            var exactDerivatives1 = ComputeExactDerivatives1();
            var exactDerivatives2 = ComputeExactDerivatives2();

            allErrors.Clear();

            // Для каждого шага
            for (int idx = 0; idx < steps.Length; idx++)
            {
                double h = steps[idx];
                txtResults.AppendText($"\n{RepeatString("═", 70)}\n");
                txtResults.AppendText($"   РЕЗУЛЬТАТЫ ДЛЯ {stepNames[idx].ToUpper()}\n");
                txtResults.AppendText($"{RepeatString("═", 70)}\n");

                var numericalDerivatives1 = ComputeNumericalDerivative1(h);
                var numericalDerivatives2 = ComputeNumericalDerivative2(h);

                if (cmbAnalysisType.SelectedIndex != 1) // не только вторая
                {
                    var errors1 = CompareDerivatives(exactDerivatives1, numericalDerivatives1, "первой", h);
                    DrawDerivativeChart(chartDerivative1, exactDerivatives1, numericalDerivatives1, h, stepNames[idx]);

                    if (idx == 1) // средний шаг выводим в метки
                    {
                        double maxErr = errors1.Max(e => e.error);
                        double avgErr = errors1.Average(e => e.error);
                        lblMaxError1.Text = maxErr.ToString("E3");
                        lblAvgError1.Text = avgErr.ToString("E3");
                    }
                }

                if (cmbAnalysisType.SelectedIndex != 0) // не только первая
                {
                    var errors2 = CompareDerivatives(exactDerivatives2, numericalDerivatives2, "второй", h);
                    DrawDerivativeChart(chartDerivative2, exactDerivatives2, numericalDerivatives2, h, stepNames[idx]);

                    if (idx == 1)
                    {
                        double maxErr = errors2.Max(e => e.error);
                        double avgErr = errors2.Average(e => e.error);
                        lblMaxError2.Text = maxErr.ToString("E3");
                        lblAvgError2.Text = avgErr.ToString("E3");
                    }
                }
            }

            // Вывод рекомендаций
            txtResults.AppendText($"\n{RepeatString("═", 70)}\n");
            txtResults.AppendText("              ВЫВОДЫ И РЕКОМЕНДАЦИИ\n");
            txtResults.AppendText($"{RepeatString("═", 70)}\n");
            txtResults.AppendText("1. Оптимальный шаг для первой производной: h ≈ 0.01 ÷ 0.05\n");
            txtResults.AppendText("   • При слишком большом шаге (0.1) — большая погрешность аппроксимации\n");
            txtResults.AppendText("   • При слишком малом шаге (0.001) — накопление вычислительной погрешности\n");
            txtResults.AppendText("\n2. Оптимальный шаг для второй производной: h ≈ 0.01 ÷ 0.1\n");
            txtResults.AppendText("   • Вторая производная более чувствительна к ошибкам округления\n");
            txtResults.AppendText("   • Формула f''(x) ≈ (f(x+h)-2f(x)+f(x-h))/h² требует большего шага\n");
            txtResults.AppendText("\n3. Особенности кубического сплайна:\n");
            txtResults.AppendText("   • Первая производная непрерывна во всех точках\n");
            txtResults.AppendText("   • Вторая производная также непрерывна (дефект 1)\n");
            txtResults.AppendText("   • Третья производная — кусочно-постоянная\n");

            txtResults.Select(0, 0);
        }

        private List<(double x, double value)> ComputeExactDerivatives1()
        {
            var result = new List<(double x, double value)>();
            double step = 0.05;

            foreach (var segment in splineSegments)
            {
                for (double x = segment.Xmin; x <= segment.Xmax + 0.001; x += step)
                {
                    if (x <= segment.Xmax + 0.001)
                        result.Add((x, segment.FirstDerivative(x)));
                }
            }
            return result.OrderBy(r => r.x).ToList();
        }

        private List<(double x, double value)> ComputeExactDerivatives2()
        {
            var result = new List<(double x, double value)>();
            double step = 0.05;

            foreach (var segment in splineSegments)
            {
                for (double x = segment.Xmin; x <= segment.Xmax + 0.001; x += step)
                {
                    if (x <= segment.Xmax + 0.001)
                        result.Add((x, segment.SecondDerivative(x)));
                }
            }
            return result.OrderBy(r => r.x).ToList();
        }

        private List<(double x, double value)> ComputeNumericalDerivative1(double h)
        {
            var result = new List<(double x, double value)>();

            foreach (var segment in splineSegments)
            {
                for (double x = segment.Xmin + h; x <= segment.Xmax - h; x += 0.05)
                {
                    double f_plus = segment.Value(x + h);
                    double f_minus = segment.Value(x - h);
                    double derivative = (f_plus - f_minus) / (2 * h);
                    result.Add((x, derivative));
                }
            }
            return result.OrderBy(r => r.x).ToList();
        }

        private List<(double x, double value)> ComputeNumericalDerivative2(double h)
        {
            var result = new List<(double x, double value)>();

            foreach (var segment in splineSegments)
            {
                for (double x = segment.Xmin + h; x <= segment.Xmax - h; x += 0.05)
                {
                    double f_plus = segment.Value(x + h);
                    double f = segment.Value(x);
                    double f_minus = segment.Value(x - h);
                    double derivative = (f_plus - 2 * f + f_minus) / (h * h);
                    result.Add((x, derivative));
                }
            }
            return result.OrderBy(r => r.x).ToList();
        }

        private List<(double x, double error)> CompareDerivatives(List<(double x, double value)> exact,
                                    List<(double x, double value)> numerical,
                                    string derivativeName, double h)
        {
            var errors = new List<(double x, double error)>();

            txtResults.AppendText($"\n┌{RepeatString("─", 68)}┐\n");
            txtResults.AppendText($"│ Анализ {derivativeName} производной (h = {h}){RepeatString(" ", 68 - 28 - derivativeName.Length)}│\n");
            txtResults.AppendText($"├{RepeatString("─", 15)}┬{RepeatString("─", 18)}┬{RepeatString("─", 18)}┬{RepeatString("─", 15)}┤\n");
            txtResults.AppendText($"│ {"x",-13} │ {"Точное значение",-16} │ {"Численное значение",-16} │ {"Погрешность",-13} │\n");
            txtResults.AppendText($"├{RepeatString("─", 15)}┼{RepeatString("─", 18)}┼{RepeatString("─", 18)}┼{RepeatString("─", 15)}┤\n");

            int minCount = Math.Min(exact.Count, numerical.Count);
            int displayed = 0;

            for (int i = 0; i < minCount; i += Math.Max(1, minCount / 30))
            {
                double error = Math.Abs(exact[i].value - numerical[i].value);
                errors.Add((exact[i].x, error));

                if (displayed < 20 || i >= minCount - 10)
                {
                    txtResults.AppendText($"│ {exact[i].x,13:F4} │ {exact[i].value,16:F6} │ {numerical[i].value,16:F6} │ {error,13:E6} │\n");
                    displayed++;
                }
                else if (displayed == 20)
                {
                    txtResults.AppendText($"│ {"...",13} │ {"...",16} │ {"...",16} │ {"...",13} │\n");
                    displayed++;
                }
            }

            double maxError = errors.Max(e => e.error);
            double avgError = errors.Average(e => e.error);

            txtResults.AppendText($"├{RepeatString("─", 15)}┴{RepeatString("─", 18)}┴{RepeatString("─", 18)}┴{RepeatString("─", 15)}┤\n");
            txtResults.AppendText($"│ МАКСИМАЛЬНАЯ ПОГРЕШНОСТЬ: {maxError,40:E6} │\n");
            txtResults.AppendText($"│ СРЕДНЯЯ ПОГРЕШНОСТЬ:     {avgError,40:E6} │\n");
            txtResults.AppendText($"└{RepeatString("─", 70)}┘\n");

            return errors;
        }

        private void DrawDerivativeChart(Chart chart, List<(double x, double value)> exact,
                                         List<(double x, double value)> numerical,
                                         double h, string stepName)
        {
            string seriesName = $"Численное дифференцирование ({stepName})";

            // Находим или создаем серию для точного значения
            Series? exactSeries = chart.Series.FindByName("Точное значение (аналитическое)");
            if (exactSeries == null)
            {
                exactSeries = new Series("Точное значение (аналитическое)");
                exactSeries.ChartType = SeriesChartType.Line;
                exactSeries.BorderWidth = 3;
                exactSeries.Color = Color.Blue;
                chart.Series.Add(exactSeries);

                foreach (var point in exact)
                    exactSeries.Points.AddXY(point.x, point.value);
            }

            // Добавляем численное значение
            Series numSeries = new Series(seriesName);
            numSeries.ChartType = SeriesChartType.Line;
            numSeries.BorderWidth = 2;
            numSeries.Color = h == 0.1 ? Color.Red : (h == 0.05 ? Color.Orange : Color.Green);
            numSeries.BorderDashStyle = h == 0.1 ? ChartDashStyle.Solid : (h == 0.05 ? ChartDashStyle.Dash : ChartDashStyle.Dot);

            foreach (var point in numerical)
                numSeries.Points.AddXY(point.x, point.value);
            chart.Series.Add(numSeries);

            // Добавляем узлы интерполяции
            Series pointsSeries = chart.Series.FindByName("Узлы интерполяции");
            if (pointsSeries == null)
            {
                pointsSeries = new Series("Узлы интерполяции");
                pointsSeries.ChartType = SeriesChartType.Point;
                pointsSeries.MarkerStyle = MarkerStyle.Circle;
                pointsSeries.MarkerSize = 8;
                pointsSeries.MarkerColor = Color.Black;
                pointsSeries.Color = Color.Black;
                chart.Series.Add(pointsSeries);

                foreach (var p in originalPoints)
                {
                    double derivValue = 0;
                    foreach (var seg in splineSegments)
                    {
                        if (p.x >= seg.Xmin - 0.001 && p.x <= seg.Xmax + 0.001)
                        {
                            derivValue = chart.Name.Contains("Первая") ? seg.FirstDerivative(p.x) : seg.SecondDerivative(p.x);
                            break;
                        }
                    }
                    pointsSeries.Points.AddXY(p.x, derivValue);
                }
            }

            chart.ChartAreas[0].RecalculateAxesScale();
            chart.Invalidate();
        }

        // Вспомогательный метод для повторения строк
        private string RepeatString(string str, int count)
        {
            return new string(str[0], count);
        }
    }
}