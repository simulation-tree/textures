using System;
using System.Numerics;

namespace Textures
{
    public struct Color
    {
        public Vector4 value;

        public byte R
        {
            readonly get => (byte)(value.X * byte.MaxValue);
            set => this.value.X = value / (float)byte.MaxValue;
        }

        public byte G
        {
            readonly get => (byte)(value.Y * byte.MaxValue);
            set => this.value.Y = value / (float)byte.MaxValue;
        }

        public byte B
        {
            readonly get => (byte)(value.Z * byte.MaxValue);
            set => this.value.Z = value / (float)byte.MaxValue;
        }

        public byte A
        {
            readonly get => (byte)(value.W * byte.MaxValue);
            set => this.value.W = value / (float)byte.MaxValue;
        }

        public float Hue
        {
            readonly get
            {
                byte r = R;
                byte g = G;
                byte b = B;
                byte min = Math.Min(r, Math.Min(g, b));
                byte max = Math.Max(r, Math.Max(g, b));
                int delta = max - min;

                if (delta < 0)
                {
                    return 0;
                }

                float hue;
                if (r == max)
                {
                    hue = (g - b) / (float)delta;
                }
                else if (g == max)
                {
                    hue = 2 + (b - r) / (float)delta;
                }
                else
                {
                    hue = 4 + (r - g) / (float)delta;
                }

                hue /= 6;
                if (hue < 0)
                {
                    hue += 1;
                }

                return hue;
            }
            set
            {
                float h = value * 6f;
                float s = Saturation;
                float v = Value;

                int i = (int)Math.Floor(h);
                float f = h - i;
                float p = v * (1 - s);
                float q = v * (1 - s * f);
                float t = v * (1 - s * (1 - f));
                float r = 0, g = 0, b = 0;

                switch (i)
                {
                    case 0:
                        r = v; g = t; b = p;
                        break;
                    case 1:
                        r = q; g = v; b = p;
                        break;
                    case 2:
                        r = p; g = v; b = t;
                        break;
                    case 3:
                        r = p; g = q; b = v;
                        break;
                    case 4:
                        r = t; g = p; b = v;
                        break;
                    default:
                        r = v; g = p; b = q;
                        break;
                }

                this.value = new Vector4(r, g, b, value);
            }
        }

        public float Saturation
        {
            readonly get
            {
                float rFloat = value.X;
                float gFloat = value.Y;
                float bFloat = value.Z;
                float max = Math.Max(rFloat, Math.Max(gFloat, bFloat));
                float min = Math.Min(rFloat, Math.Min(gFloat, bFloat));

                if (max <= 0)
                {
                    return 0;
                }

                return (max - min) / max;
            }
            set
            {
                float h = Hue;
                float s = value;
                float v = Value;

                int i = (int)(h * 6);
                float f = h * 6 - i;
                float p = v * (1 - s);
                float q = v * (1 - f * s);
                float t = v * (1 - (1 - f) * s);

                switch (i % 6)
                {
                    case 0: this.value.X = v; this.value.Y = t; this.value.Z = p; break;
                    case 1: this.value.X = q; this.value.Y = v; this.value.Z = p; break;
                    case 2: this.value.X = p; this.value.Y = v; this.value.Z = t; break;
                    case 3: this.value.X = p; this.value.Y = q; this.value.Z = v; break;
                    case 4: this.value.X = t; this.value.Y = p; this.value.Z = v; break;
                    case 5: this.value.X = v; this.value.Y = p; this.value.Z = q; break;
                }
            }
        }

        public float Value
        {
            readonly get
            {
                float rFloat = value.X;
                float gFloat = value.Y;
                float bFloat = value.Z;
                return Math.Max(rFloat, Math.Max(gFloat, bFloat));
            }
            set
            {
                float h = Hue;
                float s = Saturation;
                float v = value;

                int i = (int)(h * 6);
                float f = h * 6 - i;
                float p = v * (1 - s);
                float q = v * (1 - f * s);
                float t = v * (1 - (1 - f) * s);

                switch (i % 6)
                {
                    case 0: this.value.X = v; this.value.Y = t; this.value.Z = p; break;
                    case 1: this.value.X = q; this.value.Y = v; this.value.Z = p; break;
                    case 2: this.value.X = p; this.value.Y = v; this.value.Z = t; break;
                    case 3: this.value.X = p; this.value.Y = q; this.value.Z = v; break;
                    case 4: this.value.X = t; this.value.Y = p; this.value.Z = v; break;
                    case 5: this.value.X = v; this.value.Y = p; this.value.Z = q; break;
                }
            }
        }

        private Color(Vector4 value)
        {
            this.value = value;
        }

        public readonly override string ToString()
        {
            Span<char> buffer = stackalloc char[256];
            value.X.TryFormat(buffer, out int length);
            buffer[length++] = ',';
            value.Y.TryFormat(buffer[length..], out length);
            buffer[length++] = ',';
            value.Z.TryFormat(buffer[length..], out length);
            buffer[length++] = ',';
            value.W.TryFormat(buffer[length..], out length);
            return new string(buffer[..length]);
        }

        public readonly Pixel AsPixel()
        {
            return new Pixel(R, G, B, A);
        }

        public readonly Vector4 AsHSV()
        {
            float r = value.X;
            float g = value.Y;
            float b = value.Z;
            float a = value.W;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;
            float h = 0;
            float s = (max == 0) ? 0 : delta / max;
            float v = max;

            if (delta != 0)
            {
                if (r == max)
                {
                    h = (g - b) / delta + (g < b ? 6 : 0);
                }
                else if (g == max)
                {
                    h = (b - r) / delta + 2;
                }
                else
                {
                    h = (r - g) / delta + 4;
                }

                h /= 6;
            }

            return new Vector4(h, s, v, a);
        }

        public static Color CreateFromRGB(Vector4 rgba)
        {
            return new Color(rgba);
        }

        public static Color CreateFromRGB(Vector3 rgb, float alpha = 1f)
        {
            return new Color(new Vector4(rgb, alpha));
        }

        public static Color CreateFromRGB(float r, float g, float b, float a = 1f)
        {
            return new Color(new Vector4(r, g, b, a));
        }

        public static Color CreateFromHSV(Vector4 hsv)
        {
            Vector4 rgba = new(0, 0, 0, hsv.W);
            int i = (int)(hsv.X * 6);
            float f = hsv.X * 6 - i;
            float p = hsv.W * (1 - hsv.Y);
            float q = hsv.W * (1 - f * hsv.Y);
            float t = hsv.W * (1 - (1 - f) * hsv.Y);

            switch (i % 6)
            {
                case 0: rgba.X = hsv.W; rgba.Y = t; rgba.Z = p; break;
                case 1: rgba.X = q; rgba.Y = hsv.W; rgba.Z = p; break;
                case 2: rgba.X = p; rgba.Y = hsv.W; rgba.Z = t; break;
                case 3: rgba.X = p; rgba.Y = q; rgba.Z = hsv.W; break;
                case 4: rgba.X = t; rgba.Y = p; rgba.Z = hsv.W; break;
                case 5: rgba.X = hsv.W; rgba.Y = p; rgba.Z = q; break;
            }

            return new Color(rgba);
        }

        public static Color CreateFromHSV(Vector3 hsv, float alpha = 1f)
        {
            return CreateFromHSV(new Vector4(hsv, alpha));
        }

        public static Color CreateFromHSV(float h, float s, float v, float a = 1f)
        {
            return CreateFromHSV(new Vector4(h, s, v, a));
        }
    }
}
