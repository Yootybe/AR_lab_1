using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARRaycastManager))]

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _spawnedObjectPrehabs;
    [SerializeField] private GameObject _targetMarkerPrehab;
    [SerializeField] private GameObject[] _uiScreens;

    // Each interaction manager state correlate to UI Screens:
    // Default (0) - _uiScreens[0], SpawnObject(1) - _uiScreens[1] etc.

    private enum InterractionManagerState { Default, SpawnObject }
    private InterractionManagerState _currentState;
    private UnityAction[] _stateInitializationAction;
    private int _spawnedObjectType = -1;

    private ARRaycastManager _aRRaycastManager;
    private List<ARRaycastHit> _raycastHits;
    private GameObject _targetMarker;

    private List<Transform> _gameObjectsPos;

    private void Awake()
    {
        _aRRaycastManager = GetComponent<ARRaycastManager>();
        _raycastHits = new List<ARRaycastHit>();

        _stateInitializationAction = new UnityAction[Enum.GetNames(typeof(InterractionManagerState)).Length];
        _stateInitializationAction[(int)InterractionManagerState.Default] = InitializeDefaultScreen;
        _stateInitializationAction[(int)InterractionManagerState.Default] = InitializeObjectSpawner;

        _gameObjectsPos = new List<Transform>();
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

                default:
                    break;
            }
        }
        //ProcessFirstTouch(Input.GetTouch(0));
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
        if (_gameObjectsPos.Count == 0)
        {
            _aRRaycastManager.Raycast(touch.position, _raycastHits, TrackableType.Planes);
            GameObject curObj = Instantiate(_spawnedObjectPrehabs[_spawnedObjectType], _raycastHits[0].pose.position, _spawnedObjectPrehabs[_spawnedObjectType].transform.rotation);
            _gameObjectsPos.Add(curObj.transform);
        }
        else
        {
            bool objsClose = false;
            _aRRaycastManager.Raycast(touch.position, _raycastHits, TrackableType.Planes);
            foreach (Transform pos in _gameObjectsPos)
            {
                if (Vector3.Distance(pos.position, _raycastHits[0].pose.position) < 0.75f)
                {
                    objsClose = true;
                    break;
                }
            }
            
            if (!objsClose)
            {
                GameObject curObj = Instantiate(_spawnedObjectPrehabs[_spawnedObjectType], _raycastHits[0].pose.position, _spawnedObjectPrehabs[_spawnedObjectType].transform.rotation);
                _gameObjectsPos.Add(curObj.transform);
            }
            else
            {
                Debug.Log("Can't create object because it too close to other object");
            }
        }
        
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

    public void SelectSpawnedObjectType(int objectType)
    {
        _spawnedObjectType = objectType;
    }
}
