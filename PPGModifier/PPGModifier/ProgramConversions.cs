using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToddUtils.FileParser;
using ToddUtils.LHT;
using ToddUtils.SiemensCCIMotionVariables;
using static System.Net.Mime.MediaTypeNames;


namespace ToddUtils
{
  public class ProgramConversions
  {
    internal static void updateProgram(string filename, string outputFilename, ProgramTuningOptions options, ProgressBar progressBar)
    {
      string numberFormat = "F6";

      long count = 0;
      ToddUtils.FileParser.cFileParse fp = new ToddUtils.FileParser.cFileParse();

      List<string> output = File.ReadAllLines(filename).ToList();

      #region helper functions
      List<string> ApplyFeedrate(List<string> output, ProgramTuningOptions options)
      {

        if (!options.UseOverrideFeedRates)
        {
          return output;
        }
        FileParser.cFileParse fp = new cFileParse();
        List<string> result = new List<string>();
        bool firstLayer = true;
        int n = 0;
        foreach (string line in output)
        {
          string thisline = line;
          if(firstLayer && thisline.Contains("LAYER2"))
          {
            firstLayer = false;
          }
          switch (n)
          {
            case 0: //offpart
              if (line.Contains("F="))
              {
                fp.ReplaceArgument(line, "F=", options.TransitFeedRate * 60, out string newline);
                thisline = $"{newline} ; ({options.TransitFeedRate} mm/s)";
              }
              if (line.Contains("WHEN TRUE DO LAYER="))
                n++;
              break;
            case 1: //onpart
              if (line.Contains("F="))
              {
                double onCourseFeedRate = options.OnCourseFeedRate;
                if (firstLayer)
                {
                  onCourseFeedRate = options.OnCourseFeedRateFirstLayer;
                }

                fp.ReplaceArgument(line, "F=", onCourseFeedRate * 60, out string newline);
                thisline = $"{newline} ; ({onCourseFeedRate} mm/s)";
              }

              if (line.Contains("WHEN TRUE DO LAYER="))
              {
                n = 0;
              }

              break;
          }
          result.Add(thisline);
        }

        return result;
      }

      List<string> ApplyProcessItems(List<string> output, ProgramTuningOptions options)
      {
        FileParser.cFileParse fp = new cFileParse();
        List<string> result = new List<string>();

        if (!options.UseProcessItems)
        {
          return output;
        }


        foreach (string line in output)
        {
          string thisline = line;

          if (line.Contains("WHEN TRUE DO CFORCE="))
          {
            fp.ReplaceArgument(line, "CFORCE=", options.CourseCompactionForce, out string newforceline);
            thisline = newforceline;
          }
          if (line.Contains("WHEN TRUE DO TCFORCE="))
          {
            fp.ReplaceArgument(line, "TCFORCE=", options.TackCompactionForce, out string newforceline);
            thisline = newforceline;
          }
          if (line.Contains("NOZZLE_TEMP_SET"))
          {
            fp.GetArgument(thisline, "N", out double Nnum, false);
            thisline = $"N{(int)Nnum} NOZZLE_TEMP_SET({options.nozzleTemp:F3})";
          }


          result.Add(thisline);
        }

        return result;
      }

      List<string> ApplyBlockSpacingRules(List<string> output, ProgramTuningOptions options)
      {

        if (!options.BlockSpacingApply)
        {
          return output;
        }

        FileParser.cFileParse fp = new cFileParse();
        List<string> result = new List<string>();

        int state = 0;
        bool first = true;
        cMotionArguments lastMotionArgs = new cMotionArguments();
        for (int ii = 0; ii < output.Count; ii++)
        {
          string line = output[ii];
          cMotionArguments motionArguments = new cMotionArguments();
          if ( line.Contains("G1") || line.Contains("G9"))
          {
            motionArguments = cMotionArguments.getMotionArguments(line);
            if(first)
            {
              lastMotionArgs = motionArguments;
            }
            first = false;
          }

          switch (state)
          {
            case 0: //not on part
              if( line.Contains("FEED"))
              {
                state++;
              }
              result.Add(line);
              break;

            case 1: //paying out tow on part
              {

                if ((line.Contains("G1") || line.Contains("G9")) && line.Contains("DIST="))
                {
                  double dx = motionArguments.X - lastMotionArgs.X;
                  double dy = motionArguments.Y - lastMotionArgs.Y;
                  double dz = motionArguments.Z - lastMotionArgs.Z;
                  double du = motionArguments.ROTX - lastMotionArgs.ROTX;
                  double dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);

                  if ((line.Contains("G9") && options.StopOnCut) || (dist >= options.MinSpacing || du >= options.MinAngleChange))
                  {
                    result.Add(line);
                    if( line.Contains("G9") && options.StopOnCut)
                    {
                      if (options.UseCycle832 && !options.DoGoska)
                      {
                        result.Add("CYCLE832(5,_ROUGH,1)");
                      }
                      result.Add($"F={options.ExitFeedRate*60:F0} ; ({options.ExitFeedRate}mm/s)");
                      //result.Add("SOFT");
                    }
                    lastMotionArgs = motionArguments;
                  }
                }
                else
                {
                  result.Add(line);
                }

                if ((line.Contains("G1") || line.Contains("G9")) && !line.Contains("DIST="))
                {
                  state = 0;
                }
                break;
              }
          }
        }

        return result;
      }

