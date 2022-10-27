using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARRaycastManager))]

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private Camera _arCamera;
    [SerializeField] private GameObject[] _spawnedObjectPrehabs;
    [SerializeField] private GameObject _targetMarkerPrehab;
    [SerializeField] private GameObject[] _uiScreens;

    // Each interaction manager state correlate to UI Screens:
    // Default (0) - _uiScreens[0], SpawnObject(1) - _uiScreens[1] etc.

    private enum InterractionManagerState { Default, SpawnObject, SelectObject }
    private InterractionManagerState _currentState;
    private UnityAction[] _stateInitializationAction;
    private int _spawnedObjectType = -1;
    private int _spawnedObjectCount = 0;

    private ARRaycastManager _aRRaycastManager;
    private List<ARRaycastHit> _raycastHits;
    private GameObject _targetMarker;

    private void Awake()
    {
        _aRRaycastManager = GetComponent<ARRaycastManager>();
        _raycastHits = new List<ARRaycastHit>();

        _stateInitializationAction = new UnityAction[Enum.GetNames(typeof(InterractionManagerState)).Length];
        _stateInitializationAction[(int)InterractionManagerState.Default] = InitializeDefaultScreen;
        _stateInitializationAction[(int)InterractionManagerState.SpawnObject] = InitializeObjectSpawner;
        _stateInitializationAction[(int)InterractionManagerState.SelectObject] = InitializeObjectSelection;
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
        UpdateUIScreens();
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            bool isOverUI = touch.position.IsPointOverUIObject();

            switch (_currentState)
            {
                case InterractionManagerState.SpawnObject:
                    ProcessTouchSpawnObject(touch, isOverUI);
                    break;

                case InterractionManagerState.SelectObject:
                    ProcessTouchSelectObject(touch, isOverUI);
                    break;
                default:
                    break;
            }
        }
        //ProcessFirstTouch(Input.GetTouch(0));
    }

    private void ProcessTouchSelectObject(Touch touch, bool isOverUI)
    {
        if (touch.phase == TouchPhase.Began)
        {
            if (!isOverUI)
            {
                TrySelectObject(touch.position);
            }
        }
    }


    private GameObject lastSelectedObject;
    private void TrySelectObject(Vector2 position)
    {
        // fire a ray from the camera to the target screen position
        Ray ray = _arCamera.ScreenPointToRay(position);
        RaycastHit hitObject;

        if (Physics.Raycast(ray, out hitObject))
        {
            if (hitObject.collider.CompareTag("SpawnedObject"))
            {
                // if we hit spawned object tag, try to get SpawnedObject from it and descriptionScreen from UI screen
                GameObject selectedObject = hitObject.collider.gameObject;
                SpawnedObject objectDescription = selectedObject.GetComponent<SpawnedObject>();

                if (!objectDescription)
                    throw new MissingComponentException(objectDescription.GetType().Name + " component not found!");

                SpawnedObjectDescriptionScreen descScreen = _uiScreens[(int)InterractionManagerState.SelectObject].GetComponent<SpawnedObjectDescriptionScreen>();
                if (!descScreen)
                    throw new MissingComponentException(descScreen.GetType().Name + " component not found!");

                // then we call description screen to show info for the targeted object
                descScreen.ShowObjectDescription(objectDescription);

                lastSelectedObject = selectedObject;
            }
        }
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

        spObj.Rotation = newObject.transform.rotation;
        spObj.Scale = newObject.transform.localScale;

        spObj.GiveNumber(++_spawnedObjectCount);
    }

    private void InitializeDefaultScreen()
    {
        Debug.Log("Initialize default screen");
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

    public void showNextDescription()
    {
        SpawnedObjectDescriptionScreen descScreen = _uiScreens[(int)InterractionManagerState.SelectObject].GetComponent<SpawnedObjectDescriptionScreen>();
        if (!descScreen)
            throw new MissingComponentException(descScreen.GetType().Name + " component not found!");

        // then we call description screen to show info for the targeted object
        descScreen.UpdateDescription(lastSelectedObject.GetComponent<SpawnedObject>(), true);
    }

    public void showPrevDescription()
    {
        SpawnedObjectDescriptionScreen descScreen = _uiScreens[(int)InterractionManagerState.SelectObject].GetComponent<SpawnedObjectDescriptionScreen>();
        if (!descScreen)
            throw new MissingComponentException(descScreen.GetType().Name + " component not found!");

        // then we call description screen to show info for the targeted object
        descScreen.UpdateDescription(lastSelectedObject.GetComponent<SpawnedObject>(), false);
    }

    public void RotateClockwise()
    {
        SpawnedObjectDescriptionScreen descScreen = _uiScreens[(int)InterractionManagerState.SelectObject].GetComponent<SpawnedObjectDescriptionScreen>();
        if (!descScreen)
            throw new MissingComponentException(descScreen.GetType().Name + " component not found!");

        float angle = lastSelectedObject.GetComponent<SpawnedObject>().RotationAngle;
        if (angle == 330.0f)
            angle = 0.0f;
        else
            angle += 30.0f;

        lastSelectedObject.GetComponent<SpawnedObject>().RotationAngle = angle;

        lastSelectedObject.transform.Rotate(30.0f, 0.0f, 0.0f);

        descScreen.UpdateRotationText(angle);

        lastSelectedObject.GetComponent<SpawnedObject>().Rotation = lastSelectedObject.transform.rotation;
    }

    public void RotateAntiClockwise()
    {
        SpawnedObjectDescriptionScreen descScreen = _uiScreens[(int)InterractionManagerState.SelectObject].GetComponent<SpawnedObjectDescriptionScreen>();
        if (!descScreen)
            throw new MissingComponentException(descScreen.GetType().Name + " component not found!");

        float angle = lastSelectedObject.GetComponent<SpawnedObject>().RotationAngle;
        if (angle == 0.0f)
            angle = 330.0f;
        else
            angle -= 30.0f;

        lastSelectedObject.GetComponent<SpawnedObject>().RotationAngle = angle;

        lastSelectedObject.transform.Rotate(-30.0f, 0.0f, 0.0f);

        descScreen.UpdateRotationText(angle);

        lastSelectedObject.GetComponent<SpawnedObject>().Rotation = lastSelectedObject.transform.rotation;
    }

    public void Upscale()
    {
        SpawnedObjectDescriptionScreen descScreen = _uiScreens[(int)InterractionManagerState.SelectObject].GetComponent<SpawnedObjectDescriptionScreen>();
        if (!descScreen)
            throw new MissingComponentException(descScreen.GetType().Name + " component not found!");

        lastSelectedObject.transform.localScale = new Vector3(lastSelectedObject.transform.localScale.x * 1.2f,
            lastSelectedObject.transform.localScale.y * 1.2f, lastSelectedObject.transform.localScale.z * 1.2f);

        descScreen.UpdateScaleText(lastSelectedObject.transform.localScale);

        lastSelectedObject.GetComponent<SpawnedObject>().Scale = lastSelectedObject.transform.localScale;
    }

    public void Downscale()
    {
        SpawnedObjectDescriptionScreen descScreen = _uiScreens[(int)InterractionManagerState.SelectObject].GetComponent<SpawnedObjectDescriptionScreen>();
        if (!descScreen)
            throw new MissingComponentException(descScreen.GetType().Name + " component not found!");

        lastSelectedObject.transform.localScale = new Vector3(lastSelectedObject.transform.localScale.x * 0.8f,
            lastSelectedObject.transform.localScale.y * 0.8f, lastSelectedObject.transform.localScale.z * 0.8f);

        descScreen.UpdateScaleText(lastSelectedObject.transform.localScale);

        lastSelectedObject.GetComponent<SpawnedObject>().Scale = lastSelectedObject.transform.localScale;
    }
}
