using System;
using System.IO;

namespace FaceCheck.Server.Helper
{
    public class Util
    {
        private static readonly string WWWDir = "wwwroot";
        private static readonly string RootDir = "Photo";
        private static readonly string TempRootDir = "PhotoTemp";

        /// <summary>
        /// </summary>
        public static string SavePic(byte[] buffer, bool isPng = false)
        {
            if (!Directory.Exists(Path.Combine(WWWDir, RootDir)))
            {
                Directory.CreateDirectory(Path.Combine(WWWDir, RootDir));
            }

            string picPath = Path.Combine(Path.Combine(WWWDir, RootDir), DateTime.Now.ToString("yyyy")
                , DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"));
            return Save(buffer, isPng, ref picPath);
        }

        private static string Save(byte[] buffer, bool isPng, ref string picPath)
        {
            if (buffer != null
                && buffer.Length > 0)
            {
                if (!Directory.Exists(picPath))
                {
                    Directory.CreateDirectory(picPath);
                }
                string ext;
                if (isPng)
                {
                    ext = ".png";
                }
                else
                {
                    ext = buffer.GetPicExtention();
                }

                picPath = Path.Combine(picPath, Guid.NewGuid().ToString("N") + ext);

                File.WriteAllBytes(picPath, buffer);
                return picPath;
            }
            return string.Empty;
        }

        /// <summary>
        /// </summary>
        /// <param name="buffer"> </param>
        /// <returns> </returns>
        public static string SaveTempPic(byte[] buffer)
        {
            if (!Directory.Exists(Path.Combine(WWWDir, TempRootDir)))
            {
                Directory.CreateDirectory(Path.Combine(WWWDir, TempRootDir));
            }

            string picPath = Path.Combine(Path.Combine(WWWDir, TempRootDir), DateTime.Now.ToString("yyyy")
                , DateTime.Now.ToString("MM"), DateTime.Now.ToString("dd"));
            return Save(buffer, false, ref picPath);
        }
    }
}