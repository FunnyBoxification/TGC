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
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.Collision.ElipsoidCollision;


namespace AlumnoEjemplos.Quicksort
{
    class Barco
    {
        public int Vida { get; set; }
        public int Danio { get; set; }
        public double VelocidadMovMax { get; set; }
        public double VelocidadRotMax { get; set; }
        public TgcMesh Mesh { get; set; }
        public double Potencia { get; set; }
        public double VelocidadMov { get; set; }
        public double VelocidadRot { get; set; }
        public TgcSceneLoader Loader{get;set;}
        public List<Bala> balas{get;set;}
        public Barco BarcoEnemigo { get; set; }
        public Vector3 PosAntes { get; set; }

        public Barco(int vida, int danio, double velocidad, double rotacion, TgcMesh mesh,double potencia, TgcSceneLoader ldr)
        {
            Vida = vida;
            Danio = danio;
            VelocidadMovMax = velocidad;
            VelocidadRotMax = rotacion;
            Mesh = mesh;
            Potencia = potencia;
            VelocidadMov = 0;
            VelocidadRot = 0;
            Loader = ldr;
            PosAntes = new Vector3(0f,0f,0f);
            balas = new List<Bala>();
        }

        public void dispararBala(int tipobala, int direccion)
        {
            TgcMesh balaMesh;
            if (tipobala == 1)
            {
                TgcScene scene2 = Loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Armas\\Hacha\\Bala-TgcScene.xml");
                balaMesh = scene2.Meshes[0];
            }
            else
            {
                TgcScene scene2 = Loader.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Armas\\Hacha\\Hacha-TgcScene.xml");
                balaMesh = scene2.Meshes[0];
            }
            
            var pos = this.Mesh.Position;
            var rot = this.Mesh.Rotation;
            Vector3 a = new Vector3(0,10,0);
            balaMesh.Position = this.Mesh.Position +a ;
            
            balaMesh.Rotation = this.Mesh.Rotation;
            //dispara a 90 grados
            if (direccion == 0)
            {
                balaMesh.rotateY(Geometry.DegreeToRadian(270));
            }
            else
            {
                balaMesh.rotateY(Geometry.DegreeToRadian(90));
            }

            var bala = new Bala(balaMesh, Danio,BarcoEnemigo, tipobala);
            balas.Add(bala);
        }

        public void rotar(int p)
        {
            if (this.VelocidadRot == 0 || (VelocidadRot < 0.01 && VelocidadRot > -0.01))
            {
                VelocidadRot = 0.02 * p;
            }
            if (VelocidadRotMax > VelocidadRot && p == 1 || p == -1 && -VelocidadRotMax < VelocidadRot)
            {
                VelocidadRot = VelocidadRot + (p) * Math.Abs(VelocidadRot * (Potencia));
            }


        }

        public void acelerar()
        {
            if (this.VelocidadMov == 0 || (VelocidadMov < 0.05 && VelocidadMov > -0.05))
            {
                VelocidadMov = 0.2;
            }

            if (this.VelocidadMovMax > this.VelocidadMov)
            {
                VelocidadMov = VelocidadMov + Math.Abs(VelocidadMov * Potencia);
            }
            //VelocidadMov = VelocidadMov;

        }

        public void frenar()
        {
            for (int i = 0; i < 5; i++)
            {
                desacelerar();
            }
            VelocidadMov = 0;
            
        }

        public void desacelerar()
        {
            if (this.VelocidadMov == 0 || (this.VelocidadMov < 0.01 && this.VelocidadMov > -0.01))
            {
                VelocidadMov = -0.1;
            }
            if (-this.VelocidadMovMax / 3 < this.VelocidadMov)
            {
                VelocidadMov = VelocidadMov - Math.Abs(VelocidadMov * Potencia);
            }
            
        }

