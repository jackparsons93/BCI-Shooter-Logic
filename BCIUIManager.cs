using UnityEngine;
using TMPro;

public class BCIUIManager : MonoBehaviour
{
    public BCIReceiver bci;
    public TextMeshProUGUI uiText;

    void Update()
    {
        if (bci == null || uiText == null) return;

        string sigColor = bci.signal_ok == 1 ? "<color=green>OK</color>" : "<color=red>LOST</color>";

        // Dynamic labels based on active metric
        string metricLabel = bci.activeMetric == "TBR" ? "TBR" : "SMR Ratio";
        float rawValue = bci.currentVal;

        uiText.text = $"<b>[ BCI MONITOR ]</b>\n" +
                      $"Signal: {sigColor}\n" +
                      $"Site: <color=yellow>{bci.activeSite}</color> | Metric: <color=yellow>{bci.activeMetric}</color>\n" +
                      $"Current {metricLabel}: <color=white>{rawValue:0.000}</color>\n" +
                      $"Baseline: <color=white>{bci.baselineVal:0.000}</color>\n" +
                      $"Power Level: {(bci.performance_score * 100f):0}%\n\n" +
                      $"<b>[ CONTROLS ]</b>\n" +
                      $"1-4: Change Site (F3, Fz, F4, Cz)\n" +
                      $"Q: TBR Mode (Focus)\n" +
                      $"W: SMR Mode (Calm)\n" +
                      $"SPACE: Reset Baseline";
    }
}