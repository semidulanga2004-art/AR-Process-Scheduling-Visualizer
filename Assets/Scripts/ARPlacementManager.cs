
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GameObject schedulingLabPrefab;

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

                Debug.Log("Scheduling Lab placed!");

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
}
