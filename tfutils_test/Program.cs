namespace TFUtilsTest
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using SharpTox.Av;
    using Toxofone.Utils;

    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0 || (args.Length == 1 && args[0].Equals("-h", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("tfutils_test:");
                Console.WriteLine("  -i fileName  input image filename");
                Console.WriteLine("  -o fileName  [optional] output image filename");
                Console.WriteLine("  -n count     count of test iterations");
                return;
            }

            string inputFileName = null;
            string outputFileName = null;
            int count = 0;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-i", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/i", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        inputFileName = args[++i];
                    }
                }
                else if (args[i].Equals("-o", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/o", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        outputFileName = args[++i];
                    }
                }
                else if (args[i].Equals("-n", StringComparison.OrdinalIgnoreCase) || args[i].Equals("/n", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        String countStr = args[++i];
                        int result = -1;
                        if (int.TryParse(countStr, out result))
                        {
                            count = result;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(inputFileName))
            {
                Console.WriteLine("Input filename not supplied");
                return;
            }
            if (string.IsNullOrEmpty(outputFileName))
            {
                // do nothing
            }
            if (count <= 0)
            {
                Console.WriteLine("Count of iterations not supplied or negative: " + count.ToString());
                return;
            }

            try
            {
                Image inputImage = Image.FromFile(inputFileName);

                using (Bitmap inputBitmap = new Bitmap(inputImage))
                {
                    if (inputBitmap.PixelFormat != PixelFormat.Format32bppArgb)
                    {
                        Console.WriteLine("Failed to prepare 32 bpp image from file: " + inputFileName);
                        return;
                    }

                    Console.WriteLine(string.Format("Input file  : {0}", inputFileName));
                    Console.WriteLine(string.Format("Image size  : {0}x{1}", inputBitmap.Width, inputBitmap.Height));
                    Console.WriteLine(string.Format("Image format: {0}", inputBitmap.PixelFormat));
                    Console.WriteLine(string.Format("Output file : {0}", (!string.IsNullOrEmpty(outputFileName) ? outputFileName : "N/A")));
                    Console.WriteLine(string.Format("Count       : {0}", count));
                    Console.WriteLine();

                    VideoUtils.ResetTimingInfo();

                    for (int i = 0; i < count; i++)
                    {
                        ToxAvVideoFrame toxFrame = VideoUtils.BitmapToToxAvFrame(inputBitmap);
                        Bitmap outputBitmap = VideoUtils.ToxAvFrameToBitmap(toxFrame);
                        outputBitmap.Dispose();
                    }

                    Console.WriteLine(">>>> " + VideoUtils.GetToFrameTimingInfo());
                    Console.WriteLine("<<<< " + VideoUtils.GetFromFrameTimingInfo());

                    if (!string.IsNullOrEmpty(outputFileName))
                    {
                        ToxAvVideoFrame toxFrame = VideoUtils.BitmapToToxAvFrame(inputBitmap);
                        Bitmap outputBitmap = VideoUtils.ToxAvFrameToBitmap(toxFrame);
                        outputBitmap.Save(outputFileName, inputImage.RawFormat);
                        outputBitmap.Dispose();
                    }

                    Console.WriteLine();
                    Console.WriteLine("Another round with LibYuv ...");
                    if (VideoUtils.CpuHasSsse3)
                    {
                        Console.WriteLine("CPU has SSSE3 ...");
                    }
                    Console.WriteLine();

                    VideoUtils.ResetTimingInfo();

                    for (int i = 0; i < count; i++)
                    {
                        ToxAvVideoFrame toxFrame = VideoUtils.BitmapToToxAvFrame(inputBitmap, true);
                        Bitmap outputBitmap = VideoUtils.ToxAvFrameToBitmap(toxFrame, true);
                        outputBitmap.Dispose();
                    }

                    Console.WriteLine(">>>> " + VideoUtils.GetToFrameTimingInfo());
                    Console.WriteLine("<<<< " + VideoUtils.GetFromFrameTimingInfo());

                    if (!string.IsNullOrEmpty(outputFileName))
                    {
                        ToxAvVideoFrame toxFrame = VideoUtils.BitmapToToxAvFrame(inputBitmap, true);
                        Bitmap outputBitmap = VideoUtils.ToxAvFrameToBitmap(toxFrame, true);
                        outputFileName = AppendFileNameSuffix(outputFileName, "-libyuv");
                        outputBitmap.Save(outputFileName, inputImage.RawFormat);
                        outputBitmap.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            //Console.WriteLine();
            //Console.WriteLine("Press ENTER to exit ...");
            //Console.ReadLine();
        }

        private static string AppendFileNameSuffix(string filePath, string fileNameSuffix)
        {
            string fileDir = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileExt = Path.GetExtension(filePath);
            return Path.Combine(fileDir, fileName + fileNameSuffix + fileExt);
        }
    }
}
