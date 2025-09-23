namespace PPGModifier
{
    partial class frmPPGModifier
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      btnDoIt = new Button();
      SuspendLayout();
      // 
      // btnDoIt
      // 
      btnDoIt.Location = new Point(384, 572);
      btnDoIt.Name = "btnDoIt";
      btnDoIt.Size = new Size(803, 111);
      btnDoIt.TabIndex = 0;
      btnDoIt.Text = "Do The Thing";
      btnDoIt.UseVisualStyleBackColor = true;
      btnDoIt.Click += btnDoIt_Click;
      // 
      // frmPPGModifier
      // 
      AutoScaleDimensions = new SizeF(17F, 41F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(1712, 1027);
      Controls.Add(btnDoIt);
      Name = "frmPPGModifier";
      Text = "Form1";
      Load += frmPPGModifier_Load;
      ResumeLayout(false);
    }

    #endregion

    private Button btnDoIt;
  }
}