        public void colocarAltura(float time)
        {
            float y = Calculo(time,Mesh.Position.X);
            //Vector3 ejeX = new Vector3(1, 0, 0);
            //Vector3 vectplanchado1 = new Vector3(this.Mesh.Position.X, y, this.Mesh.Position.Z);
            //Vector3 vectnDireccion1 = Vector3.Normalize(vectplanchado1 - PosAntes);
            Mesh.move(0, (float)y+0.025f, 0);

            float z = FastMath.Cos(Mesh.Rotation.Y);
            float x = FastMath.Sin(Mesh.Rotation.Y);
            Vector3 vectDireccion1 = moverVector(new Vector3(x, 0, z),Mesh,5);
            Vector3 vectDireccion2 = moverVector(new Vector3(x, 0, z), Mesh, -5);
            

            float ypos = Calculo(time,vectDireccion1.X );
            float yant = Calculo(time, vectDireccion2.X );
            
            if (ypos > yant)
            {
                Mesh.rotateX(0.001f);
            }
            else if (ypos < yant)
            {
                Mesh.rotateX(-0.001f);
            }

            //Vector3 ejeX = new Vector3(1f, 0f, 0f);
            //Mesh.rotateX((float)Math.Acos(Vector3.Dot(vectnDireccion1, ejeX)));
            //PosAntes = Mesh.Position;

        }

        public Vector3 moverVector(Vector3 vect,TgcMesh  mesh, float mov)
        {
            float z = FastMath.Cos(mesh.Rotation.Y) * mov;
            float x = FastMath.Sin(mesh.Rotation.Y) * mov;

            vect.X += x;
            vect.Z += z;
            return vect;
            
        }


        public void volverAltura(float time)
        {
            float y = Calculo( time,Mesh.Position.X);

            Mesh.move(0, -(float)y, 0);
        }

        public void hundir(float elapsedtime)
        {
            Mesh.rotateX(0.1f * elapsedtime);
            Mesh.move(0, -10 * elapsedtime, 0);

        }

        private float Calculo(float time, float x1)
        {
            //pruebo altura de ola
            float A = 53f;
            /*double L = 50;	// wavelength
            double w = 5f * 3.1416f / L;
            double Q = 0.5f;*/
            float x = x1 /50;
            
            float y = Mesh.Position.Y;
            //float3 D = float3(1,1,0);
            // float dotD = dot(P0.xy, D);

            double C = Math.Cos(0.091*x -  time+0);
            double S = Math.Sin(0.091*x -  time+ 0);

            //float y;
            y =  A * ((float)C + (float)S);

            //bool b = Mapa.oceano_mesh.HeightmapData
           /* TgcRay rayoArriba = new TgcRay(this.Mesh.Position, new Vector3(0, 1, 0));
            TgcRay rayoAbajo = new TgcRay(this.Mesh.Position, new Vector3(0, -1, 0));
            Vector3 colisionPoint;
            if (Mapa.oceano_mesh.intersectRay(rayoArriba, out colisionPoint) || Mapa.oceano_mesh.intersectRay(rayoAbajo, out colisionPoint))
            {
                y = colisionPoint.Y;
            }*/

            return y;
        }

        public float boostOla()
        {
            float z = FastMath.Cos(Mesh.Rotation.Y) ;
            float x = FastMath.Sin(Mesh.Rotation.Y) ;
            Vector3 vectDireccion = new Vector3(x, 0, z);
            if (!(Vector3.Dot(Mesh.Rotation, vectDireccion) == 1))
            {
                if (Vector3.Cross(Mesh.Rotation, vectDireccion).Y < 0)//el barco esta a la izq
                {
                    return 1.15f; //dispara der
                }
                if (Vector3.Cross(Mesh.Rotation, vectDireccion).Y > 0) //el barco esta a la derecha o atras
                {
                    return 0.85f; //dispara izq
                }
                 
            }
            return 1;
        }



        public bool dañado { get; set; }
    }
}
