
using UnityEngine;
using System.Collections;

public class FCFSSimulation : MonoBehaviour
{
    // The CPU is already a child of SchedulingLab
    public Transform cpu;

    // Example processes and their burst times
    private string[] processNames = { "P1", "P2", "P3" };
    private int[] burstTimes = { 4, 3, 2 };

    private GameObject[] processes;

    void Start()
    {
        // Find the CPU child automatically
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

        CreateProcesses();
        StartCoroutine(RunFCFS());
    }

    void CreateProcesses()
    {
        processes = new GameObject[processNames.Length];

        for (int i = 0; i < processNames.Length; i++)
        {
            // Create one cube for each process
            GameObject block = GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

            block.name = processNames[i];
            block.transform.SetParent(transform, false);

            // Place processes in a queue beside the CPU
            block.transform.localPosition =
                new Vector3(-0.6f + i * 0.3f, 0.15f, 0.4f);

            block.transform.localScale =
                new Vector3(0.18f, 0.18f, 0.18f);

            // Give each process a different colour
            Renderer r = block.GetComponent<Renderer>();

            if (r != null)
            {
                r.material.color = new Color[]
                {
                    Color.green, Color.yellow, Color.cyan
                }[i];
            }

            // Display process name and burst time
            GameObject label = new GameObject("Label");
            label.transform.SetParent(block.transform, false);
            label.transform.localPosition = new Vector3(0, 1.0f, 0);

            TextMesh text = label.AddComponent<TextMesh>();
            text.text = processNames[i] + "\nBT: " + burstTimes[i];
            text.fontSize = 48;
            text.characterSize = 0.025f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;

            processes[i] = block;
        }
    }

   
IEnumerator RunFCFS()
{
    // Wait so the user can see all processes in the queue
    yield return new WaitForSeconds(2f);

    // Execute processes in FCFS arrival order
    for (int i = 0; i < processes.Length; i++)
    {
        GameObject currentProcess = processes[i];

        if (currentProcess == null)
            continue;

        Debug.Log(processNames[i] + " is running on the CPU");

        Vector3 start = currentProcess.transform.localPosition;
        Vector3 end = cpu.localPosition;

        float duration = burstTimes[i];
        float elapsed = 0f;

        // Move the current process toward the CPU
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            currentProcess.transform.localPosition = Vector3.Lerp(
                start,
                end,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        // Place it exactly at the CPU position
        currentProcess.transform.localPosition = end;

        Debug.Log(processNames[i] + " finished");

        // Let the user see that the process has finished
        yield return new WaitForSeconds(0.5f);

        // Remove the completed process and its label
        Destroy(currentProcess);
        processes[i] = null;

        // Brief pause before the next process starts
        yield return new WaitForSeconds(0.5f);
    }

    Debug.Log("FCFS scheduling completed!");
}

}
