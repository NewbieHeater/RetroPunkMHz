using System.Collections;

public interface IAsyncGameEvent
{
    IEnumerator InvokeRoutine();
}
