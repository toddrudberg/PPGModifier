using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
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

    //internal static List<string> evenOutBlockSpacingSafe(string fileName, ProgramTuningOptions opts, ProgressBar progressBar1)
    //{
    //  List<string> input = File.ReadAllLines(fileName).ToList();
    //  List<string> result = new List<string>(input.Count);

    //  bool onPart = false;
    //  int courseNum = 0;
    //  cMotionArguments? lastArgs = null;

    //  bool IsMotionLine(string line)
    //  {
    //    return line.Contains("G1") || line.Contains("G9");
    //  }

    //  bool HasDist(string line)
    //  {
    //    return line.Contains("DIST=");
    //  }

    //  bool IsSafeToInsert(cMotionArguments a, cMotionArguments b)
    //  {
    //    double dDist = b.DIST - a.DIST;
    //    double dRx = Math.Abs(b.RX - a.RX);
    //    double dRy = Math.Abs(b.RY - a.RY);
    //    double dRz = Math.Abs(b.RZ - a.RZ);
    //    double dRotx = Math.Abs(b.ROTX - a.ROTX);

    //    // Conservative rule:
    //    // only densify if the DIST gap is meaningfully large
    //    // and the attitude change is small enough that straight interpolation is believable.
    //    return dDist > 1.1 &&
    //           dRx < 2.0 &&
    //           dRy < 2.0 &&
    //           dRz < 2.0 &&
    //           dRotx < 2.0;
    //  }

    //  void InsertLines(cMotionArguments lastPoint, cMotionArguments thisPoint, List<string> target)
    //  {
    //    double dx = thisPoint.X - lastPoint.X;
    //    double dy = thisPoint.Y - lastPoint.Y;
    //    double dz = thisPoint.Z - lastPoint.Z;
    //    double dRotx = thisPoint.ROTX - lastPoint.ROTX;
    //    double dRx = thisPoint.RX - lastPoint.RX;
    //    double dRy = thisPoint.RY - lastPoint.RY;
    //    double dRz = thisPoint.RZ - lastPoint.RZ;
    //    double dDist = thisPoint.DIST - lastPoint.DIST;

    //    // Aim for roughly 1.0 DIST spacing without creating an extra point
    //    // when the gap is only slightly over 1.0.
    //    int numInserts = Math.Max(0, (int)Math.Floor(dDist) - 1);

    //    for (int i = 1; i <= numInserts; i++)
    //    {
    //      double ratio = (double)i / (numInserts + 1);

    //      cMotionArguments newArgs = new cMotionArguments
    //      {
    //        X = lastPoint.X + ratio * dx,
    //        Y = lastPoint.Y + ratio * dy,
    //        Z = lastPoint.Z + ratio * dz,
    //        ROTX = lastPoint.ROTX + ratio * dRotx,
    //        RX = lastPoint.RX + ratio * dRx,
    //        RY = lastPoint.RY + ratio * dRy,
    //        RZ = lastPoint.RZ + ratio * dRz,
    //        DIST = lastPoint.DIST + ratio * dDist
    //      };

    //      target.Add(
    //        $"G1 X={newArgs.X:F5} Y={newArgs.Y:F5} Z={newArgs.Z:F5} " +
    //        $"RX={newArgs.RX:F5} RY={newArgs.RY:F5} RZ={newArgs.RZ:F5} " +
    //        $"ROTX=DC({newArgs.ROTX:F5}) DIST={newArgs.DIST:F3} ; ");
    //    }
    //  }

    //  for (int i = 0; i < input.Count; i++)
    //  {
    //    progressBar1.Value = 100 * i / input.Count;
    //    string line = input[i];

    //    // Default behavior: preserve every line unless we intentionally insert before it.
    //    if (!IsMotionLine(line))
    //    {
    //      // Break interpolation continuity across non-motion lines.
    //      onPart = false;
    //      lastArgs = null;
    //      result.Add(line);
    //      continue;
    //    }

    //    if (!HasDist(line))
    //    {
    //      // Motion line, but not one of the on-part DIST blocks.
    //      // Treat as a hard boundary to avoid interpolating across feed/dwell/modal changes.
    //      onPart = false;
    //      lastArgs = null;
    //      result.Add(line);
    //      continue;
    //    }

    //    cMotionArguments currentArgs = cMotionArguments.getMotionArguments(line);

    //    if (!onPart || lastArgs == null)
    //    {
    //      // Start of a new contiguous on-part motion run.
    //      onPart = true;
    //      courseNum++;
    //      lastArgs = currentArgs;
    //      result.Add(line);
    //      continue;
    //    }

    //    if (IsSafeToInsert(lastArgs, currentArgs))
    //    {
    //      InsertLines(lastArgs, currentArgs, result);
    //    }

    //    result.Add(line);
    //    lastArgs = currentArgs;
    //  }

    //  progressBar1.Value = 100;
    //  return result;
    //}


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
                result.Add(line);
                thisline = $"F={options.TransitFeedRate * 60:F0} ; ({options.TransitFeedRate} mm/s Transit Feedrate)";
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
          int step = 0;
          foreach (string line in output)
          {
            string thisline = line;
            switch (step)
            {
              case 0:
                if (line.Contains("FEED"))
                {
                  result.Add(thisline);
                  result.Add("CYCLE832(0.2,_ORI_ROUGH,.5)");
                  step++;
                }
                else
                {
                  if (!line.Contains("G603"))
                    result.Add(thisline);
                }
                break;
              case 1:
                if( line.Contains("G9"))
                {
                  if( options.StopOnCut )
                  {
                    result.Add(thisline);
                    result.Add("G4F0.1");
                    step = 0;
                  }
                }
                else
                {
                  if( !line.Contains("G603"))
                    result.Add(thisline);
                }
                break;
            }
          }
        }
        else
        {// do nothing
          return output;
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

        bool getReadyforCut = false;
        for (int ii = 0; ii < output.Count; ii++)
        {
          string line = output[ii];
          string newline = line;
          if( line.Contains("FEED"))
          {
            getReadyforCut = true;
          }
          if(getReadyforCut && line.Contains("G9"))
          {
            result.Add(line);
            newline = $"UVMULT={options.UVMult / 100.0:F3}"; //turning it off for a test.  Does the UV laser tack it down good enough?  
            getReadyforCut=false;
          }
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

        output = result;
        result = new List<string>();
        int step = 0;
        double radialDistLast = 0.0;
        for(int ii = 0; ii< output.Count; ii++)
        {
          string line = output[ii];
          switch(step)
          {
            case 0: //looking for feed
              if(line.Contains("FEED"))
              {
                step++;
              }
              result.Add(line);
              break;
            case 1: //on part
              if( (line.Contains("G1") || line.Contains("G9")) && !line.Contains("DIST"))
              {
                step++;

                radialDistLast = -1;
              }
              result.Add(line);
              break;
            case 2: //looking for retract move
              //if (line.Contains("G1") || line.Contains("G9"))
              //{
                
              //  args = cMotionArguments.getMotionArguments(line);
              //  double radialDist;
              //  ToddUtils.LHT.cPose pose = new cPose(args.X, args.Y, args.Z, args.RX, args.RY, args.RZ);
              //  if (line.Contains("ROTX"))
              //  {
              //    ToddUtils.LHT.cTransform rX = new cTransform(0, 0, 0, -args.ROTX, 0, 0);                  
              //    ToddUtils.LHT.cTransform xPose = rX * pose.getTransform();
              //    pose = xPose.getPose();
              //    //Console.WriteLine($"rx: {xPose.rx:F3}");
              //  }

              //  cTransform surfaceNormal = new cTransform(0, 0, 0, pose.rx, pose.ry, pose.rz);
              //  cTransform xformed = !surfaceNormal * pose.getTransform();
              //  radialDist = xformed.z;
              //  Console.WriteLine($"y: {xformed.y:F1} z:{xformed.z:F1} dist: {radialDist:F1}");
              //  if( radialDistLast < -1)
              //  {
              //    radialDistLast = radialDist;
              //  }
              //  else
              //  {
              //    if(line.Contains("IF NOT EXIT_PRINT") ) //radialDist - radialDistLast > 25)
              //    {
              //      //inject M64
              //      result.Add("M64 ; (flyout complete)");
              //      step = 0;
              //    }
              //  }                
              //}
              if (line.Contains("IF NOT EXIT_PRINT")) //radialDist - radialDistLast > 25)
              {
                //inject M64
                result.Add("M64 ; (flyout complete)");
                step = 0;
              }
              result.Add(line);
              break;
          }
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
      output = ApplyInterpolationMode(output, options); progressBar.Value += 10;
      output = ApplyUVParameters(output, options); progressBar.Value += 10;
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
    internal static List<string> evenOutBlockSpacing(string fileName, ProgramTuningOptions opts, ProgressBar progressBar1)
    {
      List<string> output = File.ReadAllLines(fileName).ToList();
      int state = 0;
      int coursenum = 1;


      bool isMotionLine(string line)
      {
        return line.Contains("G1") || line.Contains("G9");
      }
      double dist(cMotionArguments lastPoint, cMotionArguments thisPoint)
      {
        return thisPoint.DIST - lastPoint.DIST;
      }
      void insertLines(cMotionArguments lastPoint, cMotionArguments thisPoint, List<string> result)
      {//N46 G1 X=-533.30000 Y=37.80015 Z=44.62500 RX=-0.5903670 RY=0.0000000 RZ=0.0000000 ROTX=DC(-0.59037) DIST=70.000 ; (70.000)
        double dx = thisPoint.X - lastPoint.X;
        double dy = thisPoint.Y - lastPoint.Y;
        double dz = thisPoint.Z - lastPoint.Z;
        double du = thisPoint.ROTX - lastPoint.ROTX;
        double drx = thisPoint.RX - lastPoint.RX;
        double dry = thisPoint.RY - lastPoint.RY;
        double drz = thisPoint.RZ - lastPoint.RZ;
        double dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        double dDist = thisPoint.DIST - lastPoint.DIST;
        int numInserts = (int)(dDist / 1.0);
        for (int i = 1; i <= numInserts; i++)
        {
          double ratio = (double)i / (numInserts + 1);
          cMotionArguments newArgs = new cMotionArguments();
          newArgs.X = lastPoint.X + ratio * dx;
          newArgs.Y = lastPoint.Y + ratio * dy;
          newArgs.Z = lastPoint.Z + ratio * dz;
          newArgs.ROTX = lastPoint.ROTX + ratio * du;
          newArgs.RX = lastPoint.RX + ratio * drx;
          newArgs.RY = lastPoint.RY + ratio * dry;
          newArgs.RZ = lastPoint.RZ + ratio * drz;
          newArgs.DIST = lastPoint.DIST + ratio * dDist;

          result.Add($"G1 X={newArgs.X:F5} Y={newArgs.Y:F5} Z={newArgs.Z:F5} RX={newArgs.RX:F5} RY={newArgs.RY:F5} RZ={newArgs.RZ:F5} ROTX=DC({newArgs.ROTX:F5}) DIST={newArgs.DIST:F3} ; ");
        }
      }

      List<string> result = new List<string>();
      cMotionArguments args = new cMotionArguments();
      for (int i = 0; i < output.Count; i++)
      {
        progressBar1.Value = 100 * i / output.Count;
        string line = output[i];


        if (true || coursenum < 5)
        {
          switch (state)
          {
            case 0: //off part
              if (isMotionLine(line) && line.Contains("DIST="))
              {
                args = cMotionArguments.getMotionArguments(line);
                coursenum++;
                state = 1;
              }
              result.Add(line);
              break;
            case 1: //on part
              if (isMotionLine(line))
              {
                Console.WriteLine(line);
                if (!line.Contains("DIST="))
                {
                  state = 0;
                }
                else
                {
                  string nextStartMotionBlock = line;
                  for (int j = i; j < output.Count; j++)
                  {
                    string jline = output[j];

                    if (isMotionLine(jline))
                    {
                      if (!jline.Contains("DIST="))
                      {
                        state = 0;
                        i = j;
                        result.Add(jline);
                        break;
                      }
                      nextStartMotionBlock = jline;
                      cMotionArguments nextArgs = cMotionArguments.getMotionArguments(jline);
                      if (dist(args, nextArgs) > 1.1)
                      {
                        insertLines(args, nextArgs, result);
                      }
                      result.Add(jline);
                      args = cMotionArguments.getMotionArguments(nextStartMotionBlock);
                    }
                    else
                    {
                      result.Add(jline);
                    }
                  }
                  break;
                }
              }
              else
              {
                result.Add(line);
              }
              break;
          }
        }
      }
      progressBar1.Value = 100;
      return result;
    }

    internal static void unwindPPG(string fileName, string outputFileName, ProgramTuningOptions opts, ProgressBar progressBar1)
    {
      List<string> output = File.ReadAllLines(fileName).ToList();
      int state = 0;
      int coursenum = 1;
      double distStart = 0;
      for (int i = 0; i < output.Count; i++)
      {
        string line = output[i];
        switch (state)
        {
          case 0:
            if ((line.Contains("G1") || line.Contains("G9")) && line.Contains("DIST="))
            {
              cMotionArguments args = cMotionArguments.getMotionArguments(line);
              distStart = args.DIST;
              state = 1;
            }
            break;
          case 1:
            if (line.Contains("G1") || line.Contains("G9"))
            {
              if (!line.Contains("DIST="))
              {
                state = 0;
                coursenum++;
                if (coursenum > 10)
                {
                  //that's enough data
                  return;
                }
              }
              else
              {
                string transformToString(cTransform x)
                {
                  string result;
                  result = $"{x.x:F3},{x.y:F3},{x.z:F3},{x.rx:F3},{x.ry:F3},{x.rz:F3}";
                  return result;
                }
                cMotionArguments args = cMotionArguments.getMotionArguments(line);
                cTransform rotx = new cTransform(0, 0, 0, -args.ROTX, 0, 0);
                cTransform xPose = rotx * new cTransform(args.X, args.Y, args.Z, args.RX, args.RY, args.RZ);
                Console.WriteLine($"{coursenum},{(args.DIST - distStart):F3},{transformToString(xPose)}");
              }
            }
            break;
        }
      }
    }
  }
}