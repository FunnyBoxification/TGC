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
        int cooldown = 0;
        

        public BarcoPlayer(int vida, int danio, float velocidad,float aceleracion, float rotacion, TgcMesh mesh, double pot, TgcSceneLoader bm) : base (vida, danio, velocidad, rotacion, mesh,pot,bm)
        {
            
        }
        
        public void Movimiento(float elapsedTime)
        {
            //Calcular proxima posicion de personaje segun Input
           
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            

            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                
                 this.acelerar();
                
               
            }

            //Atras
            if (d3dInput.keyDown(Key.S))
            {
                this.desacelerar();
               
                
            }

            //Derecha
            if (d3dInput.keyDown(Key.D) && VelocidadMov > 15)
            {
                this.rotar(1);
               
                
            }

            //Izquierda
            if (d3dInput.keyDown(Key.A) && VelocidadMov > 15)
            {
                this.rotar(-1);
               
                
            }

            if (d3dInput.keyDown(Key.R) && cooldown < 1)
            {
                this.dispararBala();
                cooldown = 400;


            }
            else
            {
                cooldown -= 1;
            }

            //Si hubo rotacion
            if (VelocidadMov > 15)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                float rotAngle = Geometry.DegreeToRadian((float)VelocidadRot * elapsedTime);
                this.Mesh.rotateY(rotAngle);
                GuiController.Instance.ThirdPersonCamera.rotateY(rotAngle);
            }

            //Si hubo desplazamiento
            //if (moving)
            {

                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                Vector3 lastPos = this.Mesh.Position;

                //La velocidad de movimiento tiene que multiplicarse por el elapsedTime para hacerse independiente de la velocida de CPU
                //Ver Unidad 2: Ciclo acoplado vs ciclo desacoplado
                this.Mesh.moveOrientedY((float)VelocidadMov * elapsedTime);
            }
        }

        
    }
}
