using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedObject : MonoBehaviour
{
    [SerializeField] private string _displayName;
    [SerializeField] private string _description_one;
    [SerializeField] private string _description_two;
    [SerializeField] private string _description_three;

    private int currentDescriptionNumber = 0;

    private int _number = -1;

    public string Name
    {
        get
        {
            if (_number >= 0)
            {
                return _displayName + " " + _number.ToString();
            }
            else
            {
                return _displayName;
            }
        }
    }

    public string Description
    {
        get
        {
            switch(currentDescriptionNumber)
            {
                case 0:
                    currentDescriptionNumber++;
                    return _description_one;
                    break;
                case 1:
                    currentDescriptionNumber++;
                    return _description_one;
                    break;
                case 2:
                    
                    break;
                default:

                    break;
            }
            return _description_one;
        }
    }

    private Quaternion rotation;
    public Quaternion Rotation
    {
        get { return rotation; }
        set { rotation = value; }
    }

    private Vector3 scale;
    public Vector3 Scale
    {
        get { return scale; }
        set { scale = value; }
    }

    public string UpdateDescription(bool isNext)
    {

        if (isNext)
        {
            switch (currentDescriptionNumber)
            {
                case 0:
                    currentDescriptionNumber++;
                    return _description_two;
                    break;
                case 1:
                    currentDescriptionNumber++;
                    return _description_three;
                    break;
                case 2:
                    currentDescriptionNumber = 0;
                    return _description_one;
                    break;
                default:
                    return _description_one;
                    break;
            }
        }
        else
        {
            switch (currentDescriptionNumber)
            {
                case 0:
                    currentDescriptionNumber = 2;
                    return _description_three;
                    break;
                case 1:
                    currentDescriptionNumber--;
                    return _description_one;
                    break;
                case 2:
                    currentDescriptionNumber--;
                    return _description_two;
                    break;
                default:
                    return _description_one;
                    break;
            }
        }
        return _description_one;
    }

    public void GiveNumber(int number)
    {
        _number = number;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
