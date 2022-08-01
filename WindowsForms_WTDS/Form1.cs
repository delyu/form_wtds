using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NModbus;
using log4net;
using log4net.Config;
using System.Diagnostics;


namespace WindowsForms_WTDS
{
  
    public partial class Form1 : Form
    {
        #region
        byte[] SendBufMode = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01, 0x06, 0x00, 0x09, 0x00, 0x01 };//测量模式打开
        //保存并传递实时测距值
        private double TAup { get; set; }
        //保存并传递实时测距值
        private double TAdown { get; set; }
        //保存并传递实时测距值
        private double TAleft { get; set; }
        //保存并传递实时测距值
        private double TAright { get; set; }
        //保存并传递实时测距值
        private double TBup { get; set; }
        //保存并传递实时测距值
        private double TBdown { get; set; }
        //保存并传递实时测距值
        private double TBleft { get; set; }
        private double TBright { get; set; }
        //保存并传递实时测距值
        //实时车轮测距值临时存储
        public double Adown;
        public double Aleft;
        public double Aright;
        public double Bup;
        public double Bleft;
        public double Bright;
        public double Bdown;
        public double Aup;
       

        private bool tboolAup = false; 
        private bool tboolAdown = false;
        private bool tboolAleft = false;
        private bool tboolAright = false;
        private bool tboolBup = false;
        private bool tboolBleft = false;
        private bool tboolBright = false;
        private bool tboolBdown = false;
        private int contAup = 0;//出现频率
        private int contAdown = 0;
        private int contAleft = 0;
        private int contAright = 0;
        private int contBup = 0;
        private int contBdown = 0;
        private int contBleft = 0;
        private int contBright = 0;
        /// <summary>
        /// 判断车轮循环控制
        /// </summary>
        private bool TstartWheel { get; set; }
        //配置文件参数
        #endregion
        public Form1()
        {
            //var id = ldms80.WheelID();           
            InitializeComponent();
           // Connect();
          
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            stopwatch.Start();
            //BasicConfigurator.Configure();
            Log.Info(string.Format("程序启动时间为{0}.", DateTime.Now.ToString()));//测试日志用
          
            ConnectAllLaser();
            ldms80.readconfig();
            //Log.Info(ldms80.rAleft2Aright.ToString() + ldms80.rAup2Adown.ToString() + ldms80.rBup2Bdown.ToString() + ldms80.rBleft2Bright.ToString());
            InitializeModbus();
               

        }
        Stopwatch stopwatch = new Stopwatch();
        //Socket Aupclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Socket Adownclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Socket Aleftclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Socket Arightclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Socket Bupclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Socket Bdownclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Socket Bleftclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Socket Brightclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        System.Net.Sockets.TcpClient Aupclient = new System.Net.Sockets.TcpClient();
        System.Net.Sockets.TcpClient Adownclient = new System.Net.Sockets.TcpClient();
        System.Net.Sockets.TcpClient Aleftclient = new System.Net.Sockets.TcpClient();
        System.Net.Sockets.TcpClient Arightclient = new System.Net.Sockets.TcpClient();
        System.Net.Sockets.TcpClient Bupclient = new System.Net.Sockets.TcpClient();
        System.Net.Sockets.TcpClient Bdownclient = new System.Net.Sockets.TcpClient();
        System.Net.Sockets.TcpClient Bleftclient = new System.Net.Sockets.TcpClient();
        System.Net.Sockets.TcpClient Brightclient = new System.Net.Sockets.TcpClient();
       
