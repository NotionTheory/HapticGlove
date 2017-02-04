namespace HapticGloveClientUI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.sensor0 = new System.Windows.Forms.ProgressBar();
            this.sensor4 = new System.Windows.Forms.ProgressBar();
            this.sensor3 = new System.Windows.Forms.ProgressBar();
            this.sensor2 = new System.Windows.Forms.ProgressBar();
            this.sensor1 = new System.Windows.Forms.ProgressBar();
            this.sensor5 = new System.Windows.Forms.ProgressBar();
            this.motor0 = new System.Windows.Forms.CheckBox();
            this.motor1 = new System.Windows.Forms.CheckBox();
            this.motor2 = new System.Windows.Forms.CheckBox();
            this.motor3 = new System.Windows.Forms.CheckBox();
            this.motor4 = new System.Windows.Forms.CheckBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // sensor0
            // 
            this.sensor0.Location = new System.Drawing.Point(12, 12);
            this.sensor0.Maximum = 255;
            this.sensor0.Name = "sensor0";
            this.sensor0.Size = new System.Drawing.Size(100, 23);
            this.sensor0.TabIndex = 0;
            // 
            // sensor4
            // 
            this.sensor4.Location = new System.Drawing.Point(12, 128);
            this.sensor4.Maximum = 255;
            this.sensor4.Name = "sensor4";
            this.sensor4.Size = new System.Drawing.Size(100, 23);
            this.sensor4.TabIndex = 1;
            // 
            // sensor3
            // 
            this.sensor3.Location = new System.Drawing.Point(12, 99);
            this.sensor3.Maximum = 255;
            this.sensor3.Name = "sensor3";
            this.sensor3.Size = new System.Drawing.Size(100, 23);
            this.sensor3.TabIndex = 2;
            // 
            // sensor2
            // 
            this.sensor2.Location = new System.Drawing.Point(12, 70);
            this.sensor2.Maximum = 255;
            this.sensor2.Name = "sensor2";
            this.sensor2.Size = new System.Drawing.Size(100, 23);
            this.sensor2.TabIndex = 3;
            // 
            // sensor1
            // 
            this.sensor1.Location = new System.Drawing.Point(12, 41);
            this.sensor1.Maximum = 255;
            this.sensor1.Name = "sensor1";
            this.sensor1.Size = new System.Drawing.Size(100, 23);
            this.sensor1.TabIndex = 4;
            // 
            // sensor5
            // 
            this.sensor5.Location = new System.Drawing.Point(12, 157);
            this.sensor5.Maximum = 255;
            this.sensor5.Name = "sensor5";
            this.sensor5.Size = new System.Drawing.Size(100, 23);
            this.sensor5.TabIndex = 5;
            // 
            // motor0
            // 
            this.motor0.AutoSize = true;
            this.motor0.Location = new System.Drawing.Point(119, 12);
            this.motor0.Name = "motor0";
            this.motor0.Size = new System.Drawing.Size(59, 17);
            this.motor0.TabIndex = 6;
            this.motor0.Text = "Thumb";
            this.motor0.UseVisualStyleBackColor = true;
            this.motor0.CheckedChanged += new System.EventHandler(this.motor_CheckedChanged);
            // 
            // motor1
            // 
            this.motor1.AutoSize = true;
            this.motor1.Location = new System.Drawing.Point(119, 41);
            this.motor1.Name = "motor1";
            this.motor1.Size = new System.Drawing.Size(52, 17);
            this.motor1.TabIndex = 7;
            this.motor1.Text = "Index";
            this.motor1.UseVisualStyleBackColor = true;
            this.motor1.CheckedChanged += new System.EventHandler(this.motor_CheckedChanged);
            // 
            // motor2
            // 
            this.motor2.AutoSize = true;
            this.motor2.Location = new System.Drawing.Point(119, 70);
            this.motor2.Name = "motor2";
            this.motor2.Size = new System.Drawing.Size(57, 17);
            this.motor2.TabIndex = 8;
            this.motor2.Text = "Middle";
            this.motor2.UseVisualStyleBackColor = true;
            this.motor2.CheckedChanged += new System.EventHandler(this.motor_CheckedChanged);
            // 
            // motor3
            // 
            this.motor3.AutoSize = true;
            this.motor3.Location = new System.Drawing.Point(119, 99);
            this.motor3.Name = "motor3";
            this.motor3.Size = new System.Drawing.Size(48, 17);
            this.motor3.TabIndex = 9;
            this.motor3.Text = "Ring";
            this.motor3.UseVisualStyleBackColor = true;
            this.motor3.CheckedChanged += new System.EventHandler(this.motor_CheckedChanged);
            // 
            // motor4
            // 
            this.motor4.AutoSize = true;
            this.motor4.Location = new System.Drawing.Point(119, 128);
            this.motor4.Name = "motor4";
            this.motor4.Size = new System.Drawing.Size(55, 17);
            this.motor4.TabIndex = 10;
            this.motor4.Text = "Pinkie";
            this.motor4.UseVisualStyleBackColor = true;
            this.motor4.CheckedChanged += new System.EventHandler(this.motor_CheckedChanged);
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(12, 187);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(162, 23);
            this.connectButton.TabIndex = 11;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(182, 221);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.motor4);
            this.Controls.Add(this.motor3);
            this.Controls.Add(this.motor2);
            this.Controls.Add(this.motor1);
            this.Controls.Add(this.motor0);
            this.Controls.Add(this.sensor5);
            this.Controls.Add(this.sensor1);
            this.Controls.Add(this.sensor2);
            this.Controls.Add(this.sensor3);
            this.Controls.Add(this.sensor4);
            this.Controls.Add(this.sensor0);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Haaaaa";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar sensor0;
        private System.Windows.Forms.ProgressBar sensor4;
        private System.Windows.Forms.ProgressBar sensor3;
        private System.Windows.Forms.ProgressBar sensor2;
        private System.Windows.Forms.ProgressBar sensor1;
        private System.Windows.Forms.ProgressBar sensor5;
        private System.Windows.Forms.CheckBox motor0;
        private System.Windows.Forms.CheckBox motor1;
        private System.Windows.Forms.CheckBox motor2;
        private System.Windows.Forms.CheckBox motor3;
        private System.Windows.Forms.CheckBox motor4;
        private System.Windows.Forms.Button connectButton;
    }
}

