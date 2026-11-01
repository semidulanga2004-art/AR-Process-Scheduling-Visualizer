
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoundRobinSimulation : MonoBehaviour
{
    public Transform cpu;
    public int timeQuantum = 2;
    [Header("Animation Settings")]
public float secondsPerBurstUnit = 3.53f;

    private string[] processNames =
        { "P1", "P2", "P3", "P4", "P5" };

    private int[] arrivalTimes = { 0, 2, 4, 5, 6 };
    private int[] burstTimes = { 4, 3, 2, 5, 1 };

    private int[] remainingTimes;
    private GameObject[] processes;

    private Queue<int> readyQueue = new Queue<int>();

    private int nextArrivalIndex = 0;
    private float simulationTime = 0f;

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

        for (int i = 0; i < processNames.Length; i++)
        {
            GameObject block = GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

            block.name = processNames[i];
            block.transform.SetParent(transform, false);

            block.transform.localPosition = Vector3.zero;

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

            // Processes remain hidden until their arrival time.
            block.SetActive(false);
        }
    }

    IEnumerator RunRoundRobin()
    {
        yield return new WaitForSeconds(2f);

        // Add processes that have arrived at time zero.
        AddArrivedProcesses(simulationTime);

        UpdateQueueVisuals();

        while (readyQueue.Count > 0 ||
               nextArrivalIndex < processNames.Length)
        {
            // If the CPU has no process to execute,
            // move simulation time to the next arrival.
            if (readyQueue.Count == 0)
            {
                simulationTime =
                    arrivalTimes[nextArrivalIndex];

                AddArrivedProcesses(simulationTime);

                UpdateQueueVisuals();
            }

            if (readyQueue.Count == 0)
                continue;

            // Select the process at the front of the queue.
            int currentIndex = readyQueue.Dequeue();

            GameObject currentProcess = processes[currentIndex];

            UpdateQueueVisuals();

            int executionTime = Mathf.Min(
                timeQuantum,
                remainingTimes[currentIndex]
            );

            Debug.Log(
                processNames[currentIndex] +
                " starts executing for " +
                executionTime + " time units."
            );

            if (SchedulingTableUI.Instance != null)
            {
                SchedulingTableUI.Instance.HighlightProcess(
                    processNames[currentIndex]
                );
            }

            // Execute the process and detect new arrivals
            // while it is running.
            yield return StartCoroutine(
                ExecuteProcess(currentIndex, executionTime)
            );

            remainingTimes[currentIndex] -= executionTime;

            if (SchedulingTableUI.Instance != null)
            {
                SchedulingTableUI.Instance.UpdateRemainingTime(
                    processNames[currentIndex],
                    remainingTimes[currentIndex]
                );
            }

            TextMesh label =
                currentProcess.GetComponentInChildren<TextMesh>();

            if (label != null)
            {
                label.text = processNames[currentIndex] +
                    "\nRemaining: " + remainingTimes[currentIndex];
            }

            if (remainingTimes[currentIndex] <= 0)
            {
                Debug.Log(
                    processNames[currentIndex] + " finished!"
                );

                yield return new WaitForSeconds(0.5f);

                Destroy(currentProcess);

                processes[currentIndex] = null;

                if (SchedulingTableUI.Instance != null)
                {
                    SchedulingTableUI.Instance.CompleteProcess(
                        processNames[currentIndex]
                    );
                }
            }
            else
            {
                Debug.Log(
                    processNames[currentIndex] +
                    " time quantum finished. Returning to the queue."
                );

                // Newly arrived processes have already been added.
                // Therefore, the unfinished process goes behind them.
                readyQueue.Enqueue(currentIndex);
            }

            UpdateQueueVisuals();

            yield return new WaitForSeconds(0.3f);
        }

        Debug.Log("Round Robin Scheduling completed!");
    }

    IEnumerator ExecuteProcess(int index, int executionTime)
    {
        GameObject process = processes[index];

        if (process == null)
            yield break;

        Vector3 start = process.transform.localPosition;
        Vector3 end = cpu.localPosition;

        float elapsed = 0f;
float sliceStartTime = simulationTime;

float animationDuration =
    executionTime * secondsPerBurstUnit;

        while (elapsed < animationDuration)
{
    elapsed += Time.deltaTime;

    float progress = Mathf.Clamp01(
        elapsed / animationDuration
    );

    process.transform.localPosition =
        Vector3.Lerp(start, end, progress);

    // Update simulated time according to animation progress.
    float currentSimulationTime =
        sliceStartTime + progress * executionTime;

    AddArrivedProcesses(currentSimulationTime);

    UpdateQueueVisuals();

    yield return null;
}

        process.transform.localPosition = end;

        simulationTime = sliceStartTime + executionTime;

        // Important: process arrivals at the exact end time
        // before returning the unfinished process to the queue.
        AddArrivedProcesses(simulationTime);
    }

    void AddArrivedProcesses(float currentTime)
    {
        while (nextArrivalIndex < processNames.Length &&
               arrivalTimes[nextArrivalIndex] <= currentTime)
        {
            int index = nextArrivalIndex;

            processes[index].SetActive(true);

            readyQueue.Enqueue(index);

            Debug.Log(
                processNames[index] +
                " arrived and entered the ready queue."
            );

            nextArrivalIndex++;
        }
    }

    void UpdateQueueVisuals()
    {
        int positionIndex = 0;

        foreach (int index in readyQueue)
        {
            if (processes[index] != null)
            {
                processes[index].transform.localPosition =
                    new Vector3(
                        -0.6f + positionIndex * 0.3f,
                        0.15f,
                        0.4f
                    );

                positionIndex++;
            }
        }
    }
}
