
using UnityEngine;
using System.Collections;

public class SRTFSimulation : MonoBehaviour
{
    [Header("CPU Settings")]
    public Transform cpu;
    public float timeUnitDuration = 1f;

    [Header("Process Information")]
    private string[] processNames =
        { "P1", "P2", "P3", "P4", "P5" };

    private int[] arrivalTimes = { 0, 2, 4, 5, 6 };
    private int[] burstTimes = { 7, 4, 1, 4, 2 };

    private int[] remainingTimes;

    private GameObject[] processes;
    private Vector3[] queuePositions;

    private int currentTime = 0;
    private bool hasStarted = false;

    private GameObject currentProcess;

    // Start the simulation
    public void StartSimulation()
    {
        if (hasStarted)
            return;

        // Find CPU if not assigned
        if (cpu == null)
        {
            Transform foundCPU = transform.Find("CPU");

            if (foundCPU != null)
                cpu = foundCPU;
        }

        if (cpu == null)
        {
            Debug.LogError("SRTF: CPU not found!");
            return;
        }

        hasStarted = true;

        currentTime = 0;
        currentProcess = null;

        remainingTimes = (int[])burstTimes.Clone();

        CreateProcesses();

        StartCoroutine(RunSRTF());
    }

    // Create all process blocks
    void CreateProcesses()
    {
        processes = new GameObject[processNames.Length];
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

           
            // Create process label
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

            // Show only arrived processes
            block.SetActive(arrivalTimes[i] <= currentTime);
        }
    }

    // Main SRTF scheduling algorithm
    IEnumerator RunSRTF()
    {
        yield return new WaitForSeconds(1f);

        int completedProcesses = 0;

        while (completedProcesses < processNames.Length)
        {
            // Find the process with the shortest remaining time
            int shortestIndex = FindShortestProcess();

            // No process has arrived yet
            if (shortestIndex == -1)
            {
                currentTime++;

                yield return new WaitForSeconds(timeUnitDuration);

                continue;
            }

            GameObject selectedProcess = processes[shortestIndex];

            // Make newly arrived processes visible
            for (int i = 0; i < processes.Length; i++)
            {
                if (processes[i] != null &&
                    arrivalTimes[i] <= currentTime)
                {
                    processes[i].SetActive(true);
                }
            }

            // Handle process switching and preemption
            if (currentProcess != selectedProcess)
            {
                // Return previous process to ready queue
                if (currentProcess != null)
                {
                    int previousIndex =
                        System.Array.IndexOf(
                            processes,
                            currentProcess
                        );

                    if (previousIndex >= 0)
                    {
                        yield return StartCoroutine(
                            MoveProcess(
                                currentProcess,
                                queuePositions[previousIndex]
                            )
                        );
                    }
                }

                // Move selected process to CPU
                yield return StartCoroutine(
                    MoveProcess(
                        selectedProcess,
                        cpu.localPosition
                    )
                );

                currentProcess = selectedProcess;

                Debug.Log(
                    selectedProcess.name +
                    " selected at time " + currentTime
                );
            }

            // Highlight current process in table
            if (SchedulingTableUI.Instance != null)
            {
                SchedulingTableUI.Instance.HighlightProcess(
                    selectedProcess.name
                );
            }

            // Execute one time unit
            Debug.Log(
                selectedProcess.name +
                " running at time " + currentTime
            );

            yield return new WaitForSeconds(timeUnitDuration);

            remainingTimes[shortestIndex]--;

            currentTime++;

            // Update 3D process label
            TextMesh label =
                selectedProcess.GetComponentInChildren<TextMesh>();

            if (label != null)
            {
                label.text = processNames[shortestIndex] +
                    "\nRemaining: " +
                    remainingTimes[shortestIndex];
            }

            // Update scheduling table
            if (SchedulingTableUI.Instance != null)
            {
                SchedulingTableUI.Instance.UpdateRemainingTime(
                    processNames[shortestIndex],
                    remainingTimes[shortestIndex]
                );
            }

            // Check whether process has completed
            if (remainingTimes[shortestIndex] <= 0)
            {
                Debug.Log(
                    processNames[shortestIndex] + " finished!"
                );

                Destroy(selectedProcess);

                processes[shortestIndex] = null;

                currentProcess = null;

                completedProcesses++;

                if (SchedulingTableUI.Instance != null)
                {
                    SchedulingTableUI.Instance.CompleteProcess(
                        processNames[shortestIndex]
                    );
                }
            }
        }

        Debug.Log("SRTF Scheduling completed!");
    }

    // Find process with shortest remaining time
    int FindShortestProcess()
    {
        int shortestIndex = -1;

        for (int i = 0; i < processes.Length; i++)
        {
            if (processes[i] == null ||
                remainingTimes[i] <= 0 ||
                arrivalTimes[i] > currentTime)
            {
                continue;
            }

            if (shortestIndex == -1 ||
                remainingTimes[i] < remainingTimes[shortestIndex])
            {
                shortestIndex = i;
            }
        }

        // Keep the current process if another process
        // only has an equal remaining time.
        if (currentProcess != null)
        {
            int currentIndex =
                System.Array.IndexOf(processes, currentProcess);

            if (currentIndex >= 0 &&
                remainingTimes[currentIndex] > 0 &&
                arrivalTimes[currentIndex] <= currentTime)
            {
                if (shortestIndex == -1 ||
                    remainingTimes[currentIndex] <=
                    remainingTimes[shortestIndex])
                {
                    shortestIndex = currentIndex;
                }
            }
        }

        return shortestIndex;
    }

    // Move process between ready queue and CPU
    IEnumerator MoveProcess(
        GameObject process,
        Vector3 target
    )
    {
        if (process == null)
            yield break;

        Vector3 start =
            process.transform.localPosition;

        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            if (process == null)
                yield break;

            elapsed += Time.deltaTime;

            process.transform.localPosition =
                Vector3.Lerp(
                    start,
                    target,
                    Mathf.Clamp01(elapsed / duration)
                );

            yield return null;
        }

        if (process != null)
        {
            process.transform.localPosition = target;
        }
    }
}
