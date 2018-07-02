using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OpenCvUtils
{
	public static Mat ConvertFromTexture2d( Texture2D texture2d, int indexX, int indexY, int tileX, int tileY )
	{
		var texWidth = texture2d.width;
		var texHeight = texture2d.height;
		var width = texWidth / tileX;
		var height = texHeight / tileY;
		var pixels = texture2d.GetPixels32();
		var mat = new Mat( height, width, MatType.CV_8UC4 );
	//	Debug.Log( string.Format( "ix:{0} iy:{1}", indexX, indexY ) );
		var indexer = mat.GetGenericIndexer<Vec4b>();
		for( int y = 0; y < height; y++ )
		{
			int py = ( ( ( ( tileY - 1 ) - indexY ) * height ) + y ) * texWidth; 
			for( int x = 0; x < width; x++ )
			{
				int px = ( indexX * width ) + x;
				var srcColor = pixels[ py + px ];
				indexer[ y, x ] = new Vec4b( 255, srcColor.b, srcColor.g, srcColor.r );
			}
		}
		return mat;
	}
	
	public static float ComputeFlowStrength( Mat src )
	{
		var width = src.Width;
		var height = src.Height;
		var srcIndexer = src.GetGenericIndexer<Vec2f>();
		var strength = float.MinValue;
		var aspect = 1;//(float)height / width;// / (float)height;
		for( int y = 0; y < height; y++ )
		{
			for( int x = 0; x < width; x++ )
			{
				var srcColor = srcIndexer[ y, x ];
				var vec = new Vector2( srcColor[ 0 ], srcColor[ 1 ] * aspect );
				if( strength < vec.magnitude )
					strength = vec.magnitude;
			}
		}
		return strength;
	}

	public static Mat ConvertFlowMap( Mat src, float strength, bool isFlipV )
	{
		var width = src.Width;
		var height = src.Height;
		var srcIndexer = src.GetGenericIndexer<Vec2f>();
		var dst = new Mat( height, width, MatType.CV_8UC4 );
		var dstIndexer = dst.GetGenericIndexer<Vec4b>();
		var invStrength = 1.0f / strength;
		var aspect = 1;//(float)height / width;// / (float)height;
		var flipV = ( isFlipV == true ) ? 1 : -1;
		for( int y = 0; y < height; y++ )
		{
			for( int x = 0; x < width; x++ )
			{
				var srcColor = srcIndexer[ y, x ];
				var vec = new Vector2( srcColor[ 0 ], srcColor[ 1 ] * flipV * aspect );
				vec *= invStrength;
				var r = (byte)( Mathf.Clamp01( vec.x * 0.5f + 0.5f ) * 255.0f ); 
				var g = (byte)( Mathf.Clamp01( vec.y * 0.5f + 0.5f ) * 255.0f ); 
				dstIndexer[ y, x ] = new Vec4b( 255, 0, g, r );
			}
		}
		return dst;
	}

	public static Color32[] ConvertToPixels( Mat mat )
	{
		var width = mat.Width;
		var height = mat.Height;
		var pixels = new Color32[ height * width ];
		var indexer = mat.GetGenericIndexer<Vec4b>();
		for( int y = 0; y < height; y++ )
		{
			for( int x = 0; x < width; x++ )
			{
				var srcColor = indexer[y, x];
				pixels[ y * width + x ] = new Color32( srcColor[ 3 ], srcColor[ 2 ], srcColor[ 1 ], srcColor[ 0 ] );
			}
		}
		return pixels;
	}
}
