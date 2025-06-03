using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class PlaceOnPlane : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown categoryDropdown;
    [SerializeField] private TMP_Dropdown variationDropdown;
    [SerializeField] private List<GameObject> sofas;
    [SerializeField] private List<GameObject> beds;
    [SerializeField] private List<GameObject> tables;
    [SerializeField] private List<GameObject> chairs;

    private ARRaycastManager raycastManager;
    private List<GameObject> placedObjects = new List<GameObject>();
    private GameObject selectedObject;
    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f;
    private float initialPinchDistance;
    private Vector3 initialScale;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Start()
    {
        
        categoryDropdown.onValueChanged.AddListener(UpdateVariationDropdown);
        UpdateVariationDropdown(categoryDropdown.value);
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleTouch(touch.position);
            }
        }

        if (selectedObject != null)
        {
            HandleGestures();
        }
    }

    public void PlaceObject()
    {
        if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            GameObject prefabToInstantiate = GetSelectedPrefab();

            if (prefabToInstantiate != null)
            {
                GameObject newObject = Instantiate(prefabToInstantiate);

                // Set position
                newObject.transform.position = hitPose.position;

                // Extract only Y-axis rotation from AR plane
                float yRotation = hitPose.rotation.eulerAngles.y;
                
                // Keep prefab’s original rotation but override only the Y-axis
                Quaternion prefabRotation = prefabToInstantiate.transform.rotation;
                Quaternion yRotationOnly = Quaternion.Euler(0, hitPose.rotation.eulerAngles.y, 0);

                // Apply only Y-axis rotation while keeping the prefab’s original X and Z rotations
                Quaternion finalRotation = Quaternion.Euler(
                    prefabRotation.eulerAngles.x,  // Keep original X rotation
                    yRotationOnly.eulerAngles.y,   // Use AR plane’s Y rotation
                    prefabRotation.eulerAngles.z   // Keep original Z rotation
                );

                newObject.transform.rotation = finalRotation;


                newObject.tag = "PlacedObject";
                placedObjects.Add(newObject);

                // Deselect any previously selected object
                selectedObject = null;
            }
        }
    }

    public void UndoLastPlacement()
    {
        if (placedObjects.Count > 0)
        {
            GameObject lastObject = placedObjects[placedObjects.Count - 1];
            placedObjects.RemoveAt(placedObjects.Count - 1);
            Destroy(lastObject);
        }
    }

    public void ClearPlacedObjects()
    {
        foreach (GameObject obj in placedObjects)
        {
            Destroy(obj);
        }
        placedObjects.Clear();
    }


    void HandleTouch(Vector2 touchPosition)
{
    Ray ray = Camera.main.ScreenPointToRay(touchPosition);
    if (Physics.Raycast(ray, out RaycastHit hit))
    {
        if (hit.collider.gameObject.CompareTag("PlacedObject"))
        {
            float timeSinceLastTap = Time.time - lastTapTime;
            lastTapTime = Time.time;

            if (timeSinceLastTap < doubleTapThreshold)
            {
                // DELETE on Double Tap
                placedObjects.Remove(hit.collider.gameObject);
                Destroy(hit.collider.gameObject);
            }
            else
            {
                // SELECT Object for Manipulation
                selectedObject = hit.collider.gameObject;
            }
        }
    }
}


    void HandleGestures()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    selectedObject.transform.position = hitPose.position;
                }
            }
        }

        if (Input.touchCount == 2 && selectedObject != null)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // Scaling
            float currentPinchDistance = Vector2.Distance(touch1.position, touch2.position);
            if (initialPinchDistance == 0)
            {
                initialPinchDistance = currentPinchDistance;
                initialScale = selectedObject.transform.localScale;
            }
            else
            {
                float scaleFactor = currentPinchDistance / initialPinchDistance;
                selectedObject.transform.localScale = initialScale * scaleFactor;
            }

            // Rotation
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;
            
            float prevAngle = Mathf.Atan2(touch1PrevPos.y - touch2PrevPos.y, touch1PrevPos.x - touch2PrevPos.x) * Mathf.Rad2Deg;
            float currentAngle = Mathf.Atan2(touch1.position.y - touch2.position.y, touch1.position.x - touch2.position.x) * Mathf.Rad2Deg;
            
            float angleDifference = currentAngle - prevAngle;
            Quaternion currentRotation = selectedObject.transform.rotation;
            Quaternion newRotation = Quaternion.Euler(
                currentRotation.eulerAngles.x,  // Keep X rotation
                currentRotation.eulerAngles.y - angleDifference,  // Rotate only Y
                currentRotation.eulerAngles.z   // Keep Z rotation
            );

            selectedObject.transform.rotation = newRotation;

        }
        else
        {
            initialPinchDistance = 0;
        }
    }

    public void UpdateVariationDropdown(int categoryIndex)
    {
        variationDropdown.ClearOptions();
        List<string> options = new List<string>();

        switch (categoryIndex)
        {
            case 0: for (int i = 0; i < sofas.Count; i++) options.Add("Sofa " + (i + 1)); break;
            case 1: for (int i = 0; i < beds.Count; i++) options.Add("Bed " + (i + 1)); break;
            case 2: for (int i = 0; i < tables.Count; i++) options.Add("Table " + (i + 1)); break;
            case 3: for (int i = 0; i < chairs.Count; i++) options.Add("Chair " + (i + 1)); break;
        }

        variationDropdown.AddOptions(options);
        variationDropdown.value = 0;
    }

    GameObject GetSelectedPrefab()
    {
        int categoryIndex = categoryDropdown.value;
        int variationIndex = variationDropdown.value;

        switch (categoryIndex)
        {
            case 0: return variationIndex < sofas.Count ? sofas[variationIndex] : null;
            case 1: return variationIndex < beds.Count ? beds[variationIndex] : null;
            case 2: return variationIndex < tables.Count ? tables[variationIndex] : null;
            case 3: return variationIndex < chairs.Count ? chairs[variationIndex] : null;
            default: return null;
        }
    }
}
