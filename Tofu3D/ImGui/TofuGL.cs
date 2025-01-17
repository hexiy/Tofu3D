// ReSharper disable once InconsistentNaming
public static class TofuGL
{
    public static bool CheckGlError(string title)
    {
        var hadError = false;
        ErrorCode error;
        var i = 1;
        while ((error = GL.GetError()) != ErrorCode.NoError)
        {
            Debug.LogError($"{title} ({i++}): {error}");
            hadError = true;
        }

        return hadError;
    }
}