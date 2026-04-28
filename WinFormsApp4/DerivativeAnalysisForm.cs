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
        private Label lblMaxError1, lblAvgError1, lblMaxError2, lblAvgError2;
        private RichTextBox rtbResults;

        public DerivativeAnalysisForm(List<CustomPolynomial> segments, List<(double x, double y)> points)
        {
            this.splineSegments = segments;
            this.originalPoints = points;
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
        }

        private void InitializeComponent()
        {
            this.Text = "Анализ производных кубического сплайна";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Создаем горизонтальный SplitContainer для деления экрана пополам
            SplitContainer mainSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = this.Width / 2,
                SplitterWidth = 5,
                BackColor = Color.Gray
            };

            // ==================== ЛЕВАЯ ПОЛОВИНА: ГРАФИКИ ====================
            Panel leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; // Добавил отступы

            TabControl tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(10, 10) // Отступы внутри вкладок
            };

            TabPage pageDeriv1 = new TabPage(" Первая производная f'(x) ");
            chartDerivative1 = CreateChart();
            chartDerivative1.Dock = DockStyle.Fill;
            chartDerivative1.Padding = new Padding(10); // Отступы для графика

            // Добавляем панель для графика с отступами
            Panel chartPanel1 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            chartPanel1.Controls.Add(chartDerivative1);
            pageDeriv1.Controls.Add(chartPanel1);

            TabPage pageDeriv2 = new TabPage(" Вторая производная f''(x) ");
            chartDerivative2 = CreateChart();
            chartDerivative2.Dock = DockStyle.Fill;
            chartDerivative2.Padding = new Padding(10);

            Panel chartPanel2 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            chartPanel2.Controls.Add(chartDerivative2);
            pageDeriv2.Controls.Add(chartPanel2);

            tabControl.TabPages.Add(pageDeriv1);
            tabControl.TabPages.Add(pageDeriv2);
            leftPanel.Controls.Add(tabControl);

            // ==================== ПРАВАЯ ПОЛОВИНА: АНАЛИЗ ====================
            Panel rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(240, 248, 255)
            };

            // Панель управления сверху
            Panel controlPanel = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(5) };

            // Тип анализа
            Label lblType = new Label { Text = "Тип анализа:", Left = 5, Top = 10, Width = 80, Font = new Font("Arial", 9, FontStyle.Bold) };
            cmbAnalysisType = new ComboBox { Left = 90, Top = 7, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Arial", 9) };
            cmbAnalysisType.Items.AddRange(new[] { "Первая производная", "Вторая производная", "Обе производные" });
            cmbAnalysisType.SelectedIndex = 2;

            // Шаги
            Label lblStep = new Label { Text = "Шаги:", Left = 250, Top = 10, Width = 40, Font = new Font("Arial", 9, FontStyle.Bold) };

            Label lblStep1 = new Label { Text = "h1 =", Left = 295, Top = 10, Width = 25 };
            nudStep1 = new NumericUpDown { Left = 325, Top = 7, Width = 70, Minimum = 0.001m, Maximum = 1m, Value = 0.1m, DecimalPlaces = 3, Increment = 0.01m, Font = new Font("Consolas", 8) };

            Label lblStep2 = new Label { Text = "h2 =", Left = 405, Top = 10, Width = 25 };
            nudStep2 = new NumericUpDown { Left = 435, Top = 7, Width = 70, Minimum = 0.001m, Maximum = 1m, Value = 0.05m, DecimalPlaces = 3, Increment = 0.01m, Font = new Font("Consolas", 8) };

            Label lblStep3 = new Label { Text = "h3 =", Left = 515, Top = 10, Width = 25 };
            nudStep3 = new NumericUpDown { Left = 545, Top = 7, Width = 70, Minimum = 0.001m, Maximum = 1m, Value = 0.01m, DecimalPlaces = 3, Increment = 0.01m, Font = new Font("Consolas", 8) };

            // Кнопка анализа
            btnAnalyze = new Button
            {
                Text = "ВЫПОЛНИТЬ АНАЛИЗ",
                Left = 630,
                Top = 5,
                Width = 150,
                Height = 30,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnAnalyze.Click += BtnAnalyze_Click;

            // Панель результатов с погрешностями
            Panel resultPanel = new Panel
            {
                Top = 50,
                Left = 5,
                Width = rightPanel.Width - 20,
                Height = 65,
                BackColor = Color.FromArgb(255, 255, 224),
                BorderStyle = BorderStyle.FixedSingle
            };
            resultPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            Label lblMax1 = new Label { Text = "Макс. погрешность f'(x):", Left = 5, Top = 5, Width = 140, Font = new Font("Arial", 8, FontStyle.Bold) };
            lblMaxError1 = new Label { Text = "—", Left = 150, Top = 5, Width = 100, Height = 22, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };

            Label lblAvg1 = new Label { Text = "Сред. погрешность f'(x):", Left = 265, Top = 5, Width = 140, Font = new Font("Arial", 8, FontStyle.Bold) };
            lblAvgError1 = new Label { Text = "—", Left = 410, Top = 5, Width = 100, Height = 22, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };

            Label lblMax2 = new Label { Text = "Макс. погрешность f''(x):", Left = 5, Top = 35, Width = 140, Font = new Font("Arial", 8, FontStyle.Bold) };
            lblMaxError2 = new Label { Text = "—", Left = 150, Top = 35, Width = 100, Height = 22, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };

            Label lblAvg2 = new Label { Text = "Сред. погрешность f''(x):", Left = 265, Top = 35, Width = 140, Font = new Font("Arial", 8, FontStyle.Bold) };
            lblAvgError2 = new Label { Text = "—", Left = 410, Top = 35, Width = 100, Height = 22, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };

            resultPanel.Controls.AddRange(new Control[] { lblMax1, lblMaxError1, lblAvg1, lblAvgError1, lblMax2, lblMaxError2, lblAvg2, lblAvgError2 });

            controlPanel.Controls.AddRange(new Control[] { lblType, cmbAnalysisType, lblStep, lblStep1, nudStep1, lblStep2, nudStep2, lblStep3, nudStep3, btnAnalyze, resultPanel });

            // RichTextBox для детальных результатов (занимает оставшееся место)
            rtbResults = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(250, 250, 250),
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.ForcedVertical
            };

            rightPanel.Controls.Add(rtbResults);
            rightPanel.Controls.Add(controlPanel);

            // Собираем SplitContainer
            mainSplitContainer.Panel1.Controls.Add(leftPanel);
            mainSplitContainer.Panel2.Controls.Add(rightPanel);

            this.Controls.Add(mainSplitContainer);

            // Обработчик изменения размера окна для сохранения пропорций
            this.Resize += (s, e) =>
            {
                if (mainSplitContainer.Width > 0)
                    mainSplitContainer.SplitterDistance = mainSplitContainer.Width / 2;

                // Обновляем размер панели результатов
                if (resultPanel != null && rightPanel != null)
                {
                    resultPanel.Width = rightPanel.Width - 20;
                }
            };
        }

        private Chart CreateChart()
        {
            Chart chart = new Chart();
            ChartArea area = new ChartArea();
            area.AxisX.Title = "x";
            area.AxisY.Title = "f'(x) / f''(x)";
            area.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
            area.AxisY.TitleFont = new Font("Arial", 12, FontStyle.Bold);
            area.AxisX.LabelStyle.Font = new Font("Arial", 10);
            area.AxisY.LabelStyle.Font = new Font("Arial", 10);
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.BackColor = Color.White;
            area.InnerPlotPosition.Auto = false;
            area.InnerPlotPosition.Height = 85;
            area.InnerPlotPosition.Width = 90;
            area.InnerPlotPosition.X = 8;
            area.InnerPlotPosition.Y = 5;
            chart.ChartAreas.Add(area);
            chart.Legends.Add(new Legend() { Docking = Docking.Top, Font = new Font("Arial", 9) });
            return chart;
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                PerformAnalysis();
                Cursor = Cursors.Default;
                MessageBox.Show("Анализ успешно завершен!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            string[] stepNames = { "h1 = " + steps[0], "h2 = " + steps[1], "h3 = " + steps[2] };

            rtbResults.Clear();

            // Заголовок
            AppendColoredText("================================================================================\n", Color.DarkBlue);
            AppendColoredText("     АНАЛИЗ ПРОИЗВОДНЫХ КУБИЧЕСКОГО СПЛАЙНА (дефект 1)\n", Color.DarkBlue, true);
            AppendColoredText("================================================================================\n\n", Color.DarkBlue);

            // Исходные данные
            AppendColoredText("ИСХОДНЫЕ ДАННЫЕ:\n", Color.DarkGreen, true);
            AppendColoredText("-----------------------------------------------------------\n", Color.DarkGreen);
            for (int i = 0; i < originalPoints.Count; i++)
            {
                AppendColoredText($"  x[{i + 1}] = {originalPoints[i].x,8:F4}    f(x[{i + 1}]) = {originalPoints[i].y,8:F4}\n", Color.Black);
            }
            AppendColoredText("\n", Color.Black);

            // Интервалы сплайна
            AppendColoredText("ИНТЕРВАЛЫ СПЛАЙНА:\n", Color.DarkOrange, true);
            AppendColoredText("-----------------------------------------------------------\n", Color.DarkOrange);
            foreach (var seg in splineSegments)
            {
                AppendColoredText($"  x in [{seg.Xmin:F4}; {seg.Xmax:F4}]\n", Color.Black);
                AppendColoredText($"    f(x)  = {seg.A:F4} + {seg.B:F4}*dx + {seg.C:F4}*dx^2 + {seg.D:F4}*dx^3\n", Color.Black);
                AppendColoredText($"    f'(x) = {seg.B:F4} + {2 * seg.C:F4}*dx + {3 * seg.D:F4}*dx^2\n", Color.Black);
                AppendColoredText($"    f''(x)= {2 * seg.C:F4} + {6 * seg.D:F4}*dx\n\n", Color.Black);
            }

            // Вычисляем аналитические производные
            var exactDerivatives1 = ComputeExactDerivatives1();
            var exactDerivatives2 = ComputeExactDerivatives2();

            // Для каждого шага
            for (int idx = 0; idx < steps.Length; idx++)
            {
                double h = steps[idx];

                AppendColoredText($"\n{"=".Repeat(80)}\n", Color.DarkBlue);
                AppendColoredText($"  РЕЗУЛЬТАТЫ ДЛЯ {stepNames[idx].ToUpper()}\n", Color.DarkBlue, true);
                AppendColoredText($"{"=".Repeat(80)}\n", Color.DarkBlue);

                var numericalDerivatives1 = ComputeNumericalDerivative1(h);
                var numericalDerivatives2 = ComputeNumericalDerivative2(h);

                if (cmbAnalysisType.SelectedIndex != 1)
                {
                    var errors1 = CompareDerivativesNice(exactDerivatives1, numericalDerivatives1, "первой", h);
                    DrawDerivativeChart(chartDerivative1, exactDerivatives1, numericalDerivatives1, h, stepNames[idx]);

                    if (idx == 1)
                    {
                        double maxErr = errors1.Max(e => e.error);
                        double avgErr = errors1.Average(e => e.error);
                        lblMaxError1.Text = maxErr.ToString("E3");
                        lblAvgError1.Text = avgErr.ToString("E3");
                    }
                }

                if (cmbAnalysisType.SelectedIndex != 0)
                {
                    var errors2 = CompareDerivativesNice(exactDerivatives2, numericalDerivatives2, "второй", h);
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
            AppendColoredText($"\n{"=".Repeat(80)}\n", Color.DarkGreen);
            AppendColoredText("  ВЫВОДЫ И РЕКОМЕНДАЦИИ\n", Color.DarkGreen, true);
            AppendColoredText($"{"=".Repeat(80)}\n", Color.DarkGreen);

            AppendColoredText("\n1. Оптимальный шаг для ПЕРВОЙ производной: h = 0.01 - 0.05\n", Color.Blue);
            AppendColoredText("   • Слишком большой шаг (0.1) -> большая погрешность аппроксимации\n", Color.Black);
            AppendColoredText("   • Слишком малый шаг (0.001) -> накопление вычислительной погрешности\n", Color.Black);

            AppendColoredText("\n2. Оптимальный шаг для ВТОРОЙ производной: h = 0.01 - 0.1\n", Color.Blue);
            AppendColoredText("   • Вторая производная более чувствительна к ошибкам округления\n", Color.Black);
            AppendColoredText("   • Формула требует большего шага для устойчивости\n", Color.Black);

            AppendColoredText("\n3. Особенности кубического сплайна (дефект 1):\n", Color.Blue);
            AppendColoredText("   • Первая производная НЕПРЕРЫВНА во всех точках\n", Color.Black);
            AppendColoredText("   • Вторая производная также НЕПРЕРЫВНА\n", Color.Black);
            AppendColoredText("   • Третья производная — кусочно-постоянная (разрывна)\n", Color.Black);

            rtbResults.Select(0, 0);
        }

        private List<(double x, double error)> CompareDerivativesNice(List<(double x, double value)> exact,
                                    List<(double x, double value)> numerical,
                                    string derivativeName, double h)
        {
            var errors = new List<(double x, double error)>();

            AppendColoredText($"\n--- АНАЛИЗ {derivativeName.ToUpper()} ПРОИЗВОДНОЙ (шаг = {h}) ---\n", Color.DarkCyan, true);
            AppendColoredText($"{new string(' ', 5)}{"x",-12} {"ТОЧНОЕ",-16} {"ЧИСЛЕННОЕ",-16} {"ПОГРЕШНОСТЬ",-15}\n", Color.DarkCyan);
            AppendColoredText($"  {new string('-', 65)}\n", Color.DarkCyan);

            int minCount = Math.Min(exact.Count, numerical.Count);
            int displayed = 0;
            int step = Math.Max(1, minCount / 25);

            for (int i = 0; i < minCount; i += step)
            {
                double error = Math.Abs(exact[i].value - numerical[i].value);
                errors.Add((exact[i].x, error));

                if (displayed < 15 || i >= minCount - 8)
                {
                    string line = $"  {exact[i].x,12:F6}   {exact[i].value,16:F8}   {numerical[i].value,16:F8}   {error,15:E6}\n";
                    Color errorColor = error < 1e-6 ? Color.Green : (error < 1e-4 ? Color.Orange : Color.Red);
                    AppendColoredText(line, errorColor);
                    displayed++;
                }
                else if (displayed == 15)
                {
                    AppendColoredText($"  {"...",12}   {"...",16}   {"...",16}   {"...",15}\n", Color.Gray);
                    displayed++;
                }
            }

            double maxError = errors.Max(e => e.error);
            double avgError = errors.Average(e => e.error);

            AppendColoredText($"  {new string('-', 65)}\n", Color.DarkCyan);
            AppendColoredText($"  МАКСИМАЛЬНАЯ ПОГРЕШНОСТЬ: {maxError:E6}\n", Color.DarkRed, true);
            AppendColoredText($"  СРЕДНЯЯ ПОГРЕШНОСТЬ:     {avgError:E6}\n", Color.DarkBlue, true);

            return errors;
        }

        private void AppendColoredText(string text, Color color, bool bold = false, Color[]? specificColors = null)
        {
            rtbResults.SelectionStart = rtbResults.TextLength;
            rtbResults.SelectionLength = 0;
            rtbResults.SelectionColor = color;

            if (bold)
                rtbResults.SelectionFont = new Font(rtbResults.Font, FontStyle.Bold);
            else
                rtbResults.SelectionFont = new Font(rtbResults.Font, FontStyle.Regular);

            rtbResults.AppendText(text);
            rtbResults.SelectionColor = Color.Black;
            rtbResults.SelectionFont = new Font(rtbResults.Font, FontStyle.Regular);
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

        private void DrawDerivativeChart(Chart chart, List<(double x, double value)> exact,
                                         List<(double x, double value)> numerical,
                                         double h, string stepName)
        {
            string seriesName = $"Численное ({stepName})";

            Series? exactSeries = chart.Series.FindByName("Точное значение");
            if (exactSeries == null)
            {
                exactSeries = new Series("Точное значение");
                exactSeries.ChartType = SeriesChartType.Line;
                exactSeries.BorderWidth = 3;
                exactSeries.Color = Color.Blue;
                chart.Series.Add(exactSeries);

                foreach (var point in exact)
                    exactSeries.Points.AddXY(point.x, point.value);
            }

            Series numSeries = new Series(seriesName);
            numSeries.ChartType = SeriesChartType.Line;
            numSeries.BorderWidth = 2;
            numSeries.Color = h == 0.1 ? Color.Red : (h == 0.05 ? Color.Orange : Color.Green);
            numSeries.BorderDashStyle = h == 0.1 ? ChartDashStyle.Solid : (h == 0.05 ? ChartDashStyle.Dash : ChartDashStyle.Dot);

            foreach (var point in numerical)
                numSeries.Points.AddXY(point.x, point.value);
            chart.Series.Add(numSeries);

            Series? pointsSeries = chart.Series.FindByName("Узлы");
            if (pointsSeries == null && originalPoints.Count > 0)
            {
                pointsSeries = new Series("Узлы");
                pointsSeries.ChartType = SeriesChartType.Point;
                pointsSeries.MarkerStyle = MarkerStyle.Circle;
                pointsSeries.MarkerSize = 10;
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
    }
}