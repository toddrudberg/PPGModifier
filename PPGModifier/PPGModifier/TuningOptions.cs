using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPGModifier
{
  public partial class TuningDialog : Form
  {


    private readonly ProgramTuningOptions _original;
    public ProgramTuningOptions Options { get; private set; }

    public TuningDialog(ProgramTuningOptions options)
    {
      InitializeComponent();
      _original = options;
      Options = Clone(options);                 // work on a copy
      propertyGrid1.SelectedObject = Options;
      Text = "Program Tuning";
      AcceptButton = btnOK; CancelButton = btnCancel;
    }

    private void btnOK_Click(object sender, EventArgs e) => DialogResult = DialogResult.OK;
    private void btnCancel_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;

    private void btnReset_Click(object sender, EventArgs e)
    {
      Options = new ProgramTuningOptions();     // reset to defaults
      propertyGrid1.SelectedObject = Options;
    }

    private static ProgramTuningOptions Clone(ProgramTuningOptions src)
    {
      var json = System.Text.Json.JsonSerializer.Serialize(src);
      return System.Text.Json.JsonSerializer.Deserialize<ProgramTuningOptions>(json)!;
    }


    //existing
    //public TuningDialog()
    //{
    //  InitializeComponent();
    //}

    private void TuningDialog_Load(object sender, EventArgs e)
    {

    }
  }
}
