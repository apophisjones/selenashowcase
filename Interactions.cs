using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Interactions : MonoBehaviour
{
    //    ----PLAYER----
    [SerializeField] public bool _interactable = false;
    private Player _player;
    private PlayerVision _playerVision;

    //    ----UTILITIES----
    private bool _useReady = true;
    
    //    ----CAMERA----
    [SerializeField] private GameObject _mainCamera;
    private MainCamera _mainCameraScript;
    [SerializeField] private CinemachineFreeLook _mainMotor;
    private CinemachineFreeLook _InteractionsPivot;
    
    //    ----UI----
    public Transform _descBackground;
    [SerializeField] public TextMeshProUGUI _descTitle; 
    [SerializeField] public GameObject _descPrefab;
    [SerializeField] public string _setDescTitle;
    [SerializeField] public TextMeshProUGUI _firstResponse;
    [SerializeField] public TextMeshProUGUI _secondResponse;
    [SerializeField] public TextMeshProUGUI _thirdResponse;
    [SerializeField] public string _setFirstResponse;
    [SerializeField] public string _setSecondResponse;
    [SerializeField] public string _setThirdResponse;

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player").GetComponent<Player>();
            if (_player == null)
            {
                Debug.LogError("Player is null");
            }

        _mainCamera = GameObject.FindWithTag("MainCamera").gameObject;
        if (_mainCamera == null)
        {
            Debug.LogError("Main Camera is null");
        }

        _mainMotor = GameObject.Find("Main_Motor").GetComponent<CinemachineFreeLook>();
        if (_mainMotor == null)
        {
            Debug.LogError("Main Motor is null");
        }
        _playerVision = GameObject.FindWithTag("PlayerVision").GetComponent<PlayerVision>();
        _InteractionsPivot = transform.Find("Interactions_Camera_Pivot").GetComponent<CinemachineFreeLook>();
        if (_InteractionsPivot == null)
        {
            Debug.LogError("interactions::no pivot");
        }
        
        _descTitle = GameObject.Find("Desc_Title").GetComponent<TextMeshProUGUI>();
        if (_descTitle == null)
        {
            Debug.LogError("Description title is null", gameObject);
        } 
        _descBackground = GameObject.Find("Desc_Background").transform;

        _firstResponse = GameObject.Find("First_Response_Text").GetComponent<TextMeshProUGUI>();
        _secondResponse = GameObject.Find("Second_Response_Text").GetComponent<TextMeshProUGUI>();
        _thirdResponse = GameObject.Find("Third_Response_Text").GetComponent<TextMeshProUGUI>();
    }
        private void Update()
    {
        if (_playerVision._lookingAtInteractableObject == true)
        {
            _interactable = true;
        }
        else
        {
            _interactable = false;
        }
        CameraUnfocus();
        RotateCamera();
    }
        
        public void ServePurpose()
    {
        if (_useReady && _mainMotor.Priority == 10)
        {
            _InteractionsPivot.Priority = 12;
            _mainMotor.Priority = 2;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            _InteractionsPivot.m_XAxis.m_MaxSpeed = 0;
            _InteractionsPivot.m_YAxis.m_MaxSpeed = 0;
            _player.StartCoroutine(UseDelay());
            _player.InteractionsHUD();
            FreezeCharacter();
            SetUIText();
        }
    }

    void SetUIText()
    {
        Instantiate(_descPrefab, _descBackground);
        _descTitle.text = _setDescTitle;
        _firstResponse.text = _setFirstResponse;
        _secondResponse.text = _setSecondResponse;
        _thirdResponse.text = _setThirdResponse;
    }
    
    public void CameraUnfocus()
    {
        if (Input.GetKeyDown(KeyCode.E) && _useReady && _InteractionsPivot.Priority == 12)
        {
            Destroy(GameObject.FindWithTag("Description_Prefab"));
            _InteractionsPivot.Priority = 2;
            _mainMotor.Priority = 10;
            StartCoroutine(UseDelay());
            _player.GameHUD();
            UnfreezeCharacter();
        }
    }

    public IEnumerator UseDelay()
    {
        _useReady = false;
        yield return new WaitForSeconds(0.1f);
        _useReady = true;
        yield break;
    }
    
    public void FreezeCharacter()
    {
        _player._playerCanMove = false;
    }
    
    public void UnfreezeCharacter()
    {
        _player._playerCanMove = true;
    }

    public void RotateCamera()
    {
        if (_InteractionsPivot.Priority == 12)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                _player.HUDLowerOpacity();
                UnityEngine.Cursor.visible = false;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                _InteractionsPivot.m_XAxis.m_MaxSpeed = 300;
                _InteractionsPivot.m_YAxis.m_MaxSpeed = 2;
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
                _player.HUDRaiseOpacity();
                _InteractionsPivot.m_XAxis.m_MaxSpeed = 0;
                _InteractionsPivot.m_YAxis.m_MaxSpeed = 0;
            }
        }
    }
}