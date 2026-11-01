
using UnityEngine;
using System.Collections;

public class SJFSimulation : MonoBehaviour
{
    public Transform cpu;

    private string[] processNames = { "P1", "P2", "P3", "P4", "P5" };
    private int[] burstTimes = { 4, 3, 2, 5, 1 };

    private GameObject[] processes;
    private bool[] completed;
    private bool hasStarted = false;

    public void StartSimulation()
    {
        if (hasStarted) return;

        if (cpu == null)
        {
            Transform foundCPU = transform.Find("CPU");

            if (foundCPU != null)
                cpu = foundCPU;
        }

        if (cpu == null)
        {
            Debug.LogError("CPU child not found!");
            return;
        }

        hasStarted = true;

        CreateProcesses();
        StartCoroutine(RunSJF());
    }
    void CreateProcesses()
    {
        processes = new GameObject[processNames.Length];
        completed = new bool[processNames.Length];

        for (int i = 0; i < processNames.Length; i++)
        {
            GameObject block = GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

            block.name = processNames[i];
            block.transform.SetParent(transform, false);

            block.transform.localPosition =
                new Vector3(-0.6f + i * 0.3f, 0.15f, 0.4f);

            block.transform.localScale =
                new Vector3(0.18f, 0.18f, 0.18f);

            

            GameObject label = new GameObject("Label");
            label.transform.SetParent(block.transform, false);
            label.transform.localPosition = new Vector3(0, 1f, 0);

            TextMesh text = label.AddComponent<TextMesh>();
            text.text = processNames[i] + "\nBT: " + burstTimes[i];
            text.fontSize = 48;
            text.characterSize = 0.025f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            ProcessBlockStyle.Apply(block, i, text);

            processes[i] = block;
        }
    }

    IEnumerator RunSJF()
    {
        yield return new WaitForSeconds(2f);

        for (int count = 0; count < processes.Length; count++)
        {
            int shortestIndex = -1;

            // Find the unfinished process with the shortest burst time
            for (int i = 0; i < processes.Length; i++)
            {
                if (completed[i])
                    continue;

                if (shortestIndex == -1 ||
                    burstTimes[i] < burstTimes[shortestIndex])
                {
                    shortestIndex = i;
                }
            }

            if (shortestIndex == -1)
                break;

            GameObject currentProcess = processes[shortestIndex];
            if (SchedulingTableUI.Instance != null)
            SchedulingTableUI.Instance.HighlightProcess(processNames[shortestIndex]);

            Debug.Log(processNames[shortestIndex] +
                " is running on the CPU");

            Vector3 start = currentProcess.transform.localPosition;
            Vector3 end = cpu.localPosition;

            float duration = burstTimes[shortestIndex];
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                currentProcess.transform.localPosition =
                    Vector3.Lerp(start, end,
                    Mathf.Clamp01(elapsed / duration));

                yield return null;
            }

            currentProcess.transform.localPosition = end;

            Debug.Log(processNames[shortestIndex] + " finished");

            yield return new WaitForSeconds(0.5f);

            Destroy(currentProcess);
            if (SchedulingTableUI.Instance != null)
            SchedulingTableUI.Instance.CompleteProcess(processNames[shortestIndex]);
            processes[shortestIndex] = null;
            completed[shortestIndex] = true;

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("SJF Scheduling completed!");
    }
}
