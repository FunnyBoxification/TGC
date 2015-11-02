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
    }
}
