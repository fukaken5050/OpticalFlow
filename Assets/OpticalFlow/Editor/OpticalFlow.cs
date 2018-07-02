using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using System.IO;
using UnityEditor;

public class OpticalFlow : ScriptableObject
{
	[SerializeField] private int _count = 0;
	[SerializeField] private int _tileH = 1;
	[SerializeField] private bool _flipV = true;
	[SerializeField] private Texture2D _texture = null;

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	public void OnGUI()
	{
		_texture = EditorGUILayout.ObjectField( "Texture", _texture, typeof(Texture2D), true ) as Texture2D;
		_count = EditorGUILayout.IntField( "Sprite Count", _count );
		_tileH = EditorGUILayout.IntField( "Tile Horizontal", _tileH );
		_flipV = EditorGUILayout.ToggleLeft( "Flip Vertical", _flipV );

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		if ( GUILayout.Button( "Generate" ) )
		{
			Generate();
		}
	}	

	private void Generate()
	{
		if( _count < 2 || _tileH < 1 )
			return;

		var tileV = ( _count / _tileH ) + ( ( ( _count % _tileH ) > 0 ) ? 1 : 0 );
		var flows = new Mat[ _count ];
		var flowf32s = new Mat[ _count - 1 ];

		for( int i = 0; i < _count - 1; i++ )
		{
			var index = i;
			var prevMat = OpenCvUtils.ConvertFromTexture2d( _texture, index % _tileH, index / _tileH, _tileH, tileV );
			index++;
			var nextMat = OpenCvUtils.ConvertFromTexture2d( _texture, index % _tileH, index / _tileH, _tileH, tileV );

			//グレースケール化
			Cv2.CvtColor( prevMat, prevMat, ColorConversionCodes.BGRA2GRAY );
			Cv2.CvtColor( nextMat, nextMat, ColorConversionCodes.BGRA2GRAY );

			//オプティカルフロー計算
			var flow = new Mat();
			Cv2.CalcOpticalFlowFarneback( prev:prevMat, next:nextMat, flow:flow, pyrScale:0.5, levels:3, winsize:10, iterations:3,  polyN:3, polySigma:1.1, flags:OpticalFlowFlags.FarnebackGaussian );//flags:OpticalFlowFlags.FarnebackGaussian );//0.8, 10, 15, 3, 5, 1.1, 0 );
			flowf32s[ i ] = flow;
		}

		var flowStrength = 0.0f;
		for( int i = 0; i < _count - 1; i++ )
		{
			var strength = OpenCvUtils.ComputeFlowStrength( flowf32s[ i ] );
			if( flowStrength < strength )
				flowStrength = strength;
		}
		Debug.Log( "FlowStrength:" + flowStrength );

		for( int i = 0; i < _count - 1; i++ )
		{
			//F32からU8へ
			flows[ i ] = OpenCvUtils.ConvertFlowMap( flowf32s[ i ], flowStrength, _flipV );
		}

		var flowMap = new Texture2D( _texture.width, _texture.height, TextureFormat.ARGB32, false, true );
		int width = (int)( _texture.width / _tileH );
		int height = (int)( _texture.height / tileV );

		for( int i = 0; i < _count - 1; i++ )
		{
			int ox = ( i % _tileH ) * width;
			int oy = ( ( tileV - 1 ) - ( i / _tileH ) )  * height;
			
			flowMap.SetPixels32( ox, oy, width, height, OpenCvUtils.ConvertToPixels( flows[ i ] ), 0 );
		}
		//ループしない用なので最後にベクトルが移動しないのを入れる
		var vzPixels = new Color32[ height * width ];
		for( int i = 0; i < vzPixels.Length; i++ )
			vzPixels[ i ] = new Color32( 127, 127, 0, 255 );
		flowMap.SetPixels32( ( _tileH - 1 ) * width, 0, width, height, vzPixels, 0 );
		flowMap.Apply( false );

		var path = "/" + _texture.name + "_flow.png";
		var bytes = flowMap.EncodeToPNG();
		File.WriteAllBytes( Application.dataPath + path, bytes );
		AssetDatabase.ImportAsset( "Assets" + path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport );
		AssetDatabase.Refresh();

		//不随パラメータ保存
		var paramPath = "Assets/" + _texture.name + "_flowparam.asset";
		var param = AssetDatabase.LoadAssetAtPath<OpticalFlowParam>( paramPath );
		if( param == null )
		{
			param = ScriptableObject.CreateInstance<OpticalFlowParam>();
			AssetDatabase.CreateAsset( param, paramPath );
			AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive );
		}
		param.flowStrength = flowStrength;
		param.textureMain = _texture;
		param.textureFlow = AssetDatabase.LoadAssetAtPath<Texture2D>( "Assets" + path );
		param.count = _count;
		param.tile.Set( _tileH, tileV );

		EditorUtility.SetDirty( param );
		AssetDatabase.ImportAsset( paramPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive );
	}
}
