using System;
using System.Runtime.InteropServices;

using Swi32.Interop;
using static Swi32.Interop.SwiInterop;

namespace Swi32.Codec
{
    public static class SwiCodec
    {
        // TODO: Supports only 8; 24 bits-per-pixel images.
        // TODO: Check parameters for validity.
        public static unsafe int WiFromRaw(
            byte[] raw, int width, int height, int bitsPerPixel, out byte[] encoded)
        {
            encoded = null;

            // Step 1:  initialize image compressor data structures

            WiRawImage* rawImage = WiCreateRawImage();
            WiCmpImage* cmpImage = WiCreateCmpImage();
            WiCmpOptions* cmpOpts = WiCreateCmpOptions();

            // Step 2:  get the image to compress

            IntPtr rawDataPtr = Marshal.AllocHGlobal(raw.Length);
            Marshal.Copy(raw, 0, rawDataPtr, raw.Length);

            rawImage->Raw = (byte*)rawDataPtr;
            rawImage->BitsPerPixel = bitsPerPixel;
            rawImage->Width = width;
            rawImage->Height = height;
            rawImage->Color = bitsPerPixel == 8 ? 0 : 1;

            // Step 3:  set compressor options

            // compression will be determined  by specifying quality
            cmpOpts->CmpControl = SwiInterop.CR_QUALITY;
            // compression quality is set at 50%
            cmpOpts->Quality = 0.50F;
            // E_NORMAL encoder is fast and gives high quality
            cmpOpts->Encoder = SwiInterop.E_NORMAL;
            // P_PATH3 path is usually good for high compression
            cmpOpts->EncodePath = SwiInterop.P_PATH3;
            cmpOpts->Progressive = 0;        // do not progressively decompress image
            cmpOpts->Magnification = 0;      // do not wavelet magnify image
            cmpOpts->EdgeEnhancement = 0;    // do not wavelet edge enhance image
            cmpOpts->ContrastEnhancement = 0;// do not wavelet contrast enhance image
            cmpOpts->FocusBoxes = null;      // do not use region of interest focusing

            // Step 4:  Compress image

            int errorCode = WiCompress(cmpOpts, rawImage, cmpImage);

            // Step 5: create output compressed image

            if (errorCode == 0)
            {
                encoded = new byte[cmpImage->Size];
                fixed (byte* pixelsPtr = encoded)
                {
                    for (int i = 0; i < encoded.Length; i++)
                    {
                        pixelsPtr[i] = cmpImage->CmpData[i];
                    }
                }
            }

            // Step 6:  free data structures internal "data" memory

            // SDK allocated cmpImage->CmpData
            WiFreeCmpImageData(cmpImage);
            // Programmer allocated image->Raw
            Marshal.FreeHGlobal(rawDataPtr);

            // Step 7:  free data structure memory

            WiDestroyRawImage(rawImage);
            WiDestroyCmpImage(cmpImage);
            WiDestroyCmpOptions(cmpOpts);

            return errorCode;
        }

