using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Swi32.Codec;

namespace SwiEncode
{
    internal class Program
    {
        private static readonly ImageFormatManager formatMgr = new ImageFormatManager();

        static Program()
        {
            formatMgr.AddImageFormat(JpegFormat.Instance);
            formatMgr.AddImageFormat(PngFormat.Instance);
            formatMgr.AddImageFormat(BmpFormat.Instance);
            formatMgr.AddImageFormat(PbmFormat.Instance);
        }

        private static Task Encode(FileInfo imgFile, FileInfo swiFile)
        {
            return Task.Run(async () =>
            {
                Stream imgStream = null;

                try
                {
                    imgStream = new FileStream(imgFile.FullName, FileMode.Open, FileAccess.Read);

                    (IImageInfo ImageInfo, IImageFormat Format) = await Image.IdentifyWithFormatAsync(imgStream);

                    _ = imgStream.Seek(0, SeekOrigin.Begin);

                    if (!formatMgr.ImageFormats.Contains(Format))
                    {
                        throw new NotSupportedException("Unsupported image format type");
                    }

                    byte[] pixels = null;

                    if (ImageInfo.PixelType.BitsPerPixel == 8)
                    {
                        Image<L8> rgbImage = await Image.LoadAsync<L8>(imgStream);
                        pixels = new byte[ImageInfo.Width * ImageInfo.Height * Unsafe.SizeOf<L8>()];
                        rgbImage.CopyPixelDataTo(pixels);
                    }
                    else if (ImageInfo.PixelType.BitsPerPixel == 24)
                    {
                        Image<Rgb24> grayImage = await Image.LoadAsync<Rgb24>(imgStream);
                        pixels = new byte[ImageInfo.Width * ImageInfo.Height * Unsafe.SizeOf<Rgb24>()];
                        grayImage.CopyPixelDataTo(pixels);
                    }
                    else
                    {
                        throw new NotSupportedException(
                            "Only 8, 24 bits-per-pixel images supported");
                    }

                    int errorCode = SwiCodec.FromRaw(pixels, ImageInfo.Width, ImageInfo.Height,
                        ImageInfo.PixelType.BitsPerPixel, out byte[] encoded);

                    if (errorCode != 0)
                    {
                        throw new InvalidOperationException(
                            $"WI encode failed: Error={errorCode}");
                    }

                    string outFolderPath = swiFile != null ? swiFile.DirectoryName : imgFile.DirectoryName;

                    string outFileName = swiFile != null
                        ? swiFile.Name
                        : $"{Path.GetFileNameWithoutExtension(imgFile.Name)}.wi";

                    string outFilePath = Path.Combine(outFolderPath, outFileName);

                    File.WriteAllBytes(outFilePath, encoded);

                    Console.WriteLine($"Encoding completed, created file:\n  {outFilePath}");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception:\n  {ex.Message}");
                }
                finally
                {
                    imgStream?.Dispose();
                }
            });
        }

        private static async Task<int> Main(string[] args)
        {
            StringBuilder fileExts = new StringBuilder();
            formatMgr.ImageFormats.ToList().ForEach(iif =>
            {
                iif.FileExtensions.ToList().ForEach(ext =>
                {
                    if (fileExts.Length > 0)
                    {
                        _ = fileExts.Append(", ");
                    }
                    _ = fileExts.Append($".{ext}");
                });
            });

            Option<FileInfo> imgFileOption = new Option<FileInfo>(
                name: "--img",
                description:
                    "File path of image to encode.\n" +
                    $"Accepted file extensions:\n{fileExts}")
            {
                IsRequired = true
            };

            imgFileOption.AddValidator(result =>
            {
                FileInfo fileInfo = result.GetValueForOption(imgFileOption);
                if (fileInfo == null || !fileInfo.Exists)
                {
                    result.ErrorMessage = "Image file does not exist";
                }
            });

            Option<FileInfo> swiFileOption = new Option<FileInfo>(
                name: "--swi",
                description:
                    "File path of encoded WI image.\n" +
                    "If not provided will use same file folder " +
                    "and file name with '.wi' extension.");

            RootCommand rootCommand = new RootCommand("Convert image file to Summus WI format");

            rootCommand.AddOption(imgFileOption);
            rootCommand.AddOption(swiFileOption);

            rootCommand.SetHandler(async (imgFile, swiFile) =>
            {
                await Encode(imgFile, swiFile);
            },
            imgFileOption, swiFileOption);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
