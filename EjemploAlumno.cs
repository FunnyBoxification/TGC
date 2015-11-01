using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Input;
using Microsoft.DirectX.DirectInput;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.Shaders;

namespace AlumnoEjemplos.Quicksort 
{
    public class EjemploAlumno : TgcExample
    {

        const float VELOCIDAD_MOVIMIENTO = 125f;
        const float VELOCIDAD_ROTACION = 20f;
        const float ACELERACION = 2f;

        BarcoPlayer barcoPrincipal;
       
        BarcoBot barcoEnemigo;

        CubeTexture g_pCubeMapAgua = null;

        static TgcScene escena;
        TgcMesh mainMesh, agua, meshBot; 
        TgcSkyBox skyBox;

        float near_plane = 1f;
        float far_plane = 10000f;
        
        Microsoft.DirectX.Direct3D.Effect efectoAgua;
        float time;
        Texture textura;
        Texture diffuseMapTexture;

        Vector3 g_LightPos;						// posicion de la luz actual (la que estoy analizando)
        Vector3 g_LightDir;						// direccion de la luz actual
        Matrix g_LightView;						// matriz de view del light
        float alfa_sol;             // pos. del sol

        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        public override string getName()
        {
            return "Grupo QuickSort";
        }

        public static List<TgcMesh> getEscenaMeshes()
        {
            return escena.Meshes;
        }

        public override string getDescription()
        {
            return "MiIdea - Descripcion de la idea";
        }

        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;

            //Cargo la escena completa que tendria que ser la del escenario con el cielo / la del agua
            //PROXIMAMENTE, ahora cargo otro escenario

            Bitmap b = (Bitmap)Bitmap.FromFile(GuiController.Instance.ExamplesDir
                    + "Shaders\\WorkshopShaders\\Media\\Heighmaps\\" + "TerrainTexture3.jpg");
            b.RotateFlip(RotateFlipType.Rotate90FlipX);
            textura = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);