      List<string> ApplyInterpolationMode(List<string> output, ProgramTuningOptions options)
      {
        FileParser.cFileParse fp = new cFileParse();
        List<string> result = new List<string>();

        int n = 0;
        foreach (string line in output)
        {
          string thisline = line;
          
          if( thisline.Contains("G603"))
          {
            if (!options.DoGoska)
            {
              if (options.UseCycle832)
              {
                result.Add("CYCLE832(5,_ROUGH,1)");
              }
              result.Add("SOFT");
            }
          }
          else if (thisline.Contains("FEED")) //BRISK is moved to CCI_INIT
          {
            result.Add(thisline);
            if (!options.DoGoska)
            {
              if (options.UseCycle832)
              {
                result.Add("CYCLE832(0,_OFF,1)");
              }
              result.Add("BRISK");
            }
            else
            {
              result.Add("CYCLE832(0.2,_ORI_ROUGH,.5)");
            }
          }
          //else if (thisline.Contains("G9"))
          //{

          //}
          else
          {
            result.Add($"{thisline}");
          }

        }

        return result;
      }

      List<string> SetOffPartTime(List<string> output, ProgramTuningOptions options)
      {
        List<string> result = output;

        if (!options.ManageOffpartTime)
        {
          return result;
        }

        ToddUtils.FileParser.cFileParse fp = new ToddUtils.FileParser.cFileParse();
        int fCodeLineNumber = -1;
        int state = 0;
        cMotionArguments endOfCourseArguments = new cMotionArguments();
        cMotionArguments startOfNextCourseArguments = new cMotionArguments();
        for (int ii = 0; ii < output.Count; ii++)
        {
          string line = output[ii];
          switch (state)
          {
            case 0:
              {
                if (line.Contains("FEED")) //looking for start of course
                {
                  state++;
                }
                break;
              }
            case 1:
              {
                if( ((line.Contains("G1") || line.Contains("G9")) && !line.Contains("DIST")) ) //looking for end of course
                {
                  //record the end of the course now
                  endOfCourseArguments = cMotionArguments.getMotionArguments(line);
                  state++;
                }
                break;
              }
            case 2:
              {
                if( line.Contains("F=")) //look for the offpart feedrate
                {
                  fCodeLineNumber = ii; //record it's line number
                  state++;
                }
                break;
              }
            case 3:
              {
                if(line.Contains("G1") || line.Contains("G9"))
                {
                  startOfNextCourseArguments = cMotionArguments.getMotionArguments(line);
                }
                if(line.Contains("FEED")) //look for start of next course
                {
                  
                  //calculate offpart distance
                  double dx = startOfNextCourseArguments.X - endOfCourseArguments.X;
                  double dy = startOfNextCourseArguments.Y - endOfCourseArguments.Y;
                  double dz = startOfNextCourseArguments.Z - endOfCourseArguments.Z;
                  double offPartDist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                  //scale offpart speed
                  double offPartSpeedMax = offPartDist / 0.2; //mm/s.  Denominator is time in seconds

                  //overwrite offpart speed if necessary
                  if( options.TransitFeedRate > offPartSpeedMax)
                  {
                    //fp.ReplaceArgument(line, "F=", offPartSpeedMax * 60, out string newline);
                    string newline = $"F={offPartSpeedMax * 60:F0} ; ({offPartSpeedMax:F0} mm/s) ";
                    result[fCodeLineNumber] = newline;
                  }
                  state = 1; //you've satisfied state 0, jump into state 1;
                }
                break;
              }
          }
        }
        return result;
      }

