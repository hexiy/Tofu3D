using System.Collections;

namespace TofuEngine;

internal class CoroutineManager
{
    private readonly List<IEnumerator> _activeCoroutines = new List<IEnumerator>();

    public CoroutineManager()
    {
        Scene.SceneStartedDisposing += _activeCoroutines.Clear;
    }

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

            if (routine.Current == null || routine.Current is WaitForEndOfFrame)
            {
                // waits a frame
                continue;
            }

            else if (routine.Current is WaitForSeconds waitForSeconds)
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
            else if (routine.Current is WaitWhile waitWhile)
            {
                if (waitWhile.WaitCondition() == true)
                {
                    continue;
                }
                else
                {
                    routine.MoveNext();
                }
            }
            else if (routine.Current is WaitUntil waitUntil)
            {
                if (waitUntil.WaitCondition() == false)
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