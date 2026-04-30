using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

namespace SplinesWithDeriv
{
    public class Form1 : Form
    {
        private DataGridView dataGridView;
        private Button btnBuild, btnAddRow, btnSetCount;
        private NumericUpDown nudPointsCount;
        private Chart chart;
        private TextBox txtStatus;
        private List<double> xNodes = new List<double>();
        private List<double> yNodes = new List<double>();
        private List<CustomPolynomial> splineSegments = new List<CustomPolynomial>();

        private GroupBox groupBoxPolynomial;
        private TextBox txtA, txtB, txtC, txtD;
        private NumericUpDown nudXmin, nudXmax;
        private Button btnAddPolynomial;
        private CheckBox chkShowSpline;
        private List<CustomPolynomial> customPolynomials = new List<CustomPolynomial>();

        // Новые элементы для численных производных
        private TextBox txtH;
        private CheckBox chkFirstDeriv, chkSecondDeriv;
        private TextBox txtErrors;

        public Form1()
        {
            this.Text = "Кубический сплайн (дефект 1)";
            this.Width = 1300;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Верхняя панель – увеличиваем высоту до 270, чтобы влезли элементы погрешностей
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 270, Padding = new Padding(10) };

            Label lblCount = new Label { Text = "Количество точек:", Left = 10, Top = 15, Width = 120 };
            nudPointsCount = new NumericUpDown { Left = 140, Top = 12, Width = 60, Minimum = 2, Maximum = 20, Value = 5 };
            btnSetCount = new Button { Text = "Задать сетку", Left = 210, Top = 10, Width = 100 };
            btnSetCount.Click += BtnSetCount_Click;
            btnAddRow = new Button { Text = "Добавить точку", Left = 330, Top = 10, Width = 120 };
            btnAddRow.Click += BtnAddRow_Click;
            btnBuild = new Button { Text = "Построить сплайн", Left = 470, Top = 10, Width = 120 };
            btnBuild.Click += BtnBuild_Click;
            btnBuild.Enabled = false;

            chkShowSpline = new CheckBox { Text = "Показать сплайн", Left = 610, Top = 12, Width = 120, Checked = true };
            chkShowSpline.CheckedChanged += (s, e) => DrawChart();

            txtStatus = new TextBox { Left = 10, Top = 50, Width = 700, Height = 50, Multiline = true, ReadOnly = true, BackColor = Color.LightYellow };

            topPanel.Controls.Add(lblCount);
            topPanel.Controls.Add(nudPointsCount);
            topPanel.Controls.Add(btnSetCount);
            topPanel.Controls.Add(btnAddRow);
            topPanel.Controls.Add(btnBuild);
            topPanel.Controls.Add(chkShowSpline);
            topPanel.Controls.Add(txtStatus);

            // Группа для ввода многочлена
            groupBoxPolynomial = new GroupBox
            {
                Text = "Добавить произвольный многочлен 3-й степени: a·x³ + b·x² + c·x + d",
                Left = 10,
                Top = 110,
                Width = 700,
                Height = 55
            };

            Label lblA = new Label { Text = "a:", Left = 10, Top = 25, Width = 20 };
            txtA = new TextBox { Left = 35, Top = 22, Width = 60, Text = "0" };
            Label lblB = new Label { Text = "b:", Left = 105, Top = 25, Width = 20 };
            txtB = new TextBox { Left = 130, Top = 22, Width = 60, Text = "0" };
            Label lblC = new Label { Text = "c:", Left = 200, Top = 25, Width = 20 };
            txtC = new TextBox { Left = 225, Top = 22, Width = 60, Text = "0" };
            Label lblD = new Label { Text = "d:", Left = 295, Top = 25, Width = 20 };
            txtD = new TextBox { Left = 320, Top = 22, Width = 60, Text = "0" };

            Label lblXrange = new Label { Text = "x от:", Left = 400, Top = 25, Width = 35 };
            nudXmin = new NumericUpDown { Left = 440, Top = 22, Width = 60, Minimum = -100, Maximum = 100, Value = 0, DecimalPlaces = 1, Increment = 0.5m };
            Label lblTo = new Label { Text = "до:", Left = 505, Top = 25, Width = 25 };
            nudXmax = new NumericUpDown { Left = 535, Top = 22, Width = 60, Minimum = -100, Maximum = 100, Value = 10, DecimalPlaces = 1, Increment = 0.5m };

