using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EBookReader.Utils
{
    public class WebHelper
    {

        /// <summary>  
        /// 获取网页的HTML码  
        /// </summary>  
        /// <param name="url">链接地址</param>  
        /// <param name="encoding">编码类型</param>  
        /// <returns></returns>  
        public static string GetHtmlStr(string url, Encoding encoding = null)
        {
            var htmlStr = string.Empty;
            try
            {
                if (!String.IsNullOrEmpty(url))
                {
                    WebRequest request = WebRequest.Create(url);            //实例化WebRequest对象  
                    using (WebResponse response = request.GetResponse()) //创建WebResponse对象  
                    {
                        using (Stream datastream = response.GetResponseStream()) //创建流对象 
                        {
                            if (datastream != null)
                            {
                                if (encoding == null)
                                {
                                    encoding = Encoding.Default;
                                }
                                using (StreamReader reader = new StreamReader(datastream, encoding))
                                {
                                    var str = reader.ReadToEnd();
                                    var bytes = encoding.GetBytes(str);
                                    htmlStr = GetText(bytes);
                                    //读取网页内容  
                                    reader.Close();
                                }                 //读取网页内容 
                                datastream.Close();
                            }
                        }
                        response.Close();
                    }
                }
            }
            catch { }
            return htmlStr.ToLower();
        }

        public static string GetText(byte[] buff)
        {
            string strReslut = string.Empty;
            if (buff.Length > 3)
            {
                if (buff[0] == 239 && buff[1] == 187 && buff[2] == 191)
                {// utf-8
                    strReslut = Encoding.UTF8.GetString(buff);
                }
                else if (buff[0] == 254 && buff[1] == 255)
                {// big endian unicode
                    strReslut = Encoding.BigEndianUnicode.GetString(buff);
                }
                else if (buff[0] == 255 && buff[1] == 254)
                {// unicode
                    strReslut = Encoding.Unicode.GetString(buff);
                }
                else if (isUtf8(buff))
                {// utf-8
                    strReslut = Encoding.UTF8.GetString(buff);
                }
                else
                {// ansi
                    strReslut = Encoding.Default.GetString(buff);
                }
            }
            return strReslut;
        }
        // 110XXXXX, 10XXXXXX
        // 1110XXXX, 10XXXXXX, 10XXXXXX
        // 11110XXX, 10XXXXXX, 10XXXXXX, 10XXXXXX
        private static bool isUtf8(byte[] buff)
        {
            for (int i = 0; i < buff.Length; i++)
            {
                if ((buff[i] & 0xE0) == 0xC0) // 110x xxxx 10xx xxxx
                {
                    if ((buff[i + 1] & 0x80) != 0x80)
                    {
                        return false;
                    }
                }
                else if ((buff[i] & 0xF0) == 0xE0) // 1110 xxxx 10xx xxxx 10xx xxxx
                {
                    if ((buff[i + 1] & 0x80) != 0x80 || (buff[i + 2] & 0x80) != 0x80)
                    {
                        return false;
                    }
                }
                else if ((buff[i] & 0xF8) == 0xF0) // 1111 0xxx 10xx xxxx 10xx xxxx 10xx xxxx
                {
                    if ((buff[i + 1] & 0x80) != 0x80 || (buff[i + 2] & 0x80) != 0x80 || (buff[i + 3] & 0x80) != 0x80)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //判断是否是GB2312 
        //bool isGBCode(const string& strIn) 
        //{ 
        //unsigned char ch1; 
        //unsigned char ch2; 

        //if (strIn.size() >= 2) 
        //{ 
        //ch1 = (unsigned char)strIn.at(0); 
        //ch2 = (unsigned char)strIn.at(1); 
        //if (ch1 >=176 && ch1 <=247 &&ch2 >=160 && ch2 <=254) 
        //return true; 
        //else return false; 
        //} 
        //else return false; 
        //} 

        ////判断是否是GBK编码 
        //bool isGBKCode(const string& strIn) 
        //{ 
        //unsigned char ch1; 
        //unsigned char ch2; 

        //if (strIn.size() >= 2) 
        //{ 
        //ch1 = (unsigned char)strIn.at(0); 
        //ch2 = (unsigned char)strIn.at(1); 
        //if (ch1 >=129 && ch1 <=254 &&ch2 >=64 && ch2 <=254) 
        //return true; 
        //else return false; 
        //} 
        //else return false; 
        //}  
    }
}
