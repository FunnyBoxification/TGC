﻿using System;
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
//using Examples.TerrainEditor;
using TgcViewer.Utils.Interpolation;
using TgcViewer.Utils;
using TgcViewer.Utils.Shaders;
using TgcViewer.Utils._2D;

using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.Quicksort 
{
    public class EjemploAlumno : TgcExample
    {

        const float VELOCIDAD_MOVIMIENTO = 125f;
        const float VELOCIDAD_ROTACION = 20f;
        const float ACELERACION = 2f;

        bool pausaActiva = false;
        bool comenzoJuego = false;

        SmartTerrain oceano;

        BarcoPlayer barcoPrincipal;
       
        BarcoBot barcoEnemigo;

        CubeTexture g_pCubeMapAgua = null;

        static TgcScene escena;
        TgcMesh mainMesh, agua, meshBot; 
        TgcSkyBox skyBox;

        float near_plane = 1f;
        float far_plane = 10000f;
        
        Microsoft.DirectX.Direct3D.Effect efectosAguaIluminacion;
        float time;
        float timemenu;
        Texture textura;
        Texture diffuseMapTexture;

        bool activar_efecto = false;


        Vector3 g_LightPos;						// posicion de la luz actual (la que estoy analizando)
        Vector3 g_LightDir;						// direccion de la luz actual
        Matrix g_LightView;						// matriz de view del light
        float alfa_sol;             // pos. del sol
        TgcBox sol;

       //inicio
        TgcSprite spriteFondo;
        TgcSprite spriteLetras;
        TgcSprite spriteInicio;
        
        //inicio
        //lluvia
        VertexBuffer screenQuadVB;
        Texture renderTarget2D;
        Surface pOldRT;
        Microsoft.DirectX.Direct3D.Effect effectlluvia;
        TgcTexture alarmTexture;
        InterpoladorVaiven intVaivenAlarm;

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
            
            //Activamos el renderizado customizado. De esta forma el framework nos delega control total sobre como dibujar en pantalla
            //La responsabilidad cae toda de nuestro lado
            GuiController.Instance.CustomRenderEnabled = true;
            g_pCubeMapAgua = TextureLoader.FromCubeFile(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Shaders\\CubeMap.dds");
            sol = TgcBox.fromSize(new Vector3(50, 50, 50), Color.LightYellow);

            

            //Cargo la escena completa que tendria que ser la del escenario con el cielo / la del agua
            //PROXIMAMENTE, ahora cargo otro escenario

            //Pruebo postprocess lluvia
            CustomVertex.PositionTextured[] screenQuadVertices = new CustomVertex.PositionTextured[]
		    {
    			new CustomVertex.PositionTextured( -1, 1, 1, 0,0), 
			    new CustomVertex.PositionTextured(1,  1, 1, 1,0),
			    new CustomVertex.PositionTextured(-1, -1, 1, 0,1),
			    new CustomVertex.PositionTextured(1,-1, 1, 1,1)
    		};
            //vertex buffer de los triangulos
            screenQuadVB = new VertexBuffer(typeof(CustomVertex.PositionTextured),
                    4, d3dDevice, Usage.Dynamic | Usage.WriteOnly,
                        CustomVertex.PositionTextured.Format, Pool.Default);
            screenQuadVB.SetData(screenQuadVertices, 0, LockFlags.None);

            //Creamos un Render Targer sobre el cual se va a dibujar la pantalla
            renderTarget2D = new Texture(d3dDevice, d3dDevice.PresentationParameters.BackBufferWidth
                    , d3dDevice.PresentationParameters.BackBufferHeight, 1, Usage.RenderTarget,
                        Format.X8R8G8B8, Pool.Default);


            //Cargar shader con efectos de Post-Procesado
            effectlluvia = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "PostProcess.fx");

            //Configurar Technique dentro del shader
            effectlluvia.Technique = "AlarmaTechnique";

            //Cargar textura que se va a dibujar arriba de la escena del Render Target
            alarmTexture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "rain.png");

            //Interpolador para efecto de variar la intensidad de la textura de alarma
            intVaivenAlarm = new InterpoladorVaiven();
            intVaivenAlarm.Min = 0;
            intVaivenAlarm.Max = 2;
            intVaivenAlarm.Speed = 10;
            intVaivenAlarm.reset();

            //Modifier para activar/desactivar efecto de alarma
            GuiController.Instance.Modifiers.addBoolean("activar_efecto", "Activar efecto", true);

            //termina post process

            //inicio//
            spriteFondo = new TgcSprite();
            spriteFondo.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "MenuPrincipal.jpg");
            
            Size screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = spriteFondo.Texture.Size;
            spriteFondo.Scaling = new Vector2(1f, 0.8f);
            spriteFondo.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - textureSize.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - textureSize.Height / 2, 0));
            
            spriteLetras = new TgcSprite();
            spriteLetras.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "Texto.png");

            spriteInicio = new TgcSprite();
            spriteInicio.Texture = TgcTexture.createTexture(GuiController.Instance.AlumnoEjemplosMediaDir + "inicio.png");

            spriteLetras.Scaling = new Vector2(0.4f*1.65f, 0.3f*1.65f);
            Size textureSize2 = spriteLetras.Texture.Size;

            spriteInicio.Scaling = new Vector2(0.4f, 0.3f);
            Size textureSize3 = spriteInicio.Texture.Size;
            
            spriteLetras.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - textureSize2.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - textureSize2.Height / 2, 0));
            spriteLetras.Position = new Vector2(spriteLetras.Position.X + 110, spriteLetras.Position.Y);

            spriteInicio.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - textureSize3.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - textureSize3.Height / 2, 0));
            spriteInicio.Position = new Vector2(spriteInicio.Position.X + 210, spriteLetras.Position.Y+95);
           

            //inicio//

            Bitmap b = (Bitmap)Bitmap.FromFile(GuiController.Instance.AlumnoEjemplosMediaDir +  "agua.bmp");
            b.RotateFlip(RotateFlipType.Rotate90FlipX);
            textura = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);

            b = (Bitmap)Bitmap.FromFile(GuiController.Instance.ExamplesMediaDir
                    + "Shaders\\BumpMapping_DiffuseMap.jpg");
            diffuseMapTexture = Texture.FromBitmap(d3dDevice, b, Usage.None, Pool.Managed);

            oceano = new SmartTerrain();
            //oceano.loadHeightmap(GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "TerrainTexture1-256x256.jpg", 30.00f, 1.0f, new Vector3(0, 0, 0));
            oceano.loadPlainHeightmap(256, 256, 0, 50.0f, 1.0f, new Vector3(0, 0, 0));
            oceano.loadTexture(GuiController.Instance.ExamplesMediaDir + "Heighmaps\\" + "TerrainTexture1-256x256.jpg");
            
            TgcSceneLoader loader = new TgcSceneLoader();
            //escena = loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Scenes\\Isla\\Isla-TgcScene.xml");
            
            //Textura del skybox
            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox LostAtSeaDay\\";
            
            //Crear SkyBox 
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 0, 0);
            skyBox.Size = new Vector3(10000, 10000, 10000);

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

            //TgcScene scene3 = loader.loadSceneFromFile(GuiController.Instance.ExamplesDir + "Shaders\\WorkshopShaders\\Media\\Piso\\Agua-TgcScene.xml");
            //agua = scene3.Meshes[0];
            //agua.Scale = new Vector3(25f, 1f, 25f);
            //agua.Position = new Vector3(0f, 0f, 0f);

            efectosAguaIluminacion = TgcShaders.loadEffect(GuiController.Instance.AlumnoEjemplosMediaDir + "shader_agua.fx");
            oceano.Effect = efectosAguaIluminacion;
            oceano.Technique = "RenderAgua";//"EnvironmentMapTechnique"; //"RenderAgua";

            

            barcoPrincipal = new BarcoPlayer(100, 50, VELOCIDAD_MOVIMIENTO, ACELERACION, VELOCIDAD_ROTACION, mainMesh,0.05,loader);
            barcoEnemigo = new BarcoBot(100, 35,100, ACELERACION, 18, meshBot, 0.05,barcoPrincipal,loader);
            barcoPrincipal.BarcoEnemigo = barcoEnemigo;

            // iluminacion en los barcos
            barcoPrincipal.Mesh.Effect =GuiController.Instance.Shaders.TgcMeshPointLightShader;// efectosAguaIluminacion;
            barcoPrincipal.Mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(barcoPrincipal.Mesh.RenderType); //"EnvironmentMapTechnique";

            barcoEnemigo.Mesh.Effect = GuiController.Instance.Shaders.TgcMeshPointLightShader; //efectosAguaIluminacion;
            barcoEnemigo.Mesh.Technique = GuiController.Instance.Shaders.getTgcMeshTechnique(barcoEnemigo.Mesh.RenderType);//"EnvironmentMapTechnique";

            //Camara en tercera persona focuseada en el barco (canoa) 

            /*GuiController.Instance.ThirdPersonCamera.Enable = true;
            GuiController.Instance.ThirdPersonCamera.setCamera(barcoPrincipal.Mesh.Position, 200, 300);
            GuiController.Instance.ThirdPersonCamera.rotateY(Geometry.DegreeToRadian(180));
            GuiController.Instance.RotCamera.Enable = false;*/


            //PARA DESARROLLO DEL ESCENARIO ES MEJOR MOVERSE CON ESTA CAMARA
            GuiController.Instance.FpsCamera.Enable = true;

            //GuiController.Instance.Modifiers.addFloat("reflection", 0, 1, 0.35f);
            GuiController.Instance.Modifiers.addVertex3f("lightPos", new Vector3(-200, 0, -200), new Vector3(200, 1000, 200), new Vector3(0, 900, 0));
            GuiController.Instance.Modifiers.addColor("lightColor", Color.LightYellow);
            //GuiController.Instance.Modifiers.addFloat("bumpiness", 0, 1, 1f);
            GuiController.Instance.Modifiers.addFloat("lightIntensity", 0, 150, 100);
            GuiController.Instance.Modifiers.addFloat("lightAttenuation", 0.1f, 2, 0.1f);
            GuiController.Instance.Modifiers.addFloat("specularEx", 0, 20, 9f);
            

            GuiController.Instance.Modifiers.addColor("mEmissive", Color.Black);
            GuiController.Instance.Modifiers.addColor("mAmbient", Color.White);
            GuiController.Instance.Modifiers.addColor("mDiffuse", Color.White);
            GuiController.Instance.Modifiers.addColor("mSpecular", Color.White);

            //Carpeta de archivos Media del alumno
            //string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            Mapa.oceano_mesh = oceano;


        }

        public override void render(float elapsedTime)
        {

            //Device de DirectX para renderizar
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            if (pausaActiva || !comenzoJuego)
            {
                timemenu += elapsedTime;
                //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
                GuiController.Instance.Drawer2D.beginDrawSprite();

                //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
                spriteFondo.render();

                spriteLetras.render();

                if (Convert.ToInt64(timemenu) % 2 != 0 && !comenzoJuego)
                {
                    spriteInicio.render();
                }
                

                //Finalizar el dibujado de Sprites
                GuiController.Instance.Drawer2D.endDrawSprite();

                if (d3dInput.keyDown(Key.Space))
                {
                    if(!comenzoJuego)
                    {
                       comenzoJuego = true;
                    }
                }

                if (d3dInput.keyDown(Key.P))
                {
                        pausaActiva = false;
                 }
            }
            else
            {
                if (d3dInput.keyDown(Key.P))
                {
                     pausaActiva = true;
                }

                if (d3dInput.keyDown(Key.L))
                {
                    if (activar_efecto)
                    {
                        activar_efecto = false;
                    }else
                    {
                        activar_efecto = true;
                    }
                }
                
                time += elapsedTime;

                //Cargamos el Render Targer al cual se va a dibujar la escena 3D. Antes nos guardamos el surface original
                //En vez de dibujar a la pantalla, dibujamos a un buffer auxiliar, nuestro Render Target.
                pOldRT = d3dDevice.GetRenderTarget(0);
                Surface pSurf = renderTarget2D.GetSurfaceLevel(0);
                d3dDevice.SetRenderTarget(0, pSurf);
                d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);


                //Dibujamos la escena comun, pero en vez de a la pantalla al Render Target
                drawSceneToRenderTarget(d3dDevice, elapsedTime);

                //Liberar memoria de surface de Render Target
                pSurf.Dispose();

                //Si quisieramos ver que se dibujo, podemos guardar el resultado a una textura en un archivo para debugear su resultado (ojo, es lento)
                //TextureLoader.Save(GuiController.Instance.ExamplesMediaDir + "Shaders\\render_target.bmp", ImageFileFormat.Bmp, renderTarget2D);


                //Ahora volvemos a restaurar el Render Target original (osea dibujar a la pantalla)
                d3dDevice.SetRenderTarget(0, pOldRT);


                //Luego tomamos lo dibujado antes y lo combinamos con una textura con efecto de alarma
                drawPostProcess(d3dDevice);

            }
        }

        private void checkearVidas(Barco barco)
        {
            if (barco.Vida < 0)
            {
                barco.Mesh.setColor(Color.Red);
            }
        }

        private void drawSceneToRenderTarget(Microsoft.DirectX.Direct3D.Device d3dDevice, float elapsedTime)
        {
            //Arrancamos el renderizado. Esto lo tenemos que hacer nosotros a mano porque estamos en modo CustomRenderEnabled = true
            d3dDevice.BeginScene();


            //Como estamos en modo CustomRenderEnabled, tenemos que dibujar todo nosotros, incluso el contador de FPS
            GuiController.Instance.Text3d.drawText("FPS: " + HighResolutionTimer.Instance.FramesPerSecond, 0, 0, Color.Yellow);

            //Tambien hay que dibujar el indicador de los ejes cartesianos
            GuiController.Instance.AxisLines.render();

            //Viejo render aqui ///
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

            //Actualzar posición de la luz
            Vector3 lightPos = (Vector3)GuiController.Instance.Modifiers["lightPos"];
            sol.Position = lightPos;
            Vector3 eyePosition = GuiController.Instance.FpsCamera.getPosition();

            if (g_pCubeMapAgua != null)
            {
                //CrearEnvMapAgua();
                efectosAguaIluminacion.SetValue("g_txCubeMapAgua", g_pCubeMapAgua);
                efectosAguaIluminacion.SetValue("texCubeMap", g_pCubeMapAgua);
            }


            efectosAguaIluminacion.SetValue("g_vLightPos", new Vector4(g_LightPos.X, g_LightPos.Y, g_LightPos.Z, 1));
            efectosAguaIluminacion.SetValue("g_vLightDir", new Vector4(g_LightDir.X, g_LightDir.Y, g_LightDir.Z, 1));
            g_LightView = Matrix.LookAtLH(g_LightPos, g_LightPos + g_LightDir, new Vector3(0, 0, 1));
            efectosAguaIluminacion.SetValue("g_mViewLightProj", g_LightView);
            efectosAguaIluminacion.SetValue("time", time);
            efectosAguaIluminacion.SetValue("aux_Tex", textura);
            efectosAguaIluminacion.SetValue("texDiffuseMap", diffuseMapTexture);

            efectosAguaIluminacion.SetValue("lightColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["lightColor"]));
            efectosAguaIluminacion.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
            efectosAguaIluminacion.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(eyePosition));
            efectosAguaIluminacion.SetValue("lightIntensity", (float)GuiController.Instance.Modifiers["lightIntensity"]);
            //barcoPrincipal.Mesh.Effect.SetValue("bumpiness", (float)GuiController.Instance.Modifiers["bumpiness"]);
            efectosAguaIluminacion.SetValue("lightAttenuation", (float)GuiController.Instance.Modifiers["lightAttenuation"]);
            //barcoPrincipal.Mesh.Effect.SetValue("reflection", (float)GuiController.Instance.Modifiers["reflection"]);
            efectosAguaIluminacion.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.FpsCamera.getPosition()));
            efectosAguaIluminacion.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));

            efectosAguaIluminacion.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
            efectosAguaIluminacion.SetValue("materialAmbientColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mAmbient"]));
            efectosAguaIluminacion.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
            efectosAguaIluminacion.SetValue("materialSpecularColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mSpecular"]));
            efectosAguaIluminacion.SetValue("materialSpecularExp", (float)GuiController.Instance.Modifiers["specularEx"]);

            //Hacer que la camara siga al personaje en su nueva posicion
            // GuiController.Instance.ThirdPersonCamera.rotateY(Geometry.DegreeToRadian(180));
            GuiController.Instance.ThirdPersonCamera.Target = barcoPrincipal.Mesh.Position;

            oceano.render();
            //Dibujar objeto principal
            //Siempre primero hacer todos los cálculos de lógica e input y luego al final dibujar todo (ciclo update-render)
            barcoPrincipal.colocarAltura(time);
            barcoEnemigo.colocarAltura(time);
            if (barcoPrincipal.Vida > 0){
            barcoPrincipal.Movimiento(elapsedTime);
            }
             if (barcoEnemigo.Vida > 0){
            barcoEnemigo.Movimiento(elapsedTime);
             }
            

            barcoPrincipal.Mesh.Effect.SetValue("lightColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["lightColor"]));
            barcoPrincipal.Mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
            barcoPrincipal.Mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(eyePosition));
            barcoPrincipal.Mesh.Effect.SetValue("lightIntensity", (float)GuiController.Instance.Modifiers["lightIntensity"]);
            //barcoPrincipal.Mesh.Effect.SetValue("bumpiness", (float)GuiController.Instance.Modifiers["bumpiness"]);
            barcoPrincipal.Mesh.Effect.SetValue("lightAttenuation", (float)GuiController.Instance.Modifiers["lightAttenuation"]);
            //barcoPrincipal.Mesh.Effect.SetValue("reflection", (float)GuiController.Instance.Modifiers["reflection"]);
            barcoPrincipal.Mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.FpsCamera.getPosition()));
            barcoPrincipal.Mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));

            barcoPrincipal.Mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
            barcoPrincipal.Mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mAmbient"]));
            barcoPrincipal.Mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
            barcoPrincipal.Mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mSpecular"]));
            barcoPrincipal.Mesh.Effect.SetValue("materialSpecularExp", (float)GuiController.Instance.Modifiers["specularEx"]);
            barcoPrincipal.Mesh.render();

            barcoEnemigo.Mesh.Effect.SetValue("lightColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["lightColor"]));
            barcoEnemigo.Mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));
            //barcoEnemigo.Mesh.Effect.SetValue("bumpiness", (float)GuiController.Instance.Modifiers["bumpiness"]);
            barcoEnemigo.Mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(eyePosition));
            barcoEnemigo.Mesh.Effect.SetValue("lightIntensity", (float)GuiController.Instance.Modifiers["lightIntensity"]);
            barcoEnemigo.Mesh.Effect.SetValue("lightAttenuation", (float)GuiController.Instance.Modifiers["lightAttenuation"]);
            //barcoEnemigo.Mesh.Effect.SetValue("reflection", (float)GuiController.Instance.Modifiers["reflection"]);
            barcoEnemigo.Mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(GuiController.Instance.FpsCamera.getPosition()));
            barcoEnemigo.Mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(lightPos));

            barcoEnemigo.Mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mEmissive"]));
            barcoEnemigo.Mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mAmbient"]));
            barcoEnemigo.Mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mDiffuse"]));
            barcoEnemigo.Mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor((Color)GuiController.Instance.Modifiers["mSpecular"]));
            barcoEnemigo.Mesh.Effect.SetValue("materialSpecularExp", (float)GuiController.Instance.Modifiers["specularEx"]);

            barcoEnemigo.Mesh.render();

            barcoPrincipal.volverAltura(time);
            barcoEnemigo.volverAltura(time);

            foreach (var bala in barcoPrincipal.balas)
            {
                if (bala.Mesh.Position.Y > 0)
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
            //escena.renderAll();
            /*Blend ant_src = d3dDevice.RenderState.SourceBlend;
            Blend ant_dest = d3dDevice.RenderState.DestinationBlend;
            bool ant_alpha = d3dDevice.RenderState.AlphaBlendEnable;
            d3dDevice.RenderState.AlphaBlendEnable = true;
            d3dDevice.RenderState.SourceBlend = Blend.SourceColor;
            d3dDevice.RenderState.DestinationBlend = Blend.InvSourceColor;
            //agua.render();
            d3dDevice.RenderState.SourceBlend = ant_src;
            d3dDevice.RenderState.DestinationBlend = ant_dest;
            d3dDevice.RenderState.AlphaBlendEnable = ant_alpha;*/
           // agua.render();
            
            //oceano.render();
            sol.render();
            skyBox.render();



            //Terminamos manualmente el renderizado de esta escena. Esto manda todo a dibujar al GPU al Render Target que cargamos antes
            d3dDevice.EndScene();
        }

        private void drawPostProcess(Microsoft.DirectX.Direct3D.Device d3dDevice)
        {
            //Arrancamos la escena
            d3dDevice.BeginScene();

            //Cargamos para renderizar el unico modelo que tenemos, un Quad que ocupa toda la pantalla, con la textura de todo lo dibujado antes
            d3dDevice.VertexFormat = CustomVertex.PositionTextured.Format;
            d3dDevice.SetStreamSource(0, screenQuadVB, 0);

            //Ver si el efecto de alarma esta activado, configurar Technique del shader segun corresponda
            
            if (activar_efecto)
            {
                effectlluvia.Technique = "AlarmaTechnique";
            }
            else
            {
                effectlluvia.Technique = "DefaultTechnique";
            }

            //Cargamos parametros en el shader de Post-Procesado
            effectlluvia.SetValue("render_target2D", renderTarget2D);
            effectlluvia.SetValue("textura_alarma", alarmTexture.D3dTexture);
            effectlluvia.SetValue("alarmaScaleFactor", intVaivenAlarm.update());

            //Limiamos la pantalla y ejecutamos el render del shader
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            effectlluvia.Begin(FX.None);
            effectlluvia.BeginPass(0);
            d3dDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            effectlluvia.EndPass();
            effectlluvia.End();

            //Terminamos el renderizado de la escena
            d3dDevice.EndScene();
        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            //escena.disposeAll();
            mainMesh.dispose();
            meshBot.dispose();
        }

       /* public void CrearEnvMapAgua()
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

                Vector3 Pos = new Vector3(0, 0, 0);//agua.Position;
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
            //}
            // restuaro el render target
           // device.SetRenderTarget(0, pOldRT);
        //}*/



        
    }
}
