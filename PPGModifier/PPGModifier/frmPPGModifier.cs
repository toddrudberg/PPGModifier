using System.Runtime.InteropServices;
using ToddUtils;
using Pastel;

namespace PPGModifier
{
  public partial class frmPPGModifier : Form
  {

    [DllImport("kernel32.dll")] static extern bool AllocConsole();
    [DllImport("kernel32.dll")] static extern bool FreeConsole();
    [DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
    [DllImport("user32.dll")] static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nW, int nH, bool repaint);
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    const int SW_SHOW = 5;

    string ConfigPath => Path.Combine(
  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
  "YourAppName",
  "program-tuning.json");

    ProgramTuningOptions _opts;


    public frmPPGModifier()
    {
      InitializeComponent();

      // Put form in upper-left half of primary screen
      // var wa = Screen.PrimaryScreen.WorkingArea;
      var wa = Screen.FromPoint(Cursor.Position).WorkingArea;
      StartPosition = FormStartPosition.Manual;
      Left = wa.Left;
      Top = wa.Top;
      Width = wa.Width / 2;
      Height = wa.Height;

      EnsureConsole();
      PositionConsoleRightHalf();
    }

    private void EnsureConsole()
    {
      if (GetConsoleWindow() == IntPtr.Zero)
      {
        AllocConsole();
        Console.WriteLine("Console logging started...".Pastel(ConsoleColor.Green));
      }
      else
      {
        // If already attached, just make sure it's visible
        ShowWindow(GetConsoleWindow(), SW_SHOW);
      }
    }

