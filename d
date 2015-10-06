[33mcommit ed031387f99d51906b1dfeb28329a3f6adcd08ad[m
Merge: 544bd39 91ad6eb
Author: Julian Crocco <crocco.julian@gmail.com>
Date:   Mon Oct 5 10:01:07 2015 -0300

    Merge branch 'master' of https://github.com/FunnyBoxification/TGC

[1mdiff --cc EjemploAlumno.cs[m
[1mindex 55bb1a2,536d57d..0060cba[m
[1m--- a/EjemploAlumno.cs[m
[1m+++ b/EjemploAlumno.cs[m
[36m@@@ -117,8 -149,15 +149,15 @@@[m [mnamespace AlumnoEjemplos.Quicksor[m
  [m
              barcoPrincipal.Movimiento(elapsedTime);[m
  [m
[32m+             efectoAgua.SetValue("g_vLightPos", new Vector4(g_LightPos.X, g_LightPos.Y, g_LightPos.Z, 1));[m
[32m+             efectoAgua.SetValue("g_vLightDir", new Vector4(g_LightDir.X, g_LightDir.Y, g_LightDir.Z, 1));[m
[32m+             g_LightView = Matrix.LookAtLH(g_LightPos, g_LightPos + g_LightDir, new Vector3(0, 0, 1));[m
[32m+             efectoAgua.SetValue("g_mViewLightProj", g_LightView);[m
[32m+             efectoAgua.SetValue("time", time);[m
[32m+             efectoAgua.SetValue("aux_Tex", textura);[m
[32m+ [m
              //Hacer que la camara siga al personaje en su nueva posicion[m
[31m -            GuiController.Instance.ThirdPersonCamera.Target = mainMesh.Position;[m
[32m +            GuiController.Instance.ThirdPersonCamera.Target = barcoPrincipal.Mesh.Position;[m
  [m
              //Dibujar objeto principal[m
              //Siempre primero hacer todos los cálculos de lógica e input y luego al final dibujar todo (ciclo update-render)[m
