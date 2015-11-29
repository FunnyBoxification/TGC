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
using System.Timers;
using System.Diagnostics;


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
        public List<Barco> BarcosEnemigos { get; set; }
        public Vector3 PosAntes { get; set; }
        public TgcMesh balamesh { get; set; }
        public Stopwatch timerDaniado { get; set; }

        public Barco(int vida, int danio, double velocidad, double rotacion, TgcMesh mesh,double potencia, TgcSceneLoader ldr, TgcMesh bala)
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
            BarcosEnemigos = new List<Barco>();
            balamesh = bala;
            daniado = false;
        }

        public void dispararBala(int tipobala, int direccion)
        {
            
            TgcMesh instance = balamesh.createMeshInstance(balamesh.Name + "_");
            if (tipobala == 1)
            {
                
                instance.Scale = new Vector3(3f, 3f, 3f);

            }
            else
            {
                
                instance.Scale = new Vector3(2f, 2f, 2f);
            }
            
            var pos = this.Mesh.Position;
            var rot = this.Mesh.Rotation;
            Vector3 a = new Vector3(0,10,0);
            instance.Position = this.Mesh.Position +a ;
            
            instance.Rotation = this.Mesh.Rotation;
            //dispara a 90 grados
            if (direccion == 0)
            {
                instance.rotateY(Geometry.DegreeToRadian(270));
            }
            else
            {
                instance.rotateY(Geometry.DegreeToRadian(90));
            }

            var enemigos = new List<Barco>();
            if (tipobala == 1)
            {
                
                var bala = new Bala(instance, Danio, BarcosEnemigos, tipobala);
                balas.Add(bala);
            }
            else
            {
                
                var bala = new Bala(instance, Danio, BarcosEnemigos, tipobala);
                balas.Add(bala);
            }
            
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
                VelocidadMov = 0.5;
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
            Mesh.move(0, (float)y + 0.035f, 0);

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

            Mesh.move(0, -(float)y - 0.035f, 0);
        }

        public void hundir(float elapsedtime)
        {
            Mesh.rotateX(0.07f * elapsedtime);
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
            double S = Math.Sin(0.091*x -  time+0);

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



        public bool daniado { get; set; }
    }
}
