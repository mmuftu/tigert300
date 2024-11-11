using Hugin.Common;
using Hugin.ExDevice;
using Hugin.POS.CompactPrinter.FP300;
using System.Reflection;
using System.Text;

namespace tigert300
{
    public partial class Form1 : Form
    {

        public static Encoding DefaultEncoding = Encoding.GetEncoding(1254);

        fpu Fpu = new fpu();


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
            if (radioButton_eth.Checked)
            {

                txtLog.Text = Fpu.Connect(false, "com1:", 115200, "192.168.1.58", 4444);
            }
            else
            {
                txtLog.Text = Fpu.Connect(true, "com1:", 115200, "192.168.1.58", 4444);
            }

           
        }
        #endregion

        #region Form1_FormClosing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Fpu.close();
            Dispose();
        }
        #endregion

        #region Form1_Load
        private void Form1_Load(object sender, EventArgs e)
        {
            Fpu.FiscalId = "FT40049085";
        }
        #endregion

    }





}
