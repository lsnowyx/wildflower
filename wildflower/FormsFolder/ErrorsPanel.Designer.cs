namespace wildflower.FormsFolder
{
    partial class ErrorsPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lbl_error = new Label();
            panel1 = new Panel();
            errorIco = new PictureBox();
            btn_yes = new Button();
            btn_close = new Button();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)errorIco).BeginInit();
            SuspendLayout();
            // 
            // lbl_error
            // 
            lbl_error.AutoSize = true;
            lbl_error.ForeColor = Color.White;
            lbl_error.Location = new Point(3, 31);
            lbl_error.Name = "lbl_error";
            lbl_error.Size = new Size(55, 15);
            lbl_error.TabIndex = 0;
            lbl_error.Text = "ErrorMsg";
            lbl_error.SizeChanged += lbl_error_SizeChanged;
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(errorIco);
            panel1.Controls.Add(btn_yes);
            panel1.Controls.Add(btn_close);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(150, 28);
            panel1.TabIndex = 1;
            // 
            // errorIco
            // 
            errorIco.BackColor = Color.Transparent;
            errorIco.Location = new Point(65, 3);
            errorIco.Name = "errorIco";
            errorIco.Size = new Size(20, 20);
            errorIco.TabIndex = 2;
            errorIco.TabStop = false;
            // 
            // btn_yes
            // 
            btn_yes.BackColor = Color.White;
            btn_yes.Enabled = false;
            btn_yes.ForeColor = Color.Black;
            btn_yes.Location = new Point(0, 3);
            btn_yes.Name = "btn_yes";
            btn_yes.Size = new Size(60, 23);
            btn_yes.TabIndex = 1;
            btn_yes.Text = "button2";
            btn_yes.UseVisualStyleBackColor = false;
            btn_yes.Visible = false;
            btn_yes.Click += btn_yes_Click;
            // 
            // btn_close
            // 
            btn_close.BackColor = Color.White;
            btn_close.Enabled = false;
            btn_close.ForeColor = Color.Black;
            btn_close.Location = new Point(90, 3);
            btn_close.Name = "btn_close";
            btn_close.Size = new Size(60, 23);
            btn_close.TabIndex = 0;
            btn_close.Text = "button1";
            btn_close.UseVisualStyleBackColor = false;
            btn_close.Visible = false;
            btn_close.Click += btn_close_Click;
            // 
            // ErrorsPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            Controls.Add(panel1);
            Controls.Add(lbl_error);
            Name = "ErrorsPanel";
            Size = new Size(150, 54);
            Load += ErrorsPanel_Load;
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)errorIco).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lbl_error;
        private Panel panel1;
        private Button btn_close;
        private Button btn_yes;
        private PictureBox errorIco;
    }
}
