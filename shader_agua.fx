//Matrices de transformacion
float4x4 matWorld; 
float4x4 matWorldView; 
float4x4 matWorldViewProj; 
float4x4 matInverseTransposeWorld; 

float3 fvLightPosition = float3( -100.00, 100.00, -100.00 );
float3 fvEyePosition = float3( 0.00, 0.00, -100.00 );
float time = 0;

// Fresnel
float FBias = 0.4;
int FPower = 1;
float FEscala = 0.5;

// Agua 
float2 vortice = float2(0,-100);
float x = 0;
float y = 0;


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
   float3 Pos :   			TEXCOORD2;		// Posicion real 3d
};
 
 VS_OUTPUT2 VSAgua( float4 Pos:POSITION0,float3 Normal:NORMAL, float2 Texcoord:TEXCOORD0)
{

   float A = 5;
	float L = 50;	// wavelength
	float w = 5*3.1416/L;
	float Q = 0.5;
   float3 P0 = Pos.xyz;
   //float3 D = float3(1,1,0);
  // float dotD = dot(P0.xy, D);
	float C = cos(0.005*P0.x - time);
	float S = sin(0.005*P0.z -  time);
   VS_OUTPUT2 Output;
   
   Pos.x = P0.x;
   Pos.y = P0.y + Q*A*(S+C);//*D.y; //C; //P0.y * 2 * ( cos(0.005*P0.x - time) + sin(0.005 * P0.z - time) );
   Pos.z = P0.z; //+ Q*A*S*D.y;
   //Proyectar posicion
   Output.Position = mul( Pos, matWorldViewProj);
   //Propago  las coord. de textura 
   Output.Texcoord  = Texcoord;
   // Calculo la posicion real
   Output.Pos = mul(Pos,matWorld).xyz;
   // Transformo la normal y la normalizo
	//Output.Norm = normalize(mul(Normal,matInverseTransposeWorld));
	Output.Norm = normalize(mul(Normal,matWorld));
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
	//float3 vN = float3(0,1,0);
	float3 vN = normalize(float3(-fp*(x-0),1,-fp*(y-0)/2));
	

	// necesito los valores en Worlds Space para acceder al cubemap
	// Reflexion
	float3 vEyeR = normalize(wsPos-fvEyePosition);
    	float3 EnvTex = reflect(vEyeR,vN);
	float4 color_reflejado = texCUBE( g_samCubeMapAgua, EnvTex);
	// Refraccion
    	float3 EnvTex1 = refract(vEyeR,-vN,1.001);
    	float3 EnvTex2 = refract(vEyeR,-vN,1.009);
    	float3 EnvTex3 = refract(vEyeR,-vN,1.02);
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


	return fvBaseColor;


}

technique RenderAgua
{
    pass p0
    {
        VertexShader = compile vs_3_0 VSAgua();
        PixelShader = compile ps_3_0 PSAgua();
    }
}