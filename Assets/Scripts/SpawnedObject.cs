using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedObject : MonoBehaviour
{
    [SerializeField] private string _displayName;
    [SerializeField] private string _description;

    public GameObject thisObjectRef;
    public SpawnedObjectDescriptionScreen thisDescriptionScreenRef;

    private int _number = -1;

    public float currentRotationSpeed = 0.0f;
    public bool rotatingRight = false;
    public bool rotatingLeft = false;

    public float transparency = 1.0f;
    public bool fingerStartMove = false;
    public bool firstLine = false;
    public bool linesMeet = false;


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
            return _description;
        }
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
        if (linesMeet)
        {
            transparency -= 0.01f;
            ChengeTransparency(thisObjectRef.GetComponent<Renderer>().material, transparency);

            if (transparency < 0.02f)
            {
                Destroy(thisObjectRef);

                thisDescriptionScreenRef.CloseObjectDescription();

                firstLine = false;
                linesMeet = false;

                /*if (_firstLine.Count > 0)
                    _firstLine.Clear();

                if (_secondLine.Count > 0)
                    _secondLine.Clear();*/

                transparency = 1.0f;
            }
        }

        if (currentRotationSpeed > 0.0f)
        {

            if (rotatingRight)
                thisObjectRef.transform.rotation *= Quaternion.Euler(0.0f, currentRotationSpeed, 0.0f);

            if (rotatingLeft)
                thisObjectRef.transform.rotation *= Quaternion.Euler(0.0f, -currentRotationSpeed, 0.0f);

            currentRotationSpeed *= 0.95f;

            if (currentRotationSpeed < 1.0f)
            {
                currentRotationSpeed = 0.0f;
                rotatingRight = false;
                rotatingLeft = false;
            }
        }
    }

    private void ChengeTransparency(Material mat, float currentTransparency)
    {
        Color oldColor = mat.color;
        Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, currentTransparency);
        mat.SetColor("_Color", newColor);
    }
}
