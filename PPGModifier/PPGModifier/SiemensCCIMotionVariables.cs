using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToddUtils.SiemensCCIMotionVariables
{
  public class cMotionArguments
  {
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double RZ { get; set; }
    public double RY { get; set; }
    public double RX { get; set; }
    public double ROTX { get; set; } // DC value
    public double DIST { get; set; } // Distance argument
    public double N { get; set; } // N value, if needed

    public static cMotionArguments getMotionArguments(string line)
    {
      ToddUtils.FileParser.cFileParse fp = new ToddUtils.FileParser.cFileParse();
      cMotionArguments args = new cMotionArguments();
      fp.GetArgument(line, "X=", out double X, false);
      fp.GetArgument(line, "Y=", out double Y, false);
      fp.GetArgument(line, "Z=", out double Z, false);
      fp.GetArgument(line, "RZ=", out double RZ, false);
      fp.GetArgument(line, "RY=", out double RY, false);
      fp.GetArgument(line, "RX=", out double RX, false);
      fp.GetArgument(line, "ROTX=DC(", out double ROTX, false);
      fp.GetArgument(line, "DIST=", out double DIST, false);
      fp.GetArgument(line, "N", out double N, false); // Optional N value

      args.X = X;
      args.Y = Y;
      args.Z = Z;
      args.RZ = RZ;
      args.RY = RY;
      args.RX = RX;
      args.ROTX = ROTX;
      args.N = N; // Set N value if it exists
      args.DIST = DIST;

      return args;
    }

    public static cMotionArguments getAPPROACH_ROTX_ARGUMENTS(string line)
    {
      cMotionArguments output = new();

      int start = line.IndexOf('(');
      int end = line.IndexOf(')');

      if (start == -1 || end == -1 || end <= start)
        return output; // return empty if format is bad

      string args = line.Substring(start + 1, end - start - 1);

      string[] parts = args.Split(',');

      List<double> values = new List<double>();
      foreach (string part in parts)
      {
        if (double.TryParse(part.Trim(), out double val))
        {
          values.Add(val);
        }
      }

      output.X = values[0];
      output.Y = values[1];
      output.Z = values[2];
      output.RX = values[3];
      output.RY = values[4];
      output.RZ = values[5];
      output.ROTX = values[6];
      output.DIST = values[7];

      ToddUtils.FileParser.cFileParse fp = new();
      fp.GetArgument(line, "N", out double N, false); // Optional N value
      output.N = (int)N;


      return output;

    }
  }
}
