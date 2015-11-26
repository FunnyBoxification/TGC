//Matrices de transformacion
float4x4 matWorld; 
float4x4 matWorldView; 
float4x4 matWorldViewProj; 
float4x4 matInverseTransposeWorld; 

float3 fvLightPosition = float3( -100.00, 100.00, -100.00 );
float3 fvEyePosition = float3( 0.00, 0.00, -100.00 );
float height;
float time = 0;

// Fresnel
float FBias = 0.4;
int FPower = 1;
float FEscala = 0.5;

//Matrix Pallette para skinning
static const int MAX_MATRICES = 26;
float4x3 bonesMatWorldArray[MAX_MATRICES];

// Agua 
float2 vortice = float2(0,-100);
float x = 0;
float y = 0;

//Parametros de la Luz
float3 lightColor; //Color RGB de la luz
float4 lightPosition; //Posicion de la luz
float4 eyePosition; //Posicion de la camara
float lightIntensity; //Intensidad de la luz
float lightAttenuation; //Factor de atenuacion de la luz

//Parametros de Spot
float3 spotLightDir; //Direccion del cono de luz
float spotLightAngleCos; //Angulo de apertura del cono de luz (en radianes)
float spotLightExponent; //Exponente de atenuacion dentro del cono de luz

//Intensidad de efecto Bump
float bumpiness;
const float3 BUMP_SMOOTH = { 0.5f, 0.5f, 1.0f };

float k_la = 0.7;							// luz ambiente global
float k_ld = 0.4;							// luz difusa
float k_ls = 1.0;							// luz specular
float fSpecularPower = 16.84;

texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
	Texture = (texDiffuseMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

float4x4 g_mViewLightProj;
float4x4 g_mProjLight;
float3   g_vLightPos;  // posicion de la luz (en World Space) = pto que representa patch emisor Bj 
float3   g_vLightDir;  // Direcion de la luz (en World Space) = normal al patch Bj

//Material del mesh
float3 materialEmissiveColor; //Color RGB
float3 materialAmbientColor; //Color RGB
float4 materialDiffuseColor; //Color ARGB (tiene canal Alpha)
float3 materialSpecularColor; //Color RGB
float materialSpecularExp; //Exponente de specular

//Textura utilizada para BumpMapping
texture texNormalMap;
sampler2D normalMap = sampler_state
{
	Texture = (texNormalMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Factor de reflexion
float reflection;
texture texCubeMap;
samplerCUBE cubeMap = sampler_state
{
	Texture = (texCubeMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};


// Textura auxiliar:
texture aux_Tex;
sampler2D auxMap =
sampler_state
{
   Texture = (aux_Tex);
   ADDRESSU = MIRROR;
   ADDRESSV = MIRROR;
   MINFILTER = LINEAR;
   MAGFILTER = LINEAR;
   MIPFILTER = LINEAR;
};

texture  g_txCubeMapAgua;
samplerCUBE g_samCubeMapAgua = 
sampler_state
{
    Texture = <g_txCubeMapAgua>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

struct PS_INPUT_DIRECTIONAL
{
	float2 Texcoord : TEXCOORD0;
	float3 lightingNormal : TEXCOORD1;
	float4 lightingPosition : TEXCOORD2;
};

//Output del Vertex Shader
struct VS_OUTPUT 
{
    float4 oPos : POSITION;
    float2 Tex : TEXCOORD0;
    float3 tsEye : TEXCOORD1;
    float3 tsPos : TEXCOORD2;
    float3 wsPos : TEXCOORD3;
    float4 vPosLight : TEXCOORD4;
};

struct PS_INPUT2 
{
	float2 Texcoord : TEXCOORD0;
	float3 lightingNormal : TEXCOORD1;
	float4 lightingPosition : TEXCOORD2;
	float3 wsPos: TEXCOORD3;
	float4 vPosLight : TEXCOORD4;
	float3 Normal : NORMAL0;
};

//Input del Vertex Shader
struct VS_INPUT2 
{
	float4 Position : POSITION0;
	float3 Normal :   NORMAL0;
	float4 Color : COLOR;
	float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT4 
{
	float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float4 lightingPosition : TEXCOORD1;
	float3 lightingNormal : TEXCOORD2;
};


// ------------------------------------
// Shadow map
#define SMAP_SIZE 512
#define EPSILON 0.000005f


texture  g_txShadow;	// textura para el shadow map
sampler2D g_samShadow =
sampler_state
{
    Texture = <g_txShadow>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VS_OUTPUT2 
{
   float4 Position :        POSITION0;
   float2 Texcoord :        TEXCOORD0;
   float3 Norm :			TEXCOORD1;		// Normales
   float3 Pos :   			TEXCOORD3;		// Posicion real 3d
   float3 Normal : NORMAL0;
   float4 lightingPosition : TEXCOORD2;
};

//Estructura para guardar datos de la Luz
struct LightSampleValues {
	float3 L;
	float iL;
};

 
 VS_OUTPUT2 VSAgua( float4 Pos:POSITION0,float3 Normal:NORMAL0, float2 Texcoord:TEXCOORD0)
{

   float A = 3;

   float3 P0 = Pos.xyz;

   float C =  cos(0.091*P0.x - time + 0.91); //+0.2);
   float S =  sin(0.091*P0.x -  time+ 0.91);// +0.2 );

   VS_OUTPUT2 Output;
   
   Pos.x = P0.x;
   Pos.y =  (A+50) * (C + S) - 2 ;
   Pos.z = P0.z; 

   //Proyectar posicion
   Output.Position = mul( Pos, matWorldViewProj);
   //Propago  las coord. de textura 
   Output.Texcoord  = Texcoord + time/100 ;
   // Calculo la posicion real
   Output.Pos = mul(Pos,matWorld).xyz;
   // Transformo la normal y la normalizo
	//Output.Norm = normalize(mul(Normal,matInverseTransposeWorld));
	Output.Norm = normalize(mul(Normal,matWorldView));
	Output.Normal = mul(Normal,matWorldView).xyz;
	Output.lightingPosition = mul(Pos, matWorldView);
   return( Output );

}




float4 PSAgua(	float3 oPos: POSITION,
				float2 Texcoord:	TEXCOORD0, 				
				float3 tsEye:		TEXCOORD1,		// estan en tangent space!
				float3 tsPos:		TEXCOORD2,
				float3 wsPos:		TEXCOORD3,		// pos. en world space
				float4 vPosLight : TEXCOORD4		// pos. en Esp. Proyeccion de la luz
				
			) : COLOR0
{      


	float4 vCurrSample;
	// uso una funcion de onda para computar la altura del heightmap
	// calculos en Tangent space: 
	float k = 1;
	float vel = -2;
	float fCurrH = 0;
	float x = tsPos.x/250.0;
	float y = tsPos.y/250.0;
	// ondulaciones globales
	fCurrH +=(0.5+sin( k*(x+time*vel))/2 + 0.5+cos(k*(y+time*vel))/2);

	fCurrH = (fCurrH * 0.04 + 0.01)/tsEye.z;

	Texcoord += tsEye.xy * fCurrH;
	vCurrSample = tex2D( diffuseMap, Texcoord*0.1);
	
	// enviroment map 
	float dist = distance(float2(x,y),float2(0,0));
	float fp = cos( k*dist+time*vel)*k/dist*0.01;
	float3 vN = float3(0,1,0);
	//float3 vN = normalize(float3(-fp*(x-0),1,-fp*(y-0)/2));
	

	// necesito los valores en Worlds Space para acceder al cubemap
	// Reflexion
	float3 vEyeR = normalize(wsPos-fvEyePosition);
    	float3 EnvTex = reflect(vEyeR,vN);
	float4 color_reflejado = texCUBE( g_samCubeMapAgua, EnvTex);
	// Refraccion
    	float3 EnvTex1 = refract(vEyeR,-vN,0);
    	float3 EnvTex2 = refract(vEyeR,-vN,0);
    	float3 EnvTex3 = refract(vEyeR,-vN,0);
	float4 color_refractado = float4(
			tex2D( auxMap, float2(EnvTex1.x+1,-EnvTex1.z+1)*0.5).x,
			tex2D( auxMap, float2(EnvTex2.x+1,-EnvTex2.z+1)*0.5).y,
			tex2D( auxMap, float2(EnvTex3.x+1,-EnvTex3.z+1)*0.5).z,
			1);
	

    	float Fresnel = 0.1 + 0.1*pow(1 + abs(dot(vN,vEyeR)),7);    
    	float4 fvBaseColor = color_refractado*(1-Fresnel);
    
	k = 0.75;
	fvBaseColor = k*fvBaseColor + (1-k)*vCurrSample;
	fvBaseColor.a = 0.5 + (1-Fresnel)*0.5;


	float I = 0.65465;

	
	fvBaseColor.rgb *=saturate(fvBaseColor*(saturate(k_la+k_ld)) + k_ls); //I;

	//fvBaseColor.rgb = float3(1,0,1);
    //return fvBaseColor;

	return fvBaseColor;


}

//Calcular valores de luz para Directional Light
LightSampleValues computeDirLightValues()
{
	LightSampleValues values;
	values.L = lightPosition.xyz;
	values.iL = lightIntensity;
	return values;
}

// Calcular valores de luz para Point Light
LightSampleValues computePointLightValues(in float4 surfacePosition)
{
	LightSampleValues values;
	values.L = lightPosition.xyz - surfacePosition.xyz;
	float dist = length(values.L);
	values.L = values.L / dist; // normalize
	
	//attenuation
	float distAtten = dist * lightAttenuation;
	
	values.iL = lightIntensity / distAtten;
	return values;
}

//Calcular color RGB de Ambient
float3 computeAmbientComponent(in LightSampleValues light)
{
	return light.iL * lightColor * materialAmbientColor;
}

// //Calcular color RGB de Diffuse
float3 computeDiffuseComponent(in float3 surfaceNormal, in LightSampleValues light)
{
	return light.iL * lightColor * materialDiffuseColor.rgb * max(0.0, dot(surfaceNormal, light.L));
}

// //Calcular color RGB de Specular
float3 computeSpecularComponent(in float3 surfaceNormal, in float4 surfacePosition, in LightSampleValues light)
{
	float3 viewVector = normalize(-surfacePosition.xyz);
	float3 reflectionVector = 2.0 * dot(light.L, surfaceNormal) * surfaceNormal - light.L;
	return (dot(surfaceNormal, light.L) <= 0.0)
			? float3(0.0,0.0,0.0)
			: (
				light.iL * lightColor * materialSpecularColor 
				* pow( max( 0.0, dot(reflectionVector, viewVector) ), materialSpecularExp )
			);
}

//Pixel Shader para Point Light
float4 point_light_ps( PS_INPUT2 input ) : COLOR0
{      
	float3 Nn = normalize(input.lightingNormal);
	//Calcular datos de iluminacion para Directional Light
	LightSampleValues light = computePointLightValues(input.lightingPosition);
	
	//Sumar Emissive + Ambient + Diffuse
	float3 interpolatedNormal = normalize(input.lightingNormal);
	float4 diffuseLighting;
	diffuseLighting.rgb = materialEmissiveColor + computeAmbientComponent(light) + computeDiffuseComponent(interpolatedNormal, light);
	diffuseLighting.a = materialDiffuseColor.a;
	
	//Calcular Specular por separado
	float4 specularLighting;
	specularLighting.rgb = computeSpecularComponent(interpolatedNormal, input.lightingPosition, light);
	specularLighting.a = 0.5;
	
	//Obtener texel de la textura
	float4 texelColor = tex2D(auxMap, input.Texcoord); //* 0.65465;
		
	//Modular Diffuse con color de la textura y sumar luego Specular
	float4 finalColor = diffuseLighting * texelColor + specularLighting;
	finalColor.a = 0;

	
	return finalColor;
}

VS_OUTPUT4 vs_general(VS_INPUT2 input)
{
	VS_OUTPUT4 output;

	//Proyectar posicion
	output.Position = mul(input.Position, matWorldViewProj);

	//Las Coordenadas de textura quedan igual
	output.Texcoord = input.Texcoord;

	// The position and normal for lighting
	// must be in camera space, not homogeneous space
	
	//Almacenar la posicion del vertice en ViewSpace para ser usada luego por la luz
	output.lightingPosition = mul(input.Position, matWorldView);
	
	//Almacenar la normal del vertice en ViewSpace para ser usada luego por la luz
	output.lightingNormal = mul(input.Normal, matWorldView);

	return output;
}

technique RenderAgua
{
    pass p0
    {
        VertexShader = compile vs_3_0 VSAgua();
        PixelShader = compile ps_3_0 PSAgua();
    }
	pass p1
    {
        VertexShader = compile vs_3_0 VSAgua();
		//PixelShader = compile ps_3_0 spot_light_ps();
        PixelShader = compile  ps_3_0 point_light_ps();
		//PixelShader = compile ps_3_0 directional_light_ps();
    }

}



/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**************************************************************************************/
/* DIFFUSE_MAP */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT_DIFFUSE_MAP
{
	float4 Position : POSITION0;
	float4 Color : COLOR;
	float2 Texcoord : TEXCOORD0;
	float3 Normal :   NORMAL0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
	float4 BlendWeights : BLENDWEIGHT;
    float4 BlendIndices : BLENDINDICES;

};

//Output del Vertex Shader
struct VS_OUTPUT_DIFFUSE_MAP
{
	float4 Position : POSITION0;
	float4 Color : COLOR0; 
	float2 Texcoord : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
    float3 WorldTangent	: TEXCOORD2;
    float3 WorldBinormal : TEXCOORD3;
	float3 WorldPosition : TEXCOORD4;
	float3 LightVec	: TEXCOORD5;
	float3 HalfAngleVec	: TEXCOORD6;
};



//Vertex Shader
VS_OUTPUT_DIFFUSE_MAP vs_DiffuseMap(VS_INPUT_DIFFUSE_MAP input)
{
	VS_OUTPUT_DIFFUSE_MAP output;

	//Pasar indices de float4 a array de int
	int BlendIndicesArray[4] = (int[4])input.BlendIndices;
	
	//Skinning de posicion
	float3 skinPosition = mul(input.Position, bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;;
	skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
	skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
	skinPosition += mul(input.Position, bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
	
	//Skinning de normal
	float3 skinNormal = mul(input.Normal, (float3x3)bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
	skinNormal += mul(input.Normal, (float3x3)bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
	skinNormal += mul(input.Normal, (float3x3)bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
	skinNormal += mul(input.Normal, (float3x3)bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w; 
	output.WorldNormal = normalize(skinNormal);
	
	//Skinning de tangent
	float3 skinTangent = mul(input.Tangent, (float3x3)bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
	skinTangent += mul(input.Tangent, (float3x3)bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
	skinTangent += mul(input.Tangent, (float3x3)bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
	skinTangent += mul(input.Tangent, (float3x3)bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
	output.WorldTangent = normalize(skinTangent);
	
	//Skinning de binormal
	float3 skinBinormal = mul(input.Binormal, (float3x3)bonesMatWorldArray[BlendIndicesArray[0]]) * input.BlendWeights.x;
	skinBinormal += mul(input.Binormal, (float3x3)bonesMatWorldArray[BlendIndicesArray[1]]) * input.BlendWeights.y;
	skinBinormal += mul(input.Binormal, (float3x3)bonesMatWorldArray[BlendIndicesArray[2]]) * input.BlendWeights.z;
	skinBinormal += mul(input.Binormal, (float3x3)bonesMatWorldArray[BlendIndicesArray[3]]) * input.BlendWeights.w;
	output.WorldBinormal = normalize(skinBinormal);
	
	
	//Proyectar posicion (teniendo en cuenta lo que se hizo por skinning)
	output.Position = mul(float4(skinPosition.xyz, 1.0), matWorldViewProj);

	//Enviar color directamente
	output.Color = input.Color;

	//Enviar Texcoord directamente
	output.Texcoord = input.Texcoord;
	
	//Posicion pasada a World-Space (necesaria para atenuación por distancia)
	output.WorldPosition = mul(input.Position, matWorld);
	
	//LightVec (L): vector que va desde el vertice hacia la luz. Usado en Diffuse y Specular
	output.LightVec = lightPosition.xyz - output.WorldPosition;
	
	//ViewVec (V): vector que va desde el vertice hacia la camara.
	float3 viewVector = eyePosition.xyz - output.WorldPosition;
	
	//HalfAngleVec (H): vector de reflexion simplificado de Phong-Blinn (H = |V + L|). Usado en Specular
	output.HalfAngleVec = viewVector + output.LightVec;
	
	  
	return output;
}


//Input del Pixel Shader
struct PS_DIFFUSE_MAP
{
	float4 Color : COLOR0; 
	float2 Texcoord : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
	float3 WorldPosition : TEXCOORD4;
	float3 LightVec	: TEXCOORD5;
	float3 HalfAngleVec	: TEXCOORD6;	
};


//Pixel Shader
float4 ps_DiffuseMap(PS_DIFFUSE_MAP input) : COLOR0
{      
	//Normalizar vectores
	float3 Nn = normalize(input.WorldNormal);
	float3 Ln = normalize(input.LightVec);
	float3 Hn = normalize(input.HalfAngleVec);
	
	//Calcular intensidad de luz, con atenuacion por distancia
	float distAtten = length(lightPosition.xyz - input.WorldPosition) * lightAttenuation;
	float intensity = lightIntensity / distAtten; //Dividimos intensidad sobre distancia (lo hacemos lineal pero tambien podria ser i/d^2)
	
	//Obtener texel de la textura
	float4 texelColor = tex2D(diffuseMap, input.Texcoord);
	
	//Componente Ambient
	float3 ambientLight = intensity * lightColor * materialAmbientColor;
	
	//Componente Diffuse: N dot L
	float3 n_dot_l = dot(Nn, Ln);
	float3 diffuseLight = intensity * lightColor * materialDiffuseColor.rgb * max(0.0, n_dot_l); //Controlamos que no de negativo
	
	//Componente Specular: (N dot H)^exp
	float3 n_dot_h = dot(Nn, Hn);
	float3 specularLight = n_dot_l <= 0.0
			? float3(0.0, 0.0, 0.0)
			: (intensity * lightColor * materialSpecularColor * pow(max( 0.0, n_dot_h), materialSpecularExp));
	
	/* Color final: modular (Emissive + Ambient + Diffuse) por el color de la textura, y luego sumar Specular.
	   El color Alpha sale del diffuse material */
	float4 finalColor = float4(saturate(materialEmissiveColor + ambientLight + diffuseLight) * texelColor + specularLight, materialDiffuseColor.a);
	
	
	return finalColor;
}



/*
* Technique DIFFUSE_MAP
*/
technique DIFFUSE_MAP
{
   pass Pass_0
   {
	  VertexShader = compile vs_2_0 vs_DiffuseMap();
	  PixelShader = compile ps_2_0 ps_DiffuseMap();
   }
}