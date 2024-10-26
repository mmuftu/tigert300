using Hugin.Common;
using Hugin.ExDevice;
using Hugin.POS.CompactPrinter.FP300;
using System.Reflection;
using System.Text;

namespace tigert300
{
    public partial class Form1 : Form,IBridge
    {
        //fpu fpu = new fpu();
        // SerialConnection serialConnection = new SerialConnection("COM1:", 115200);
      //   TCPConnection tcpConnection = new   TCPConnection("192.168.1.58", 4444);
        private static string fiscalId = "FT40049085";
        private static ICompactPrinter printer = null;
        public static Encoding DefaultEncoding = Encoding.GetEncoding(1254);
        private static bool isMatchedBefore = false;
        //private static IBridge bridge = null;

        #region Form1
        public Form1()
        {
            InitializeComponent();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
        #endregion

        #region button_connect_Click
        private void button_connect_Click(object sender, EventArgs e)
        {
            string errPrefix = FormMessage.CONNECTION_ERROR + ": ";
            try
            {
                if (this.Connection == null)
                {
                    if (radioButton_serial.Checked)
                    {
                        // Seri port baðlantýsý oluþturulur
                        this.Connection = new SerialConnection("COM1", 115200);
                    }
                    else
                    {
                        // TCP/IP baðlantý oluþturulur
                        int port = 4444;
                        this.Connection = new TCPConnection("192.168.1.58", port);
                    }

                    this.Log(FormMessage.CONNECTING + "... (" + FormMessage.PLEASE_WAIT + ")");
                    this.Connection.Open();
               
                    errPrefix = FormMessage.MATCHING_ERROR + ": ";

                    // ÖKC ile eþleme adýmý
                    MatchExDevice();
          
                    button_connect.Text = FormMessage.DISCONNECT;

                    this.Log(FormMessage.CONNECTED);
                    Signingincashier(1, "99"); // hangi aþamalarda hangi kasiyer ile login olunmalý sor ?
                }
                else
                {
                    this.Connection.Close();
                    this.Connection = null;
                    button_connect.Text = FormMessage.CONNECT;
                    this.Log(FormMessage.DISCONNECTED);
                }
            }
            catch (System.Exception ex)
            {
                this.Log(FormMessage.OPERATION_FAILS + ": " + errPrefix + ex.Message);

                if (conn != null)
                {
                    button_connect.Text = FormMessage.DISCONNECT;
                }
            }

        }
        #endregion

        #region Form1_FormClosing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (conn != null)
                conn.Close();

            Dispose();
        }
        #endregion

        #region Form1_Load
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        #endregion

        #region
        public static string FiscalId
        {
            get { return fiscalId; }
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

        #region  MatchExDevice       
        private void MatchExDevice()
        {
            SetFiscalId(fiscalId);

            // DeviceInfo sýnýfý gerekli bilgiler ile doldurulur
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
                // Motherboard serisi alýnýr
                serverInfo.SerialNum = CreateMD5(GetMBId()).Substring(0, 8);
            }
            catch
            {
                // Seri alýnýrken sýkýntý yaþanýrsa default bir deðer verilebilir
                serverInfo.SerialNum = "ABCD1234";
            }


            if (conn.IsOpen)
            {
                if (isMatchedBefore)
                {
                    // Eðer önceden eþleme yapýldýysa sadece connection objesinin kütüphaneye set edlmesi yeterli olacaktýr.
                    printer.SetCommObject(conn.ToObject());
                    return;
                }
                try
                {
                    printer = new CompactPrinter();

                    // Eþleme öncesi ÖKC sicil numarasý kütüphane üzerinde ilgili alana set edilir.
                    printer.FiscalRegisterNo = fiscalId;

                  /*kapattým  try
                    {
                        // Loglama yapýlacak dizin ve log seviyesi istenirse set edilir. Opsiyonel seçeneklerdir. 
                        //Set edilmemesi durumunda default deðerler kullanýlýr.
                        if (!String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["LogDirectory"]))
                        {
                            printer.LogDirectory = System.Configuration.ConfigurationManager.AppSettings["LogDirectory"];
                        }

                        printer.LogerLevel = int.Parse(System.Configuration.ConfigurationManager.AppSettings["LogLevel"]);
                    }
                    catch { }*/

                    // Eþleme baþlatýlýr. Baþarýlý ise true, baþarýsýz ise false döner.
                    if (!printer.Connect(conn.ToObject(), serverInfo))
                    {
                        throw new OperationCanceledException(FormMessage.UNABLE_TO_MATCH);
                    }

                    // ÖKC üzerinde desteklenen baðlantý kapasitesi kontrol edilir, oluþturulan connection ile farklý ise düzenleme yapýlýr.
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

        #region Log
        private delegate void LogDelegate(String log);
        public void Log(string log)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new LogDelegate(Log), log);
            }
            else
            {
                txtLog.AppendText("# " + log + "\r\n");
                txtLog.SelectionStart = txtLog.Text.Length;
                txtLog.ScrollToCaret();
            }
        }
        private delegate void LogDelegate2();
        public void Log()
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new LogDelegate2(Log));
            }
            else
            {
                // 1 Command
                // 2 Sequnce Number
                // 3 FPU State
                // 4 Error Code
                // 5 Error Message

                if (printer != null)
                {
                    string lastlog = printer.GetLastLog();

                    txtLog.SelectionColor = Color.CornflowerBlue;
                    txtLog.AppendText("***************************************************" + Environment.NewLine);

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
                                txtLog.SelectionColor = Color.White;
                                if (sequnce.Length == 1)
                                    txtLog.AppendText(String.Format("{0} {1}:", sequnce, FormMessage.COMMAND.PadRight(12, ' ')));
                                else if (sequnce.Length == 2)
                                    txtLog.AppendText(String.Format("{0} {1}:", sequnce, FormMessage.COMMAND.PadRight(11, ' ')));
                                else
                                    txtLog.AppendText(String.Format("{0} {1}:", sequnce, FormMessage.COMMAND.PadRight(10, ' ')));


                                txtLog.SelectionColor = Color.Red;
                                txtLog.AppendText(command + "\t" + Environment.NewLine);

                                txtLog.SelectionColor = Color.White;
                                txtLog.AppendText("  " + FormMessage.FPU_STATE.PadRight(12, ' ') + ":");
                                txtLog.SelectionColor = Color.LightSkyBlue;
                                txtLog.AppendText(state + "\t" + Environment.NewLine);
                            }
                            txtLog.SelectionColor = Color.White;
                            txtLog.AppendText("  " + FormMessage.RESPONSE.PadRight(12, ' ') + ":");

                            Color responseColor = Color.LimeGreen;

                            if (errorCode != "0")
                            {
                                responseColor = Color.Red;
                                if (state == FormMessage.NEED_SERVICE && errorCode != "3")
                                {
                                    responseColor = Color.Yellow;
                                }
                            }

                            txtLog.SelectionColor = responseColor;
                            txtLog.AppendText(errorMsg + Environment.NewLine);

                            txtLog.SelectionStart = txtLog.Text.Length;
                            txtLog.ScrollToCaret();
                            txtLog.SelectionColor = Color.White;
                        }
                    }
                }
            }
        }
        #endregion

        #region Signingincashier
        public void Signingincashier(int id,string password)
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



    }





}
