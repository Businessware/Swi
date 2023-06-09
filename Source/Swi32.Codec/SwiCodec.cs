using System;
using System.Runtime.InteropServices;

using Swi32.Interop;

namespace Swi32.Codec
{
    public static class SwiCodec
    {
        // TODO: Supports only 8; 24 bits-per-pixel images.
        // TODO: Check parameters for validity.
        public static unsafe int FromRaw(
            byte[] raw, int width, int height, int bitsPerPixel, out byte[] encoded)
        {
            encoded = null;

            // Step 1:  initialize image compressor data structures

            SwiInterop.WiRawImage* rawImage = SwiInterop.WiCreateRawImage();
            SwiInterop.WiCmpImage* cmpImage = SwiInterop.WiCreateCmpImage();
            SwiInterop.WiCmpOptions* cmpOpts = SwiInterop.WiCreateCmpOptions();

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

            int errorCode = SwiInterop.WiCompress(cmpOpts, rawImage, cmpImage);

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
            SwiInterop.WiFreeCmpImageData(cmpImage);
            // Programmer allocated image->Raw
            Marshal.FreeHGlobal(rawDataPtr);

            // Step 7:  free data structure memory

            SwiInterop.WiDestroyRawImage(rawImage);
            SwiInterop.WiDestroyCmpImage(cmpImage);
            SwiInterop.WiDestroyCmpOptions(cmpOpts);

            return errorCode;
        }

        // TODO: Check parameters for validity.
        public static unsafe int ToRaw(
            byte[] encoded, out byte[] raw, out int width, out int height, out int bitsPerPixel)
        {
            raw = null;
            width = 0;
            height = 0;
            bitsPerPixel = 0;

            // Step 1:  initialize image decompressor data structures

            SwiInterop.WiRawImage* rawImage = SwiInterop.WiCreateRawImage();
            SwiInterop.WiCmpImage* cmpImage = SwiInterop.WiCreateCmpImage();
            SwiInterop.WiDecmpOptions* decmpOpts = SwiInterop.WiCreateDecmpOptions();

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
                errorCode = SwiInterop.WiDecompressSubHeader(decmpOpts, rawImage, cmpImage);
                if (errorCode == 0)
                {
                    errorCode = SwiInterop.WiDecompressSubImage(decmpOpts, rawImage, cmpImage);
                }
                SwiInterop.WiEndDecompress(decmpOpts, rawImage, cmpImage);
            }

            // Step 5: create output image

            errorCode = 0;

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
            SwiInterop.WiFreeRawImageData(rawImage);
            // Programmer allocated cmpImage->CmpData
            Marshal.FreeHGlobal(cmpDataPtr);

            // Step 7:  free data structure memory

            SwiInterop.WiDestroyRawImage(rawImage);
            SwiInterop.WiDestroyCmpImage(cmpImage);
            SwiInterop.WiDestroyDecmpOptions(decmpOpts);

            return errorCode;
        }
    }
}
