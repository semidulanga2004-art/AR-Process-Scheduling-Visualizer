
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoundRobinSimulation : MonoBehaviour
{
    [Header("CPU Settings")]
    public Transform cpu;
    public int timeQuantum = 2;

    [Header("Animation Settings")]
    public float secondsPerBurstUnit = 3.53f;
    public float movementDuration = 0.35f;

    [Header("Queue Layout")]
    public Vector3 queueStartPosition =
        new Vector3(-0.6f, 0.15f, 0.4f);

    public float queueSpacing = 0.3f;

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
        simulationTime = 0f;
        nextArrivalIndex = 0;
        readyQueue.Clear();

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

            block.transform.localPosition = queueStartPosition;

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

            // Only display processes when they arrive.
            block.SetActive(false);
        }
    }

    IEnumerator RunRoundRobin()
    {
        yield return new WaitForSeconds(2f);

        AddArrivedProcesses(simulationTime);
        UpdateQueueVisuals();

        while (readyQueue.Count > 0 ||
               nextArrivalIndex < processNames.Length)
        {
            // Handle CPU idle time.
            if (readyQueue.Count == 0)
            {
                simulationTime =
                    arrivalTimes[nextArrivalIndex];

                AddArrivedProcesses(simulationTime);
                UpdateQueueVisuals();
            }

            if (readyQueue.Count == 0)
                continue;

            // Select the first process in the ready queue.
            int currentIndex = readyQueue.Dequeue();

            GameObject currentProcess = processes[currentIndex];

            UpdateQueueVisuals();

            // Move the selected process to the CPU.
            yield return StartCoroutine(
                MoveProcess(currentProcess, cpu.localPosition)
            );

            int executionTime = Mathf.Min(
                timeQuantum,
                remainingTimes[currentIndex]
            );

            Debug.Log(
                processNames[currentIndex] +
                " executing for " + executionTime +
                " time units."
            );

            if (SchedulingTableUI.Instance != null)
            {
                SchedulingTableUI.Instance.HighlightProcess(
                    processNames[currentIndex]
                );
            }

            // Execute the process while detecting new arrivals.
            yield return StartCoroutine(
                ExecuteProcess(currentIndex, executionTime)
            );

            remainingTimes[currentIndex] -= executionTime;

            // Update the process label.
            TextMesh label =
                currentProcess.GetComponentInChildren<TextMesh>();

            if (label != null)
            {
                label.text = processNames[currentIndex] +
                    "\nRemaining: " + remainingTimes[currentIndex];
            }

            if (SchedulingTableUI.Instance != null)
            {
                SchedulingTableUI.Instance.UpdateRemainingTime(
                    processNames[currentIndex],
                    remainingTimes[currentIndex]
                );
            }

            // Check whether the process has finished.
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
                    " quantum finished. Returning to ready queue."
                );

                // Return unfinished process to the back.
                // Existing waiting and newly arrived processes
                // remain ahead of it.
                Vector3 returnPosition =
                    GetQueuePosition(readyQueue.Count);

                yield return StartCoroutine(
                    MoveProcess(currentProcess, returnPosition)
                );

                readyQueue.Enqueue(currentIndex);
            }

            UpdateQueueVisuals();

            yield return new WaitForSeconds(0.3f);
        }

        Debug.Log("Round Robin Scheduling completed!");
    }

    IEnumerator ExecuteProcess(int index, int executionTime)
    {
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

            float currentSimulationTime =
                sliceStartTime + progress * executionTime;

            // Add newly arrived processes during execution.
            AddArrivedProcesses(currentSimulationTime);

            UpdateQueueVisuals();

            yield return null;
        }

        // Ensure the exact end time is processed.
        simulationTime = sliceStartTime + executionTime;

        AddArrivedProcesses(simulationTime);
    }

    IEnumerator MoveProcess(GameObject process, Vector3 target)
    {
        if (process == null)
            yield break;

        Vector3 start = process.transform.localPosition;

        float elapsed = 0f;

        while (elapsed < movementDuration)
        {
            if (process == null)
                yield break;

            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / movementDuration
            );

            process.transform.localPosition =
                Vector3.Lerp(start, target, progress);

            yield return null;
        }

        if (process != null)
        {
            process.transform.localPosition = target;
        }
    }

    Vector3 GetQueuePosition(int positionIndex)
    {
        return queueStartPosition +
            new Vector3(positionIndex * queueSpacing, 0f, 0f);
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
                " arrived at time " + arrivalTimes[index]
            );

            nextArrivalIndex++;

            UpdateQueueVisuals();
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
                    GetQueuePosition(positionIndex);

                positionIndex++;
            }
        }
    }
}
