
using UnityEngine;
using System.Collections;

public class RoundRobinSimulation : MonoBehaviour
{
    public Transform cpu;
    public int timeQuantum = 2;

    private string[] processNames =
        { "P1", "P2", "P3", "P4", "P5" };

    private int[] burstTimes = { 4, 3, 2, 5, 1 };

    private int[] remainingTimes;
    private GameObject[] processes;
    private Vector3[] queuePositions;

    private bool hasStarted = false;

    public void StartSimulation()
    {
        if (hasStarted)
            return;

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

        if (timeQuantum <= 0)
        {
            Debug.LogError("Time quantum must be greater than zero!");
            return;
        }

        hasStarted = true;

        CreateProcesses();
        StartCoroutine(RunRoundRobin());
    }

    void CreateProcesses()
    {
        processes = new GameObject[processNames.Length];

        remainingTimes = (int[])burstTimes.Clone();

        queuePositions = new Vector3[processNames.Length];

        for (int i = 0; i < processNames.Length; i++)
        {
            GameObject block = GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

            block.name = processNames[i];

            block.transform.SetParent(transform, false);

            Vector3 position = new Vector3(
                -0.6f + i * 0.3f,
                0.15f,
                0.4f
            );

            block.transform.localPosition = position;

            queuePositions[i] = position;

            block.transform.localScale =
                new Vector3(0.18f, 0.18f, 0.18f);

            

            GameObject label = new GameObject("Label");

            label.transform.SetParent(block.transform, false);

            label.transform.localPosition =
                new Vector3(0, 1f, 0);

            TextMesh text = label.AddComponent<TextMesh>();

            text.text = processNames[i] +
                "\nRemaining: " + remainingTimes[i];

            text.fontSize = 48;
            text.characterSize = 0.025f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            ProcessBlockStyle.Apply(block, i, text);

            processes[i] = block;
        }
    }

    IEnumerator RunRoundRobin()
    {
        yield return new WaitForSeconds(2f);

        bool unfinished = true;

        while (unfinished)
        {
            unfinished = false;

            for (int i = 0; i < processes.Length; i++)
            {
                if (remainingTimes[i] <= 0 || processes[i] == null)
                    continue;

                unfinished = true;

                // Check whether another unfinished process is waiting
                bool anotherProcessWaiting = false;

                for (int j = 0; j < processes.Length; j++)
                {
                    if (j != i &&
                        remainingTimes[j] > 0 &&
                        processes[j] != null)
                    {
                        anotherProcessWaiting = true;
                        break;
                    }
                }

                // Use the time quantum when other processes are waiting.
                // Otherwise, let the final process finish.
                int executionTime = anotherProcessWaiting
                    ? Mathf.Min(timeQuantum, remainingTimes[i])
                    : remainingTimes[i];

                Debug.Log(
                    processNames[i] +
                    " is running for " +
                    executionTime +
                    " time units."
                );

                GameObject currentProcess = processes[i];

                // Highlight current process in the table
                if (SchedulingTableUI.Instance != null)
                {
                    SchedulingTableUI.Instance.HighlightProcess(
                        processNames[i]
                    );
                }

                // Move process to CPU
                yield return StartCoroutine(MoveProcess(
                    currentProcess,
                    currentProcess.transform.localPosition,
                    cpu.localPosition,
                    executionTime
                ));

                remainingTimes[i] -= executionTime;

                // Update remaining time in the table
                if (SchedulingTableUI.Instance != null)
                {
                    SchedulingTableUI.Instance.UpdateRemainingTime(
                        processNames[i],
                        remainingTimes[i]
                    );
                }

                // Update remaining time above the cube
                TextMesh label =
                    currentProcess.GetComponentInChildren<TextMesh>();

                if (label != null)
                {
                    label.text = processNames[i] +
                        "\nRemaining: " + remainingTimes[i];
                }

                if (remainingTimes[i] <= 0)
                {
                    Debug.Log(processNames[i] + " finished!");

                    yield return new WaitForSeconds(0.5f);

                    Destroy(currentProcess);

                    processes[i] = null;

                    // Remove completed process from the table
                    if (SchedulingTableUI.Instance != null)
                    {
                        SchedulingTableUI.Instance.CompleteProcess(
                            processNames[i]
                        );
                    }
                }
                else
                {
                    Debug.Log(
                        processNames[i] +
                        " time slice finished. Returning to queue."
                    );

                    // Return unfinished process to its queue position
                    yield return StartCoroutine(MoveProcess(
                        currentProcess,
                        currentProcess.transform.localPosition,
                        queuePositions[i],
                        0.5f
                    ));
                }

                yield return new WaitForSeconds(0.3f);
            }
        }

        Debug.Log("Round Robin Scheduling completed!");
    }

    IEnumerator MoveProcess(
        GameObject process,
        Vector3 start,
        Vector3 end,
        float duration)
    {
        if (process == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (process == null)
                yield break;

            elapsed += Time.deltaTime;

            process.transform.localPosition = Vector3.Lerp(
                start,
                end,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        if (process != null)
        {
            process.transform.localPosition = end;
        }
    }
}
