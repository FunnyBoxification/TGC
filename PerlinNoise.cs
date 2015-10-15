using System;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;

namespace AlumnoEjemplos.Quicksort
{
    class PerlinNoise
    {
        static Random random = new Random();

        #region ::METODOS PRNCIPALES::
        /// <summary>
        /// Método que genera un nuevo Heightmap en forma de textura en base a un Perlin Noise.
        /// </summary>
        /// <param name="width">Ancho de la textura.</param>
        /// <param name="height">Alto de la textura.</param>
        /// <param name="octaves">Cantidad de octavas.</param>
        /// <param name="noise">out de la matriz flato[][] que representa el Perlin Noise.</param>        
        /// <returns>Una textura que contiene un Heightmap.</returns>
        public static Texture GetNuevoHeightmap(int width, int height, int octaves, out float[][] noise)
        {
            Color[][] image = PerlinNoise.MapToEscalaDeGrises(noise = PerlinNoise.GenerarPerlinNoise(width, height, octaves));
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    bitmap.SetPixel(i, j, image[i][j]);

            return Texture.FromBitmap(GuiController.Instance.D3dDevice, bitmap, Usage.None, Pool.Managed);
        }

        /// <summary>
        /// Método que obtiene un Perlin Noise en forma de Matriz de float[][].
        /// </summary>
        /// <param name="width">Alto del Perlin Noise.</param>
        /// <param name="height">Ancho del Perlin Noise.</param>
        /// <param name="octaveCount">Cantidad de octavas.</param>
        /// <returns>Matriz de floats.</returns>
        public static float[][] GenerarPerlinNoise(int width, int height, int octaveCount)
        {
            float[][] baseNoise = GenrarWhiteNoise(width, height);

            return GenerarPerlinNoise(baseNoise, octaveCount);
        }
        #endregion

        #region ::METODOS EXTRA::
        /// <summary>
        /// Método para obtener una matriz de floats con numeros aleatorios.
        /// </summary>
        /// <param name="width">Ancho de la matriz.</param>
        /// <param name="height">Alto de la matriz.</param>
        /// <returns>Matriz de floats[][].</returns>
        public static float[][] GenrarWhiteNoise(int width, int height)
        {
            float[][] noise = GetArrayVacio<float>(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    noise[i][j] = (float)random.NextDouble() % 1;
                }
            }

            return noise;
        }
        /// <summary>
        /// Método que interpola linealmente entre dos valores.
        /// </summary>
        /// <param name="x0">Valor incial.</param>
        /// <param name="x1">Valor final.</param>
        /// <param name="alpha">Peso.</param>
        /// <returns>Valor interpolado.</returns>
        public static float Interpolar(float x0, float x1, float alpha)
        {
            return x0 * (1 - alpha) + alpha * x1;
        }

        /// <summary>
        /// Método que interpola entre dos colores con un peso.
        /// </summary>
        /// <param name="col0">Color incial.</param>
        /// <param name="col1">Color final.</param>
        /// <param name="alpha">Peso.</param>
        /// <returns>Color interpolado.</returns>
        public static Color Interpolar(Color col0, Color col1, float alpha)
        {
            float beta = 1 - alpha;
            return Color.FromArgb(
                255,
                (int)(col0.R * alpha + col1.R * beta),
                (int)(col0.G * alpha + col1.G * beta),
                (int)(col0.B * alpha + col1.B * beta));
        }

        /// <summary>
        /// Método que Interpola Cosenoidalmente entre dos valores.
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static float InterpolarCoseno(float x0, float x1, float alpha)
        {
            float ft = alpha * (float)Math.PI;
            float f = (1 - (float)Math.Cos(ft)) * 0.5f;
            return x0 * (1 - f) + x1 * f;
        }

        public static Color GetColor(Color gradientStart, Color gradientEnd, float t)
        {
            float u = 1 - t;

            Color color = Color.FromArgb(
                255,
                (int)(gradientStart.R * u + gradientEnd.R * t),
                (int)(gradientStart.G * u + gradientEnd.G * t),
                (int)(gradientStart.B * u + gradientEnd.B * t));

            return color;
        }

