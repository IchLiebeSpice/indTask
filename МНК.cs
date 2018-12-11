using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Drawing2D;

namespace МНК
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private const float Xmin = -10.0f;
        private const float Xmax = 10.0f;
        private const float Ymin = -10.0f;
        private const float Ymax = 10.0f;
        private Matrix DrawingTransform;
        private Matrix InverseTransform;

        private bool HasSolution = false;
        private List<double> BestCoeffs;
        private List<PointF> Points = new List<PointF>();

        private void Form1_Load(object sender, EventArgs e)
        {
            RectangleF world_rect = new RectangleF(Xmin, Ymin, Xmax - Xmin, Ymax - Ymin);
            PointF[] pts =
            {
                new PointF(0, picGraph.ClientSize.Height),
                new PointF(picGraph.ClientSize.Width, picGraph.ClientSize.Height),
                new PointF(0, 0),
            };
            DrawingTransform = new Matrix(world_rect, pts);
            InverseTransform = DrawingTransform.Clone();
            InverseTransform.Invert();

            rchEquation.Select(5, 1);
            rchEquation.SelectionCharOffset = -7;
            rchEquation.Select(7, 1);
            rchEquation.SelectionCharOffset = 7;
            rchEquation.Select(12, 1);
            rchEquation.SelectionCharOffset = -7;
            rchEquation.Select(14, 1);
            rchEquation.SelectionCharOffset = 7;
            rchEquation.Select(19, 1);
            rchEquation.SelectionCharOffset = -7;
            rchEquation.Select(21, 1);
            rchEquation.SelectionCharOffset = 7;
            rchEquation.Select(26, 1);
            rchEquation.SelectionCharOffset = -7;
            rchEquation.Select(28, 1);
            rchEquation.SelectionCharOffset = 7;
        }

        private void picGraph_MouseClick(object sender, MouseEventArgs e)
        {
            PointF[] pts = { new PointF(e.X, e.Y) };
            InverseTransform.TransformPoints(pts);

            Points.Add(pts[0]);
            picGraph.Refresh();
        }

        private void picGraph_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Transform = DrawingTransform;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            DrawAxes(e.Graphics);

            if (HasSolution)
            {
                using (Pen thin_pen = new Pen(Color.Blue, 0))
                {
                    const double x_step = 0.1;
                    double y0 = CurveFunctions.F(BestCoeffs, Xmin);
                    for (double x = Xmin + x_step; x <= Xmax; x += x_step)
                    {
                        double y1 = CurveFunctions.F(BestCoeffs, x);
                        e.Graphics.DrawLine(thin_pen,
                            (float)(x - x_step), (float)y0, (float)x, (float)y1);
                        y0 = y1;
                    }
                }
            }

            const float dx = (Xmax - Xmin) / 100;
            const float dy = (Ymax - Ymin) / 100;
            using (Pen thin_pen = new Pen(Color.Black, 0))
            {
                foreach (PointF pt in Points)
                {
                    e.Graphics.FillRectangle(Brushes.White,
                        pt.X - dx, pt.Y - dy, 2 * dx, 2 * dy);
                    e.Graphics.DrawRectangle(thin_pen,
                        pt.X - dx, pt.Y - dy, 2 * dx, 2 * dy);
                }
            }
        }

        private void DrawAxes(Graphics gr)
        {
            using (Pen thin_pen = new Pen(Color.Black, 0))
            {
                const float xthick = 0.2f;
                const float ythick = 0.2f;
                gr.DrawLine(thin_pen, Xmin, 0, Xmax, 0);
                for (float x = Xmin; x <= Xmax; x += 1.0f)
                {
                    gr.DrawLine(thin_pen, x, -ythick, x, ythick);
                }
                gr.DrawLine(thin_pen, 0, Ymin, 0, Ymax);
                for (float y = Ymin; y <= Ymax; y += 1.0f)
                {
                    gr.DrawLine(thin_pen, -xthick, y, xthick, y);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Points = new List<PointF>();
            HasSolution = false;
            picGraph.Refresh();

            txtCoeffs.Clear();
            txtError.Clear();
        }

        private void btnFit_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            txtCoeffs.Clear();
            txtError.Clear();
            Application.DoEvents();
            DateTime start_time = DateTime.Now;

            int degree = (int)nudDegree.Value;
            BestCoeffs = CurveFunctions.FindPolynomialLeastSquaresFit(Points, degree);
            HasSolution = true;

            DateTime stop_time = DateTime.Now;
            TimeSpan elapsed = stop_time - start_time;
            Console.WriteLine("Time: " +
                elapsed.TotalSeconds.ToString("0.00") + " seconds");

            string txt = "";
            foreach (double coeff in BestCoeffs)
            {
                txt += " " + coeff.ToString();
            }
            txtCoeffs.Text = txt.Substring(1);

            ShowError();

            HasSolution = true;
            picGraph.Refresh();

            this.Cursor = Cursors.Default;
        }

        private void btnGraph_Click(object sender, EventArgs e)
        {
            string[] coeffs = txtCoeffs.Text.Split();
            BestCoeffs = new List<double>();
            foreach (string coeff in coeffs)
            {
                BestCoeffs.Add(double.Parse(coeff));
            }
            ShowError();
            picGraph.Refresh();
        }

        private void ShowError()
        {
            double error = Math.Sqrt(
                CurveFunctions.ErrorSquared(Points, BestCoeffs));
            txtError.Text = error.ToString();
        }
    }
}
