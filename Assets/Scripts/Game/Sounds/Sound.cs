using System;
using UnityEngine;

[Serializable]
public struct Sound
{
	public AudioClip clip;
	[Range(0f, 1f)]
	public float volume;
	[Range(0, 256)]
	public int priority;
	public bool loop;
}