        public static Color[][] MapGradient(Color gradientStart, Color gradientEnd, float[][] perlinNoise)
        {
            int width = perlinNoise.Length;
            int height = perlinNoise[0].Length;

            Color[][] image = GetArrayVacio<Color>(width, height); //an array of colours

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    image[i][j] = GetColor(gradientStart, gradientEnd, perlinNoise[i][j]);
                }
            }

            return image;
        }

        /// <summary>
        /// Metodo que genera una matriz vacía del tipo y tamaño especificados.
        /// </summary>
        /// <typeparam name="T">Tipo de dato de la matriz.</typeparam>
        /// <param name="width">Ancho de la matriz.</param>
        /// <param name="height">Alto de la matriz.</param>
        /// <returns>Una matriz vacía del tipo y tamaño especificados.</returns>
        public static T[][] GetArrayVacio<T>(int width, int height)
        {
            T[][] image = new T[width][];

            for (int i = 0; i < width; i++)
            {
                image[i] = new T[height];
            }

            return image;
        }


        public static float[][] GenrarRuidoSuave(float[][] baseNoise, int octave)
        {
            int width = baseNoise.Length;
            int height = baseNoise[0].Length;

            float[][] smoothNoise = GetArrayVacio<float>(width, height);

            int samplePeriod = 1 << octave; // calculates 2 ^ k
            float sampleFrequency = 1.0f / samplePeriod;

            for (int i = 0; i < width; i++)
            {
                //calculate the horizontal sampling indices
                int sample_i0 = (i / samplePeriod) * samplePeriod;
                int sample_i1 = (sample_i0 + samplePeriod) % width; //wrap around
                float horizontal_blend = (i - sample_i0) * sampleFrequency;

                for (int j = 0; j < height; j++)
                {
                    //calculate the vertical sampling indices
                    int sample_j0 = (j / samplePeriod) * samplePeriod;
                    int sample_j1 = (sample_j0 + samplePeriod) % height; //wrap around
                    float vertical_blend = (j - sample_j0) * sampleFrequency;

                    //blend the top two corners
                    float top = Interpolar(baseNoise[sample_i0][sample_j0],
                        baseNoise[sample_i1][sample_j0], horizontal_blend);

                    //blend the bottom two corners
                    float bottom = Interpolar(baseNoise[sample_i0][sample_j1],
                        baseNoise[sample_i1][sample_j1], horizontal_blend);

                    //final blend
                    smoothNoise[i][j] = Interpolar(top, bottom, vertical_blend);
                }
            }

            return smoothNoise;
        }

        /// <summary>
        /// Método que genera una matriz de Perlin Noise.
        /// </summary>
        /// <param name="baseNoise">Matriz de ruido base que se usará para generar el Perlin Noise final.</param>
        /// <param name="octaveCount">Cantidad de octavas.</param>
        /// <returns>Una matriz que representa un Perlin Noise.</returns>
        public static float[][] GenerarPerlinNoise(float[][] baseNoise, int octaveCount)
        {
            int width = baseNoise.Length;
            int height = baseNoise[0].Length;

            float[][][] smoothNoise = new float[octaveCount][][]; //an array of 2D arrays containing

            float persistance = 0.7f;

            //genera ruido más suave
            for (int i = 0; i < octaveCount; i++)
            {
                smoothNoise[i] = GenrarRuidoSuave(baseNoise, i);
            }

            // Cargo una matriz de float con el tamaño adecuado, inicializada en 0
            float[][] perlinNoise = GetArrayVacio<float>(width, height);

            float amplitude = 1.0f;
            float totalAmplitude = 0.0f;

            //blend noise together
            for (int octave = octaveCount - 1; octave >= 0; octave--)
            {
                amplitude *= persistance;
                totalAmplitude += amplitude;

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        perlinNoise[i][j] += smoothNoise[octave][i][j] * amplitude;
                    }
                }
            }

            //normalizado
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    perlinNoise[i][j] /= totalAmplitude;
                }
            }

            return perlinNoise;
        }

        /// <summary>
        /// Metodo que convierte una matriz de ruido en una matriz de colores.
        /// </summary>
        /// <param name="greyValues">Matriz de float[][] que representa un Perlin Noise.</param>
        /// <returns>Una matriz de Color[][] que contiene valores de grises.</returns>
        public static Color[][] MapToEscalaDeGrises(float[][] greyValues)
        {
            int width = greyValues.Length;
            int height = greyValues[0].Length;

            Color[][] image = GetArrayVacio<Color>(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int grey = (int)(255 * greyValues[i][j]);
                    Color color = Color.FromArgb(255, grey, grey, grey);

                    image[i][j] = color;
                }
            }

            return image;
        }

        /// <summary>
        /// Método que obtiene un Normalmap de un Perlin Noise.
        /// </summary>
        /// <param name="NoiseArray">Perlin Noise representado como una matriz de floats.</param>
        /// <returns>Matriz de Vectores donde cada punto es un verctor normal a la superficie.</returns>
        public static Vector3[][] GetNormalmapDePerlinNoise(float[][] NoiseArray)
        {
            int width = NoiseArray.GetLength(0);
            int height = NoiseArray[0].GetLength(0);
            var ScaleY = 1;
            var ScaleXZ = 1;
            var Normal = new Vector3[width][];
            for (int i = 0; i < width; ++i)
            {
                Normal[i] = new Vector3[height];

                for (int j = 0; j < height; ++j)
                {

                    // Las comparaciones necesarias para ver los casos en los que esta en el borde de la matriz.
                    float sx = NoiseArray[i < width - 1 ? i + 1 : i][j] - NoiseArray[i > 0 ? i - 1 : i][j];
                    if (i == 0 || i == width - 1)
                        sx *= 2;

                    float sy = NoiseArray[i][j < height - 1 ? j + 1 : j] - NoiseArray[i][j > 0 ? j - 1 : j];
                    if (j == 0 || j == height - 1)
                        sy *= 2;

                    // Carga y normaliza las normales de cada punto
                    Normal[i][j] = new Vector3(-sx * ScaleY, 2 * ScaleXZ, sy * ScaleY);
                    Normal[i][j].Normalize();
                }
            }
            return Normal;
        }

        /// <summary>
        /// Método que transforma una matrix Vector3[][] representando las normales de un Heightmap en una textura.
        /// </summary>
        /// <param name="Normalmap">Matrix de Vector3[][] de normales.</param>
        /// <returns></returns>
        public static Texture GetTextureFromNormal(Vector3[][] Normalmap)
        {
            int width = Normalmap.GetLength(0);
            int height = Normalmap[0].GetLength(0);

            var Normals = new Color[width][];
            for (int i = 0; i < width; ++i)
            {
                Normals[i] = new Color[height];

                for (int j = 0; j < height; ++j)
                {
                    ColorValue color = new ColorValue(Normalmap[i][j].X, Normalmap[i][j].Y, Normalmap[i][j].Z);
                    Normals[i][j] = Color.FromArgb(color.ToArgb());
                }
            }

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    bitmap.SetPixel(i, j, Normals[i][j]);

            return Texture.FromBitmap(GuiController.Instance.D3dDevice, bitmap, Usage.None, Pool.Managed);

        }
        #endregion
    }
}