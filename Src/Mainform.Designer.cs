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
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.mnuGame = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGameNew = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGameExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 24);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.RefreshOnResize = false;
            this.pnlMain.Size = new System.Drawing.Size(585, 451);
            this.pnlMain.TabIndex = 1;
            this.pnlMain.PaintBuffer += new System.Windows.Forms.PaintEventHandler(this.paintBuffer);
            this.pnlMain.Paint += new System.Windows.Forms.PaintEventHandler(this.paint);
            this.pnlMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mouseMove);
            this.pnlMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mouseDown);
            this.pnlMain.Resize += new System.EventHandler(this.resize);
            this.pnlMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mouseUp);
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuGame});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Size = new System.Drawing.Size(585, 24);
            this.mnuMain.TabIndex = 0;
            this.mnuMain.Text = "menuStrip1";
            // 
            // mnuGame
            // 
            this.mnuGame.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuGameNew,
            this.mnuGameExit});
            this.mnuGame.Name = "mnuGame";
            this.mnuGame.Size = new System.Drawing.Size(46, 20);
            this.mnuGame.Text = "&Game";
            // 
            // mnuGameNew
            // 
            this.mnuGameNew.Name = "mnuGameNew";
            this.mnuGameNew.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.mnuGameNew.Size = new System.Drawing.Size(169, 22);
            this.mnuGameNew.Text = "Start &new game";
            this.mnuGameNew.Click += new System.EventHandler(this.startNewGame);
            // 
            // mnuGameExit
            // 
            this.mnuGameExit.Name = "mnuGameExit";
            this.mnuGameExit.Size = new System.Drawing.Size(169, 22);
            this.mnuGameExit.Text = "E&xit";
            this.mnuGameExit.Click += new System.EventHandler(this.exit);
            // 
            // Mainform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 475);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.mnuMain);
            this.Name = "Mainform";
            this.Text = "PieceDeal";
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private RT.Util.Controls.DoubleBufferedPanel pnlMain;
        private System.Windows.Forms.MenuStrip mnuMain;
        private System.Windows.Forms.ToolStripMenuItem mnuGame;
        private System.Windows.Forms.ToolStripMenuItem mnuGameNew;
        private System.Windows.Forms.ToolStripMenuItem mnuGameExit;
    }
}

