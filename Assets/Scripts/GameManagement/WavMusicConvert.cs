using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;
using System;

public class WavMusicConvert
{

	public static List<AudioClip> musicClips = new List<AudioClip>();
	public static List<string> names = new List<string>();

	public static string ToASCII(byte[] arr, long from, int len)
	{
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < len; i++)
			sb.Append((char)arr[i + from]);

		return sb.ToString();
	}
	public static AudioClip ConvertFromBytes(byte[] arr, string name)
	{
		try
		{
			int formatting = 0;
			int curPos = 0;
			int numChannels = 0;
			long sampleRate = 0;
			long byteRate = 0;
			int blockAlign = 0;
			int bitsPerSample = 0;
			long toRead = 0;
			float[] ffv;
			if (ToASCII(arr, 0, 4).Equals("RIFF"))
			{
				curPos += 4;
				//int toRead = 0;
				//DebugBytes(arr, 4, 4);
				if (BuildInt(arr, 4, 4) != arr.Length - 8 || !ToASCII(arr, 8, 4).Equals("WAVE"))
				{
					throw new System.Exception("Malformed WAVE");
				}
				curPos += 8;
				if (!ToASCII(arr, curPos, 4).Equals("fmt ") || BuildInt(arr, curPos + 4, 4) != 16)
				{
					throw new System.Exception("Malformed WAVE");
				}
				curPos += 8;
				formatting = (int)BuildInt(arr, curPos, 2);

				curPos += 2;
				numChannels = (int)BuildInt(arr, curPos, 2);
				curPos += 2;
				sampleRate = BuildInt(arr, curPos, 4);
				curPos += 4;
				byteRate = BuildInt(arr, curPos, 4);
				curPos += 4;
				blockAlign = (int)BuildInt(arr, curPos, 2);
				curPos += 2;
				bitsPerSample = (int)BuildInt(arr, curPos, 2);
				curPos += 2;
				Debug.Log(formatting + " " + numChannels + " " + sampleRate + " " + byteRate + " " + blockAlign + " " + bitsPerSample);
				if (!ToASCII(arr, curPos, 4).Equals("data"))
				{
					throw new System.Exception("Malformed WAVE");
				}
				curPos += 4;
				toRead = BuildInt(arr, curPos, 4);
				curPos += 4;
				ffv = ReadAllBytes(arr, curPos, toRead, blockAlign, bitsPerSample, numChannels);
				AudioClip ccp = AudioClip.Create(name, ffv.Length, numChannels, (int)sampleRate, false);
				Debug.Log(ccp.SetData(ffv, 0));
				return ccp;
			}
		}
		catch (System.Exception ex)
		{
			Debug.Log("Malformed wave file!" + ex.Message + " \n\n " + ex.StackTrace);
		}
		return null;
	}

	public static void register(AudioClip audioClip, string v)
	{
		musicClips.Add(audioClip);
		names.Add(v);
		Debug.Log("Registered clip " + v);
	}

	private static float[] ReadAllBytes(byte[] arr, long curPos, long toRead, int BA, int BPS, int channels)
	{
		float[] fp = new float[(arr.LongLength - curPos) / (BPS / 8)];
		long maxValue = 1;
		for (int i = 0; i < BPS / 8; i++)
			maxValue *= 256;

		StringBuilder sbs = new StringBuilder();
		for (long l = 0; l < fp.LongLength; l++)
			fp[l] = (BuildInt(arr, curPos + l * BPS / 8, BPS / 8) * 2.0f / maxValue) - 1.0f;

		Debug.Log(sbs.ToString());
		return fp;

	}
	public static void DebugBytes(byte[] arr, int from, int len)
	{
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < len; i++)
			sb.Append((int)arr[i + from] + " ");

		Debug.Log(sb.ToString().TrimEnd());
	}
	public static long BuildInt(byte[] arr, long from, int len)
	{
		long l = 0;
		for (int i = len - 1; i >= 0; i--)
			l = 256 * l + (int)arr[from + i];

		return l;
	}
	public static byte[] ReadBytes(string path)
	{
		return File.ReadAllBytes(path);
	}
}
