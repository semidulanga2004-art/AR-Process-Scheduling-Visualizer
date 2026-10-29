
using UnityEngine;
using System.Collections;

public class PriorityScheduling : MonoBehaviour
{
    public Transform cpu;

    private string[] processNames = { "P1", "P2", "P3" };
    private int[] burstTimes = { 4, 3, 2 };
    private int[] priorities = { 3, 1, 2 };

    private GameObject[] processes;

    void Start()
    {
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
        StartCoroutine(RunPriority());
    }

    void CreateProcesses()
    {
        processes = new GameObject[processNames.Length];

        for (int i = 0; i < processNames.Length; i++)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = processNames[i];
            block.transform.SetParent(transform, false);

            block.transform.localPosition =
                new Vector3(-0.6f + i * 0.3f, 0.15f, 0.4f);

            block.transform.localScale =
                new Vector3(0.18f, 0.18f, 0.18f);

            Renderer r = block.GetComponent<Renderer>();

            if (r != null)
            {
                r.material.color = new Color[]
                {
                    Color.green, Color.yellow, Color.cyan
                }[i];
            }

            GameObject label = new GameObject("Label");
            label.transform.SetParent(block.transform, false);
            label.transform.localPosition = new Vector3(0, 1f, 0);

            TextMesh text = label.AddComponent<TextMesh>();
            text.text = processNames[i]
                + "\nBT: " + burstTimes[i]
                + "\nPriority: " + priorities[i];

            text.fontSize = 48;
            text.characterSize = 0.025f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;

            processes[i] = block;
        }
    }

    IEnumerator RunPriority()
    {
        yield return new WaitForSeconds(2f);

        // Find the process with the highest priority.
        // A smaller priority number means higher priority.
        bool[] completed = new bool[processNames.Length];

        for (int count = 0; count < processes.Length; count++)
        {
            int bestIndex = -1;

            for (int i = 0; i < processes.Length; i++)
            {
                if (completed[i] || processes[i] == null)
                    continue;

                if (bestIndex == -1 ||
                    priorities[i] < priorities[bestIndex])
                {
                    bestIndex = i;
                }
            }

            if (bestIndex == -1)
                break;

            GameObject currentProcess = processes[bestIndex];

            Debug.Log(processNames[bestIndex]
                + " is running on the CPU. Priority: "
                + priorities[bestIndex]);

            Vector3 start = currentProcess.transform.localPosition;
            Vector3 end = cpu.localPosition;

            float duration = burstTimes[bestIndex];
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

            Debug.Log(processNames[bestIndex] + " finished.");

            yield return new WaitForSeconds(0.5f);

            Destroy(currentProcess);
            processes[bestIndex] = null;
            completed[bestIndex] = true;

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Priority Scheduling completed!");
    }
}
