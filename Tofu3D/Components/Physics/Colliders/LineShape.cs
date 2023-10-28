namespace Scripts;

public class LineShape : Shape
{
    //[ShowInEditor]
    //[System.ComponentModel.Editor(typeof(Editor.MethodEditor), typeof(System.Drawing.Design.UITypeEditor))]
    //public bool EditPoints { get; set; } = false;
    public float Length = 0;

    public float? StaticAngle;

    public Vector2 GetLineStart() // put both methods into tuple method?
    {
        if (StaticAngle != null)
            return Transform.WorldPosition +
                   new Vector2((float)Math.Cos((float)StaticAngle), (float)Math.Sin((float)StaticAngle));

        return Transform.WorldPosition +
               new Vector2((float)Math.Cos(Transform.Rotation.Z), (float)Math.Sin(Transform.Rotation.Z));
    }

    public Vector2 GetLineEnd()
    {
        if (StaticAngle != null)
            return Transform.WorldPosition +
                   new Vector2(-(float)Math.Cos((float)StaticAngle), (float)Math.Sin((float)StaticAngle)) * Length;

        return Transform.WorldPosition +
               new Vector2(-(float)Math.Cos(Transform.Rotation.Z), (float)Math.Sin(Transform.Rotation.Z)) * Length;
    }
}