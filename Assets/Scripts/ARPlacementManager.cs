
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    // Finds the real-world surfaces detected by AR Foundation
    public ARRaycastManager raycastManager;

    // The SchedulingLab prefab we want to place
    public GameObject schedulingLabPrefab;

    // Stores the results of each AR raycast
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Prevents placing multiple labs accidentally
    private bool labPlaced = false;

    void Update()
    {
        // Wait until the user touches the screen
        if (Input.touchCount == 0 || labPlaced)
            return;

        Touch touch = Input.GetTouch(0);

        // Only place the lab when a new touch begins
        if (touch.phase == TouchPhase.Began)
        {
            // Check whether the touch hits a detected horizontal plane
            if (raycastManager.Raycast(
                touch.position,
                hits,
                TrackableType.PlaneWithinPolygon))
            {
                // Get the position and rotation of the detected surface
                Pose hitPose = hits[0].pose;

                // Create the SchedulingLab at that location
                Instantiate(
                    schedulingLabPrefab,
                    hitPose.position,
                    hitPose.rotation
                );

                // Allow only one placement for now
                labPlaced = true;

                Debug.Log("Scheduling Lab placed successfully!");
            }
        }
    }
}
