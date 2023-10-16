using System.Linq;

namespace Tofu3D;

public class Curve
{
    private static readonly int NUM_OF_POINTS = 1000;
    public float[] _points;

    public List<Vector2> DefiningPoints = new List<Vector2>()
        { new Vector2(0, 0.5f), new Vector2(0.5f, 0f), new Vector2(0.8f, 0.5f) };

    public Curve()
    {
        RecalculateCurve();
    }

    public void RecalculateCurve()
    {
        DefiningPoints.Sort((vector2, vector3) => vector2.X.CompareTo(vector3.X));
        _points = new float[NUM_OF_POINTS];
        Vector2[] uh = HigherOrderBezierCurve(DefiningPoints, NUM_OF_POINTS);
        for (int i = 0; i < _points.Length; i++)
        {
            // _points[i] = Mathf.Sin((float)i / NUM_OF_POINTS * Mathf.TwoPi * 10) * 0.5f + 0.5f;
            _points[i] = uh[i].Y;
        }
    }

    public void AddDefiningPoint(Vector2 point)
    {
        DefiningPoints.Add(point);
    }

    // A function that takes an array of n+1 points that define a higher-order Bezier curve
    // and returns an array of points that approximate the curve
    public static Vector2[] HigherOrderBezierCurve(List<Vector2> points, int numPoints)
    {
        // Check if the input array has at least two points
        if (points == null || points.Count < 2)
        {
            throw new ArgumentException("Invalid input array");
        }

        // Check if the number of points to generate is positive
        if (numPoints <= 0)
        {
            throw new ArgumentException("Invalid number of points");
        }

        // Create an output array of the same size as the number of points
        Vector2[] output = new Vector2[numPoints];

        // Loop through the output array and calculate each point using the higher-order Bezier formula
        for (int i = 0; i < numPoints; i++)
        {
            // Calculate the parameter t that corresponds to the current point
            double t = (double)i / (numPoints - 1);

            // Initialize the coordinates of the current point to zero
            double x = 0;
            double y = 0;

            // Loop through the input array and add each term to the coordinates
            for (int j = 0; j < points.Count; j++)
            {
                // Calculate the binomial coefficient for the current term
                int binom = BinomialCoefficient(points.Count - 1, j);

                // Calculate the Bernstein polynomial for the current term
                double bernstein = Math.Pow(1 - t, points.Count - 1 - j) * Math.Pow(t, j);

                // Add the product of the binomial coefficient, the Bernstein polynomial and the point coordinates to the output coordinates
                x += binom * bernstein * points[j].X;
                y += binom * bernstein * points[j].Y;
            }

            // Assign the coordinates to the output array
            output[i] = new Vector2((float)x, (float)y);
        }

        // Return the output array
        return output;
    }

    // A helper function that calculates the binomial coefficient using a recursive formula
    public static int BinomialCoefficient(int n, int k)
    {
        // Check if n and k are non-negative integers
        if (n < 0 || k < 0 || n < k)
        {
            throw new ArgumentException("Invalid arguments");
        }

        // Base cases
        if (k == 0 || k == n)
        {
            return 1;
        }

        // Recursive case
        return BinomialCoefficient(n - 1, k - 1) + BinomialCoefficient(n - 1, k);
    }
}