using UnityEngine;
using UnityEditor;

public sealed class OpticalFlowImporter : AssetPostprocessor
{
	public override int GetPostprocessOrder()
	{
		return 0;
	}

	void OnPreprocessTexture()
	{
		TextureImporter textureImporter = assetImporter as TextureImporter;

		if( textureImporter.assetPath.Contains( "_flow") == false )
			return;

		textureImporter.wrapMode = TextureWrapMode.Clamp;
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.sRGBTexture = false;
		textureImporter.mipmapEnabled = false;
	}
}
