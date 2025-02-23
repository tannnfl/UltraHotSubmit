using UnityEngine;
using UnityEngine.UI;

public class UIFollow : MonoBehaviour
{
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private Transform subject;

    private void Update()
    {
        if (subject)
        {
            transform.position = PlayerCamera.WorldToScreenPoint(subject.position);
        }
    }

}

