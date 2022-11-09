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
    private GameObject _selectedObject;

    private SpawnedObjectDescriptionScreen _descriptionScreen;

    private List<Vector2> _points;

    private void Awake()
    {
        _aRRaycastManager = GetComponent<ARRaycastManager>();
        _raycastHits = new List<ARRaycastHit>();

        _stateInitializationAction = new UnityAction[Enum.GetNames(typeof(InterractionManagerState)).Length];
        _stateInitializationAction[(int)InterractionManagerState.Default] = InitializeDefaultScreen;
        _stateInitializationAction[(int)InterractionManagerState.SpawnObject] = InitializeObjectSpawner;
        _stateInitializationAction[(int)InterractionManagerState.SelectObject] = InitializeObjectSelection;

        _points = new List<Vector2>();
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
        _currentState = (InterractionManagerState)screenNymber;
        UpdateUIScreens();
    }

    public void DisplayDefaultScreen()
    {
        _currentState = InterractionManagerState.Default;
        _targetMarker.SetActive(false);
        UpdateUIScreens();
    }

    bool fingerStartMove = false;

    private void Update()
    {
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
                    ProcessTouchSelectObject(touch1, isOverUI);
                    break;
                default:
                    break;
            }

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
                        /*if (!ProcessTouchSelectObject(touch1, isOverUI))
                        {
                            MoveSelectedObject(touch1);
                        }*/

                        
                        if (touch1.phase == TouchPhase.Moved && fingerStartMove)
                        {
                            Debug.Log("_points.Add(touch1.position)");
                            _points.Add(touch1.position);
                        }

                        if (touch1.phase == TouchPhase.Ended && fingerStartMove)
                        {
                            Debug.Log("touch1.phase == TouchPhase.Ended");
                            fingerStartMove = false;
                            RotateIfPointsOnCircle(_points);
                        }


                        if (touch1.phase == TouchPhase.Began)
                                fingerStartMove = true;
                        
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
        if (startRotate)
        {
            _selectedObject.transform.rotation *= Quaternion.Euler(0.0f, currentRotationSpeed, 0.0f);
            currentRotationSpeed *= 0.95f;

            if (currentRotationSpeed < 1.0f)
            {
                startRotate = false;
                currentRotationSpeed = 10000.0f;
            }

            _points.Clear();
        }

        if (crossedLeftToRight && crossedRightToLeft)
        {
            transparency -= 0.01f;
            ChengeTransparency(_selectedObject.GetComponent<Renderer>().material, transparency);

            if (transparency < 0.02f)
            {
                Destroy(_selectedObject);

                _descriptionScreen.CloseObjectDescription();

                crossedLeftToRight = false;
                crossedRightToLeft = false;

                transparency = 1.0f;
            }
        }

    }

    private void RotateIfPointsOnCircle(List<Vector2> points)
    {
        Debug.Log("RotateIfPointsOnCircle(List<Vector2> points) 1");

        // The bottom-left of the screen or window is at (0, 0). The top-right of the screen or window is at (Screen.width, Screen.height).
        Vector2 firstPoint = points[0];

        Debug.Log("RotateIfPointsOnCircle(List<Vector2> points) 2");
        Vector2 lastPoint = points[points.Count - 1];

        Debug.Log("RotateIfPointsOnCircle(List<Vector2> points) 3");

        float centerX = (firstPoint.x + lastPoint.x) / 2;
        float centerY = (firstPoint.y + lastPoint.y) / 2;

        float trueRadius2 = ((lastPoint.x - firstPoint.x) * (lastPoint.x - firstPoint.x) + (lastPoint.y - firstPoint.y) * (lastPoint.y - firstPoint.y)) / 4;

        Debug.Log("RotateIfPointsOnCircle(List<Vector2> points) 4");
        float trueRadius = (float)Math.Sqrt(trueRadius2);

        //float radiusX = (firstPoint.x - lastPoint.x) / 2;
        //float radiusY = (firstPoint.y - lastPoint.y) / 2;

        //if (radiusX < 0.0f)
        // radiusX *= -1.0f;

        // if (radiusY < 0.0f)
        //radiusY *= -1.0f;

        // float radius = 0.0f;

        //if (radiusX > radiusY)
        //radius = radiusX;
        // else
        // radius = radiusY;

        //float radius2 = radius * radius;

        //float centerX = firstPoint.x + radiusX;
        //float centerY = firstPoint.y + radiusY;

        for (int i = 0; i < points.Count; i++)
        {

            float radius2 = (points[i].x - centerX) * (points[i].x - centerX) +
                 (points[i].y - centerY) * (points[i].y - centerY);

            float deltaRadPlus2 = (trueRadius + trueRadius * 0.85f) * (trueRadius + trueRadius * 0.85f);
            float deltaRadMinus2 = (trueRadius - trueRadius * 0.85f) * (trueRadius - trueRadius * 0.85f);

            //Debug.Log(radius2.ToString());
            //Debug.Log(deltaRadPlus2.ToString());
            //Debug.Log(deltaRadMinus2.ToString());

            Debug.Log("trueRadius " + trueRadius.ToString());
            Debug.Log("firstPoint.x " + firstPoint.x.ToString());
            Debug.Log("firstPoint.y " + firstPoint.y.ToString());
            Debug.Log("lastPoint.x " + lastPoint.x.ToString());
            Debug.Log("lastPoint.y " + lastPoint.y.ToString());
            Debug.Log("centerX " + centerX.ToString());
            Debug.Log("centerY " + centerY.ToString());
            Debug.Log("points[i].x " + points[i].x.ToString());
            Debug.Log("points[i].y " + points[i].y.ToString());

            bool moreThanRad = (points[i].x - centerX) * (points[i].x - centerX) +
                 (points[i].y - centerY) * (points[i].y - centerY) <= deltaRadPlus2;

            bool lessThanRad = (points[i].x - centerX) * (points[i].x - centerX) +
                 (points[i].y - centerY) * (points[i].y - centerY) >= deltaRadMinus2;

            //Debug.Log("for (int i = 1; i < points.Count - 1; i++)");

            if (!(moreThanRad && lessThanRad))
            {
                //Debug.Log("i " + i.ToString());
                //if ((float)i > (float)((points.Count - 2) * 0.65f))
                    break;

                Debug.Log("!(moreThanRad || lessThanRad)");
                return;
            }
        }

        startRotate = true;
    }

    bool startRotate = false;
    float currentRotationSpeed = 10000.0f;
    public void RotateAfterCircleSwipe()
    {
        //Debug.Log("RotateAfterCircleSwipe()");
        if (_selectedObject && _currentState == InterractionManagerState.SelectObject)
        {
            //Debug.Log("RotateAfterCircleSwipe() 1");
            // startRotate = true;
            /* while (currentRotationSpeed > 1.0f)
            {
                startRotate = true;
                Debug.Log("RotateAfterCircleSwipe() 2");
                _selectedObject.transform.rotation *= Quaternion.Euler(0.0f, currentRotationSpeed, 0.0f);
                currentRotationSpeed *= 0.99f;
            }
            currentRotationSpeed = 10000000.0f; */
        }
    }

    bool crossedLeftToRight = false;
    bool crossedRightToLeft = false;
    //bool fullCross = false;

    float transparency = 1.0f;

    public void CrossLeftToRight()
    {
        //Debug.Log("CrossLeftToRight()");
        if (!crossedLeftToRight)
        {
            crossedLeftToRight = true;
            return;
        }

        //Debug.Log("CrossLeftToRight() 1");

        /*if (_selectedObject)
        {

            Debug.Log("CrossLeftToRight() 2");
            float transparency = 1.0f;
            while (transparency > 0.0f)
            {
                transparency -= 0.025f;
                ChengeTransparency(_selectedObject.GetComponent<Renderer>().material, transparency);
            }
            Destroy(_selectedObject);
            crossedLeftToRight = false;
        }*/
    }

    public void CrossRightToLeft()
    {

        //Debug.Log("CrossRightToLeft()");
        if (!crossedRightToLeft)
        {
            
            crossedRightToLeft = true;
            return;
        }

        //Debug.Log("CrossRightToLeft() 1");

        /*if (_selectedObject)
        {
            Debug.Log("CrossRightToLeft() 2");
            float transparency = 1.0f;
            while (transparency > 0.0f)
            {
                transparency -= 0.025f;
                ChengeTransparency(_selectedObject.GetComponent<Renderer>().material, transparency);
            }
            Destroy(_selectedObject);
            crossedRightToLeft = false;
        }*/
    }

    private void ChengeTransparency(Material mat, float currentTransparency)
    {
        Color oldColor = mat.color;
        Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, currentTransparency);
        mat.SetColor("_Color", newColor);
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

                _descriptionScreen = descScreen;

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

    public void ProcessFingerTap(int tapCount)
    {
        if (tapCount == 2)
        DisplayDefaultScreen();
    }
}
