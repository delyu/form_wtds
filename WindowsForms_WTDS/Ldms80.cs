using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace WindowsForms_WTDS
{
    public class Ldms80contorl
    {
        static Ldms80contorl()
        {
           
        }
        public byte [] SendBufModeAup = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x06, 0x00, 0x07, 0x00, 0x00 };//Aup连续测量
        public byte [] SendBufStartAup = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x06, 0x00, 0x09, 0x00, 0x01 };//Aup开始测量
        public byte [] SendBufStopAup = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x06, 0x00, 0x09, 0x00, 0x00 };//Aup停止测量
        public byte [] CheckAup = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x01, 0x00 };//指令发送正常
        public byte [] SendBufRecvdataAup = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, 0x00, 0x0B, 0x00, 0x03 };//Aup获取测量数据
        public byte [] SendBufGetInfoAup = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, 0x00, 0x0A, 0x00, 0x01 };//Aup获取仪器状态
        //实时车轮测距值临时存储
        public double Adown { get; set; }
        public double Aleft { get; set; }
        public double Aright { get; set; }
        public double Bup { get; set; }
        public double Bleft { get; set; }
        public double Bright { get; set; }
        public double Bdown { get; set; }
        public double Aup { get; set; }
        /// <summary>
        /// 有效测距平均值清零；
        /// </summary>
        public void InitializeABlaser()
        {
            Aup = 0;
            Adown = 0;
            Aleft = 0;
            Aright = 0;
            Bup = 0;
            Bdown = 0;
            Bleft = 0;
            Bright = 0;
        }
        ////配置文件参数
        public double rAup { get; set; }
        public double rAdown { get; set; }
        public double rAup2Adown { get; set; }
        public double rAleft { get; set; }
        public double rAright { get; set; }
        public double rAleft2Aright { get; set; }
        public double rBup { get; set; }
        public double rBdown { get; set; }
        public double rBup2Bdown { get; set; }
        public double rBleft { get; set; }
        public double rBright { get; set; }
        public double rBleft2Bright { get; set; }
        public double rwheelSpeed { get; set; }

        public double rwheeltimeSpanmin { get; set; }
        public double rwheeltimeSpanmax { get; set; }
        public double rwheelpoint { get; set; }

        //车牌号读取
        public string WheelID()
        {
            string wheelID = "-1";
            string path = Directory.GetCurrentDirectory()+@"\\WHEEL_num.txt";           
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default))
                {
                    var vs = sr.ReadToEnd();
                    //   var vs = File.ReadAllText(path);
                    sr.Close();
                    fs.Close();
                    wheelID = vs.ToString();
                    int index = wheelID.Length - 1;
                    wheelID = wheelID.Remove(index);
                }
            }
            catch (Exception ex)
            {
                Log.Error("WHEEL_num.txt读取错误",ex);
            }
            return wheelID;
        }
        /// <summary>
        /// 读取配置文件
        /// </summary>
        public void readconfig()
        {
            rwheelpoint = 3;
            rwheeltimeSpanmin = 10;
            rwheeltimeSpanmax = 30;
            string path = Directory.GetCurrentDirectory() + @"\\config.txt";
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default))
                {
                   var vs = sr.ReadToEnd().Replace("\r\n", "^").Split('^');
                    sr.Close();
                    fs.Close();
                    try
                    {
                       // var vs = File.ReadAllLines(path);

                        for (int i = 0; i < vs.Length; i++)
                        {
                            string[] a = vs[i].Split(':');
                            if (a.Length > 1)
                            {
                                switch (a[0])
                                {
                                    case "Aup":
                                        rAup = Convert.ToDouble(a[1]);
                                        break;
                                    case "Adown":
                                        rAdown = Convert.ToDouble(a[1]);
                                        break;
                                    case "Aud":
                                        rAup2Adown = Convert.ToDouble(a[1]);
                                        break;
                                    case "Aleft":
                                        rAleft = Convert.ToDouble(a[1]);
                                        break;
                                    case "Aright":
                                        rAright = Convert.ToDouble(a[1]);
                                        break;
                                    case "Alr":
                                        rAleft2Aright = Convert.ToDouble(a[1]);
                                        break;
                                    case "Bup":
                                        rBup = Convert.ToDouble(a[1]);
                                        break;
                                    case "Bdown":
                                        rBdown = Convert.ToDouble(a[1]);
                                        break;
                                    case "Bud":
                                        rBup2Bdown = Convert.ToDouble(a[1]);
                                        break;
                                    case "Bleft":
                                        rBleft = Convert.ToDouble(a[1]);
                                        break;
                                    case "Bright":
                                        rBright = Convert.ToDouble(a[1]);
                                        break;
                                    case "Blr":
                                        rBleft2Bright = Convert.ToDouble(a[1]);
                                        break;
                                    case "Speed":
                                        rwheelSpeed = Convert.ToDouble(a[1]);
                                        break;
                                    case "WheelTimespanmin":
                                        rwheeltimeSpanmin = Convert.ToDouble(a[1]);
                                        break;
                                    case "WheelTimespanmax":
                                        rwheeltimeSpanmax = Convert.ToDouble(a[1]);
                                        break;
                                    case "Wheelpoint":
                                        rwheelpoint = Convert.ToDouble(a[1]);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("config.txt读取错误", ex);
                    }
                }
            }
          
           
        }
        public void writeWheelsave(string data)
        {
          
            string path = Directory.GetCurrentDirectory() + @"\WHEEL_SAVED.txt";
            string path2 = Directory.GetCurrentDirectory() + @"\WHEEL.txt";

            try
            {
                //using (FileStream fs1 = new FileStream(path2, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                //{
                using (StreamWriter sW1 = new StreamWriter(path2, false, System.Text.Encoding.Default))
                {
                    sW1.Write(data);
                    sW1.Flush();
                    sW1.Close();
                }
                //}


                using (StreamWriter sW2 = new StreamWriter(path, true, System.Text.Encoding.Default))
                {
                    sW2.WriteLine(DateTime.Now.ToString() + data);
                    sW2.Flush();
                    sW2.Close();                   
                }


            }
            catch (Exception ex)
            {
                Log.Error("WHEEL.txt写错误", ex);
            }

        }
    }
    
}
