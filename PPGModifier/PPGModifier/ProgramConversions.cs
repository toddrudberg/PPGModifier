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
        int step = 0;
        foreach (string line in output)
        {
          string thisline = line;
          if(firstLayer && thisline.Contains("LAYER2"))
          {
            firstLayer = false;
          }
          switch (step)
          {
            case 0: //offpart
              if (line.Contains("F="))
              {
                fp.ReplaceArgument(line, "F=", options.TransitFeedRate * 60, out string newline, "F0");
                thisline = $"{newline} ; ({options.TransitFeedRate} mm/s Transit Feedrate)";
              }
              if( line.Contains("APPROACH"))
              {
                step++;
              }
              break;
            case 1:
              if( line.Contains("F="))
              {
                //throw this line out
                thisline = $"; {thisline} ; throwing out this F command.";
              }
              if (line.Contains("FEED"))
              {
                //result.Add(line); //poop out the FEED command Feedrate must be before the FEED command!
                double onCourseFeedRate = options.OnCourseFeedRate;
                if (firstLayer)
                {
                  onCourseFeedRate = options.OnCourseFeedRateFirstLayer;
                }
                thisline = $"F={(onCourseFeedRate * 60):F0} ; ({onCourseFeedRate} mm/s On-course Feedrate for {(firstLayer == true ? "First Layer" : "2nd Layer and up")} - note must be prior to FEED)";
                result.Add(thisline);
                thisline = line;
                step++;
              }
              break;
            case 2: //onpart
              if (line.Contains("F="))
              { // just in case there are any other F commands along this course.
                double onCourseFeedRate = options.OnCourseFeedRate;
                if (firstLayer)
                {
                  onCourseFeedRate = options.OnCourseFeedRateFirstLayer;
                }

                fp.ReplaceArgument(line, "F=", onCourseFeedRate * 60, out string newline, "F0");
                thisline = $"{newline} ; ({onCourseFeedRate} mm/s On-course Feedrate)";
              }
              if( line.Contains("G9"))
              {
                result.Add(thisline); //poop out the G9 line
                thisline = $"F={(options.ExitFeedRate * 60):F0} ;  ({options.ExitFeedRate} mm/s Exit Feedrate)";   //add the exit feedrate           
              }


              if (line.Contains("WHEN TRUE DO LAYER="))
              {
                step = 0;
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

        if (options.HardInterpolation)
        {
          int step = 0;
          for (int i = 0; i < output.Count; i++)
          {
            string line = output[i];
            switch (step)
            {
              case 0: //find the begginning of the program
                if ((line.Contains("G1") || line.Contains("G9")) && line.Contains("DIST="))
                {
                  result.Add("SOFT ; (soften the initial approach move)"); // this softens the approach move
                  step++;
                }
                result.Add(line);
                break;
              case 1:
                result.Add(line);
                if ( line.Contains("FEED"))
                {
                  result.Add("BRISK ; (harden the path)");
                  result.Add("CYCLE832(0,_OFF,1) ; (ensure Cycle832 is dissabled)");
                  step++;
                }
                break;
              case 2:
                
                result.Add(line);

                if (!options.StopOnCut)
                  step++;
                else if( line.Contains("G9"))
                {
                  result.Add("SOFT ; (soften the cut accel and offpart move)");
                  step++;
                }
                
                break;

              case 3:
                if (line.Contains("M63"))
                {
                  result.Add("SOFT ; (soften offpart moves)");
                  step = 1;
                }
                result.Add(line);
                break;
            }            
          }
        }
        else if (options.SoftInterpolation)
        {
          foreach (string line in output)
          {
            string thisline = line;
            if (thisline.Contains("FEED")) //BRISK is moved to CCI_INIT
            {
              result.Add(thisline);
              result.Add("CYCLE832(0.2,_ORI_ROUGH,.5)");
            }
            else
            {
              result.Add($"{thisline}");
            }
          }
        }
        else
        {// do nothing
          return output;
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