using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace DetectorBordes
{
    public class FormDetectorBordes : Form
    {
        private PictureBox pbOriginal;
        private PictureBox pbProcesada;
        private Button btnCargar;
        private Button btnDetectar;
        private Button btnGuardar;
        private Button btnResetear;
        private Bitmap imagenOriginal;
        private Bitmap imagenProcesada;

        public FormDetectorBordes()
        {
            // Llamamos al método que crea la interfaz
            CrearInterfaz();
        }

        // Este método reemplaza a InitializeComponent
        private void CrearInterfaz()
        {
            this.Text = "Detector de Bordes - Sobel";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Panel principal con TableLayout
            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            // PictureBox para imagen original
            pbOriginal = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // PictureBox para imagen procesada
            pbProcesada = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // Panel de botones
            FlowLayoutPanel panelBotones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            btnCargar = new Button { Text = "Cargar Imagen", Width = 120, Height = 35 };
            btnDetectar = new Button { Text = "Detectar Bordes", Width = 120, Height = 35 };
            btnGuardar = new Button { Text = "Guardar Resultado", Width = 120, Height = 35 };
            btnResetear = new Button { Text = "Resetear", Width = 120, Height = 35 };

            // Eventos
            btnCargar.Click += (s, e) => CargarImagen();
            btnDetectar.Click += (s, e) => DetectarBordes();
            btnGuardar.Click += (s, e) => GuardarImagen();
            btnResetear.Click += (s, e) => Resetear();

            panelBotones.Controls.AddRange(new Control[] {
                btnCargar, btnDetectar, btnGuardar, btnResetear
            });

            mainPanel.Controls.Add(pbOriginal, 0, 0);
            mainPanel.Controls.Add(pbProcesada, 1, 0);
            mainPanel.Controls.Add(panelBotones, 0, 1);
            mainPanel.SetColumnSpan(panelBotones, 2);

            this.Controls.Add(mainPanel);
        }

        private void CargarImagen()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.Title = "Seleccionar imagen";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        imagenOriginal = new Bitmap(ofd.FileName);
                        pbOriginal.Image = new Bitmap(imagenOriginal);
                        pbProcesada.Image = null;
                        imagenProcesada = null;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DetectarBordes()
        {
            if (pbOriginal.Image == null)
            {
                MessageBox.Show("Primero debe cargar una imagen.", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                imagenProcesada = AplicarSobel(imagenOriginal);
                pbProcesada.Image = imagenProcesada;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al detectar bordes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap AplicarSobel(Bitmap imagenOriginal)
        {
            Bitmap imagenGris = ConvertirAGrises(imagenOriginal);

            int ancho = imagenGris.Width;
            int alto = imagenGris.Height;
            Bitmap resultado = new Bitmap(ancho, alto);

            int[,] sobelX = {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            };

            int[,] sobelY = {
                { -1, -2, -1 },
                { 0, 0, 0 },
                { 1, 2, 1 }
            };

            for (int y = 1; y < alto - 1; y++)
            {
                for (int x = 1; x < ancho - 1; x++)
                {
                    int gx = 0;
                    int gy = 0;

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            Color pixel = imagenGris.GetPixel(x + kx, y + ky);
                            int valor = pixel.R;
                            gx += sobelX[ky + 1, kx + 1] * valor;
                            gy += sobelY[ky + 1, kx + 1] * valor;
                        }
                    }

                    int magnitud = (int)Math.Sqrt(gx * gx + gy * gy);
                    magnitud = Math.Min(255, magnitud);
                    Color color = Color.FromArgb(magnitud, magnitud, magnitud);
                    resultado.SetPixel(x, y, color);
                }
            }

            return resultado;
        }

        private Bitmap ConvertirAGrises(Bitmap original)
        {
            Bitmap gris = new Bitmap(original.Width, original.Height);

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixel = original.GetPixel(x, y);
                    int promedio = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                    Color grisColor = Color.FromArgb(promedio, promedio, promedio);
                    gris.SetPixel(x, y, grisColor);
                }
            }

            return gris;
        }

        private void GuardarImagen()
        {
            if (pbProcesada.Image == null)
            {
                MessageBox.Show("No hay imagen procesada para guardar.", "Advertencia",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                sfd.Title = "Guardar imagen procesada";
                sfd.FileName = "bordes_detectados.png";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ImageFormat formato = ImageFormat.Png;
                        if (sfd.FilterIndex == 2) formato = ImageFormat.Jpeg;
                        else if (sfd.FilterIndex == 3) formato = ImageFormat.Bmp;

                        pbProcesada.Image.Save(sfd.FileName, formato);
                        MessageBox.Show("Imagen guardada correctamente.", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Resetear()
        {
            pbOriginal.Image = null;
            pbProcesada.Image = null;
            if (imagenOriginal != null)
            {
                imagenOriginal.Dispose();
                imagenOriginal = null;
            }
            if (imagenProcesada != null)
            {
                imagenProcesada.Dispose();
                imagenProcesada = null;
            }
        }
    }
}