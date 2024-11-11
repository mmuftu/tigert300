namespace tigert300
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button_connect = new Button();
            txtLog = new RichTextBox();
            radioButton_eth = new RadioButton();
            radioButton_serial = new RadioButton();
            SuspendLayout();
            // 
            // button_connect
            // 
            button_connect.Location = new Point(26, 19);
            button_connect.Name = "button_connect";
            button_connect.Size = new Size(170, 23);
            button_connect.TabIndex = 0;
            button_connect.Text = "Connect()";
            button_connect.UseVisualStyleBackColor = true;
            button_connect.Click += button_connect_Click;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(20, 61);
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(347, 584);
            txtLog.TabIndex = 3;
            txtLog.Text = "";
            // 
            // radioButton_eth
            // 
            radioButton_eth.AutoSize = true;
            radioButton_eth.Location = new Point(216, 25);
            radioButton_eth.Name = "radioButton_eth";
            radioButton_eth.Size = new Size(69, 19);
            radioButton_eth.TabIndex = 4;
            radioButton_eth.TabStop = true;
            radioButton_eth.Text = "Ethernet";
            radioButton_eth.UseVisualStyleBackColor = true;
            // 
            // radioButton_serial
            // 
            radioButton_serial.AutoSize = true;
            radioButton_serial.Location = new Point(302, 25);
            radioButton_serial.Name = "radioButton_serial";
            radioButton_serial.Size = new Size(44, 19);
            radioButton_serial.TabIndex = 5;
            radioButton_serial.TabStop = true;
            radioButton_serial.Text = "Seri";
            radioButton_serial.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(385, 668);
            Controls.Add(radioButton_serial);
            Controls.Add(radioButton_eth);
            Controls.Add(txtLog);
            Controls.Add(button_connect);
            Name = "Form1";
            Text = "Form1";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button_connect;
        private RichTextBox txtLog;
        private RadioButton radioButton_eth;
        private RadioButton radioButton_serial;
    }
}
