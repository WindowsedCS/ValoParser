using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse_Conversion.Textures.ASTC;
using CUE4Parse_Conversion.Textures.BC;
using CUE4Parse_Conversion.Textures.DXT;
using SkiaSharp;
using static CUE4Parse.Utils.TypeConversionUtils;

namespace CUE4Parse_Conversion.Textures
{
    public static class TextureDecoder
    {
        public static SKBitmap? Decode(this UTexture2D texture, ETexturePlatform platform = ETexturePlatform.DesktopMobile) => texture.Decode(texture.GetFirstMip(), platform);
        public static SKBitmap? Decode(this UTexture2D texture, FTexture2DMipMap? mip, ETexturePlatform platform = ETexturePlatform.DesktopMobile)
        {
            if (!texture.IsVirtual && mip != null)
            {
                DecodeTexture(mip, texture.Format, texture.isNormalMap, platform, out var data, out var colorType);

                var width = mip.SizeX;
                var height = mip.SizeY;
                var info = new SKImageInfo(width, height, colorType, SKAlphaType.Unpremul);
                var bitmap = new SKBitmap();

                unsafe
                {
                    var pixelsPtr = NativeMemory.Alloc((nuint) data.Length);
                    fixed (byte* p = data)
                    {
                        Unsafe.CopyBlockUnaligned(pixelsPtr, p, (uint) data.Length);
                    }

                    bitmap.InstallPixels(info, new IntPtr(pixelsPtr), info.RowBytes, (address, _) => NativeMemory.Free(address.ToPointer()));
                }

                if (!texture.bRenderNearestNeighbor)
                {
                    return bitmap;
                }

                var resized = bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.None);
                bitmap.Dispose();
                return resized;
            }

            return null;
        }

        public static void DecodeTexture(FTexture2DMipMap? mip, EPixelFormat format, bool isNormalMap, ETexturePlatform platform, out byte[] data, out SKColorType colorType)
        {
            switch (platform)
            {
                case ETexturePlatform.Playstation when mip != null:
                    PlaystationDecoder.DecodeTexturePlaystation(mip, format, isNormalMap, out data, out colorType);
                    break;
                case ETexturePlatform.NintendoSwitch when mip != null:
                    NintendoSwitchDecoder.DecodeTextureNSW(mip, format, isNormalMap, out data, out colorType);
                    break;
                default:
                    DecodeTexture(mip, format, isNormalMap, out data, out colorType);
                    break;
            }
        }

