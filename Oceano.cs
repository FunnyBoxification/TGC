using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.Interpolation;
using TgcViewer.Utils.Shaders;

namespace AlumnoEjemplos.Quicksort
{
    /// <summary>
    /// Herramienta para crear el Oceano.
    /// </summary>
    public static class Oceano
    {
        // The two-triangle generated model for the ocean
        //Parametros del plano
        public const int TAMAÑO = 4000; //dado que el plano es cuadrado X y Z son iguales
        public const int DISTANCIA_ENTRE_VERTICES = 25;

        public static Vector3 _normal = new Vector3(0f, 1f, 0f);
        public static Vector3 _pos = new Vector3(0, 400, 0);

        //Formulas
        public const int RADIO = TAMAÑO / DISTANCIA_ENTRE_VERTICES;
        public const int LARGO = (2 * RADIO + 1);
        public const int CANTIDAD_DE_VERTICES = LARGO * LARGO;
        public const int CANTIDAD_DE_TRIANGULOS = 2 * (LARGO - 1) * (LARGO - 1);
        
        // Buffers
        public static CustomVertex.PositionNormalTextured[] _vertices;
        public static VertexBuffer _vertexBuffer;

        // Texturas
        public static Texture surf_reflection, surf_refraction, surf_fresnel;

        public static Texture textPerlinNoise1, textPerlinNoise2,
                                textPerlinNoise1_2Octavas, textPerlinNoise2_2Octavas,
                                textPerlinNoise1_4Octavas, textPerlinNoise2_4Octavas,
                                textPerlinNoise1_8Octavas, textPerlinNoise2_8Octavas;

        public static float[][] PerlinNoise1, PerlinNoise2,
                                PerlinNoise1_2Octavas, PerlinNoise2_2Octavas,
                                PerlinNoise1_4Octavas, PerlinNoise2_4Octavas,
                                PerlinNoise1_8Octavas, PerlinNoise2_8Octavas;

        // Shaders
        public static Effect PerlinShader;

        public static InterpoladorVaiven interpoladorPerlinNoiseHeightmaps;

        //public static Surface g_depthstencil;

        public static float Desplazamiento = 0;

        public static Surface depthstencil;

        /// <summary>
        /// Metodo que carga los valores necesarios para inicializar el Oceano.
        /// </summary>
        public static void Cargar()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            // Creo la textura de reflexion
            surf_reflection = new Texture(d3dDevice, GuiController.Instance.Panel3d.Width, GuiController.Instance.Panel3d.Height, 1, Usage.RenderTarget, Format.A32B32G32R32F, Pool.Default);

            // Creo la textura de refraccion
            surf_refraction = new Texture(d3dDevice, GuiController.Instance.Panel3d.Width, GuiController.Instance.Panel3d.Height, 1, Usage.RenderTarget, Format.A32B32G32R32F, Pool.Default);

            // Cargo la textura de fresnel (relación entre los campos eléctricos transmitido y reflejado) "fresnel_water_sRGB.bmp"
            surf_fresnel = TextureLoader.FromFile(d3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "Texturas\\fresnel_water_sRGB.bmp");

            // Carga el shader del movimiento del oceano 
            //PerlinShader = Utiles.CargarShaderConTechnique("perlin.fx");


            Effect PerlinShader = TgcShaders.loadEffect(GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Shaders\\shader_agua.fx");
            //Effect shader = ShaderUtils.loadEffect(Utiles.ShadersDir(Nombre));
            PerlinShader.Technique = "RenderAgua";


            // Cargar informacion de vertices: (X,Y,Z) + coord textura
            _vertices = new CustomVertex.PositionNormalTextured[CANTIDAD_DE_VERTICES];
            int i = 0;
            for (int x = -RADIO; x <= RADIO; x++)
            {
                for (int z = -RADIO; z <= RADIO; z++)
                {
                    _vertices[i++] = new CustomVertex.PositionNormalTextured(
                        new Vector3(x * DISTANCIA_ENTRE_VERTICES,400, z * DISTANCIA_ENTRE_VERTICES),
                        _normal,
                        ((float)(x + RADIO) / ((float)LARGO - 1)),
                        ((float)(z + RADIO) / ((float)LARGO - 1))
                     );
                }
            };

            // Creamos el VertexBuffer
            _vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), CANTIDAD_DE_VERTICES, d3dDevice, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionNormalTextured.Format, Pool.Default);

