namespace ShipsAssistant
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
            this.lvShipDetails = new System.Windows.Forms.ListView();
            this.tvFilters = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // lvShipDetails
            // 
            this.lvShipDetails.Location = new System.Drawing.Point(43, 42);
            this.lvShipDetails.Name = "lvShipDetails";
            this.lvShipDetails.Size = new System.Drawing.Size(446, 261);
            this.lvShipDetails.TabIndex = 0;
            this.lvShipDetails.UseCompatibleStateImageBehavior = false;
            // 
            // tvFilters
            // 
            this.tvFilters.Location = new System.Drawing.Point(521, 42);
            this.tvFilters.Name = "tvFilters";
            this.tvFilters.Size = new System.Drawing.Size(223, 261);
            this.tvFilters.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(833, 335);
            this.Controls.Add(this.tvFilters);
            this.Controls.Add(this.lvShipDetails);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvShipDetails;
        private System.Windows.Forms.TreeView tvFilters;
    }
}

