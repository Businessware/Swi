using System;
using System.Runtime.InteropServices;

namespace Swi32.Interop
{
    public static partial class SwiInterop
    {
        // Progressive
        public const int PR_NONE = 0;
        public const int PR_NORMAL = 1;
        public const int PR_FAST = 2;

        // Encoders
        public const int E_SLOW = 0;
        public const int E_NORMAL = 1;
        public const int E_FAST = 2;
        public const int E_FASTEST = 3;

        // EncodePaths
        public const int P_PATH1 = 0;
        public const int P_PATH2 = 1;
        public const int P_PATH3 = 2;

        // Compression Ratio Specifications
        public const int CR_QUALITY = 0;
        public const int CR_CLOSEST = 1;
        public const int CR_FLOOR = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct WiBox
        {
            /// int
            public int Left;
            /// int
            public int Top;
            /// int
            public int Right;
            /// int
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WiImageBlock
        {
            /// int
            public int Page;
            /// int
            public int PageLeft;
            /// int
            public int PageTop;
            /// int
            public int PageWidth;
            /// int
            public int PageHeight;
            /// int
            public int Block;
            /// int
            public int BlockLeft;
            /// int
            public int BlockTop;
            /// int
            public int BlockWidth;
            /// int
            public int BlockHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct WiRawImage
        {
            /// unsigned char*
            public byte* Raw;
            /// int
            public int Height;
            /// int
            public int Width;
            /// int
            public int BitsPerPixel;
            /// int
            public int Color;
            /// int
            public int LevelHeight;
            /// int
            public int LevelWidth;
            /// void*
            public void* AppData;
            /// unsigned char*
            public byte* Comment;
            /// int
            public int CommentLength;
            /// unsigned char*
            public byte* AppExtension;
            /// int
            public int AppExtensionLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct WiCmpImage
        {
            /// unsigned char*
            public byte* CmpData;
            /// int
            public int Size;
        }

        public unsafe delegate int ByteIOFunc(void* param0, int* param1);

        public unsafe delegate int ScanlineIOFunc(void* param0, WiImageBlock* param1, int param2, byte** param3);

        public unsafe delegate int ExtensionIOFunc(void* param0, int param1, byte** param2, int* param3);

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct WiCmpOptions
        {
            /// float
            public float Quality;
            /// float
            public float CmpRatio;
            /// int
            public int CmpControl;
            /// int
            public int Encoder;
            /// int
            public int EncodePath;
            /// int
            public int Progressive;
            /// ByteIOFunc
            public IntPtr WriteNextByte;
            /// void*
            public void* WriteParam;
            /// int
            public int Magnification;
            /// int
            public int EdgeEnhancement;
            /// int
            public int ContrastEnhancement;
            /// int
            public int FocusWeight;
            /// WiBox*
            public WiBox* FocusBoxes;
            /// int
            public int nBoxes;
            /// int
            public int HighColorQuality;
            /// ScanlineIOFunc
            public IntPtr ReadScanline;
            /// void*
            public void* ReadScanlineParam;
            /// int
            public int BlockSize;
            /// WiBox*
            public IntPtr Blocks;
            /// int
            public int nBlocks;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct WiDecmpOptions
        {
            /// int
            public int Smoothing;
            /// int
            public int Fast;
            /// ByteIOFunc
            public IntPtr ReadNextByte;
            /// void*
            public void* ReadParam;
            /// int
            public int Sharpening;
            /// ScanlineIOFunc
            public IntPtr WriteScanline;
            /// void*
            public void* WriteScanlineParam;
            /// WiImageBlock
            public WiImageBlock SubImage;
            /// int
            public int Magnification;
            /// ExtensionIOFunc
            public IntPtr WriteAppExtension;
            /// void*
            public void* WriteAppExtensionParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SiBox
        {
            /// int
            public int left;
            /// int
            public int top;
            /// int
            public int right;
            /// int
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct SiImageInfo
        {
            /// unsigned char*
            public byte* Raw;
            /// int
            public int Height;
            /// int
            public int Width;
            /// int
            public int BitsPerPixel;
            /// int
            public int Color;
            /// int
            public int LevelHeight;
            /// int
            public int LevelWidth;
            /// void*
            public void* AppData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct SiCmpDataInfo
        {
            /// unsigned char*
            public byte* CmpData;
            /// int
            public int Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct SiCmpOption
        {
            /// float
            public float Quality;
            /// float
            public float CmpRatio;
            /// int
            public int AutoRatio;
            /// int
            public int Encoder;
            /// int
            public int EncodePath;
            /// int
            public int Progressive;
            /// ByteIOFunc
            public IntPtr WriteNextByte;
            /// void*
            public void* WriteParam;
            /// int
            public int Magnification;
            /// int
            public int EdgeEnhancement;
            /// int
            public int ContrastEnhancement;
            /// int
            public int FocusWeight;
            /// SiBox*
            public SiBox* FocusBoxes;
            /// int
            public int nBoxes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct SiDecmpOption
        {
            /// int
            public int Smoothing;
            /// int
            public int Fast;
            /// ByteIOFunc
            public IntPtr ReadNextByte;
            /// void*
            public void* ReadParam;
            /// int
            public int Sharpening;
            /// ScanlineIOFunc
            public IntPtr WriteScanline;
            /// void*
            public void* WriteScanlineParam;
        }

        [DllImport("swi32.dll", EntryPoint = "WiCreateRawImage", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe WiRawImage* WiCreateRawImage();

        [DllImport("swi32.dll", EntryPoint = "WiFreeRawImageData", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void WiFreeRawImageData(WiRawImage* rawImage);

        [DllImport("swi32.dll", EntryPoint = "WiDestroyRawImage", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void WiDestroyRawImage(WiRawImage* rawImage);

        [DllImport("swi32.dll", EntryPoint = "WiCreateCmpImage", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe WiCmpImage* WiCreateCmpImage();

        [DllImport("swi32.dll", EntryPoint = "WiFreeCmpImageData", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void WiFreeCmpImageData(WiCmpImage* cmpImage);

        [DllImport("swi32.dll", EntryPoint = "WiDestroyCmpImage", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void WiDestroyCmpImage(WiCmpImage* cmpImage);

        [DllImport("swi32.dll", EntryPoint = "WiCreateCmpOptions", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe WiCmpOptions* WiCreateCmpOptions();

        [DllImport("swi32.dll", EntryPoint = "WiDestroyCmpOptions", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void WiDestroyCmpOptions(WiCmpOptions* cmpOptions);

        [DllImport("swi32.dll", EntryPoint = "WiCompress", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int WiCompress(WiCmpOptions* decmpOptions, WiRawImage* rawImage, WiCmpImage* cmpImage);

        [DllImport("swi32.dll", EntryPoint = "WiCreateDecmpOptions", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe WiDecmpOptions* WiCreateDecmpOptions();

        [DllImport("swi32.dll", EntryPoint = "WiDestroyDecmpOptions", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void WiDestroyDecmpOptions(WiDecmpOptions* decmpOptions);

        [DllImport("swi32.dll", EntryPoint = "WiBeginDecompress", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int WiBeginDecompress(WiDecmpOptions* decmpOptions, WiRawImage* rawImage, WiCmpImage* cmpImage);

        [DllImport("swi32.dll", EntryPoint = "WiDecompressSubHeader", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int WiDecompressSubHeader(WiDecmpOptions* decmpOptions, WiRawImage* rawImage, WiCmpImage* cmpImage);

        [DllImport("swi32.dll", EntryPoint = "WiDecompressSubImage", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int WiDecompressSubImage(WiDecmpOptions* decmpOptions, WiRawImage* rawImage, WiCmpImage* cmpImage);

        [DllImport("swi32.dll", EntryPoint = "WiEndDecompress", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void WiEndDecompress(WiDecmpOptions* decmpOptions, WiRawImage* rawImage, WiCmpImage* cmpImage);

        // Obsolete functions

        [DllImport("swi32.dll", EntryPoint = "SiCompress", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int SiCompress(SiImageInfo* imageInfo, SiCmpOption* cmpOption, SiCmpDataInfo* cmpDataInfo);

        [DllImport("swi32.dll", EntryPoint = "SiDecompress", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int SiDecompress(SiCmpDataInfo* cmpDataInfo, SiDecmpOption* decmpOption, SiImageInfo* imageInfo);

        [DllImport("swi32.dll", EntryPoint = "SiGetImageInfo", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe int SiGetImageInfo(SiImageInfo* imageInfo);

        [DllImport("swi32.dll", EntryPoint = "SiFreeImageInfo", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void SiFreeImageInfo(SiImageInfo* imageInfo);

        [DllImport("swi32.dll", EntryPoint = "SiFreeCmpDataInfo", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void SiFreeCmpDataInfo(SiCmpDataInfo* cmpDataInfo);
    }
}