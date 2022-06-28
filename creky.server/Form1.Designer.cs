namespace creky.server
{
    partial class creky
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
            if (disposing && (components != null))
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
            this.tbInput = new System.Windows.Forms.TextBox();
            this.btnEncrypt = new System.Windows.Forms.Button();
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.tbKeyIn = new System.Windows.Forms.TextBox();
            this.tbKeyOut = new System.Windows.Forms.TextBox();
            this.btnBruteForce = new System.Windows.Forms.Button();
            this.tbBytesIn = new System.Windows.Forms.TextBox();
            this.tbMsgOut = new System.Windows.Forms.TextBox();
            this.tbKeyInDe = new System.Windows.Forms.TextBox();
            this.tbTextOutDe = new System.Windows.Forms.TextBox();
            this.btnDecrypt = new System.Windows.Forms.Button();
            this.tbBytesInDe = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbInput
            // 
            this.tbInput.Location = new System.Drawing.Point(12, 30);
            this.tbInput.Name = "tbInput";
            this.tbInput.Size = new System.Drawing.Size(268, 20);
            this.tbInput.TabIndex = 0;
            // 
            // btnEncrypt
            // 
            this.btnEncrypt.Location = new System.Drawing.Point(12, 57);
            this.btnEncrypt.Name = "btnEncrypt";
            this.btnEncrypt.Size = new System.Drawing.Size(230, 23);
            this.btnEncrypt.TabIndex = 1;
            this.btnEncrypt.Text = "Encrypt";
            this.btnEncrypt.UseVisualStyleBackColor = true;
            this.btnEncrypt.Click += new System.EventHandler(this.btnEncrypt_Click);
            // 
            // tbOutput
            // 
            this.tbOutput.Location = new System.Drawing.Point(12, 86);
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.Size = new System.Drawing.Size(268, 20);
            this.tbOutput.TabIndex = 2;
            // 
            // tbKeyIn
            // 
            this.tbKeyIn.Location = new System.Drawing.Point(286, 30);
            this.tbKeyIn.Name = "tbKeyIn";
            this.tbKeyIn.Size = new System.Drawing.Size(268, 20);
            this.tbKeyIn.TabIndex = 4;
            this.tbKeyIn.Text = "18238004";
            // 
            // tbKeyOut
            // 
            this.tbKeyOut.Location = new System.Drawing.Point(12, 196);
            this.tbKeyOut.Name = "tbKeyOut";
            this.tbKeyOut.Size = new System.Drawing.Size(268, 20);
            this.tbKeyOut.TabIndex = 7;
            // 
            // btnBruteForce
            // 
            this.btnBruteForce.Location = new System.Drawing.Point(12, 167);
            this.btnBruteForce.Name = "btnBruteForce";
            this.btnBruteForce.Size = new System.Drawing.Size(230, 23);
            this.btnBruteForce.TabIndex = 6;
            this.btnBruteForce.Text = "Brute Force";
            this.btnBruteForce.UseVisualStyleBackColor = true;
            this.btnBruteForce.Click += new System.EventHandler(this.btnBruteForce_Click);
            // 
            // tbBytesIn
            // 
            this.tbBytesIn.Location = new System.Drawing.Point(12, 140);
            this.tbBytesIn.Name = "tbBytesIn";
            this.tbBytesIn.Size = new System.Drawing.Size(542, 20);
            this.tbBytesIn.TabIndex = 5;
            // 
            // tbMsgOut
            // 
            this.tbMsgOut.Location = new System.Drawing.Point(286, 196);
            this.tbMsgOut.Name = "tbMsgOut";
            this.tbMsgOut.Size = new System.Drawing.Size(268, 20);
            this.tbMsgOut.TabIndex = 8;
            // 
            // tbKeyInDe
            // 
            this.tbKeyInDe.Location = new System.Drawing.Point(286, 252);
            this.tbKeyInDe.Name = "tbKeyInDe";
            this.tbKeyInDe.Size = new System.Drawing.Size(268, 20);
            this.tbKeyInDe.TabIndex = 12;
            this.tbKeyInDe.Text = "18238004";
            // 
            // tbTextOutDe
            // 
            this.tbTextOutDe.Location = new System.Drawing.Point(12, 308);
            this.tbTextOutDe.Name = "tbTextOutDe";
            this.tbTextOutDe.Size = new System.Drawing.Size(268, 20);
            this.tbTextOutDe.TabIndex = 11;
            // 
            // btnDecrypt
            // 
            this.btnDecrypt.Location = new System.Drawing.Point(12, 279);
            this.btnDecrypt.Name = "btnDecrypt";
            this.btnDecrypt.Size = new System.Drawing.Size(230, 23);
            this.btnDecrypt.TabIndex = 10;
            this.btnDecrypt.Text = "Decrypt";
            this.btnDecrypt.UseVisualStyleBackColor = true;
            this.btnDecrypt.Click += new System.EventHandler(this.btnDecrypt_Click);
            // 
            // tbBytesInDe
            // 
            this.tbBytesInDe.Location = new System.Drawing.Point(12, 252);
            this.tbBytesInDe.Name = "tbBytesInDe";
            this.tbBytesInDe.Size = new System.Drawing.Size(268, 20);
            this.tbBytesInDe.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 232);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Decrypt";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 120);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Brute froce";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Encrypt";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // creky
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(643, 348);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbKeyInDe);
            this.Controls.Add(this.tbTextOutDe);
            this.Controls.Add(this.btnDecrypt);
            this.Controls.Add(this.tbBytesInDe);
            this.Controls.Add(this.tbMsgOut);
            this.Controls.Add(this.tbKeyOut);
            this.Controls.Add(this.btnBruteForce);
            this.Controls.Add(this.tbBytesIn);
            this.Controls.Add(this.tbKeyIn);
            this.Controls.Add(this.tbOutput);
            this.Controls.Add(this.btnEncrypt);
            this.Controls.Add(this.tbInput);
            this.Name = "creky";
            this.Text = "creky";
            this.Load += new System.EventHandler(this.creky_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbInput;
        private System.Windows.Forms.Button btnEncrypt;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.TextBox tbKeyIn;
        private System.Windows.Forms.TextBox tbKeyOut;
        private System.Windows.Forms.Button btnBruteForce;
        private System.Windows.Forms.TextBox tbBytesIn;
        private System.Windows.Forms.TextBox tbMsgOut;
        private System.Windows.Forms.TextBox tbKeyInDe;
        private System.Windows.Forms.TextBox tbTextOutDe;
        private System.Windows.Forms.Button btnDecrypt;
        private System.Windows.Forms.TextBox tbBytesInDe;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}

