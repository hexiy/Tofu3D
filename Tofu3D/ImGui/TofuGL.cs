// ReSharper disable once InconsistentNaming
public static class TofuGL
{
    public static bool CheckGlError(string errorLabel = "")
    {
        var hadError = false;
        ErrorCode error;
        var i = 1;
        while ((error = GL.GetError()) != ErrorCode.NoError)
        {
            Debug.LogError($"{errorLabel} ({i++}): {error}");
            hadError = true;
        }

        return hadError;
    }
}