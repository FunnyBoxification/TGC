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
        public double VelocidadMov { get; set; }
        public double VelocidadRot { get; set; }
        public TgcMesh Mesh { get; set; }

        public Barco(int vida, int danio, double velocidad, double rotacion, TgcMesh mesh)
        {
            Vida = vida;
            Danio = danio;
            VelocidadMov = velocidad;
            VelocidadRot = rotacion;
            Mesh = mesh;
        }
    }
}
