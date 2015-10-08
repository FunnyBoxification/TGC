using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;

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
        LinkedList<Bala> balas;

        public Barco(int vida, int danio, double velocidad, double rotacion, TgcMesh mesh,double potencia)
        {
            Vida = vida;
            Danio = danio;
            VelocidadMovMax = velocidad;
            VelocidadRotMax = rotacion;
            Mesh = mesh;
            Potencia = potencia;
            VelocidadMov = 0;
            VelocidadRot = 0;
            balas = new LinkedList<Bala>();
        }

        public void dispararBala()
        {
            var bala = new Bala(Mesh.Position);
            balas.AddLast(bala);
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
