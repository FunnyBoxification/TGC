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

namespace AlumnoEjemplos.Quicksort
{
    class BarcoPlayer : Barco
    {
        public BarcoPlayer(int vida, int danio, float velocidad, float rotacion, TgcMesh mesh, double pot) : base (vida, danio, velocidad, rotacion, mesh,pot)
        {
             
        }
        
        public void Movimiento(float elapsedTime)
        {
            //Calcular proxima posicion de personaje segun Input
            float moveForward = 0f;
            float rotate = 0;
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            bool moving = false;
            bool rotating = false;

            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                
                moveForward =  this.acelerar();
                moving = true;
            }

            //Atras
            if (d3dInput.keyDown(Key.S))
            {
                moveForward = this.desacelerar();
                moving = true;
            }

            //Derecha
            if (d3dInput.keyDown(Key.D))
            {
                rotate = VELOCIDAD_ROTACION;
                rotating = true;
            }

            //Izquierda
            if (d3dInput.keyDown(Key.A))
            {
                rotate = -VELOCIDAD_ROTACION;
                rotating = true;
            }

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                float rotAngle = Geometry.DegreeToRadian(rotate * elapsedTime);
                this.Mesh.rotateY(rotAngle);
                GuiController.Instance.ThirdPersonCamera.rotateY(rotAngle);
            }

            //Si hubo desplazamiento
            if (moving)
            {

                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                Vector3 lastPos = this.Mesh.Position;

                //La velocidad de movimiento tiene que multiplicarse por el elapsedTime para hacerse independiente de la velocida de CPU
                //Ver Unidad 2: Ciclo acoplado vs ciclo desacoplado
                this.Mesh.moveOrientedY(moveForward * elapsedTime);
            }
        }

        private float acelerar()
        {
            if (this.VelocidadMovMax > this.VelocidadMov){
                VelocidadMov = VelocidadMov + VelocidadMov * Potencia;
            }
            return (float)VelocidadMov;
        }

        private float desacelerar()
        {
            if (this.VelocidadMovMax > this.VelocidadMov)
            {
                VelocidadMov = VelocidadMov - VelocidadMov * Potencia;
            }
            return (float)VelocidadMov;
        }
    }
}
