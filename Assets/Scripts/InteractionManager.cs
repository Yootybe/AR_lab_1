using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
[RequireComponent(typeof(ARFaceManager))]
[RequireComponent(typeof(ARAnchorManager))]

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private Camera _arCamera;
    [SerializeField] private GameObject[] _spawnedObjectPrehabs;
    [SerializeField] private GameObject _targetMarkerPrehab;
    [SerializeField] private GameObject[] _uiScreens;

    // Each interaction manager state correlate to UI Screens:
    // Default (0) - _uiScreens[0], SpawnObject(1) - _uiScreens[1] etc.

    private enum InterractionManagerState { Default, SpawnObject, SelectObject, ImageTracker, FaceMask }
    private InterractionManagerState _currentState;
    private UnityAction[] _stateInitializationAction;
    private int _spawnedObjectType = -1;
    private int _spawnedObjectCount = 0;
    private bool _foundPlanes = false;

    private ARRaycastManager _aRRaycastManager;
    private List<ARRaycastHit> _raycastHits;
    private ARPlaneManager _aRPlaneManager;
    private ARAnchorManager _aRAnchorManager;
    private ARFaceManager _aRFaceManager;
    private GameObject _targetMarker;
    private GameObject _selectedObject;

    private void Awake()
    {
        _aRAnchorManager = GetComponent<ARAnchorManager>();
        _aRAnchorManager.anchorsChanged += OnAnchorsChanged;

        _aRRaycastManager = GetComponent<ARRaycastManager>();
        _raycastHits = new List<ARRaycastHit>();

        // setup plane manager and subscribe to planes changed event
        _aRPlaneManager = GetComponent<ARPlaneManager>();
        _aRPlaneManager.planesChanged += OnPlanesChanged;

        _aRFaceManager = GetComponent<ARFaceManager>();

        _stateInitializationAction = new UnityAction[Enum.GetNames(typeof(InterractionManagerState)).Length];
        _stateInitializationAction[(int)InterractionManagerState.Default] = InitializeDefaultScreen;
        _stateInitializationAction[(int)InterractionManagerState.SpawnObject] = InitializeObjectSpawner;
        _stateInitializationAction[(int)InterractionManagerState.SelectObject] = InitializeObjectSelection;
        _stateInitializationAction[(int)InterractionManagerState.ImageTracker] = InitializeImageTracker;
        _stateInitializationAction[(int)InterractionManagerState.FaceMask] = InitializeFaceMask;
    }

    private void InitializeFaceMask()
    {
        _aRRaycastManager.enabled = false;
        _aRAnchorManager.enabled = false;
        ShowPlanes(false);
        ShowFrontalCamera(true);
        ShowFaces(true);
    }

    private void ShowFaces(bool state)
    {
        foreach (ARFace face in _aRFaceManager.trackables)
        {
            face.gameObject.SetActive(state);
        }
        _aRFaceManager.enabled = state;
    }

    private void ShowFrontalCamera(bool state)
    {
        ARCameraManager cameraManager = _arCamera.GetComponent<ARCameraManager>();
        if(!cameraManager)
        {
            throw new MissingComponentException(cameraManager.GetType().Name + " component not found");
        }

        if(state)
        {
            cameraManager.requestedFacingDirection = CameraFacingDirection.User;
        }
        else
        {
            cameraManager.requestedFacingDirection = CameraFacingDirection.World;
        }
    }

    private void InitializeImageTracker()
    {
        ImageTracker tracker = _uiScreens[(int)InterractionManagerState.ImageTracker].GetComponent<ImageTracker>();

        if (!tracker)
            throw new MissingComponentException(tracker.GetType().Name + " component not found!");

        ShowPlanes(false);
        tracker.Initialize();
    }

    private void ShowPlanes(bool state)
    {
        foreach (ARPlane plane in _aRPlaneManager.trackables)
        {
            plane.gameObject.SetActive(state);
        }

        _aRPlaneManager.enabled = state;
    }

    private void OnAnchorsChanged(ARAnchorsChangedEventArgs args)
    {
        foreach (ARAnchor anchor in args.added)
        {
            Debug.Log("Added anchor " + anchor.name);
        }

        foreach (ARAnchor anchor in args.updated)
        {
            Debug.Log("Updated anchor " + anchor.name);
        }

        foreach (ARAnchor anchor in args.removed)
        {
            Debug.Log("Removed anchor " + anchor.name);
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach (ARPlane plane in args.added)
        {
            Debug.Log("Added plane " + plane.name);
            if (!_foundPlanes)
            {
                _foundPlanes = true;
                UpdateUIScreens();
            }
        }

        foreach (ARPlane plane in args.updated)
        {
            Debug.Log("Updated plane " + plane.name);
        }

        foreach (ARPlane plane in args.removed)
        {
            Debug.Log("Removed plane " + plane.name);
        }
    }

    private void Start()
    {
        // Create target marker
        _targetMarker = Instantiate(
            original: _targetMarkerPrehab,
            position: Vector3.zero,
            rotation: _targetMarkerPrehab.transform.rotation
        );

        _targetMarker.SetActive(false);

        // Reset current state
        _currentState = InterractionManagerState.Default;
        UpdateUIScreens();

        // hide default screen until no planes found
        _uiScreens[(int)InterractionManagerState.Default].SetActive(false);
    }

    private void UpdateUIScreens()
    {
        // Hide every UI screen
        foreach (GameObject uiObject in _uiScreens)
        {
            uiObject.SetActive(false);
        }

        // Show the UI screen for the current state
        _uiScreens[(int)_currentState].SetActive(true);

        // Call initialization function of the current UI screen
        _stateInitializationAction[(int)_currentState]();
    }

    public void DisplayUIScreen(int screenNymber)
    {
        _currentState = (InterractionManagerState) screenNymber;
        UpdateUIScreens();
    }

    public void DisplayDefaultScreen()
    {
        _currentState = InterractionManagerState.Default;
        _targetMarker.SetActive(false);
        ShowPlanes(true);
        ShowFaces(false);
        UpdateUIScreens();
    }

    private void Update()
    {
        if (!_foundPlanes)
            return;

        if (Input.touchCount > 0)
        {
            Touch touch1 = Input.GetTouch(0);
            bool isOverUI = touch1.position.IsPointOverUIObject();

            switch (_currentState)
            {
                case InterractionManagerState.SpawnObject:
                    ProcessTouchSpawnObject(touch1, isOverUI);
                    break;

                case InterractionManagerState.SelectObject:
                    // if there;s only one touch, we try to select object
                    if (Input.touchCount == 1)
                    {
                        // try to select object, if it wasn't possible, try to move it
                        if (!ProcessTouchSelectObject(touch1, isOverUI))
                        {
                            MoveSelectedObject(touch1);
                        }
                    }
                    else if (Input.touchCount == 2)
                    {
                        RotateSelectedObject(touch1, Input.GetTouch(1));
                    }
                    break;
                default:
                    break;
            }
        }
        //ProcessFirstTouch(Input.GetTouch(0));
    }

    private void RotateSelectedObject(Touch touch1, Touch touch2)
    {
        if (!_selectedObject)
            return;

        if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float distance = Vector2.Distance(touch1.position, touch2.position);
            float distancePrev = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
            float delta = distance - distancePrev;

            if (Mathf.Abs(delta) > 0.0f)
                delta *= 0.1f;
            else
                delta *= -0.1f;

            _selectedObject.transform.rotation *= Quaternion.Euler(0.0f, delta, 0.0f);
        }
    }

    private void MoveSelectedObject(Touch touch1)
    {
        if (!_selectedObject)
            return;

        if (touch1.phase == TouchPhase.Moved)
        {
            _aRRaycastManager.Raycast(touch1.position, _raycastHits, TrackableType.Planes);
            _selectedObject.transform.position = _raycastHits[0].pose.position;
        }
    }

    private bool ProcessTouchSelectObject(Touch touch, bool isOverUI)
    {
        if (touch.phase == TouchPhase.Began)
        {
            if (!isOverUI)
            {
                return TrySelectObject(touch.position);
            }
        }
        return false;
    }

    private bool TrySelectObject(Vector2 position)
    {
        // fire a ray from the camera to the target screen position
        Ray ray = _arCamera.ScreenPointToRay(position);
        RaycastHit hitObject;

        if (Physics.Raycast(ray, out hitObject))
        {
            if (hitObject.collider.CompareTag("SpawnedObject"))
            {
                // if we hit spawned object tag, try to get SpawnedObject from it and descriptionScreen from UI screen
                _selectedObject = hitObject.collider.gameObject;
                SpawnedObject objectDescription = _selectedObject.GetComponent<SpawnedObject>();

                if (!objectDescription)
                    throw new MissingComponentException(objectDescription.GetType().Name + " component not found!");

                SpawnedObjectDescriptionScreen descScreen = _uiScreens[(int)InterractionManagerState.SelectObject].GetComponent<SpawnedObjectDescriptionScreen>();
                if (!descScreen)
                    throw new MissingComponentException(descScreen.GetType().Name + " component not found!");

                // then we call description screen to show info for the targeted object
                descScreen.ShowObjectDescription(objectDescription);

                return true;
            }
        }
        return false;
    }

    private void ProcessTouchSpawnObject(Touch touch, bool overUI)
    {
        // If none are ye selected, return

        if (_spawnedObjectType == -1)
            return;


        if (touch.phase == TouchPhase.Began)
        {
            if (!overUI)
            {
                ShowMarker(true);
                MoveMarker(touch.position);
            }
            
            //SpawnObject(touch);
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            MoveMarker(touch.position);
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            SpawnObject(touch);
            ShowMarker(false);
        }
            
    }

    private void ShowMarker(bool value)
    {
        _targetMarker.SetActive(value);
    }

    private void MoveMarker(Vector2 touchPosition)
    {
        _aRRaycastManager.Raycast(
            screenPoint: touchPosition,
            hitResults: _raycastHits,
            trackableTypes: TrackableType.Planes
            );
        _targetMarker.transform.position = _raycastHits[0].pose.position;
    }

    private void SpawnObject(Touch touch)
    {
        _aRRaycastManager.Raycast(touch.position, _raycastHits, TrackableType.Planes);
        GameObject newObject = Instantiate(_spawnedObjectPrehabs[_spawnedObjectType], _raycastHits[0].pose.position, _spawnedObjectPrehabs[_spawnedObjectType].transform.rotation);

        
        //give number to the new spawned object
        SpawnedObject spObj = newObject.GetComponent<SpawnedObject>();
        if (!spObj)
            throw new MissingComponentException(spObj.GetType().Name + " component not found!");

        spObj.GiveNumber(++_spawnedObjectCount);

        // add anchor to the object
        AnchorObject(newObject);
    }

    private void AnchorObject(GameObject obj)
    {
        if (!obj.GetComponent<ARAnchor>())
        {
            obj.AddComponent<ARAnchor>();
        }
    }

    private void InitializeDefaultScreen()
    {
        Debug.Log("Initialize default screen");
        _selectedObject = null;
        _aRPlaneManager.enabled = true;
        _aRRaycastManager.enabled = true;
        _aRAnchorManager.enabled = true;
        ShowMarker(false);
    }

    private void InitializeObjectSpawner()
    {
        Debug.Log("Initialize spawner");
        _spawnedObjectType = -1;
    }

    private void InitializeObjectSelection()
    {
        SpawnedObjectDescriptionScreen descScreen = _uiScreens[(int)InterractionManagerState.SelectObject].GetComponent<SpawnedObjectDescriptionScreen>();
        if (!descScreen)
            throw new MissingComponentException(descScreen.GetType().Name + " component not found!");

        descScreen.InitializeScreen();
    }

    public void SelectSpawnedObjectType(int objectType)
    {
        _spawnedObjectType = objectType;
    }

    public void ProcessFingerTap(int tapCount)
    {
        if (tapCount == 2)
        DisplayDefaultScreen();
    }
}
