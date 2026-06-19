using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace ClasificadorTexturas
{
    public class Form1 : Form  // QUITÉ "partial" porque no hay Designer
    {
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Button btnCargar;
        private Button btnClasificar;
        private Button btnGuardar;
        private Label lblResultados;
        private Label lblTitulo;

        private Bitmap imagenOriginal;
        private Bitmap imagenSobrepuesta;
        private Dictionary<string, double> ultimosPorcentajes;
        private string ultimaRutaImagen;
        private string ultimaTexturapredominante;
        private double ultimaConfianza;

        // Colores de referencia
        private readonly (string Nombre, Color ColorReferencia, Color ColorSobrepuesto)[] texturas = new[]
        {
            ("PASTO",    Color.FromArgb(75, 135, 55),     Color.FromArgb(0, 255, 0)),
            ("TIERRA",   Color.FromArgb(155, 125, 85),    Color.FromArgb(139, 69, 19)),
            ("CEMENTO",  Color.FromArgb(185, 185, 180),   Color.FromArgb(128, 128, 128)),
            ("ASFALTO",  Color.FromArgb(55, 55, 58),      Color.FromArgb(0, 0, 0))
        };

        // CAMBIA ESTA CADENA DE CONEXIÓN según tu SQL Server
        private readonly string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=TexturasDB;Trusted_Connection=True;";
        // Si usas SQL Server full, usa algo como:
        // private readonly string connectionString = @"Server=localhost\SQLEXPRESS;Database=TexturasDB;Trusted_Connection=True;";

        public Form1()
        {
            InicializarFormulario();
            InicializarControles();
            InicializarBaseDeDatos();
            ActualizarEstadoBotones();
        }

        private void InicializarFormulario()
        {
            this.Text = "CLASIFICADOR DE TEXTURAS - FOTOGRAMETRÍA";
            this.Size = new Size(1100, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
        }

        private void InicializarControles()
        {
            // Título
            lblTitulo = new Label
            {
                Text = "📊 CLASIFICADOR DE TEXTURAS - FOTOGRAMETRÍA",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            this.Controls.Add(lblTitulo);

            // PictureBox 1 (Original)
            pictureBox1 = new PictureBox
            {
                Location = new Point(20, 50),
                Size = new Size(400, 400),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };
            this.Controls.Add(pictureBox1);

            Label lbl1 = new Label
            {
                Text = "Imagen Original",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(20, 460),
                AutoSize = true
            };
            this.Controls.Add(lbl1);

            // PictureBox 2 (Sobrepuesto)
            pictureBox2 = new PictureBox
            {
                Location = new Point(640, 50),
                Size = new Size(400, 400),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };
            this.Controls.Add(pictureBox2);

            Label lbl2 = new Label
            {
                Text = "Imagen con Sobreposición",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(640, 460),
                AutoSize = true
            };
            this.Controls.Add(lbl2);

            // Botones
            btnCargar = new Button
            {
                Text = "📁 Cargar Imagen",
                Location = new Point(20, 490),
                Size = new Size(150, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCargar.Click += BtnCargar_Click;
            this.Controls.Add(btnCargar);

            btnClasificar = new Button
            {
                Text = "🎨 Clasificar",
                Location = new Point(180, 490),
                Size = new Size(150, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnClasificar.Click += BtnClasificar_Click;
            this.Controls.Add(btnClasificar);

            btnGuardar = new Button
            {
                Text = "💾 Guardar Resultados",
                Location = new Point(340, 490),
                Size = new Size(150, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            // Panel de resultados
            Panel panelResultados = new Panel
            {
                Location = new Point(20, 540),
                Size = new Size(1020, 200),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            Label lblEncabezado = new Label
            {
                Text = "📊 RESULTADOS DEL ANÁLISIS",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(10, 10),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true
            };
            panelResultados.Controls.Add(lblEncabezado);

            lblResultados = new Label
            {
                Location = new Point(10, 35),
                Size = new Size(990, 150),
                Font = new Font("Courier New", 9),
                Text = "Carga una imagen y clasifícala para ver resultados",
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = false
                // WordWrap eliminado - no existe en .NET 10
            };
            panelResultados.Controls.Add(lblResultados);

            this.Controls.Add(panelResultados);

            ultimosPorcentajes = new Dictionary<string, double>();
        }

        private void ActualizarEstadoBotones()
        {
            btnClasificar.Enabled = imagenOriginal != null;
            btnGuardar.Enabled = imagenSobrepuesta != null && ultimosPorcentajes.Count > 0;
        }

        private void BtnCargar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp|Todos los archivos|*.*";
                dialog.Title = "Seleccionar imagen";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        imagenOriginal = new Bitmap(dialog.FileName);
                        pictureBox1.Image = imagenOriginal;
                        ultimaRutaImagen = dialog.FileName;
                        imagenSobrepuesta = null;
                        pictureBox2.Image = null;
                        lblResultados.Text = "Imagen cargada. Haz clic en 'Clasificar' para analizar.";
                        ActualizarEstadoBotones();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al cargar la imagen: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnClasificar_Click(object sender, EventArgs e)
        {
            if (imagenOriginal == null)
            {
                MessageBox.Show("Por favor carga una imagen primero.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var (conteos, porcentajes) = AnalizarTexturas(imagenOriginal);
                ultimosPorcentajes = porcentajes;

                imagenSobrepuesta = AplicarSobreposicion(imagenOriginal);
                pictureBox2.Image = imagenSobrepuesta;

                var paresOrdenados = porcentajes.OrderByDescending(x => x.Value).ToList();
                ultimaTexturapredominante = paresOrdenados[0].Key;
                double mayor = paresOrdenados[0].Value;
                double segundo = paresOrdenados.Count > 1 ? paresOrdenados[1].Value : 0;
                ultimaConfianza = (mayor - segundo) / 100.0;

                MostrarResultados(porcentajes, ultimaTexturapredominante, ultimaConfianza);
                ActualizarEstadoBotones();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al clasificar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (imagenSobrepuesta == null || ultimosPorcentajes.Count == 0)
            {
                MessageBox.Show("Primero debes clasificar una imagen.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                byte[] imagenBytes = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    imagenOriginal.Save(ms, ImageFormat.Jpeg);
                    imagenBytes = ms.ToArray();
                }

                string nombreImagen = Path.GetFileName(ultimaRutaImagen);
                GuardarEnBaseDeDatos(nombreImagen, ultimosPorcentajes, ultimaTexturapredominante, ultimaConfianza, imagenBytes);
                MessageBox.Show("Resultados guardados exitosamente en la base de datos.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private (Dictionary<string, int> conteos, Dictionary<string, double> porcentajes) AnalizarTexturas(Bitmap img)
        {
            Dictionary<string, int> conteos = new Dictionary<string, int>();
            foreach (var textura in texturas)
            {
                conteos[textura.Nombre] = 0;
            }

            int ancho = img.Width;
            int alto = img.Height;
            int pixelesAnalizados = 0;

            for (int y = 1; y < alto - 1; y++)
            {
                for (int x = 1; x < ancho - 1; x++)
                {
                    int sumaR = 0, sumaG = 0, sumaB = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            Color pixel = img.GetPixel(x + dx, y + dy);
                            sumaR += pixel.R;
                            sumaG += pixel.G;
                            sumaB += pixel.B;
                        }
                    }

                    Color promedio = Color.FromArgb(sumaR / 9, sumaG / 9, sumaB / 9);
                    string texturaCercana = EncontrarTexturaCercana(promedio);
                    conteos[texturaCercana]++;
                    pixelesAnalizados++;
                }
            }

            Dictionary<string, double> porcentajes = new Dictionary<string, double>();
            foreach (var textura in texturas)
            {
                double porcentaje = pixelesAnalizados > 0 ? (conteos[textura.Nombre] / (double)pixelesAnalizados) * 100.0 : 0;
                porcentajes[textura.Nombre] = Math.Round(porcentaje, 1);
            }

            return (conteos, porcentajes);
        }

        private string EncontrarTexturaCercana(Color promedio)
        {
            string texturaCercana = texturas[0].Nombre;
            double distanciaMinima = double.MaxValue;

            foreach (var textura in texturas)
            {
                double distancia = CalcularDistancia(promedio, textura.ColorReferencia);
                if (distancia < distanciaMinima)
                {
                    distanciaMinima = distancia;
                    texturaCercana = textura.Nombre;
                }
            }

            return texturaCercana;
        }

        private double CalcularDistancia(Color c1, Color c2)
        {
            int dr = c1.R - c2.R;
            int dg = c1.G - c2.G;
            int db = c1.B - c2.B;
            return Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        private Bitmap AplicarSobreposicion(Bitmap original)
        {
            Bitmap resultado = new Bitmap(original.Width, original.Height);
            int ancho = original.Width;
            int alto = original.Height;

            for (int y = 0; y < alto; y++)
            {
                for (int x = 0; x < ancho; x++)
                {
                    Color colorResultado;

                    if (x == 0 || x == ancho - 1 || y == 0 || y == alto - 1)
                    {
                        colorResultado = Color.FromArgb(200, 200, 200);
                    }
                    else
                    {
                        int sumaR = 0, sumaG = 0, sumaB = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                Color pixel = original.GetPixel(x + dx, y + dy);
                                sumaR += pixel.R;
                                sumaG += pixel.G;
                                sumaB += pixel.B;
                            }
                        }

                        Color promedio = Color.FromArgb(sumaR / 9, sumaG / 9, sumaB / 9);
                        string texturaCercana = EncontrarTexturaCercana(promedio);
                        var textura = texturas.FirstOrDefault(t => t.Nombre == texturaCercana);
                        colorResultado = textura.ColorSobrepuesto;
                    }

                    resultado.SetPixel(x, y, colorResultado);
                }
            }

            return resultado;
        }

        private void MostrarResultados(Dictionary<string, double> porcentajes, string predominante, double confianza)
        {
            string resultado = "🌍 ANÁLISIS COMPLETADO\n";
            resultado += "─────────────────────────────────────────────────────────\n\n";

            foreach (var textura in texturas)
            {
                double porcentaje = porcentajes[textura.Nombre];
                int barras = (int)(porcentaje / 4);
                string barraVisual = new string('█', barras) + new string('░', 25 - barras);
                resultado += $"{textura.Nombre.PadRight(10)} {porcentaje:F1}%".PadRight(20) + $"│{barraVisual}│\n";
            }

            resultado += "\n─────────────────────────────────────────────────────────\n\n";
            resultado += $"🏆 TEXTURA PREDOMINANTE: {predominante} ({porcentajes[predominante]:F1}%)\n";
            resultado += $"📈 CONFIANZA: {Math.Round(confianza * 100, 1)}%";

            lblResultados.Text = resultado;
        }

        private void InicializarBaseDeDatos()
        {
            try
            {
                string masterConnection = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;";

                using (SqlConnection conn = new SqlConnection(masterConnection))
                {
                    conn.Open();
                    string createDbQuery = @"
                        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TexturasDB')
                        BEGIN
                            CREATE DATABASE TexturasDB;
                        END
                    ";
                    using (SqlCommand cmd = new SqlCommand(createDbQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string createTableQuery = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HistorialClasificaciones')
                        BEGIN
                            CREATE TABLE HistorialClasificaciones (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                NombreImagen NVARCHAR(255) NOT NULL,
                                FechaClasificacion DATETIME NOT NULL DEFAULT GETDATE(),
                                PorcentajePasto FLOAT NOT NULL,
                                PorcentajeTierra FLOAT NOT NULL,
                                PorcentajeCemento FLOAT NOT NULL,
                                PorcentajeAsfalto FLOAT NOT NULL,
                                TexturaPredominante NVARCHAR(50) NOT NULL,
                                ConfianzaPromedio FLOAT NOT NULL,
                                RutaImagenOriginal NVARCHAR(500) NULL,
                                ImagenBytes VARBINARY(MAX) NULL
                            );
                        END
                    ";
                    using (SqlCommand cmd = new SqlCommand(createTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar base de datos: {ex.Message}\n\nLa aplicación funcionará pero no podrá guardar.",
                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GuardarEnBaseDeDatos(string nombreImagen, Dictionary<string, double> porcentajes,
            string predominante, double confianza, byte[] imagenBytes)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string insertQuery = @"
                    INSERT INTO HistorialClasificaciones 
                    (NombreImagen, PorcentajePasto, PorcentajeTierra, PorcentajeCemento, 
                     PorcentajeAsfalto, TexturaPredominante, ConfianzaPromedio, RutaImagenOriginal, ImagenBytes)
                    VALUES (@nombre, @pasto, @tierra, @cemento, @asfalto, @predominante, @confianza, @ruta, @imagen)
                ";
                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombreImagen);
                    cmd.Parameters.AddWithValue("@pasto", porcentajes["PASTO"]);
                    cmd.Parameters.AddWithValue("@tierra", porcentajes["TIERRA"]);
                    cmd.Parameters.AddWithValue("@cemento", porcentajes["CEMENTO"]);
                    cmd.Parameters.AddWithValue("@asfalto", porcentajes["ASFALTO"]);
                    cmd.Parameters.AddWithValue("@predominante", predominante);
                    cmd.Parameters.AddWithValue("@confianza", confianza);
                    cmd.Parameters.AddWithValue("@ruta", ultimaRutaImagen ?? "");
                    cmd.Parameters.AddWithValue("@imagen", imagenBytes ?? Array.Empty<byte>());

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}