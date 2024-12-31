using System;
using System.Diagnostics;
using System.Numerics;

namespace Textures
{
    public static class Vector4Extensions
    {
        public static Vector4 FromHSV(this Vector3 hsv, float alpha = 1)
        {
            float hue = hsv.X;
            float saturation = hsv.Y;
            float value = hsv.Z;
            ThrowIfOutOfRange(hue);

            byte pie = (byte)(hue * 6);
            float f = hue * 6 - pie;
            float p = value * (1 - saturation);
            float q = value * (1 - f * saturation);
            float t = value * (1 - (1 - f) * saturation);
            return pie switch
            {
                0 => new(value, t, p, alpha),
                1 => new(q, value, p, alpha),
                2 => new(p, value, t, alpha),
                3 => new(p, q, value, alpha),
                4 => new(t, p, value, alpha),
                5 => new(value, p, q, alpha),
                _ => default
            };
        }

        public static Vector4 FromHSV(this Vector4 hsva)
        {
            Vector3 hsv;
            hsv.X = hsva.X;
            hsv.Y = hsva.Y;
            hsv.Z = hsva.Z;
            float alpha = hsva.W;
            return FromHSV(hsv, alpha);
        }

        public static Vector4 ToHSV(this Vector4 color)
        {
            float r = color.X;
            float g = color.Y;
            float b = color.Z;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;
            float hue = 0f;
            if (delta != 0)
            {
                if (max == r)
                {
                    hue = (g - b) / delta;
                }
                else if (max == g)
                {
                    hue = 2 + (b - r) / delta;
                }
                else
                {
                    hue = 4 + (r - g) / delta;
                }
                hue *= 60;
                if (hue < 0)
                {
                    hue += 360;
                }
            }

            hue /= 360;
            float saturation = max == 0 ? 0 : delta / max;
            float value = max;
            float alpha = color.W;
            return new(hue, saturation, value, alpha);
        }

        public static float GetHue(this Vector4 color)
        {
            float r = color.X;
            float g = color.Y;
            float b = color.Z;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;
            float hue = 0f;
            if (delta != 0)
            {
                if (max == r)
                {
                    hue = (g - b) / delta;
                }
                else if (max == g)
                {
                    hue = 2 + (b - r) / delta;
                }
                else
                {
                    hue = 4 + (r - g) / delta;
                }
                hue *= 60;
                if (hue < 0)
                {
                    hue += 360;
                }
            }

            return hue / 360;
        }

        public static void SetHue(this ref Vector4 color, float hue)
        {
            Vector4 hsva = new(hue, color.GetSaturation(), color.GetValue(), color.W);
            color = FromHSV(hsva);
        }

        public static float GetSaturation(this Vector4 color)
        {
            float max = Math.Max(color.X, Math.Max(color.Y, color.Z));
            float min = Math.Min(color.X, Math.Min(color.Y, color.Z));
            float delta = max - min;
            return max == 0 ? 0 : delta / max;
        }

        public static void SetSaturation(this ref Vector4 color, float saturation)
        {
            Vector4 hsva = new(color.GetHue(), saturation, color.GetValue(), color.W);
            color = FromHSV(hsva);
        }

        public static float GetValue(this Vector4 color)
        {
            return Math.Max(color.X, Math.Max(color.Y, color.Z));
        }

        public static void SetValue(this ref Vector4 color, float value)
        {
            Vector4 hsva = new(color.GetHue(), color.GetSaturation(), value, color.W);
            color = FromHSV(hsva);
        }

        [Conditional("DEBUG")]
        private static void ThrowIfOutOfRange(float hue)
        {
            if (hue < 0 || hue > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(hue), hue, "Hue must be between contained within the 0-1 range");
            }
        }
    }
}