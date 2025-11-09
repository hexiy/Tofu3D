// ReSharper disable InconsistentNaming

using System.Threading;

namespace TofuEngine;

public static class IdManager
{
    private static int GameObjectNextId = 0;

    private static int ComponentNextId = 0;

    public static int GetNextGameObjectIdAndIncrementIt()
    {
        return Interlocked.Increment(ref GameObjectNextId) - 1;
    }

    public static int SetNextGameObjectId(int id)
    {
        return Interlocked.Exchange(ref GameObjectNextId, id);
    }

    public static void ResetGameObjectNextId()
    {
        Interlocked.Exchange(ref GameObjectNextId, 0);
    }


    public static int GetNextComponentIdAndIncrementIt()
    {
        return Interlocked.Increment(ref ComponentNextId) - 1;
    }

    public static int SetNextComponentId(int id)
    {
        return Interlocked.Exchange(ref ComponentNextId, id);
    }

    public static void ResetComponentNextId()
    {
        Interlocked.Exchange(ref ComponentNextId, 0);
    }
}