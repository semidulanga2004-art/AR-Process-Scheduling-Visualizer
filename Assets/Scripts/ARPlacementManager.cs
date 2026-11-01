
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class ARPlacementManager : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public GameObject schedulingLabPrefab;
    public VoiceOverController voiceOverController;
    public TMP_Text instructionText;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool labPlaced = false;

    void Update()
    {
        if (Input.touchCount == 0 || labPlaced)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            if (raycastManager.Raycast(
                touch.position,
                hits,
                TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;

                // Place the SchedulingLab
                GameObject placedLab = Instantiate(
                    schedulingLabPrefab,
                    hitPose.position,
                    hitPose.rotation
                );

                labPlaced = true;
                                // Hide the surface instruction message
                if (instructionText != null)
                {
                    instructionText.gameObject.SetActive(false);
                }
                // Hide all detected plane outlines
                if (planeManager != null)
                {
                    foreach (ARPlane plane in planeManager.trackables)
                    {
                        plane.gameObject.SetActive(false);
                    }

                    // Stop further plane detection
                    planeManager.enabled = false;
                }

                Debug.Log("Scheduling Lab placed!");
                // Start the selected algorithm's voice-over
                if (voiceOverController != null)
                {
                    voiceOverController.StartVoiceOver();
                }
                else
                {
                    Debug.LogError("VoiceOverController not assigned!");
                }

                // Start the selected algorithm
                string algorithm = SceneNavigation.selectedAlgorithm;
                // Display the table for the selected algorithm
                if (SchedulingTableUI.Instance != null)
                {
                    SchedulingTableUI.Instance.InitializeTable(algorithm);
                }
                else
                {
                    Debug.LogError("SchedulingTableUI not found!");
                }

                if (algorithm == "FCFS")
                {
                    FCFSSimulation fcfs =
                        placedLab.GetComponent<FCFSSimulation>();

                    if (fcfs != null)
                        fcfs.StartSimulation();
                    else
                        Debug.LogError("FCFS script not found!");
                }
                else if (algorithm == "Priority")
                {
                    PriorityScheduling priority =
                        placedLab.GetComponent<PriorityScheduling>();

                    if (priority != null)
                        priority.StartSimulation();
                    else
                        Debug.LogError("Priority script not found!");
                }
                    else if (algorithm == "SJF")
                {
                        SJFSimulation sjf =
                        placedLab.GetComponent<SJFSimulation>();

                     if (sjf != null)
                                sjf.StartSimulation();
                     else
                    Debug.LogError("SJF script not found!");
                }
                else if (algorithm == "RoundRobin")
                {
                    RoundRobinSimulation rr =
                    placedLab.GetComponent<RoundRobinSimulation>();

                    if (rr != null)
                          rr.StartSimulation();
                    else
                    Debug.LogError("Round Robin script not found!");
                }
                
                    else if (algorithm == "SRTF")
                    {
                        SRTFSimulation srtf =
                            placedLab.GetComponent<SRTFSimulation>();

                        if (srtf != null)
                        {
                            srtf.StartSimulation();
                        }
                        else
                        {
                            Debug.LogError("SRTF script not found on SchedulingLab!");
                        }
                    }

                else
                {
                    Debug.LogWarning(
                        "This algorithm has not been connected yet: "
                        + algorithm
                    );
                }
            }
        }
    }
    void OnEnable()
{
    if (planeManager != null)
    {
        planeManager.planesChanged += OnPlanesChanged;
    }
}

void OnDisable()
{
    if (planeManager != null)
    {
        planeManager.planesChanged -= OnPlanesChanged;
    }
}

void OnPlanesChanged(ARPlanesChangedEventArgs args)
{
    if (labPlaced || instructionText == null)
        return;

    if (args.added.Count > 0 || args.updated.Count > 0)
{
    instructionText.text =
        "Surface Identified!\nTap a detected surface to start learning.";

    instructionText.color = new Color32(46, 173, 112, 255);
}
}
}
