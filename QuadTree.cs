using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using TgcViewer;
using TgcViewer.Utils.TgcGeometry;

namespace AlumnoEjemplos.Quicksort
{  
    public static class QuadTree
    {
        //Estructura Nodo
        class Nodo
        {
            public float positionX, positionZ, width;
            public List<int> listaDeIndices;
            public List<int> listaDeIndicesLodI;
            public List<int> listaDeIndicesLodII;
            public Nodo[] hijos;
            public TgcBoundingBox caja;
        };
        
        static Nodo _nodoPadre;
        public const int MAX_SIZE = 250;
        public const int FRANJA_LOD = 1000;

        public static void Cargar( float centroX, float centroZ, int tamaño, Device device)
        {
            //Creo el nodo Padre
            //Llamo a la funcion recursiva para crear los nodos desde el padre
            _nodoPadre = CrearNodo(centroX, centroZ, tamaño, device);
        }

        static Nodo CrearNodo(float positionX, float positionZ, float width, Device device)
        {
	        float offsetX, offsetZ;

            Nodo node = new Nodo();
            //Creo el nogo y su posicion en el mundo
	        node.positionX = positionX;
	        node.positionZ = positionZ;
	        node.width = width;

            //Creo la bounding box para le nodo
            var boxLowerX = node.positionX - node.width;
            //var boxLowerY = ParametrosDeConfiguracion.Agua.NivelDelMar - (ParametrosDeConfiguracion.Modifier.Amplitud_Maxima + ParametrosDeConfiguracion.Modifier.Escala_Maxima);
            var boxLowerZ = node.positionZ - node.width;

            var boxUpperX = node.positionX + node.width;
            //var boxUpperY = ParametrosDeConfiguracion.Agua.NivelDelMar + (ParametrosDeConfiguracion.Modifier.Amplitud_Maxima + ParametrosDeConfiguracion.Modifier.Escala_Maxima);
            var boxUpperZ = node.positionZ + node.width;


            node.caja = new TgcBoundingBox(
                new Vector3(boxLowerX, 0, boxLowerZ),
                new Vector3(boxUpperX, 0, boxUpperZ));

	        //Inicializo los hijos como nulos
            node.hijos = null;
            
          //Si es grande que el maximo permitido lo divido en hijos
            if (node.width > MAX_SIZE)
            {
                node.hijos = new Nodo[4];
                for (int i = 0; i < 4; i++)
                {
                    //Calculo el offset para los nuevos hijos
                    offsetX = (((i % 2) < 1) ? -1.0f : 1.0f) * (width / 2.0f);
                    offsetZ = (((i % 4) < 2) ? -1.0f : 1.0f) * (width / 2.0f);

                    //Asigno al array de hijos el nuevo nodo
                    node.hijos[i] = CrearNodo((positionX + offsetX), (positionZ + offsetZ), (width / 2.0f), device);
                }
            }
            else
            {
                node.listaDeIndices = new List<int>();
                node.listaDeIndicesLodI = new List<int>();
                node.listaDeIndicesLodII = new List<int>();
            }
	        return node;
        }
        public static void AgregarIndices(List<int> indicesDeUnCuadrado)
        {
            var MinPosX = Oceano._vertices[indicesDeUnCuadrado[0]].X;
            var MaxPosX = Oceano._vertices[indicesDeUnCuadrado[2]].X;
            var MinPosZ = Oceano._vertices[indicesDeUnCuadrado[0]].Z;
            var MaxPosZ = Oceano._vertices[indicesDeUnCuadrado[2]].Z;

            AgregarIndicesNodo(_nodoPadre, indicesDeUnCuadrado, MinPosX, MaxPosX, MinPosZ,MaxPosZ);
            return;
        }
        static void AgregarIndicesNodo(Nodo nodo,List<int> indicesDeUnCuadrado,float MinPosX,float MaxPosX,float MinPosZ,float MaxPosZ)
        {
            if (MinPosX >= (nodo.positionX - nodo.width) && MaxPosX <= (nodo.positionX + nodo.width) &&
                MinPosZ >= (nodo.positionZ - nodo.width) && MaxPosZ <= (nodo.positionZ + nodo.width))
            {
                //Si es una hoja lo agrego a la lista sino recorro hasta el hijo
                if (nodo.hijos == null)
                {
                    nodo.listaDeIndices.AddRange(indicesDeUnCuadrado);
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        AgregarIndicesNodo(nodo.hijos[i], indicesDeUnCuadrado, MinPosX, MaxPosX, MinPosZ, MaxPosZ);
                    }
                }
            }
            return;
        }

