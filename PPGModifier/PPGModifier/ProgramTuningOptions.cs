using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class ProgramTuningOptions
{
  [Category("Formatting"), DisplayName("Number Format"), Description("Standard numeric format string, e.g. F6, G5, 0.000")]
  [DefaultValue("F6")]
  public string NumberFormat { get; set; } = "F6";

  #region Block Spacing

  [Category("Block Spacing"), DisplayName("Apply Block Spacing"), Description("Do you want to adjust block spacing?")]
  [DefaultValue(false)]
  public bool BlockSpacingApply { get; set; } = false;

  [Category("Block Spacing"), DisplayName("Min Spacing (mm)"), Description("Minimum distance to print a line")]
  [DefaultValue(10.0)]
  public double MinSpacing { get; set; } = 10.0;

  [Category("Block Spacing"), DisplayName("Min Angle Change (deg)"), Description("Minimum angle change to print a line")]
  [DefaultValue(10.0)]
  public double MinAngleChange { get; set; } = 10.0;
  #endregion

  #region Feed Rates
  [Category("Feed Rates"), DisplayName("1 - Use Override Feed Rates")]
  [DefaultValue(true)]
  public bool UseOverrideFeedRates { get; set; } = false;

  [Category("Feed Rates"), DisplayName("3 - On Course FR (mm/s)"), Description("General on course speed (500 mm/s max).")]
  [DefaultValue(500)]
  public double OnCourseFeedRate { get; set; } = 400;

  [Category("Feed Rates"), DisplayName("2 - On Course FR First Layer (mm/s)"), Description("First Layer on course speed (500 mm/s max).")]
  [DefaultValue(200)]
  public double OnCourseFeedRateFirstLayer { get; set; } = 200;

  [Category("Feed Rates"), DisplayName("4 - Exit Feedrate (mm/s)"), Description("Exit FeedRate (500 mm/s max, 50 default).  From the end of cut to end of part.  Especially effective during stop on cut.  A slower speed straightens the tails and allows more UV cure which holds the ends down and makes subsquent layers easier to print.")]
  [DefaultValue(50)]
  public double ExitFeedRate { get; set; } = 50;

  [Category("Feed Rates"), DisplayName("5 - Transit Feedrate (mm/s)"), Description("Rapid traverse speed (1500 mm/s max).")]
  [DefaultValue(1500)]
  public double TransitFeedRate { get; set; } = 1500;

  #endregion

  #region Process Items
  [Category("Process Items"), DisplayName("1 - Apply Process Item Overrides")]
  [DefaultValue(true)]
  public bool UseProcessItems { get; set; } = true;

  [Category("Process Items"), DisplayName("2 - Tack Compaction Force (N)"), Description("4-6N generally")]
  [DefaultValue(4.0)]
  public double TackCompactionForce { get; set; } = 4.0;

  [Category("Process Items"), DisplayName("2 - Course Compaction Force (N)"), Description("2-4N generally")]
  [DefaultValue(2.5)]
  public double CourseCompactionForce { get; set; } = 2.5;

  [Category("Process Items"), DisplayName("3 - Nozzle Temp (deg C)"), Description("60 for Ceremat, 70 for Aero.")]
  [DefaultValue(70)]
  public double nozzleTemp { get; set; } = 70.0;
  #endregion

  #region Interpolation Items

  [Category("Interpolation Control"), DisplayName("Hard Interpolation (flat part)"), Description("Better for flat parts.  You get more honest motion and better coordination with the FWM.  Quicker response for all regimes of motion and especially offpart.")]
  [DefaultValue(false)]
  public bool HardInterpolation { get; set; } = true;

  [Category("Interpolation Control"), DisplayName("Super Soft Interpolation (rotator)"), Description("Very soft interpolation.  Generally good for rotator.  Adds 2-3s per course of calculation overhead.")]
  [DefaultValue(false)]
  public bool SoftInterpolation { get; set; } = false;
  #endregion

  #region Logic
  [Category("Logic"), DisplayName("Stop on Cut"), Description("Required for all non-cut-on-the-fly machines.  Currently only Mercury will get this upgrade.")]
  [DefaultValue(false)]
  public bool StopOnCut { get; set; } = false;

  [Category("Logic"), DisplayName("Insert M61"), Description("Soon this will be required on all machins.  As of this writing on only 1506 needs M61.")]
  [DefaultValue(false)]
  public bool InsertM61 { get; set; } = false;

  [Category("Logic"), DisplayName("ManageOffPartTime"), Description("Required on all machines except 1506.  M61 handles this in the future.  Before M61, I was required to manage the time from the end of a course to the start of a next.  If the time falls below a threashold, I calculate the maximum speed required to ensure this threashold is met and overwrite the programmed off-part feedrate.")]
  [DefaultValue(true)]
  public bool ManageOffpartTime { get; set; } = false;
  #endregion

  #region UV Control
  [Category("UV Control"), DisplayName("1 - Override UV Parameters")]
  [DefaultValue(true)]
  public bool OverrideUVParameters { get; set; } = true;

  [Category("UV Control"), DisplayName("2 - UVMULT"), Description("Scaler for the UV process")]
  [DefaultValue(1.0)]
  public double UVMult { get; set; } = 1.0;

  [Category("UV Control"), DisplayName("3 - Tack offset")]
  [Description("Default: 5000")]
  [DefaultValue(5000.0)]
  public double UVTackOffset { get; set; } = 5000d;

  [Category("UV Control"), DisplayName("4 - Tack slope")]
  [DefaultValue(675.0)]
  [Description("Default: 675")]
  public double UVTackSlope { get; set; } = 675d;

  [Category("UV Control"), DisplayName("5 - Course offset leading")]
  [DefaultValue(1300.0)]
  [Description("Default: 1300")]
  public double UVCourseOffsetLeading { get; set; } = 1300d;

  [Category("UV Control"), DisplayName("6 - Course slope leading")]
  [DefaultValue(100.0)]
  [Description("Default: 100")]
  public double UVCourseSlopeLeading { get; set; } = 100d;

  [Category("UV Control"), DisplayName("7 - Course offset trailing")]
  [DefaultValue(1300.0)]
  [Description("Default: 1300")]
  public double UVCourseOffsetTrailing { get; set; } = 1300d;

  [Category("UV Control"), DisplayName("8 - Course slope trailing")]
  [DefaultValue(175.0)]
  [Description("Default: 175")]
  public double UVCourseSlopeTrailing { get; set; } = 175d;
  #endregion

  // JSON persistence
  public static ProgramTuningOptions Load(string path)
  {
    try
    {
      if (File.Exists(path))
        return JsonSerializer.Deserialize<ProgramTuningOptions>(File.ReadAllText(path)) ?? new ProgramTuningOptions();
    }
    catch { /* fall back to defaults */ }
    return new ProgramTuningOptions();
  }

  public static void SaveAs(string path, ProgramTuningOptions opts)
  {
    var json = JsonSerializer.Serialize(opts, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(path, json);
  }
}
