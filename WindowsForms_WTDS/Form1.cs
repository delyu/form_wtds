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
        //保存并传递实时测距值
        private double TBright { get; set; }

        private bool tboolAup = false; 
        private bool tboolAdown = false;
        private bool tboolAleft = false;
        private bool tboolAright = false;
        private bool tboolBup = false;
        private bool tboolBleft = false;
        private bool tboolBright = false;
        private bool tboolBdown = false;
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
          //  BasicConfigurator.Configure();
            Log.Info(string.Format("程序启动时间为{0}.", DateTime.Now.ToString()));//测试日志用
            ldms80.readconfig();
            InitializeModbus();
           

        }
        Socket Aupclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket Adownclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket Aleftclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket Arightclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket Bupclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket Bdownclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket Bleftclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket Brightclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
        /// 初始化modbus，建立连接
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
        #region 接收数据
        private async void ReadAupParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {if (!stopTemp)
                {
                    try
                    {
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
                            ConnectAup();
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
                            ConnectAdown();
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
                            ConnectAleft();
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
                            ConnectAright();
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
                            ConnectBup();
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
                            ConnectBdown();
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
                            ConnectBleft();
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
                            ConnectBright();
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
        /// <summary>
        /// 停止测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)

        {
            masterAup.Dispose();
            startRecv = false;
            TstartWheel = false;            
        }

        /// <summary>
        /// 打开测量模式,初始化测距仪
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button2_Click(object sender, EventArgs e)
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
            if (Aupclient != null)
            {
                Writesingle(Aupclient, SendBufMode);
            }

        }
        private void ConnectAdown()
        {
            ConnectLaserIP(Adownclient, "192.168.1.201", 502);
            if (Adownclient != null)
            {
                Writesingle(Adownclient, SendBufMode);
            }
        }
        private void ConnectAleft()
        {
            ConnectLaserIP(Aleftclient, "192.168.1.202", 502);
            if (Aleftclient != null)
            {
                Writesingle(Aleftclient, SendBufMode);
            }
        }
        private void ConnectAright()
        {
            ConnectLaserIP(Arightclient, "192.168.1.203", 502);

            if (Arightclient != null)
            {
                Writesingle(Arightclient, SendBufMode);
            }
        }
        private void ConnectBup()
        {
            ConnectLaserIP(Bupclient, "192.168.1.204", 502);
            if (Bupclient != null)
            {
                Writesingle(Bupclient, SendBufMode);
            }
        }
        private void ConnectBdown()
        {
            ConnectLaserIP(Bdownclient, "192.168.1.205", 502);

            if (Bdownclient != null)
            {
                Writesingle(Bdownclient, SendBufMode);
            }

        }
        private void ConnectBleft()
        {
            ConnectLaserIP(Bleftclient, "192.168.1.206", 502);
            if (Bleftclient != null)
            {
                Writesingle(Bleftclient, SendBufMode);
            }

        }
        private void ConnectBright()
        {
            ConnectLaserIP(Brightclient, "192.168.1.207", 502);
            if (Brightclient != null)
            {
                Writesingle(Brightclient, SendBufMode);
            }
        }


        private void sentLaser(byte[] mode)
        {
            if (Aupclient != null)
            {
                Writesingle(Aupclient, mode);
            }
            if (Adownclient != null)
            {
                Writesingle(Adownclient, mode);
            }
            if (Aleftclient != null)
            {
                Writesingle(Aleftclient, mode);
            }
            if (Arightclient != null)
            {
                Writesingle(Arightclient, mode);
            }
            if (Bupclient != null)
            {
                Writesingle(Bupclient, mode);
            }
            if (Bdownclient != null)
            {
                Writesingle(Bdownclient, mode);
            }
            if (Bleftclient != null)
            {
                Writesingle(Bleftclient, mode);
            }
            if (Brightclient != null)
            {
                Writesingle(Brightclient, mode);
            }

        }
        /// <summary>
        /// 连接对应的tcpip
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        private void ConnectLaserIP(Socket socket,string ip,int port )
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
        /// <summary>
        /// 发送modbustcpip指令
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="vs">指令</param>
        private async void Writesingle(Socket socket, byte[] vs)
        {
            try
            {
                socket.Send(vs);
                var buffer = new byte[1024 * 1024];
                int msg;
                await Task.Run(() => msg = Aupclient.Receive(buffer));
                //  Aupclient.BeginReceive(buffer, 0,20,SocketFlags.None, ReceiveCallback, Aupclient);
            }
            catch (Exception ex)
            {
                Log.Info(socket.RemoteEndPoint.ToString() + ex.Message.ToString());
            }
           
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
        private void Button3_Click(object sender, EventArgs e)
        {
            InitializeModbus();
        }
        /// <summary>
        /// 初始化指令，重新建立接收线程；
        /// </summary>
        private async void InitializeModbus()
        {
            ModbusTcpMasterWriteSingleRegister();
            ReceiveModbus();
            await Task.Run(() => FormToolsFlash());
            await Task.Run(() => EstimateWheel());
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Button1_Click( this,null);
                       
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
            int contAud = 0;//出现频率
            int contAlr = 0;
            int contBud = 0;
            int contBlr = 0;
            int wheelA = 2;//判断A测是否正常
            int wheelB = 2;//判断B测是否正常
            var t0 = DateTime.Now;
            ldms80.InitializeABlaser();
            while (TstartWheel)
            {
                if (!stopTemp)
                {
                    if (tboolAup && tboolAdown)
                    {
                        if (JudgeLaser(TAup, TAdown, ldms80.rAup, ldms80.rAdown, ldms80.rAup2Adown))
                        {
                            contAud++;
                            tboolAup = false; tboolAdown = false;
                            ldms80.Aup = (ldms80.Aup * contAud + TAup) / (contAud + 1);
                            ldms80.Adown = (ldms80.Adown * contAud + TAdown) / (contAud + 1);
                        }
                    }
                    if (tboolAleft && tboolAright)
                    {
                        if (JudgeLaser(TAleft, TAright, ldms80.rAleft, ldms80.rAright, ldms80.rAleft2Aright))
                        {
                            contAlr++;
                            tboolAleft = false; tboolAright = false;
                            ldms80.Aleft = (ldms80.Aleft * contAlr + TAleft) / (contAlr + 1);
                            ldms80.Aright = (ldms80.Aright * contAlr + TAright) / (contAlr + 1);

                        }
                    }
                    if (tboolBup && tboolBdown)
                    {
                        if (JudgeLaser(TBup, TBdown, ldms80.rBup, ldms80.rBdown, ldms80.rBup2Bdown))
                        {
                            contBud++;
                            tboolBup = false; tboolBdown = false;
                            ldms80.Bup = (ldms80.Bup * contBud + TBup) / (contBud + 1);
                            ldms80.Bdown = (ldms80.Bdown * contBud + TBdown) / (contBud + 1);
                        }
                    }
                    if (tboolBleft && tboolBright)
                    {
                        if (JudgeLaser(TBleft, TBright, ldms80.rBleft, ldms80.rBright, ldms80.rBleft2Bright))
                        {
                            contBlr++;
                            tboolBleft = false; tboolBright = false;
                            ldms80.Bleft = (ldms80.Bleft * contBlr + TBleft) / (contBlr + 1);
                            ldms80.Bright = (ldms80.Bright * contBlr + TBright) / (contBlr + 1);
                        }
                    }
                    TimeSpan timeSpan = DateTime.Now - t0;
                    ////change 判断条件
                    if (timeSpan.TotalSeconds > ldms80.rwheeltimeSpanmin)
                    {
                        if (contAud > ldms80.rwheelpoint && contAlr > ldms80.rwheelpoint)
                            wheelA = 1;
                        if (contBud > ldms80.rwheelpoint && contBlr > ldms80.rwheelpoint)//判断时间间隔，输出测量值
                            wheelB = 1;
                        if (wheelA == 1 && wheelB == 1)

                        {
                            t0 = DateTime.Now;
                            contAud = 0;
                            contAlr = 0;
                            contBud = 0;
                            contBlr = 0;
                            ldms80.writeWheelsave(Wheeldata(ldms80.Aup, ldms80.Adown, ldms80.Aleft, ldms80.Aright, wheelA.ToString(), ldms80.Bup, ldms80.Bdown, ldms80.Bleft, ldms80.Bright, wheelB.ToString()));
                            ldms80.InitializeABlaser();
                            wheelB = 2;
                            wheelA = 2;
                            stopTemp = true;
                            sentLaser(ldms80.SendBufStopAup);
                            timer1.Interval = (int)ldms80.rwheeltimeSpanmin * 1000;
                            timer1.Start();

                        }
                    }
                    if (timeSpan.TotalSeconds > ldms80.rwheeltimeSpanmax)
                    {
                        t0 = DateTime.Now;
                        contAud = 0;
                        contAlr = 0;
                        contBud = 0;
                        contBlr = 0;
                        ldms80.writeWheelsave(Wheeldata(ldms80.Aup, ldms80.Adown, ldms80.Aleft, ldms80.Aright, wheelA.ToString(), ldms80.Bup, ldms80.Bdown, ldms80.Bleft, ldms80.Bright, wheelB.ToString()));
                        ldms80.InitializeABlaser();
                        wheelB = 2;
                        wheelA = 2;
                        stopTemp = true;
                        sentLaser(ldms80.SendBufStopAup);
                        ControlInvoker.Invoke(this, delegate
                        {
                            timer1.Interval = (int)ldms80.rwheeltimeSpanmin * 1000;                           
                            timer1.Enabled = true;
                            timer1.Start();
                        });
                    }
                }
                Task.Delay(1);
            }
        }
        private string Wheeldata(double aup,double adown,double aleft,double aright,string  a,double bup,double bdown,double bleft,double bright,string b )
        {
            double aud = aup - adown;
            double alr = aleft - aright;
            double bud = bup - bdown;
            double blr = bleft - bright;
            string data =ldms80.WheelID()+"&"+ldms80.rAup2Adown.ToString("#0.000") +"&"+ldms80.rAleft2Aright.ToString("#0.000")
                + "$"+aup.ToString("#0.000") + "&"+adown.ToString("#0.000") + "&"+aud.ToString("#0.000") + "&"+aleft.ToString("#0.000") + "&"+aright.ToString("#0.000") + "&"+alr.ToString("#0.000") + "&"+a+"&"+ldms80.rwheelSpeed.ToString("#0.00")
                + "$"+ bup.ToString("#0.000") + "&" + bdown.ToString("#0.000") + "&" + bud.ToString("#0.000") + "&" + bleft.ToString("#0.000") + "&" + bright.ToString("#0.000") + "&" + blr.ToString("#0.000") + "&" + b +"&"+ ldms80.rwheelSpeed.ToString("#0.00") + "#";
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
     

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            sentLaser(ldms80.SendBufStartAup);
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

  


}
