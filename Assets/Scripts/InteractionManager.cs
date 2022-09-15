using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private GameObject _spawnedObjectPrehab;

    private ARRaycastManager _aRRaycastManager;
    private List<ARRaycastHit> _raycastHits;
    private List<GameObject> _gameObjects;

    private bool firstTouch = false;
    private int clickCounter;

    private void Awake()
    {
        _aRRaycastManager = GetComponent<ARRaycastManager>();
        _raycastHits = new List<ARRaycastHit>();
        _gameObjects = new List<GameObject>();
        clickCounter = 0;
    }

    private void Start()
    {
                
    }

    private void Update()
    {
        if (Input.touchCount > 0)
            ProcessFirstTouch(Input.GetTouch(0));
    }

    private void ProcessFirstTouch(Touch touch)
    {
        if (!firstTouch)
            firstTouch = true;

        if (touch.phase == TouchPhase.Began)
            SpawnObject(touch);
    }

    private void SpawnObject(Touch touch)
    {
        if (firstTouch && _gameObjects.Count == 0)
        {
            _aRRaycastManager.Raycast(touch.position, _raycastHits, TrackableType.Planes);
            _gameObjects.Add(Instantiate(_spawnedObjectPrehab, _raycastHits[0].pose.position, _spawnedObjectPrehab.transform.rotation));
        }

        clickCounter++;

        if (clickCounter == 3)
        {
            Destroy(_gameObjects[_gameObjects.Count - 1]);
            
            _aRRaycastManager.Raycast(touch.position, _raycastHits, TrackableType.Planes);
            _gameObjects.Add(Instantiate(_spawnedObjectPrehab, _raycastHits[0].pose.position, _spawnedObjectPrehab.transform.rotation));

            clickCounter = 0;
        }
    }
}
