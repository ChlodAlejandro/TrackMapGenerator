namespace TrackMapGenerator.Util
{
    public static class MathD
    {
        public static double InvLerp(double a, double b, double t)
        {
            return (t - a) / (b - a);
        }

        public static double Lerp(double a, double b, double t)
        {
            return (1.0f - t) * a + b * t;
        }

        public static double Remap(double aMin, double aMax, double bMin, double bMax, double t)
        {
            return Lerp(bMin, bMax, InvLerp(aMin, aMax, t));
        }
    }
}