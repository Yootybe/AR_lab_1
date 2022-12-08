using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static System.Net.Mime.MediaTypeNames;

public class ImageTracker : MonoBehaviour
{
    [SerializeField] private List<GameObject> ObjectsToPlace;
    [SerializeField] private ARTrackedImageManager aRTrackedImageManager;

    [SerializeField] private List<GameObject> FirstComics;
    [SerializeField] private List<GameObject> SecondComics;
    [SerializeField] private List<GameObject> ThirdComics;

    private List<GameObject> CurrentComics;

    private GameObject lastActivatedObject;
    private GameObject deletableObject;
    private Vector3 TrueObjPosition;

    private int lastIndexInComics = -1;

    private GameObject defaultObject;

    private int _refImageCount;
    private Dictionary<string, GameObject> _allObjects;
    private IReferenceImageLibrary _refLibrary;

    private Dictionary<string, GameObject> _FirstComics;
    private Dictionary<string, GameObject> _SecondComics;
    private Dictionary<string, GameObject> _ThirdComics;

    private GameObject[] ComicsButtons;
    private GameObject[] DefaultScreenButtons;

    private GameObject RotatingObj;
    private GameObject ROFront;
    private GameObject ROBack;

    enum ComicsType
    {
        Default = 0,
        First = 1,
        Second = 2,
        Third = 3
    }

    private ComicsType currentComicsType = ComicsType.Default;

