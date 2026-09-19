using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FuzzyCoffeeMaker
{
    public partial class Form1 : Form
    {
        private readonly double brewShort = 60.0;
        private readonly double brewMedium = 120.0;
        private readonly double brewLong = 180.0;

        public Form1()
        {
            InitializeComponent();
            InitializeCharts();

            // Wire up event handlers to trigger whenever sliders move
            trackBarWater.Scroll += new EventHandler(Input_Changed);
            trackBarStrength.Scroll += new EventHandler(Input_Changed);

            // Wire up the new TextBox event
            txtWaterInput.TextChanged += new EventHandler(txtWaterInput_TextChanged);

            // Initialize the UI
            txtWaterInput.Text = trackBarWater.Value.ToString();
            RunSystem();
        }

        private void InitializeCharts()
        {
            // --- Configure Water Chart ---
            chartWater.Series.Clear();
            chartWater.ChartAreas[0].AxisX.Minimum = 100;
            chartWater.ChartAreas[0].AxisX.Maximum = 500;
            chartWater.ChartAreas[0].AxisY.Maximum = 1.1;
            chartWater.ChartAreas[0].AxisY.Title = "Membership Degree";

            // X-Coordinates: (-100, 100, 300)
            Series waterSmall = new Series("Small") { ChartType = SeriesChartType.Line, BorderWidth = 2 };
            waterSmall.Points.AddXY(100, 1); // Peak at min slider value
            waterSmall.Points.AddXY(300, 0);

            // X-Coordinates: (100, 300, 500)
            Series waterMedium = new Series("Medium") { ChartType = SeriesChartType.Line, BorderWidth = 2, Color = Color.Green };
            waterMedium.Points.AddXY(100, 0);
            waterMedium.Points.AddXY(300, 1); // Peak in exact middle
            waterMedium.Points.AddXY(500, 0);

            // X-Coordinates: (300, 500, 700)
            Series waterLarge = new Series("Large") { ChartType = SeriesChartType.Line, BorderWidth = 2 };
            waterLarge.Points.AddXY(300, 0);
            waterLarge.Points.AddXY(500, 1); // Peak at max slider value

            Series currentWaterLine = new Series("Current") { ChartType = SeriesChartType.Line, BorderWidth = 3, Color = Color.Red };

            chartWater.Series.Add(waterSmall);
            chartWater.Series.Add(waterMedium);
            chartWater.Series.Add(waterLarge);
            chartWater.Series.Add(currentWaterLine);

            // --- Configure Strength Chart ---
            chartStrength.Series.Clear();
            chartStrength.ChartAreas[0].AxisX.Minimum = 1;
            chartStrength.ChartAreas[0].AxisX.Maximum = 10;
            chartStrength.ChartAreas[0].AxisY.Maximum = 1.1;
            chartStrength.ChartAreas[0].AxisY.Title = "Membership Degree";

            // X-Coordinates: (-3.5, 1, 5.5)
            Series strengthMild = new Series("Mild") { ChartType = SeriesChartType.Line, BorderWidth = 2 };
            strengthMild.Points.AddXY(1, 1); // Peak at min slider value
            strengthMild.Points.AddXY(5.5, 0);

            // X-Coordinates: (1, 5.5, 10)
            Series strengthMedium = new Series("Medium") { ChartType = SeriesChartType.Line, BorderWidth = 2, Color = Color.Green };
            strengthMedium.Points.AddXY(1, 0);
            strengthMedium.Points.AddXY(5.5, 1); // Peak in exact middle
            strengthMedium.Points.AddXY(10, 0);

            // X-Coordinates: (5.5, 10, 14.5)
            Series strengthStrong = new Series("Strong") { ChartType = SeriesChartType.Line, BorderWidth = 2 };
            strengthStrong.Points.AddXY(5.5, 0);
            strengthStrong.Points.AddXY(10, 1); // Peak at max slider value

            Series currentStrengthLine = new Series("Current") { ChartType = SeriesChartType.Line, BorderWidth = 3, Color = Color.Red };

            chartStrength.Series.Add(strengthMild);
            chartStrength.Series.Add(strengthMedium);
            chartStrength.Series.Add(strengthStrong);
            chartStrength.Series.Add(currentStrengthLine);

            // --- Configure Output Bar Chart ---
            chartOutput.Series.Clear();
            chartOutput.ChartAreas[0].AxisY.Maximum = 1.0;
            chartOutput.ChartAreas[0].AxisY.Title = "Firing Strength";
            chartOutput.Titles.Add("Output Singletons");

            Series outputBars = new Series("Outputs") { ChartType = SeriesChartType.Column, Color = Color.Black };
            chartOutput.Series.Add(outputBars);
        }



        // Triggered when EITHER slider moves
        private void Input_Changed(object sender, EventArgs e)
        {
            // Only update the textbox if the user is NOT actively typing in it.
            // This prevents the cursor from jumping while typing.
            if (!txtWaterInput.Focused)
            {
                txtWaterInput.Text = trackBarWater.Value.ToString();
            }
            RunSystem();
        }

        // Triggered when the user types in the TextBox
        private void txtWaterInput_TextChanged(object sender, EventArgs e)
        {
            // 1. Try to parse the typed text into an integer
            if (int.TryParse(txtWaterInput.Text, out int parsedValue))
            {
                // 2. Clamp the number so it doesn't crash the slider (Min 100, Max 500)
                int safeValue = Math.Max(trackBarWater.Minimum, Math.Min(trackBarWater.Maximum, parsedValue));

                // 3. Update the slider and run the simulation only if the value actually changed
                if (trackBarWater.Value != safeValue)
                {
                    trackBarWater.Value = safeValue;
                    RunSystem();
                }
            }
        }

        private void RunSystem()
        {
            // --- 1. CRISP INPUTS ---
            double waterVolume = trackBarWater.Value;
            double strengthPreference = trackBarStrength.Value;

            lblStrengthVal.Text = $"{strengthPreference} / 10";

            // Update Input Chart Markers
            chartWater.Series["Current"].Points.Clear();
            chartWater.Series["Current"].Points.AddXY(waterVolume, 0);
            chartWater.Series["Current"].Points.AddXY(waterVolume, 1);

            chartStrength.Series["Current"].Points.Clear();
            chartStrength.Series["Current"].Points.AddXY(strengthPreference, 0);
            chartStrength.Series["Current"].Points.AddXY(strengthPreference, 1);

            // --- 2. FUZZIFICATION (3 States per Input) ---
            double waterSmall = TriangularMembership(waterVolume, -100, 100, 300);
            double waterMedium = TriangularMembership(waterVolume, 100, 300, 500);
            double waterLarge = TriangularMembership(waterVolume, 300, 500, 700);

            double strengthMild = TriangularMembership(strengthPreference, -3.5, 1, 5.5);
            double strengthMedium = TriangularMembership(strengthPreference, 1, 5.5, 10);
            double strengthStrong = TriangularMembership(strengthPreference, 5.5, 10, 14.5);

            lblWaterSmall.Text = $"Water Small: {waterSmall:F2}";
            if (lblWaterMedium != null) lblWaterMedium.Text = $"Water Med: {waterMedium:F2}";
            lblWaterLarge.Text = $"Water Large: {waterLarge:F2}";

            lblStrengthMild.Text = $"Strength Mild: {strengthMild:F2}";
            if (lblStrengthMedium != null) lblStrengthMedium.Text = $"Strength Med: {strengthMedium:F2}";
            lblStrengthStrong.Text = $"Strength Strong: {strengthStrong:F2}";

            // --- 3. RULE EVALUATION (Sugeno min-inference with 9 rules) ---
            double r1 = Math.Min(waterSmall, strengthMild);    // Short
            double r2 = Math.Min(waterSmall, strengthMedium);  // Short
            double r3 = Math.Min(waterSmall, strengthStrong);  // Medium

            double r4 = Math.Min(waterMedium, strengthMild);   // Short
            double r5 = Math.Min(waterMedium, strengthMedium); // Medium
            double r6 = Math.Min(waterMedium, strengthStrong); // Long

            double r7 = Math.Min(waterLarge, strengthMild);    // Medium
            double r8 = Math.Min(waterLarge, strengthMedium);  // Long
            double r9 = Math.Min(waterLarge, strengthStrong);  // Long

            // Update UI Rules
            lblRule1.Text = $"R1 (Short): {r1:F2}";
            lblRule2.Text = $"R2 (Short): {r2:F2}";
            lblRule3.Text = $"R3 (Medium): {r3:F2}";
            lblRule4.Text = $"R4 (Short): {r4:F2}";
            if (lblRule5 != null) lblRule5.Text = $"R5 (Medium): {r5:F2}";
            if (lblRule6 != null) lblRule6.Text = $"R6 (Long): {r6:F2}";
            if (lblRule7 != null) lblRule7.Text = $"R7 (Medium): {r7:F2}";
            if (lblRule8 != null) lblRule8.Text = $"R8 (Long): {r8:F2}";
            if (lblRule9 != null) lblRule9.Text = $"R9 (Long): {r9:F2}";

            // --- 4. DEFUZZIFICATION (Weighted Average) ---
            double numerator = (r1 * brewShort) + (r2 * brewShort) + (r4 * brewShort) +   // Short rules
                               (r3 * brewMedium) + (r5 * brewMedium) + (r7 * brewMedium) + // Medium rules
                               (r6 * brewLong) + (r8 * brewLong) + (r9 * brewLong);       // Long rules

            double denominator = r1 + r2 + r3 + r4 + r5 + r6 + r7 + r8 + r9;
            double crispOutput = denominator > 0 ? (numerator / denominator) : 0.0;

            lblCrispOutput.Text = $"{crispOutput:F1} sec";

            // --- 5. UPDATE OUTPUT BAR CHART ---
            double totalShort = Math.Max(r1, Math.Max(r2, r4));
            double totalMedium = Math.Max(r3, Math.Max(r5, r7));
            double totalLong = Math.Max(r6, Math.Max(r8, r9));

            chartOutput.Series["Outputs"].Points.Clear();
            chartOutput.Series["Outputs"].Points.AddXY("Short (60s)", totalShort);
            chartOutput.Series["Outputs"].Points.AddXY("Medium (120s)", totalMedium);
            chartOutput.Series["Outputs"].Points.AddXY("Long (180s)", totalLong);
        }

        // Triangular Membership Function Logic
        private double TriangularMembership(double x, double a, double b, double c)
        {
            if (x <= a || x >= c) return 0.0;
            if (x == b) return 1.0;
            if (x > a && x < b) return (x - a) / (b - a);
            return (c - x) / (c - b);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void lblCrispOutput_Click(object sender, EventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void lblWaterVal_Click(object sender, EventArgs e)
        {

        }

        private void chartStrength_Click(object sender, EventArgs e)
        {

        }
    }
}
