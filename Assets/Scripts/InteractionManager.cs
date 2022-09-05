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

    private void Awake()
    {
        _aRRaycastManager = GetComponent<ARRaycastManager>();
        _raycastHits = new List<ARRaycastHit>();
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
        if (touch.phase == TouchPhase.Began)
            SpawnObject(touch);
    }

    private void SpawnObject(Touch touch)
    {
        _aRRaycastManager.Raycast(touch.position, _raycastHits, TrackableType.Planes);
        Instantiate(_spawnedObjectPrehab, _raycastHits[0].pose.position, _spawnedObjectPrehab.transform.rotation);
    }
}