    private void OnEnable()
    {
        aRTrackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    private void OnDisable()
    {
        aRTrackedImageManager.trackedImagesChanged -= OnImageChanged;
    }

    private void Start()
    {
        RotatingObj = GameObject.Find("RotatingObj");
        ROFront = GameObject.Find("ROFront");
        ROBack = GameObject.Find("ROBack");

        RotatingObj.SetActive(false);
        ROFront.SetActive(false);
        ROBack.SetActive(false);

        _refLibrary = aRTrackedImageManager.referenceLibrary;
        _refImageCount = _refLibrary.count;
        LoadObjectDictionary();
    }

    bool rotateRight = false;
    bool rotateLeft = false;

    float rotatingAngle = 0.0f;

    private void Update()
    {
        if (rotateRight)
        {
            RotatingObj.transform.rotation *= Quaternion.Euler(0.0f, 0.0f, -2.0f);

            rotatingAngle += 2.0f;

            if (rotatingAngle > 88.0f)
            {
                rotateRight = false;
                rotatingAngle = 0.0f;
            }
        }

        if (rotateLeft)
        {

        }
    }

    public void Initialize()
    {
        foreach (var key in _allObjects)
            key.Value.SetActive(true);
    }

    private void LoadObjectDictionary()
    {
        _allObjects = new Dictionary<string, GameObject>();

        _FirstComics = new Dictionary<string, GameObject>();
        _SecondComics = new Dictionary<string, GameObject>();
        _ThirdComics = new Dictionary<string, GameObject>();

        for (int i = 0; i < _refImageCount; i++)
        {
            GameObject newOverlay = new GameObject();

            for (int j = 0; j < 10; j++)
            {
                newOverlay = FirstComics[j];
                if (FirstComics[j].gameObject.scene.rootCount == 0)
                {
                    newOverlay = Instantiate(FirstComics[j], transform.localPosition, Quaternion.identity);
                }
                _FirstComics.Add(j.ToString(), newOverlay);
                newOverlay.SetActive(false);
            }

            for (int j = 0; j < 10; j++)
            {
                newOverlay = SecondComics[j];
                if (SecondComics[j].gameObject.scene.rootCount == 0)
                {
                    newOverlay = Instantiate(SecondComics[j], transform.localPosition, Quaternion.identity);
                }
                _SecondComics.Add(j.ToString(), newOverlay);
                newOverlay.SetActive(false);
            }   

            for (int j = 0; j < 10; j++)
            {
                newOverlay = ThirdComics[j];
                if (ThirdComics[j].gameObject.scene.rootCount == 0)
                {
                    newOverlay = Instantiate(ThirdComics[j], transform.localPosition, Quaternion.identity);
                }
                _ThirdComics.Add(j.ToString(), newOverlay);
                newOverlay.SetActive(false);
            }

            newOverlay = ObjectsToPlace[i];
            if (ObjectsToPlace[i].gameObject.scene.rootCount == 0)
            {
                newOverlay = Instantiate(ObjectsToPlace[i], transform.localPosition, Quaternion.identity);
            }

            _allObjects.Add(_refLibrary[i].name, newOverlay);
            newOverlay.SetActive(false);
        }
    }

    private void ActivateTrackedObject(string imageName)
    {
        Debug.Log("Tracked the target: " + imageName);
        _allObjects[imageName].SetActive(true);
        _allObjects[imageName].transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

        lastActivatedObject = _allObjects[imageName];
        defaultObject = _allObjects[imageName];

        ComicsButtons = GameObject.FindGameObjectsWithTag("ComicsButtons");

        for (int i = 0; i < ComicsButtons.Length; i++)
        {
            Button button = ComicsButtons[i].GetComponent<Button>();

            if (button.name == "Horizon")
            {
                Debug.LogWarning("HorizonHorizonHorizonHorizonHorizon");
                button.onClick.AddListener(ActivateHorizon);
            }

            if (button.name == "BladeRunner")
                button.onClick.AddListener(ActivateBladeRunner);

            if (button.name == "GhostInTheShell")
                button.onClick.AddListener(ActivateGitS);

            ComicsButtons[i].SetActive(false);
        }
    }

    private void ActivateDefaultScreenButtons()
    {
        DefaultScreenButtons = GameObject.FindGameObjectsWithTag("DefaultScreen");

        for (int i = 0; i < DefaultScreenButtons.Length; i++)
        {
            Button button = DefaultScreenButtons[i].GetComponent<Button>();

            button.onClick.AddListener(ActivateDefaultCover);

            DefaultScreenButtons[i].SetActive(false);
        }

        for (int i = 0; i < DefaultScreenButtons.Length; i++)
        {
            DefaultScreenButtons[i].SetActive(true);
        }
    }


    private void UpdateTrackedObject(ARTrackedImage trackedImage)
    {

        TrueObjPosition = trackedImage.transform.position;

        switch (currentComicsType)
        {
            case ComicsType.Default:
                {
                    for (int i = 0; i < ComicsButtons.Length; i++)
                    {
                        ComicsButtons[i].SetActive(true);
                    }

                    if (trackedImage.trackingState == TrackingState.Tracking)
                    {
                        //_allObjects[trackedImage.referenceImage.name].SetActive(true);
                        _allObjects[trackedImage.referenceImage.name].transform.position = trackedImage.transform.position;
                        _allObjects[trackedImage.referenceImage.name].transform.rotation = trackedImage.transform.rotation;

                        ROFront = _allObjects[trackedImage.referenceImage.name];
                    }
                    else
                    {
                        _allObjects[trackedImage.referenceImage.name].SetActive(false);

                        for (int i = 0; i < ComicsButtons.Length; i++)
                        {
                            ComicsButtons[i].SetActive(false);
                        }

                        for (int i = 0; i < DefaultScreenButtons.Length; i++)
                        {
                            DefaultScreenButtons[i].SetActive(false);
                        }
                    }
                }
                break;

            case ComicsType.First:
                {
                    ActivateDefaultScreenButtons();

                    if (trackedImage.trackingState == TrackingState.Tracking)
                    {
                        //_FirstComics[lastIndexInComics.ToString()].SetActive(true);
                        _FirstComics[lastIndexInComics.ToString()].transform.position = trackedImage.transform.position;
                        _FirstComics[lastIndexInComics.ToString()].transform.rotation = trackedImage.transform.rotation;
                        _FirstComics[lastIndexInComics.ToString()].transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

                        // Rotating right
                        ROBack = _FirstComics[lastIndexInComics.ToString()];

                        if (lastIndexInComics == 0)
                        {
                            ROFront = defaultObject;
                        }
                        else
                        {
                            _FirstComics[(lastIndexInComics - 1).ToString()].transform.position = trackedImage.transform.position;
                            _FirstComics[(lastIndexInComics - 1).ToString()].transform.rotation = trackedImage.transform.rotation;
                            _FirstComics[(lastIndexInComics - 1).ToString()].transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0002f);
                            ROFront = _FirstComics[(lastIndexInComics - 1).ToString()];
                        }

                        rotateRight = true;
                        
                        //lastActivatedObject = _FirstComics[lastIndexInComics.ToString()];
                    }
                    else
                    {
                        _FirstComics[lastIndexInComics.ToString()].SetActive(false);

                        for (int i = 0; i < ComicsButtons.Length; i++)
                        {
                            ComicsButtons[i].SetActive(false);
                        }

                        for (int i = 0; i < DefaultScreenButtons.Length; i++)
                        {
                            DefaultScreenButtons[i].SetActive(false);
                        }
                    }
                }
                break;

            case ComicsType.Second:
                {
                    ActivateDefaultScreenButtons();

                    if (trackedImage.trackingState == TrackingState.Tracking)
                    {
                        //_SecondComics[lastIndexInComics.ToString()].SetActive(true);
                        _SecondComics[lastIndexInComics.ToString()].transform.position = trackedImage.transform.position;
                        _SecondComics[lastIndexInComics.ToString()].transform.rotation = trackedImage.transform.rotation;
                        _SecondComics[lastIndexInComics.ToString()].transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

                        lastActivatedObject = _SecondComics[lastIndexInComics.ToString()];
                    }
                    else
                    {
                        _SecondComics[lastIndexInComics.ToString()].SetActive(false);

                        for (int i = 0; i < ComicsButtons.Length; i++)
                        {
                            ComicsButtons[i].SetActive(false);
                        }

                        for (int i = 0; i < DefaultScreenButtons.Length; i++)
                        {
                            DefaultScreenButtons[i].SetActive(false);
                        }
                    }
                }
                break;

            case ComicsType.Third:
                {

                    ActivateDefaultScreenButtons();

                    if (trackedImage.trackingState == TrackingState.Tracking)
                    {
                        //_ThirdComics[lastIndexInComics.ToString()].SetActive(true);
                        _ThirdComics[lastIndexInComics.ToString()].transform.position = trackedImage.transform.position;
                        _ThirdComics[lastIndexInComics.ToString()].transform.rotation = trackedImage.transform.rotation;
                        _ThirdComics[lastIndexInComics.ToString()].transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

                        lastActivatedObject = _ThirdComics[lastIndexInComics.ToString()];
                    }
                    else
                    {
                        _ThirdComics[lastIndexInComics.ToString()].SetActive(false);

                        for (int i = 0; i < ComicsButtons.Length; i++)
                        {
                            ComicsButtons[i].SetActive(false);
                        }

                        for (int i = 0; i < DefaultScreenButtons.Length; i++)
                        {
                            DefaultScreenButtons[i].SetActive(false);
                        }
                    }
                }
                break;

            default:
                break;
        }
    }

