using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class TimedActivator : MonoBehaviour {
    public Text theText;

    public static Stopwatch StartTimer()
    {
            Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();
            return st;
    }

    public static string StopTimer(Stopwatch st, string Function)
    {
            st.Stop();
            return (Function + " Completed " + st.ElapsedMilliseconds + "ms");
    }

    public void TimedActivate()
    {
        Stopwatch st = StartTimer();
        this.gameObject.SetActive(true);
        theText.text = StopTimer(st, gameObject.name);
    }
}
