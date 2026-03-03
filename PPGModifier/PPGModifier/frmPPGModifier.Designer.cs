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
      // frmPPGModifier
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
      BackgroundImageLayout = ImageLayout.Stretch;
      ClientSize = new Size(705, 546);
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
  }
}
