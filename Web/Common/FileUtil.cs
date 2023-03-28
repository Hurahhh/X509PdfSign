using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Web.Common
{
    public class FileUtil
    {
        public static bool IsLocked(FileInfo fileInfo)
        {
            try
            {
                using (var stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (Exception)
            {
                return true;
            }

            return false;
        }
    }
}