# Swi - Summus Wavelet Image C# codec and applications
### Introduction
This project is a further response to the work done to decode the image encrypted within the South African Driver's License (SADL) PDF417 barcode, documented here:

* <a href="https://github.com/the-mars-rover/rsa_identification/issues/2" target="_blank">Decode Drivers License Image Data #2</a>

The project currently does **NOT** correctly decode the SADL WI image, but at least provides a solid and easily configurable framework for further experimentation.
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
* Only 32-bit (x86) assemblies are implemented, as the Summus DLL is 32-bits (Win32). Our goal is to (hopefully) provide the functionality as 64-bit applications on both Windows, Linux, and Android.
* This build targets .NET Framework 4.6.1, as this is the minimum requirement for the included packages for the applications (see below). The code can be easily re-compiled for .NET, .NET Core, or .NET Standard.
### Requirements
* Visual Studio 2022+, Community, Pro, or Enterprise. Not tested, but should also work with Visual Studio Code and Visual Studio 2017, 2019, etc.
* <a href="https://learn.microsoft.com/en-us/dotnet/standard/commandline/" target="_blank">System.CommandLine</a> package available on <a href="https://www.nuget.org/packages/System.CommandLine" target="_blank">Nuget</a> - used for clean design and superior functionality when building console application with complex command line parameters.
* <a href="https://github.com/SixLabors/ImageSharp" target="_blank">SixLabors.ImageSharp</a> package available on <a href="https://www.nuget.org/packages/SixLabors.ImageSharp" target="_blank">Nuget</a> - provides a set of API's to easily convert image formats and has no dependencies on the image codec's available on .NET (does not require any reference to `System.Drawing`).
### Observations (so far!)
* **The codec works for encoding followed by decoding known images.**
* The codec does not appear to work when decoding the SADL WI image. In this case the following is observed while doing rudimentary testing:
  * When using the default `WiDecmpOptions` settings (as per code), the method `WiDecompressSubImage` return error code '10'. If the error is ignored, the decoded image is displayable as large black and white smudges.
  * When an image is size-reduced by 50%, and the `WiCmpOptions.Magnification` option is set to 1 (instead of 0), the resulting decoded image is magnified by 100% i.e. if the source image has dimensions 100x125 pixels (WxH), then setting the `WiDecmpOptions.Magnification` to 1 results in decoded image dimensions 200x250 (WxH), which is the size of the SADL image. Doing this results in smaller encoded image sizes when the image is hugely compressed but with about the same decoded image quality (using visual judgment). Sadly, this does not seem to be the answer as decoding the SADL WI image with this setting results in error code '3', and the `WiRawImage.Raw` buffer is `null`.
### Any ideas?
* Please feel free to use the code of this project as you like - there are no licensing requirements, etc.
* If we are missing something simple, please give us a *`kick-up-the-proverbial`* and help us with the solution.


  



