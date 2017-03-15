using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EBookReader.Utils
{
    public class FileHelper
    {
        /// <summary>
        /// 返回文件路径的文件名（保留后缀）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            string _path = path.Replace("/", "\\").Trim();
            return _path.Substring(_path.LastIndexOf('\\') + 1);
        }

        /// <summary>
        /// 返回文件路径的文件名（无后缀）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileNameNoneExt(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            string filename = GetFileName(path);
            if (filename.LastIndexOf(".") <= 0)
                return string.Empty;
            return filename.Substring(0, filename.LastIndexOf("."));
        }
        /// <summary>
        /// 返回文件路径的后缀（无"."）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileExtNonePoint(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            return path.Substring(path.LastIndexOf('.') + 1);
        }

        /// <summary>
        /// 转换为UTF-8文件
        /// </summary>
        /// <param name="filepath">文件路径</param>
        /// <param name="newfilepath">新的UTF-8文件路径</param>
        public static void FileToUTF8(string filepath, string newfilepath)
        {
            //Directory.CreateDirectory(newfilepath);
            //File.Create(newfilepath);
            if (!File.Exists(filepath))
            {
                if (!File.Exists(newfilepath))
                {
                    CreateFile(newfilepath);
                }
                return;
            }
            var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            Encoding encoding = GetEncoding(fs, Encoding.Default);
            StreamReader streamReader;
            if (encoding == Encoding.Default)
            {
                var bytes = new byte[Convert.ToInt32(fs.Length)];
                fs.Read(bytes, 0, bytes.Length);
                if (IsUTF8(bytes))
                {
                    streamReader = new StreamReader(filepath, Encoding.UTF8);
                }
                else
                {
                    streamReader = new StreamReader(filepath, Encoding.Default);
                }
            }
            else
                streamReader = new StreamReader(filepath, encoding);
            if (!File.Exists(newfilepath))
            {
                CreateFile(newfilepath);
            }

            File.WriteAllText(newfilepath, streamReader.ReadToEnd(), Encoding.UTF8);
        }

        /// <summary>
        /// 通过文件流，获取编码(只能判断有BOM头的文件)
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="defaultEncoding">默认的变法格式</param>
        /// <returns>编码格式</returns>
        public static Encoding GetEncoding(FileStream stream, Encoding defaultEncoding)
        {
            Encoding targetEncoding = defaultEncoding;
            if (stream != null && stream.Length >= 2)
            {
                //保存文件流的前4个字节
                byte byte1 = 0;
                byte byte2 = 0;
                byte byte3 = 0;
                byte byte4 = 0;
                //保存当前Seek位置
                long origPos = stream.Seek(0, SeekOrigin.Begin);
                stream.Seek(0, SeekOrigin.Begin);

                int nByte = stream.ReadByte();
                byte1 = Convert.ToByte(nByte);
                byte2 = Convert.ToByte(stream.ReadByte());
                if (stream.Length >= 3)
                {
                    byte3 = Convert.ToByte(stream.ReadByte());
                }
                if (stream.Length >= 4)
                {
                    byte4 = Convert.ToByte(stream.ReadByte());
                }
                //根据文件流的前4个字节判断Encoding
                //Unicode {0xFF, 0xFE};
                //BE-Unicode {0xFE, 0xFF};
                //UTF8 = {0xEF, 0xBB, 0xBF};
                if (byte1 == 0xFE && byte2 == 0xFF)//UnicodeBe
                {
                    targetEncoding = Encoding.BigEndianUnicode;
                }
                if (byte1 == 0xFF && byte2 == 0xFE && byte3 != 0xFF)//Unicode
                {
                    targetEncoding = Encoding.Unicode;
                }
                if (byte1 == 0xEF && byte2 == 0xBB && byte3 == 0xBF)//UTF8
                {
                    targetEncoding = Encoding.UTF8;
                }
                //恢复Seek位置       
                stream.Seek(origPos, SeekOrigin.Begin);
            }
            return targetEncoding;
        }

        /// <summary>
        /// 判断无BOM头的文件是否是UTF8编码
        /// </summary>
        /// <param name="bytes">文件头</param>
        /// <returns>是否为UTF8编码</returns>
        public static bool IsUTF8(byte[] bytes)
        {
            var isUTF8 = true;
            var i = 0;
            while (i < bytes.Length)
            {
                byte b = bytes[i];

                if (b < 0x80) // (10000000): 值小于0x80的为ASCII字符
                {
                    i++;
                }
                else if (b < (0xC0)) // (11000000): 值介于0x80与0xC0之间的为无效UTF-8字符
                {
                    isUTF8 = false;
                    break;
                }
                else if (b < (0xE0)) // (11100000): 此范围内为2字节UTF-8字符
                {
                    if (i >= bytes.Length - 1)
                        break;
                    if ((bytes[i + 1] & (0xC0)) != 0x80)
                    {
                        isUTF8 = false;
                        break;
                    }
                    i += 2;
                }
                else if (b < (0xF0)) // (11110000): 此范围内为3字节UTF-8字符
                {
                    if (i >= bytes.Length - 2)
                        break;
                    if ((bytes[i + 1] & (0xC0)) != 0x80 || (bytes[i + 2] & (0xC0)) != 0x80)
                    {
                        isUTF8 = false;
                        break;
                    }
                    i += 3;
                }
                else
                {
                    isUTF8 = false;
                    break;
                }

            }
            return isUTF8;
        }
        /// <summary>
        /// 创建子文件夹
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <returns></returns>
        public static void CreateFile(string path)
        {
            //非法路径
            if (string.IsNullOrEmpty(path))
                return;
            //文件夹已存在
            if (Directory.Exists(path))
                return;

            try
            {
                CreatFilePath(path);
                var stream = File.Create(path);
                stream.Close();
            }
            catch
            {
            }
        }
        /// <summary>
        /// 创建子文件
        /// </summary>
        /// <param name="path">文件路径</param>
        public static void CreatFilePath(string path)
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
                CreatePath(fileInfo.Directory.FullName);
        }

        /// <summary>
        /// 创建子文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CreatePath(string path)
        {
            //非法路径
            if (string.IsNullOrEmpty(path))
                return false;
            //文件夹已存在
            if (Directory.Exists(path))
                return true;

            string[] split = path.Split(new[] { '/', '\\' });
            string strT = "";
            foreach (var s in split)
            {
                if (String.IsNullOrEmpty(s)) continue;
                strT += s;
                if (!Directory.Exists(strT))
                {
                    if (string.IsNullOrEmpty(strT))
                        continue;
                    //创建文件夹
                    Directory.CreateDirectory(strT);
                }
                strT += "\\";
            }
            return true;
        }

        /// <summary>
        /// MD5函数
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <returns>MD5结果</returns>
        public static string StringToMD5(string str)
        {
            byte[] b = Encoding.UTF8.GetBytes(str);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
                ret += b[i].ToString("x").PadLeft(2, '0');

            return ret;
        }
    }
}
