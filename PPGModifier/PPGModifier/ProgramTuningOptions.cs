using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class ProgramTuningOptions
{
  [Category("Formatting"), DisplayName("Number Format"), Description("Standard numeric format string, e.g. F6, G5, 0.000")]
  [DefaultValue("F6")]
  public string NumberFormat { get; set; } = "F6";

  [Category("Block Spacing"), DisplayName("Min Spacing (mm)"), Description("Minimum distance to print a line")]
  [DefaultValue(10.0)]
  public double MinSpacing { get; set; } = 10.0;

  [Category("Block Spacing"), DisplayName("Min Angle Change (deg)"), Description("Minimum angle change to print a line")]
  [DefaultValue(10.0)]
  public double MinAngleChange { get; set; } = 10.0;

  [Category("Logic"), DisplayName("Use Override Feed Rates")]
  [DefaultValue(true)]
  public bool UseOverrideFeedRates { get; set; } = false;

  [Category("Feed Rates"), DisplayName("On Course FR (mm/s)"), Description("How fast do you want to print?")]
  [DefaultValue(500)]
  public double OnCourseFeedRate { get; set; } = 400;

  [Category("Feed Rates"), DisplayName("On Course FR First Layer (mm/s)"), Description("Sort Course FeedRate?")]
  [DefaultValue(200)]
  public double OnCourseFeedRateFirstLayer { get; set; } = 10;

  [Category("Feed Rates"), DisplayName("Transit Feedrate (mm/s)"), Description("How fast do you rapid traverse?")]
  [DefaultValue(1500)]
  public double TransitFeedRate { get; set; } = 1500;

  [Category("Process Items"), DisplayName("Tack Compaction Force (N)"), Description("Just do something!")]
  [DefaultValue(4.0)]
  public double TackCompactionForce { get; set; } = 4.0;

  [Category("Process Items"), DisplayName("Course Compaction Force (N)"), Description("Just do something!")]
  [DefaultValue(2.5)]

  public double CourseCompactionForce { get; set; } = 2.5;

  [Category("Process Items"), DisplayName("Nozzle Temp (deg C)"), Description("Just do something!")]
  [DefaultValue(70)]

  public double nozzleTemp { get; set; } = 70.0;

  [Category("Logic"), DisplayName("Remove courseRetract")]
  [DefaultValue(false)]
  public bool Remove_courseRetract { get; set; } = false;

  [Category("Logic"), DisplayName("Stop on Cut")]
  [DefaultValue(true)]
  public bool StopOnCut { get; set; } = false;

  [Category("Logic"), DisplayName("Apply Process Item Overrides")]
  [DefaultValue(true)]
  public bool UseProcessItems { get; set; } = true;

  [Category("Logic"), DisplayName("Insert Cycle832 (rotator friendly)")]
  [DefaultValue(true)]
  public bool UseCycle832 { get; set; } = true;


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

  public void Save(string path)
  {
    var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(path, json);
  }

  public static void SaveAs(string path, ProgramTuningOptions opts)
  {
    var json = JsonSerializer.Serialize(opts, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(path, json);
  }
}
