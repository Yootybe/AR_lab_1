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
    private int lastIndexInComics = -1;

    private GameObject defaultObject;

    private int _refImageCount;
    private Dictionary<string, GameObject> _allObjects;
    private IReferenceImageLibrary _refLibrary;
    //private Image 

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

    public void Initialize()
    {
        foreach (var key in _allObjects)
            key.Value.SetActive(true);
    }

    private void LoadObjectDictionary()
    {
        _allObjects = new Dictionary<string, GameObject>();
        for (int i = 0; i < _refImageCount; i++)
        {
            GameObject newOverlay = new GameObject();
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
        _allObjects[imageName].transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);

        lastActivatedObject = _allObjects[imageName];
        defaultObject = _allObjects[imageName];
    }

    private void UpdateTrackedObject(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            _allObjects[trackedImage.referenceImage.name].SetActive(true);
            _allObjects[trackedImage.referenceImage.name].transform.position = trackedImage.transform.position;
            _allObjects[trackedImage.referenceImage.name].transform.rotation = trackedImage.transform.rotation;

            lastActivatedObject = _allObjects[trackedImage.referenceImage.name];
            defaultObject = _allObjects[trackedImage.referenceImage.name];
        }
        else
        {
            _allObjects[trackedImage.referenceImage.name].SetActive(false);
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
        lastActivatedObject = null;
        defaultObject.SetActive(true);
    }

    public void ActivateHorizon()
    {
        CurrentComics = FirstComics;

        lastActivatedObject.SetActive(false);
        lastActivatedObject = CurrentComics[0];
        lastActivatedObject.SetActive(true);

        lastIndexInComics = 0;
    }

    public void ActivateBladeRunner()
    {
        CurrentComics = SecondComics;

        lastActivatedObject.SetActive(false);
        lastActivatedObject = CurrentComics[0];
        lastActivatedObject.SetActive(true);

        lastIndexInComics = 0;
    }

    public void ActivateGitS()
    {
        CurrentComics = ThirdComics;

        lastActivatedObject.SetActive(false);
        lastActivatedObject = CurrentComics[0];
        lastActivatedObject.SetActive(true);

        lastIndexInComics = 0;
    }

    public void TurnPageRight()
    {
        if (lastIndexInComics < 9)
        {
            lastIndexInComics++;

            lastActivatedObject.SetActive(false);
            lastActivatedObject = CurrentComics[lastIndexInComics];
            lastActivatedObject.SetActive(true);
        }
    }

    public void TurnPageLeft()
    {
        if (lastIndexInComics > 0)
        {
            lastIndexInComics--;

            lastActivatedObject.SetActive(false);
            lastActivatedObject = CurrentComics[lastIndexInComics];
            lastActivatedObject.SetActive(true);
        }

        if (lastIndexInComics == 0)
        {
            ActivateDefaultCover();
        }
    }
}