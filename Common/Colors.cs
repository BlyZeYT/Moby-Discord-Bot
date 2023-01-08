namespace Moby.Common;

public readonly struct HSV
{
    public double H { get; }
    public double S { get; }
    public double V { get; }

    public HSV()
    {
        H = 0;
        S = 0;
        V = 0;
    }

    public HSV(double h, double s, double v)
    {
        H = h;
        S = s;
        V = v;
    }
}

public readonly struct HSL
{
    public double H { get; }
    public double S { get; }
    public double L { get; }

    public HSL()
    {
        H = 0;
        S = 0;
        L = 0;
    }

    public HSL(double h, double s, double l)
    {
        H = h;
        S = s;
        L = l;
    }
}

public readonly struct CMYK
{
    public double C { get; }
    public double M { get; }
    public double Y { get; }
    public double K { get; }

    public CMYK()
    {
        C = 0;
        M = 0;
        Y = 0;
        K = 0;
    }

    public CMYK(double c, double m, double y, double k)
    {
        C = c;
        M = m;
        Y = y;
        K = k;
    }
}

public readonly struct YCbCr
{
    public int Y { get; }
    public int Cb { get; }
    public int Cr { get; }

    public YCbCr()
    {
        Y = 0;
        Cb = 0;
        Cr = 0;
    }

    public YCbCr(int y, int cb, int cr)
    {
        Y = y;
        Cb = cb;
        Cr = cr;
    }
}