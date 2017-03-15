using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EBookReader.Utils
{
    public class IniUtils
    {
        // 声明INI文件的写操作函数 WritePrivateProfileString()
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);
        //获取ini文件所有的section  
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileSectionNamesA(byte[] buffer, int iLen, string fileName); 

        /// <summary>
        /// 写入ini文档
        /// </summary>
        /// <param name="path">ini文档路径</param>
        /// <param name="section">片段</param>
        /// <param name="key">关键字</param>
        /// <param name="value">值</param>
        public static void Write(string path, string section, string key, string value)
        {
            // section=配置节，key=键名，value=键值，path=路径
            WritePrivateProfileString(section, key, value, path);
        }
        /// <summary>
        /// 读ini文件
        /// </summary>
        /// <param name="path">ini文档路径</param>
        /// <param name="section">片段</param>
        /// <param name="key">关键字</param>
        /// <returns>值</returns>
        public static string Read(string path, string section, string key)
        {
            // 每次从ini中读取多少字节
            var temp = new System.Text.StringBuilder(255);
            // section=配置节，key=键名，temp=上面，path=路径
            GetPrivateProfileString(section, key, "", temp, 255, path);
            return temp.ToString();
        }
        /// <summary>
        /// 读ini文件
        /// </summary>
        /// <param name="path">ini文档路径</param>
        /// <param name="section">片段</param>
        /// <param name="key">关键字</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>值</returns>
        public static string Read(string path, string section, string key, string defaultValue = "")
        {
            // 每次从ini中读取多少字节
            var temp = new System.Text.StringBuilder(255);
            // section=配置节，key=键名，temp=上面，path=路径
            GetPrivateProfileString(section, key, defaultValue, temp, 255, path);
            return temp.ToString();
        }

        /// <summary>  
        /// 返回该配置文件中所有Section名称的集合  
        /// </summary>  
        /// <returns></returns>  
        public static List<string> ReadSections(string path)
        {
            var buffer = new byte[65535];
            int rel = GetPrivateProfileSectionNamesA(buffer, buffer.GetUpperBound(0), path);
            var list = new List<string>();
            if (rel > 0)
            { 
                var iPos = 0;
                for (var iCnt = 0; iCnt < rel; iCnt++)
                {
                    if (buffer[iCnt] == 0x00)
                    {
                        var tmp = System.Text.Encoding.Default.GetString(buffer, iPos, iCnt - iPos).Trim();
                        iPos = iCnt + 1;
                        if (tmp != "")
                            list.Add(tmp);
                    }
                }
            }
            return list;
        }  
    }
}