        private static void DecodeTexture(FTexture2DMipMap? mip, EPixelFormat format, bool isNormalMap, out byte[] data, out SKColorType colorType)
        {
            if (mip == null) throw new NotImplementedException($"Unknown pixel format: {format}");

            switch (format)
            {
                case EPixelFormat.PF_DXT1:
                    data = DXTDecoder.DXT1(mip.Data.Data, mip.SizeX, mip.SizeY, mip.SizeZ);
                    colorType = SKColorType.Rgba8888;
                    break;
                case EPixelFormat.PF_DXT5:
                    data = DXTDecoder.DXT5(mip.Data.Data, mip.SizeX, mip.SizeY, mip.SizeZ);
                    colorType = SKColorType.Rgba8888;
                    break;
                case EPixelFormat.PF_ASTC_4x4:
                case EPixelFormat.PF_ASTC_6x6:
                case EPixelFormat.PF_ASTC_8x8:
                case EPixelFormat.PF_ASTC_10x10:
                case EPixelFormat.PF_ASTC_12x12:
                    data = ASTCDecoder.RGBA8888(
                        mip.Data.Data,
                        FormatHelper.GetBlockWidth(format),
                        FormatHelper.GetBlockHeight(format),
                        FormatHelper.GetBlockDepth(format),
                        mip.SizeX, mip.SizeY, mip.SizeZ);
                    colorType = SKColorType.Rgba8888;

                    if (isNormalMap)
                    {
                        // UE4 drops blue channel for normal maps before encoding, restore it
                        unsafe
                        {
                            var offset = 0;
                            fixed (byte* d = data)
                            {
                                for (var i = 0; i < mip.SizeX * mip.SizeY; i++)
                                {
                                    d[offset + 2] = BCDecoder.GetZNormal(d[offset], d[offset + 1]);
                                    offset += 4;
                                }
                            }
                        }
                    }

                    break;
                case EPixelFormat.PF_BC4:
                    data = BCDecoder.BC4(mip.Data.Data, mip.SizeX, mip.SizeY);
                    colorType = SKColorType.Rgb888x;
                    break;
                case EPixelFormat.PF_BC5:
                    data = BCDecoder.BC5(mip.Data.Data, mip.SizeX, mip.SizeY);
                    colorType = SKColorType.Rgb888x;
                    break;
                case EPixelFormat.PF_BC6H:
                    // BC6H doesn't work no matter the pixel format, the closest we can get is either
                    // Rgb565 DETEX_PIXEL_FORMAT_FLOAT_RGBX16 or Rgb565 DETEX_PIXEL_FORMAT_FLOAT_BGRX16

                    data = Detex.DecodeDetexLinear(mip.Data.Data, mip.SizeX, mip.SizeY, true,
                        DetexTextureFormat.DETEX_TEXTURE_FORMAT_BPTC_FLOAT,
                        DetexPixelFormat.DETEX_PIXEL_FORMAT_FLOAT_RGBX16);
                    colorType = SKColorType.Rgb565;
                    break;
                case EPixelFormat.PF_BC7:
                    data = Detex.DecodeDetexLinear(mip.Data.Data, mip.SizeX, mip.SizeY, false,
                        DetexTextureFormat.DETEX_TEXTURE_FORMAT_BPTC,
                        DetexPixelFormat.DETEX_PIXEL_FORMAT_RGBA8);
                    colorType = SKColorType.Rgba8888;
                    break;
                case EPixelFormat.PF_ETC1:
                    data = Detex.DecodeDetexLinear(mip.Data.Data, mip.SizeX, mip.SizeY, false,
                        DetexTextureFormat.DETEX_TEXTURE_FORMAT_ETC1,
                        DetexPixelFormat.DETEX_PIXEL_FORMAT_RGBA8);
                    colorType = SKColorType.Rgba8888;
                    break;
                case EPixelFormat.PF_ETC2_RGB:
                    data = Detex.DecodeDetexLinear(mip.Data.Data, mip.SizeX, mip.SizeY, false,
                        DetexTextureFormat.DETEX_TEXTURE_FORMAT_ETC2,
                        DetexPixelFormat.DETEX_PIXEL_FORMAT_RGBA8);
                    colorType = SKColorType.Rgba8888;
                    break;
                case EPixelFormat.PF_ETC2_RGBA:
                    data = Detex.DecodeDetexLinear(mip.Data.Data, mip.SizeX, mip.SizeY, false,
                        DetexTextureFormat.DETEX_TEXTURE_FORMAT_ETC2_EAC,
                        DetexPixelFormat.DETEX_PIXEL_FORMAT_RGBA8);
                    colorType = SKColorType.Rgba8888;
                    break;
                case EPixelFormat.PF_R16F:
                case EPixelFormat.PF_R16F_FILTER:
                    unsafe
                    {
                        fixed (byte* d = mip.Data.Data)
                        {
                            data = ConvertRawR16DataToRGB888X(mip.SizeX, mip.SizeY, d, mip.SizeX * 2); // 2 BPP
                        }
                    }

                    colorType = SKColorType.Rgb888x;
                    break;
                case EPixelFormat.PF_B8G8R8A8:
                    data = mip.Data.Data;
                    colorType = SKColorType.Bgra8888;
                    break;
                case EPixelFormat.PF_G8:
                    data = mip.Data.Data;
                    colorType = SKColorType.Gray8;
                    break;
                case EPixelFormat.PF_FloatRGBA:
                    unsafe
                    {
                        fixed (byte* d = mip.Data.Data)
                        {
                            data = ConvertRawR16G16B16A16FDataToRGBA8888(mip.SizeX, mip.SizeY, d, mip.SizeX * 8, false); // 8 BPP
                        }
                    }

                    colorType = SKColorType.Rgba8888;
                    break;
                default: throw new NotImplementedException($"Unknown pixel format: {format}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe byte[] ConvertRawR16DataToRGB888X(int width, int height, byte* inp, int srcPitch)
        {
            // e.g. shadow maps
            var ret = new byte[width * height * 4];
            for (int y = 0; y < height; y++)
            {
                var srcPtr = (ushort*) (inp + y * srcPitch);
                var destPtr = y * width * 4;
                for (int x = 0; x < width; x++)
                {
                    var value16 = *srcPtr++;
                    var value = FColor.Requantize16to8(value16);

                    ret[destPtr++] = value;
                    ret[destPtr++] = value;
                    ret[destPtr++] = value;
                    ret[destPtr++] = 255;
                }
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe byte[] ConvertRawR16G16B16A16FDataToRGBA8888(int width, int height, byte* inp, int srcPitch, bool linearToGamma)
        {
            float minR = 0.0f, minG = 0.0f, minB = 0.0f, minA = 0.0f;
            float maxR = 1.0f, maxG = 1.0f, maxB = 1.0f, maxA = 1.0f;

            for (int y = 0; y < height; y++)
            {
                var srcPtr = (ushort*) (inp + y * srcPitch);

                for (int x = 0; x < width; x++)
                {
                    minR = MathF.Min(HalfToFloat(srcPtr[0]), minR);
                    minG = MathF.Min(HalfToFloat(srcPtr[1]), minG);
                    minB = MathF.Min(HalfToFloat(srcPtr[2]), minB);
                    minA = MathF.Min(HalfToFloat(srcPtr[3]), minA);
                    maxR = MathF.Max(HalfToFloat(srcPtr[0]), maxR);
                    maxG = MathF.Max(HalfToFloat(srcPtr[1]), maxG);
                    maxB = MathF.Max(HalfToFloat(srcPtr[2]), maxB);
                    maxA = MathF.Max(HalfToFloat(srcPtr[3]), maxA);
                    srcPtr += 4;
                }
            }

            var ret = new byte[width * height * 4];
            for (int y = 0; y < height; y++)
            {
                var srcPtr = (ushort*) (inp + y * srcPitch);
                var destPtr = y * width * 4;

                for (int x = 0; x < width; x++)
                {
                    var color = new FLinearColor(
                        (HalfToFloat(*srcPtr++) - minR) / (maxR - minR),
                        (HalfToFloat(*srcPtr++) - minG) / (maxG - minG),
                        (HalfToFloat(*srcPtr++) - minB) / (maxB - minB),
                        (HalfToFloat(*srcPtr++) - minA) / (maxA - minA)
                    ).ToFColor(linearToGamma);
                    ret[destPtr++] = color.R;
                    ret[destPtr++] = color.G;
                    ret[destPtr++] = color.B;
                    ret[destPtr++] = color.A;
                }
            }

            return ret;
        }
    }
}
