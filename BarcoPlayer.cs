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
    class BarcoPlayer : Barco { 

        bool rotating;
        bool moving;

        public BarcoPlayer(int vida, int danio, float velocidad,float aceleracion, float rotacion, TgcMesh mesh, double pot) : base (vida, danio, velocidad, rotacion, mesh,pot)
        {
             rotating = false;
             moving = false;
        }
        
        public void Movimiento(float elapsedTime)
        {
            //Calcular proxima posicion de personaje segun Input
           
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            

            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                
                 this.acelerar();
                this.moving = true;
               
            }

            //Atras
            if (d3dInput.keyDown(Key.S))
            {
                this.desacelerar();
                this.moving = true;
                
            }

            //Derecha
            if (d3dInput.keyDown(Key.D))
            {
                this.rotar(1);
                this.rotating = true;
                
            }

            //Izquierda
            if (d3dInput.keyDown(Key.A))
            {
                this.rotar(-1);
                this.rotating = true;
                
            }

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                float rotAngle = Geometry.DegreeToRadian((float)VelocidadRot * elapsedTime);
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
                this.Mesh.moveOrientedY((float)VelocidadMov * elapsedTime);
            }
        }

        private void rotar(int p)
        {
            /*if (this.VelocidadRotMax > this.VelocidadRot)
            {
                VelocidadRot = VelocidadRot + VelocidadRot * (1/Potencia)*(p);
            }*/
            VelocidadRot = p * VelocidadRot;
            
        }

        private void acelerar()
        {
            /*if (this.VelocidadMovMax > this.VelocidadMov){
                VelocidadMov = VelocidadMov + VelocidadMov * Potencia;
            }*/
            VelocidadMov = VelocidadMov;
            
        }

        private void desacelerar()
        {
            if (this.VelocidadMovMax > this.VelocidadMov)
            {
                VelocidadMov = VelocidadMov - VelocidadMov * Potencia;
            }
            
        }
    }
}
