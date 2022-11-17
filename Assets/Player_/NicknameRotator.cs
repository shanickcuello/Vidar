using System;
using UnityEngine;

namespace Player_
{
    public class NicknameRotator : MonoBehaviour
    {
        private Transform cameraTransform;
        private void Start()
        {
            cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            gameObject.transform.LookAt(cameraTransform);
        }
    }
}
