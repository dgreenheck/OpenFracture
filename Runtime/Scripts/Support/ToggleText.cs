using UnityEngine;

public class ToggleText : MonoBehaviour
{
    public KeyCode toggleKey;

    public GameObject textObject;

    // Start is called before the first frame update
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            textObject.SetActive(!textObject.activeSelf);
        }
    }
}
