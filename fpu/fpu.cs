using Hugin.ExDevice;
using Hugin.GMPCommon;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hugin.POS.CompactPrinter.FP300;
using System.Reflection;
using Hugin.Common;
using System.Xml.Linq;
using tigert300;
using System.Diagnostics;
using System.Numerics;


namespace tigert300
{
    public class fpu():IBridge
    {
        private static bool isMatchedBefore = false;
        private static ICompactPrinter printer = null;
        public static string txtLog = "";
        public static string fiscalId;


        #region Signingincashier
        public void Signingincashier(int id, string password)
        {

            try
            {
                ParseResponse(new CPResponse(this.Printer.SignInCashier(id, password)));
            }
            catch (System.Exception ex)
            {
                this.Log("Hata: " + ex.Message);
            }
        }
        #endregion

        #region ParseResponse
        private void ParseResponse(CPResponse response)
        {
            try
            {
                if (response.ErrorCode == 0)
                {
                    string retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        this.Log(String.Format(FormMessage.DATE.PadRight(12, ' ') + ":{0}", retVal));
                    }

                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        this.Log(String.Format(FormMessage.TIME.PadRight(12, ' ') + ":{0}", retVal));
                    }
                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        this.Log(String.Format("NOTE".PadRight(12, ' ') + ":{0}", retVal));
                    }
                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        this.Log(String.Format(FormMessage.AMOUNT.PadRight(12, ' ') + ":{0}", retVal));
                    }
                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        this.Log(FormMessage.DOCUMENT_ID.PadRight(12, ' ') + ":" + retVal);
                    }

                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                    }

                    retVal = response.GetNextParam();
                    if (!String.IsNullOrEmpty(retVal))
                    {
                        String authNote = "";
                        try
                        {
                            switch (int.Parse(retVal))
                            {
                                case 0:
                                    authNote = FormMessage.SALE;
                                    break;
                                case 1:
                                    authNote = "PROGRAM";
                                    break;
                                case 2:
                                    authNote = FormMessage.SALE + " & Z";
                                    break;
                                case 3:
                                    authNote = FormMessage.ALL;
                                    break;
                                default:
                                    authNote = "";
                                    break;
                            }

                            this.Log(FormMessage.AUTHORIZATION.PadRight(12, ' ') + ":" + authNote);
                        }
                        catch { }
                    }
                }

            }
            catch (Exception ex)
            {
                this.Log(FormMessage.OPERATION_FAILS + ": " + ex.Message);
            }
        }

        #endregion

        #region Log
        private delegate void LogDelegate(String log);
        public void Log(string log)
        {
          
            
                txtLog= txtLog+("# " + log + "\r\n");
               
        }
        private delegate void LogDelegate2();
        public void Log()
        {
           
          
                // 1 Command
                // 2 Sequnce Number
                // 3 FPU State
                // 4 Error Code
                // 5 Error Message

                if (printer != null)
                {
                    string lastlog = printer.GetLastLog();


                txtLog = txtLog + ("***************************************************" + Environment.NewLine);

                    if (!String.IsNullOrEmpty(lastlog))
                    {
                        if (!lastlog.Contains("|"))
                        {
                            Log(lastlog);
                            return;
                        }

                        string[] parsedLog = lastlog.Split('|');

                        if (parsedLog.Length == 5)
                        {

                            string command = parsedLog[0];
                            string sequnce = parsedLog[1];
                            string state = parsedLog[2];
                            string errorCode = parsedLog[3];
                            string errorMsg = parsedLog[4];

                            if (command != "NULL")
                            {
                              
                                if (sequnce.Length == 1)
                                txtLog = txtLog + (String.Format("{0} {1}:", sequnce, FormMessage.COMMAND.PadRight(12, ' ')));
                                else if (sequnce.Length == 2)
                                txtLog = txtLog + (String.Format("{0} {1}:", sequnce, FormMessage.COMMAND.PadRight(11, ' ')));
                                else
                                txtLog = txtLog + (String.Format("{0} {1}:", sequnce, FormMessage.COMMAND.PadRight(10, ' ')));



                                txtLog = txtLog + (command + "\t" + Environment.NewLine);


                                txtLog = txtLog + ("  " + FormMessage.FPU_STATE.PadRight(12, ' ') + ":");

                                txtLog = txtLog + (state + "\t" + Environment.NewLine);
                            }

                             txtLog = txtLog + ("  " + FormMessage.RESPONSE.PadRight(12, ' ') + ":");



                        txtLog = txtLog + (errorMsg + Environment.NewLine);

                        
                    }
                }
            }
        }


        #endregion

        #region FiscalId
        public string FiscalId
        {
            get { return fiscalId; }
            set { fiscalId = value; }
        }
        #endregion

        #region SetFiscalId
        public static void SetFiscalId(string strId)
        {
            int id = int.Parse(strId.Substring(2));

            if (id == 0 || id > 99999999)
            {
                throw new Exception("Geçersiz mali numara.");
            }
            fiscalId = strId;

            if (printer != null)
                printer.FiscalRegisterNo = fiscalId;
        }
        #endregion

        #region GetIPAddress
        private string GetIPAddress()
        {
            System.Net.IPHostEntry host;
            string localIP = "?";
            host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }
        #endregion

        #region Printer
        public ICompactPrinter Printer
        {
            get
            {
                return printer;
            }
        }
        #endregion

        #region conn
        private static IConnection conn;
        public IConnection Connection
        {
            get
            {
                return conn;
            }
            set
            {
                conn = value;
            }
        }
        #endregion

        #region  MatchExDevice       
        private void MatchExDevice()
        {
            SetFiscalId(fiscalId);

            // DeviceInfo sınıfı gerekli bilgiler ile doldurulur
            DeviceInfo serverInfo = new DeviceInfo();
            serverInfo.IP = System.Net.IPAddress.Parse(GetIPAddress());
            serverInfo.IPProtocol = IPProtocol.IPV4;

            serverInfo.Brand = "HUGIN";

            if (!String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Brand"]))
            {
                serverInfo.Brand = System.Configuration.ConfigurationManager.AppSettings["Brand"];
            }

            serverInfo.Model = "HUGIN COMPACT";
            serverInfo.Port = 4444;
            serverInfo.TerminalNo = fiscalId.PadLeft(8, '0');
            serverInfo.Version = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).LastWriteTime.ToShortDateString();
            try
            {
                // Motherboard serisi alınır
                serverInfo.SerialNum = CreateMD5(GetMBId()).Substring(0, 8);
            }
            catch
            {
                // Seri alınırken sıkıntı yaşanırsa default bir değer verilebilir
                serverInfo.SerialNum = "ABCD1234";
            }


            if (conn.IsOpen)
            {
                if (isMatchedBefore)
                {
                    // Eğer önceden eşleme yapıldıysa sadece connection objesinin kütüphaneye set edlmesi yeterli olacaktır.
                    printer.SetCommObject(conn.ToObject());
                    return;
                }
                try
                {
                    printer = new CompactPrinter();

                    // Eşleme öncesi ÖKC sicil numarası kütüphane üzerinde ilgili alana set edilir.
                    printer.FiscalRegisterNo = fiscalId;

                    /*kapattım  try
                      {
                          // Loglama yapılacak dizin ve log seviyesi istenirse set edilir. Opsiyonel seçeneklerdir. 
                          //Set edilmemesi durumunda default değerler kullanılır.
                          if (!String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["LogDirectory"]))
                          {
                              printer.LogDirectory = System.Configuration.ConfigurationManager.AppSettings["LogDirectory"];
                          }

                          printer.LogerLevel = int.Parse(System.Configuration.ConfigurationManager.AppSettings["LogLevel"]);
                      }
                      catch { }*/

                    // Eşleme başlatılır. Başarılı ise true, başarısız ise false döner.
                    if (!printer.Connect(conn.ToObject(), serverInfo))
                    {
                        throw new OperationCanceledException(FormMessage.UNABLE_TO_MATCH);
                    }

                    // ÖKC üzerinde desteklenen bağlantı kapasitesi kontrol edilir, oluşturulan connection ile farklı ise düzenleme yapılır.
                    // Check supported printer size and set if it is different
                    if (printer.PrinterBufferSize != conn.BufferSize)
                    {
                        conn.BufferSize = printer.PrinterBufferSize;
                    }
                    printer.SetCommObject(conn.ToObject());
                    isMatchedBefore = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                CPResponse.Bridge = this;
            }
        }
        #endregion

        #region CreateMD5
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
        #endregion

        #region GetMBId
        private string GetMBId()
        {
            System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            System.Management.ManagementObjectCollection moc = mos.Get();
            string motherBoard = "";
            foreach (System.Management.ManagementObject mo in moc)
            {
                motherBoard = (string)mo["SerialNumber"];
            }

            return motherBoard;
        }
        #endregion

        #region close
        public void close()
        {
            if (conn != null)
                conn.Close();
        }
        #endregion

        #region Connect
        public string Connect(bool ConnType,string serial_port,int baudrate,string ip,int port)
        {
            string return_msg = "";
            string errPrefix = FormMessage.CONNECTION_ERROR + ": ";
            try
            {
                if (this.Connection == null)
                {
                    if (ConnType)
                    {
                        // Seri port bağlantısı oluşturulur
                        this.Connection = new SerialConnection(serial_port, baudrate);
                    }
                    else
                    {
                        // TCP/IP bağlantı oluşturulur
                        this.Connection = new TCPConnection(ip, port);
                    }

                    this.Log(FormMessage.CONNECTING + "... (" + FormMessage.PLEASE_WAIT + ")");
                    this.Connection.Open();

                    errPrefix = FormMessage.MATCHING_ERROR + ": ";
                 
                    // ÖKC ile eşleme adımı
                    MatchExDevice();
                    Signingincashier(1, "1234"); // hangi aşamalarda hangi kasiyer ile login olunmalı sor ?
                    PrintDocument();
                    return_msg = txtLog;

                    this.Log(FormMessage.CONNECTED);
            
                }
                else
                {
                    this.Connection.Close();
                    this.Connection = null;
                    return_msg = txtLog;
                    this.Log(FormMessage.DISCONNECTED);
                }
              
            }
            catch (System.Exception ex)
            {
                this.Log(FormMessage.OPERATION_FAILS + ": " + errPrefix + ex.Message);

                if (conn != null)
                {
                    return_msg = txtLog;
                }
            }
            return txtLog;
        }
        #endregion

        public void PrintDocument()
        {

            try
            {

                CPResponse response_header = new CPResponse(Printer.PrintDocumentHeader());
                CPResponse response_item = new CPResponse(Printer.PrintItem(1, 1,1,"AA",null, -1, -1));
                CPResponse response_payment = new CPResponse(Printer.PrintPayment(0, 0, 99));
                CPResponse response_close = new CPResponse(Printer.CloseReceipt(false));
            }
            catch
            {
            }
        }
    }

    #region ContentType
    enum ContentType
    {
        NONE,
        REPORT,
        FILE
    }
    #endregion

    #region IConnection
    public interface IConnection
    {
        void Open();
        bool IsOpen { get; }
        void Close();
        int FPUTimeout { get; set; }
        object ToObject();
        int BufferSize { get; set; }
    }
    #endregion

    #region MySerialPort
    public class MySerialPort : SerialPort
    {
        public MySerialPort(string portName, int baudrate) :
            base(portName, baudrate)
        {
        }
#if ON_RDP
        protected override void Dispose(bool disposing)
        {
            // our variant for
            // 
            // http://social.msdn.microsoft.com/Forums/en-US/netfxnetcom/thread/8b02d5d0-b84e-447a-b028-f853d6c6c690
            // http://connect.microsoft.com/VisualStudio/feedback/details/140018/serialport-crashes-after-disconnect-of-usb-com-port

            var stream = (System.IO.Stream)typeof(SerialPort).GetField("internalSerialStream", 
                                                                    System.Reflection.BindingFlags.Instance | 
                                                                    System.Reflection.BindingFlags.NonPublic).GetValue(this);

            if (stream != null)
            {
                try { stream.Dispose(); }
                catch { }
            }

            base.Dispose(disposing);
        }
#endif
    }
    #endregion

    #region  SerialConnection
