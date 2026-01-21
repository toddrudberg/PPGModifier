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
    internal static void updateProgram(string filename, string outputFilename, ProgramTuningOptions options)
    {
      string numberFormat = "F6";

      double minSpacing = 10.0; // Minimum distance to trigger a print of the line
      double minAngleChange = 10.0; // Minimum angle change to trigger a print of the line
      double onCourseFeedRate = 24000.0;
      double transitFeedRate = 30000.0;
      bool remove_courseRetract = false;
      bool useOverrideFeedRates = false;

      numberFormat = options.NumberFormat;
      minSpacing = options.MinSpacing;
      minAngleChange = options.MinAngleChange;
      onCourseFeedRate = options.OnCourseFeedRate * 60;
      transitFeedRate = options.TransitFeedRate * 60;
      remove_courseRetract = options.Remove_courseRetract;
      useOverrideFeedRates = options.UseOverrideFeedRates;

      long count = 0;
      ToddUtils.FileParser.cFileParse fp = new ToddUtils.FileParser.cFileParse();
      List<string> output = new List<string>();



      List<string> ApplyFeedrate(List<string> output, ProgramTuningOptions options)
      {
        FileParser.cFileParse fp = new cFileParse();
        List<string> result = new List<string>();

        int n = 0;
        foreach (string line in output)
        {
          string thisline = line;
          switch (n)
          {
            case 0: //offpart
              if (line.Contains("F="))
              {
                fp.ReplaceArgument(line, "F=", options.OnCourseFeedRate * 60, out string newline);
                thisline = newline;
              }
              if (line.Contains("WHEN TRUE DO LAYER="))
                n++;
              break;
            case 1: //onpart
              if (line.Contains("F="))
              {
                fp.ReplaceArgument(line, "F=", options.OnCourseFeedRate * 60, out string newline);
                thisline = newline;
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

      output = ApplyFeedrate(output, options);

      //output the results:
      string text = string.Join(Environment.NewLine, output);
      if (!string.IsNullOrEmpty(outputFilename))
      {

        File.WriteAllText(outputFilename, text);
        Console.WriteLine($"Output written to {outputFilename}");
      }
      else
      {
        Clipboard.SetText(text);
        Console.WriteLine("Output copied to clipboard.");
      }
    }
    internal static void adjustBlockSpacing(string fileName, string outputFileName, ProgramTuningOptions options)
    {
      string numberFormat = "F6";

      double minSpacing = 10.0; // Minimum distance to trigger a print of the line
      double minAngleChange = 10.0; // Minimum angle change to trigger a print of the line
      double onCourseFeedRate = 24000.0;
      double transitFeedRate = 30000.0;
      bool remove_courseRetract = false;
      bool useOverrideFeedRates = false;
      bool test = false;

      numberFormat = options.NumberFormat;
      minSpacing = options.MinSpacing;
      minAngleChange = options.MinAngleChange;
      onCourseFeedRate = options.OnCourseFeedRate * 60;
      transitFeedRate = options.TransitFeedRate * 60;
      remove_courseRetract = options.Remove_courseRetract;
      useOverrideFeedRates = options.UseOverrideFeedRates;

      long count = 0;
      ToddUtils.FileParser.cFileParse fp = new ToddUtils.FileParser.cFileParse();
      List<string> output = new List<string>();

      // Helper subFunctions:
      string removeExtraDigits(string line, cMotionArguments args)
      {
        if (test)
          test = false;
        string outputline = line;
        fp.ReplaceArgument(outputline, "X=", args.X, out outputline, numberFormat);
        fp.ReplaceArgument(outputline, "Y=", args.Y, out outputline, numberFormat);
        fp.ReplaceArgument(outputline, "Z=", args.Z, out outputline, numberFormat);
        fp.ReplaceArgument(outputline, "RX=", args.RX, out outputline, numberFormat);
        fp.ReplaceArgument(outputline, "RY=", args.RY, out outputline, numberFormat);
        fp.ReplaceArgument(outputline, "RZ=", args.RZ, out outputline, numberFormat);
        fp.ReplaceArgument(outputline, "ROTX=DC(", args.ROTX, out outputline, numberFormat);
        return outputline;
      }

      // Main Process:

      int state = 0;
      cMotionArguments argsLastPrint = new cMotionArguments();
      string lastLine = "";
      string lastMotionLine = "";
      string[] lines = File.ReadAllLines(fileName);
      int nFeedLine = 0;
      int nMcodeDropped = 0;
      double lastDISTCommand = 0;
      for (int i = 0; i < lines.Length; i++)
      {
        string line = lines[i];
        if (count % 1000 == 0)
        {
          fp.GetArgument(line, "N", out double dog, true);
          Console.WriteLine($"Processing Line: {dog:F0}");
        }
        count++;

        switch (state)
        {
          case 0:
            if (line.Contains("FEED"))
            {
              nFeedLine = output.Count;

              nMcodeDropped = 0;

              state++;
            }
            break;
          case 1:
            if (line.Contains("M69"))
              state++;
            break;
          case 2:

            if (line.Contains("SET_PATH_ACCEL"))
            {
              //line = "SET_PATH_ACCES(5000,50000)"; not this one
              output.Add("G645");
            }
            else if (line.Contains("F="))
            {
              if (useOverrideFeedRates)
              {
                output.Add($"F={transitFeedRate:F1} ; set feedrate for transition");
              }
              else
              {
                output.Add($"{line} ; set feedrate for transition");
              }
              state = 0;
            }

            break;
        }

        if (line.Contains("G1") || line.Contains("G9"))
        {
          if (line.Contains("DIST="))
          {
            fp.GetArgument(line, "DIST=", out lastDISTCommand, false);
          }
        }

        if (state == 0)
        {

          if (line.Contains("G1") || line.Contains("G9"))
          {
            cMotionArguments theseArgs = cMotionArguments.getMotionArguments(line);
            string outputGline = removeExtraDigits(line, theseArgs);
            output.Add(outputGline);
          }
          else if (line.Contains("G602"))
          {
            //output.Add("G64");
            //output.Add("CPRECON");
            //output.Add("COMPCAD");
            //output.Add("COMPCURV");
            //output.Add("G603");
            //output.Add("G642 ADIS=10.0 CTOL=2.0");
            //output.Add("G641 ADIS=10.0");
            //output.Add("G645");
          }
          else if (line.Contains("SET_PATH_ACCEL"))
          {
            //output.Add("STOPRE");
            output.Add("SET_PATH_ACCEL(5000,50000) ; boost it for now");
          }
          else
          {
            output.Add(line);
          }
        }
        else if (state > 0)
        {

          if (line.Contains("G1") || line.Contains("G9"))
          {
            cMotionArguments args = cMotionArguments.getMotionArguments(line);
            double dx = args.X - argsLastPrint.X;
            double dy = args.Y - argsLastPrint.Y;
            double dz = args.Z - argsLastPrint.Z;
            double dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            double drx = Math.Abs((args.RX - argsLastPrint.RX).m180p180());
            double dry = args.RY - argsLastPrint.RY;
            double drz = Math.Abs((args.RZ - argsLastPrint.RZ).m180p180());


            if (dist > minSpacing || dry > minAngleChange || drz > minAngleChange || drx > minAngleChange)
            {
              string outputline = removeExtraDigits(line, args);

              output.Add(outputline);
              lastMotionLine = line;
              argsLastPrint = args;
            }
          }

          //Setup the Feed Command
          else if (line.Contains("FEED"))
          {
            output.Add("STOPRE");
            output.Add(line);
            //nMcodeDropped++;
            //string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] > {lastDISTCommand:F3} DO M60; FEED";
            //output[nFeedLine] += "\n" + movedMCode;
            //nMcodeDropped++;
            //movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] > {lastDISTCommand:F3} DO M62; UV Related";
            //output[nFeedLine] += "\n" + movedMCode;

            //output.Add("G644 ;  good for smoothish parts, need to evaluate for rectangular parts");
            //output.Add("G642 ADIS=3.0 ;  good for smoothish parts, need to evaluate for rectangular parts");
          }

          else if (line.Contains("M72") || line.Contains("M69") || line.Contains("M61") || line.Contains("M68") || line.Contains("M64") || line.Contains("M74") || line.Contains("CFORCE") || line.Contains("SET_PATH_ACCEL") || line.Contains("F="))
          {
            //if (lastLine.Contains("G1") || lastLine.Contains("G9"))
            {
              //if (lastLine != lastMotionLine)
              //{
              //  argsLastPrint = cMotionArguments.getMotionArguments(lastLine);
              //  string outputline = removeExtraDigits(lastLine, argsLastPrint);
              //  output.Add(outputline);
              //}
              //cMotionArguments mcodePositin = cMotionArguments.getMotionArguments(lastLine);
              if (line.Contains("M72"))
              {
                nMcodeDropped++;
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M72 ; Activate Cut Motor";
                //output[nFeedLine] += "\n" + movedMCode;
              }
              else if (line.Contains("M61"))
              {
                nMcodeDropped++;
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M61 ; Cut Prepare";
                //output[nFeedLine] += "\n" + movedMCode;
              }
              else if (line.Contains("M68"))
              {
                nMcodeDropped++;
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M68 ; Engage Compaction Brake";
                //output[nFeedLine] += "\n" + movedMCode;
              }
              else if (line.Contains("M69"))
              {
                nMcodeDropped++;
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M69 ; Disengage Compaction Break";
                //output[nFeedLine] += "\n" + movedMCode;
              }
              else if (line.Contains("M64"))
              {
                bool moveCutToSynchronousAction = true;
                if (moveCutToSynchronousAction)
                {
                  nMcodeDropped++;
                  string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M64 ; Cut";
                  output[nFeedLine] += "\n;" + movedMCode;
                }
                if (lastLine != lastMotionLine)
                {
                  output.Add(lastLine);
                }
                if (!moveCutToSynchronousAction)
                  output.Add(line);
              }
              else if (line.Contains("M74"))
              {
                nMcodeDropped++;
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M74 ; End of Tack";
                //output[nFeedLine] += "\n" + movedMCode;
              }

              else if (line.Contains("CFORCE"))
              {
                nMcodeDropped++;
                int n = line.IndexOf("CFORCE");
                string cforceCommand = line.Substring(n);
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO {cforceCommand} ; Printing Compaction";
                output[nFeedLine] += "\n" + movedMCode;
              }
              else if (line.Contains("F="))
              {
                fp.GetArgument(line, "F=", out double feedrate, false);

                if (useOverrideFeedRates)
                {
                  output[nFeedLine] += $"\nF={onCourseFeedRate:F1} ; set feedrate for course";
                }
                else
                {
                  output[nFeedLine] += $"\n{line} ; set feedrate for course";
                }
              }
            }
            //else
            //{
            //  fp.GetArgument(line, "N", out double dog, true);
            //  MessageBox.Show($"Uh Oh. check line {dog}");
            //}

          }
          else
          {
            output.Add(line);
          }
          lastLine = line;
        }

      }



      // apply the Ultimate Reality Path
      //if (options.Remove_courseRetract)
      {
        List<string> URP = new List<string>(output);
        output = DoURP(URP, options);
        //List<string> BlendRetract = new List<string>(output);
        //output = DoBlend(BlendRetract);
      }
      output = Flatten(output);

      output = AddCourseLength(output, options);
      output = DeleteM64SetCompactionForce(output,  options);
      output = MoveCourseFeedRate(output, options);
      output = NukeSetPathAccel(output, options);
      output = ChangeToLayerPath(output, options);
      output = ApplyFeedrate(output, options);
      output = Flatten(output);

      //renumber the part program
      //renumber the program:
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

      //output the results:
      string text = string.Join(Environment.NewLine, outputRenumbered);
      if (!string.IsNullOrEmpty(outputFileName))
      {

        File.WriteAllText(outputFileName, text);
        Console.WriteLine($"Output written to {outputFileName}");
      }
      else
      {
        Clipboard.SetText(text);
        Console.WriteLine("Output copied to clipboard.");
      }
    }

    private static List<string> ChangeToLayerPath(List<string> output, ProgramTuningOptions options)
    {
      FileParser.cFileParse fp = new cFileParse();
      List<string> result = new List<string>();
      foreach (string line in output)
      {
        if( line.Contains("GOTO COURSE_NUMBER"))
        {
          result.Add("GOTO ENTRY_POINT");
        }
        else if(line.Contains("WHEN TRUE DO LAYER_NUM="))
        {
          string[] dammit = line.Split(' ');
          fp.GetArgument(line, "LAYER_NUM=", out double layer);
          fp.GetArgument(line, "COURSE_NUM=", out double course);
          if(line.Contains("SKIP_RESTART"))
          {
            string res = $"SKIP_RESTART: WHEN TRUE DO LAYER={(int)layer} PATH={(int)course}";
            result.Add(res);
          }
          else if (dammit[1].Contains("COURSE"))
          {
            //LAYER1_PATH2: WHEN TRUE DO LAYER=1 PATH=2
            string res = $"LAYER{(int)layer}_PATH{(int)course}: WHEN TRUE DO LAYER={(int)layer} PATH={(int)course}";
            result.Add(res);
          }
          else
          {
            string res = $"WHEN TRUE DO LAYER={(int)layer} PATH={(int)course}";
            result.Add(res);
          }
        }
        else
        {
          result.Add(line);
        }
      }
      return result;
    }

    private static List<string> ApplyFeedrate(List<string> output, ProgramTuningOptions options)
    {
      FileParser.cFileParse fp = new cFileParse();
      List<string> result = new List<string>();

      int n = 0;
      foreach (string line in output)
      {
        string thisline = line;
        switch (n)
        {
          case 0:
            if (line.Contains("BRISK"))
              n++;
            break;
          case 1:
            if (line.Contains("F="))
            {
              fp.ReplaceArgument(line, "F=", options.OnCourseFeedRate * 60, out string newline);
              thisline = newline;
              n = 0;
            }

            break;
        }
        result.Add (thisline);
      }

      return result;
    }

    private static List<string> NukeSetPathAccel(List<string> output, ProgramTuningOptions options)
    {
      List<string> result = new List<string>();
      foreach (string s in output)
      {
        if (!s.Contains("SET_PATH_ACCEL"))
        {
          result.Add(s);
        }
      }
      return result;
    }

    private static List<string> MoveCourseFeedRate(List<string> output, ProgramTuningOptions options)
    {
      List<string> result = new List<string>();
      int state = 0;
      string fline = "";
      for (int ii = 0; ii < output.Count; ii++)
      {
        string line = output[ii];

        switch (state)
        {
          case 0:

            if (line.Contains("WHEN TRUE DO LAYER_NUM="))
            {
              state++;
            }
            result.Add(line);
            break;

          case 1:
            if (line.Contains("F="))
            {
              state++;
              fline = line;
            }
            else 
            {
              result.Add(line); 
            }
            break;

          case 2:
            if (line.Contains("FEED"))
            {
              result.Add(line);
              result.Add("BRISK");
              result.Add(fline);
              state = 0;
            }
            else
            {
              result.Add(line);
            }
            break;
        }        
      }
      return result;
    }

    private static List<string> DeleteM64SetCompactionForce(List<string> output, ProgramTuningOptions options)
    {
      List<string> result = new List<string>();
      ToddUtils.FileParser.cFileParse fp = new ToddUtils.FileParser.cFileParse();
      for (int ii = 0; ii < output.Count; ii++)
      {
        string line = output[ii];
        if (line.Contains("M64"))
        {

          //line = line.Replace(";", "");
          int idx = line.IndexOf(';');
          if (idx >= 0)
          {
            line = line.Remove(idx, 1);
          }
          line = line.Replace("M64", "M65");
          //result.Add(line);
        }
        else if(line.Contains("WHEN TRUE DO CFORCE="))
        {
          fp.ReplaceArgument(line, "CFORCE=", options.TackCompactionForce, out string newforceline);
          line = newforceline;
          result.Add(line);
        }
        else
          result.Add(line);
      }
      return result;
    }

    private static List<string> Flatten(List<string> output)
    {
      List<string> flattened = new List<string>();
      foreach (var chunk in output)
      {
        flattened.AddRange(chunk.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
      }

      output.Clear();
      output = flattened;
      return output;
    }

    private static List<string> DoBlend(List<string> blendRetract)
    {
      List<string> result = new List<string>();
      string endOfCourse = "";
      string beginningOfCourse = "";
      for (int i = 0; i < blendRetract.Count; i++)
      {
        string line = blendRetract[i];

        if( line.Contains("DIST="))
        {
          endOfCourse = line;
        }

        if( line.Contains("beginning of approach"))
        {
          beginningOfCourse = line;
        }

        if(line.Contains("start of course"))
        {
          cMotionArguments courseStart = cMotionArguments.getMotionArguments(line);
          cMotionArguments approach = cMotionArguments.getMotionArguments(beginningOfCourse);
          double dx = approach.X - courseStart.X;
          double dy = approach.Y - courseStart.Y;
          double dz = approach.Z - courseStart.Z;

          cPose p0 = new cPose()
          {
            X = courseStart.X,
            Y = courseStart.Y,
            Z = courseStart.Z,
            rX = courseStart.RX,
            rY = courseStart.RY,
            rZ = courseStart.RZ,
          };

          cPose p1 = new cPose()
          {
            X = approach.X,
            Y = approach.Y,
            Z = approach.Z,
            rX = approach.RX,
            rY = approach.RY,
            rZ = approach.RZ,
          };

          var blendPoints = GenerateParabolicBlend(p0, p1, 20);

          for(int bp = blendPoints.Count - 1; bp > 0; bp--)
          {
            cPose pt = blendPoints[bp];
            string blendLine = $"G1 X={pt.X:F6} Y={pt.Y:F6} Z={pt.Z:F6} RX={pt.rX:F6} RY={pt.rY:F6} RZ={pt.rZ:F6}";
            result.Add(blendLine);
          }
          result.Add(line);
        }
        
        else if(line.Contains("fly out"))
        {
          cMotionArguments flyOutEnd = cMotionArguments.getMotionArguments(line);
          cMotionArguments CourseEnd = cMotionArguments.getMotionArguments(endOfCourse);

          double dx = flyOutEnd.X - CourseEnd.X;
          double dy = flyOutEnd.Y - CourseEnd.Y;
          double dz = flyOutEnd.Z - CourseEnd.Z;

          cPose p0 = new cPose()
          {
            X = CourseEnd.X,
            Y = CourseEnd.Y,
            Z = CourseEnd.Z,
            rX = CourseEnd.RX,
            rY = CourseEnd.RY,
            rZ = CourseEnd.RZ,
          };

          cPose p1 = new cPose()
          {
            X = flyOutEnd.X,
            Y = flyOutEnd.Y,
            Z = flyOutEnd.Z,
            rX = flyOutEnd.RX,
            rY = flyOutEnd.RY,
            rZ = flyOutEnd.RZ,
          };

          var blendPoints = GenerateParabolicBlend(p0, p1, 20);

          for (int bp = 0; bp < blendPoints.Count - 1; bp++)
          {
            cPose pt = blendPoints[bp];
            string blendLine = $"G1 X={pt.X:F6} Y={pt.Y:F6} Z={pt.Z:F6} RX={pt.rX:F6} RY={pt.rY:F6} RZ={pt.rZ:F6}";
            result.Add(blendLine);
          }
          result.Add(line);
        }
        else
        {
          result.Add(line);
        }
      } 

      return result;
    }

    public static List<cPose> GenerateParabolicBlend(cPose p0, cPose p1, int nSeg)
    {
      if (nSeg < 2) nSeg = 2;

      List<cPose> pts = new List<cPose>();
      double dx = p1.X - p0.X;
      double dy = p1.Y - p0.Y;
      double dz = p1.Z - p0.Z;

      // build points and accumulate 3D arc length for DIST
      double prevX = p0.X, prevY = p0.Y, prevZ = p0.Z;
      double cumulative = 0.0;

      for (int i = 1; i <= nSeg; i++)
      {
        double t = (double)i / nSeg;    // 0..1
        double t2 = t * t;

        double x = p0.X + t * dx;        // linear XY
        double y = p0.Y + t * dy;
        double z = p0.Z + dz * t2;       // parabolic Z

        double dstep = Math.Sqrt(
            (x - prevX) * (x - prevX) +
            (y - prevY) * (y - prevY) +
            (z - prevZ) * (z - prevZ));
        cumulative += dstep;


        cPose dogshit = new cPose();
        dogshit.X = x;
        dogshit.Y = y;
        dogshit.Z = z;
        dogshit.rX = p0.rX;
        dogshit.rY = p0.rY;
        dogshit.rZ = p0.rZ;
        pts.Add(dogshit);

        prevX = x; prevY = y; prevZ = z;
      }

      // ensure the last point exactly equals p1 (in case of numerics)
      var last = pts[^1];
      last.X = p1.X; last.Y = p1.Y; last.Z = p1.Z;
      last.rX = p1.rX; last.rY = p1.rY; last.rZ = p1.rZ; // or keep p0’s if you prefer fixed orientation
      pts[^1] = last;

      return pts;
    }

    private static List<string> DoURP(List<string> uRP, ProgramTuningOptions options)
    {
      int state = 0;
      string sStartOfCourse = "";
      List<string> result = new List<string>();
      double startDistCommand = 0;
      ToddUtils.FileParser.cFileParse fp = new ToddUtils.FileParser.cFileParse();
      string transifFR = "";
      for (int i = 0; i < uRP.Count; i++)
      {

        string line = uRP[i];
        switch (state)
        {
          case 0:
            result.Add(line);
            if (line.Contains("APPROACH"))
            {
              sStartOfCourse = "";
              state++;
            }
            break;

          case 1:
            string case1Line = line;
            if (line.Contains("G1")) //this is the approach line
            {
              case1Line = line + " ; beginning of approach";
              state++;
            }
            result.Add(case1Line);
            break;

          case 2: //looking for end of course synchronous actions
            string case2Line = line;
            bool case2ExtraneousFR = false;
            if (line.Contains("G9") )
            {
              sStartOfCourse = line.Replace("G9", "G1");
              fp.GetArgument(line, "DIST=", out startDistCommand, false);
              break;
            }
            else if (line.Contains("G1"))
            {
              break;
            }
            else if (line.Contains("STOPRE"))
            {
              //break;
            }
            else if (line.Contains("FEED"))
            {
              case2Line = ";" + line + " ; move to synchronous action";
              break; //don't print the feed line
            }
            else if (line.Contains("ID=11"))
            {
              string feedCmd = $"ID=10 WHEN $AA_IM[DIST] > {startDistCommand:F3} DO M60; Feed Starts Here";
              //result.Add(feedCmd);
            }

            if (line.Contains("F=") && !line.Contains(" set feedrate for course"))
            {
              //skip this line
              case2ExtraneousFR = true;
            }            

            if (!case2ExtraneousFR)
            {
              result.Add(case2Line);
            }

            if (line.Contains("UV(1)"))
            {
              result.RemoveAt(result.Count - 1); //remove the UV line
              //result.Add(case2Line + " ; move to synchronous action");
              string s = sStartOfCourse;
              const string key = "DIST=";

              int idx = s.IndexOf(key, StringComparison.OrdinalIgnoreCase);

              string beforeDist = (idx >= 0)
                  ? s.Substring(0, idx)
                  : s;   // if DIST= not found, return whole string

              beforeDist = beforeDist.Replace("G1", "G9");
              result.Add(beforeDist);
              result.Add("FEED");
              result.Add(sStartOfCourse + " ; start of course ");
              state++;
            }
            break;

          case 3:
            string case3Line = line;
            if (line.Contains("G9") && !options.StopOnCut)
            {
              case3Line = line.Replace("G9", "G1");
              //case3Line += "\nM65";

            }
            if(line.Contains("M64"))
            {
              case3Line = "; " + line + " move to synchronous action";
              //break; // don't add the line. 
            }
            if( line.Contains("UV"))
            {
              case3Line = line + " ; move to synchronous action";
              state++;
              //break; // don't add the line.
            }
            result.Add(case3Line);
            break;

          case 4:
            if( line.Contains("F="))
            {
              //result.Add(line);
              transifFR = line + " ; transit feedrate ";
              state++;
            }

            break;

          case 5:
            if( line.Contains("G1"))
            {
              result.Add(line + "; fly out");
              result.Add(transifFR);
              state++;
            }
            break;

          case 6:
            string case6Line = line;
            if( (line.Contains("G1") || line.Contains("F="))&&!options.Remove_courseRetract)
            {
              break;
            }
            if( line.Contains("APPROACH"))
            {
              sStartOfCourse = "";
              state = 1;
            }
            result.Add(case6Line);
            break;
        }
      }
      return result;
    }

    internal static List<string> AddCourseLength(List<string> input, ProgramTuningOptions options)
    {
      var result = new List<string>();
      int state = 0;
      bool bFirstPass = true;

      int courseHeadIndex = -1;
      int skipRestartIndex = -1;
      int fCommandIndex = -1;
      double DistStart = 0.0;
      double DistEnd = 0.0;

      static double TryGetCutDIST(string line)
      {
        // Matches: ... $AA_IM[DIST] >= 37497.770 ...
        var m = Regex.Match(line, @"\$AA_IM\[DIST\]\s*>=\s*([0-9]*\.?[0-9]+)");

        if (double.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
          return v;
        return -1;

      }

      string InsertDistComment(string line, string comment)
      {
        const string tag = "DIST=";
        int idx = line.IndexOf(tag);
        if (idx < 0)
          return line; // no DIST= found

        int valueStart = idx + tag.Length;

        // Move through digits and decimal point
        int valueEnd = valueStart;
        while (valueEnd < line.Length &&
               (char.IsDigit(line[valueEnd]) || line[valueEnd] == '.' || line[valueEnd] == '-'))
        {
          valueEnd++;
        }

        // Extract original number
        string originalValue = line.Substring(valueStart, valueEnd - valueStart);

        // Build new line
        string before = line.Substring(0, valueEnd);
        string after = line.Substring(valueEnd);

        return $"{before} ({comment}){after}";
      }

      for (int i = 0; i < input.Count; i++)
      {
        string line = input[i];

        switch (state)
        {
          case 0:
            courseHeadIndex = -1;
            skipRestartIndex = -1;
            fCommandIndex = -1;
            if (line.Contains("G1") || line.Contains("G9"))
            {
              if (line.Contains("DIST="))
              {
                cMotionArguments motionArguments = cMotionArguments.getMotionArguments(line);
                DistStart = motionArguments.DIST;
              }
            }
            if (line.Contains("COURSE"))
            {
              if (line.Contains("WHEN TRUE DO LAYER_NUM"))
              {
                courseHeadIndex = i;
                state++;
              }
            }
            break;
          case 1:
            if (line.Contains("SKIP_RESTART:") || bFirstPass)
            {
              skipRestartIndex = i;
              state++;
            }
            break;

          case 2:
            if (line.Contains("F=") || bFirstPass)
            {
              fCommandIndex = i;
              state++;
            }
            break;

          case 3:

            if (line.Contains("M64"))
            {
              double CutDIST = TryGetCutDIST(line);
              double courseLength = CutDIST - DistStart;
              if (courseLength < 0)
                courseLength = 0;

              if( courseLength < 100)
              {
                cFileParse fp = new cFileParse();
                string fline = result[fCommandIndex];
                fp.ReplaceArgument(fline, "F=", options.ShortCourseFeedRate * 60.0, out fline);
                result[fCommandIndex] = fline;
              }

              //result[courseHeadIndex] = result[courseHeadIndex] + $" DIST_AT_CUT={CutDIST:F3} ;({courseLength:F3})";
              //result[fCommandIndex] = result[fCommandIndex] + $" DIST_AT_CUT={CutDIST:F3} ;({courseLength:F3})";
              
              //removed temporailly to work with V1.11
              result.Add($"DIST_AT_CUT={CutDIST:F3} ;({courseLength:F3})");

              //if ( !bFirstPass )
              //  result[skipRestartIndex] = result[skipRestartIndex] + $" DIST_AT_CUT={CutDIST:F3} ;({courseLength:F3})";
              bFirstPass = false;
              state++;
            }
            else if (line.Contains("DIST") && line.Contains("ID"))
            {
              //N3287 ID=11 WHEN $AA_IM[DIST] >= 27868.830 DO M74; End of Tack

              string[] splits = line.Split(' ');
              double value = double.Parse(splits[4], CultureInfo.InvariantCulture);
              //splits[5] += $" ({value - DistStart:F3})";

              //string newLine = string.Join(" ", splits);
              //line = newLine;

              int commentLocation = line.IndexOf(';');

              if (commentLocation < 0)
                line = line + $" ; ({value - DistStart:F3})";
              else
                line = line.Insert(commentLocation + 1, $" ({value - DistStart:F3})");

              line += $" ({value - DistStart:F3})";
            }
            break;

          case 4:
            if (line.Contains("DIST="))
            {
              cMotionArguments anArg = cMotionArguments.getMotionArguments(line);   
              DistEnd = anArg.DIST;

              //line = InsertDistComment(line, $"{anArg.DIST - DistStart:F3}");

              int commentLocation = line.IndexOf(';');

              if (commentLocation < 0)
                line = line + $" ; ({anArg.DIST - DistStart:F3})";
              else
                line = line.Insert(commentLocation + 1, $" ({anArg.DIST - DistStart:F3})");
            }
            if ( line.Contains("UV(0)"))
            {
              DistStart = DistEnd; //initialize DistStart to the last Dist command in the course. 
              state = 0;
            }
            break;
        }
        result.Add(line);
      }


      return result;
    }

    internal static void decoupleROTX(string fileName, string outputFileName)
    {
      List<string> lines = File.ReadAllLines(fileName).ToList();
      List<string> result = new List<string>();

      /*
        N47 G1 X=-281.244180 Y=27.077910 Z=26.973910 RX=-45.000000 RY=0.000000 RZ=134.998034 ROTX=DC(-45.000000)
        N48 G9 X=-281.244180 Y=27.077910 Z=26.973910 RX=-45.000000 RY=0.000000 RZ=134.998034 ROTX=DC(-45.000000) DIST=0.000
        FGREF[ROTX]=50.12 F1200
      */

      cMotionArguments arguments = new cMotionArguments();

      for (int ii = 0; ii < lines.Count; ii++)
      {
        string line = lines[ii];
        if (line.Contains("G1") || line.Contains("G9"))
        {
          arguments = cMotionArguments.getMotionArguments(line);
          //if (arguments.ROTX != 0)
          {
            cPose robotPose = new cPose();
            robotPose.X = arguments.X;
            robotPose.Y = arguments.Y;
            robotPose.Z = arguments.Z;
            robotPose.rX = arguments.RX;
            robotPose.rY = arguments.RY;
            robotPose.rZ = arguments.RZ;

            cLHT lhtRobot = new cLHT();
            lhtRobot.setTransformFromEulerXYZ(robotPose);

            cLHT lhtRotX = new cLHT();
            lhtRotX.setTransformFromEulerXYZ(new cPose() { rX = -arguments.ROTX });
            cLHT lhtNew = lhtRotX * lhtRobot;
            cPose newPose = lhtNew.getPoseEulerXYZ();
            double radius = Math.Sqrt(newPose.y * newPose.y + newPose.z * newPose.z);

            ToddUtils.FileParser.cFileParse fp = new ToddUtils.FileParser.cFileParse();
            fp.ReplaceArgument(line, "X=", newPose.X, out line, "F6");
            fp.ReplaceArgument(line, "Y=", newPose.Y, out line, "F6");
            fp.ReplaceArgument(line, "Z=", newPose.Z, out line, "F6");
            fp.ReplaceArgument(line, "RX=", newPose.rX, out line, "F6");
            fp.ReplaceArgument(line, "RY=", newPose.rY, out line, "F6");
            fp.ReplaceArgument(line, "RZ=", newPose.rZ, out line, "F6");

            int n = line.IndexOf("ROTX=DC");
            if (n != -1)
            {
              int nend = line.IndexOf(")");
              //line = line.Insert(n, $"FGREF[ROTX]={radius:F3} ");
              string bullshit = line.Substring(n, nend - n + 1);
              line = line.Replace(bullshit, $"ROTX={arguments.ROTX:F3}");
            }

            result.Add(line);
          }
        }
        else
        {
          result.Add(line);
        }
      }
      string text = string.Join(Environment.NewLine, result);
      File.WriteAllText(outputFileName, text);
      Console.WriteLine($"Output written to {outputFileName}");
    }

    internal static void MoveCompactBrake(string fileName, string outputFileName, ProgramTuningOptions opts)
    {


      string ReplaceDistanceValue(string line, double newValue)
      {
        // Match >= followed by optional spaces and a number (integer or decimal)
        var pattern = @"(>=\s*)([0-9]*\.?[0-9]+)";
        var replacement = $"$1{newValue:F3}";  // format with 3 decimal places like the example
        return System.Text.RegularExpressions.Regex.Replace(line, pattern, replacement);
      }
      static double TryGetDistFromGuard(string line)
      {
        // Matches: ... $AA_IM[DIST] >= 37497.770 ...
        var m = Regex.Match(line, @"\$AA_IM\[DIST\]\s*>=\s*([0-9]*\.?[0-9]+)");

        if (double.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
          return v;
        return -1;

      }

      List<string> lines = File.ReadAllLines(fileName).ToList();
      List<string> result = new List<string>();
      cMotionArguments arguments = new cMotionArguments();
      FileParser.cFileParse fp = new FileParser.cFileParse();
      double CutDist = 0.0;
      foreach (var line in lines)
      {
        string newLine = line;
        if (line.Contains("M64")) // Cutting action
        {
          CutDist = TryGetDistFromGuard(line);

        }
        else if (line.Contains("M69")) // Compaction Brake action
        {
          double newValue = CutDist + 1.0; // or whatever you need
          newLine = Regex.Replace(
              newLine,
              @"(?<=\$AA_IM\[DIST\]\s*>=\s*)([0-9]*\.?[0-9]+)",
              newValue.ToString("F3", CultureInfo.InvariantCulture));
        }
        result.Add(newLine);
      }
      string text = string.Join(Environment.NewLine, result);
      File.WriteAllText(outputFileName, text);
      Console.WriteLine($"Output written to {outputFileName}");
    }

    internal static void MoveCutPrepare(string fileName, string outputFileName, ProgramTuningOptions opts)
    {
      string ReplaceCutPrepareDISTArg(string line, double newValue)
      {
        // Match "$AA_IM[DIST]" followed by ">=" and a number
        var pattern = @"(?<=\$AA_IM\[DIST\]\s*>=\s*)([0-9]*\.?[0-9]+)";
        // Replace only the numeric literal, preserving the rest
        return System.Text.RegularExpressions.Regex.Replace(
            line,
            pattern,
            newValue.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)
        );
      }

      static double TryGetCutDIST(string line)
      {
        // Matches: ... $AA_IM[DIST] >= 37497.770 ...
        var m = Regex.Match(line, @"\$AA_IM\[DIST\]\s*>=\s*([0-9]*\.?[0-9]+)");

        if (double.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
          return v;
        return -1;

      }

      List<string> lines = File.ReadAllLines(fileName).ToList();
      List<string> result = new List<string>();
      cMotionArguments arguments = new cMotionArguments();
      FileParser.cFileParse fp = new FileParser.cFileParse();
      double CutDist = 0.0;
      double BeginningDist = 0.0;
      int theCompactionPrepareLineIndex = -1;
      
      for (int i = 0; i < lines.Count; i++)
      {
        string line = lines[i];
        string newLine = line;

        if( line.Contains("G1") || line.Contains("G9"))
        {
          cMotionArguments MotinArgs = cMotionArguments.getMotionArguments(line);
          BeginningDist = MotinArgs.DIST;
        }
        arguments = cMotionArguments.getMotionArguments(lines[0]);
        if (line.Contains("M61")) // Cut Prepare action
        {
          //CutDist = TryGetDistFromGuard(line);
          theCompactionPrepareLineIndex = result.Count;

        }
        else if (line.Contains("M64")) // Cut 
        {
          CutDist = TryGetCutDIST(line);
          if (theCompactionPrepareLineIndex != -1)
          {
            double newValue = CutDist - 100.0; // or whatever you need
            if( newValue < BeginningDist)
            {
              newValue = BeginningDist + 10.0;
            }
            string compactionPrepareLine = result[theCompactionPrepareLineIndex];
            compactionPrepareLine = ReplaceCutPrepareDISTArg(compactionPrepareLine, newValue);

            result[theCompactionPrepareLineIndex] = compactionPrepareLine;
          }
        }
        result.Add(newLine);
      }
      string text = string.Join(Environment.NewLine, result);
      File.WriteAllText(outputFileName, text);
      Console.WriteLine($"Output written to {outputFileName}");
    }

    internal static List<string> MoveCutPrepare(List<string> input)
    {
      List<string> result = new List<string>();
      string ReplaceCutPrepareDISTArg(string line, double newValue)
      {
        // Match "$AA_IM[DIST]" followed by ">=" and a number
        var pattern = @"(?<=\$AA_IM\[DIST\]\s*>=\s*)([0-9]*\.?[0-9]+)";
        // Replace only the numeric literal, preserving the rest
        return System.Text.RegularExpressions.Regex.Replace(
            line,
            pattern,
            newValue.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)
        );
      }

      static double TryGetCutDIST(string line)
      {
        // Matches: ... $AA_IM[DIST] >= 37497.770 ...
        var m = Regex.Match(line, @"\$AA_IM\[DIST\]\s*>=\s*([0-9]*\.?[0-9]+)");

        if (double.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
          return v;
        return -1;

      }

      cMotionArguments arguments = new cMotionArguments();
      FileParser.cFileParse fp = new FileParser.cFileParse();
      double CutDist = 0.0;
      double BeginningDist = 0.0;
      int theCompactionPrepareLineIndex = -1;

      for (int i = 0; i < input.Count; i++)
      {
        string line = input[i];
        string newLine = line;

        if (line.Contains("G1") || line.Contains("G9"))
        {
          cMotionArguments MotinArgs = cMotionArguments.getMotionArguments(line);
          BeginningDist = MotinArgs.DIST;
        }
        if (line.Contains("M61")) // Cut Prepare action
        {
          //CutDist = TryGetDistFromGuard(line);
          theCompactionPrepareLineIndex = result.Count;
        }
        else if (line.Contains("M64")) // Cut 
        {
          CutDist = TryGetCutDIST(line);
          if (theCompactionPrepareLineIndex != -1)
          {
            double newValue = CutDist - 100.0; // or whatever you need
            if (newValue < BeginningDist)
            {
              newValue = BeginningDist + 10.0;
            }
            string compactionPrepareLine = result[theCompactionPrepareLineIndex];
            compactionPrepareLine = ReplaceCutPrepareDISTArg(compactionPrepareLine, newValue);

            result[theCompactionPrepareLineIndex] = compactionPrepareLine;
          }
        }
        result.Add(newLine);
      }
      return result;
    }
  }
}