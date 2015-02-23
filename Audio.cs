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
        /// <summary>
        /// Collection of WAV resources we've stored on disk. The finalizer of the TempFileCollection will
        /// attempt to delete these files for us.
        /// </summary>
        TempFileCollection tempFiles = new TempFileCollection();

        /// <summary>
        /// Collection of soundname->filename mappings
        /// </summary>
        public static Dictionary<string, string> cachedWavs = new Dictionary<string, string>();

        /// <summary>
        /// Lock this object to protect against concurrent writes to the cachedWavs collection.
        /// </summary>
        public static object cachedWavsLock = new object();

        #region NativeAPI
        private const UInt32 SND_ASYNC = 0x0001;
        private const UInt32 SND_LOOP = 0x0008;
        private const UInt32 SND_NOSTOP = 0x0010;

        [DllImport("winmm.dll")]
        private static extern bool PlaySound(byte[] data, IntPtr hMod, UInt32 dwFlags);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern bool PlaySound(string pszSound, IntPtr hmod, UInt32 fdwSound);
        #endregion NativeAPI

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

        private string GetWavResource(string wav)
        {
            if (cachedWavs.ContainsKey(wav))
            {
                return cachedWavs[wav];
            }

            lock (cachedWavsLock)
            {
                // Recheck inside the lock.
                if (cachedWavs.ContainsKey(wav))
                {
                    return cachedWavs[wav];
                }

                string strName = Assembly.GetExecutingAssembly().GetName().Name + wav;

                // get the resource into a stream
                using (Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream(strName))
                {
                    string tempfile = System.IO.Path.GetTempFileName();
                    tempFiles.AddFile(tempfile, false);
                    var arrWav = new Byte[strm.Length];
                    strm.Read(arrWav, 0, (int)strm.Length);
                    File.WriteAllBytes(tempfile, arrWav);
                    cachedWavs.Add(wav, tempfile);
                    return tempfile;
                }
            }
        }
    }

}