#if ON_RDP
    public class SerialConnection : IConnection, IDisposable
#else
    public class SerialConnection : IConnection
#endif
    {
        private string portName = String.Empty;
        private int baudRate = 115200;
        private static MySerialPort sp = null;
        private static int supportedBufferSize = ProgramConfig.DEFAULT_BUFFER_SIZE;

        public SerialConnection(string portName, int baudrate)
        {
            this.portName = portName;
            this.baudRate = baudrate;

            try
            {
                if (IsOpen)
                {
                    Close();
                }
            }
            catch
            {
            }
        }

#if ON_RDP
        ~SerialConnection()
        {
            Dispose();
        }
#endif

        public void Open()
        {
            sp = new MySerialPort(portName, baudRate);
            sp.WriteTimeout = 4500;
            sp.ReadTimeout = 4500;
            sp.ReadBufferSize = supportedBufferSize;
            sp.WriteBufferSize = supportedBufferSize;
            sp.Encoding = Form1.DefaultEncoding;
            sp.Open();
        }

        public bool IsOpen
        {
            get
            {
                if (sp != null && sp.IsOpen)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Close()
        {
            if (sp != null)
            {
                sp.Close();
            }
        }

        public int FPUTimeout
        {
            get
            {
                return sp.ReadTimeout;
            }
            set
            {
                sp.ReadTimeout = value;
            }
        }

        public object ToObject()
        {
            return sp;
        }

#if ON_RDP
        public void Dispose()
        {
            try
            {
                Close();
                sp = null;
            }
            catch
            {
            }
        }
#endif


        public int BufferSize
        {
            get
            {
                return sp.ReadBufferSize;
            }
            set
            {
                // Close the connection
                Close();
                // Set new buffer size
                supportedBufferSize = value;
                // Re-open the connection
                Open();
            }
        }
    }
    #endregion

    #region TCPConnection
    public class TCPConnection : IConnection, IDisposable
    {
        private Socket client = null;
        private string ipAddress = String.Empty;
        private int port = 0;
        private static int supportedBufferSize = ProgramConfig.DEFAULT_BUFFER_SIZE;

        public TCPConnection(String ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
        }

        // Destructor of this class.
        ~TCPConnection()
        {
            Dispose();
        }

        public void Open()
        {
            // Close if there is any idle connection
            this.Close();

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(this.ipAddress), this.port);
            client = new Socket(AddressFamily.InterNetwork,
                              SocketType.Stream, ProtocolType.Tcp);
            // Set initalize values
            client.ReceiveTimeout = 4500;
            client.ReceiveBufferSize = supportedBufferSize;
            client.SendBufferSize = supportedBufferSize;
            // Connect to destination
            client.Connect(ipep);
        }

        public bool IsOpen
        {
            get
            {
                if (client != null && client.Connected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Close()
        {
            if (IsOpen)
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }

        public int FPUTimeout
        {
            get
            {
                return client.ReceiveTimeout;
            }
            set
            {
                client.ReceiveTimeout = value;
            }
        }

        public int BufferSize
        {
            get
            {
                return client.SendBufferSize;
            }
            set
            {
                // Close the connection
                Close();
                // Set new buffer size
                supportedBufferSize = value;
                // Re-open the connection
                Open();
            }
        }

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch (System.Exception)
            {

            }
        }

        public object ToObject()
        {
            return client;
        }
    }
    #endregion

 
}
