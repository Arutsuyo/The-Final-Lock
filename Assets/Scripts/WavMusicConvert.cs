using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;

public class WavMusicConvert
{
    public static string ToASCII(byte[] arr, int from, int len)
    {
        StringBuilder sb = new StringBuilder();
        for(int i = 0; i < len; i++)
        {
            sb.Append((char)arr[i + from]);
        }
        return sb.ToString();
    }
    public static void ConvertFromBytes(byte[] arr)
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
                if (!ToASCII(arr, curPos, 4).Equals("fmt ") || BuildInt(arr, curPos+4, 4) != 16)
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
                if(!ToASCII(arr, curPos, 4).Equals("data"))
                {
                    throw new System.Exception("Malformed WAVE");
                }
                curPos += 4;
                toRead = BuildInt(arr, curPos, 4);
                curPos += 4;

            }
        }
        catch (System.Exception)
        {
            Debug.Log("Malformed wave file!");
        }
    }
    public static void DebugBytes(byte[] arr, int from, int len)
    {
        StringBuilder sb = new StringBuilder();
        for(int i = 0; i < len; i++)
        {
            sb.Append((int)arr[i + from] + " ");
        }
        Debug.Log(sb.ToString().TrimEnd());
    }
    public static long BuildInt(byte[] arr, int from, int len)
    {
        long l = 0;
        for(int i = len - 1; i >= 0; i--)
        {
            l = 256 * l + (int)arr[from + i];
        }
        return l;
    }
    public static byte[] ReadBytes(string path)
    {
        return File.ReadAllBytes(path);
    }
}
