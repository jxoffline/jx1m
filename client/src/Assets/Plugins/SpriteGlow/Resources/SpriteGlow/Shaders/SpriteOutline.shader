// Draws an HDR outline over the sprite borders. 
// Based on Sprites/Default shader from Unity 2017.3.

Shader "Sprites/Outline"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0

        [HDR] _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _AlphaThreshold("Alpha Threshold", Range(0, 1)) = 0.01
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM

                #include "UnityCG.cginc"

                #pragma vertex ComputeVertex
                #pragma fragment ComputeFragment
                #pragma target 2.0
                #pragma multi_compile_instancing
                #pragma multi_compile_local _ PIXELSNAP_ON
                #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
                #pragma multi_compile _ SPRITE_OUTLINE_OUTSIDE

                #ifndef SAMPLE_DEPTH_LIMIT
                #define SAMPLE_DEPTH_LIMIT 10
                #endif

                #ifdef UNITY_INSTANCING_ENABLED

                UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
                UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
                UNITY_INSTANCING_BUFFER_END(PerDrawSprite)
                #define _RendererColor UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
                #define _Flip UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)

                UNITY_INSTANCING_BUFFER_START(PerDrawSpriteOutline)
                UNITY_DEFINE_INSTANCED_PROP(float,  _IsOutlineEnabledArray)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _OutlineColorArray)
                UNITY_DEFINE_INSTANCED_PROP(float,  _OutlineSizeArray)
                UNITY_DEFINE_INSTANCED_PROP(float,  _AlphaThresholdArray)
                UNITY_INSTANCING_BUFFER_END(PerDrawSpriteOutline)
                #define _IsOutlineEnabled UNITY_ACCESS_INSTANCED_PROP(PerDrawSpriteOutline, _IsOutlineEnabledArray)
                #define _OutlineColor UNITY_ACCESS_INSTANCED_PROP(PerDrawSpriteOutline, _OutlineColorArray)
                #define _AlphaThreshold UNITY_ACCESS_INSTANCED_PROP(PerDrawSpriteOutline, _AlphaThresholdArray)

                #endif 

                CBUFFER_START(UnityPerDrawSprite)
                #ifndef UNITY_INSTANCING_ENABLED
                fixed4 _RendererColor;
                fixed2 _Flip;
                #endif
                float _EnableExternalAlpha;
                CBUFFER_END

                CBUFFER_START(UnityPerDrawSpriteOutline)
                #ifndef UNITY_INSTANCING_ENABLED
                fixed4 _OutlineColor;
                float _IsOutlineEnabled, _AlphaThreshold;
                #endif
                CBUFFER_END

                sampler2D _MainTex, _AlphaTex;
                float4 _MainTex_TexelSize;
                fixed4 _Color;

                struct VertexInput
                {
                    float4 Vertex : POSITION;
                    float4 Color : COLOR;
                    float2 TexCoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct VertexOutput
                {
                    float4 Vertex : SV_POSITION;
                    fixed4 Color : COLOR;
                    float2 TexCoord : TEXCOORD0;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
                {
                    return float4(pos.xy * flip, pos.z, 1.0);
                }

                VertexOutput ComputeVertex(VertexInput vertexInput)
                {
                    VertexOutput vertexOutput;

                    UNITY_SETUP_INSTANCE_ID(vertexInput);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);

                    vertexOutput.Vertex = UnityFlipSprite(vertexInput.Vertex, _Flip);
                    vertexOutput.Vertex = UnityObjectToClipPos(vertexInput.Vertex);
                    vertexOutput.TexCoord = vertexInput.TexCoord;
                    vertexOutput.Color = vertexInput.Color * _Color * _RendererColor;

                    #ifdef PIXELSNAP_ON
                    vertexOutput.Vertex = UnityPixelSnap(vertexOutput.Vertex);
                    #endif

                    return vertexOutput;
                }

                fixed4 SampleSpriteTexture(float2 uv)
                {
                    fixed4 color = tex2D(_MainTex, uv);

                    #if ETC1_EXTERNAL_ALPHA
                    fixed4 alpha = tex2D(_AlphaTex, uv);
                    color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
                    #endif

                    return color;
                }

                fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
                {
                    fixed4 color = SampleSpriteTexture(vertexOutput.TexCoord) * vertexOutput.Color;
                    color.rgb *= color.a;

                    /// CORE GLOW
                    fixed4 emission;
                    emission = tex2D(_MainTex, vertexOutput.TexCoord);
                    emission = color;
                    emission.rgb *= emission.a * color.a * _AlphaThreshold * _OutlineColor * _OutlineColor.a / 100.0;
                    color.rgb += emission.rgb;
                    /// END

                    return color;
                }

                ENDCG
            }
        }
}
