namespace ColorEngine
{
    public struct Hue
    {
        public double P { get; }
        public double Q { get; }
        public double T { get; }

        public Hue() : this(0, 0, 0)
        {
        }

        public Hue(double p, double q, double t)
        {
            P = p;
            Q = q;
            T = t;
        }

        public double ToRgbCoefficient()
        {
            double p = P;
            double q = Q;
            double t = T;

            if (t < 0)
                t += 1;
            else if (t > 1)
                t -= 1;

            if (t < 1 / 6)
                return p + (q - p) * 6 * t;

            if (t < 1 / 2)
                return q;

            if (t < 2 / 3)
            {
                double product = (q - p) * (2 / 3 - t) * 6;
                return p + product;
            }

            return p;
        }
    }
}