        // TODO: Check parameters for validity.
        public static unsafe int WiToRaw(
            byte[] encoded, out byte[] raw, out int width, out int height, out int bitsPerPixel)
        {
            raw = null;
            width = 0;
            height = 0;
            bitsPerPixel = 0;

            // Step 1:  initialize image decompressor data structures

            WiRawImage* rawImage = WiCreateRawImage();
            WiCmpImage* cmpImage = WiCreateCmpImage();
            WiDecmpOptions* decmpOpts = WiCreateDecmpOptions();

            // Step 2:  get the compressed image to decompress

            IntPtr cmpDataPtr = Marshal.AllocHGlobal(encoded.Length);
            Marshal.Copy(encoded, 0, cmpDataPtr, encoded.Length);

            cmpImage->Size = encoded.Length;
            cmpImage->CmpData = (byte*)cmpDataPtr;

            // Step 3:  set decompressor options
            
            decmpOpts->Smoothing = 0;              // do not use wavelet smoothing
            decmpOpts->Fast = 0;                   // do not use fast decompression method
            decmpOpts->ReadNextByte = IntPtr.Zero; // do not use user defined bitstream I/0
            decmpOpts->ReadParam = null;           // do not use free variable
            decmpOpts->Sharpening = 1;             // wavelet sharpen image
            
            // Step 4:  decompress image

            int errorCode = SwiInterop.WiBeginDecompress(decmpOpts, rawImage, cmpImage);

            if (errorCode == 0)
            {
                errorCode = WiDecompressSubHeader(decmpOpts, rawImage, cmpImage);
                if (errorCode == 0)
                {
                    errorCode = WiDecompressSubImage(decmpOpts, rawImage, cmpImage);
                }
                WiEndDecompress(decmpOpts, rawImage, cmpImage);
            }

            // Step 5: create output image

            if (errorCode == 0)
            {
                if ((rawImage->Color == 0 && rawImage->BitsPerPixel == 8)
                    ||
                    (rawImage->Color == 1 && rawImage->BitsPerPixel == 24))
                {
                    width = rawImage->Width;
                    height = rawImage->Height;
                    bitsPerPixel = rawImage->BitsPerPixel;

                    int imageSize = width * height;
                    if (bitsPerPixel == 24)
                    {
                        imageSize *= 3;
                    }

                    raw = new byte[imageSize];
                    fixed (byte* pixelsPtr = raw)
                    {
                        for (int i = 0; i < raw.Length; i++)
                        {
                            pixelsPtr[i] = rawImage->Raw[i];
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException(
                        "Only 8 and 24 bits-per-pixel supported");
                }
            }

            // Step 6: free data structures internal "data" memory

            // SDK allocated image->Raw
            WiFreeRawImageData(rawImage);
            // Programmer allocated cmpImage->CmpData
            Marshal.FreeHGlobal(cmpDataPtr);

            // Step 7:  free data structure memory

            WiDestroyRawImage(rawImage);
            WiDestroyCmpImage(cmpImage);
            WiDestroyDecmpOptions(decmpOpts);

            return errorCode;
        }

        // TODO: Supports only 8; 24 bits-per-pixel images.
        // TODO: Check parameters for validity.
        public static unsafe int SiFromRaw(
            byte[] raw, int width, int height, int bitsPerPixel, out byte[] encoded)
        {
            encoded = null;

            IntPtr rawDataPtr = Marshal.AllocHGlobal(raw.Length);
            Marshal.Copy(raw, 0, rawDataPtr, raw.Length);

            SiImageInfo imageInfo = new SiImageInfo()
            {
                BitsPerPixel = bitsPerPixel,
                Color = bitsPerPixel == 8 ? 0 : 1,
                Height = height,
                Raw = (byte*)rawDataPtr,
                Width = width,
            };

            SiCmpOption cmpOption = new SiCmpOption()
            {
                Quality = 0.27F
            };

            SiCmpDataInfo cmpDataInfo = new SiCmpDataInfo();

            int errorCode = SiCompress(&imageInfo, &cmpOption, &cmpDataInfo);

            if (errorCode == 0)
            {
                encoded = new byte[cmpDataInfo.Size];
                fixed (byte* pixelsPtr = encoded)
                {
                    for (int i = 0; i < encoded.Length; i++)
                    {
                        pixelsPtr[i] = cmpDataInfo.CmpData[i];
                    }
                }
            }

            SiFreeCmpDataInfo(&cmpDataInfo);

            Marshal.FreeHGlobal(rawDataPtr);

            return errorCode;
        }

        // TODO: Check parameters for validity.
        public static unsafe int SiToRaw(
            byte[] encoded, out byte[] raw, out int width, out int height, out int bitsPerPixel)
        {
            raw = null;
            width = 0;
            height = 0;
            bitsPerPixel = 0;

            IntPtr cmpDataPtr = Marshal.AllocHGlobal(encoded.Length);
            Marshal.Copy(encoded, 0, cmpDataPtr, encoded.Length);

            SiCmpDataInfo cmpDataInfo = new SiCmpDataInfo()
            {
                CmpData = (byte*)cmpDataPtr,
                Size = encoded.Length
            };

            SiDecmpOption decmpOption = new SiDecmpOption()
            {
                Sharpening = 1,
            };

            SiImageInfo imageInfo = new SiImageInfo();

            int errorCode = SiDecompress(&cmpDataInfo, &decmpOption, &imageInfo);

            if (errorCode == 0)
            {
                if ((imageInfo.Color == 0 && imageInfo.BitsPerPixel == 8)
                    ||
                    (imageInfo.Color == 1 && imageInfo.BitsPerPixel == 24))
                {
                    width = imageInfo.Width;
                    height = imageInfo.Height;
                    bitsPerPixel = imageInfo.BitsPerPixel;

                    int imageSize = width * height;
                    if (bitsPerPixel == 24)
                    {
                        imageSize *= 3;
                    }

                    raw = new byte[imageSize];
                    fixed (byte* pixelsPtr = raw)
                    {
                        for (int i = 0; i < raw.Length; i++)
                        {
                            pixelsPtr[i] = imageInfo.Raw[i];
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException(
                        "Only 8 and 24 bits-per-pixel supported");
                }
            }

            SiFreeImageInfo(&imageInfo);
            Marshal.FreeHGlobal(cmpDataPtr);

            return errorCode;
        }
    }
}
