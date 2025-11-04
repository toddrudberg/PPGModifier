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
      btnDecoupleROTX = new Button();
      SuspendLayout();
      // 
      // btnDoIt
      // 
      btnDoIt.Location = new Point(158, 209);
      btnDoIt.Margin = new Padding(1, 1, 1, 1);
      btnDoIt.Name = "btnDoIt";
      btnDoIt.Size = new Size(331, 41);
      btnDoIt.TabIndex = 0;
      btnDoIt.Text = "Do The Thing";
      btnDoIt.UseVisualStyleBackColor = true;
      btnDoIt.Click += btnDoIt_Click;
      // 
      // btnDecoupleROTX
      // 
      btnDecoupleROTX.Location = new Point(158, 254);
      btnDecoupleROTX.Name = "btnDecoupleROTX";
      btnDecoupleROTX.Size = new Size(331, 39);
      btnDecoupleROTX.TabIndex = 1;
      btnDecoupleROTX.Text = "Decouple ROTX";
      btnDecoupleROTX.UseVisualStyleBackColor = true;
      btnDecoupleROTX.Click += btnDecoupleROTX_Click;
      // 
      // frmPPGModifier
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(705, 376);
      Controls.Add(btnDecoupleROTX);
      Controls.Add(btnDoIt);
      Margin = new Padding(1, 1, 1, 1);
      Name = "frmPPGModifier";
      Text = "Form1";
      Load += frmPPGModifier_Load;
      ResumeLayout(false);
    }

    #endregion

    private Button btnDoIt;
    private Button btnDecoupleROTX;
  }
}
