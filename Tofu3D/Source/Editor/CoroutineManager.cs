using System.Collections;

namespace Tofu3D;

public class CoroutineManager
{
    private readonly List<IEnumerator> _activeCoroutines = new List<IEnumerator>();

    public void StartCoroutine(IEnumerator routine)
    {
        _activeCoroutines.Add(routine);
    }

    internal void Update()
    {
        // go from end to start so we can just remove them without changing index
        for (int i = _activeCoroutines.Count - 1; i >= 0; i--)
        {
            IEnumerator routine = _activeCoroutines[i];
            if (routine.Current == null)
            {
                bool routineRunning = routine.MoveNext();
                if (routineRunning == false)
                {
                    _activeCoroutines.RemoveAt(i);
                    continue;
                }
            }


            if (routine.Current is WaitForSeconds waitForSeconds)
            {
                waitForSeconds.SecondsToWait -= Time.EditorDeltaTime;

                if (waitForSeconds.SecondsToWait > 0)
                {
                    continue;
                }
                else
                {
                    routine.MoveNext();
                }
            }
        }
    }
}