        public static void AgregarLODI(List<int> indicesDeUnCuadrado)
        {
            var MinPosX = Oceano._vertices[indicesDeUnCuadrado[0]].X;
            var MaxPosX = Oceano._vertices[indicesDeUnCuadrado[2]].X;
            var MinPosZ = Oceano._vertices[indicesDeUnCuadrado[0]].Z;
            var MaxPosZ = Oceano._vertices[indicesDeUnCuadrado[2]].Z;

            AgregarLODINodo(_nodoPadre, indicesDeUnCuadrado, MinPosX, MaxPosX, MinPosZ, MaxPosZ);
            return;
        }
        static void AgregarLODINodo(Nodo nodo, List<int> indicesDeUnCuadrado, float MinPosX, float MaxPosX, float MinPosZ, float MaxPosZ)
        {
            if (MinPosX >= (nodo.positionX - nodo.width) && MaxPosX <= (nodo.positionX + nodo.width) &&
                MinPosZ >= (nodo.positionZ - nodo.width) && MaxPosZ <= (nodo.positionZ + nodo.width))
            {
                //Si es una hoja lo agrego a la lista sino recorro hasta el hijo
                if (nodo.hijos == null)
                {
                    nodo.listaDeIndicesLodI.AddRange(indicesDeUnCuadrado);
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        AgregarLODINodo(nodo.hijos[i], indicesDeUnCuadrado, MinPosX, MaxPosX, MinPosZ, MaxPosZ);
                    }
                }
            }
            return;
        }

        public static void AgregarLODII(List<int> indicesDeUnCuadrado)
        {
            var MinPosX = Oceano._vertices[indicesDeUnCuadrado[0]].X;
            var MaxPosX = Oceano._vertices[indicesDeUnCuadrado[2]].X;
            var MinPosZ = Oceano._vertices[indicesDeUnCuadrado[0]].Z;
            var MaxPosZ = Oceano._vertices[indicesDeUnCuadrado[2]].Z;

            AgregarLODIINodo(_nodoPadre, indicesDeUnCuadrado, MinPosX, MaxPosX, MinPosZ, MaxPosZ);
            return;
        }
        static void AgregarLODIINodo(Nodo nodo, List<int> indicesDeUnCuadrado, float MinPosX, float MaxPosX, float MinPosZ, float MaxPosZ)
        {
            if (MinPosX >= (nodo.positionX - nodo.width) && MaxPosX <= (nodo.positionX + nodo.width) &&
                MinPosZ >= (nodo.positionZ - nodo.width) && MaxPosZ <= (nodo.positionZ + nodo.width))
            {
                //Si es una hoja lo agrego a la lista sino recorro hasta el hijo
                if (nodo.hijos == null)
                {
                    nodo.listaDeIndicesLodII.AddRange(indicesDeUnCuadrado);
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        AgregarLODIINodo(nodo.hijos[i], indicesDeUnCuadrado, MinPosX, MaxPosX, MinPosZ, MaxPosZ);
                    }
                }
            }
            return;
        }

        public static List<int> IndiceVerticesVisibles()
        {
           //Trae todos los indices visibles dentro del frustum
           return  IndicesVisiblesDentroDelFrustum(_nodoPadre);
        }

        
        static List<int> IndicesVisiblesDentroDelFrustum(Nodo node)
        {            
            Device deviceContext = GuiController.Instance.D3dDevice;

            // Check to see if the node can be viewed, height doesn't matter in a quad tree.
            //test frustum-box intersection
            TgcCollisionUtils.FrustumResult c = TgcCollisionUtils.classifyFrustumAABB(GuiController.Instance.Frustum, node.caja);


	        // Si el nodo no es visible devuelvo lista vacia de indices
            if (c == TgcCollisionUtils.FrustumResult.OUTSIDE)
	        {
		        return new List<int>();
	        }

            //Si el nodo es visible en su totalidad traigo todos indices de los nodos hijos
            if (c == TgcCollisionUtils.FrustumResult.INSIDE)
            {
                return AgregarTodosLosNodos(node);
            }

            //Si el nodo es hoja devuelvo la lista de indices
            if (node.hijos == null)
            {
                return IndicesConNivelDeDetalle(node);                
            }

            //Si el nodo es parcialmente visible y no es hoja le pido a cada nodo hijo que verifique si es visible
            var lista = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                if (node.hijos[i] != null)
                {
                    lista.AddRange(IndicesVisiblesDentroDelFrustum(node.hijos[i]));
                }
            }
            return lista;

        }
        private static List<int> IndicesConNivelDeDetalle(Nodo node)
        {
            var posicionCamara = GuiController.Instance.CurrentCamera.getPosition();
            var posicionCentro = node.caja.calculateBoxCenter();
            var distancia = (posicionCentro - posicionCamara).Length();
            
            if(distancia < FRANJA_LOD)
                return node.listaDeIndices;

            if (distancia > 2*FRANJA_LOD)
                return node.listaDeIndicesLodII;

            return node.listaDeIndicesLodI;
        }

        private static List<int> AgregarTodosLosNodos(Nodo node)
        {
            Nodo[] children = node.hijos;

            //es hoja, cargar todos los indices
            if (children == null)
            {
                return IndicesConNivelDeDetalle(node);
            }
            //pedir hojas a hijos
            var lista = new List<int>();
            for (int i = 0; i < children.Length; i++)
            {
                lista.AddRange(AgregarTodosLosNodos(children[i]));
            }
            return lista;
        }
        private static void RenderBoxes(Nodo node) 
        {
            node.caja.render();
            if (node.hijos == null) return;
            foreach(var nodo in node.hijos)
            {
                RenderBoxes(nodo);
            }
        }
        public static void Render()
        {
            RenderBoxes(_nodoPadre);
        }
    }
}
