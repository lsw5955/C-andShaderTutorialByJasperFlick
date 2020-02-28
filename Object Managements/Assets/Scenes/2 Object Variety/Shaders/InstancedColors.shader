//指定为材质资源设置Shader时, 可以找到该Shader的选项菜单位置
Shader "Custom/InstancedColors" {
	Properties{
		//这些东西就对应了材质的属性, 左边双引号中的字符串就是暴露在材质资源Inspector中的属性名称, 
		//没错, 你可能想到了, 所谓材质material, 本质上就像是Shader的实例, Shader定义了材质, 材质需要关联一个Shader
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200
			CGPROGRAM
			// 基于物理的标准照明模型，并在所有灯光类型上启用阴影
			#pragma surface surf Standard fullforwardshadows
			//对均衡缩放的对象添加该指令, 会让实例化过程更有效率, 因为它需要的数据更少
			#pragma instancing_options assumeuniformscaling
			#pragma target 3.0
			sampler2D _MainTex;
			struct Input {
		   float2 uv_MainTex;
			};
			half _Glossiness;
			half _Metallic;
			UNITY_INSTANCING_BUFFER_START(Props)
		   UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			UNITY_INSTANCING_BUFFER_END(Props)
			void surf(Input IN, inout SurfaceOutputStandard o) {
		   fixed4 c = tex2D(_MainTex, IN.uv_MainTex) *
			UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
		   o.Albedo = c.rgb;
		   o.Metallic = _Metallic;
		   o.Smoothness = _Glossiness;
		   o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}