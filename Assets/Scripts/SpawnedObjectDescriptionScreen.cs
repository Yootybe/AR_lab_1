using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedObjectDescriptionScreen : MonoBehaviour
{
    [SerializeField] private GameObject _descriptionPanel;

    [SerializeField] private UnityEngine.UI.Text _objectNameText;
    [SerializeField] private UnityEngine.UI.Text _objectDescriptionText;
    [SerializeField] private UnityEngine.UI.Text _objectRotation;
    [SerializeField] private UnityEngine.UI.Text _objectScale;

    public void InitializeScreen()
    {
        _descriptionPanel.SetActive(false);
    }

    public void ShowObjectDescription(SpawnedObject obj)
    {
        _objectNameText.text = obj.Name;
        _objectDescriptionText.text = obj.Description;
        _objectScale.text = obj.Scale.x.ToString();
        _objectRotation.text = obj.Rotation.eulerAngles.x.ToString();
        _descriptionPanel.SetActive(true);
    }

    public void UpdateDescription(SpawnedObject obj, bool isNext)
    {
        _objectDescriptionText.text = obj.UpdateDescription(isNext);
    }

    public void UpdateRotationText(float angle)
    {
        _objectRotation.text = angle.ToString();
    }

    public void UpdateScaleText(Vector3 scale)
    {
        _objectScale.text = scale.x.ToString();
    }

    public void BackToMain()
    {
        InteractionManager manager = FindObjectOfType<InteractionManager>();
        if (!manager)
            throw new MissingComponentException(manager.GetType().Name + " not found!");
        manager.DisplayDefaultScreen();
    }
}
