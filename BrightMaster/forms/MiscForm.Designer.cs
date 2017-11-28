namespace BrightMaster.forms
{
    partial class MiscForm
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
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtSaveFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkAutoFindBound = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.chkMannual = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(189, 96);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "浏览";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtSaveFolder
            // 
            this.txtSaveFolder.Location = new System.Drawing.Point(4, 70);
            this.txtSaveFolder.Name = "txtSaveFolder";
            this.txtSaveFolder.Size = new System.Drawing.Size(260, 20);
            this.txtSaveFolder.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "保存路径：";
            // 
            // chkAutoFindBound
            // 
            this.chkAutoFindBound.AutoSize = true;
            this.chkAutoFindBound.Location = new System.Drawing.Point(4, 11);
            this.chkAutoFindBound.Name = "chkAutoFindBound";
            this.chkAutoFindBound.Size = new System.Drawing.Size(98, 17);
            this.chkAutoFindBound.TabIndex = 4;
            this.chkAutoFindBound.Text = "自动识别边界";
            this.chkAutoFindBound.UseVisualStyleBackColor = true;
            this.chkAutoFindBound.CheckedChanged += new System.EventHandler(this.chkAutoFindBound_CheckedChanged);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(189, 125);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "确定";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // chkMannual
            // 
            this.chkMannual.AutoSize = true;
            this.chkMannual.Location = new System.Drawing.Point(4, 34);
            this.chkMannual.Name = "chkMannual";
            this.chkMannual.Size = new System.Drawing.Size(98, 17);
            this.chkMannual.TabIndex = 6;
            this.chkMannual.Text = "手动设置阙值";
            this.chkMannual.UseVisualStyleBackColor = true;
            this.chkMannual.CheckedChanged += new System.EventHandler(this.chkMannualThreshold_CheckedChanged);
            // 
            // MiscForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(268, 154);
            this.Controls.Add(this.chkMannual);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.chkAutoFindBound);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtSaveFolder);
            this.Controls.Add(this.btnBrowse);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MiscForm";
            this.Text = "其他设置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtSaveFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkAutoFindBound;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.CheckBox chkMannual;
    }
}