    private void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var addedImage in args.added)
        {
            ActivateTrackedObject(addedImage.referenceImage.name);
        }

        foreach (var updated in args.updated)
        {
            UpdateTrackedObject(updated);
        }

        foreach (var trackedImage in args.removed)
        {
            Destroy(trackedImage.gameObject);
        }
    }
    public void BackToMain()
    {
        InteractionManager manager = FindObjectOfType<InteractionManager>();
        if (!manager)
            throw new MissingComponentException(manager.GetType().Name + " not found!");

        foreach (var key in _allObjects)
            key.Value.SetActive(false);

        manager.DisplayDefaultScreen();
    }

    public void ActivateDefaultCover()
    {
        lastIndexInComics = -1;

        lastActivatedObject.SetActive(false);
        lastActivatedObject = defaultObject;

        currentComicsType = ComicsType.Default;
    }

    public void ActivateHorizon()
    {
        lastIndexInComics = 0;
        currentComicsType = ComicsType.First;
        //lastActivatedObject.SetActive(false);
        RotatingObj.SetActive(false);
    }

    public void ActivateBladeRunner()
    {
        lastIndexInComics = 0;
        currentComicsType = ComicsType.Second;
        lastActivatedObject.SetActive(false);
    }

    public void ActivateGitS()
    {
        lastIndexInComics = 0;
        currentComicsType = ComicsType.Third;
        lastActivatedObject.SetActive(false);
    }

    public void TurnPageRight()
    {
        Debug.LogWarning("TurnPageRight() 1");

        if (lastIndexInComics < 9)
        {
            Debug.LogWarning("TurnPageRight() 2");

            lastIndexInComics++;

            lastActivatedObject.SetActive(false);
        }
    }

    public void TurnPageLeft()
    {
        Debug.LogWarning("TurnPageLeft() 1");

        if (lastIndexInComics > 0)
        {
            Debug.LogWarning("TurnPageLeft() 2");

            lastIndexInComics--;

            lastActivatedObject.SetActive(false);
        }

        if (lastIndexInComics == 0)
        {
            Debug.LogWarning("TurnPageLeft() 3");

            ActivateDefaultCover();
        }
    }
}