      List<string> ApplyUVParameters(List<string> output, ProgramTuningOptions options)
      {
        FileParser.cFileParse fp = new cFileParse();
        List<string> result = new List<string>();

        string CalculateUVParameters(string UVCommandLine, double slope, double intercept)
        {
          double uv0 = 0;
          double uv1 = intercept;
          double uv100 = 100.0 * slope + intercept;
          double uv500 = 500.0 * slope + intercept;

          return $"{UVCommandLine}(0,{uv0:F0},1,{uv1:F0},100,{uv100:F0},500,{uv500:F0})";
        }

        if (!options.OverrideUVParameters)
        {
          return output;
        }

        /*
            N22 UV_MAP_LEADING(0,0,50,4726,100,9341,500,46720)
            N23 UV_MAP_TRAILING(0,0,50,8562,100,17428,500,88180)
            N24 TACK_UV_MAP_LEADING(0,0,1,5000,5,5001,42,30012)
            N25 TACK_UV_MAP_TRAILING(0,0,1,5000,5,5001,42,30012)
        */

        for (int ii = 0; ii < output.Count; ii++)
        {
          string line = output[ii];
          string newline = line;
          if( line.Contains(" UV_MAP_LEADING("))
          {
            newline = CalculateUVParameters("UV_MAP_LEADING", options.UVCourseSlopeLeading, options.UVCourseOffsetLeading);
          }
          else if( line.Contains(" UV_MAP_TRAILING("))
          {
            newline = CalculateUVParameters("UV_MAP_TRAILING", options.UVCourseSlopeTrailing, options.UVCourseOffsetTrailing);
          }
          else if (line.Contains(" TACK_UV_MAP_LEADING("))
          {
            newline = CalculateUVParameters("TACK_UV_MAP_LEADING", options.UVTackSlope, (double)options.UVTackOffset);
          }
          else if (line.Contains(" TACK_UV_MAP_TRAILING("))
          {
            newline = CalculateUVParameters("TACK_UV_MAP_TRAILING", options.UVTackSlope, (double)options.UVTackOffset);
          }
          else if (line.Contains("UVMULT="))
          {
            fp.GetArgument(newline, "N", out double Nnum, false);
            newline = $"N{(int)Nnum} UVMULT={options.UVMult:F3}";
          }
          result.Add(newline);
        }

        return result;
      }

      List<string> InsertM61(List<string> output, ProgramTuningOptions options)
      {
        if(!options.InsertM61)
        {
          return output;
        }
        FileParser.cFileParse fp = new cFileParse();
        List<string> result = new List<string>();

        cMotionArguments args = new cMotionArguments();
        bool DistLastScan = false;

        for (int ii = 0; ii < output.Count; ii++)
        {
          string line = output[ii];
          if (line.Contains("G1") || line.Contains("G9"))
          {
            if( line.Contains("DIST="))
            {
              if(!DistLastScan)
              {
                result.Add("M61 ; (Wait for previous cycle to complete)");
              }
              DistLastScan = true;
            }
            else
            {
              DistLastScan = false;
            }
          }
          result.Add(line);
        }
        return result;
      }

