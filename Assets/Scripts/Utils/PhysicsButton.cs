using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Utils
{
    public class PhysicsButton : MonoBehaviour
    {
        [SerializeField] private Collider bodyButtonCollider;
        [SerializeField] private Collider buttonCollider;
        
        [SerializeField] private AudioSource pressedSound;
        [SerializeField] private AudioSource releasedSound;
        [SerializeField] private UnityEvent  onPressed;
        [SerializeField] private Animator    animator;

        private static readonly int Pushed = Animator.StringToHash("pushed");

        private void Awake()
        {
            Physics.IgnoreCollision(buttonCollider, bodyButtonCollider);
        }

        private void OnTriggerEnter(Collider other)
        {
            animator.SetBool(Pushed, true);
            Pressed();
        }
        
        private void OnTriggerExit(Collider other)
        {
            animator.SetBool(Pushed, false);
            Released();
        }

        private void Pressed()
        {
            pressedSound.pitch = 1;
            pressedSound.Play();
            onPressed.Invoke();
        }

        private void Released()
        {
            releasedSound.pitch = Random.Range(1.1f, 1.2f);
            releasedSound.Play();
        }
    }
}