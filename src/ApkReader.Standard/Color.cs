using System;

namespace ApkReader
{
    //
    // Summary:
    //     Represents an ARGB (alpha, red, green, blue) color.
    public struct Color
    {
        //
        // Summary:
        //     Represents a color that is null.
        public static readonly Color Empty;
        //
        // Summary:
        //     Gets a system-defined color that has an ARGB value of #FF000000.
        //
        // Returns:
        //     A System.Drawing.Color representing a system-defined color.
        public static readonly Color Black = FromArgb(0, 0, 0);
        //
        // Summary:
        //     Gets a system-defined color that has an ARGB value of #FFFFFFFF.
        //
        // Returns:
        //     A System.Drawing.Color representing a system-defined color.
        public static readonly Color White = FromArgb(0xff, 0xff, 0xff);
        //
        // Summary:
        //     Gets a system-defined color.
        //
        // Returns:
        //     A System.Drawing.Color representing a system-defined color.
        public static readonly Color Transparent = FromArgb(0, 0, 0, 0);

        //
        // Summary:
        //     Gets the alpha component value of this System.Drawing.Color structure.
        //
        // Returns:
        //     The alpha component value of this System.Drawing.Color.
        public byte A { get; set; }
        //
        // Summary:
        //     Gets the red component value of this System.Drawing.Color structure.
        //
        // Returns:
        //     The red component value of this System.Drawing.Color.
        public byte R { get; set; }
        //
        // Summary:
        //     Gets the green component value of this System.Drawing.Color structure.
        //
        // Returns:
        //     The green component value of this System.Drawing.Color.
        public byte G { get; set; }
        //
        // Summary:
        //     Gets the blue component value of this System.Drawing.Color structure.
        //
        // Returns:
        //     The blue component value of this System.Drawing.Color.
        public byte B { get; set; }
        //
        // Summary:
        //     Specifies whether this System.Drawing.Color structure is uninitialized.
        //
        // Returns:
        //     This property returns true if this color is uninitialized; otherwise, false.
        public bool IsEmpty => Equals(Empty);

        //
        // Summary:
        //     Tests whether the specified object is a System.Drawing.Color structure and is
        //     equivalent to this System.Drawing.Color structure.
        //
        // Parameters:
        //   obj:
        //     The object to test.
        //
        // Returns:
        //     true if obj is a System.Drawing.Color structure equivalent to this System.Drawing.Color
        //     structure; otherwise, false.
        public override bool Equals(object obj)
        {
            if (obj is Color)
            {
                return Equals((Color) obj);
            }

            return false;
        }

        //
        // Summary:
        //     Tests whether the specified object is a System.Drawing.Color structure and is
        //     equivalent to this System.Drawing.Color structure.
        //
        // Parameters:
        //   obj:
        //     The object to test.
        //
        // Returns:
        //     true if obj is a System.Drawing.Color structure equivalent to this System.Drawing.Color
        //     structure; otherwise, false.
        public bool Equals(Color color)
        {
            return color.A == A &&
                   color.R == A &&
                   color.G == A &&
                   color.B == A;
        }

        //
        // Summary:
        //     Returns a hash code for this System.Drawing.Color structure.
        //
        // Returns:
        //     An integer value that specifies the hash code for this System.Drawing.Color.
        public override int GetHashCode()
        {
            return ShiftAndWrap(A.GetHashCode(), 6) ^ ShiftAndWrap(R.GetHashCode(), 4) ^
                   ShiftAndWrap(G.GetHashCode(), 2) ^ B.GetHashCode();
        }

        //
        // Summary:
        //     Tests whether two specified System.Drawing.Color structures are equivalent.
        //
        // Parameters:
        //   left:
        //     The System.Drawing.Color that is to the left of the equality operator.
        //
        //   right:
        //     The System.Drawing.Color that is to the right of the equality operator.
        //
        // Returns:
        //     true if the two System.Drawing.Color structures are equal; otherwise, false.
        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        //
        // Summary:
        //     Tests whether two specified System.Drawing.Color structures are different.
        //
        // Parameters:
        //   left:
        //     The System.Drawing.Color that is to the left of the inequality operator.
        //
        //   right:
        //     The System.Drawing.Color that is to the right of the inequality operator.
        //
        // Returns:
        //     true if the two System.Drawing.Color structures are different; otherwise, false.
        public static bool operator !=(Color left, Color right)
        {
            return false == left.Equals(right);
        }

