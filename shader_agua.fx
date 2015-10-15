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


VS_OUTPUT VSAgua( float4 Pos : POSITION,
             float2 Texcoord : TEXCOORD0,
             float3 normal : NORMAL)
{
	VS_OUTPUT Output;
	
	float k = 3;
	float vel = -3;
	float sinarg = (5+sin( k*(Pos.x+time*vel))/2 + 5+cos(k*(Pos.y+time*vel))/2); //mul(Pos.x, 2.5) + time;
	
	Pos.y = mul(2.0, sin(sinarg));


	float4 VertexPositionWS = mul( Pos,matWorld );
	
	Output.wsPos = VertexPositionWS.xyz;		// de paso devuelvo la Posicion en worldspace
    // y de paso propago la posicion del vertice en el espacio de proyeccion de la luz
   Output.vPosLight = mul( VertexPositionWS, g_mViewLightProj );
	
	float3 E = fvEyePosition.xyz - Output.wsPos;
	
	// calculo la tg y la binormal 
	//float3 up = float3(0,0,1);
	//float3 tangent = cross(up,normal);
	//float3 binormal = cross(normal,tangent);
	
	normal = float3(0,1,0);
	float3 tangent = float3(0,0,1);
	float3 binormal = float3(1,0,0);
	
	float3x3 tangentToWorldSpace;
	tangentToWorldSpace[0] = mul( tangent, matWorld );
	tangentToWorldSpace[1] = mul( binormal, matWorld);
	tangentToWorldSpace[2] = mul( normal, matWorld);

	float3x3 worldToTangentSpace = transpose(tangentToWorldSpace);

	// proyecto
    Output.oPos = mul( Pos, matWorldViewProj );

    // Propago la textura
    Output.Tex = Texcoord;


	// devuelvo la pos. del ojo expresados en tangent space: 
	Output.tsEye = mul( E, worldToTangentSpace );
	// devuelvo la pos. del pto en tg space
	Output.tsPos = mul( VertexPositionWS.xyz,worldToTangentSpace);
	
	return( Output );
 }


float4 PSAgua(	float3 Pos: POSITION,
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