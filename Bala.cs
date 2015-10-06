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

namespace AlumnoEjemplos.MiGrupo
{
    class Bala
    {
        public int danio { get; set; }
        public TgcMesh Mesh { get; set; }
        public float altura { get; set; }
        public float direccion { get; set; }

        public void Mover(float elapsedTime)
        {


            this.Mesh.moveOrientedY((float) altura +elapsedTime);

        }

    }
}