            btnAddPolynomial = new Button { Text = "Добавить многочлен", Left = 610, Top = 20, Width = 80 };
            btnAddPolynomial.Click += BtnAddPolynomial_Click;

            groupBoxPolynomial.Controls.Add(lblA);
            groupBoxPolynomial.Controls.Add(txtA);
            groupBoxPolynomial.Controls.Add(lblB);
            groupBoxPolynomial.Controls.Add(txtB);
            groupBoxPolynomial.Controls.Add(lblC);
            groupBoxPolynomial.Controls.Add(txtC);
            groupBoxPolynomial.Controls.Add(lblD);
            groupBoxPolynomial.Controls.Add(txtD);
            groupBoxPolynomial.Controls.Add(lblXrange);
            groupBoxPolynomial.Controls.Add(nudXmin);
            groupBoxPolynomial.Controls.Add(lblTo);
            groupBoxPolynomial.Controls.Add(nudXmax);
            groupBoxPolynomial.Controls.Add(btnAddPolynomial);

            topPanel.Controls.Add(groupBoxPolynomial);

            // ------------------------------------------------------------
            // Новая группа: численные производные сплайна
            GroupBox groupBoxDeriv = new GroupBox
            {
                Text = "Численные производные сплайна",
                Left = 10,
                Top = 170,
                Width = 700,
                Height = 85
            };

            Label lblH = new Label { Text = "Шаг h:", Left = 10, Top = 25, Width = 50 };
            txtH = new TextBox { Left = 65, Top = 22, Width = 60, Text = "0.01" };
            chkFirstDeriv = new CheckBox { Text = "f'(x)", Left = 140, Top = 22, Width = 60, Checked = false };
            chkSecondDeriv = new CheckBox { Text = "f''(x)", Left = 210, Top = 22, Width = 70, Checked = false };

            // При изменении любого параметра перерисовываем график
            txtH.TextChanged += (s, e) => DrawChart();
            chkFirstDeriv.CheckedChanged += (s, e) => DrawChart();
            chkSecondDeriv.CheckedChanged += (s, e) => DrawChart();

            // Текстовое поле для отображения погрешностей
            txtErrors = new TextBox
            {
                Left = 10,
                Top = 52,
                Width = 680,
                Height = 25,
                ReadOnly = true,
                BackColor = Color.LightYellow,
                Font = new Font("Consolas", 8.25f)
            };

            groupBoxDeriv.Controls.Add(lblH);
            groupBoxDeriv.Controls.Add(txtH);
            groupBoxDeriv.Controls.Add(chkFirstDeriv);
            groupBoxDeriv.Controls.Add(chkSecondDeriv);
            groupBoxDeriv.Controls.Add(txtErrors);
            topPanel.Controls.Add(groupBoxDeriv);
            // ------------------------------------------------------------

            // Таблица для точек
            dataGridView = new DataGridView
            {
                Dock = DockStyle.Left,
                Width = 300,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            dataGridView.Columns.Add("X", "x");
            dataGridView.Columns.Add("Y", "f(x)");
            dataGridView.Columns[0].ValueType = typeof(double);
            dataGridView.Columns[1].ValueType = typeof(double);

            // График
            chart = new Chart { Dock = DockStyle.Fill };
            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "x";
            chartArea.AxisY.Title = "f(x)";
            chartArea.AxisX.MajorGrid.Interval = 1;
            chartArea.AxisY.MajorGrid.Interval = 1;
            chart.ChartAreas.Add(chartArea);
            chart.Legends.Add(new Legend("Legend") { Docking = Docking.Top });

            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };
            splitContainer.Panel1.Controls.Add(dataGridView);
            splitContainer.Panel2.Controls.Add(chart);
            splitContainer.SplitterDistance = 300;

            this.Controls.Add(splitContainer);
            this.Controls.Add(topPanel);
        }

