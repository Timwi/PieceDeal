using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace BatchRunPOV
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 1)
                Directory.SetCurrentDirectory(args[1]);
            string povCommand = "pov";
            if (args.Length > 0)
                povCommand = args[0];

            List<Process> runningProcesses = new List<Process>();

            foreach (var shape in new string[] { "circle", "cross", "cube", "cone" })
            {
                foreach (var clr in new string[] { "red", "green", "blue", "yellow" })
                {
                    foreach (bool white in new bool[] { false, true })
                    {
                        string newFileName = shape + "-" + clr + (white ? "-white" : "-black") + ".pov";
                        string modifiedFile = "#include \"pdshapes.inc\"\nPDShapeLight()\n";
                        modifiedFile += "object { PD" + shape + " PDShapeMaterial(PD" + clr + ") }\n";
                        if (white) modifiedFile += "PDWhite()\n";
                        File.WriteAllText(newFileName, modifiedFile);
                        runningProcesses.Add(Process.Start(new ProcessStartInfo { FileName = povCommand, Arguments = newFileName }));
                    }
                }
            }

            for (int i = 0; i < 10; i++)
            {
                foreach (bool white in new bool[] { false, true })
                {
                    string newFileName = i.ToString() + (white ? "-white.pov" : "-black.pov");
                    File.WriteAllText(newFileName, "#include \"pddigits.inc\"\nPDdigit()\nobject { PDdigit" + i + " material { PDDigitsMaterial } }\n" + (white ? "PDWhite()\n" : ""));
                    runningProcesses.Add(Process.Start(new ProcessStartInfo { FileName = povCommand, Arguments = newFileName }));
                }
            }
        }
    }
}
