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
    class BarcoBot : Barco
    {
        float cooldown = 0;
        
        public Vector3 LastPos { get; set; }

        public BarcoBot(int vida, int danio, float velocidad,float aceleracion, float rotacion, TgcMesh mesh, double pot, Barco barcoEnemigo,TgcSceneLoader bm) : base (vida, danio, velocidad, rotacion, mesh,pot,bm)
        {
            BarcoEnemigo = barcoEnemigo;
            LastPos = Mesh.Position;

        }

        public float distancia()
        {
            Vector3 vectplanchado1 = new Vector3(BarcoEnemigo.Mesh.Position.X, 0, BarcoEnemigo.Mesh.Position.Z);
            Vector3 vectplanchado2 = new Vector3(this.Mesh.Position.X, 0, this.Mesh.Position.Z);
            
            return (Vector3.Length(vectplanchado1 - vectplanchado2));
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
            if( distancia() > 500){
                 acelerar();
                var vect = new Vector3(FastMath.Sin(Mesh.Rotation.Y), 0 ,FastMath.Cos(Mesh.Rotation.Y));
                var vect2 = new Vector3(FastMath.Sin(BarcoEnemigo.Mesh.Rotation.Y), 0, FastMath.Cos(BarcoEnemigo.Mesh.Rotation.Y));
                 Vector3 vectplanchado1 = new Vector3(BarcoEnemigo.Mesh.Position.X, 0, BarcoEnemigo.Mesh.Position.Z);
                    Vector3 vectplanchado2 = new Vector3(this.Mesh.Position.X, 0, this.Mesh.Position.Z);
                    Vector3 vectnDireccion = Vector3.Normalize(vectplanchado1-vectplanchado2);
                Vector3 vectnMio = Vector3.Normalize(this.Mesh.Rotation);
                var cdot = Vector3.Dot(vectnDireccion, vectnMio);                
                var angulo = Geometry.RadianToDegree((float)Math.Acos(Vector3.Dot(vectnDireccion, vect)));
                
                //var a =VDot(vectEnem.Normalize(), vectMio.Normalize());
                if (!(Vector3.Dot(vect, vectnDireccion) == 1))
                {

                    if (Vector3.Cross(vect, vectnDireccion).Y > 0  && VelocidadMov > 15)//el barco esta a la izq
                    {
                        this.rotar(1);
                    }
                    if (Vector3.Cross(vect, vectnDireccion).Y <0 && VelocidadMov > 15) //el barco esta a la derecha o atras
                    {
                        this.rotar(-1);
                    }

                }

            }
            else if (distancia() < 200)
            {
                
                var vect = new Vector3(FastMath.Sin(Mesh.Rotation.Y), 0, FastMath.Cos(Mesh.Rotation.Y));
                //var vect2 = new Vector3(FastMath.Sin(BarcoEnemigo.Mesh.Rotation.Y), 0, FastMath.Cos(BarcoEnemigo.Mesh.Rotation.Y));
                 Vector3 vectplanchado1 = new Vector3(BarcoEnemigo.Mesh.Position.X, 0, BarcoEnemigo.Mesh.Position.Z);
                    Vector3 vectplanchado2 = new Vector3(this.Mesh.Position.X, 0, this.Mesh.Position.Z);
                    Vector3 vectnDireccion = Vector3.Normalize(vectplanchado1 - vectplanchado2);
                //Vector3 vectnMio = Vector3.Normalize(this.Mesh.Rotation);
                //var cdot = Vector3.Dot(vectnDireccion, vectnMio);                
                //var angulo = Geometry.RadianToDegree((float)Math.Acos(Vector3.Dot(vectnDireccion, vect)));

                //var a =VDot(vectEnem.Normalize(), vectMio.Normalize());
                if (!(Vector3.Dot(vect, vectnDireccion) == 1))
                {

                    if (Vector3.Cross(vect, vectnDireccion).Y > 0 && VelocidadMov > 15)//el barco esta a la izq
                    {

                        this.rotar(-1); //roto al revez
                    }
                    if (Vector3.Cross(vect, vectnDireccion).Y< 0 && VelocidadMov> 15) //el barco esta a la derecha o atras
                    {
                        this.rotar(1); //roto al revez
                    }
                    

                }
                acelerar();
            }else
            {
                if(cooldown < 1){
                    frenar();

                    var vect = new Vector3(FastMath.Sin(Mesh.Rotation.Y), 0, FastMath.Cos(Mesh.Rotation.Y));
                    Vector3 vectplanchado1 = new Vector3(BarcoEnemigo.Mesh.Position.X, 0, BarcoEnemigo.Mesh.Position.Z);
                    Vector3 vectplanchado2 = new Vector3(this.Mesh.Position.X, 0, this.Mesh.Position.Z);
                    Vector3 vectnDireccion = Vector3.Normalize(vectplanchado1-vectplanchado2);

                    if (!(Vector3.Dot(vect, vectnDireccion) == 1))
                    {
                        if (Vector3.Cross(vect, vectnDireccion).Y > 0)//el barco esta a la izq
                        {
                            dispararBala(1, 1); //dispara der
                        }
                        if (Vector3.Cross(vect, vectnDireccion).Y < 0) //el barco esta a la derecha o atras
                        {
                            dispararBala(1, 0); //dispara izq
                        }
                    }       
                    cooldown = 5;
                }else
                {
                 cooldown -= elapsedTime*1;
                 var vect = new Vector3(FastMath.Sin(Mesh.Rotation.Y), 0, FastMath.Cos(Mesh.Rotation.Y));
                 Vector3 vectnDireccion = Vector3.Normalize(BarcoEnemigo.Mesh.Position - this.Mesh.Position);
                 var angulo = (float)Math.Acos(Vector3.Dot(vectnDireccion, vect));

                 
                     //if (Vector3.Cross(vect, vectnDireccion).Y > 0)//el barco esta a la izq
                     //{
                         if (Math.Cos(angulo) > 0)
                         {
                             this.Mesh.rotateY(Geometry.DegreeToRadian((float)-6 * elapsedTime));
                         }
                         if (Math.Cos(angulo) < 0)
                         {
                             this.Mesh.rotateY(Geometry.DegreeToRadian((float)6 * elapsedTime));
                         }
                     //}
                     //if (Vector3.Cross(vect, vectnDireccion).Y < 0) //el barco esta a la derecha o atras
                     //{
                     //    this.Mesh.rotateY(Geometry.DegreeToRadian((float)-1 * elapsedTime)); //dispara izq
                     //}
                        
                }
            }

           

            //Si hubo rotacion
            if (VelocidadMov > 15)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                float rotAngle = Geometry.DegreeToRadian((float)VelocidadRot * elapsedTime);
                this.Mesh.rotateY(rotAngle);
                //GuiController.Instance.ThirdPersonCamera.rotateY(rotAngle);
            }

            //Si hubo desplazamiento
            //if (moving)
            {

                //Aplicar movimiento hacia adelante o atras segun la orientacion actual del Mesh
                Vector3 lastPos = this.Mesh.Position;

                //La velocidad de movimiento tiene que multiplicarse por el elapsedTime para hacerse independiente de la velocida de CPU
                //Ver Unidad 2: Ciclo acoplado vs ciclo desacoplado
                this.Mesh.moveOrientedY((float)VelocidadMov *boostOla()* elapsedTime);
            }
        }
    }
}
