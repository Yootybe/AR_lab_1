using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private GameObject[] Buttons;

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

        Buttons = GameObject.FindGameObjectsWithTag("IBButtons");

        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].SetActive(false);
        }
    }


    /*bool endOfTurningPage = true;
    private void Update()
    {
        if(endOfTurningPage)
            return;

        deletableObject.transform.position -= new Vector3(10.0f, 0.0f, 0.0f);
        lastActivatedObject.transform.position -= new Vector3(10.0f, 0.0f, 0.0f);

        if (lastActivatedObject.transform.position.x < TrueObjPosition.x)
        {
            endOfTurningPage = true;
            deletableObject.SetActive(false);
            lastActivatedObject.transform.position = TrueObjPosition;
        }
    }*/

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
        _allObjects[imageName].transform.localScale = new Vector3(0.00025f, 0.00045f, 0.0002f);

        lastActivatedObject = _allObjects[imageName];
        defaultObject = _allObjects[imageName];
    }


    private void UpdateTrackedObject(ARTrackedImage trackedImage)
    {
        //if (!endOfTurningPage)
          //  return;

        TrueObjPosition = trackedImage.transform.position;

        switch (currentComicsType)
        {
            case ComicsType.Default:
                {
                    for (int i = 0; i < Buttons.Length; i++)
                    {
                        Buttons[i].SetActive(true);
                    }

                    if (trackedImage.trackingState == TrackingState.Tracking)
                    {
                        _allObjects[trackedImage.referenceImage.name].SetActive(true);
                        _allObjects[trackedImage.referenceImage.name].transform.position = trackedImage.transform.position;
                        _allObjects[trackedImage.referenceImage.name].transform.rotation = trackedImage.transform.rotation;

                    }
                    else
                    {
                        _allObjects[trackedImage.referenceImage.name].SetActive(false);

                        for (int i = 0; i < Buttons.Length; i++)
                        {
                            Buttons[i].SetActive(false);
                        }
                    }

                    
                }
                break;

            case ComicsType.First:
                {
                    if (trackedImage.trackingState == TrackingState.Tracking)
                    {
                        _FirstComics[lastIndexInComics.ToString()].SetActive(true);
                        //_FirstComics[lastIndexInComics.ToString()].transform.position = trackedImage.transform.position + new Vector3(500.0f, 0.0f, 0.0f);
                        _FirstComics[lastIndexInComics.ToString()].transform.position = trackedImage.transform.position;
                        _FirstComics[lastIndexInComics.ToString()].transform.rotation = trackedImage.transform.rotation;
                        _FirstComics[lastIndexInComics.ToString()].transform.localScale = new Vector3(0.00025f, 0.00045f, 0.0002f);

                        lastActivatedObject = _FirstComics[lastIndexInComics.ToString()];

                        //endOfTurningPage = true;
                    }
                    else
                    {
                        _FirstComics[lastIndexInComics.ToString()].SetActive(false);

                        for (int i = 0; i < Buttons.Length; i++)
                        {
                            Buttons[i].SetActive(false);
                        }
                    }
                }
                break;

            case ComicsType.Second:
                {
                    if (trackedImage.trackingState == TrackingState.Tracking)
                    {
                        _SecondComics[lastIndexInComics.ToString()].SetActive(true);
                        _SecondComics[lastIndexInComics.ToString()].transform.position = trackedImage.transform.position;
                        _SecondComics[lastIndexInComics.ToString()].transform.rotation = trackedImage.transform.rotation;
                        _SecondComics[lastIndexInComics.ToString()].transform.localScale = new Vector3(0.00025f, 0.00045f, 0.0002f);

                        lastActivatedObject = _SecondComics[lastIndexInComics.ToString()];
                    }
                    else
                    {
                        _SecondComics[lastIndexInComics.ToString()].SetActive(false);

                        for (int i = 0; i < Buttons.Length; i++)
                        {
                            Buttons[i].SetActive(false);
                        }
                    }
                }
                break;

            case ComicsType.Third:
                {
                    if (trackedImage.trackingState == TrackingState.Tracking)
                    {
                        _ThirdComics[lastIndexInComics.ToString()].SetActive(true);
                        _ThirdComics[lastIndexInComics.ToString()].transform.position = trackedImage.transform.position;
                        _ThirdComics[lastIndexInComics.ToString()].transform.rotation = trackedImage.transform.rotation;
                        _ThirdComics[lastIndexInComics.ToString()].transform.localScale = new Vector3(0.00025f, 0.00045f, 0.0002f);

                        lastActivatedObject = _ThirdComics[lastIndexInComics.ToString()];
                    }
                    else
                    {
                        _ThirdComics[lastIndexInComics.ToString()].SetActive(false);

                        for (int i = 0; i < Buttons.Length; i++)
                        {
                            Buttons[i].SetActive(false);
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

        //defaultObject.SetActive(true);
    }

    public void ActivateHorizon()
    {
        //CurrentComics = FirstComics;

        //lastActivatedObject.SetActive(false);
        //lastActivatedObject = CurrentComics[0];
        //lastActivatedObject.SetActive(true);

        //lastIndexInComics = 0;


        lastIndexInComics = 0;
        currentComicsType = ComicsType.First;
        lastActivatedObject.SetActive(false);

        //lastActivatedObject.transform.position -= new Vector3(1.0f, 0.0f, 0.0f);
        //deletableObject = lastActivatedObject;
    }

    public void ActivateBladeRunner()
    {
        lastIndexInComics = 0;

        currentComicsType = ComicsType.Second;

        //CurrentComics = _SecondComics[lastIndexInComics.ToString()];

        lastActivatedObject.SetActive(false);
        //lastActivatedObject = CurrentComics[0];
        //lastActivatedObject.SetActive(true);

        
    }

    public void ActivateGitS()
    {
        //CurrentComics = ThirdComics;

        currentComicsType = ComicsType.Third;
        lastIndexInComics = 0;

        lastActivatedObject.SetActive(false);
        //lastActivatedObject = CurrentComics[0];
        //lastActivatedObject.SetActive(true);

    }

    public void TurnPageRight()
    {
        Debug.LogWarning("TurnPageRight() 1");

        if (lastIndexInComics < 9)
        {
            Debug.LogWarning("TurnPageRight() 2");

            lastIndexInComics++;

            lastActivatedObject.SetActive(false);
            lastActivatedObject.transform.position -= new Vector3(1.0f, 0.0f, 0.0f);
            //deletableObject = lastActivatedObject;

            

            //lastActivatedObject = CurrentComics[lastIndexInComics];
            //lastActivatedObject.SetActive(true);
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
            //lastActivatedObject = CurrentComics[lastIndexInComics];
            //lastActivatedObject.SetActive(true);
        }

        if (lastIndexInComics == 0)
        {
            Debug.LogWarning("TurnPageLeft() 3");

            ActivateDefaultCover();
        }
    }
}