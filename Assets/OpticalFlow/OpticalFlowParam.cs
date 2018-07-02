using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpticalFlowParam : ScriptableObject
{
	public float flowStrength;
	public Texture2D textureMain;
	public Texture2D textureFlow;
	public int count;
	public Vector2 tile;
}
