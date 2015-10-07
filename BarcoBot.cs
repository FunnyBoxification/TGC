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
    class BarcoBot : Barco
    {
        public Barco BarcoEnemigo { get; set; }

        public BarcoBot(int vida, int danio, float velocidad,float aceleracion, float rotacion, TgcMesh mesh, double pot, Barco barcoEnemigo) : base (vida, danio, velocidad, rotacion, mesh,pot)
        {
            BarcoEnemigo = barcoEnemigo;
        }

        public float distancia()
        {
            return (Vector3.Length(BarcoEnemigo.Mesh.Position - this.Mesh.Position));
        }

        public Boolean estaEnCarrera()
        {
            Vector3 vect = BarcoEnemigo.Mesh.Position - this.Mesh.Position;
            var vnulo = new Vector3(0,0,0);

            if(Vector3.Cross(vect,this.Mesh.Rotation) == vnulo){
                return true;
            }else{
                return false;
            }


        }

        public void Movimiento(float elapsedTime)
        {
            //Calcular proxima posicion de personaje segun Input

            TgcD3dInput d3dInput = GuiController.Instance.D3dInput;

            
            // distancia para aproximarse al barco
            if( distancia() > 100){
                 acelerar();
                Vector3 vectnDireccion = Vector3.Normalize(BarcoEnemigo.Mesh.Position - this.Mesh.Position);
                Vector3 vectnMio = Vector3.Normalize(this.Mesh.Position);
                //var a =VDot(vectEnem.Normalize(), vectMio.Normalize());
                if (!(Vector3.Dot(vectnDireccion, vectnMio) == 1))
                {

                    if (Vector3.Dot(vectnDireccion, vectnMio) > 0)//el barco esta a la izq
                    {
                        this.rotar(-1);
                    }
                    else //el barco esta a la derecha o atras
                    {
                        this.rotar(1);
                    }

                }

            }
            else
            {
                frenar();
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
