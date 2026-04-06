using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class ProgramTuningOptions
{
  [Category("Formatting"), DisplayName("Number Format"), Description("Standard numeric format string, e.g. F6, G5, 0.000")]
  [DefaultValue("F6")]
  public string NumberFormat { get; set; } = "F6";

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

  [Category("Process Items"), DisplayName("4 - tack distance [mm]"), Description("How long do you want to apply tack UV and tack compaction (10mm-50mm)?")]
  [DefaultValue(30)]
  public double tackSettingsDistance{ get; set; } = 30.0;
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

  [Category("Logic"), DisplayName("Insert M61/M64?"), Description("Soon this will be required on all machines - check ppg to see if it already has them.")]
  [DefaultValue(false)]
  public bool InsertM61 { get; set; } = false;

  #endregion

  #region UV Control
  [Category("UV Control"), DisplayName("1 - Override UV Parameters")]
  [Description("You need to override the table to extend speeds to 125mm/s - just do it.")]
  [DefaultValue(true)]
  public bool OverrideUVParameters { get; set; } = true;

  [Category("UV Control"), DisplayName("2 - UVMULT")] 
  [Description("AERO use 0.10, CEREMAT and others 1.0")]
  [DefaultValue(1.0)]
  public double UVMult { get; set; } = 1.0;

  [Category("UV Control"), DisplayName("3 - Tack offset")]
  [Description("Default: 5000")]
  [DefaultValue(5000.0)]
  public double UVTackOffset { get; set; } = 5000d;

  [Category("UV Control"), DisplayName("4 - Tack slope")]
  [Description("Default: 675")]
  [DefaultValue(675.0)]
  public double UVTackSlope { get; set; } = 675d;

  [Category("UV Control"), DisplayName("5 - Course offset leading")]
  [Description("Default: 1300")]
  [DefaultValue(1300.0)]
  public double UVCourseOffsetLeading { get; set; } = 1300d;

  [Category("UV Control"), DisplayName("6 - Course slope leading")]
  [Description("Default: 100")]
  [DefaultValue(100.0)]
  public double UVCourseSlopeLeading { get; set; } = 100d;

  [Category("UV Control"), DisplayName("7 - Course offset trailing")]
  [Description("Default: 1300")]
  [DefaultValue(1300.0)]
  public double UVCourseOffsetTrailing { get; set; } = 1300d;

  [Category("UV Control"), DisplayName("8 - Course slope trailing")]
  [Description("Default: 175")]
  [DefaultValue(175.0)]
  public double UVCourseSlopeTrailing { get; set; } = 175d;
  #endregion

  #region UVLaserSettings

  const string categoryUVLaserSettings = "UV Laser Control";

  [Category(categoryUVLaserSettings), DisplayName("1 - Use UV Laser?")]
  [Description("Does this machine have a UV Laser?")]
  [DefaultValue(false)]
  public bool UseUVLaser { get; set; } = false;

  [Category(categoryUVLaserSettings), DisplayName("2 - UV Tack Dose mJ/cm²")]
  [Description("Default: 100.0")]
  [DefaultValue(12.5)]
  public double UVTackDose { get; set; } = 12.5d;

  [Category(categoryUVLaserSettings), DisplayName("3 - UV Course Dose mJ/cm²")]
  [DefaultValue(67.5)]
  [Description("Default: 67.5")]
  public double UVCouseDose { get; set; } = 67.5d;
  #endregion

  #region Acceleration Settings
  const string categoryAccelerationSettings = "Acceleration Settings";
  [Category(categoryAccelerationSettings), DisplayName("1 - Override Acceleration Settings?")]
  [Description("Do you want to overide the stock Acceleration Settings?")]
  [DefaultValue(false)]
  public bool UseAccelerationSettings { get; set; } = false;

  [Category(categoryAccelerationSettings), DisplayName("2 - Box Filter [s]")]
  [Description("Default: .25s - 1s.  Adds double box filter smoothing during jump from tack speed to course speed.")]
  [DefaultValue(.5)]
  public double AccelerationBoxFilter { get; set; } = 0.5d;

  [Category(categoryAccelerationSettings), DisplayName("3 - tack to course speed acceleration [g]")]
  [Description("Default: .1g - .5g  desired peak acceleration during ramp from tack speed to course speed.")]
  [DefaultValue(.25)]
  public double AccelerationCourseAcceleration { get; set; } = 0.5d;

  [Category(categoryAccelerationSettings), DisplayName("4 - feed feedrate [mm/s]")]
  [Description("Default: 10mm/s - 50mm/s initial feed speed (tack speed).")]
  [DefaultValue(15)]
  public double AccelerationTackSpeed { get; set; } = 15.0;

  [Category(categoryAccelerationSettings), DisplayName("5 - tack feed distance [mm]")]
  [Description("Default: 15mm - 30mm.  Distance at tack speed.")]
  [DefaultValue(15)]
  public double AccelerationFeedDistance { get; set; } = 15.0;

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
