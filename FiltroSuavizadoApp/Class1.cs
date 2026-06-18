using System;
using System.Drawing;
using System.Windows.Forms;

namespace FiltroSuavizado
{
    public class Form1 : Form
    {
        private Button btnCargar, btnFiltrar, btnGuardar;
        private PictureBox picOriginal, picFiltrada;
        private Bitmap imgOriginal, imgFiltrada;

        public Form1()
        {
            this.Text = "Filtro Suavizado 3x3";
            this.Size = new Size(750, 480);
            this.StartPosition = FormStartPosition.CenterScreen;

            btnCargar = new Button() { Text = "Cargar", Location = new Point(12, 12), Size = new Size(100, 30) };
            btnFiltrar = new Button() { Text = "Suavizar", Location = new Point(120, 12), Size = new Size(100, 30), Enabled = false };
            btnGuardar = new Button() { Text = "Guardar", Location = new Point(228, 12), Size = new Size(100, 30), Enabled = false };

            picOriginal = new PictureBox() { BorderStyle = BorderStyle.FixedSingle, Location = new Point(12, 50), Size = new Size(340, 340), SizeMode = PictureBoxSizeMode.Zoom };
            picFiltrada = new PictureBox() { BorderStyle = BorderStyle.FixedSingle, Location = new Point(370, 50), Size = new Size(340, 340), SizeMode = PictureBoxSizeMode.Zoom };

            btnCargar.Click += (s, e) => CargarImagen();
            btnFiltrar.Click += (s, e) => FiltrarImagen();
            btnGuardar.Click += (s, e) => GuardarImagen();

            this.Controls.Add(btnCargar);
            this.Controls.Add(btnFiltrar);
            this.Controls.Add(btnGuardar);
            this.Controls.Add(picOriginal);
            this.Controls.Add(picFiltrada);
        }

        private void CargarImagen()
        {
            OpenFileDialog dlg = new OpenFileDialog() { Filter = "Imágenes|*.jpg;*.png;*.bmp" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                imgOriginal = new Bitmap(dlg.FileName);
                picOriginal.Image = imgOriginal;
                btnFiltrar.Enabled = true;
                btnGuardar.Enabled = false;
            }
        }

        private void FiltrarImagen()
        {
            if (imgOriginal == null) return;
            Cursor = Cursors.WaitCursor;
            imgFiltrada = Suavizar(imgOriginal);
            picFiltrada.Image = imgFiltrada;
            btnGuardar.Enabled = true;
            Cursor = Cursors.Default;
        }

        private void GuardarImagen()
        {
            SaveFileDialog dlg = new SaveFileDialog() { Filter = "PNG|*.png|JPEG|*.jpg" };
            if (imgFiltrada != null && dlg.ShowDialog() == DialogResult.OK)
                imgFiltrada.Save(dlg.FileName);
        }

        private Bitmap Suavizar(Bitmap original)
        {
            Bitmap resultado = new Bitmap(original.Width, original.Height);
            for (int y = 1; y < original.Height - 1; y++)
                for (int x = 1; x < original.Width - 1; x++)
                {
                    int r = 0, g = 0, b = 0;
                    for (int fy = -1; fy <= 1; fy++)
                        for (int fx = -1; fx <= 1; fx++)
                        {
                            Color p = original.GetPixel(x + fx, y + fy);
                            r += p.R; g += p.G; b += p.B;
                        }
                    resultado.SetPixel(x, y, Color.FromArgb(r / 9, g / 9, b / 9));
                }
            return resultado;
        }
    }
}