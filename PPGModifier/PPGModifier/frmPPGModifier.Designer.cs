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
      btnMoveCompaction = new Button();
      btnMoveCutPrepare = new Button();
      button1 = new Button();
      SuspendLayout();
      // 
      // btnDoIt
      // 
      btnDoIt.Location = new Point(158, 209);
      btnDoIt.Margin = new Padding(1);
      btnDoIt.Name = "btnDoIt";
      btnDoIt.Size = new Size(331, 41);
      btnDoIt.TabIndex = 0;
      btnDoIt.Text = "Do The Thing";
      btnDoIt.UseVisualStyleBackColor = true;
      btnDoIt.Click += btnDoIt_Click;
      // 
      // btnDecoupleROTX
      // 
      btnDecoupleROTX.Location = new Point(158, 270);
      btnDecoupleROTX.Name = "btnDecoupleROTX";
      btnDecoupleROTX.Size = new Size(331, 39);
      btnDecoupleROTX.TabIndex = 1;
      btnDecoupleROTX.Text = "Decouple ROTX";
      btnDecoupleROTX.UseVisualStyleBackColor = true;
      btnDecoupleROTX.Click += btnDecoupleROTX_Click;
      // 
      // btnMoveCompaction
      // 
      btnMoveCompaction.Location = new Point(158, 325);
      btnMoveCompaction.Name = "btnMoveCompaction";
      btnMoveCompaction.Size = new Size(331, 39);
      btnMoveCompaction.TabIndex = 2;
      btnMoveCompaction.Text = "Move Compaction Brake";
      btnMoveCompaction.UseVisualStyleBackColor = true;
      btnMoveCompaction.Click += btnMoveCompactionBrake;
      // 
      // btnMoveCutPrepare
      // 
      btnMoveCutPrepare.Location = new Point(158, 381);
      btnMoveCutPrepare.Name = "btnMoveCutPrepare";
      btnMoveCutPrepare.Size = new Size(331, 39);
      btnMoveCutPrepare.TabIndex = 3;
      btnMoveCutPrepare.Text = "Move Cut Prepare";
      btnMoveCutPrepare.UseVisualStyleBackColor = true;
      btnMoveCutPrepare.Click += btnMoveCutPrepare_Click;
      // 
      // button1
      // 
      button1.Location = new Point(158, 156);
      button1.Margin = new Padding(1);
      button1.Name = "button1";
      button1.Size = new Size(331, 41);
      button1.TabIndex = 4;
      button1.Text = "Do The Thing 1.12";
      button1.UseVisualStyleBackColor = true;
      button1.Click += button1_Click;
      // 
      // frmPPGModifier
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(705, 546);
      Controls.Add(button1);
      Controls.Add(btnMoveCutPrepare);
      Controls.Add(btnMoveCompaction);
      Controls.Add(btnDecoupleROTX);
      Controls.Add(btnDoIt);
      Margin = new Padding(1);
      Name = "frmPPGModifier";
      Text = "Form1";
      Load += frmPPGModifier_Load;
      ResumeLayout(false);
    }

    #endregion

    private Button btnDoIt;
    private Button btnDecoupleROTX;
    private Button btnMoveCompaction;
    private Button btnMoveCutPrepare;
    private Button button1;
  }
}
