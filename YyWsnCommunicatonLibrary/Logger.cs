using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace YyWsnCommunicatonLibrary
{
    public class Logger
    {

        private static object ob = "lock";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strFunctionName"></param>
        /// <param name="strErrorNum"></param>
        /// <param name="strErrorDescription"></param>
        public static void AddLog(string LogText)
        {


            string strMatter; //错误内容
            string strPath; //错误文件的路径
            DateTime dt = DateTime.Now;
            string fileName;
            try
            {

                fileName = "YYWSN_" + dt.ToString("yyyyMMdd") + ".log";
                strPath = Directory.GetCurrentDirectory() + "\\Log";


                if (Directory.Exists(strPath) == false)  //工程目录下 Log目录 '目录是否存在,为true则没有此目录
                {
                    Directory.CreateDirectory(strPath); //建立目录　Directory为目录对象
                }
                strPath = strPath + "\\" + fileName;
                strMatter = LogText;
                //TODO: 去掉末尾的\r\n


                lock (ob)
                {
                    StreamWriter FileWriter = new StreamWriter(strPath, true, Encoding.Unicode); //创建日志文件
                    FileWriter.WriteLine(strMatter);
                    FileWriter.Close(); //关闭StreamWriter对象
                    FileWriter = null;

                }


            }
            catch (Exception ex)
            {

                string str = ex.Message.ToString();
            }
        }


    }
}
