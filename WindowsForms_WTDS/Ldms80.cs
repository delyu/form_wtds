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
        public double tBleft { get; set; }
        public double Bright { get; set; }

        public double Bdown { get; set; }
        public double Aup { get; set; }
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
                var vs =File.ReadAllText(path);
                wheelID = vs.ToString();
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
            try
            {
                var vs = File.ReadAllLines(path);

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
                            case "tBleft":
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
