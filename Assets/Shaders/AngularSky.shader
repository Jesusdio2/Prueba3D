Shader "Custom/AngularSky"
{
    Properties
    {
        _ColorA ("Color A", Color) = (0.2, 0.6, 1, 1)
        _ColorB ("Color B", Color) = (1, 0.7, 0.3, 1)
        _UseGray ("Usar gris intermedio", Float) = 0
        _Darkness ("Oscurecer hacia negro", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldDir : TEXCOORD0;
            };

            fixed4 _ColorA;
            fixed4 _ColorB;
            float _UseGray;
            float _Darkness;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Convertir posición del vértice a espacio mundo
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // Dirección desde la cámara hacia el vértice
                float3 viewDir = worldPos - _WorldSpaceCameraPos;

                // Normalizar y guardar en TEXCOORD0
                o.worldDir = normalize(viewDir);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Dirección base del degradado (ej. hacia la izquierda)
                float3 baseDir = normalize(float3(-1, 0, 0));

                // Dirección horizontal del fragmento
                float3 horizDir = normalize(float3(i.worldDir.x, 0, i.worldDir.z));

                // Producto punto para obtener alineación
                float t = dot(horizDir, baseDir); // -1 a 1
                t = saturate(t * 0.5 + 0.5);       // convertir a 0 a 1

                fixed4 color;

                if (_UseGray > 0.5)
                {
                    fixed4 gray = fixed4(0.5, 0.5, 0.5, 1);
                    if (t < 0.5)
                        color = lerp(_ColorA, gray, t * 2.0);
                    else
                        color = lerp(gray, _ColorB, (t - 0.5) * 2.0);
                }
                else
                {
                    color = lerp(_ColorA, _ColorB, t);
                }

                // Oscurecer hacia negro
                color = lerp(color, fixed4(0,0,0,1), _Darkness);
                return color;
            }
            ENDCG
        }
    }
}
