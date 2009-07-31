namespace PieceDeal
{
    partial class Mainform
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
            this.pnlMain = new RT.Util.Controls.DoubleBufferedPanel();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRoll
            // 
            // pnlMain
            // 
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(585, 475);
            this.pnlMain.TabIndex = 1;
            this.pnlMain.PaintBuffer += new System.Windows.Forms.PaintEventHandler(this.paintBuffer);
            this.pnlMain.Paint += new System.Windows.Forms.PaintEventHandler(this.paint);
            this.pnlMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mouseMove);
            this.pnlMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mouseDown);
            this.pnlMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mouseUp);
            this.pnlMain.RefreshOnResize = false;
            // 
            // Mainform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 475);
            this.Controls.Add(this.pnlMain);
            this.Name = "Mainform";
            this.Text = "PieceDeal";
            pnlMain.Resize += new System.EventHandler(this.resize);
            this.pnlMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private RT.Util.Controls.DoubleBufferedPanel pnlMain;
    }
}

