using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static System.Net.Mime.MediaTypeNames;

public class ImageTracker : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager aRTrackedImageManager;

    [SerializeField] private GameObject ReplasedObj;
    private GameObject _ReplasedObj;
    private GameObject _ReplasedObjFront;
    private GameObject _ReplasedObjBack;

    [SerializeField] private GameObject DefaultCover;
    private GameObject _DefaultCover;

    [SerializeField] private List<GameObject> FirstComics;
    [SerializeField] private List<GameObject> SecondComics;
    [SerializeField] private List<GameObject> ThirdComics;

    private Dictionary<string, GameObject> _FirstComics;
    private Dictionary<string, GameObject> _SecondComics;
    private Dictionary<string, GameObject> _ThirdComics;

    private List<GameObject> CurrentComics;

    private GameObject lastActivatedObject;

    private int lastIndexInComics = -1;
    private int _refImageCount;

    private IReferenceImageLibrary _refLibrary;

    private GameObject[] ComicsButtons;
    private GameObject[] DefaultScreenButtons;

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
            _ReplasedObj.transform.rotation *= Quaternion.Euler(0.0f, 0.0f, -2.0f);

            rotatingAngle += 2.0f;

            if (rotatingAngle > 188.0f)
            {
                GameObject tempFront = Instantiate(_ReplasedObjFront);
                _ReplasedObjFront = Instantiate(_ReplasedObjBack);
                _ReplasedObjBack = Instantiate(tempFront);

                rotateRight = false;
                rotatingAngle = 0.0f;
            }
        }

        if (rotateLeft)
        {
            _ReplasedObj.transform.rotation *= Quaternion.Euler(0.0f, 0.0f, 2.0f);

            rotatingAngle += 2.0f;

            if (rotatingAngle > 188.0f)
            {
                GameObject tempFront = Instantiate(_ReplasedObjFront);
                _ReplasedObjFront = Instantiate(_ReplasedObjBack);
                _ReplasedObjBack = Instantiate(tempFront);

                rotateLeft = false;
                rotatingAngle = 0.0f;
            }
        }
    }

    public void Initialize()
    {
        // TODO: add not only _allObjects
        /*foreach (var key in _allObjects)
            key.Value.SetActive(true);*/

        //_ReplasedObj.SetActive(true);
    }

    private void LoadObjectDictionary()
    {
        _FirstComics = new Dictionary<string, GameObject>();
        _SecondComics = new Dictionary<string, GameObject>();
        _ThirdComics = new Dictionary<string, GameObject>();

        for (int i = 0; i < _refImageCount; i++)
        {
            GameObject newOverlay = new GameObject();

            newOverlay = ReplasedObj;
            if (ReplasedObj.gameObject.scene.rootCount == 0)
            {
                newOverlay = Instantiate(ReplasedObj, transform.localPosition, Quaternion.identity);
            }
            _ReplasedObj = newOverlay;
            newOverlay.SetActive(false);

            newOverlay = DefaultCover;
            if (DefaultCover.gameObject.scene.rootCount == 0)
            {
                newOverlay = Instantiate(DefaultCover, transform.localPosition, Quaternion.identity);
            }
            _DefaultCover = newOverlay;
            newOverlay.SetActive(false);

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
        }
    }

    private void ActivateTrackedObject(string imageName)
    {
        Debug.Log("Tracked the target: " + imageName);
        _ReplasedObj.SetActive(true);
        _ReplasedObj.transform.localPosition = new Vector3(0.00025f, 0.00065f, 0.0003f);

        GameObject[] objInside = _ReplasedObj.GetComponentsInChildren<GameObject>(true);
        for (int i = 0; i < objInside.Length; i++)
        {
            if (objInside[i].name == "ROFront")
                _ReplasedObjFront = objInside[i];

            if (objInside[i].name == "ROBack")
                _ReplasedObjBack = objInside[i];
        }

        _ReplasedObjFront.SetActive(true);
        _ReplasedObjBack.SetActive(true);

        _ReplasedObjFront = _DefaultCover;
        _ReplasedObjFront.transform.position = _ReplasedObj.transform.position;
        _ReplasedObjFront.transform.rotation = _ReplasedObj.transform.rotation;


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
                        _ReplasedObj.SetActive(true);
                        _ReplasedObj.transform.position = trackedImage.transform.position;
                        _ReplasedObj.transform.rotation = trackedImage.transform.rotation;
                        _ReplasedObj.transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

                        _ReplasedObjBack = _DefaultCover;
                        _ReplasedObjBack.SetActive(true);
                        _ReplasedObjBack.transform.position = trackedImage.transform.position;
                        _ReplasedObjBack.transform.rotation = trackedImage.transform.rotation;
                        _ReplasedObjBack.transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

                        rotateLeft = true;
                    }
                    else
                    {
                        _ReplasedObj.SetActive(false);

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
                        _ReplasedObj.SetActive(true);
                        _ReplasedObj.transform.position = trackedImage.transform.position;
                        _ReplasedObj.transform.rotation = trackedImage.transform.rotation;
                        _ReplasedObj.transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

                        _ReplasedObjBack = _FirstComics[lastIndexInComics.ToString()];
                        _ReplasedObjBack.SetActive(true);
                        _ReplasedObjBack.transform.position = trackedImage.transform.position;
                        _ReplasedObjBack.transform.rotation = trackedImage.transform.rotation;
                        _ReplasedObjBack.transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

                        if (doTurnRight)
                        {
                            rotateRight = true;
                            doTurnRight = false;
                        }

                        if (doTurnLeft)
                        {
                            rotateLeft = true;
                            doTurnLeft = false;
                        }
                    }
                    else
                    {
                        _ReplasedObj.SetActive(false);

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
                        _ReplasedObj.SetActive(true);
                        _ReplasedObj.transform.position = trackedImage.transform.position;
                        _ReplasedObj.transform.rotation = trackedImage.transform.rotation;
                        _ReplasedObj.transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

                        _ReplasedObjBack = _SecondComics[lastIndexInComics.ToString()];
                        _ReplasedObjBack.SetActive(true);
                        _ReplasedObjBack.transform.position = trackedImage.transform.position;
                        _ReplasedObjBack.transform.rotation = trackedImage.transform.rotation;
                        _ReplasedObjBack.transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

                        if (doTurnRight)
                        {
                            rotateRight = true;
                            doTurnRight = false;
                        }

                        if (doTurnLeft)
                        {
                            rotateLeft = true;
                            doTurnLeft = false;
                        }
                    }
                    else
                    {
                        _ReplasedObj.SetActive(false);

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
                        _ReplasedObj.SetActive(true);
                        _ReplasedObj.transform.position = trackedImage.transform.position;
                        _ReplasedObj.transform.rotation = trackedImage.transform.rotation;
                        _ReplasedObj.transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

                        _ReplasedObjBack = _ThirdComics[lastIndexInComics.ToString()];
                        _ReplasedObjBack.SetActive(true);
                        _ReplasedObjBack.transform.position = trackedImage.transform.position;
                        _ReplasedObjBack.transform.rotation = trackedImage.transform.rotation;
                        _ReplasedObjBack.transform.localScale = new Vector3(0.00025f, 0.00065f, 0.0003f);

                        if (doTurnRight)
                        {
                            rotateRight = true;
                            doTurnRight = false;
                        }

                        if (doTurnLeft)
                        {
                            rotateLeft = true;
                            doTurnLeft = false;
                        }
                    }
                    else
                    {
                        _ReplasedObj.SetActive(false);

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

        //foreach (var key in _allObjects)
        //key.Value.SetActive(false);

        _ReplasedObj.SetActive(false);

        manager.DisplayDefaultScreen();
    }


    bool doTurnRight = false;
    bool doTurnLeft = false;
    public void ActivateDefaultCover()
    {
        lastIndexInComics = -1;

        _ReplasedObj.SetActive(false);

        currentComicsType = ComicsType.Default;
    }

    public void ActivateHorizon()
    {
        lastIndexInComics = 0;
        currentComicsType = ComicsType.First;

        doTurnRight = true;

        _ReplasedObj.SetActive(false);
    }

    public void ActivateBladeRunner()
    {
        lastIndexInComics = 0;
        currentComicsType = ComicsType.Second;
        doTurnRight = true;

        _ReplasedObj.SetActive(false);
    }

    public void ActivateGitS()
    {
        lastIndexInComics = 0;
        currentComicsType = ComicsType.Third;
        doTurnRight = true;

        _ReplasedObj.SetActive(false);
    }

    public void TurnPageRight()
    {
        Debug.LogWarning("TurnPageRight() 1");

        if (lastIndexInComics < 9)
        {
            Debug.LogWarning("TurnPageRight() 2");

            lastIndexInComics++;

            doTurnRight = true;

            _ReplasedObj.SetActive(false);
        }
    }

    public void TurnPageLeft()
    {
        Debug.LogWarning("TurnPageLeft() 1");

        if (lastIndexInComics > 0)
        {
            Debug.LogWarning("TurnPageLeft() 2");

            lastIndexInComics--;

            doTurnLeft = true;

            _ReplasedObj.SetActive(false);
        }

        if (lastIndexInComics == 0)
        {
            Debug.LogWarning("TurnPageLeft() 3");

            ActivateDefaultCover();
        }
    }
}