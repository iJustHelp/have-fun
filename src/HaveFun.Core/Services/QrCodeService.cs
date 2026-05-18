using System.Globalization;
using System.Text;

namespace HaveFun.Core;

public sealed class QrCodeService : IQrCodeService
{
    private const int Version = 3;
    private const int Size = Version * 4 + 17;
    private const int DataCodewordCount = 55;
    private const int ErrorCorrectionCodewordCount = 15;

    public string CreateSvgDataUri(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("QR code text is required.", nameof(text));
        }

        var modules = Encode(text);
        var svg = CreateSvg(modules);
        var bytes = Encoding.UTF8.GetBytes(svg);

        return $"data:image/svg+xml;base64,{Convert.ToBase64String(bytes)}";
    }

    private static bool[,] Encode(string text)
    {
        var dataCodewords = CreateDataCodewords(text);
        var errorCorrectionCodewords = CreateErrorCorrectionCodewords(dataCodewords, ErrorCorrectionCodewordCount);
        var allCodewords = dataCodewords.Concat(errorCorrectionCodewords).ToArray();
        var modules = new bool[Size, Size];
        var reserved = new bool[Size, Size];

        DrawFunctionPatterns(modules, reserved);
        DrawCodewords(modules, reserved, allCodewords);
        ApplyMask(modules, reserved);
        DrawFormatBits(modules);

        return modules;
    }

    private static byte[] CreateDataCodewords(string text)
    {
        var textBytes = Encoding.UTF8.GetBytes(text);

        if (textBytes.Length > 53)
        {
            throw new InvalidOperationException("The shared URL is too long for the built-in QR code size.");
        }

        var bits = new List<bool>();
        AppendBits(bits, 0b0100, 4);
        AppendBits(bits, textBytes.Length, 8);

        foreach (var textByte in textBytes)
        {
            AppendBits(bits, textByte, 8);
        }

        var remainingBits = DataCodewordCount * 8 - bits.Count;
        AppendBits(bits, 0, Math.Min(4, remainingBits));

        while (bits.Count % 8 != 0)
        {
            bits.Add(false);
        }

        var codewords = new List<byte>();

        for (var index = 0; index < bits.Count; index += 8)
        {
            codewords.Add((byte)GetBitsValue(bits, index, 8));
        }

        for (var padByte = 0xec; codewords.Count < DataCodewordCount; padByte ^= 0xec ^ 0x11)
        {
            codewords.Add((byte)padByte);
        }

        return codewords.ToArray();
    }

    private static byte[] CreateErrorCorrectionCodewords(byte[] dataCodewords, int degree)
    {
        var generator = CreateGeneratorPolynomial(degree);
        var remainder = new byte[degree];

        foreach (var dataCodeword in dataCodewords)
        {
            var factor = dataCodeword ^ remainder[0];
            Array.Copy(remainder, 1, remainder, 0, degree - 1);
            remainder[^1] = 0;

            for (var index = 0; index < degree; index++)
            {
                remainder[index] ^= Multiply(generator[index], factor);
            }
        }

        return remainder;
    }

    private static byte[] CreateGeneratorPolynomial(int degree)
    {
        var result = new byte[] { 1 };

        for (var index = 0; index < degree; index++)
        {
            var next = new byte[result.Length + 1];

            for (var coefficientIndex = 0; coefficientIndex < result.Length; coefficientIndex++)
            {
                next[coefficientIndex] ^= Multiply(result[coefficientIndex], 1);
                next[coefficientIndex + 1] ^= Multiply(result[coefficientIndex], Power(2, index));
            }

            result = next;
        }

        return result.Skip(1).ToArray();
    }

    private static void DrawFunctionPatterns(bool[,] modules, bool[,] reserved)
    {
        DrawFinderPattern(modules, reserved, 3, 3);
        DrawFinderPattern(modules, reserved, Size - 4, 3);
        DrawFinderPattern(modules, reserved, 3, Size - 4);
        DrawAlignmentPattern(modules, reserved, 22, 22);

        for (var index = 0; index < Size; index++)
        {
            if (!reserved[6, index])
            {
                SetFunctionModule(modules, reserved, 6, index, index % 2 == 0);
            }

            if (!reserved[index, 6])
            {
                SetFunctionModule(modules, reserved, index, 6, index % 2 == 0);
            }
        }

        SetFunctionModule(modules, reserved, 8, Size - 8, true);

        for (var index = 0; index < 9; index++)
        {
            if (index != 6)
            {
                ReserveModule(reserved, 8, index);
                ReserveModule(reserved, index, 8);
            }
        }

        for (var index = Size - 8; index < Size; index++)
        {
            ReserveModule(reserved, 8, index);
            ReserveModule(reserved, index, 8);
        }
    }

    private static void DrawFinderPattern(bool[,] modules, bool[,] reserved, int centerX, int centerY)
    {
        for (var y = -4; y <= 4; y++)
        {
            for (var x = -4; x <= 4; x++)
            {
                var distance = Math.Max(Math.Abs(x), Math.Abs(y));
                var moduleX = centerX + x;
                var moduleY = centerY + y;

                if (moduleX < 0 || moduleY < 0 || moduleX >= Size || moduleY >= Size)
                {
                    continue;
                }

                SetFunctionModule(modules, reserved, moduleX, moduleY, distance is not 2 and not 4);
            }
        }
    }

    private static void DrawAlignmentPattern(bool[,] modules, bool[,] reserved, int centerX, int centerY)
    {
        for (var y = -2; y <= 2; y++)
        {
            for (var x = -2; x <= 2; x++)
            {
                var distance = Math.Max(Math.Abs(x), Math.Abs(y));
                SetFunctionModule(modules, reserved, centerX + x, centerY + y, distance != 1);
            }
        }
    }

    private static void DrawCodewords(bool[,] modules, bool[,] reserved, byte[] codewords)
    {
        var bitIndex = 0;
        var upward = true;

        for (var right = Size - 1; right >= 1; right -= 2)
        {
            if (right == 6)
            {
                right--;
            }

            for (var vertical = 0; vertical < Size; vertical++)
            {
                var y = upward ? Size - 1 - vertical : vertical;

                for (var column = 0; column < 2; column++)
                {
                    var x = right - column;

                    if (reserved[x, y])
                    {
                        continue;
                    }

                    var dark = bitIndex < codewords.Length * 8 &&
                        ((codewords[bitIndex / 8] >> (7 - bitIndex % 8)) & 1) != 0;
                    modules[x, y] = dark;
                    bitIndex++;
                }
            }

            upward = !upward;
        }
    }

    private static void ApplyMask(bool[,] modules, bool[,] reserved)
    {
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                if (!reserved[x, y] && (x + y) % 2 == 0)
                {
                    modules[x, y] = !modules[x, y];
                }
            }
        }
    }

    private static void DrawFormatBits(bool[,] modules)
    {
        const int formatBits = 0x77c4;

        for (var index = 0; index <= 5; index++)
        {
            modules[8, index] = GetBit(formatBits, index);
        }

        modules[8, 7] = GetBit(formatBits, 6);
        modules[8, 8] = GetBit(formatBits, 7);
        modules[7, 8] = GetBit(formatBits, 8);

        for (var index = 9; index < 15; index++)
        {
            modules[14 - index, 8] = GetBit(formatBits, index);
        }

        for (var index = 0; index < 8; index++)
        {
            modules[Size - 1 - index, 8] = GetBit(formatBits, index);
        }

        for (var index = 8; index < 15; index++)
        {
            modules[8, Size - 15 + index] = GetBit(formatBits, index);
        }
    }

    private static string CreateSvg(bool[,] modules)
    {
        const int border = 4;
        var viewBoxSize = Size + border * 2;
        var builder = new StringBuilder();

        builder.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {viewBoxSize} {viewBoxSize}\" shape-rendering=\"crispEdges\">");
        builder.Append("<path fill=\"#fff\" d=\"M0 0h");
        builder.Append(viewBoxSize);
        builder.Append('v');
        builder.Append(viewBoxSize);
        builder.Append("H0z\"/>");
        builder.Append("<path fill=\"#111827\" d=\"");

        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                if (modules[x, y])
                {
                    builder.Append('M');
                    builder.Append(x + border);
                    builder.Append(' ');
                    builder.Append(y + border);
                    builder.Append("h1v1H");
                    builder.Append(x + border);
                    builder.Append('z');
                }
            }
        }

        builder.Append("\"/></svg>");

        return builder.ToString();
    }

    private static void SetFunctionModule(bool[,] modules, bool[,] reserved, int x, int y, bool dark)
    {
        modules[x, y] = dark;
        reserved[x, y] = true;
    }

    private static void ReserveModule(bool[,] reserved, int x, int y)
    {
        reserved[x, y] = true;
    }

    private static void AppendBits(List<bool> bits, int value, int length)
    {
        for (var index = length - 1; index >= 0; index--)
        {
            bits.Add(((value >> index) & 1) != 0);
        }
    }

    private static int GetBitsValue(IReadOnlyList<bool> bits, int startIndex, int length)
    {
        var value = 0;

        for (var index = 0; index < length; index++)
        {
            value = value << 1 | (bits[startIndex + index] ? 1 : 0);
        }

        return value;
    }

    private static bool GetBit(int value, int index)
    {
        return ((value >> index) & 1) != 0;
    }

    private static byte Power(byte value, int power)
    {
        var result = (byte)1;

        for (var index = 0; index < power; index++)
        {
            result = Multiply(result, value);
        }

        return result;
    }

    private static byte Multiply(int left, int right)
    {
        var product = 0;

        for (var index = 7; index >= 0; index--)
        {
            product = product << 1 ^ (product >= 0x80 ? 0x11d : 0);

            if (((right >> index) & 1) != 0)
            {
                product ^= left;
            }
        }

        return (byte)product;
    }
}
