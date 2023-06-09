# Swi - Summus Wavelet Image C# codec and applications
### Introduction
This project is a further response to the work done to decode the image encrypted within the South African Driver's License (SADL) PDF417 barcode, documented here:

* <a href="https://github.com/the-mars-rover/rsa_identification/issues/2" target="_blank">Decode Drivers License Image Data #2</a>

The project currently does not correctly decode the WI image, but at least provides a solid and easily configurable framework for further experimentation.

### Swi Project
The project is a Visual Studio solution with the following C# projects:

* **Swi32.Interop**: The C# interop wrapper code to directly access the Summus SWI32.dll methods and data.
* **Swi32.Codec**: The C# codec API which provides decoding and encoding methods via the interop API.
* **SwiEncode**: C# console application to convert a common standard image to the encoded WI format.
* **SwiDecode**: C# console application to convert the encoded WI format to a common standard image for display.
* The included 'Images' folder contains some sample images.

### Implementation
* The decoder and encoder applications provide for the conversion of WI images to-and-from standard images including BMP, PBM (PPM/PGM), PNG, and JPG.
* The implementation currently provides for 24-bit RGB, and 8-bit gray-scale images.
* Only 32-bit (x86) assemblies are implemented, as the Summus DLL is 32-bits (Win32). Our goal is to provide the functionality as 64-bit applications on both Windows, Linux, and Android.
* This build targets .NET Framework 4.6.1, as this is the minimum requirement for the included packages for the applications (see below). The code can be easily re-compiled for .NET, .NET Core, or .NET Standard.

### Requirements
* Visual Studio 2022+, Community, Pro, or Enterprise. Not tested, but should also work with Visual Studio Code and Visual Studio 2017, 2019, etc.
* <a href="https://learn.microsoft.com/en-us/dotnet/standard/commandline/" target="_blank">System.CommandLine</a> package available on <a href="https://www.nuget.org/packages/System.CommandLine" target="_blank">Nuget</a> - used for clean design and superior functionality when building console application with complex command line parameters.
* <a href="https://github.com/SixLabors/ImageSharp" target="_blank">SixLabors.ImageSharp</a> package available on <a href="https://www.nuget.org/packages/SixLabors.ImageSharp" target="_blank">Nuget</a> - provides a set of API's to easily convert image formats and has no dependencies on the image codecs available on .NET (does not require any reference to `System.Drawing`).

### Observations (so far!)
* The codec works for encoding/decoding known images.
* The codec does not appear to work when decoding the SADL embedded WI image. In this case the following is observed:
*    The method WiDecompressSubImage 



