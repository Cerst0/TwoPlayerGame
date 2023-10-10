Shader "Unlit/GradientUI"
{
    Properties
    {
        _BlendColor ("BlendColor", Color) = (0 , 0 , 0 , 1)
        _OldColor ("OldColor", Color) = (1, 1, 1, 1)
		_Progress ("Progress", float) = 0
		_MaxRange ("MaxBlendRange", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			fixed4 _OldColor;
			fixed4 _BlendColor;
			fixed _Progress;
			fixed _MaxRange;

            #include "UnityCG.cginc"

            struct Meshdata
            {
                fixed4 vertex : POSITION;
                fixed2 uv : TEXCOORD0;
            };

            struct v2f
            {
                fixed2 uv : TEXCOORD0;
                fixed4 vertex : SV_POSITION;
            };

            v2f vert (Meshdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = (0, 0, 0, 1);
				fixed cord = i.uv.y;
				fixed blendWidth = _Progress;
				fixed2 blendRange;
                fixed progress = _Progress;
				
				if(cord > 0.5){ cord -= .5f;}
                    else{cord = .5 - cord;}
				cord *= 2;

				if(_Progress > _MaxRange)
                {
                    blendWidth = _MaxRange;
                }
				
				progress = saturate(_Progress - blendWidth);

				blendRange = fixed2(progress, progress + blendWidth);
				
				bool doLerp = (cord >= blendRange.x && cord <= blendRange.y);
				
				if(doLerp)
                {
					float t = (cord - blendRange.x) / blendWidth;
                    col = lerp (_BlendColor, _OldColor, t);
                    //col = float4(1, 1, 1, 1);
                }
                else{
					if(cord < blendRange.x)
                    {
						col = _BlendColor;
                    }
					else if ( cord > blendRange.y){ col = _OldColor; }
                }
				
                return col; 
            }
            ENDCG
        }
    }
}
