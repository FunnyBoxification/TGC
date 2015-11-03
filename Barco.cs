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
            double y = Calculo(time);

            Mesh.move(0, (float)y*10, 0);
        }
        public void volverAltura(float time)
        {
            double y = Calculo( time);

            Mesh.move(0, -(float)y*10, 0);
        }

        private double Calculo(float time)
        {
            //pruebo altura de ola
            double A = 5;
            double L = 50;	// wavelength
            double w = 5f * 3.1416f / L;
            double Q = 0.5f;
            double x = Mesh.Position.X;
            double z = Mesh.Position.Z;
            double y = Mesh.Position.Y;
            //float3 D = float3(1,1,0);
            // float dotD = dot(P0.xy, D);

            double C = Math.Cos(0.005 * x -time);
            double S = Math.Sin(0.005 * z -time);
            


            y = Q * A * (S + C);
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
                    return 1.1f; //dispara der
                }
                if (Vector3.Cross(Mesh.Rotation, vectDireccion).Y > 0) //el barco esta a la derecha o atras
                {
                    return 0.9f; //dispara izq
                }
                 
            }
            return 1;
        }


    }
}