      List<string> RenumberPartProgram(List<string> output)
      {
        int nNum = 1;
        List<string> outputRenumbered = new List<string>();
        for (int i = 0; i < output.Count; i++)
        {
          string line = output[i];
          string trimmed = line.TrimStart();

          if (i == 59)
          {
            Console.WriteLine(line);
          }

          if (trimmed.StartsWith("N"))
          {
            //Console.WriteLine(line);
            // split into "Nxxx" and the rest
            int spaceIndex = trimmed.IndexOf(' ');
            if (spaceIndex > 0)
            {
              string rest = trimmed.Substring(spaceIndex + 1);
              outputRenumbered.Add($"N{nNum} {rest}");
            }
            else
            {
              outputRenumbered.Add($"N{nNum}");
            }
            nNum++;
          }
          else
          {
            // skip numbering truly blank lines
            if (string.IsNullOrWhiteSpace(line))
            {
              outputRenumbered.Add(line);
            }
            else
            {
              outputRenumbered.Add($"N{nNum} {line}");
              nNum++;
            }
          }
        }
        return outputRenumbered;
      }

      List<string> CollectStats(List<string> output)
      {
        List<string> result = new List<string>();
        ToddUtils.FileParser.cFileParse fp = new cFileParse();

        int activeLayer = 0;
        int activePath = 0;
        int totalPaths = 0;

        foreach (string line in output)
        {
          {
            if (line.Contains("LAYER="))
            {
              fp.GetArgument(line, "LAYER=", out double _layerNum, false);

              if (activeLayer != (int)_layerNum)
              {

                if ((int)_layerNum > 1)
                {
                  result.Add($"Layer: {activeLayer} has {activePath} Paths");
                  totalPaths += activePath;
                }
              }
              activeLayer = (int)_layerNum;
              fp.GetArgument(line, "PATH=", out double _path, false);
              activePath = (int)_path;
            }
          }
        }
        result.Add($"Layer: {activeLayer} has {activePath} Paths");
        result.Add($"There are {activeLayer} Layers and {totalPaths+=activePath} Paths");
        return result;
      }
      #endregion

      progressBar.Value = 0;
      output = ApplyFeedrate(output, options); progressBar.Value += 10;
      output = ApplyProcessItems(output, options); progressBar.Value += 10;
      output = ApplyBlockSpacingRules(output, options); progressBar.Value += 10;
      output = ApplyInterpolationMode(output, options); progressBar.Value += 10;
      output = ApplyUVParameters(output, options); progressBar.Value += 10;
      output = SetOffPartTime(output, options); progressBar.Value += 10;
      output = InsertM61(output, options); progressBar.Value += 10;
      output = RenumberPartProgram(output); progressBar.Value += 10;
      List<string> summary = CollectStats(output); progressBar.Value += 10;

      //output the results:
      string text = string.Join(Environment.NewLine, output);
      string _summary = string.Join(Environment.NewLine, summary);
      if (!string.IsNullOrEmpty(outputFilename))
      {

        File.WriteAllText(outputFilename, text);
        Console.WriteLine($"Output written to {outputFilename}");
        string statsFileName = outputFilename.Replace("_bs.mpf", "_stats.txt");
        File.WriteAllText(statsFileName, _summary);

        
      }
      else
      {
        Clipboard.SetText(text);
        Console.WriteLine("Output copied to clipboard.");
      }

      Console.WriteLine("File Summary");
      foreach (string line in summary)
      {
        Console.WriteLine(line);
      }
      progressBar.Value = progressBar.Maximum;
    }
    
  }
}