using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestDraw : MonoBehaviour
{
	[SerializeField] private OpticalFlowParam _param = null;
	[SerializeField] private float _speed = 1.0f;
	[SerializeField] private RawImage[] _rawImages = null;

	private int ShaderIdDivide = Shader.PropertyToID( "_Divide" );
	private int ShaderIdIndex = Shader.PropertyToID( "_Index" );
	private int ShaderIdBlendWeight = Shader.PropertyToID( "_BlendWeight" );
	private int shaderIdFlowStrength = Shader.PropertyToID( "_FlowStrength" );
	//private int shaderIdMainTexture = Shader.PropertyToID( "_MainTex" );
	private int shaderIdFlowTexture = Shader.PropertyToID( "_FlowTex" );

	private IEnumerator Start()
	{
		var time = 0.0f;

		_rawImages[ 0 ].texture = _param.textureMain;
		var material0 = Instantiate<Material>( _rawImages[ 0 ].material );
		material0.EnableKeyword( "_FRAGMENT_FLOW" );
		material0.SetVector( ShaderIdDivide, new Vector4( _param.tile.x, _param.tile.y, 1.0f / _param.tile.x, 1.0f / _param.tile.y ) );
	//	material0.SetTexture( shaderIdMainTexture, _param.textureMain );
		material0.SetTexture( shaderIdFlowTexture, _param.textureFlow );
		var flowStrength = _param.flowStrength / _rawImages[ 0 ].texture.width;
		Debug.Log( string.Format( "Shader flowStrength:{0}", flowStrength ) );
		material0.SetFloat( shaderIdFlowStrength, flowStrength );
		_rawImages[ 0 ].material = material0;

		_rawImages[ 1 ].texture = _param.textureMain;
		var material1 = Instantiate<Material>( _rawImages[ 1 ].material );
		material1.EnableKeyword( "_FRAGMENT_LINEAR" );
		material1.SetVector( ShaderIdDivide, new Vector4( _param.tile.x, _param.tile.y, 1.0f / _param.tile.x, 1.0f / _param.tile.y ) );
	//	material1.SetTexture( shaderIdMainTexture, _param.textureMain );
		_rawImages[ 1 ].material = material1;
		
		while( true )
		{
			material0.SetInt( ShaderIdIndex, (int)time );
			material0.SetFloat( ShaderIdBlendWeight, time - (int)time );

			material1.SetInt( ShaderIdIndex, (int)time );
			material1.SetFloat( ShaderIdBlendWeight, time - (int)time );
			
			time += Time.deltaTime * _speed;
			if( (int)time >= _param.count - 1 )
				time -= (float)( _param.count - 1 );
			yield return null;
		}
		//yield break;
	}
}