            // Almacenar información en VertexBuffer
            _vertexBuffer.SetData(_vertices, 0, LockFlags.None);

            //Creo el quadTree para este terreno
            QuadTree.Cargar(_pos.X, _pos.Z, TAMAÑO, GuiController.Instance.D3dDevice);

            // creo los indices para el IndexBuffer usando un array de int
            // Son por 3 vertices por triangulo y son 2 triangulos
            for (int z = 0; z < LARGO - 1; z++)
                for (int x = 0; x < LARGO - 1; x++)
                {
                    var lista = new List<int>();
                    //Primer Triangulo
                    lista.Add(x + z * LARGO);
                    lista.Add(x + 1 + z * LARGO);
                    lista.Add(x + LARGO + 1 + z * LARGO);

                    //Segundo Triangulo
                    lista.Add(x + LARGO + 1 + z * LARGO);
                    lista.Add(x + LARGO + z * LARGO);
                    lista.Add(x + z * LARGO);

                    //Cargo los indices en los nodos del QuadTree
                    QuadTree.AgregarIndices(lista);
                };
            //LOD I
            for (int z = 0; z < LARGO - 1; z = z + 2)
                for (int x = 0; x < LARGO - 1; x = x + 2)
                {
                    var lista = new List<int>();
                    //Primer Triangulo
                    lista.Add(x + z * LARGO);
                    lista.Add(x + 2 + z * LARGO);
                    lista.Add(x + 2 * LARGO + 2 + z * LARGO);

                    //Segundo Triangulo
                    lista.Add(x + 2 * LARGO + 2 + z * LARGO);
                    lista.Add(x + 2 * LARGO + z * LARGO);
                    lista.Add(x + z * LARGO);

                    //Cargo los indices en los nodos del QuadTree
                    QuadTree.AgregarLODI(lista);
                };
            //LOD II
            for (int z = 0; z < LARGO - 1; z = z + 4)
                for (int x = 0; x < LARGO - 1; x = x + 4)
                {
                    var lista = new List<int>();
                    //Primer Triangulo
                    lista.Add(x + z * LARGO);
                    lista.Add(x + 4 + z * LARGO);
                    lista.Add(x + 4 * LARGO + 4 + z * LARGO);

                    //Segundo Triangulo
                    lista.Add(x + 4 * LARGO + 4 + z * LARGO);
                    lista.Add(x + 4 * LARGO + z * LARGO);
                    lista.Add(x + z * LARGO);

                    //Cargo los indices en los nodos del QuadTree
                    QuadTree.AgregarLODII(lista);
                };


            // Genera los heightmaps entre los que interpola la superficie, se generan para 2,4 y 8 octavas para poder usar
            // las diferentes configuraciones cambiando los Modifiers.

            // 2 ocatavas (ruido fuerte).
            // perlin 1
            textPerlinNoise1_2Octavas = PerlinNoise.GetNuevoHeightmap(Oceano.LARGO, Oceano.LARGO, 2, out PerlinNoise1_2Octavas);
            textPerlinNoise2_2Octavas = PerlinNoise.GetNuevoHeightmap(Oceano.LARGO, Oceano.LARGO, 2, out PerlinNoise2_2Octavas);
            // 4 ocatavas (ruido normal).
            textPerlinNoise1_4Octavas = PerlinNoise.GetNuevoHeightmap(Oceano.LARGO, Oceano.LARGO, 4, out PerlinNoise1_4Octavas);
            textPerlinNoise2_4Octavas = PerlinNoise.GetNuevoHeightmap(Oceano.LARGO, Oceano.LARGO, 4, out PerlinNoise2_4Octavas);
            // 8 octavas (ruido suave).
            textPerlinNoise1_8Octavas = PerlinNoise.GetNuevoHeightmap(Oceano.LARGO, Oceano.LARGO, 8, out PerlinNoise1_8Octavas);
            textPerlinNoise2_8Octavas = PerlinNoise.GetNuevoHeightmap(Oceano.LARGO, Oceano.LARGO, 8, out PerlinNoise2_8Octavas);