        Ldms80contorl ldms80 = new Ldms80contorl();
        //ModbusIpMaster master;
        // private static ModbusFactory modbusFactory;
        private static ModbusFactory modbusFactory;
        private static IModbusMaster masterAup;
        private static IModbusMaster masterAdown;
        private static IModbusMaster masterAleft;
        private static IModbusMaster masterAright;
        private static IModbusMaster masterBup;
        private static IModbusMaster masterBdown;
        private static IModbusMaster masterBleft;
        private static IModbusMaster masterBright;
        private bool startRecv=false;
        private bool stopTemp = false;
        /// <summary>
        /// 初始化modbus，建立连接,临时关闭，换TCP
        /// </summary>
        public void ModbusTcpMasterWriteSingleRegister()
        {
            //初始化modbusmaster
            modbusFactory = new ModbusFactory();
            //在本地测试 所以使用回环地址,modbus协议规定端口号 502
            try
            {
                masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                //设置读取超时时间
                masterAup.Transport.ReadTimeout = 2000;
                masterAup.Transport.Retries = 2000;
            }
            catch { }
            try
            {
                masterAdown = modbusFactory.CreateMaster(new TcpClient("192.168.1.201", 502));
                masterAdown.Transport.ReadTimeout = 2000;
                masterAdown.Transport.Retries = 2000;
            }
            catch { }
            try
            {
                masterAleft = modbusFactory.CreateMaster(new TcpClient("192.168.1.202", 502));
                masterAleft.Transport.ReadTimeout = 2000;
                masterAleft.Transport.Retries = 2000;
            }
            catch { }
            try
            {
                masterAright = modbusFactory.CreateMaster(new TcpClient("192.168.1.203", 502));
                masterAright.Transport.ReadTimeout = 2000;
                masterAright.Transport.Retries = 2000;
            }
            catch { }
            try
            {
                masterBup = modbusFactory.CreateMaster(new TcpClient("192.168.1.204", 502));
                masterBup.Transport.ReadTimeout = 2000;
                masterBup.Transport.Retries = 2000;
            }
            catch { }
            try
            {
                masterBdown = modbusFactory.CreateMaster(new TcpClient("192.168.1.205", 502));
                masterBdown.Transport.ReadTimeout = 2000;
                masterBdown.Transport.Retries = 2000;
            }
            catch { }
            try
            {
                masterBleft = modbusFactory.CreateMaster(new TcpClient("192.168.1.206", 502));
                masterBleft.Transport.ReadTimeout = 2000;
                masterBleft.Transport.Retries = 2000;
            }
            catch { }
            try
            {
                masterBright = modbusFactory.CreateMaster(new TcpClient("192.168.1.207", 502));
                masterBright.Transport.ReadTimeout = 2000;
                masterBright.Transport.Retries = 2000;
            }
            catch { }

            // WriteSingleRegister
        }
        #region 通过Nmodbus接收数据
        private async void ReadAupParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {if (!stopTemp)
                {
                    try
                    {
                        stopwatch.Stop();
                        TimeSpan ts = stopwatch.Elapsed;
                        //重新实例化是为了 modbus slave更换连接时不报错
                        // masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                        //读取保持寄存器
                        var registerBuffer = masterAup.ReadHoldingRegisters(01, 0, 17);
                        double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                        TAup = v;
                        tboolAup = true;
                        string dist = v.ToString();
                        Log.laser("Aup&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                        if (registerBuffer[10] != 0)
                        {
                             Writesingle(Aupclient, SendBufMode);
                            Log.Info("Aup激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                        }
                        ControlInvoker.Invoke(this, delegate
                        {
                            textBox_Aup.Text = dist;
                            label_static_Aup.Text = registerBuffer[9] + "_" + registerBuffer[14] + "℃_Err " + registerBuffer[10];
                        });


                    }
                    catch (Exception ex)
                    {
                        //  LogHelper.WriteLog(ex.Message.ToString());
                        Log.Info(string.Format("readAup:{0}", ex.Message.ToString()));
                        break;
                        // MessageBox.Show(ex.Message+"readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadAdownParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {
                        //重新实例化是为了 modbus slave更换连接时不报错
                        // masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                        //读取保持寄存器
                        var registerBuffer = masterAdown.ReadHoldingRegisters(01, 0, 17);
                        double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                        TAdown = v;
                        tboolAdown = true;
                        string dist = v.ToString();
                        Log.laser("Adown&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                        if (registerBuffer[10] != 0)
                        {
                             Writesingle(Adownclient, SendBufMode);
                            Log.Info("Adown激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                        }
                        ControlInvoker.Invoke(this, delegate
                        {
                            textBox_Adown.Text = dist;
                            textBox_Au2d.Text = (TAup - TAdown).ToString("#0.0000");
                            label_static_Adown.Text = registerBuffer[9] + "_" + registerBuffer[14] + "℃_Err " + registerBuffer[10];
                        });

                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readAdown:{0}", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadAleftParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {
                        //重新实例化是为了 modbus slave更换连接时不报错
                        // masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                        //读取保持寄存器
                        var registerBuffer = masterAleft.ReadHoldingRegisters(01, 0, 17);
                        double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                        TAleft = v;
                        tboolAleft = true;
                        string dist = v.ToString();
                        Log.laser("Aleft&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                        if (registerBuffer[10] != 0)
                        {
                             Writesingle(Aleftclient, SendBufMode);
                            Log.Info("Aleft激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                        }
                        ControlInvoker.Invoke(this, delegate
                        {
                            textBox_Aleft.Text = dist;
                            label_static_Aleft.Text = registerBuffer[9] + "_" + registerBuffer[14] + "℃_Err " + registerBuffer[10];
                        });

                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readAleft:{0}", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadArightParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {
                        //重新实例化是为了 modbus slave更换连接时不报错
                        // masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                        //读取保持寄存器
                        var registerBuffer = masterAright.ReadHoldingRegisters(01, 0, 17);
                        double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                        TAright = v;
                        tboolAright = true;
                        string dist = v.ToString();
                        Log.laser("Aright&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                        if (registerBuffer[10] != 0)
                        {
                             Writesingle(Arightclient, SendBufMode);
                            Log.Info("Aright激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                        }
                        ControlInvoker.Invoke(this, delegate
                        {
                            textBox_Aright.Text = dist;
                            textBox_Al2r.Text = (TAleft - TAright).ToString("#0.0000");
                            label_static_Aright.Text = registerBuffer[9] + "_" + registerBuffer[14] + "℃_Err " + registerBuffer[10];
                        });

                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readAright:{0}.", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadBupParametersFunction()
        {
           
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {                                      //读取保持寄存器
                        var registerBuffer = masterBup.ReadHoldingRegisters(01, 0, 17);
                        double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                        TBup = v;
                        tboolBup = true;
                        string dist = v.ToString();
                        Log.laser("Bup&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                        if (registerBuffer[10] != 0)
                        {
                            Writesingle(Bupclient, SendBufMode);
                            Log.Info("Bup激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                        }
                        ControlInvoker.Invoke(this, delegate
                        {
                            textBox_Bup.Text = dist;
                            label_static_Bup.Text = registerBuffer[9] + "_" + registerBuffer[14] + "℃_Err " + registerBuffer[10];
                        });
                    }
                    catch (Exception ex)
                    {
                        //LogHelper.WriteLog(ex.Message.ToString());
                        Log.Info(string.Format("readBup:{0}.", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message+"readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadBdownParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {//读取保持寄存器
                        var registerBuffer = masterBdown.ReadHoldingRegisters(01, 0, 17);
                        double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                        TBdown = v;
                        tboolBdown = true;
                        string dist = v.ToString();
                        Log.laser("Bdown&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                        if (registerBuffer[10] != 0)
                        {
                             Writesingle(Bdownclient, SendBufMode);
                            Log.Info("Bdown激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                        }
                        ControlInvoker.Invoke(this, delegate
                        {
                            textBox_Bdown.Text = dist;
                            textBox_Bu2d.Text = (TBup - TBdown).ToString("#0.0000");
                            label_static_Bdown.Text = registerBuffer[9] + "_" + registerBuffer[14] + "℃_Err " + registerBuffer[10];
                        });

                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readBdown:{0}", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async  void ReadBleftParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {
                        //重新实例化是为了 modbus slave更换连接时不报错
                        // masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                        //读取保持寄存器
                        var registerBuffer = masterBleft.ReadHoldingRegisters(01, 0, 17);
                        double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                        TBleft = v;
                        tboolBleft = true;
                        string dist = v.ToString();
                        Log.laser("Bleft&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                        if (registerBuffer[10] != 0)
                        {
                             Writesingle(Bleftclient, SendBufMode);
                            Log.Info("Bleft激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                        }
                        ControlInvoker.Invoke(this, delegate
                        {
                            textBox_Bleft.Text = dist;
                            label_static_Bleft.Text = registerBuffer[9] + "_" + registerBuffer[14] + "℃_Err " + registerBuffer[10];
                        });

                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readBleft:{0}", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadBrightParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {
                        //读取保持寄存器
                        var registerBuffer = masterBright.ReadHoldingRegisters(01, 0, 17);
                        double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                        TBright = v;
                        tboolBright = true;
                        string dist = v.ToString();
                        Log.laser("Bright&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                        if (registerBuffer[10] != 0)
                        {
                             Writesingle(Brightclient , SendBufMode);
                            Log.Info("Bright激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                        }
                        ControlInvoker.Invoke(this, delegate
                        {
                            textBox_Bright.Text = dist;
                            textBox_Bl2r.Text = (TBleft - TBright).ToString("0.0000");
                            label_static_Bright.Text = registerBuffer[9] + "_" + registerBuffer[14] + "℃_Err " + registerBuffer[10];
                        });

                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readBright:{0}", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        /// <summary>
        /// 开启接收线程
        /// </summary>
        private async void ReceiveModbus()

        {
            try
            {
                await Task.Run(() => ReadAupParametersFunction());
                await Task.Run(() => ReadAdownParametersFunction());
                await Task.Run(() => ReadAleftParametersFunction());
                await Task.Run(() => ReadArightParametersFunction());
                await Task.Run(() => ReadBupParametersFunction());
                await Task.Run(() => ReadBdownParametersFunction());
                await Task.Run(() => ReadBleftParametersFunction());
                await Task.Run(() => ReadBrightParametersFunction());
                /*
                //Thread threadAup = new Thread(readAupParametersFunction);
                //threadAup.Start();
                //Thread threadAdown = new Thread(readAdownParametersFunction);
                //threadAdown.Start();
                //Thread threadAleft = new Thread(readAleftParametersFunction);
                //threadAleft.Start();
                //Thread threadAright = new Thread(readArightParametersFunction);
                //threadAright.Start();
                //Thread threadBup = new Thread(readBupParametersFunction);
                //threadBup.Start();
                //Thread threadBdown = new Thread(readBdownParametersFunction);
                //threadBdown.Start();
                //Thread threadBleft = new Thread(readBleftParametersFunction);
                //threadBleft.Start();
                //Thread threadBright = new Thread(readBrightParametersFunction);
                //threadBright.Start();*/
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("ReceiveModbus:", ex.Message.ToString()));
                //MessageBox.Show(ex.Message);
            }
        }
        #endregion
        #region 通过TCP接收数据
        private async void ReadAupTcpFunction()
        {
            startRecv = true;         
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {                       
                        var registerBuffer =  Writesingle(Aupclient,ldms80.SendBufRecv);
                        if (registerBuffer!=null)
                        {
                            if (registerBuffer.Length > 36)
                            {
                                double v = (((registerBuffer[33] << 24) + (registerBuffer[34] << 16) + (registerBuffer[35] << 8) + registerBuffer[36]) * 0.0001);

                                TAup = v;
                                tboolAup = true;
                                string dist = v.ToString();
                                Log.laser("Aup&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                                if (registerBuffer[30] != 0)
                                {
                                    Writesingle(Aupclient, ldms80.SendBufStart);
                                    Log.Info("Aup激光测距仪出现故障重启,故障代码：" + registerBuffer[30]);
                                }
                                ControlInvoker.Invoke(this, delegate
                                {
                                    textBox_Aup.Text = dist;
                                    label_static_Aup.Text = registerBuffer[28] + "_" + registerBuffer[38] + "℃_Err " + registerBuffer[30];
                                });
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        //  LogHelper.WriteLog(ex.Message.ToString());
                        Log.Info(string.Format("readAup:{0}", ex.Message.ToString()));
                        break;
                        // MessageBox.Show(ex.Message+"readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadAdownTcpFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {
                        byte[] registerBuffer = Writesingle(Adownclient, ldms80.SendBufRecv);
                        if (registerBuffer != null)
                        {
                            if (registerBuffer.Length > 36)
                            {
                                double v = (((registerBuffer[33] << 24) + (registerBuffer[34] << 16) + (registerBuffer[35] << 8) + registerBuffer[36]) * 0.0001);
                                TAdown = v;
                                tboolAdown = true;
                                string dist = v.ToString();
                                Log.laser("Adown&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                                if (registerBuffer[30] != 0)
                                {
                                    Writesingle(Adownclient, ldms80.SendBufStart);
                                    Log.Info("Adown激光测距仪出现故障重启,故障代码：" + registerBuffer[30]);
                                }
                                ControlInvoker.Invoke(this, delegate
                                {
                                    textBox_Adown.Text = dist;
                                    textBox_Au2d.Text = (TAup - TAdown).ToString("#0.0000");
                                    label_static_Adown.Text = registerBuffer[28] + "_" + registerBuffer[38] + "℃_Err " + registerBuffer[30];
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readAdown:{0}", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadAleftTcpFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {
                        //重新实例化是为了 modbus slave更换连接时不报错
                        // masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                        //读取保持寄存器
                        byte[] registerBuffer = Writesingle(Aleftclient, ldms80.SendBufRecv);
                        if (registerBuffer != null)
                        {
                            if (registerBuffer.Length > 36)
                            {
                                double v = (((registerBuffer[33] << 24) + (registerBuffer[34] << 16) + (registerBuffer[35] << 8) + registerBuffer[36]) * 0.0001);
                                TAleft = v;
                                tboolAleft = true;
                                string dist = v.ToString();
                                Log.laser("Aleft&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                                if (registerBuffer[30] != 0)
                                {
                                    Writesingle(Aleftclient, ldms80.SendBufStart);
                                    Log.Info("Aleft激光测距仪出现故障重启,故障代码：" + registerBuffer[30]);
                                }
                                ControlInvoker.Invoke(this, delegate
                                {
                                    textBox_Aleft.Text = dist;
                                    label_static_Aleft.Text = registerBuffer[28] + "_" + registerBuffer[38] + "℃_Err " + registerBuffer[30];
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readAleft:{0}", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadArightTcpFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {
                        byte[] registerBuffer = Writesingle(Arightclient, ldms80.SendBufRecv);
                        if (registerBuffer != null)
                        {
                            if (registerBuffer.Length > 36)
                            {
                                double v = (((registerBuffer[33] << 24) + (registerBuffer[34] << 16) + (registerBuffer[35] << 8) + registerBuffer[36]) * 0.0001);

                                TAright = v;
                                tboolAright = true;
                                string dist = v.ToString();
                                Log.laser("Aright&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                                if (registerBuffer[30] != 0)
                                {
                                    Writesingle(Arightclient, ldms80.SendBufStart);
                                    Log.Info("Aright激光测距仪出现故障重启,故障代码：" + registerBuffer[30]);
                                }
                                ControlInvoker.Invoke(this, delegate
                                {
                                    textBox_Aright.Text = dist;
                                    textBox_Al2r.Text = (TAleft - TAright).ToString("#0.0000");
                                    label_static_Aright.Text = registerBuffer[28] + "_" + registerBuffer[38] + "℃_Err " + registerBuffer[30];
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readAright:{0}.", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadBupTcpFunction()
        {
            //System.Net.Sockets.TcpClient Bupclient = new System.Net.Sockets.TcpClient();
            //await Bupclient.ConnectAsync("192.168.1.204", 502);
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {   //Bupclient.                                  //读取保持寄存器
                        byte[] registerBuffer = Writesingle(Bupclient, ldms80.SendBufRecv);
                        if (registerBuffer != null)
                        {
                            if (registerBuffer.Length > 36)
                            {
                                double v = (((registerBuffer[33] << 24) + (registerBuffer[34] << 16) + (registerBuffer[35] << 8) + registerBuffer[36]) * 0.0001);
                                TBup = v;
                                tboolBup = true;
                                string dist = v.ToString();
                                Log.laser("Bup&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                                if (registerBuffer[30] != 0)
                                {
                                    Writesingle(Bupclient, ldms80.SendBufStart);
                                    Log.Info("Bup激光测距仪出现故障重启,故障代码：" + registerBuffer[30]);
                                }
                                ControlInvoker.Invoke(this, delegate
                                {
                                    textBox_Bup.Text = dist;
                                    label_static_Bup.Text = registerBuffer[28] + "_" + registerBuffer[38] + "℃_Err " + registerBuffer[30];
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //LogHelper.WriteLog(ex.Message.ToString());
                        Log.Info(string.Format("readBup:{0}.", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message+"readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadBdownTcpFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {//读取保持寄存器
                        byte[] registerBuffer = Writesingle(Bdownclient, ldms80.SendBufRecv);
                        if (registerBuffer != null)
                        {
                            if (registerBuffer.Length > 36)
                            {
                                double v = (((registerBuffer[33] << 24) + (registerBuffer[34] << 16) + (registerBuffer[35] << 8) + registerBuffer[36]) * 0.0001);
                                TBdown = v;
                                tboolBdown = true;
                                string dist = v.ToString();
                                Log.laser("Bdown&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                                if (registerBuffer[30] != 0)
                                {
                                    Writesingle(Bdownclient, ldms80.SendBufStart);
                                    Log.Info("Bdown激光测距仪出现故障重启,故障代码：" + registerBuffer[30]);
                                }
                                ControlInvoker.Invoke(this, delegate
                                {
                                    textBox_Bdown.Text = dist;
                                    textBox_Bu2d.Text = (TBup - TBdown).ToString("#0.0000");
                                    label_static_Bdown.Text = registerBuffer[28] + "_" + registerBuffer[38] + "℃_Err " + registerBuffer[30];
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readBdown:{0}", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadBleftTcpFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {
                        byte[] registerBuffer = Writesingle(Bleftclient, ldms80.SendBufRecv);
                        if (registerBuffer != null)
                        {
                            if (registerBuffer.Length > 36)
                            {
                                double v = (((registerBuffer[33] << 24) + (registerBuffer[34] << 16) + (registerBuffer[35] << 8) + registerBuffer[36]) * 0.0001);                               
                                TBleft = v;
                                tboolBleft = true;
                                string dist = v.ToString();
                                Log.laser("Bleft&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                                if (registerBuffer[30] != 0)
                                {
                                    Writesingle(Bleftclient, ldms80.SendBufStart);
                                    Log.Info("Bleft激光测距仪出现故障重启,故障代码：" + registerBuffer[30]);
                                }
                                ControlInvoker.Invoke(this, delegate
                                {
                                    textBox_Bleft.Text = dist;
                                    label_static_Bleft.Text = registerBuffer[28] + "_" + registerBuffer[38] + "℃_Err " + registerBuffer[30];
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readBleft:{0}", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        private async void ReadBrightTcpFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                if (!stopTemp)
                {
                    try
                    {
                        //读取保持寄存器
                        byte[] registerBuffer = Writesingle(Brightclient, ldms80.SendBufRecv);
                        if (registerBuffer != null)
                        {
                            if (registerBuffer.Length > 36)
                            {
                                double v = (((registerBuffer[33] << 24) + (registerBuffer[34] << 16) + (registerBuffer[35] << 8) + registerBuffer[36]) * 0.0001);
                                TBright = v;
                                tboolBright = true;
                                string dist = v.ToString();
                                Log.laser("Bright&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                                if (registerBuffer[30] != 0)
                                {
                                    Writesingle(Brightclient, ldms80.SendBufStart);
                                    Log.Info("Bright激光测距仪出现故障重启,故障代码：" + registerBuffer[30]);
                                }
                                ControlInvoker.Invoke(this, delegate
                                {
                                    textBox_Bright.Text = dist;
                                    textBox_Bl2r.Text = (TBleft - TBright).ToString("0.0000");
                                    label_static_Bright.Text = registerBuffer[28] + "_" + registerBuffer[38] + "℃_Err " + registerBuffer[30];
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Info(string.Format("readBright:{0}", ex.Message.ToString()));
                        break;
                        //MessageBox.Show(ex.Message + "readParametersFunction");
                    }
                    await Task.Delay(50);
                }
                await Task.Delay(1);
            }
        }
        /// <summary>
        /// 开启TCPip接收线程
        /// </summary>
        private async void ReceiveTcpIp()

        {
            try
            {
                await Task.Run(() => ReadAupTcpFunction());
                await Task.Run(() => ReadAdownTcpFunction());
                await Task.Run(() => ReadAleftTcpFunction());
                await Task.Run(() => ReadArightTcpFunction());
                await Task.Run(() => ReadBupTcpFunction());
                await Task.Run(() => ReadBdownTcpFunction());
                await Task.Run(() => ReadBleftTcpFunction());
                await Task.Run(() => ReadBrightTcpFunction());
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("ReceiveModbus:", ex.Message.ToString()));
                //MessageBox.Show(ex.Message);
            }
        }
        #endregion
        /// <summary>
        /// 停止测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_stop_Click(object sender, EventArgs e)

        {
            InitializeABlaser();
            stopTemp = true;
            sentLaser(ldms80.SendBufStop);
            startRecv = false;
            TstartWheel = false;           
        }

        /// <summary>
        /// 打开测量模式,初始化测距仪
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Buttonconnect_Click(object sender, EventArgs e)
        {
            ConnectAllLaser();
            //sentLaser(ldms80.SendBufStart);
        }
        private void ConnectAllLaser()
        {
            ConnectAup();
            ConnectAdown();
            ConnectAleft();
            ConnectAright();
            ConnectBup();
            ConnectBdown();
            ConnectBleft();
            ConnectBright();
        }

        private void ConnectAup()
        {
            ConnectLaserIP(Aupclient, "192.168.1.200", 502);
            //if (Aupclient != null)
            //{
            //    Writesingle(Aupclient, SendBufMode);
            //}

        }
        private void ConnectAdown()
        {
            ConnectLaserIP(Adownclient, "192.168.1.201", 502);
            //if (Adownclient != null)
            //{
            //    Writesingle(Adownclient, SendBufMode);
            //}
        }
        private void ConnectAleft()
        {
            ConnectLaserIP(Aleftclient, "192.168.1.202", 502);
            //if (Aleftclient != null)
            //{
            //    Writesingle(Aleftclient, SendBufMode);
            //}
        }
        private void ConnectAright()
        {
            ConnectLaserIP(Arightclient, "192.168.1.203", 502);

            //if (Arightclient != null)
            //{
            //    Writesingle(Arightclient, SendBufMode);
            //}
        }
        private void ConnectBup()
        {
            ConnectLaserIP(Bupclient, "192.168.1.204", 502);
            //if (Bupclient != null)
            //{
            //    Writesingle(Bupclient, SendBufMode);
            //}
        }
        private void ConnectBdown()
        {
            ConnectLaserIP(Bdownclient, "192.168.1.205", 502);

            //if (Bdownclient != null)
            //{
            //    Writesingle(Bdownclient, SendBufMode);
            //}

        }
        private void ConnectBleft()
        {
            ConnectLaserIP(Bleftclient, "192.168.1.206", 502);
            //if (Bleftclient != null)
            //{
            //    Writesingle(Bleftclient, SendBufMode);
            //}

        }
        private void ConnectBright()
        {
            ConnectLaserIP(Brightclient, "192.168.1.207", 502);
            //if (Brightclient != null)
            //{
            //    Writesingle(Brightclient, SendBufMode);
            //}
        }

        /// <summary>
        /// 用于集中发送一次指令
        /// </summary>
        /// <param name="mode"></param>
        private void sentLaser(byte[] mode)
        {
            Writesingle(Aupclient, mode);
            Writesingle(Adownclient, mode);
            Writesingle(Aleftclient, mode);
            Writesingle(Arightclient, mode);
            Writesingle(Bupclient, mode);
            Writesingle(Bdownclient, mode);
            Writesingle(Bleftclient, mode);
            Writesingle(Brightclient, mode);
        }
        /// <summary>
        /// 连接对应的tcpip
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        private void ConnectLaserIP(Socket socket,string ip,int port)
        {
            try
            {
                socket.Connect(ip, 502);
            }
            catch (Exception ex)
            {
                Log.Info(ip+ "connectLaserIP:"+ ex.Message.ToString());
            }
        }
        private void ConnectLaserIP(TcpClient socket, string ip, int port)
        {
            try
            {
                socket.Connect(ip, 502);
            }
            catch (Exception ex)
            {
                Log.Info(ip + "connectLaserIP:" + ex.Message.ToString());
            }
        }
        /// <summary>
        /// 发送modbustcpip指令
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="vs">指令</param>
        private async Task<byte[]> Writesingle(Socket socket, byte[] vs)
        {

            try
            {
                socket.Send(vs);
                var buffer = new byte[1024 * 1024];
                int msg = 0;
                           
                   
                await Task.Run(() => msg = socket.Receive(buffer));
                byte[] rebuffer = new byte[msg];
                Array.Copy(buffer, rebuffer, msg);
                return rebuffer;
                //  Aupclient.BeginReceive(buffer, 0,20,SocketFlags.None, ReceiveCallback, Aupclient);
            }
            catch (Exception ex)
            {
                Log.Info(socket.RemoteEndPoint.ToString() + ex.Message.ToString());
            }
            return null;
           
        }
        private byte[] Writesingle(TcpClient client, byte[] vs)
        {
            try
            {
                var buffer = new byte[10 * 10];
                int msg = 0;
                var s = client.GetStream();
                if (s.CanWrite)
                    s.Write(vs, 0, vs.Length);
                if (s.DataAvailable)
                { //判断有数据再读，否则Read会阻塞线程。后面的业务逻辑无法处理                
                    msg = s.Read(buffer, 0, buffer.Length);
                    if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0 && buffer[3] == 0 && buffer[4] == 0&&buffer[6]==1&&buffer[32]==1)
                    {
                        byte[] rebuffer = new byte[msg];
                        Array.Copy(buffer, rebuffer, msg);
                        return rebuffer;
                    }
                    else
                        buffer[0] = 1;
                }                
            }
            catch (Exception ex)
            {
                Log.Info(client.Client.RemoteEndPoint.ToString() + ex.Message.ToString());
            }
            return null;

        }
     
        /// <summary>
        /// 异步接收用
        /// </summary>
        /// <param name="result"></param>
        static void ReceiveCallback(IAsyncResult result)
        {
            Socket ts = (Socket)result.AsyncState;
            ts.EndReceive(result);
            result.AsyncWaitHandle.Close();
            var buffer=new byte[20];
            ts.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), ts);
        }
        private void Button_startRecv_Click(object sender, EventArgs e)
        {
            InitializeModbus();
        }
        /// <summary>
        /// 初始化指令，重新建立接收线程；
        /// </summary>
        private async void InitializeModbus()
        {
            stopTemp = false;
            // await Task.Run(()=> ModbusTcpMasterWriteSingleRegister());
            //ConnectAllLaser();
            sentLaser(ldms80.SendBufStart);
            ReceiveTcpIp();
            //ReceiveModbus();           
            await Task.Run(() => FormToolsFlash());
            await Task.Run(() => EstimateWheel());
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Button_stop_Click( this,null);
                       
        }
        //关闭窗体按钮 

        private async void FormToolsFlash()
        {
            bool start = true;
            while (start)
            {
                try
                {
                    ControlInvoker.Invoke(this, delegate
                    {
                        label_static_Aup.Text = "_" + "℃_Err ";
                        label_static_Adown.Text = "_" + "℃_Err ";
                        label_static_Aleft.Text = "_" + "℃_Err ";
                        label_static_Aright.Text = "_" + "℃_Err ";
                        label_static_Bup.Text = "_" + "℃_Err ";
                        label_static_Bdown.Text = "_" + "℃_Err ";
                        label_static_Bleft.Text = "_" + "℃_Err ";
                        label_static_Bright.Text = "_" + "℃_Err ";
                        label_sysdate.Text = DateTime.Now.ToString();
                        label_systime.Text = DateTime.Now.ToString("hh: mm:ss");                        
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(string.Format("formtools", ex.Message.ToString()));
                    Form1_Load(this, null);
                    break;
                }
               await Task.Delay(1000);
            }           
        }
        /// <summary>
        /// 车轮是否经过判断,单位时间内，如果有，则存储值；如果无，则输出最近测距值，并报异常；
        /// </summary>
        private void EstimateWheel()
        {
            TstartWheel = true;           
            int wheelA = 2;//判断A测是否正常
            int wheelB = 2;//判断B测是否正常
            var t0 = DateTime.Now;
            var t1 = DateTime.Now;//测到车轮后附加测量3秒用；
            bool dely = true ;//判断是否已经完成附加测量

            ldms80.InitializeABlaser();
            while (TstartWheel)
            {
                if (!stopTemp)
                {
                    if (tboolAup&& JudgeLaser(TAup, ldms80.rAup, ldms80.rAup2Adown,ref contAup,ref Aup))
                    {       tboolAup = false; 
                    }
                    if (tboolAdown && JudgeLaser(TAdown, ldms80.rAdown, ldms80.rAup2Adown,ref contAdown,ref Adown))
                    {                       
                        tboolAdown = false; 
                    }
                    if (tboolAleft && JudgeLaser(TAleft, ldms80.rAleft, ldms80.rAleft2Aright, ref contAleft, ref Aleft))
                    {
                        tboolAleft = false;
                    }
                    if (tboolAright && JudgeLaser(TAright, ldms80.rAright, ldms80.rAleft2Aright, ref contAright, ref Aright))
                    {
                        tboolAright = false;
                    }
                    if (tboolBup && JudgeLaser(TBup, ldms80.rBup, ldms80.rBup2Bdown, ref contBup, ref Bup))
                    {
                        tboolBup = false;
                    }
                    if (tboolBdown && JudgeLaser(TBdown, ldms80.rBdown, ldms80.rBup2Bdown, ref contBdown, ref Bdown))
                    {
                        tboolBdown = false;
                    }
                    if (tboolBleft && JudgeLaser(TBleft, ldms80.rBleft, ldms80.rBleft2Bright, ref contBleft, ref Bleft))
                    {
                        tboolBleft = false;
                    }
                    if (tboolBright && JudgeLaser(TBright, ldms80.rBright, ldms80.rBleft2Bright, ref contBright, ref Bright))
                    {
                        tboolBright = false;
                    }
                    TimeSpan timeSpan = DateTime.Now - t0;
                    ////change 判断条件
                    if (timeSpan.TotalSeconds > ldms80.rwheeltimeSpanmin)
                    {
                        if (JudgeWheeel(contAup,contAdown,contAleft,contAright,(int)ldms80.rwheelpoint)>1)
                            wheelA = 1;
                        if (JudgeWheeel(contBup, contBdown, contBleft, contBright, (int)ldms80.rwheelpoint) > 1)//判断时间间隔，输出测量值
                            wheelB = 1;
                        if (wheelA == 1 && wheelB == 1)
                        {
                        //    if (dely)
                        //    {
                        //        t1 = DateTime.Now;
                        //        dely = false;
                        //    }
                        //    TimeSpan delytime = DateTime.Now - t1;
                        //    if (delytime.Milliseconds>1000)
                        //    {
                                t0 = DateTime.Now;
                                dely = true;                              
                                ldms80.writeWheelsave(Wheeldata(Aup, Adown, Aleft, Aright, wheelA.ToString(), Bup, Bdown, Bleft, Bright, wheelB.ToString()));
                                //ldms80.InitializeABlaser();
                                InitializeABlaser();
                                wheelB = 2;
                                wheelA = 2;
                                stopTemp = true;
                                sentLaser(ldms80.SendBufStop);
                                ControlInvoker.Invoke(this, delegate
                                {
                                    timer1.Interval = (int)ldms80.rwheeltimeSpanmin * 1000;
                                    timer1.Enabled = true;
                                    timer1.Start();
                                });
                           // }
                        }
                    }
                    if (timeSpan.TotalSeconds > ldms80.rwheeltimeSpanmax)
                    {
                        t0 = DateTime.Now;                       
                        ldms80.writeWheelsave(Wheeldata(Aup, Adown, Aleft, Aright, wheelA.ToString(), Bup, Bdown, Bleft, Bright, wheelB.ToString()));
                        wheelB = 2;
                        wheelA = 2;
                       // ldms80.InitializeABlaser();
                        InitializeABlaser();
                        //stopTemp = true;
                        //sentLaser(ldms80.SendBufStop);
                        //ControlInvoker.Invoke(this, delegate
                        //{
                        //    timer1.Interval = (int)ldms80.rwheeltimeSpanmin * 1000;
                        //    timer1.Enabled = true;
                        //    timer1.Start();
                        //});
                    }
                }
                Task.Delay(1);
            }
        }
        public void InitializeABlaser()
        {
            TAup = 0;
            TAdown = 0;
            TAleft = 0;
            TAright = 0;
            TBup = 0;
            TBdown = 0;
            TBleft = 0;
            TBright = 0;
            tboolAup = false;
            tboolAdown = false;
            tboolAleft = false;
            tboolAright = false;
            tboolBup = false;
            tboolBleft = false;
            tboolBright = false;
            tboolBdown = false;
            contAup = 0;//出现频率
            contAdown = 0;
            contAleft = 0;
            contAright = 0;
            contBup = 0;
            contBdown = 0;
            contBleft = 0;
            contBright = 0;
            //实时车轮测距值临时存储
            Adown = 0;
            Aleft = 0; 
            Aright = 0;
            Bup = 0;
            Bleft = 0;
            Bright = 0;
            Bdown = 0;
            Aup = 0;

    }
        /// <summary>
        /// 输出车轮测量数据
        /// </summary>
        /// <param name="aup">a上值</param>
        /// <param name="adown">a下值</param>
        /// <param name="aleft">a左值</param>
        /// <param name="aright"a右值></param>
        /// <param name="a">a状态</param>
        /// <param name="bup"></param>
        /// <param name="bdown"></param>
        /// <param name="bleft"></param>
        /// <param name="bright"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private string Wheeldata(double aup,double adown,double aleft,double aright,string  a,double bup,double bdown,double bleft,double bright,string b )
        {
            double aud = aup - adown*1.00001;//剔除两个数据恰好相同的情形。
            double alr = aleft - aright * 1.00001;
            double bud = bup - bdown * 1.00001;
            double blr = bleft - bright * 1.00001;
            if ((aud != 0 && Math.Abs(aud) < ldms80.rAup2Adown) || (alr != 0 && Math.Abs(alr) < ldms80.rAup2Adown))
            {
                a = 1.ToString();
            }
            if ((bud != 0 && Math.Abs(bud) < ldms80.rBup2Bdown) || (blr != 0 && Math.Abs(blr) < ldms80.rBup2Bdown))
            {
                b = 1.ToString();
            }
            string data =ldms80.WheelID()+"&"+ldms80.rAup2Adown.ToString("#0.000") +"&"+ldms80.rAleft2Aright.ToString("#0.000") + "$"
                +aup.ToString("#0.000") + "&"+ adown.ToString("#0.000") + "&" +aud.ToString("#0.000") + "&" + a + "&"
                +aleft.ToString("#0.000") + "&"+aright.ToString("#0.000") + "&"+alr.ToString("#0.000") + "&"+a+"&"+ldms80.rwheelSpeed.ToString("#0.00")+ "$"
                +bup.ToString("#0.000") + "&" + bdown.ToString("#0.000") + "&" + bud.ToString("#0.000") + "&" + b+ "&" 
                + bleft.ToString("#0.000") + "&" + bright.ToString("#0.000") + "&" + blr.ToString("#0.000") + "&" + b +"&"+ ldms80.rwheelSpeed.ToString("#0.00") + "#";
            return data;

///"# 时间&台车车号&上下允许偏差设定值&左右允许偏差设定值
///$A侧上点值 & A侧下点值 & A侧上下偏差值 & A侧上下状态 & A侧左点值 & A侧右点值 & A侧左右偏差值 & A侧左右状态 & A侧速度$
///B侧上点值 & B侧下点值 & B侧上下偏差值 & B侧上下状态 & B侧左点值 & B侧右点值 & B侧左右偏差值 & B侧左右状态 & B侧速度#

        }
        private bool JudgeLaser(double d1,double d2,double rd1,double rd2,double para)
        {
            if ((Math.Abs(d1 - rd1) <= para && Math.Abs(d2 - rd2) <= para ))
                return true;
            else
                return false;
        }

        private bool JudgeLaser(double d1,  double rd1,  double para,ref int cont,ref double avedist)
        {
            if (Math.Abs(d1 - rd1) <= para)
            {                
                avedist = (avedist * cont + d1) / (cont + 1);
                cont++;
                return true;
            }
            else
                return false;
        }
        private int JudgeWheeel(int up,int down,int left,int right,int count)
        {
            int ret = 0;
            if (up > count)
                ret++;
            if (down > count)
                ret++;
            if (left > count)
                ret++;
            if (right > count)
                ret++;
            return ret;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            sentLaser(ldms80.SendBufStart);
            stopTemp = false;           
            timer1.Stop();
        }
    }
    /// <summary>
    /// 专门负责处理多线程UI调用,避免提前关闭窗体，会引发InvalidOperationException
    /// </summary>
    public static class ControlInvoker
    {
        public static void Invoke(Control ctl, MethodInvoker method)
        {
            if (!ctl.IsHandleCreated)
                return;

            if (ctl.IsDisposed)
                return;

            if (ctl.InvokeRequired)
            {
                ctl.Invoke(method);
            }
            else
            {
                method();
            }
        }
    }
    /// <summary>
    /// tcp连接状态检测
    /// </summary>
    public static class TcpClientEx
    {
        public static bool IsOnline(this TcpClient c)
        {
            return !((c.Client.Poll(1000, SelectMode.SelectRead) && (c.Client.Available == 0)) || !c.Client.Connected);
        }
    }



}
