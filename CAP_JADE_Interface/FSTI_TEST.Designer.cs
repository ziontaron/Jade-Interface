namespace CAP_JADE_Interface
{
    partial class FSTI_TEST
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
            this.Fields = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.TransactionName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.LogListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // Fields
            // 
            this.Fields.Location = new System.Drawing.Point(12, 32);
            this.Fields.Name = "Fields";
            this.Fields.Size = new System.Drawing.Size(439, 20);
            this.Fields.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "FSTI Transaction Fields";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(500, 28);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 27);
            this.button1.TabIndex = 2;
            this.button1.Text = "Process";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TransactionName
            // 
            this.TransactionName.FormattingEnabled = true;
            this.TransactionName.Items.AddRange(new object[] {
            "POMT11",
            "POMT12",
            "POMT16",
            "PORV00",
            "PORV01",
            "MORV00",
            "ITMB03",
            "IMTR01"});
            this.TransactionName.Location = new System.Drawing.Point(137, 63);
            this.TransactionName.Name = "TransactionName";
            this.TransactionName.Size = new System.Drawing.Size(121, 21);
            this.TransactionName.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "FSTI Transaction";
            // 
            // LogListBox
            // 
            this.LogListBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.LogListBox.FormattingEnabled = true;
            this.LogListBox.Location = new System.Drawing.Point(0, 104);
            this.LogListBox.Name = "LogListBox";
            this.LogListBox.Size = new System.Drawing.Size(600, 355);
            this.LogListBox.TabIndex = 5;
            // 
            // FSTI_TEST
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 459);
            this.Controls.Add(this.LogListBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TransactionName);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Fields);
            this.Name = "FSTI_TEST";
            this.Text = "FSTI_TEST";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Fields;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox TransactionName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox LogListBox;
    }
}