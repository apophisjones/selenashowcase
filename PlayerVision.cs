using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.UnityGUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVision : MonoBehaviour
{
    [SerializeField] public bool _lookingAtInteractableObject = false;
    [SerializeField] private GameObject _interactionPrompt;
    private Interactions _interactions;
    private Player _player;
    private MainCamera _mainCamera;
    private SphereCollider _sphereCollider;
    public bool _lookingAtDialogueVictim = false;
    private Dialogue _dialogue;
    private DialogueHub _dialogueHub;

    void Awake()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.LogError("Player is null");
        }
        _mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<MainCamera>();
        if (_mainCamera == null)
        {
            Debug.LogError("Main Camera is null");
        }

        _sphereCollider = gameObject.GetComponent<SphereCollider>();
        if (_sphereCollider == null)
        {
            Debug.LogError("PlayerVision::Mesh Collider is Null");
        }
        _dialogueHub = GameObject.Find("Dialogue_Hub").GetComponent<DialogueHub>();
    }
    void Update()
    {
        ShowPrompt();
        Interaction();
        UnfocusCollider();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Interactable" && _mainCamera._mainCameraActive)
        {
            _lookingAtInteractableObject = true; 
            _interactions = other.GetComponent<Interactions>();
        }  
        else if (other.tag == "Dialogue_Interactable" && _mainCamera._mainCameraActive)
        {
            _lookingAtDialogueVictim = true;
            _dialogue = other.GetComponent<Dialogue>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _lookingAtInteractableObject = false;
        _lookingAtDialogueVictim = false;
    }

    void ShowPrompt()
    {
        if (_lookingAtInteractableObject || _lookingAtDialogueVictim)
        {
            _interactionPrompt.gameObject.SetActive(true);
        }
        else
        {
            _interactionPrompt.gameObject.SetActive(false);
        }
    }

    void Interaction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        { 
            if (_lookingAtInteractableObject)
            {
                Debug.Log("I have interacted with this object");
                _interactions.ServePurpose();
            }
            else if (_lookingAtDialogueVictim)
            {
                _dialogueHub.ShowHub(_dialogue);
            }
        }
    }

    void UnfocusCollider()
    {
        if (_mainCamera.gameObject.activeInHierarchy == false)
        {
            _sphereCollider.enabled = false;
            _interactionPrompt.gameObject.SetActive(false);
        }
        else if (_mainCamera.gameObject.activeInHierarchy)
        {
            _sphereCollider.enabled = true;
        }
    }
}