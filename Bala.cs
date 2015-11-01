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
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.Quicksort
{
    class Bala
    {

        bool col = false;
        public double alturaMax { get; set; }
        public bool activa { get; set; }
        public bool subiendo { get; set; }
        public Bala(TgcMesh mesh, int dam,Barco barcoEnem)
        {

            Mesh = mesh;
            danio = dam;
            BarcoEnemigo = barcoEnem;
            alturaMax = 50;
            activa = true;
            subiendo = true;
        }
        public int danio { get; set; }
        public TgcMesh Mesh { get; set; }
        public float altura { get; set; }
        public float direccion { get; set; }
        public Barco BarcoEnemigo { get; set; }
        

        public void Mover(float elapsedTime)
        {

            TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(Mesh.BoundingBox, BarcoEnemigo.Mesh.BoundingBox);
            if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
            {
                if (col == false)
                {
                    BarcoEnemigo.Vida = BarcoEnemigo.Vida - danio;
                    col = true;
                }
            }
            foreach (TgcMesh mesh in EjemploAlumno.getEscenaMeshes())
            {

                 result = TgcCollisionUtils.classifyBoxBox(this.Mesh.BoundingBox, mesh.BoundingBox);
                if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                {
                    activa = false;
                }
            }

            this.Mesh.rotateX((float)10 * elapsedTime);
            this.Mesh.moveOrientedY((float) 120 *elapsedTime);
            if (subiendo)
            {
                this.Mesh.move(new Vector3(0, elapsedTime * 10f, 0));
                if(Mesh.Position.Y > alturaMax){
                    subiendo = false;
                }

            }
            else
            {
                this.Mesh.move( new Vector3(0, - elapsedTime*10f, 0));

            }


        }

    }
}
