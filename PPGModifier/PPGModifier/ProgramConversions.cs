using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToddUtils.LHT;
using ToddUtils.SiemensCCIMotionVariables;
using static System.Net.Mime.MediaTypeNames;


namespace ToddUtils
{
  public class ProgramConversions
  {
    internal static void adjustBlockSpacing(string fileName, string outputFileName, ProgramTuningOptions options)
    {
      string numberFormat = "F6";

      double minSpacing = 10.0; // Minimum distance to trigger a print of the line
      double minAngleChange = 10.0; // Minimum angle change to trigger a print of the line
      double onCourseFeedRate = 24000.0;
      double transitFeedRate = 30000.0;
      bool remove_courseRetract = false;

      numberFormat = options.NumberFormat;
      minSpacing = options.MinSpacing;
      minAngleChange = options.MinAngleChange;
      onCourseFeedRate = options.OnCourseFeedRate;
      transitFeedRate = options.TransitFeedRate;
      remove_courseRetract = options.Remove_courseRetract;

      long count = 0;
      ToddUtils.FileParser.cFileParse fp = new ToddUtils.FileParser.cFileParse();
      List<string> output = new List<string>();

      // Helper subFunctions:
      string removeExtraDigits(string line, cMotionArguments args)
      {
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
              output.Add("G645");
            }
            else if (line.Contains("F="))
            {
              output.Add($"F={transitFeedRate:F1} ; set feedrate for transit");
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
            output.Add("G603");
            output.Add("G645");
          }
          //else if (line.Contains("SET_PATH_ACCEL"))
          //{
          //  output.Add("STOPRE");
          //  output.Add(line);
          //}
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
            //output.Add("G644 ;  good for smoothish parts, need to evaluate for rectangular parts");
            output.Add("G645 ;  good for smoothish parts, need to evaluate for rectangular parts");
          }

          //deal with SET_PATH_ACCEL
          //else if (line.Contains("SET_PATH_ACCEL"))
          //{
          //  output.Add("STOPRE");
          //  output.Add(line);
          //}

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
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M72; Activate Cut Motor";
                output[nFeedLine] += "\n" + movedMCode;
              }
              else if (line.Contains("M61"))
              {
                nMcodeDropped++;
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M61; Cut Prepare";
                output[nFeedLine] += "\n" + movedMCode;
              }
              else if (line.Contains("M68"))
              {
                nMcodeDropped++;
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M68; Engage Compaction Brake";
                output[nFeedLine] += "\n" + movedMCode;
              }
              else if (line.Contains("M69"))
              {
                nMcodeDropped++;
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M69; Disengage Compaction Break";
                output[nFeedLine] += "\n" + movedMCode;
              }
              else if (line.Contains("M64"))
              {
                nMcodeDropped++;
                //string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M64";
                if (lastLine != lastMotionLine)
                {
                  output.Add(lastLine);
                }
                output.Add(line);
              }
              else if (line.Contains("M74"))
              {
                nMcodeDropped++;
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO M74; End of Tack";
                output[nFeedLine] += "\n" + movedMCode;
              }
              else if (line.Contains("CFORCE"))
              {
                nMcodeDropped++;
                int n = line.IndexOf("CFORCE");
                string cforceCommand = line.Substring(n);
                string movedMCode = $"ID={nMcodeDropped + 10} WHEN $AA_IM[DIST] >= {lastDISTCommand:F3} DO {cforceCommand}; Printing Compaction";
                output[nFeedLine] += "\n" + movedMCode;
              }
              else if (line.Contains("F="))
              {
                fp.GetArgument(line, "F=", out double feedrate, false);
                output[nFeedLine] += $"\nF={onCourseFeedRate:F1} ; set feedrate for course";
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


      //renumber the program:
      int nNum = 1;
      List<string> outputRenumbered = new List<string>();

      List<string> flattened = new List<string>();
      foreach (var chunk in output)
      {
        flattened.AddRange(chunk.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
      }

      output.Clear();
      output = flattened;

      // apply the Ultimate Reality Path
      if (options.Remove_courseRetract)
      {
        List<string> URP = new List<string>(output);
        output = DoURP(URP);
      }

      for (int i = 0; i < output.Count; i++)
      {
        string line = output[i];
        string trimmed = line.TrimStart();

        if (i == 52)
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





      //output the resutlts:
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

    private static List<string> DoURP(List<string> uRP)
    {
      int state = 0;
      string sStartOfCourse = "";
      List<string> result = new List<string>();
      double startDistCommand = 0;
      ToddUtils.FileParser.cFileParse fp = new ToddUtils.FileParser.cFileParse();
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
            if (line.Contains("G9"))
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
              break;
            }
            else if (line.Contains("FEED"))
            {
              case2Line = ";" + line + " ; move to synchronous action";
            }
            else if (line.Contains("ID=11"))
            {
              string feedCmd = $"ID=10 WHEN $AA_IM[DIST] >= {startDistCommand:F3} DO FEED; Feed Starts Here";
              result.Add(feedCmd);
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
              result.Add(";" + case2Line + " ; end of course synchronous actions and move this to synchronous action");
              result.Add(sStartOfCourse + " ; start of course ");
              state++;
            }
            break;

          case 3:
            string case3Line = line;
            if( line.Contains("G9"))
            {
              case3Line = line.Replace("G9", "G1");
            }
            if(line.Contains("M64"))
            {
              case3Line = "; " + line + " move to synchronous action";
            }
            if( line.Contains("UV"))
            {
              case3Line = ";" + line + " ; move to synchronous action";
              state++;
            }
            result.Add(case3Line);
            break;

          case 4:
            if( line.Contains("F="))
            {
              result.Add(line);
              state++;
            }

            break;

          case 5:
            if( line.Contains("G1"))
            {
              result.Add(line + "; fly out");
              state++;
            }
            break;

          case 6:
            string case6Line = line;
            if( line.Contains("G1") || line.Contains("F="))
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
  }
}