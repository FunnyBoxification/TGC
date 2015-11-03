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
    class BarcoPlayer : Barco {
        float cooldown = 0;
        

        public BarcoPlayer(int vida, int danio, float velocidad,float aceleracion, float rotacion, TgcMesh mesh, double pot, TgcSceneLoader bm) : base (vida, danio, velocidad, rotacion, mesh,pot,bm)
        {
            
        }
        
        public void Movimiento(float elapsedTime)
        {
            //Calcular proxima posicion de personaje segun Input
           
            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;
            //bool moving = false;
            

            //Adelante
            if (d3dInput.keyDown(Key.W))
            {
                
                 this.acelerar();
                 //moving = true;
                
               
            }

            //Atras
            if (d3dInput.keyDown(Key.S))
            {
                this.desacelerar();
                //moving = true;
               
                
            }

            //Derecha
            if (d3dInput.keyDown(Key.D) && VelocidadMov > 15)
            {
                this.rotar(1);
                //moving = true;
               
                
            }

            //Izquierda
            if (d3dInput.keyDown(Key.A) && VelocidadMov > 15)
            {
                this.rotar(-1);
                //moving = true;
               
                
            }

            if (d3dInput.keyDown(Key.E) && cooldown <= 0)
            {
                this.dispararBala(0,1);
                cooldown = 4;


            }
            else if (d3dInput.keyDown(Key.Q) && cooldown <= 0)
            {
                this.dispararBala(0,0);
                cooldown = 4;


            }else
            {
                cooldown = cooldown - 1 * elapsedTime;
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
           
            {

                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                Vector3 lastPos = this.Mesh.Position;

                //La velocidad de movimiento tiene que multiplicarse por el elapsedTime para hacerse independiente de la velocida de CPU
                //Ver Unidad 2: Ciclo acoplado vs ciclo desacoplado
                this.Mesh.moveOrientedY((float)VelocidadMov * elapsedTime);

                bool collide = false;
      
                    TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(this.Mesh.BoundingBox, BarcoEnemigo.Mesh.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        collide = true;
                        Vida = 0;
                        BarcoEnemigo.Vida = 0;
                        
                    }
                    /*foreach(TgcMesh mesh in EjemploAlumno.getEscenaMeshes() ) {

                    result = TgcCollisionUtils.classifyBoxBox(this.Mesh.BoundingBox, mesh.BoundingBox);
                    if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        collide = true;
                    }
                    }*/
        



                //Si hubo colision, restaurar la posicion anterior
                if (collide)
                {
                    this.Mesh.Position = lastPos;
                    if (VelocidadMov > 0)
                    {
                        VelocidadMov = 0;
                    }
                }
               
            }
        }

        
    }
}
