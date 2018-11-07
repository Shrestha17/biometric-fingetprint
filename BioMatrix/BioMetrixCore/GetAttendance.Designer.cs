namespace BioMetrixCore
{
    partial class GetAttendance
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
            this.btnGetAttendance = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnGetAttendance
            // 
            this.btnGetAttendance.Location = new System.Drawing.Point(94, 93);
            this.btnGetAttendance.Name = "btnGetAttendance";
            this.btnGetAttendance.Size = new System.Drawing.Size(114, 84);
            this.btnGetAttendance.TabIndex = 0;
            this.btnGetAttendance.Text = "GetAttendanceToDB";
            this.btnGetAttendance.UseVisualStyleBackColor = true;
            this.btnGetAttendance.Click += new System.EventHandler(this.btnGetAttendance_Click);
            // 
            // GetAttendance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Controls.Add(this.btnGetAttendance);
            this.Name = "GetAttendance";
            this.Text = "GetAttendance";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnGetAttendance;
    }
}