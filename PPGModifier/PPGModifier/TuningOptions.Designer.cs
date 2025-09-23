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
      SuspendLayout();
      // 
      // propertyGrid1
      // 
      propertyGrid1.Location = new Point(12, 28);
      propertyGrid1.Name = "propertyGrid1";
      propertyGrid1.Size = new Size(1949, 1485);
      propertyGrid1.TabIndex = 0;
      // 
      // btnOK
      // 
      btnOK.DialogResult = DialogResult.OK;
      btnOK.Location = new Point(591, 1669);
      btnOK.Name = "btnOK";
      btnOK.Size = new Size(188, 58);
      btnOK.TabIndex = 1;
      btnOK.Text = "OK";
      btnOK.UseVisualStyleBackColor = true;
      
      // 
      // btnCancel
      // 
      btnCancel.DialogResult = DialogResult.Cancel;
      btnCancel.Location = new Point(840, 1669);
      btnCancel.Name = "btnCancel";
      btnCancel.Size = new Size(188, 58);
      btnCancel.TabIndex = 2;
      btnCancel.Text = "Cancel";
      btnCancel.UseVisualStyleBackColor = true;
      
      // 
      // TuningDialog
      // 
      AutoScaleDimensions = new SizeF(17F, 41F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(1973, 1739);
      Controls.Add(btnCancel);
      Controls.Add(btnOK);
      Controls.Add(propertyGrid1);
      Name = "TuningDialog";
      Text = "Tuning Options";
      Load += TuningDialog_Load;
      ResumeLayout(false);
    }

    #endregion

    private PropertyGrid propertyGrid1;
    private Button btnOK;
    private Button btnCancel;
  }
}