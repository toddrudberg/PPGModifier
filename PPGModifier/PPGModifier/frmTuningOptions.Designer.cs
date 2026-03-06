namespace PPGModifier
{
  partial class TuningDialog
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
      propertyGrid1 = new PropertyGrid();
      btnOK = new Button();
      btnCancel = new Button();
      btnReset = new Button();
      SuspendLayout();
      // 
      // propertyGrid1
      // 
      propertyGrid1.Location = new Point(5, 10);
      propertyGrid1.Margin = new Padding(1);
      propertyGrid1.Name = "propertyGrid1";
      propertyGrid1.Size = new Size(803, 580);
      propertyGrid1.TabIndex = 0;
      // 
      // btnOK
      // 
      btnOK.DialogResult = DialogResult.OK;
      btnOK.Location = new Point(258, 623);
      btnOK.Margin = new Padding(1);
      btnOK.Name = "btnOK";
      btnOK.Size = new Size(77, 21);
      btnOK.TabIndex = 1;
      btnOK.Text = "OK";
      btnOK.UseVisualStyleBackColor = true;
      btnOK.Click += btnOK_Click;
      // 
      // btnCancel
      // 
      btnCancel.DialogResult = DialogResult.Cancel;
      btnCancel.Location = new Point(337, 623);
      btnCancel.Margin = new Padding(1);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new Size(77, 21);
      btnCancel.TabIndex = 2;
      btnCancel.Text = "Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      btnCancel.Click += btnCancel_Click;
      // 
      // btnReset
      // 
      btnReset.DialogResult = DialogResult.Cancel;
      btnReset.Location = new Point(416, 623);
      btnReset.Margin = new Padding(1);
      btnReset.Name = "btnReset";
      btnReset.Size = new Size(131, 21);
      btnReset.TabIndex = 3;
      btnReset.Text = "Reset to Defaults";
      btnReset.UseVisualStyleBackColor = true;
      btnReset.Click += btnReset_Click;
      // 
      // TuningDialog
      // 
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(827, 654);
      Controls.Add(btnReset);
      Controls.Add(btnCancel);
      Controls.Add(btnOK);
      Controls.Add(propertyGrid1);
      Margin = new Padding(1);
      Name = "TuningDialog";
      Text = "Tuning Options";
      Load += TuningDialog_Load;
      ResumeLayout(false);
    }

    #endregion

    private PropertyGrid propertyGrid1;
    private Button btnOK;
    private Button btnCancel;
    private Button btnReset;
  }
}