    private void PositionConsoleRightHalf()
    {
      var cw = GetConsoleWindow();
      if (cw == IntPtr.Zero) return;

      //var wa = Screen.PrimaryScreen.WorkingArea;
      var wa = Screen.FromPoint(Cursor.Position).WorkingArea;
      int x = wa.Left + wa.Width / 2;
      int y = wa.Top;
      int w = wa.Width / 2;
      int h = wa.Height;

      MoveWindow(cw, x, y, w, h, true);
    }

    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);
      // Re-apply in case DPI/layout changed during startup
      PositionConsoleRightHalf();
    }



    private void frmPPGModifier_Load(object sender, EventArgs e)
    {
      Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
      _opts = ProgramTuningOptions.Load(ConfigPath);
    }

    protected override void OnResize(EventArgs e)
    {
      base.OnResize(e);

      btnDoTheThingv1_12.Left = (this.ClientSize.Width - btnDoTheThingv1_12.Width) / 2;
      btnUnwidePPG.Left = (this.ClientSize.Width - btnDoTheThingv1_12.Width) / 2;
      btnEvenBlockSpacing.Left = (this.ClientSize.Width - btnEvenBlockSpacing.Width) / 2;
      progressBar1.Left = (this.ClientSize.Width - progressBar1.Width) / 2;
      progressBar1.Visible = false;
      void AdjustLabels(Label label)
      {
        label.Text = "";
        label.BackColor = Color.Transparent;
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.Left = (this.ClientSize.Width - label.Width) / 2;
      }
      AdjustLabels(label1);
      AdjustLabels(linkLabel1);
    }

    private void linkLabel1_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        string file = linkLabel1.Links[0].LinkData as string;

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
          FileName = file,
          UseShellExecute = true
        });
      }
      if (e.Button == MouseButtons.Right)
      {
        string file = linkLabel1.Links[0].LinkData as string;

        System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + file + "\"");
      }
    }

    private void btnDoTheThingv1_12_Click(object sender, EventArgs e)
    {
      this.Enabled = false;

      string configToUse = ConfigPath;

      var result = MessageBox.Show(
          "Open last tuning file?\n\nYes = Open last\nNo = Choose file\nCancel = Abort",
          "Open Tuning Options",
          MessageBoxButtons.YesNoCancel,
          MessageBoxIcon.Question);

      if (result == DialogResult.Cancel)
      {
        this.Enabled = true;
        return;
      }

      if (result == DialogResult.No)
      {
        using var ofdConfig = new OpenFileDialog();
        ofdConfig.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
        ofdConfig.Title = "Select Tuning Options File";

        if (ofdConfig.ShowDialog(this) != DialogResult.OK)
        {
          this.Enabled = true;
          return;
        }

        configToUse = ofdConfig.FileName;
      }

      // Load selected config
      _opts = ProgramTuningOptions.Load(configToUse);

      using var dlg = new TuningDialog(_opts);
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        _opts = dlg.Options;
        ProgramTuningOptions.SaveAs(ConfigPath, _opts);
      }
      else
      {
        this.Enabled = true;
        return;
      }

      using var ofd = new OpenFileDialog();
      ofd.Filter = "GCode files (*.mpf)|*.mpf|All files (*.*)|*.*";
      ofd.Title = "Select GCode File";

      if (ofd.ShowDialog(this) != DialogResult.OK)
      {
        MessageBox.Show("No file selected.");
        this.Enabled = true;
        return;
      }

      string directory = Path.GetDirectoryName(ofd.FileName);
      string filenameWithoutExt = Path.GetFileNameWithoutExtension(ofd.FileName);
      string outputFileName = Path.Combine(directory, filenameWithoutExt + "_bs.mpf");
      progressBar1.Visible = true;
      ProgramConversions.updateProgram(ofd.FileName, outputFileName, _opts, this.progressBar1);
      string outputFileTunringUsedName = outputFileName.Replace("_bs.mpf", "_opts.json");
      ProgramTuningOptions.SaveAs(outputFileTunringUsedName, _opts);
      label1.Text = $"File output to:";
      linkLabel1.Text = outputFileName;
      linkLabel1.Links.Clear();
      linkLabel1.Links.Add(0, linkLabel1.Text.Length, outputFileName);

      this.Enabled = true;
    }

    private void btnUnwidePPG_Click(object sender, EventArgs e)
    {
      using var ofd = new OpenFileDialog();
      ofd.Filter = "GCode files (*.mpf)|*.mpf|All files (*.*)|*.*";
      ofd.Title = "Select GCode File";

      if (ofd.ShowDialog(this) != DialogResult.OK)
      {
        MessageBox.Show("No file selected.");
        this.Enabled = true;
        return;
      }

      string directory = Path.GetDirectoryName(ofd.FileName);
      string filenameWithoutExt = Path.GetFileNameWithoutExtension(ofd.FileName);
      string outputFileName = Path.Combine(directory, filenameWithoutExt + "_bs.mpf");
      progressBar1.Visible = true;
      ProgramConversions.unwindPPG(ofd.FileName, outputFileName, _opts, this.progressBar1);
    }

    private void btnEvenBlockSpacing_Click(object sender, EventArgs e)
    {
      using var ofd = new OpenFileDialog();
      ofd.Filter = "GCode files (*.mpf)|*.mpf|All files (*.*)|*.*";
      ofd.Title = "Select GCode File";

      if (ofd.ShowDialog(this) != DialogResult.OK)
      {
        MessageBox.Show("No file selected.");
        this.Enabled = true;
        return;
      }

      string directory = Path.GetDirectoryName(ofd.FileName);
      string filenameWithoutExt = Path.GetFileNameWithoutExtension(ofd.FileName);
      string outputFileName = Path.Combine(directory, filenameWithoutExt + "_space.mpf");
      progressBar1.Visible = true;
      List<string> result = ProgramConversions.evenOutBlockSpacing(ofd.FileName, _opts, this.progressBar1);

      //output the file

      File.WriteAllLines(outputFileName, result);
      //string outputFIleName2 = outputFileName.Replace("_space", "_s2");
      //File.WriteAllLines(outputFIleName2, result2);
      label1.Text = $"File output to:";
      linkLabel1.Text = outputFileName;
      linkLabel1.Links.Clear();
      linkLabel1.Links.Add(0, linkLabel1.Text.Length, outputFileName);
    }
  }
}
