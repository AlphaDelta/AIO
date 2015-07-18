using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AIO
{
    public partial class AIOGenomeViewer : Form
    {
        public Genome Genome = null;
        public AIOGenomeViewer()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        const int neuronsize = 20, neuronmid = 10;
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

            e.Graphics.FillRectangle(Brushes.MidnightBlue, e.ClipRectangle);

            if (Genome == null) return;

            int mid = (int)Math.Round(e.ClipRectangle.Width / 2f);

            List<ViewerNeuron> neurons = new List<ViewerNeuron>();

            int inputx = e.ClipRectangle.Left + (mid - (int)Math.Round((Genome.Input.Length * neuronsize + (Genome.Input.Length - 1f) * neuronsize) / 2f));
            int inputy = e.ClipRectangle.Bottom - neuronsize - 10;
            int curx = inputx;
            for (int i = 0; i < Genome.Input.Length; i++)
            {
                e.Graphics.FillRectangle(Brushes.White, curx, inputy, neuronsize, neuronsize);
                neurons.Add(new ViewerNeuron() { ID = (int)Genome.Input[i].ID, X = curx, Y = inputy });
                curx += neuronsize * 2;
            }

            int outputx = e.ClipRectangle.Left + (mid - (int)Math.Round((Genome.Output.Length * neuronsize + (Genome.Output.Length - 1f) * neuronsize) / 2f));
            int outputy = 10;
            curx = outputx;
            for (int i = 0; i < Genome.Output.Length; i++)
            {
                e.Graphics.FillRectangle(Brushes.White, curx, outputy, neuronsize, neuronsize);
                neurons.Add(new ViewerNeuron() { ID = (int)Genome.Output[i].ID, X = curx, Y = outputy });
                curx += neuronsize * 2;
            }

            foreach (NeuralConnection c in Genome.Connections)
            {
                ViewerNeuron to = null, from = null;
                foreach (ViewerNeuron vn in neurons)
                {
                    if (to != null && from != null) break;
                    if (vn.ID == c.In.ID) from = vn;
                    else if (vn.ID == c.Out.ID) to = vn;
                }

                if (to == null || from == null) continue;

                e.Graphics.DrawLine((c.Weight == 1f ? Pens.Gray : (c.Weight < 1f ? Pens.Red : Pens.Green)), from.X + neuronmid, from.Y, to.X + neuronmid, to.Y + neuronsize);
            }
        }

        class ViewerNeuron
        {
            public int ID, X, Y;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }
    }
}
