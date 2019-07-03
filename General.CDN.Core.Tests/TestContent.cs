using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General.CDN.Core.Tests
{
    public class TestContent
    {
        public static string GetContentFilePhysicalPath(string strPath)
        {
            strPath = strPath.TrimStart('/');
            return MapPathLocal("Content/" + strPath);
        }

        public static string GetTempFilePhysicalPath(string strPath)
        {
            strPath = strPath.TrimStart('/');
            return MapPathLocal("Temp/" + strPath);
        }

        private static string MapPathLocal(string strPath)
        {
            //if (System.Web.HttpContext.Current != null)
            //    return System.Web.HttpContext.Current.Server.MapPath(strPath);

            string strAppDirectory = FileServerLocal.GetAppDirectory();
            strAppDirectory = strAppDirectory.Replace("\\netcoreapp2.2", "");
            strAppDirectory = strAppDirectory.Replace("\\Release", "");
            strAppDirectory = strAppDirectory.Replace("\\Debug", "");
            strAppDirectory = strAppDirectory.Replace("\\bin", "");
            strAppDirectory = strAppDirectory.Replace("\\\\", "\\");

            if (!strAppDirectory.EndsWith("\\"))
                strAppDirectory += "\\";

            return System.IO.Path.Combine(strAppDirectory, strPath.Replace("~", string.Empty).Replace('/', '\\'));
        }
    }
}
