using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Collections;

namespace BabySmash
{
    public class Winmm
    {
        public const UInt32 SND_ASYNC = 1;
        public const UInt32 SND_MEMORY = 4;

        // this is the overload we want to play embedded resource...
        [DllImport("Winmm.dll")]
        public static extern bool PlaySound(byte[] data, IntPtr hMod, UInt32 dwFlags);

        public static Dictionary<string, byte[]> cachedWavs = new Dictionary<string, byte[]>();
        public static object cachedWavsLock = new object();
        
        public static void PlayWavResource(string wav)
        {
            byte[] b = GetWavResource(wav);
            PlaySound(b, IntPtr.Zero, SND_ASYNC | SND_MEMORY);
        }

        private static byte[] GetWavResource(string wav)
        {
            //TODO: Is this valid double-check caching?
            byte[] b = null;
            if(cachedWavs.ContainsKey(wav))
                 b = cachedWavs[wav];
            if (b == null)
            {
                lock (cachedWavsLock)
                {
                    // get the namespace 
                    string strNameSpace = Assembly.GetExecutingAssembly().GetName().Name.ToString();

                    // get the resource into a stream
                    using (Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream(strNameSpace + wav))
                    {
                        if (str == null)
                            throw new ArgumentException(wav + " not found!");
                        // bring stream into a byte array
                        byte[] bStr = new Byte[str.Length];
                        str.Read(bStr, 0, (int)str.Length);
                        cachedWavs.Add(wav, bStr);
                        return bStr;
                    }

                }
            }
            return b;
        }
    }
} 
