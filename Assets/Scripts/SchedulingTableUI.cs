
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SchedulingTableUI : MonoBehaviour
{
    public static SchedulingTableUI Instance;

    public GameObject tablePanel;
    public Transform columnHeaders;
    public Transform tableContent;

    private Dictionary<string, GameObject> processRows =
        new Dictionary<string, GameObject>();

    private Color normalColor =
        new Color(0.15f, 0.18f, 0.23f, 0.95f);

    private Color highlightColor =
        new Color(0.1f, 0.45f, 0.25f, 1f);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        tablePanel.SetActive(false);
    }

    void ConfigureLayout()
    {
        // Configure column headings
        HorizontalLayoutGroup headerLayout =
            columnHeaders.GetComponent<HorizontalLayoutGroup>();

        if (headerLayout == null)
            headerLayout = columnHeaders.gameObject
                .AddComponent<HorizontalLayoutGroup>();

        headerLayout.spacing = 5;
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = true;
        headerLayout.childForceExpandHeight = false;

        LayoutElement headerElement =
            columnHeaders.GetComponent<LayoutElement>();

        if (headerElement == null)
            headerElement = columnHeaders.gameObject
                .AddComponent<LayoutElement>();

        headerElement.preferredHeight = 55;
        headerElement.flexibleHeight = 0;

        // Configure process rows container
        VerticalLayoutGroup contentLayout =
            tableContent.GetComponent<VerticalLayoutGroup>();

        if (contentLayout == null)
            contentLayout = tableContent.gameObject
                .AddComponent<VerticalLayoutGroup>();

        contentLayout.spacing = 5;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
    }

    public void InitializeTable(string algorithm)
    {
        ConfigureLayout();

        tablePanel.SetActive(true);

        ClearChildren(columnHeaders);
        ClearChildren(tableContent);

        processRows.Clear();

        string[] headers;
        string[,] data;

        if (algorithm == "Priority")
        {
            headers = new string[]
            {
                "Process", "Burst Time", "Priority"
            };

            data = new string[,]
            {
                { "P1", "4", "3" },
                { "P2", "3", "1" },
                { "P3", "2", "2" },
                { "P4", "5", "4" },
                { "P5", "1", "2" }
            };
        }
        else if (algorithm == "RoundRobin")
        {
            headers = new string[]
            {
                "Process", "Remaining Time", "Quantum"
            };

            data = new string[,]
            {
                { "P1", "4", "2" },
                { "P2", "3", "2" },
                { "P3", "2", "2" },
                { "P4", "5", "2" },
                { "P5", "1", "2" }
            };
        }
        
        else if (algorithm == "SRTF")
        {
            headers = new string[]
            {
                "Process", "Remaining", "Arrival", "Burst"
            };

            data = new string[,]
            {
                { "P1", "7", "0", "7" },
                { "P2", "4", "2", "4" },
                { "P3", "1", "4", "1" },
                { "P4", "4", "5", "4" },
                { "P5", "2", "6", "2" }
            };
        }

        else
        {
            headers = new string[]
            {
                "Process", "Burst Time", "Waiting Time", "Turnaround"
            };

            data = new string[,]
            {
                { "P1", "4", "0", "4" },
                { "P2", "3", "4", "7" },
                { "P3", "2", "7", "9" },
                { "P4", "5", "9", "14" },
                { "P5", "1", "14", "15" }
            };
        }

        // Create headings
        for (int i = 0; i < headers.Length; i++)
        {
            CreateCell(columnHeaders, headers[i], true);
        }

        // Create all five process rows
        for (int i = 0; i < data.GetLength(0); i++)
        {
            string processName = data[i, 0];

            GameObject row = new GameObject(
                processName,
                typeof(RectTransform),
                typeof(Image),
                typeof(HorizontalLayoutGroup),
                typeof(LayoutElement)
            );

            row.transform.SetParent(tableContent, false);

            Image background = row.GetComponent<Image>();
            background.color = normalColor;

            HorizontalLayoutGroup rowLayout =
                row.GetComponent<HorizontalLayoutGroup>();

            rowLayout.spacing = 5;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = false;

            LayoutElement rowElement =
                row.GetComponent<LayoutElement>();

            rowElement.minHeight = 65;
            rowElement.preferredHeight = 65;
            rowElement.flexibleHeight = 0;

            // Create cells in this row
            for (int j = 0; j < headers.Length; j++)
            {
                CreateCell(row.transform, data[i, j], false);
            }

            processRows.Add(processName, row);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            tablePanel.GetComponent<RectTransform>()
        );
    }

    void CreateCell(Transform parent, string value, bool isHeader)
    {
        GameObject cell = new GameObject(
            "Cell",
            typeof(RectTransform),
            typeof(TextMeshProUGUI),
            typeof(LayoutElement)
        );

        cell.transform.SetParent(parent, false);

        TextMeshProUGUI text =
            cell.GetComponent<TextMeshProUGUI>();

        text.text = value;
        text.fontSize = isHeader ? 22 : 24;
        text.fontStyle = isHeader
            ? FontStyles.Bold
            : FontStyles.Normal;

        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;

        LayoutElement element =
            cell.GetComponent<LayoutElement>();

        element.flexibleWidth = 1;
        element.minWidth = 0;
        element.preferredHeight = isHeader ? 55 : 65;
    }

    public void HighlightProcess(string processName)
    {
        foreach (var item in processRows)
        {
            Image background = item.Value.GetComponent<Image>();

            background.color = item.Key == processName
                ? highlightColor
                : normalColor;
        }
    }

    public void CompleteProcess(string processName)
    {
        if (processRows.ContainsKey(processName))
        {
            Destroy(processRows[processName]);
            processRows.Remove(processName);
        }
    }

    public void UpdateRemainingTime(string processName, int remaining)
    {
        if (processRows.ContainsKey(processName))
        {
            Transform row = processRows[processName].transform;

            TextMeshProUGUI text =
                row.GetChild(1).GetComponent<TextMeshProUGUI>();

            text.text = remaining.ToString();
        }
    }

    public void HideTable()
    {
        tablePanel.SetActive(false);
    }

    public void ShowTable()
    {
        tablePanel.SetActive(true);
    }

    void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}
