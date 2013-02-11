using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace BatchRunPOV
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 1)
                Directory.SetCurrentDirectory(args[1]);
            string povCommand = @"D:\PovRay\bin\pvengine64.exe";
            if (args.Length > 0)
                povCommand = args[0];

            string povArgs = "/RENDER -D +W800 +H600 +A0.1";

            List<Process> runningProcesses = new List<Process>();

            var shapes = new string[] { "circle", "cross", "cube", "cone" };
            var colors = new string[] { "red", "green", "blue", "yellow" };

            foreach (var shape in shapes)
            {
                foreach (var clr in colors)
                {
                    foreach (bool white in new bool[] { false, true })
                    {
                        string newFileName = shape + "-" + clr + (white ? "-white" : "-black") + ".pov";
                        string modifiedFile = "#include \"pdshapes.inc\"\nPDShapeLight()\n";
                        modifiedFile += "object { PD" + shape + " PDShapeMaterial(PD" + clr + ") }\n";
                        if (white) modifiedFile += "PDWhite()\n";
                        File.WriteAllText(newFileName, modifiedFile);
                        runningProcesses.Add(Process.Start(new ProcessStartInfo { FileName = povCommand, Arguments = povArgs + " " + newFileName + " /EXIT" }));
                    }
                }
            }

            foreach (var proc in runningProcesses)
                proc.WaitForExit();
            runningProcesses.Clear();

            foreach (var shape in shapes)
            {
                foreach (var clr in colors)
                {
                    string whiteFileName = shape + "-" + clr + "-white.bmp";
                    string blackFileName = shape + "-" + clr + "-black.bmp";
                    createAlphaBitmap(new Bitmap(blackFileName), new Bitmap(whiteFileName)).Save(shape + "-" + clr + ".png");
                }
            }

#if false
            int maxDigit = 9;
            for (int i = 0; i <= maxDigit; i++)
            {
                foreach (bool white in new bool[] { false, true })
                {
                    string newFileName = i.ToString() + (white ? "-white.pov" : "-black.pov");
                    File.WriteAllText(newFileName, "#include \"pddigits.inc\"\nPDdigit()\nobject { PDdigit" + i + " material { PDDigitsMaterial } }\n" + (white ? "PDWhite()\n" : ""));
                    runningProcesses.Add(Process.Start(new ProcessStartInfo { FileName = povCommand, Arguments = povArgs + " " + newFileName + " /EXIT" }));
                }
            }

            foreach (var proc in runningProcesses)
                proc.WaitForExit();
            runningProcesses.Clear();

            for (int i = 0; i <= maxDigit; i++)
            {
                string whiteFileName = i.ToString() + "-white.bmp";
                string blackFileName = i.ToString() + "-black.bmp";
                createAlphaBitmap(new Bitmap(blackFileName), new Bitmap(whiteFileName)).Save(i.ToString() + ".png");
            }
#endif
        }

        /// <summary>
        /// Expects two 600×600 bitmaps which represent a graphic against a black and a white background.
        /// Returns an approximation of the graphic with alpha transparency.
        /// </summary>
        static Bitmap createAlphaBitmap(Bitmap bmpBlack, Bitmap bmpWhite)
        {
            Bitmap output = new Bitmap(600, 600, PixelFormat.Format32bppArgb);
            unsafe
            {
                var blackData = bmpBlack.LockBits(new Rectangle(0, 0, bmpBlack.Width, bmpBlack.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var whiteData = bmpWhite.LockBits(new Rectangle(0, 0, bmpWhite.Width, bmpWhite.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var outputData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                for (int y = 0; y < 600; y++)
                {
                    byte* pBlack = (byte*) blackData.Scan0 + y * blackData.Stride;
                    byte* pWhite = (byte*) whiteData.Scan0 + y * whiteData.Stride;
                    byte* pOut = (byte*) outputData.Scan0 + y * outputData.Stride;
                    for (int outputX = 0; outputX < 600; outputX++)
                    {
                        int intputX = outputX + 100;
                        double bBlack = (double) pBlack[3 * intputX] / 255;
                        double gBlack = (double) pBlack[3 * intputX + 1] / 255;
                        double rBlack = (double) pBlack[3 * intputX + 2] / 255;

                        double bWhite = (double) pWhite[3 * intputX] / 255;
                        double gWhite = (double) pWhite[3 * intputX + 1] / 255;
                        double rWhite = (double) pWhite[3 * intputX + 2] / 255;

                        double lBlack = (Math.Max(rBlack, Math.Max(gBlack, bBlack)) + Math.Min(rBlack, Math.Min(gBlack, bBlack))) / 2;
                        double lWhite = (Math.Max(rWhite, Math.Max(gWhite, bWhite)) + Math.Min(rWhite, Math.Min(gWhite, bWhite))) / 2;

                        double alpha = lBlack + 1 - lWhite;
                        double red = rBlack / (rBlack + 1 - rWhite);
                        double green = gBlack / (gBlack + 1 - gWhite);
                        double blue = bBlack / (bBlack + 1 - bWhite);

                        pOut[4 * outputX] = (byte) (255 * blue);
                        pOut[4 * outputX + 1] = (byte) (255 * green);
                        pOut[4 * outputX + 2] = (byte) (255 * red);
                        pOut[4 * outputX + 3] = (byte) (255 * alpha);
                    }
                }
                bmpBlack.UnlockBits(blackData);
                bmpWhite.UnlockBits(whiteData);
                output.UnlockBits(outputData);
            }
            return output;
        }
    }
}