        private void BtnAddPolynomial_Click(object sender, EventArgs e)
        {
            try
            {
                double a = double.Parse(txtA.Text);
                double b = double.Parse(txtB.Text);
                double c = double.Parse(txtC.Text);
                double d = double.Parse(txtD.Text);
                double xmin = (double)nudXmin.Value;
                double xmax = (double)nudXmax.Value;

                if (xmin >= xmax)
                    throw new Exception("x_min должно быть меньше x_max");

                customPolynomials.Add(new CustomPolynomial(a, b, c, d, xmin, xmax));
                txtStatus.Text = $"Добавлен многочлен: {a}·x³ + {b}·x² + {c}·x + {d}, x∈[{xmin};{xmax}]";
                DrawChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSetCount_Click(object sender, EventArgs e)
        {
            int n = (int)nudPointsCount.Value;
            dataGridView.Rows.Clear();
            for (int i = 0; i < n; i++)
                dataGridView.Rows.Add(0, 0);

            if (n == 5)
            {
                double[] exampleX = { 2, 4, 5, 6, 7 };
                double[] exampleY = { 6, 6, 1, -1, 11 };
                for (int i = 0; i < n; i++)
                {
                    dataGridView.Rows[i].Cells[0].Value = exampleX[i];
                    dataGridView.Rows[i].Cells[1].Value = exampleY[i];
                }
            }
            txtStatus.Text = $"Задана сетка из {n} точек.";
            btnBuild.Enabled = true;
        }

        private void BtnAddRow_Click(object sender, EventArgs e)
        {
            dataGridView.Rows.Add(0, 0);
            txtStatus.Text = $"Всего точек: {dataGridView.Rows.Count}";
            nudPointsCount.Value = dataGridView.Rows.Count;
        }

        private void BtnBuild_Click(object sender, EventArgs e)
        {
            try
            {
                LoadPoints();
                BuildSpline();
                DrawChart();
                txtStatus.Text = "Сплайн построен успешно!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtStatus.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void LoadPoints()
        {
            xNodes.Clear();
            yNodes.Clear();
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                var cellX = dataGridView.Rows[i].Cells[0].Value;
                var cellY = dataGridView.Rows[i].Cells[1].Value;
                if (cellX == null || cellY == null)
                    throw new Exception("Есть пустые ячейки!");
                xNodes.Add(Convert.ToDouble(cellX));
                yNodes.Add(Convert.ToDouble(cellY));
            }

            var points = xNodes.Zip(yNodes, (x, y) => new { x, y }).OrderBy(p => p.x).ToList();
            xNodes = points.Select(p => p.x).ToList();
            yNodes = points.Select(p => p.y).ToList();

            for (int i = 1; i < xNodes.Count; i++)
                if (xNodes[i] <= xNodes[i - 1])
                    throw new Exception("x должны строго возрастать!");
        }

        private void BuildSpline()
        {
            int n = xNodes.Count;
            double[] h = new double[n];
            for (int i = 1; i < n; i++)
                h[i] = xNodes[i] - xNodes[i - 1];

            double[] c = new double[n];
            c[0] = 0;
            c[n - 1] = 0;

            if (n == 2)
            {
                double a = yNodes[0];
                double b = (yNodes[1] - yNodes[0]) / h[1];
                splineSegments.Clear();
                splineSegments.Add(new CustomPolynomial(a, b, 0, 0, xNodes[0], xNodes[1]));
                return;
            }

            int m = n - 2;
            float[,] matrix = new float[m, m];
            float[] vector = new float[m];

            for (int i = 0; i < m; i++)
            {
                int idx = i + 1;
                double hi = h[idx];
                double hi1 = h[idx + 1];
                double left = (yNodes[idx] - yNodes[idx - 1]) / hi;
                double right = (yNodes[idx + 1] - yNodes[idx]) / hi1;

                if (i > 0)
                    matrix[i, i - 1] = (float)hi;

                matrix[i, i] = (float)(2 * (hi + hi1));

                if (i < m - 1)
                    matrix[i, i + 1] = (float)hi1;

                vector[i] = (float)(3 * (right - left));
            }

            float[] cInternal = gaussWithMaxElementByMatrix(matrix, vector, m);
            if (cInternal == null)
                throw new Exception("Не удалось решить систему уравнений для сплайна");

            for (int i = 1; i < n - 1; i++)
                c[i] = cInternal[i - 1];

            splineSegments.Clear();
            for (int i = 1; i < n; i++)
            {
                double hi = h[i];
                double a = yNodes[i - 1];
                double b = (yNodes[i] - yNodes[i - 1]) / hi - hi * (2 * c[i - 1] + c[i]) / 3;
                double d = (c[i] - c[i - 1]) / (3 * hi);

                splineSegments.Add(new CustomPolynomial(a, b, c[i - 1], d, xNodes[i - 1], xNodes[i]));
            }
        }

        private float[] gaussWithMaxElementByMatrix(float[,] A, float[] Y, int N)
        {
            float[] X = new float[N];
            float[,] A_temp = CopyMatrix(A, N);
            float[] Y_temp = CopyVector(Y, N);
            int[] columnOrder = new int[N];
            for (int i = 0; i < N; i++) columnOrder[i] = i;

            const float eps = 0.0000001f;

            for (int k = 0; k < N; k++)
            {
                int maxRow = k, maxCol = k;
                float maxVal = Math.Abs(A_temp[k, k]);
                for (int i = k; i < N; i++)
                {
                    for (int j = k; j < N; j++)
                    {
                        if (Math.Abs(A_temp[i, j]) > maxVal)
                        {
                            maxVal = Math.Abs(A_temp[i, j]);
                            maxRow = i;
                            maxCol = j;
                        }
                    }
                }

                if (maxVal < eps)
                {
                    MessageBox.Show("Решение получить невозможно из-за вырожденности матрицы", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                if (maxRow != k)
                {
                    for (int j = 0; j < N; j++)
                    {
                        float temp = A_temp[k, j];
                        A_temp[k, j] = A_temp[maxRow, j];
                        A_temp[maxRow, j] = temp;
                    }
                    float tempY = Y_temp[k];
                    Y_temp[k] = Y_temp[maxRow];
                    Y_temp[maxRow] = tempY;
                }

                if (maxCol != k)
                {
                    for (int i = 0; i < N; i++)
                    {
                        float temp = A_temp[i, k];
                        A_temp[i, k] = A_temp[i, maxCol];
                        A_temp[i, maxCol] = temp;
                    }
                    int tempCol = columnOrder[k];
                    columnOrder[k] = columnOrder[maxCol];
                    columnOrder[maxCol] = tempCol;
                }

                for (int i = k + 1; i < N; i++)
                {
                    float factor = A_temp[i, k] / A_temp[k, k];
                    for (int j = k; j < N; j++)
                        A_temp[i, j] -= factor * A_temp[k, j];
                    Y_temp[i] -= factor * Y_temp[k];
                }
            }

            float[] X_temp = new float[N];
            for (int i = N - 1; i >= 0; i--)
            {
                X_temp[i] = Y_temp[i];
                for (int j = i + 1; j < N; j++)
                    X_temp[i] -= A_temp[i, j] * X_temp[j];

                if (Math.Abs(A_temp[i, i]) < 0.0000001f)
                {
                    MessageBox.Show("Деление на ноль при обратном ходе", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                X_temp[i] /= A_temp[i, i];
            }

            for (int i = 0; i < N; i++)
                X[columnOrder[i]] = X_temp[i];

            return X;
        }

        private float[,] CopyMatrix(float[,] source, int N)
        {
            float[,] result = new float[N, N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    result[i, j] = source[i, j];
            return result;
        }

        private float[] CopyVector(float[] source, int N)
        {
            float[] result = new float[N];
            for (int i = 0; i < N; i++)
                result[i] = source[i];
            return result;
        }

        /// <summary>
        /// Вычисляет значение сплайна в точке x.
        /// </summary>
        private double EvalSpline(double x)
        {
            if (xNodes.Count < 2 || splineSegments.Count == 0)
                return double.NaN;

            for (int i = 1; i < xNodes.Count; i++)
            {
                if (x >= xNodes[i - 1] && x <= xNodes[i])
                {
                    double dx = x - xNodes[i - 1];
                    var seg = splineSegments[i - 1];
                    return seg.A + seg.B * dx + seg.C * dx * dx + seg.D * dx * dx * dx;
                }
            }
            return double.NaN;
        }

        /// <summary>
        /// Возвращает точное значение первой производной сплайна в точке x.
        /// </summary>
        private double ExactFirstDerivative(double x)
        {
            if (xNodes.Count < 2 || splineSegments.Count == 0)
                return double.NaN;

            for (int i = 1; i < xNodes.Count; i++)
            {
                if (x >= xNodes[i - 1] && x <= xNodes[i])
                {
                    double dx = x - xNodes[i - 1];
                    var seg = splineSegments[i - 1];
                    // S'(x) = B + 2*C*dx + 3*D*dx^2
                    return seg.B + 2.0 * seg.C * dx + 3.0 * seg.D * dx * dx;
                }
            }
            return double.NaN;
        }

        /// <summary>
        /// Возвращает точное значение второй производной сплайна в точке x.
        /// </summary>
        private double ExactSecondDerivative(double x)
        {
            if (xNodes.Count < 2 || splineSegments.Count == 0)
                return double.NaN;

            for (int i = 1; i < xNodes.Count; i++)
            {
                if (x >= xNodes[i - 1] && x <= xNodes[i])
                {
                    double dx = x - xNodes[i - 1];
                    var seg = splineSegments[i - 1];
                    // S''(x) = 2*C + 6*D*dx
                    return 2.0 * seg.C + 6.0 * seg.D * dx;
                }
            }
            return double.NaN;
        }

        private void DrawChart()
        {
            chart.Series.Clear();
            txtErrors.Text = "";

            // Рисуем сплайн
            if (chkShowSpline.Checked && splineSegments.Count > 0)
            {
                Series seriesSpline = new Series("Кубический сплайн");
                seriesSpline.ChartType = SeriesChartType.Line;
                seriesSpline.BorderWidth = 4;
                seriesSpline.Color = Color.Red;
                seriesSpline.Legend = "Legend";

                foreach (var segment in splineSegments)
                {
                    int steps = 100;
                    for (int j = 0; j <= steps; j++)
                    {
                        double x = segment.Xmin + (segment.Xmax - segment.Xmin) * j / steps;
                        double dx = x - segment.Xmin;
                        double y = segment.A + segment.B * dx + segment.C * dx * dx + segment.D * dx * dx * dx;
                        seriesSpline.Points.AddXY(x, y);
                    }
                }
                chart.Series.Add(seriesSpline);
            }

            // Рисуем произвольные многочлены
            foreach (var poly in customPolynomials)
            {
                string seriesName = $"Многочлен: {poly.A}·x³ + {poly.B}·x² + {poly.C}·x + {poly.D}";
                Series seriesPoly = new Series(seriesName);
                seriesPoly.ChartType = SeriesChartType.Line;
                seriesPoly.BorderWidth = 2;
                seriesPoly.Color = Color.FromArgb(255, 0, 255, 0);
                seriesPoly.Legend = "Legend";

                int steps = 200;
                for (int j = 0; j <= steps; j++)
                {
                    double x = poly.Xmin + (poly.Xmax - poly.Xmin) * j / steps;
                    double y = poly.A * x * x * x + poly.B * x * x + poly.C * x + poly.D;
                    seriesPoly.Points.AddXY(x, y);
                }
                chart.Series.Add(seriesPoly);
            }

            // Рисуем исходные точки
            if (xNodes.Count > 0)
            {
                Series seriesPoints = new Series("Исходные точки");
                seriesPoints.ChartType = SeriesChartType.Point;
                seriesPoints.MarkerStyle = MarkerStyle.Circle;
                seriesPoints.MarkerSize = 10;
                seriesPoints.MarkerColor = Color.Blue;
                seriesPoints.Color = Color.Blue;

                for (int i = 0; i < xNodes.Count; i++)
                    seriesPoints.Points.AddXY(xNodes[i], yNodes[i]);

                chart.Series.Add(seriesPoints);
            }

            // ============================================================
            //   Численные производные и расчёт погрешностей
            // ============================================================
            if (splineSegments.Count > 0 && xNodes.Count >= 2)
            {
                double h;
                if (!double.TryParse(txtH.Text,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out h) || h <= 0)
                {
                    h = 0.01;
                    txtH.Text = "0.01";
                }

                double xMin = xNodes.Min();
                double xMax = xNodes.Max();

                // Расширяем границы на h внутрь, чтобы можно было вычислить разности
                double drawXmin = xMin + h;
                double drawXmax = xMax - h;
                if (drawXmax <= drawXmin)
                {
                    drawXmin = xMin;
                    drawXmax = xMax;
                }

                // Массивы для погрешностей
                List<double> errorsFirst = new List<double>();
                List<double> errorsSecond = new List<double>();

                // Первая производная
                if (chkFirstDeriv.Checked)
                {
                    Series serDeriv1 = new Series("f'(x) (числ.)");
                    serDeriv1.ChartType = SeriesChartType.Line;
                    serDeriv1.BorderWidth = 2;
                    serDeriv1.Color = Color.DarkGreen;
                    serDeriv1.Legend = "Legend";

                    int steps = 300;
                    for (int i = 0; i <= steps; i++)
                    {
                        double x = drawXmin + (drawXmax - drawXmin) * i / steps;
                        double fp = EvalSpline(x + h);
                        double fm = EvalSpline(x - h);
                        if (!double.IsNaN(fp) && !double.IsNaN(fm))
                        {
                            double deriv = (fp - fm) / (2.0 * h);
                            serDeriv1.Points.AddXY(x, deriv);

                            // Погрешность
                            double exact = ExactFirstDerivative(x);
                            if (!double.IsNaN(exact))
                                errorsFirst.Add(Math.Abs(deriv - exact));
                        }
                    }
                    chart.Series.Add(serDeriv1);
                }

                // Вторая производная
                if (chkSecondDeriv.Checked)
                {
                    Series serDeriv2 = new Series("f''(x) (числ.)");
                    serDeriv2.ChartType = SeriesChartType.Line;
                    serDeriv2.BorderWidth = 2;
                    serDeriv2.Color = Color.Purple;
                    serDeriv2.Legend = "Legend";

                    int steps = 300;
                    for (int i = 0; i <= steps; i++)
                    {
                        double x = drawXmin + (drawXmax - drawXmin) * i / steps;
                        double f0 = EvalSpline(x);
                        double fp = EvalSpline(x + h);
                        double fm = EvalSpline(x - h);
                        if (!double.IsNaN(f0) && !double.IsNaN(fp) && !double.IsNaN(fm))
                        {
                            double deriv2 = (fp - 2.0 * f0 + fm) / (h * h);
                            serDeriv2.Points.AddXY(x, deriv2);

                            double exact = ExactSecondDerivative(x);
                            if (!double.IsNaN(exact))
                                errorsSecond.Add(Math.Abs(deriv2 - exact));
                        }
                    }
                    chart.Series.Add(serDeriv2);
                }

                // Формируем строку с погрешностями
                string errorMsg = "";
                if (errorsFirst.Count > 0)
                    errorMsg += $"f'(x): макс. погр. = {errorsFirst.Max():E4}, средняя погр. = {errorsFirst.Average():E4}  ";
                if (errorsSecond.Count > 0)
                    errorMsg += $"f''(x): макс. погр. = {errorsSecond.Max():E4}, средняя погр. = {errorsSecond.Average():E4}";
                txtErrors.Text = errorMsg;
            }

            // Автоматическое масштабирование осей
            ChartArea chartArea = chart.ChartAreas[0];
            chartArea.AxisX.Minimum = double.NaN;
            chartArea.AxisX.Maximum = double.NaN;
            chartArea.AxisY.Minimum = double.NaN;
            chartArea.AxisY.Maximum = double.NaN;
            chartArea.AxisX.Interval = double.NaN;
            chartArea.AxisY.Interval = double.NaN;
            chartArea.AxisX.LabelStyle.Format = "0.##";
            chartArea.AxisY.LabelStyle.Format = "0.##";
            chartArea.AxisX.MajorGrid.Interval = double.NaN;
            chartArea.AxisY.MajorGrid.Interval = double.NaN;

            chart.Invalidate();
        }
    }

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
    }
}