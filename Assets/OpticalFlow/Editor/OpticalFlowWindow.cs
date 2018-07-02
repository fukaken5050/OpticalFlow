using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using System.IO;
using UnityEditor;

public class OpticalFlowWindow : EditorWindow
{
	private OpticalFlow _obj = null;

	[MenuItem( "Tools/OpticalFlow" )]
	public static void Create()
	{
		var window = GetWindow<OpticalFlowWindow>( "OpticalFlowWindow" );
		window.Show();
	}

	private void OnEnable()
	{
		var ms = MonoScript.FromScriptableObject( this );
		var path = AssetDatabase.GetAssetPath( ms );
		path = path.Replace( Path.GetFileName( path ), "" );
		path = path + "OpticalFlow.asset";
		_obj = AssetDatabase.LoadAssetAtPath<OpticalFlow>( path );
		if( _obj == null )
		{
			_obj = ScriptableObject.CreateInstance<OpticalFlow>();
			AssetDatabase.CreateAsset( _obj, path );
			AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive );
		}
	}

	private void OnDisable()
	{
		if( _obj == null )
			return;
		EditorUtility.SetDirty( _obj );
		AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath( _obj ), ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive );
	}

	private void OnGUI()
	{
		_obj.OnGUI();
	}
}
