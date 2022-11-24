using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public class ImageTracker : MonoBehaviour
{
    [SerializeField] private List<GameObject> ObjectsToPlace;
    [SerializeField] private ARTrackedImageManager aRTrackedImageManager;

    private int _refImageCount;
    private Dictionary<string, GameObject> _allObjects;
    private IReferenceImageLibrary _refLibrary;
    private VideoPlayer activatedVideo;
    private GameObject[] activatedVideos;

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

        activatedVideos = GameObject.FindGameObjectsWithTag("VideoPlayer1");
        if (!activatedVideo)
        {
            throw new MissingComponentException(activatedVideo.GetType().Name + " not found!");
            Debug.LogWarning("activatedVideo NOT FOUND");
        }
        else
        {
            Debug.LogWarning("FOUND");
        }
           
        videoPlaying = true;
    }

    private void UpdateTrackedObject(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            _allObjects[trackedImage.referenceImage.name].SetActive(true);
            _allObjects[trackedImage.referenceImage.name].transform.position = trackedImage.transform.position;
            _allObjects[trackedImage.referenceImage.name].transform.rotation = trackedImage.transform.rotation;
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

    bool videoPlaying = false;
    bool videoStopped = false;
    public void StartPause()
    {
        bool curVideoPlaying = false;
        bool curVideoStopped = false;

        for (int i = 0; i < activatedVideos.Length; i++)
        {
            curVideoPlaying = videoPlaying;
            curVideoStopped = videoStopped;

            activatedVideo = activatedVideos[i].GetComponent<VideoPlayer>();

            if (activatedVideo)
            {
                if (curVideoPlaying)
                {
                    activatedVideo.Pause();
                    curVideoPlaying = false;
                }
                else
                {
                    if (videoStopped)
                        activatedVideo.frame = 0;

                    activatedVideo.Play();
                    curVideoPlaying = true;
                    curVideoStopped = false;
                }
            }
        }

        videoPlaying = curVideoPlaying;
        videoStopped = curVideoStopped;
    }

    
    public void Stop()
    {

        for (int i = 0; i < activatedVideos.Length; i++)
        {
            activatedVideo = activatedVideos[i].GetComponent<VideoPlayer>();

            if (activatedVideo)
                activatedVideo.Stop();
        }

        videoPlaying = false;
        videoStopped = true;
    }

    public void increaseSpeed()
    {
        for (int i = 0; i < activatedVideos.Length; i++)
        {

            activatedVideo = activatedVideos[i].GetComponent<VideoPlayer>();

            if (activatedVideo)
                activatedVideo.playbackSpeed += 1.0f;
        }
    }

    public void decreaseSpeed()
    {
        for (int i = 0; i < activatedVideos.Length; i++)
        {
            activatedVideo = activatedVideos[i].GetComponent<VideoPlayer>();

            if (activatedVideo)
                activatedVideo.playbackSpeed -= 1.0f;
        }
    }

    public void Plus5Sec()
    {
        for (int i = 0; i < activatedVideos.Length; i++)
        {
            activatedVideo = activatedVideos[i].GetComponent<VideoPlayer>();

            if (activatedVideo)
            {
                    activatedVideo.time += 5.0f;
            }
        }
    }

    public void Minus5Sec()
    {
        for (int i = 0; i < activatedVideos.Length; i++)
        {
            activatedVideo = activatedVideos[i].GetComponent<VideoPlayer>();

            if (activatedVideo)
            {
                if (activatedVideo.time > 5.0f)
                    activatedVideo.time -= 5.0f;
            }
        }
    }
}