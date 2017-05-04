using System;
using System.Collections.Generic;
using System.Text;

namespace AIOAPI
{
    public class IniReader
    {
        [System.Runtime.InteropServices.DllImport("kernel32")]

        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        // 声明INI文件的读操作函数 GetPrivateProfileString()

        [System.Runtime.InteropServices.DllImport("kernel32")]

        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);

        private string sPath = null;

        public IniReader(string path)
        {
            this.sPath = path;
        }

        public string ReadNode(string section, string key)
        {
            StringBuilder key_value = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", key_value, 255, sPath);
            return key_value.ToString();
        }
    }
}
