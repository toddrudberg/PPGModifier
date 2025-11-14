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

  [Category("Feed Rates"), DisplayName("Use Override Feed Rates")]
  [DefaultValue(true)]
  public bool UseOverrideFeedRates { get; set; } = false;

  [Category("Feed Rates"), DisplayName("On Course Feedrate (mm/s)"), Description("How fast do you want to print?")]
  [DefaultValue(24000)]
  public double OnCourseFeedRate { get; set; } = 400;

  [Category("Feed Rates"), DisplayName("Transit Feedrate (mm/s)"), Description("How fast do you rapid traverse?")]
  [DefaultValue(30000)]
  public double TransitFeedRate { get; set; } = 1500;

  [Category("Logic"), DisplayName("Remove courseRetract")]
  [DefaultValue(false)]
  public bool Remove_courseRetract { get; set; } = false;

  [Category("Logic"), DisplayName("Stop on Cut")]
  [DefaultValue(true)]
  public bool StopOnCut { get; set; } = false;



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
}
