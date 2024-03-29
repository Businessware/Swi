﻿using System;
using System.CommandLine;
using System.IO;
using System.Linq;
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

namespace SwiDecode
{
    internal class Program
    {
        private static readonly ImageFormatManager rgbFormatMgr;
        private static readonly ImageFormatManager grayFormatMgr;

        static Program()
        {
            rgbFormatMgr = new ImageFormatManager();
            rgbFormatMgr.SetEncoder(JpegFormat.Instance,
                new JpegEncoder() { ColorType = JpegColorType.Rgb });
            rgbFormatMgr.SetEncoder(PngFormat.Instance,
                new PngEncoder() { ColorType = PngColorType.Rgb });
            rgbFormatMgr.SetEncoder(BmpFormat.Instance,
                new BmpEncoder() { BitsPerPixel = BmpBitsPerPixel.Pixel24 });
            rgbFormatMgr.SetEncoder(PbmFormat.Instance,
                new PbmEncoder() { ColorType = PbmColorType.Rgb });

            grayFormatMgr = new ImageFormatManager();
            grayFormatMgr.SetEncoder(JpegFormat.Instance,
                new JpegEncoder() { ColorType = JpegColorType.Luminance });
            grayFormatMgr.SetEncoder(PngFormat.Instance,
                new PngEncoder() { ColorType = PngColorType.Grayscale });
            grayFormatMgr.SetEncoder(BmpFormat.Instance,
                new BmpEncoder() { BitsPerPixel = BmpBitsPerPixel.Pixel8 });
            grayFormatMgr.SetEncoder(PbmFormat.Instance,
                new PbmEncoder() { ColorType = PbmColorType.Grayscale });
        }

        private static Task Decode(FileInfo swiFile, FileInfo imgFile, bool oldDecoder)
        {
            return Task.Run(async () =>
            {
                byte[] encoded = File.ReadAllBytes(swiFile.FullName);

                int errorCode = 0;
                byte[] pixels = null;
                int width = 0;
                int height = 0;
                int bitsPerPixel = 0;

                if (!oldDecoder)
                {
                    errorCode = SwiCodec.WiToRaw(encoded, out pixels,
                        out width, out height, out bitsPerPixel);
                }
                else
                {
                    errorCode = SwiCodec.SiToRaw(encoded, out pixels,
                        out width, out height, out bitsPerPixel);
                }

                if (errorCode != 0)
                {
                    throw new InvalidOperationException(
                        $"{(oldDecoder ? "SI" : "WI")} decode failed: Error={errorCode}");
                }

                string fileExt = Path.GetExtension(imgFile.Name).Substring(1).ToLower();

                IImageFormat format = null;
                IImageEncoder encoder = null;
                Image image = null;

                if (bitsPerPixel == 8)
                {
                    image = Image.LoadPixelData<L8>(pixels, width, height);
                    format = grayFormatMgr.FindFormatByFileExtension(fileExt)
                        ??
                        throw new NotSupportedException(
                            "Unsupported grayscale image format type");
                    encoder = grayFormatMgr.FindEncoder(format)
                        ??
                        throw new NotSupportedException(
                            "Unsupported grayscale image encoder");
                }
                else if (bitsPerPixel == 24)
                {
                    image = Image.LoadPixelData<Rgb24>(pixels, width, height);
                    format = rgbFormatMgr.FindFormatByFileExtension(fileExt)
                        ??
                        throw new NotSupportedException("Unsupported RGB image format type");
                    encoder = rgbFormatMgr.FindEncoder(format)
                        ??
                        throw new NotSupportedException("Unsupported RGB image encoder");
                }
                else
                {
                    throw new NotSupportedException(
                        "Only 8, 24 bits-per-pixel images supported");
                }

                if (!imgFile.Directory.Exists)
                {
                    imgFile.Directory.Create();
                }

                await image.SaveAsync(imgFile.FullName, encoder);
            });
        }

        private static async Task<int> Main(string[] args)
        {
            StringBuilder fileExts = new StringBuilder();
            rgbFormatMgr.ImageFormats.ToList().ForEach(iif =>
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

            Option<FileInfo> swiFileOption = new Option<FileInfo>(
                name: "--swi",
                description:
                    "File path of WI image to decode.")
            {
                IsRequired = true
            };

            swiFileOption.AddValidator(result =>
            {
                FileInfo fileInfo = result.GetValueForOption(swiFileOption);
                if (fileInfo == null || !fileInfo.Exists)
                {
                    result.ErrorMessage = "WI file does not exist";
                }
            });

            Option<FileInfo> imgFileOption = new Option<FileInfo>(
                name: "--img",
                description:
                    "File path of decoded image file.\n" +
                    $"Specify file type as follows:\n{fileExts}")
            {
                IsRequired = true
            };

            Option<bool> oldOption = new Option<bool>(
                name: "--old",
                description:
                    "If 'true', the old decoding method is used.\n" +
                    "If 'false', or omitted, the new method is used.");

            RootCommand rootCommand = new RootCommand("Convert image file to Summus WI format");

            rootCommand.AddOption(swiFileOption);
            rootCommand.AddOption(imgFileOption);
            rootCommand.AddOption(oldOption);

            rootCommand.SetHandler(async (swiFile, imgFile, oldDecoder) =>
            {
                try
                {
                    await Decode(swiFile, imgFile, oldDecoder);
                    Console.WriteLine($"Decoding completed, created file:\n  {imgFile.FullName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception:\n  {ex.Message}");
                }
            },
            swiFileOption, imgFileOption, oldOption);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