            b = (Bitmap)Bitmap.FromFile(GuiController.Instance.ExamplesMediaDir
                    + "Shaders\\BumpMapping_DiffuseMap.jpg");
            diffuseMapTexture = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);
            
            
            
            TgcSceneLoader loader = new TgcSceneLoader();
            escena = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Isla\\Isla-TgcScene.xml");
            
            //Textura del skybox
            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";
            
            //Crear SkyBox 
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(3000, 1000, 3000);

            //Configurar color
            //skyBox.Color = Color.OrangeRed;

            //Configurar las texturas para cada una de las 6 caras
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "lostatseaday_up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "lostatseaday_dn.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "lostatseaday_lf.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "lostatseaday_rt.jpg");

            //Hay veces es necesario invertir las texturas Front y Back si se pasa de un sistema RightHanded a uno LeftHanded
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "lostatseaday_bk.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "lostatseaday_ft.jpg");



            //Actualizar todos los valores para crear el SkyBox
            skyBox.updateValues();

            //Cargo el mesh del/los barco/s -> porque se carga como escena y no cargo el mesh directamente?

            TgcScene scene2 = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Boteconcañon\\BoteConCanion-TgcScene.xml");
            mainMesh = scene2.Meshes[0];
            mainMesh.Position = new Vector3(400f,0f, 400f);
            TgcScene scene4 = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Boteconcañon\\BoteConCanion-TgcScene.xml");
            meshBot = scene4.Meshes[0];
            meshBot.Position = new Vector3(-400f,0f,400f);

            TgcScene scene3 = loader.loadSceneFromFile(GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\Piso\\Agua-TgcScene.xml");
            agua = scene3.Meshes[0];
            agua.Scale = new Vector3(25f, 1f, 25f);
            agua.Position = new Vector3(0f, 0f, 0f);

            efectoAgua = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "shader_agua.fx");
            agua.Effect = efectoAgua;
            agua.Technique = "RenderAgua";

            

            barcoPrincipal = new BarcoPlayer(100, 20, VELOCIDAD_MOVIMIENTO, ACELERACION, VELOCIDAD_ROTACION, mainMesh,0.05,loader);
            barcoEnemigo = new BarcoBot(100, 25,100, ACELERACION, 18, meshBot, 0.05,barcoPrincipal,loader);
            barcoPrincipal.BarcoEnemigo = barcoEnemigo;
            //Camara en tercera persona focuseada en el barco (canoa) 
            GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(barcoPrincipal.Mesh.Position, 200, 300);
            GuiController.Instance.ThirdPersonCamera.rotateY(Geometry.DegreeToRadian(180));
            GuiController.Instance.RotCamera.Enable = false;


            //PARA DESARROLLO DEL ESCENARIO ES MEJOR MOVERSE CON ESTA CAMARA
            //GuiController.Instance.FpsCamera.Enable = true;

            //Carpeta de archivos Media del alumno
            //string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;


        }

        public override void render(float elapsedTime)
        {
            //Device de DirectX para renderizar
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;
            time += elapsedTime;

            /*
            //Obtener valor de UserVar (hay que castear)
            int valor = (int)GuiController.Instance.UserVars.getValue("variablePrueba");
            

            //Obtener valores de Modifiers
            float valorFloat = (float)GuiController.Instance.Modifiers["valorFloat"];
            string opcionElegida = (string)GuiController.Instance.Modifiers["valorIntervalo"];
            Vector3 valorVertice = (Vector3)GuiController.Instance.Modifiers["valorVertice"];
            */
            alfa_sol = 1.5f;
            g_LightPos = new Vector3(2000f * (float)Math.Cos(alfa_sol), 2000f * (float)Math.Sin(alfa_sol), 0f);
            g_LightDir = -g_LightPos;
            g_LightDir.Normalize();

            barcoPrincipal.Movimiento(elapsedTime);
            barcoEnemigo.Movimiento(elapsedTime);

            if (g_pCubeMapAgua == null)
            {
                CrearEnvMapAgua();
                efectoAgua.SetValue("g_txCubeMapAgua", g_pCubeMapAgua);
            }


            efectoAgua.SetValue("g_vLightPos", new Vector4(g_LightPos.X, g_LightPos.Y, g_LightPos.Z, 1));
            efectoAgua.SetValue("g_vLightDir", new Vector4(g_LightDir.X, g_LightDir.Y, g_LightDir.Z, 1));
            g_LightView = Matrix.LookAtLH(g_LightPos, g_LightPos + g_LightDir, new Vector3(0, 0, 1));
            efectoAgua.SetValue("g_mViewLightProj", g_LightView);
            efectoAgua.SetValue("time", time);
            efectoAgua.SetValue("aux_Tex", textura);
            efectoAgua.SetValue("texDiffuseMap",diffuseMapTexture);

            //Hacer que la camara siga al personaje en su nueva posicion
           // GuiController.Instance.ThirdPersonCamera.rotateY(Geometry.DegreeToRadian(180));
            GuiController.Instance.ThirdPersonCamera.Target = barcoPrincipal.Mesh.Position;
            

            //Dibujar objeto principal
            //Siempre primero hacer todos los cálculos de lógica e input y luego al final dibujar todo (ciclo update-render)
            barcoPrincipal.Mesh.render();
            barcoEnemigo.Mesh.render();
            foreach (var bala in barcoPrincipal.balas)
            {
                if (bala.Mesh.Position.Y >0)
                {
                    bala.Mover(elapsedTime);
                    bala.Mesh.render();

                }
            }

               
            foreach (var bala in barcoEnemigo.balas)
                {
                    if (bala.Mesh.Position.Y > 0)
                    {
                        bala.Mover(elapsedTime);
                        bala.Mesh.render();
                    }
                }

            checkearVidas(barcoEnemigo);
            checkearVidas(barcoPrincipal);
                //Dibujamos la escena
                escena.renderAll();
                Blend ant_src = d3dDevice.RenderState.SourceBlend;
                Blend ant_dest = d3dDevice.RenderState.DestinationBlend;
                bool ant_alpha = d3dDevice.RenderState.AlphaBlendEnable;
                d3dDevice.RenderState.AlphaBlendEnable = true;
                d3dDevice.RenderState.SourceBlend = Blend.SourceColor;
                d3dDevice.RenderState.DestinationBlend = Blend.InvSourceColor;
                agua.render();
                d3dDevice.RenderState.SourceBlend = ant_src;
                d3dDevice.RenderState.DestinationBlend = ant_dest;
                d3dDevice.RenderState.AlphaBlendEnable = ant_alpha;
                //agua.render();

                skyBox.render();

            

        }

        private void checkearVidas(Barco barco)
        {
            if (barco.Vida < 0)
            {
                barco.Mesh.setColor(Color.Red);
            }
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            escena.disposeAll();
            mainMesh.dispose();
            meshBot.dispose();
        }

        public void CrearEnvMapAgua()
        {
            // creo el enviroment map para el agua
            Microsoft.DirectX.Direct3D.Device device = GuiController.Instance.D3dDevice;
            g_pCubeMapAgua = new CubeTexture(device, 256, 1, Usage.RenderTarget,
                Format.A16B16G16R16F, Pool.Default);
            Surface pOldRT = device.GetRenderTarget(0);
            // ojo: es fundamental que el fov sea de 90 grados.
            // asi que re-genero la matriz de proyeccion
            device.Transform.Projection =
                Matrix.PerspectiveFovLH(Geometry.DegreeToRadian(90.0f),
                    1f, near_plane, far_plane);
            // Genero las caras del enviroment map
            for (CubeMapFace nFace = CubeMapFace.PositiveX; nFace <= CubeMapFace.NegativeZ; ++nFace)
            {
                Surface pFace = g_pCubeMapAgua.GetCubeMapSurface(nFace, 0);
                device.SetRenderTarget(0, pFace);
                Vector3 Dir, VUP;
                Color color;
                switch (nFace)
                {
                    default:
                    case CubeMapFace.PositiveX:
                        // Left
                        Dir = new Vector3(1, 0, 0);
                        VUP = new Vector3(0, 1, 0);
                        color = Color.Black;
                        break;
                    case CubeMapFace.NegativeX:
                        // Right
                        Dir = new Vector3(-1, 0, 0);
                        VUP = new Vector3(0, 1, 0);
                        color = Color.Red;
                        break;
                    case CubeMapFace.PositiveY:
                        // Up
                        Dir = new Vector3(0, 1, 0);
                        VUP = new Vector3(0, 0, -1);
                        color = Color.Gray;
                        break;
                    case CubeMapFace.NegativeY:
                        // Down
                        Dir = new Vector3(0, -1, 0);
                        VUP = new Vector3(0, 0, 1);
                        color = Color.Yellow;
                        break;
                    case CubeMapFace.PositiveZ:
                        // Front
                        Dir = new Vector3(0, 0, 1);
                        VUP = new Vector3(0, 1, 0);
                        color = Color.Green;
                        break;
                    case CubeMapFace.NegativeZ:
                        // Back
                        Dir = new Vector3(0, 0, -1);
                        VUP = new Vector3(0, 1, 0);
                        color = Color.Blue;
                        break;
                }

                Vector3 Pos = agua.Position;
                if (nFace == CubeMapFace.NegativeY)
                    Pos.Y += 2000;

                device.Transform.View = Matrix.LookAtLH(Pos, Pos + Dir, VUP);
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, color, 1.0f, 0);
                //device.BeginScene();
                //Renderizar: solo algunas cosas:
                /*if (nFace == CubeMapFace.NegativeY)
                {
                    //Renderizar terreno
                    terrain.render();
                }
                else
                {
                    //Renderizar SkyBox
                    skyBox.render();
                    // dibujo el bosque
                    foreach (TgcMesh instance in bosque)
                        instance.render();
                }*/
                //string fname = string.Format("face{0:D}.bmp", nFace);
                //SurfaceLoader.Save(fname, ImageFileFormat.Bmp, pFace);

                //device.EndScene();
            }
            // restuaro el render target
            device.SetRenderTarget(0, pOldRT);
        }


    }
}
