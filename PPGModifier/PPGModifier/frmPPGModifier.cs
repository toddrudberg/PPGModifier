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

    private void btnDoIt_Click(object sender, EventArgs e)
    {
      this.Enabled = false;

      using var dlg = new TuningDialog(_opts);
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        _opts = dlg.Options;
        _opts.Save(ConfigPath);
      }
      else
      {
        this.Enabled = true;
        return; // cancelled
      }

      OpenFileDialog ofd = new OpenFileDialog();
      ofd.Filter = "GCode files (*.mpf)|*.mpf|All files (*.*)|*.*";
      ofd.Title = "Select GCode File";
      if (ofd.ShowDialog() != DialogResult.OK)
      {
        MessageBox.Show("No file selected.");
        this.Enabled = true;
        return;
      }
      // Write the output lines to a new file
      string directory = Path.GetDirectoryName(ofd.FileName);
      string filenameWithoutExt = Path.GetFileNameWithoutExtension(ofd.FileName);
      string outputFileName = Path.Combine(directory, filenameWithoutExt + "_bs.mpf");
      ProgramConversions.adjustBlockSpacing(ofd.FileName, outputFileName, _opts);
      this.Enabled = true;
    }

    private void frmPPGModifier_Load(object sender, EventArgs e)
    {
      Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
      _opts = ProgramTuningOptions.Load(ConfigPath);
    }
  }
}
