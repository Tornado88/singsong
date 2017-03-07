using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
//using Boo.Lang;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class DisplayLrc : MonoBehaviour
{
    public Text MusicLrc;
    public Text MusicLrc1;
    public Text MusicLrc2;
    private AudioSource mp3; //音乐
    private Encoding enc; //更改歌词字符编码
    private StreamReader sr; //读取歌词
    public System.Collections.Generic.List<string> strList = new System.Collections.Generic.List<string>();//获取到的歌词
    public System.Collections.Generic.List<float> timList = new System.Collections.Generic.List<float>();//获取到的时间

    
    public string path; //歌词路径



    private void Start()
    {
        mp3 = gameObject.GetComponentInChildren<AudioSource>() as AudioSource;
        path = Application.dataPath + "/Trc/" + mp3.clip.name + ".lrc"; //获取歌词路径，并同步歌词和歌曲名称
        enc = GetEncoding(path, Encoding.GetEncoding("UTF-8"));//GB18030 UTF8
        sr = new StreamReader(path, enc);
        OpenFile(); //打开歌词文件

        mp3.Play();
    }

    //打开歌词文件
    public void OpenFile()
    {
        string oneLine = "";
        try
        {
            while (!sr.EndOfStream)
            {
                oneLine = sr.ReadLine() + "\r\n";
                Replacestr(oneLine);
                Time(oneLine);
            }
            sr.Close();
        }
        catch (Exception e)
        {
            Debug.Log("exception: "+ e.Message);
            sr.Close();
        }
    }

    //去掉除歌词外的其他东西
    public void Replacestr(string inputstr)
    {
        string sPattner = "(\\[)" + "(\\d+)" + "(:)" + "(\\d+)" + "(\\.)" + "(\\d+)" + "(\\])";
        Regex reg = new Regex(sPattner);
        foreach (Match mc in reg.Matches(inputstr))
        {  
            strList.Add(inputstr.Substring(inputstr.IndexOf(mc.Value)+ mc.Value.Length));
        }
    }

    //获取字符串的时间
    public void Time(string t)
    {
        string sPattner = "(\\[)" + "(\\d+)" + "(:)" + "(\\d+)" + "(\\.)" + "(\\d+)" + "(\\])";// "(?<t>\\[\\d.*\\]+)(?<q><[^>]*>)(?<w>[^\\[]+\r\n)";
        Regex reg = new Regex(sPattner);
        foreach (Match mc in reg.Matches(t))
        {
            string[] tmpStrs = mc.Value.Split(new char[] { '[', ':', ']' });
            float tmpf = Convert.ToSingle(tmpStrs[1]) * 60 + Convert.ToSingle(tmpStrs[2]);
            timList.Add(tmpf);
        }
    }


    private int x = 0; //获取的歌词当前的行数
    //匹配时间
    private void SuitedTime()
    {
        float tmpf = mp3.time;
        for (int i = x; i < timList.Count-1; i++)
        {
            if (timList[i] <= tmpf)
            {
                MusicLrc.text = i - 1 >= 0 ? strList[i - 1] : "";
                MusicLrc1.text = strList[i];
                MusicLrc2.text = strList[i + 1];
                x = i + 1;
            }
            else
                break;
        }
    }

    private void Update()
    {
        SuitedTime();//匹配当前时间和歌词
    }

    private static Encoding GetEncoding(string file, Encoding defEnc)
    {
        using (var stream = File.OpenRead(file))
        {
            //判断流可读？  
            if (!stream.CanRead)
                return null;
            //字节数组存储BOM  
            var bom = new byte[4];
            //实际读入的长度  
            int readc;
            readc = stream.Read(bom, 0, 4);
            if (readc >= 2)
            {
                if (readc >= 4)
                {
                    //UTF32，Big-Endian  
                    if (CheckBytes(bom, 4, 0x00, 0x00, 0xFE, 0xFF))
                        return new UTF32Encoding(true, true);

                    //UTF32，Little-Endian  
                    if (CheckBytes(bom, 4, 0xFF, 0xFE, 0x00, 0x00))
                        return new UTF32Encoding(false, true);
                }
                //UTF8  
                if (readc >= 3 && CheckBytes(bom, 3, 0xEF, 0xBB, 0xBF))
                    return new UTF8Encoding(true);
                //UTF16，Big-Endian  
                if (CheckBytes(bom, 2, 0xFE, 0xFF))
                    return new UnicodeEncoding(true, true);

                //UTF16，Little-Endian  
                if (CheckBytes(bom, 2, 0xFF, 0xFE))
                    return new UnicodeEncoding(false, true);
            }
            return defEnc;
        }
    }

    private static bool CheckBytes(byte[] bytes, int count, params int[] values)
    {
        for (int i = 0; i < count; i++)
            if (bytes[i] != values[i])
                return false;
        return true;
    }
}