            // Carga los valores iniciales de la Matriz de Perlin Noise.
            PerlinNoise1 = PerlinNoise1_8Octavas;
            PerlinNoise2 = PerlinNoise2_8Octavas;

            // Carga los valores iniciales de la textura que se usara como Heightmap y Normalmap para la superficie del oceano.
            textPerlinNoise1 = textPerlinNoise1_8Octavas;
            textPerlinNoise2 = textPerlinNoise2_8Octavas;

            // Peso (alpha) para interpolar, usa el InterpoladorVaiven para ir alterandolo.
            interpoladorPerlinNoiseHeightmaps = new InterpoladorVaiven();
            interpoladorPerlinNoiseHeightmaps.Min = 0;
            interpoladorPerlinNoiseHeightmaps.Max = 1;
            interpoladorPerlinNoiseHeightmaps.Speed = 0.5f;


            //guardar el stencil inicial
            //g_depthstencil = d3dDevice.DepthStencilSurface;
        }


        /// <summary>
        /// Render del oceano y su shader.
        /// </summary>
        public static void Render()
        {
            /*if (!ParametrosDeConfiguracion.RenderOceano)
                return;*/

            Device d3dDevice = GuiController.Instance.D3dDevice;
            
            bool culling = true;
                        
            if (culling)
            {
                // cargo la camara fps para hacer el culling en base al frustum de esa camara
                GuiController.Instance.ThirdPersonCamera.Enable = false;
                GuiController.Instance.FpsCamera.Enable = true;
               // GuiController.Instance.FpsCamera.setCamera(Barco.mesh.Position, Barco.vDireccion * GuiController.Instance.Frustum.FarPlane.D);
                GuiController.Instance.CurrentCamera.updateCamera();
                GuiController.Instance.CurrentCamera.updateViewMatrix(d3dDevice);
                GuiController.Instance.Frustum.updateVolume(d3dDevice.Transform.View, d3dDevice.Transform.Projection);
            }

            // Especificar formato de triangulos
            d3dDevice.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            d3dDevice.SetStreamSource(0, _vertexBuffer, 0);

            // Almacenar información en IndexBuffer
            var listaIndices = QuadTree.IndiceVerticesVisibles();
            var cantidadDeIndices = listaIndices.Count;
            if (cantidadDeIndices != 0)
            {
                var indexBuffer = new IndexBuffer(typeof(int), sizeof(int) * cantidadDeIndices, d3dDevice, Usage.Dynamic | Usage.WriteOnly, Pool.Default);
                indexBuffer.SetData(listaIndices.ToArray(), 0, LockFlags.None);
                d3dDevice.Indices = indexBuffer;
            }
                        
            if (culling)
            {
                // vuelvo a cargar la camara en 3ra persona
                GuiController.Instance.ThirdPersonCamera.Enable = true;
                GuiController.Instance.FpsCamera.Enable = false;
                GuiController.Instance.CurrentCamera.updateCamera();
                GuiController.Instance.CurrentCamera.updateViewMatrix(d3dDevice);
            }

            // Cargamos parametros en el shader

            // Matrices de transformacion para pasar de world a clip space
            PerlinShader.SetValue("world", GuiController.Instance.D3dDevice.Transform.World);
            PerlinShader.SetValue("view", GuiController.Instance.D3dDevice.Transform.View);
            PerlinShader.SetValue("proj", GuiController.Instance.D3dDevice.Transform.Projection);

            // Propiedades del Sol (posicion, shiness, strength)
           // PerlinShader.SetValue("sun_vec", new Vector4(Sol.Posicion.X, Sol.Posicion.Y, Sol.Posicion.Z, 0.0f));
           // PerlinShader.SetValue("sun_shininess", 4 * ParametrosDeConfiguracion.Sol.Shininess);
           // PerlinShader.SetValue("sun_strength", ParametrosDeConfiguracion.Sol.Strength);

            // Valores de Enviroment Mapping
            PerlinShader.SetValue("reflrefr_offset", 2.0f);
            PerlinShader.SetValue("LODbias", 0.5f);
            Vector3 posCamara = GuiController.Instance.CurrentCamera.getPosition();
            PerlinShader.SetValue("view_position", new Vector4(posCamara.X, posCamara.Y, posCamara.Z, 1));
            PerlinShader.SetValue("EnvironmentMap", TextureLoader.FromCubeFile(GuiController.Instance.D3dDevice, GuiController.Instance.ExamplesMediaDir + "cubemap-evul.dds"));
            PerlinShader.SetValue("FresnelMap", surf_fresnel);
            PerlinShader.SetValue("Refractionmap", surf_refraction);
            PerlinShader.SetValue("Reflectionmap", surf_reflection);

            // Dimensiones del Oceano
            PerlinShader.SetValue("radio", RADIO);
            PerlinShader.SetValue("largo", LARGO);
            PerlinShader.SetValue("dev", DISTANCIA_ENTRE_VERTICES);

            // periodo del día, para saber si reflejar el sol o la luna
            PerlinShader.SetValue("noche", true);

            // Caso particular para placas que no tengan texture lookup de 
            // cualquier textura se usa el formato particular A32B32G32R32F.
            /*if (ParametrosDeConfiguracion.TexturaA32B32G32R32F)
            {
                textPerlinNoise1 = TextureLoader.FromFile(d3dDevice, Utiles.TexturasDir("PerlinNoiseHeightmap1.png"), 0, 0, 1, 0,
                             Format.A32B32G32R32F, Pool.Managed, Filter.Point, Filter.Point, Color.Black.ToArgb());
                textPerlinNoise2 = TextureLoader.FromFile(d3dDevice, Utiles.TexturasDir("PerlinNoiseHeightmap2.png"), 0, 0, 1, 0,
                   Format.A32B32G32R32F, Pool.Managed, Filter.Point, Filter.Point, Color.Black.ToArgb());
            }*/
            /*else
            {*/
                // Setea las texturas de acuerdo a las octavas seleccionadas en el combo.
                /*switch (ParametrosDeConfiguracion.Agua.Octavas)
                {
                    case 2:*/
                        textPerlinNoise1 = textPerlinNoise1_2Octavas;
                        textPerlinNoise2 = textPerlinNoise2_2Octavas;
                        PerlinNoise1 = PerlinNoise1_2Octavas;
                        PerlinNoise2 = PerlinNoise2_2Octavas;
                        //break;
                    /*case 4:
                        textPerlinNoise1 = textPerlinNoise1_4Octavas;
                        textPerlinNoise2 = textPerlinNoise2_4Octavas;
                        PerlinNoise1 = PerlinNoise1_4Octavas;
                        PerlinNoise2 = PerlinNoise2_4Octavas;
                        break;
                    case 8:
                        textPerlinNoise1 = textPerlinNoise1_8Octavas;
                        textPerlinNoise2 = textPerlinNoise2_8Octavas;
                        PerlinNoise1 = PerlinNoise1_8Octavas;
                        PerlinNoise2 = PerlinNoise2_8Octavas;
                        break;
                }
            }*/

            // Texturas que representan los heightmaps
            PerlinShader.SetValue("perlinNoise1", textPerlinNoise1);
            PerlinShader.SetValue("perlinNoise2", textPerlinNoise2);

            // Parametros para el movimiento del oceano
            Desplazamiento += GuiController.Instance.ElapsedTime;
            PerlinShader.SetValue("desplazamiento", Desplazamiento);
            PerlinShader.SetValue("maxHeightSuperficial", 200);
            PerlinShader.SetValue("amplitud", 5f);
            PerlinShader.SetValue("frecuencia", 6.5f);
            PerlinShader.SetValue("smallvalue", 2f);

            // Niebla
            //PerlinShader.SetValue("FogActiva", ParametrosDeConfiguracion.Niebla);
            //ColorValue fogcolor = Utiles.CamaraSumergida ? ColorValue.FromColor(ParametrosDeConfiguracion.Agua.Color) : ColorValue.FromColor(Color.DarkGray);
           // PerlinShader.SetValue("FogColor", fogcolor);
           // PerlinShader.SetValue("FogStart", Niebla.START);
            //PerlinShader.SetValue("FogEnd", Niebla.END);

            // Colorear el oceano bajo la moneda
            //Vector4 moneda_pos = new Vector4(Juego.Moneda.Position.X, Juego.Moneda.Position.Y, Juego.Moneda.Position.Z, 1);
            //PerlinShader.SetValue("moneda_pos", moneda_pos);

            // Posicion de la isla para atenuacion de las olas
            PerlinShader.SetValue("isla_pos", new Vector4(0f, 0f, 0f, 1));


            // Parametro de animacion del movimiento superficial
            // para interpolar entre los dos heightmaps de perlin noise
            interpoladorPerlinNoiseHeightmaps.update();
            PerlinShader.SetValue("alpha", interpoladorPerlinNoiseHeightmaps.Current);

            // Aplico el effect            
            /*if (ParametrosDeConfiguracion.Shader)
            {
                PerlinShader.Begin(FX.None);
                PerlinShader.BeginPass(0);
            }*/

            // Dibuja la superficie
            d3dDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CANTIDAD_DE_VERTICES, 0, cantidadDeIndices / 3);

            // Fin del effect
            /*if (ParametrosDeConfiguracion.Shader)
            {
                PerlinShader.EndPass();
                PerlinShader.End();
            }*/
        }


        /// <summary>
        /// Render cuándo la camara está sumergida bajo el agua.
        /// </summary>        
        public static void RenderSubmarino()
        {
            //falta...
        }


        public static void RenderReflexion()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;

            // seteamos rendertarget de reflexion
            Surface target, bb;
            bb = d3dDevice.GetRenderTarget(0);
            target = Oceano.surf_reflection.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, target);
            //d3dDevice.DepthStencilSurface = Oceano.depthstencil;

            // limpiamos target & z en el device
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, 0, 1.0f, 0);

            // reflejamos la escena
            Matrix store, scale;
            store = d3dDevice.GetTransform(TransformType.World);
           // scale = Utiles.CamaraSumergida ? Matrix.Identity : Matrix.Scaling(1, -1, 1);
            //d3dDevice.MultiplyTransform(TransformType.World, scale);

            // agregamos un clip-plane
            //Plane cplane = new Plane(0, -1, 0, 0);
            //d3dDevice.ClipPlanes[0].Plane = cplane;
            //d3dDevice.SetRenderState(RenderStates.ClipPlaneEnable, true);

            //en el reflejo el sol y barco va invertidos con respecto a y
            //Sol.Render(EstadoRender.REFLEXION);
            //Barco.Render(EstadoRender.REFLEXION);

            // sacamos el clip plane
            //d3dDevice.SetRenderState(RenderStates.ClipPlaneEnable, false);
            //d3dDevice.ClipPlanes[0].Enabled = false;

            // restoreamos el render target original
            d3dDevice.SetTransform(TransformType.World, store);
            d3dDevice.SetRenderTarget(0, bb);
            //d3dDevice.DepthStencilSurface = g_depthstencil;
        }


        public static void RenderRefraccion()
        {
            Device d3dDevice = GuiController.Instance.D3dDevice;
            Surface target, bb;

            // seteamos el rendertarget de refraccion            
            bb = d3dDevice.GetRenderTarget(0);
            target = Oceano.surf_refraction.GetSurfaceLevel(0);
            d3dDevice.SetRenderTarget(0, target);

            // limpiamos target & z en el device
            //d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, ParametrosDeConfiguracion.Agua.Color.ToArgb(), 1.0f, 0);
            Matrix store, scale;

            // apachurramos la escena          
            store = d3dDevice.GetTransform(TransformType.World);
            scale = Matrix.Scaling(1.0f, 0.75f, 1.0f);
            d3dDevice.MultiplyTransform(TransformType.World, scale);

            // agregamos unclip-plane
            //Plane cplane = new Plane(0, -1, 0, 1.7f * ParametrosDeConfiguracion.Agua.AlturaSuperficieal);
            // d3dDevice.ClipPlanes[0].Plane = cplane;
            // d3dDevice.ClipPlanes[0].Enabled = true;
            // d3dDevice.SetRenderState(RenderStates.ClipPlaneEnable, true);

            // render del sol unicamente
           // Isla.Render();
            //Sol.Render(EstadoRender.REFRACCION);


            // sacamos el clip plane       
            //d3dDevice.SetRenderState(RenderStates.ClipPlaneEnable, false);
            //d3dDevice.ClipPlanes[0].Enabled = false;

            // restoreamos el render target original
            d3dDevice.RenderState.ZBufferFunction = Compare.LessEqual;
            d3dDevice.SetTransform(TransformType.World, store);
            d3dDevice.SetRenderTarget(0, bb);
        }

        /// <summary>
        /// Método para simular la transformación que el VertexShader le realiza a un punto.
        /// </summary>
        /// <param name="Posicion">Posición del vértice.</param>
        /// <returns>Vértice aproximado al resultado que genera el Shader.</returns>
        public static Vector3 AplicarCPUShader(Vector3 Posicion)
        {
            float X = Posicion.X;
            float Y;
            float Z = Posicion.Z;

            // pseudo coordenadas de textura
            float U = (X / Oceano.DISTANCIA_ENTRE_VERTICES + Oceano.RADIO) / (Oceano.LARGO - 1);
            float V = (Z / Oceano.DISTANCIA_ENTRE_VERTICES + Oceano.RADIO) / (Oceano.LARGO - 1);

            //genero la onda seno como en el shader
            /*float ola = (float)(Math.Sin(U * 2 * 3.14159 * ParametrosDeConfiguracion.Agua.Frecuencia + Oceano.Desplazamiento) *
                                Math.Cos(V * 2 * 3.14159 * ParametrosDeConfiguracion.Agua.Frecuencia + Oceano.Desplazamiento));
            Y = ola * ParametrosDeConfiguracion.Agua.Amplitud;*/

            // interpolacion entre los dos heightmaps como en el shader
            try
            {
                float height1 = Oceano.PerlinNoise1[(int)(U * Oceano.LARGO)][(int)(V * Oceano.LARGO)];
                float height2 = Oceano.PerlinNoise2[(int)(U * Oceano.LARGO)][(int)(V * Oceano.LARGO)];
                 Y = PerlinNoise.Interpolar(height1, height2, Oceano.interpoladorPerlinNoiseHeightmaps.Current) * 5.0f;
                //( PerlinNoise.Interpolar(height1, height2, Oceano.interpoladorPerlinNoiseHeightmaps.Current) - 0.5f)
                //     * ParametrosDeConfiguracion.Agua.AlturaSuperficieal*100.0f;
            }
            catch (Exception e)
            {
                Y = Posicion.Y;                
            }
            return new Vector3(X, Y, Z);
        }

        /// <summary>
        ///  Liberar recursos
        /// </summary>
        public static void Dispose()
        {
            _vertexBuffer.Dispose();
            surf_reflection.Dispose();
            surf_refraction.Dispose();
            surf_fresnel.Dispose();
            PerlinShader.Dispose();
        }
    }
}
