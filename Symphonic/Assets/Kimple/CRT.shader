// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:True,rprd:True,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:32733,y:34086,varname:node_3138,prsc:2|diff-7451-OUT,spec-8370-OUT,gloss-2250-OUT,emission-4757-OUT;n:type:ShaderForge.SFN_Tex2d,id:5621,x:29324,y:33856,ptovrint:True,ptlb:Screen,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False|UVIN-3363-OUT;n:type:ShaderForge.SFN_Multiply,id:7619,x:31773,y:34137,varname:node_7619,prsc:2|A-4462-OUT,B-2799-OUT;n:type:ShaderForge.SFN_Slider,id:2250,x:32684,y:34003,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:_Gloss,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:4757,x:32029,y:34188,varname:node_4757,prsc:2|A-2950-OUT,B-7619-OUT;n:type:ShaderForge.SFN_Vector3,id:9593,x:30903,y:33941,varname:node_9593,prsc:2,v1:1,v2:0,v3:0;n:type:ShaderForge.SFN_Vector3,id:7254,x:30897,y:34142,varname:node_7254,prsc:2,v1:0,v2:1,v3:0;n:type:ShaderForge.SFN_Vector3,id:2397,x:30897,y:34365,varname:node_2397,prsc:2,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_Multiply,id:3612,x:30903,y:34029,varname:node_3612,prsc:2|A-9593-OUT,B-6838-OUT,C-2928-R;n:type:ShaderForge.SFN_Multiply,id:4883,x:30897,y:34233,varname:node_4883,prsc:2|A-7254-OUT,B-3516-OUT,C-2928-G;n:type:ShaderForge.SFN_Multiply,id:3946,x:30897,y:34448,varname:node_3946,prsc:2|A-2397-OUT,B-6786-OUT,C-2928-B;n:type:ShaderForge.SFN_Tex2d,id:2928,x:30345,y:34194,ptovrint:False,ptlb:PixelTexture,ptin:_PixelTexture,varname:_PixelTexture,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Add,id:7337,x:31155,y:34137,varname:node_7337,prsc:2|A-3612-OUT,B-4883-OUT,C-3946-OUT;n:type:ShaderForge.SFN_Clamp01,id:4462,x:31525,y:34137,varname:node_4462,prsc:2|IN-1491-OUT;n:type:ShaderForge.SFN_Tex2d,id:3095,x:28590,y:34166,varname:node_3095,prsc:2,ntxv:2,isnm:False|UVIN-6594-UVOUT,TEX-1387-TEX;n:type:ShaderForge.SFN_Panner,id:6594,x:28383,y:34166,varname:node_6594,prsc:2,spu:0,spv:1|UVIN-5811-OUT,DIST-9809-OUT;n:type:ShaderForge.SFN_TexCoord,id:6360,x:27759,y:34147,varname:node_6360,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:1491,x:31359,y:34137,varname:node_1491,prsc:2|A-7337-OUT,B-7685-OUT;n:type:ShaderForge.SFN_Clamp01,id:7685,x:31179,y:34481,varname:node_7685,prsc:2|IN-4975-OUT;n:type:ShaderForge.SFN_Slider,id:8865,x:30791,y:35007,ptovrint:False,ptlb:ScanlineDarkness,ptin:_ScanlineDarkness,varname:_ScanlineDarkness,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.15,max:1;n:type:ShaderForge.SFN_Add,id:4975,x:30881,y:34689,varname:node_4975,prsc:2|A-5508-OUT,B-5056-OUT;n:type:ShaderForge.SFN_Time,id:5389,x:27979,y:34416,varname:node_5389,prsc:2;n:type:ShaderForge.SFN_Multiply,id:9809,x:28167,y:34435,varname:node_9809,prsc:2|A-5389-T,B-3788-OUT;n:type:ShaderForge.SFN_Slider,id:3788,x:28088,y:34597,ptovrint:False,ptlb:ScanlineSpeed,ptin:_ScanlineSpeed,varname:_ScanlineSpeed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3,max:5;n:type:ShaderForge.SFN_Append,id:5811,x:28184,y:34166,varname:node_5811,prsc:2|A-6360-U,B-8288-OUT;n:type:ShaderForge.SFN_Multiply,id:8288,x:28000,y:34186,varname:node_8288,prsc:2|A-6360-V,B-2913-OUT;n:type:ShaderForge.SFN_Slider,id:2913,x:27900,y:34341,ptovrint:False,ptlb:ScanlineThickness,ptin:_ScanlineThickness,varname:_ScanlineThickness,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.95,max:10;n:type:ShaderForge.SFN_Tex2d,id:8510,x:31773,y:33891,ptovrint:False,ptlb:SpecularText,ptin:_SpecularText,varname:_SpecularText,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:7ae17514abd8e7141827efd07865b7b8,ntxv:1,isnm:False;n:type:ShaderForge.SFN_Multiply,id:7060,x:32240,y:34121,varname:node_7060,prsc:2|A-8510-R,B-2950-OUT;n:type:ShaderForge.SFN_Clamp01,id:8370,x:32478,y:34121,varname:node_8370,prsc:2|IN-7060-OUT;n:type:ShaderForge.SFN_Lerp,id:9620,x:28897,y:33951,varname:node_9620,prsc:2|A-7290-OUT,B-59-OUT,T-3095-G;n:type:ShaderForge.SFN_Multiply,id:59,x:28699,y:33972,varname:node_59,prsc:2|A-8806-V,B-4191-OUT;n:type:ShaderForge.SFN_Vector1,id:4191,x:28508,y:33995,varname:node_4191,prsc:2,v1:0.001;n:type:ShaderForge.SFN_Vector1,id:7290,x:28686,y:33915,varname:node_7290,prsc:2,v1:0;n:type:ShaderForge.SFN_ToggleProperty,id:3974,x:29324,y:33785,ptovrint:False,ptlb:ScreenON,ptin:_ScreenON,varname:_ScreenON,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True;n:type:ShaderForge.SFN_TexCoord,id:8806,x:27759,y:33841,varname:node_8806,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Add,id:3363,x:29097,y:33858,varname:node_3363,prsc:2|A-2375-OUT,B-9620-OUT;n:type:ShaderForge.SFN_Set,id:2627,x:28956,y:34204,varname:Scanline,prsc:2|IN-3330-OUT;n:type:ShaderForge.SFN_Set,id:6456,x:28942,y:34464,varname:Vignette,prsc:2|IN-7835-OUT;n:type:ShaderForge.SFN_Get,id:5508,x:30641,y:34689,varname:node_5508,prsc:2|IN-2627-OUT;n:type:ShaderForge.SFN_Get,id:2950,x:31752,y:34069,varname:node_2950,prsc:2|IN-6456-OUT;n:type:ShaderForge.SFN_OneMinus,id:7835,x:28761,y:34464,varname:node_7835,prsc:2|IN-2856-B;n:type:ShaderForge.SFN_OneMinus,id:3330,x:28786,y:34204,varname:node_3330,prsc:2|IN-3095-G;n:type:ShaderForge.SFN_Tex2dAsset,id:1387,x:28383,y:34348,ptovrint:False,ptlb:ScanlineVignette,ptin:_ScanlineVignette,varname:_ScanlineVignette,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:2856,x:28579,y:34407,varname:node_2856,prsc:2,ntxv:0,isnm:False|TEX-1387-TEX;n:type:ShaderForge.SFN_Slider,id:8083,x:27407,y:33914,ptovrint:False,ptlb:Vertical Adjustment,ptin:_VerticalAdjustment,varname:_VSync,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-0.1,cur:0,max:0.1;n:type:ShaderForge.SFN_Append,id:2375,x:28333,y:33862,varname:node_2375,prsc:2|A-8150-OUT,B-8868-OUT;n:type:ShaderForge.SFN_Add,id:8868,x:28146,y:33972,varname:node_8868,prsc:2|A-8083-OUT,B-1642-OUT,C-2361-OUT;n:type:ShaderForge.SFN_Multiply,id:1642,x:27961,y:33992,varname:node_1642,prsc:2|A-8806-V,B-1559-OUT;n:type:ShaderForge.SFN_RemapRange,id:1559,x:27759,y:33992,varname:node_1559,prsc:2,frmn:-0.1,frmx:0.1,tomn:0.9,tomx:1.1|IN-8083-OUT;n:type:ShaderForge.SFN_Vector1,id:7451,x:32478,y:34038,varname:node_7451,prsc:2,v1:0;n:type:ShaderForge.SFN_Get,id:6838,x:30654,y:34050,varname:node_6838,prsc:2|IN-9318-OUT;n:type:ShaderForge.SFN_Get,id:3516,x:30641,y:34253,varname:node_3516,prsc:2|IN-8242-OUT;n:type:ShaderForge.SFN_Get,id:6786,x:30641,y:34467,varname:node_6786,prsc:2|IN-2956-OUT;n:type:ShaderForge.SFN_Multiply,id:107,x:29566,y:33823,varname:node_107,prsc:2|A-3974-OUT,B-5621-R;n:type:ShaderForge.SFN_Multiply,id:7774,x:29566,y:33946,varname:node_7774,prsc:2|A-3974-OUT,B-5621-G;n:type:ShaderForge.SFN_Multiply,id:5102,x:29566,y:34058,varname:node_5102,prsc:2|A-3974-OUT,B-5621-B;n:type:ShaderForge.SFN_Power,id:4556,x:29808,y:34056,varname:node_4556,prsc:2|VAL-107-OUT,EXP-7080-OUT;n:type:ShaderForge.SFN_Power,id:5717,x:29796,y:34189,varname:node_5717,prsc:2|VAL-7774-OUT,EXP-7080-OUT;n:type:ShaderForge.SFN_Power,id:1667,x:29796,y:34307,varname:node_1667,prsc:2|VAL-5102-OUT,EXP-7080-OUT;n:type:ShaderForge.SFN_Set,id:8242,x:30005,y:34189,varname:GammaG,prsc:2|IN-5717-OUT;n:type:ShaderForge.SFN_Set,id:2956,x:30005,y:34307,varname:GammaB,prsc:2|IN-1667-OUT;n:type:ShaderForge.SFN_Set,id:9318,x:30005,y:34056,varname:GammaR,prsc:2|IN-4556-OUT;n:type:ShaderForge.SFN_OneMinus,id:5056,x:30870,y:34829,varname:node_5056,prsc:2|IN-8865-OUT;n:type:ShaderForge.SFN_Slider,id:7080,x:29339,y:34215,ptovrint:False,ptlb:Gamma Correction,ptin:_GammaCorrection,varname:node_7080,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:2,max:4;n:type:ShaderForge.SFN_Slider,id:2799,x:31425,y:34314,ptovrint:False,ptlb:Brightness,ptin:_Brightness,varname:node_2799,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:3,max:20;n:type:ShaderForge.SFN_Panner,id:1283,x:28211,y:34727,varname:node_1283,prsc:2,spu:2,spv:3|UVIN-3203-UVOUT;n:type:ShaderForge.SFN_TexCoord,id:3203,x:28021,y:34727,varname:node_3203,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Tex2d,id:5754,x:28586,y:34726,varname:node_5754,prsc:2,ntxv:0,isnm:False|UVIN-1283-UVOUT,TEX-1387-TEX;n:type:ShaderForge.SFN_Multiply,id:7357,x:28869,y:34773,varname:node_7357,prsc:2|A-5754-R,B-2935-OUT;n:type:ShaderForge.SFN_Slider,id:6462,x:28316,y:34910,ptovrint:False,ptlb:Noise,ptin:_Noise,varname:node_6462,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.2,max:1;n:type:ShaderForge.SFN_RemapRange,id:2935,x:28661,y:34912,varname:node_2935,prsc:2,frmn:0,frmx:1,tomn:0,tomx:0.005|IN-6462-OUT;n:type:ShaderForge.SFN_Set,id:6182,x:29071,y:34773,varname:Noise,prsc:2|IN-7357-OUT;n:type:ShaderForge.SFN_Get,id:9963,x:27570,y:33764,varname:node_9963,prsc:2|IN-6182-OUT;n:type:ShaderForge.SFN_Add,id:8150,x:28146,y:33848,varname:node_8150,prsc:2|A-9963-OUT,B-8806-U;n:type:ShaderForge.SFN_Multiply,id:2361,x:27759,y:33714,varname:node_2361,prsc:2|A-3563-OUT,B-9963-OUT;n:type:ShaderForge.SFN_Vector1,id:3563,x:27570,y:33694,varname:node_3563,prsc:2,v1:0.5;proporder:3974-2799-7080-8865-3788-2913-8083-6462-2250-5621-2928-8510-1387;pass:END;sub:END;*/

