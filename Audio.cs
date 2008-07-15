using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;

namespace BabySmash
{
    public class Win32Audio 
    {
        public const UInt32 SND_ASYNC = 0x0001;
        public const UInt32 SND_LOOP = 0x0008;
        public const UInt32 SND_MEMORY = 0x0004;
        public const UInt32 SND_NOSTOP = 0x0010;


        // this is the overload we want to play embedded resource...

        public static Dictionary<string, string> cachedWavs = new Dictionary<string, string>();
        public static object cachedWavsLock = new object();


        [DllImport("winmm.dll")]
        public static extern bool PlaySound(byte[] data, IntPtr hMod, UInt32 dwFlags);

[DllImport("winmm.dll", SetLastError = true)]
static extern bool PlaySound(string pszSound, IntPtr hmod, UInt32 fdwSound);

public void PlayWavResource(string wav)
{
    string s = GetWavResource(wav);
    PlaySound(s, IntPtr.Zero, SND_ASYNC);
}

public void PlayWavResourceYield(string wav)
{
    string s = GetWavResource(wav);
    PlaySound(s, IntPtr.Zero, SND_ASYNC | SND_NOSTOP);
}

TempFileCollection tempFiles = new TempFileCollection();

private string GetWavResource(string wav)
{
    //TODO: Is this valid double-check caching?
    string retVal = null;
    if (cachedWavs.ContainsKey(wav))
        retVal = cachedWavs[wav];
    if (retVal == null)
    {
        lock (cachedWavsLock)
        {
            // get the namespace 
            string strNameSpace = Assembly.GetExecutingAssembly().GetName().Name;

            // get the resource into a stream
            using (Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream(strNameSpace + wav))
            {
                string tempfile = System.IO.Path.GetTempFileName();
                tempFiles.AddFile(tempfile,false);
                var bStr = new Byte[str.Length];
                str.Read(bStr, 0, (int)str.Length);
                File.WriteAllBytes(tempfile, bStr);
                cachedWavs.Add(wav, tempfile);
                return tempfile;
            }
        }
    }
    return retVal;
}

        //private static byte[] GetWavResource(string wav)
        //{
        //    //TODO: Is this valid double-check caching?
        //    byte[] b = null;
        //    if (cachedWavs.ContainsKey(wav))
        //        b = cachedWavs[wav];
        //    if (b == null)
        //    {
        //        lock (cachedWavsLock)
        //        {
        //            // get the namespace 
        //            string strNameSpace = Assembly.GetExecutingAssembly().GetName().Name;

        //            // get the resource into a stream
        //            using (Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream(strNameSpace + wav))
        //            {
        //                if (str == null)
        //                    throw new ArgumentException(wav + " not found!");
        //                // bring stream into a byte array
        //                var bStr = new Byte[str.Length];
        //                str.Read(bStr, 0, (int) str.Length);
        //                cachedWavs.Add(wav, bStr);
        //                return bStr;
        //            }
        //        }
        //    }
        //    return b;
        //}

    }
}