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
        private double tAup { get; set; }
        private double tAdown { get; set; }
        private double tAleft { get; set; }
        private double tAright { get; set; }
        private double tBup { get; set; }
        private double tBdown { get; set; }
        private double tBleft { get; set; }
        private double tBright { get; set; }

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
        private bool tboolWheel { get; set; }
        //配置文件参数
        #endregion
        public Form1()
        {
            //var id = ldms80.WheelID();
            ldms80.readconfig();
            InitializeComponent();
           // Connect();
          
        }
        private void Form1_Load(object sender, EventArgs e)
        {
          //  BasicConfigurator.Configure();
            Log.Info(string.Format("程序启动时间为{0}.", DateTime.Now.ToString()));//测试日志用
            //Log.Info("201912271709插入一条消息");
            initializeModbus();


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
        /// <summary>
        /// 初始化modbus，建立连接
        /// </summary>
        public  void ModbusTcpMasterWriteSingleRegister()
        {
            //初始化modbusmaster
            modbusFactory = new ModbusFactory();
            //在本地测试 所以使用回环地址,modbus协议规定端口号 502
            masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
            //设置读取超时时间
            masterAup.Transport.ReadTimeout = 2000;
            masterAup.Transport.Retries = 2000;
            masterAdown = modbusFactory.CreateMaster(new TcpClient("192.168.1.201", 502));
            masterAdown.Transport.ReadTimeout = 2000;
            masterAdown.Transport.Retries = 2000;
            masterAleft = modbusFactory.CreateMaster(new TcpClient("192.168.1.202", 502));
            masterAleft.Transport.ReadTimeout = 2000;
            masterAleft.Transport.Retries = 2000;
            masterAright = modbusFactory.CreateMaster(new TcpClient("192.168.1.203", 502));
            masterAright.Transport.ReadTimeout = 2000;
            masterAright.Transport.Retries = 2000;
            masterBup = modbusFactory.CreateMaster(new TcpClient("192.168.1.204", 502));
            masterBup.Transport.ReadTimeout = 2000;
            masterBup.Transport.Retries = 2000;
            masterBdown = modbusFactory.CreateMaster(new TcpClient("192.168.1.205", 502));
            masterBdown.Transport.ReadTimeout = 2000;
            masterBdown.Transport.Retries = 2000;
            masterBleft = modbusFactory.CreateMaster(new TcpClient("192.168.1.206", 502));
            masterBleft.Transport.ReadTimeout = 2000;
            masterBleft.Transport.Retries = 2000;
            masterBright = modbusFactory.CreateMaster(new TcpClient("192.168.1.207", 502));
            masterBright.Transport.ReadTimeout = 2000;
            masterBright.Transport.Retries = 2000;            
            // WriteSingleRegister
        }
        private async void readAupParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                try
                {
                    //重新实例化是为了 modbus slave更换连接时不报错
                    // masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                    //读取保持寄存器
                    var registerBuffer = masterAup.ReadHoldingRegisters(01, 0, 17);
                    double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                    tAup = v;
                    tboolAup = true;
                    string dist = v.ToString();
                    Log.laser("Aup&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                    if(registerBuffer[10]!=0)
                    {
                        connectAup();
                        Log.Info("Aup激光测距仪出现故障重启,故障代码："+registerBuffer[10]);
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
        }
        private async void readAdownParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                try
                {
                    //重新实例化是为了 modbus slave更换连接时不报错
                    // masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                    //读取保持寄存器
                    var registerBuffer = masterAdown.ReadHoldingRegisters(01, 0, 17);
                    double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                    tAdown = v;
                    tboolAdown = true;
                    string dist = v.ToString();
                    Log.laser("Adown&"+DateTime.Now.ToString("HH:mm:ss.fff") +"&"+ dist);
                    if (registerBuffer[10] != 0)
                    {
                        connectAdown();
                        Log.Info("Adown激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                    }
                    ControlInvoker.Invoke(this, delegate
                    {
                        textBox_Adown.Text = dist;
                    });
                    ControlInvoker.Invoke(this, delegate
                    {
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
        }
        private async void readAleftParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                try
                {
                    //重新实例化是为了 modbus slave更换连接时不报错
                    // masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                    //读取保持寄存器
                    var registerBuffer = masterAleft.ReadHoldingRegisters(01, 0, 17);
                    double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                    tAleft = v;
                    tboolAleft = true;
                    string dist = v.ToString();
                    Log.laser("Aleft&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                    if (registerBuffer[10] != 0)
                    {
                        connectAleft();
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
        }
        private async void readArightParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                try
                {
                    //重新实例化是为了 modbus slave更换连接时不报错
                    // masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                    //读取保持寄存器
                    var registerBuffer = masterAright.ReadHoldingRegisters(01, 0, 17);
                    double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                    tAright = v;
                    tboolAright = true;
                    string dist = v.ToString();
                    Log.laser("Aright&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                    if (registerBuffer[10] != 0)
                    {
                        connectAright();
                        Log.Info("Aright激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                    }
                    ControlInvoker.Invoke(this, delegate
                    {
                        textBox_Aright.Text = dist;

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
        }
        private async void readBupParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                try
                {                                      //读取保持寄存器
                    var registerBuffer = masterBup.ReadHoldingRegisters(01, 0, 17);
                    double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                    tBup = v;
                    tboolBup = true;
                    string dist = v.ToString();
                    Log.laser("Bup&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                    if (registerBuffer[10] != 0)
                    {
                        connectBup();
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
        }
        private async void readBdownParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                try
                {//读取保持寄存器
                    var registerBuffer = masterBdown.ReadHoldingRegisters(01, 0, 17);
                    double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                    tBdown = v;
                    tboolBdown = true;
                    string dist = v.ToString();
                    Log.laser("Bdown&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                    if (registerBuffer[10] != 0)
                    {
                        connectBdown();
                        Log.Info("Bdown激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                    }
                    ControlInvoker.Invoke(this, delegate
                    {
                        textBox_Bdown.Text = dist;
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
        }
        private async  void readBleftParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                try
                {
                    //重新实例化是为了 modbus slave更换连接时不报错
                    // masterAup = modbusFactory.CreateMaster(new TcpClient("192.168.1.200", 502));
                    //读取保持寄存器
                    var registerBuffer = masterBleft.ReadHoldingRegisters(01, 0, 17);
                    double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                    tBleft = v;
                    tboolBleft = true;
                    string dist = v.ToString();
                    Log.laser("Bleft&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                    if (registerBuffer[10] != 0)
                    {
                        connectBleft();
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
        }
        private async void readBrightParametersFunction()
        {
            startRecv = true;
            while (startRecv)
            {
                try
                {
                                       //读取保持寄存器
                    var registerBuffer = masterBright.ReadHoldingRegisters(01, 0, 17);
                    double v = (((registerBuffer[12] << 16) + registerBuffer[13]) * 0.0001);
                    tBright = v;
                    tboolBright = true;
                    string dist = v.ToString();
                    Log.laser("Bright&" + DateTime.Now.ToString("HH:mm:ss.fff") + "&" + dist);
                    if (registerBuffer[10] != 0)
                    {
                        connectBright();
                        Log.Info("Bright激光测距仪出现故障重启,故障代码：" + registerBuffer[10]);
                    }
                    ControlInvoker.Invoke(this, delegate
                    {
                        textBox_Bright.Text = dist;
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
        }
        /// <summary>
        /// 开启接收线程
        /// </summary>
        private async void ReceiveModbus()

        {
            try
            {
                await Task.Run(() => readAupParametersFunction());
                await Task.Run(() => readAdownParametersFunction());
                await Task.Run(() => readAleftParametersFunction());
                await Task.Run(() => readArightParametersFunction());
                await Task.Run(() => readBupParametersFunction());
                await Task.Run(() => readBdownParametersFunction());
                await Task.Run(() => readBleftParametersFunction());
                await Task.Run(() => readBrightParametersFunction());
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
        
        /// <summary>
        /// 停止测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)

        {
            masterAup.Dispose();
            startRecv = false;            
        }

        /// <summary>
        /// 打开测量模式,初始化测距仪
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            connectAup();
            connectAdown();
            connectAleft();
            connectAright();
            connectBup();
            connectBdown();
            connectBleft();
            connectBright();
        }

        private void connectAup()
        {
            connectLaserIP(Aupclient, "192.168.1.200", 502);
            if (Aupclient != null)
            {
                Writesingle(Aupclient, SendBufMode);
            }

        }
        private void connectAdown()
        {
            connectLaserIP(Adownclient, "192.168.1.201", 502);
            if (Adownclient != null)
            {
                Writesingle(Adownclient, SendBufMode);
            }
        }
        private void connectAleft()
        {
            connectLaserIP(Aleftclient, "192.168.1.202", 502);
            if (Aleftclient != null)
            {
                Writesingle(Aleftclient, SendBufMode);
            }
        }
        private void connectAright()
        {
            connectLaserIP(Arightclient, "192.168.1.203", 502);

            if (Arightclient != null)
            {
                Writesingle(Arightclient, SendBufMode);
            }
        }
        private void connectBup()
        {
            connectLaserIP(Bupclient, "192.168.1.204", 502);
            if (Bupclient != null)
            {
                Writesingle(Bupclient, SendBufMode);
            }
        }
        private void connectBdown()
        {
            connectLaserIP(Bdownclient, "192.168.1.205", 502);

            if (Bdownclient != null)
            {
                Writesingle(Bdownclient, SendBufMode);
            }

        }
        private void connectBleft()
        {
            connectLaserIP(Bleftclient, "192.168.1.206", 502);
            if (Bleftclient != null)
            {
                Writesingle(Bleftclient, SendBufMode);
            }

        }
        private void connectBright()
        {
            connectLaserIP(Brightclient, "192.168.1.207", 502);
            if (Brightclient != null)
            {
                Writesingle(Brightclient, SendBufMode);
            }
        }
        /// <summary>
        /// 连接对应的tcpip
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        private void connectLaserIP(Socket socket,string ip,int port )
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
                Log.Info(socket.RemoteEndPoint.ToString()+ ex.Message.ToString());
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
        private void button3_Click(object sender, EventArgs e)
        {
            initializeModbus();
        }
        /// <summary>
        /// 初始化指令，重新建立接收线程；
        /// </summary>
        private async void initializeModbus()
        {
            ModbusTcpMasterWriteSingleRegister();
            ReceiveModbus();
           await Task.Run(() => formToolsFlash());
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            button1_Click( this,null);
                       
        }
        //关闭窗体按钮 

        private async void formToolsFlash()
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
        private void estimateWheel()
        {
            tboolWheel = true;
            int contAud = 0;//出现频率
            int contAlr = 0;
            int contBud = 0;
            int contBlr = 0;
            var t0 = DateTime.Now;
            while (tboolWheel)
            {
                if (tboolAup && tboolAdown )
                {
                    if (judgeLaser(tAup , tAdown, ldms80.rAup,ldms80.rAdown,ldms80.rAup2Adown))
                    {
                        contAud++;
                        tboolAup = tboolAdown = false;
                    }
                }
                if (tboolAleft && tboolAright)
                {
                    if (judgeLaser(tAleft, tAright,ldms80.rAleft,ldms80.rAright,ldms80.rAleft2Aright))
                    {
                        contAlr++;
                        tboolAleft = tboolAright = false;
                    }
                }
                if (tboolBup && tboolBdown)
                {
                    if (judgeLaser(tBup, tBdown, ldms80.rBup, ldms80.rBdown, ldms80.rBup2Bdown))
                    {
                        contBud++;
                        tboolBup = tboolBdown = false;
                    }
                }
                if (tboolBleft && tboolBright)
                {
                    if (judgeLaser(tBleft, tBright, ldms80.rBleft, ldms80.rBright, ldms80.rBleft2Bright))
                    {
                        contBlr++;
                        tboolBleft = tboolBright = false;
                    }
                }
                TimeSpan timeSpan = DateTime.Now - t0;
                ////change 判断条件
                if (timeSpan.TotalSeconds > ldms80.rwheeltimeSpanmin && contAud > ldms80.rwheelpoint && contBud > ldms80.rwheelpoint)//判断时间间隔，输出测量值
                {
                    t0 = DateTime.Now;
                }
                if(timeSpan.TotalSeconds>ldms80.rwheeltimeSpanmax)
                {
                    t0 = DateTime.Now;
                }
            }
        }
        private bool judgeLaser(double d1,double d2,double rd1,double rd2,double para)
        {
            if ((Math.Abs(d1 - rd1) <= para && Math.Abs(d2 - rd2) <= para ))
                return true;
            else
                return false;
        }
        private bool judgeLaserB(double up, double down, double left, double right)
        {
            if ((Math.Abs(up - ldms80.rBup) <= ldms80.rBup2Bdown && Math.Abs(down - ldms80.rBdown) <= ldms80.rBup2Bdown && Math.Abs(left - ldms80.rBleft) <= ldms80.rBleft2Bright && Math.Abs(right - ldms80.rBright) <= ldms80.rBleft2Bright))
                return true;
            else
                return false;
        }

        private void timeWheel()
        {
           // System.Time
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