Shader "Printer/CRT" {
    Properties {
        [MaterialToggle] _ScreenON ("ScreenON", Float ) = 1
        _Brightness ("Brightness", Range(0, 20)) = 3
        _GammaCorrection ("Gamma Correction", Range(1, 4)) = 2
        _ScanlineDarkness ("ScanlineDarkness", Range(0, 1)) = 0.15
        _ScanlineSpeed ("ScanlineSpeed", Range(0, 5)) = 0.3
        _ScanlineThickness ("ScanlineThickness", Range(0, 10)) = 0.95
        _VerticalAdjustment ("Vertical Adjustment", Range(-0.1, 0.1)) = 0
        _Noise ("Noise", Range(0, 1)) = 0.2
        _Gloss ("Gloss", Range(0, 1)) = 0
        [HDR]_MainTex ("Screen", 2D) = "black" {}
        [HDR]_PixelTexture ("PixelTexture", 2D) = "black" {}
        _SpecularText ("SpecularText", 2D) = "gray" {}
        _ScanlineVignette ("ScanlineVignette", 2D) = "black" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Gloss;
            uniform sampler2D _PixelTexture; uniform float4 _PixelTexture_ST;
            uniform float _ScanlineDarkness;
            uniform float _ScanlineSpeed;
            uniform float _ScanlineThickness;
            uniform sampler2D _SpecularText; uniform float4 _SpecularText_ST;
            uniform fixed _ScreenON;
            uniform sampler2D _ScanlineVignette; uniform float4 _ScanlineVignette_ST;
            uniform float _VerticalAdjustment;
            uniform float _GammaCorrection;
            uniform float _Brightness;
            uniform float _Noise;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                UNITY_LIGHT_ATTENUATION(attenuation,i, i.posWorld.xyz);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _Gloss;
                float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMin[0] = unity_SpecCube0_BoxMin;
                    d.boxMin[1] = unity_SpecCube1_BoxMin;
                #endif
                #if UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMax[0] = unity_SpecCube0_BoxMax;
                    d.boxMax[1] = unity_SpecCube1_BoxMax;
                    d.probePosition[0] = unity_SpecCube0_ProbePosition;
                    d.probePosition[1] = unity_SpecCube1_ProbePosition;
                #endif
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float4 _SpecularText_var = tex2D(_SpecularText,TRANSFORM_TEX(i.uv0, _SpecularText));
                float4 node_2856 = tex2D(_ScanlineVignette,TRANSFORM_TEX(i.uv0, _ScanlineVignette));
                float Vignette = (1.0 - node_2856.b);
                float node_2950 = Vignette;
                float node_8370 = saturate((_SpecularText_var.r*node_2950));
                float3 specularColor = float3(node_8370,node_8370,node_8370);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 indirectSpecular = (gi.indirect.specular)*specularColor;
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                float node_7451 = 0.0;
                float3 diffuseColor = float3(node_7451,node_7451,node_7451);
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float4 node_5441 = _Time;
                float2 node_1283 = (i.uv0+node_5441.g*float2(2,3));
                float4 node_5754 = tex2D(_ScanlineVignette,TRANSFORM_TEX(node_1283, _ScanlineVignette));
                float Noise = (node_5754.r*(_Noise*0.005+0.0));
                float node_9963 = Noise;
                float4 node_5389 = _Time;
                float2 node_6594 = (float2(i.uv0.r,(i.uv0.g*_ScanlineThickness))+(node_5389.g*_ScanlineSpeed)*float2(0,1));
                float4 node_3095 = tex2D(_ScanlineVignette,TRANSFORM_TEX(node_6594, _ScanlineVignette));
                float2 node_3363 = (float2((node_9963+i.uv0.r),(_VerticalAdjustment+(i.uv0.g*(_VerticalAdjustment*1.0+1.0))+(0.5*node_9963)))+lerp(0.0,(i.uv0.g*0.001),node_3095.g));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_3363, _MainTex));
                float GammaR = pow((_ScreenON*_MainTex_var.r),_GammaCorrection);
                float4 _PixelTexture_var = tex2D(_PixelTexture,TRANSFORM_TEX(i.uv0, _PixelTexture));
                float GammaG = pow((_ScreenON*_MainTex_var.g),_GammaCorrection);
                float GammaB = pow((_ScreenON*_MainTex_var.b),_GammaCorrection);
                float Scanline = (1.0 - node_3095.g);
                float3 emissive = (node_2950*(saturate((((float3(1,0,0)*GammaR*_PixelTexture_var.r)+(float3(0,1,0)*GammaG*_PixelTexture_var.g)+(float3(0,0,1)*GammaB*_PixelTexture_var.b))*saturate((Scanline+(1.0 - _ScanlineDarkness)))))*_Brightness));
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Gloss;
            uniform sampler2D _PixelTexture; uniform float4 _PixelTexture_ST;
            uniform float _ScanlineDarkness;
            uniform float _ScanlineSpeed;
            uniform float _ScanlineThickness;
            uniform sampler2D _SpecularText; uniform float4 _SpecularText_ST;
            uniform fixed _ScreenON;
            uniform sampler2D _ScanlineVignette; uniform float4 _ScanlineVignette_ST;
            uniform float _VerticalAdjustment;
            uniform float _GammaCorrection;
            uniform float _Brightness;
            uniform float _Noise;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                UNITY_LIGHT_ATTENUATION(attenuation,i, i.posWorld.xyz);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _Gloss;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float4 _SpecularText_var = tex2D(_SpecularText,TRANSFORM_TEX(i.uv0, _SpecularText));
                float4 node_2856 = tex2D(_ScanlineVignette,TRANSFORM_TEX(i.uv0, _ScanlineVignette));
                float Vignette = (1.0 - node_2856.b);
                float node_2950 = Vignette;
                float node_8370 = saturate((_SpecularText_var.r*node_2950));
                float3 specularColor = float3(node_8370,node_8370,node_8370);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float node_7451 = 0.0;
                float3 diffuseColor = float3(node_7451,node_7451,node_7451);
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Gloss;
            uniform sampler2D _PixelTexture; uniform float4 _PixelTexture_ST;
            uniform float _ScanlineDarkness;
            uniform float _ScanlineSpeed;
            uniform float _ScanlineThickness;
            uniform sampler2D _SpecularText; uniform float4 _SpecularText_ST;
            uniform fixed _ScreenON;
            uniform sampler2D _ScanlineVignette; uniform float4 _ScanlineVignette_ST;
            uniform float _VerticalAdjustment;
            uniform float _GammaCorrection;
            uniform float _Brightness;
            uniform float _Noise;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                float4 node_2856 = tex2D(_ScanlineVignette,TRANSFORM_TEX(i.uv0, _ScanlineVignette));
                float Vignette = (1.0 - node_2856.b);
                float node_2950 = Vignette;
                float4 node_8273 = _Time;
                float2 node_1283 = (i.uv0+node_8273.g*float2(2,3));
                float4 node_5754 = tex2D(_ScanlineVignette,TRANSFORM_TEX(node_1283, _ScanlineVignette));
                float Noise = (node_5754.r*(_Noise*0.005+0.0));
                float node_9963 = Noise;
                float4 node_5389 = _Time;
                float2 node_6594 = (float2(i.uv0.r,(i.uv0.g*_ScanlineThickness))+(node_5389.g*_ScanlineSpeed)*float2(0,1));
                float4 node_3095 = tex2D(_ScanlineVignette,TRANSFORM_TEX(node_6594, _ScanlineVignette));
                float2 node_3363 = (float2((node_9963+i.uv0.r),(_VerticalAdjustment+(i.uv0.g*(_VerticalAdjustment*1.0+1.0))+(0.5*node_9963)))+lerp(0.0,(i.uv0.g*0.001),node_3095.g));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_3363, _MainTex));
                float GammaR = pow((_ScreenON*_MainTex_var.r),_GammaCorrection);
                float4 _PixelTexture_var = tex2D(_PixelTexture,TRANSFORM_TEX(i.uv0, _PixelTexture));
                float GammaG = pow((_ScreenON*_MainTex_var.g),_GammaCorrection);
                float GammaB = pow((_ScreenON*_MainTex_var.b),_GammaCorrection);
                float Scanline = (1.0 - node_3095.g);
                o.Emission = (node_2950*(saturate((((float3(1,0,0)*GammaR*_PixelTexture_var.r)+(float3(0,1,0)*GammaG*_PixelTexture_var.g)+(float3(0,0,1)*GammaB*_PixelTexture_var.b))*saturate((Scanline+(1.0 - _ScanlineDarkness)))))*_Brightness));
                
                float node_7451 = 0.0;
                float3 diffColor = float3(node_7451,node_7451,node_7451);
                float4 _SpecularText_var = tex2D(_SpecularText,TRANSFORM_TEX(i.uv0, _SpecularText));
                float node_8370 = saturate((_SpecularText_var.r*node_2950));
                float3 specColor = float3(node_8370,node_8370,node_8370);
                float roughness = 1.0 - _Gloss;
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
