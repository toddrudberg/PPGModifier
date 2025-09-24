using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToddUtils.SiemensCCIMotionVariables;
using ToddUtils.LHT;


namespace ToddUtils
{
  public class ProgramConversions
  {
    internal static void adjustBlockSpacing(string fileName, string outputFileName, ProgramTuningOptions options)
    {
      string numberFormat = "F6";

      double minSpacing = 10.0; // Minimum distance to trigger a print of the line
      double minAngleChange = 10.0; // Minimum angle change to trigger a print of the line
      bool remove_courseRetract = false;

      numberFormat = options.NumberFormat;
      minSpacing = options.MinSpacing;
      minAngleChange = options.MinAngleChange;
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
              state++;
            }
            if (!remove_courseRetract)
              state = 0;
            break;
          case 3:
            if (line.Contains("EXIT_PRINT"))
            {
              state++;
              continue;
            }
            break;
          case 4:
            if (line.Contains("SKIP_EXIT:"))
            {
              state = 0;
            }
            continue;
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
            output.Add("G644 ;  good for smoothish parts, need to evaluate for rectangular parts");
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
                output[nFeedLine] += $"\nF={feedrate:F1} ; set feedrate for course";
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
  }
}