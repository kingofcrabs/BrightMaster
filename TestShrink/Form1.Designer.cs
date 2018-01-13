namespace TestShrink
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
            this.drawPts1 = new TestShrink.DrawPts();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNewPt = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // drawPts1
            // 
            this.drawPts1.Location = new System.Drawing.Point(12, 27);
            this.drawPts1.Name = "drawPts1";
            this.drawPts1.Size = new System.Drawing.Size(658, 483);
            this.drawPts1.TabIndex = 0;
            this.drawPts1.Text = "drawPts1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(22, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Pts";
            // 
            // txtNewPt
            // 
            this.txtNewPt.Location = new System.Drawing.Point(12, 526);
            this.txtNewPt.Name = "txtNewPt";
            this.txtNewPt.Size = new System.Drawing.Size(100, 20);
            this.txtNewPt.TabIndex = 2;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(118, 526);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(799, 638);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtNewPt);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.drawPts1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DrawPts drawPts1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNewPt;
        private System.Windows.Forms.Button btnAdd;
    }
}