        //
        // Summary:
        //     Gets the 32-bit ARGB value of this System.Drawing.Color structure.
        //
        // Returns:
        //     The 32-bit ARGB value of this System.Drawing.Color.
        public int ToArgb()
        {
            uint value;

            if (BitConverter.IsLittleEndian)
            {
                value = (uint) (B << 24) | (uint) (G << 16) | (uint) (R << 8) | A;
            }
            else
            {
                value = (uint) (A << 24) | (uint) (R << 16) | (uint) (G << 8) | B;
            }

            return (int) value;
        }

        //
        // Summary:
        //     Creates a System.Drawing.Color structure from a 32-bit ARGB value.
        //
        // Parameters:
        //   argb:
        //     A value specifying the 32-bit ARGB value.
        //
        // Returns:
        //     The System.Drawing.Color structure that this method creates.
        public static Color FromArgb(int value)
        {
            var uintValue = (uint) value;

            byte a;
            byte r;
            byte g;
            byte b;

            if (BitConverter.IsLittleEndian)
            {
                b = (byte) (uintValue >> 24);
                g = (byte) (uintValue >> 16);
                r = (byte) (uintValue >> 8);
                a = (byte) uintValue;
            }
            else
            {
                a = (byte) (uintValue >> 24);
                r = (byte) (uintValue >> 16);
                g = (byte) (uintValue >> 8);
                b = (byte) uintValue;
            }

            return FromArgb(a, r, g, b);
        }

        //
        // Summary:
        //     Creates a System.Drawing.Color structure from the specified 8-bit color values
        //     (red, green, and blue). The alpha value is implicitly 255 (fully opaque). Although
        //     this method allows a 32-bit value to be passed for each color component, the
        //     value of each component is limited to 8 bits.
        //
        // Parameters:
        //   red:
        //     The red component value for the new System.Drawing.Color. Valid values are 0
        //     through 255.
        //
        //   green:
        //     The green component value for the new System.Drawing.Color. Valid values are
        //     0 through 255.
        //
        //   blue:
        //     The blue component value for the new System.Drawing.Color. Valid values are 0
        //     through 255.
        //
        // Returns:
        //     The System.Drawing.Color that this method creates.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     red, green, or blue is less than 0 or greater than 255.
        public static Color FromArgb(int red, int green, int blue)
        {
            return FromArgb(0xff, red, green, blue);
        }

        //
        // Summary:
        //     Creates a System.Drawing.Color structure from the four ARGB component (alpha,
        //     red, green, and blue) values. Although this method allows a 32-bit value to be
        //     passed for each component, the value of each component is limited to 8 bits.
        //
        // Parameters:
        //   alpha:
        //     The alpha component. Valid values are 0 through 255.
        //
        //   red:
        //     The red component. Valid values are 0 through 255.
        //
        //   green:
        //     The green component. Valid values are 0 through 255.
        //
        //   blue:
        //     The blue component. Valid values are 0 through 255.
        //
        // Returns:
        //     The System.Drawing.Color that this method creates.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     alpha, red, green, or blue is less than 0 or greater than 255.
        public static Color FromArgb(int alpha, int red, int green, int blue)
        {
            var color = new Color
            {
                A = ConvertToByte(alpha),
                R = ConvertToByte(red),
                G = ConvertToByte(green),
                B = ConvertToByte(blue)
            };


            return color;
        }

        private static byte ConvertToByte(int value)
        {
            if (value < byte.MinValue || value > byte.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return (byte) value;
        }

        private static int ShiftAndWrap(int value, int positions)
        {
            // NOTE: this is from here: https://msdn.microsoft.com/en-us/library/system.object.gethashcode(v=vs.110).aspx

            positions = positions & 0x1F;

            // Save the existing bit pattern, but interpret it as an unsigned integer.
            var number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            // Preserve the bits to be discarded.
            var wrapped = number >> (32 - positions);
            // Shift and wrap the discarded bits.
            return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);
        }
    }
}