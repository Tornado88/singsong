﻿using System;
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
    private AudioSource mp3; //音乐
    private Encoding enc; //更改歌词字符编码
    private StreamReader sr; //读取歌词
    public System.Collections.Generic.List<string> strList = new System.Collections.Generic.List<string>();//获取到的歌词
    public System.Collections.Generic.List<string> timList = new System.Collections.Generic.List<string>();//获取到的时间
    public System.Collections.Generic.List<string> strTimList = new System.Collections.Generic.List<string>();//歌词单个字的持续时间
    //private ArrayList[] list;
    public int strindex; //歌词行数
    public int timindex; //歌曲行数
    public int strtimes;//所有歌词单字的持续时间
    public int x = 0; //获取的歌词当前的行数
    public string path; //歌词路径
    public int m_Minutes; //分钟
    public int m_Seconds; //秒


    private void Start()
    {
        mp3 = gameObject.GetComponentInChildren<AudioSource>() as AudioSource;
        path = Application.dataPath + "/Trc/" + mp3.clip.name + ".lrc"; //获取歌词路径，并同步歌词和歌曲名称
        enc = GetEncoding(path, Encoding.GetEncoding("UTF-8"));//GB18030 UTF8
        sr = new StreamReader(path, enc);
        OpenFile(); //打开歌词文件


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
                StrTime(oneLine);
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
        string lrcTxt = null;
        string pattner = @"(?<q><[^>]*>)";//去除歌词中<>字符正则表达式
        string sPattner = "(?<t>\\[\\d.*\\]+)(?<w>[^\\[]+\r\n)";//<t>中文以外的表达式<w>显示的歌词
        Regex reg = new Regex(sPattner);
        foreach (Match mc in reg.Matches(inputstr))
        {
            lrcTxt = mc.Groups["w"].ToString();
            if (lrcTxt != null)//读取当前行不为空
            {
                lrcTxt = Regex.Replace(lrcTxt, pattner, "");//把歌词中的<>去除掉
                //strList[strindex] = lrcTxt;
                strList.Add(lrcTxt);
                strindex++;//下一行
            }
        }
    }

    //获取字符串的时间
    public void Time(string t)
    {
        string c = null;
        string re1 = "(\\[)";   // Any Single Character 1
        string re2 = "(\\d+)";  // Integer Number 1
        string re3 = "(:)"; // Any Single Character 2
        string re4 = "(\\d+)";  // Integer Number 2
        string re5 = "(\\.)";   // Any Single Character 3
        string re6 = "(\\d+)";  // Integer Number 3
        string re7 = "(\\])";	// Any Single Character 4

        string sPattner = re1+re2+re3+re4+re5+re6+re7;// "(?<t>\\[\\d.*\\]+)(?<q><[^>]*>)(?<w>[^\\[]+\r\n)";
        Regex reg = new Regex(sPattner);
        foreach (Match mc in reg.Matches(t))
        {
            c = mc.Groups["t"].ToString() + "\r\n";
            if (c != null)
            {
                //timList[timindex] = c.Substring(1, 5);
                timList.Add(c.Substring(1, 5));
                timindex++;
            }
        }
    }


    //获取字符持续时间
    public void StrTime(string t)
    {
        string b = null;
        string sPattner = @"(?<q><[^>]*)";
        Regex reg = new Regex(sPattner);
        foreach (Match mc in reg.Matches(t))
        {
            b = mc.Groups["q"].ToString() + "\r\n";
            if (b != null)
            {
                //strTimList[strtimes] = b.Substring(1, 5);
                strTimList.Add(b.Substring(1, 5));
                float times = UnityEngine.Time.deltaTime;
                strtimes++;
            }
        }
    }




    //匹配时间
    private void SuitedTime()
    {
        string p = (m_Minutes.ToString("00") + ":" + m_Seconds.ToString("00"));
        //print(p);
        for (int i = x; i < timindex; i++)
        {
            if (timList[i] == p)
            {
                MusicLrc.text = strList[i];
                MusicLrc1.text = strList[i];
                x = i + 1;
                //rollTest._instance.UpdatePlane();
            }
        }
    }

    private void Update()
    {
        m_Minutes = (int)mp3.time / 60;//计算音乐当前播放分钟
        m_Seconds = (int)mp3.time % 60;//计算音乐当前播放秒
        SuitedTime();//匹配时间
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
