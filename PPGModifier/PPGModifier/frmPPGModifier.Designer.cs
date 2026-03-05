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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPPGModifier));
      btnDoTheThingv1_12 = new Button();
      progressBar1 = new ProgressBar();
      label1 = new Label();
      linkLabel1 = new LinkLabel();
      SuspendLayout();
      // 
      // btnDoTheThingv1_12
      // 
      btnDoTheThingv1_12.Location = new Point(176, 19);
      btnDoTheThingv1_12.Margin = new Padding(1);
      btnDoTheThingv1_12.Name = "btnDoTheThingv1_12";
      btnDoTheThingv1_12.Size = new Size(331, 41);
      btnDoTheThingv1_12.TabIndex = 4;
      btnDoTheThingv1_12.Text = "Do The Thing 1.12";
      btnDoTheThingv1_12.UseVisualStyleBackColor = true;
      btnDoTheThingv1_12.Click += btnDoTheThingv1_12_Click;
      // 
      // progressBar1
      // 
      progressBar1.Location = new Point(176, 64);
      progressBar1.Name = "progressBar1";
      progressBar1.Size = new Size(331, 23);
      progressBar1.TabIndex = 5;
      // 
      // label1
      // 
      label1.Location = new Point(176, 90);
      label1.Name = "label1";
      label1.Size = new Size(331, 23);
      label1.TabIndex = 6;
      label1.Text = "label1";
      // 
      // linkLabel1
      // 
      linkLabel1.Location = new Point(76, 113);
      linkLabel1.Name = "linkLabel1";
      linkLabel1.Size = new Size(505, 23);
      linkLabel1.TabIndex = 7;
      linkLabel1.TabStop = true;
      linkLabel1.Text = "linkLabel1";
      linkLabel1.MouseUp += linkLabel1_MouseUp;
      // 
      // frmPPGModifier
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
      BackgroundImageLayout = ImageLayout.Stretch;
      ClientSize = new Size(705, 546);
      Controls.Add(linkLabel1);
      Controls.Add(label1);
      Controls.Add(progressBar1);
      Controls.Add(btnDoTheThingv1_12);
      DoubleBuffered = true;
      Icon = (Icon)resources.GetObject("$this.Icon");
      Margin = new Padding(1);
      Name = "frmPPGModifier";
      Text = "PPG Toolkit";
      Load += frmPPGModifier_Load;
      ResumeLayout(false);
    }

    #endregion
    private Button btnDoTheThingv1_12;
    private ProgressBar progressBar1;
    private Label label1;
    private LinkLabel linkLabel1;
  }
}
