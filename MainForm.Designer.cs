namespace DockerManager
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.flowLayoutPanelContainers = new System.Windows.Forms.FlowLayoutPanel();
            this.lblStatus = new MaterialSkin.Controls.MaterialLabel();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // flowLayoutPanelContainers
            // 
            this.flowLayoutPanelContainers.AutoScroll = true;
            this.flowLayoutPanelContainers.Location = new System.Drawing.Point(12, 80);
            this.flowLayoutPanelContainers.Name = "flowLayoutPanelContainers";
            this.flowLayoutPanelContainers.Size = new System.Drawing.Size(790, 620);
            this.flowLayoutPanelContainers.TabIndex = 0;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 730);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(43, 19);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "Ready";
            // 
            // refreshTimer
            // 
            this.refreshTimer.Enabled = true;
            this.refreshTimer.Interval = 3000;
            this.refreshTimer.Tick += new System.EventHandler(this.refreshTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 780);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.flowLayoutPanelContainers);
            this.Name = "MainForm";
            this.Text = "Docker Manager";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelContainers;
        private MaterialSkin.Controls.MaterialLabel lblStatus;
        private System.Windows.Forms.Timer refreshTimer;
    }
}
