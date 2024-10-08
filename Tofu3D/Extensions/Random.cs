namespace Tofu3D;

public static class Random
{
    public static System.Random Rnd = new();

    public static int Range(int min, int max) => Rnd.Next(max - min) + min;

    public static float Range(float max) => (float)Rnd.NextDouble() * max;

    public static float Range(float min, float max) => (float)Rnd.NextDouble() * (max - min) + min;

    public static Color ColorRange(Color color1, Color color2)
    {
        var howMuch = Range(1);
        var r = Mathf.Lerp(color1.R, color2.R, howMuch);
        return new Color(Mathf.Lerp(color1.R, color2.R, howMuch) / 255,
            Mathf.Lerp(color1.G, color2.G, howMuch) / 255,
            Mathf.Lerp(color1.B, color2.B, howMuch) / 255,
            Mathf.Lerp(color1.A, color2.A, howMuch) / 255);
    }

    public static Color RandomColor() => new(Range(1), Range(1), Range(1), 1);
}