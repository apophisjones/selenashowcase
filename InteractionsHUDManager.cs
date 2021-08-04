using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionsHUDManager : MonoBehaviour
{
    private HUD _HUD;
    private CanvasGroup _canvasGroup;

    private float _alpha = 1;
    private void Awake()
    {
          _HUD = GameObject.Find("HUD").GetComponent<HUD>();
        if (_HUD == null)
        {
            Debug.LogError("InteractionsHUDManager::HUD is null");
        }

        _canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            Debug.LogError("InteractionsHUD::CanvasGroup is null");
        }
    }

    private void Update()
    {
        _canvasGroup.alpha = _alpha;
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void LowerOpacity()
    {
        StartCoroutine(ChangeOpacity(1f, 0.25f, 0.25f));
    }

    public void RaiseOpacity()
    {
        StartCoroutine(ChangeOpacity(_alpha, 1f, 0.25f));
    }
    IEnumerator ChangeOpacity(float a_start, float a_end, float dur)
    {
        float elapsed = 0.0f;
        while (elapsed < dur )
        {
            _alpha = Mathf.Lerp( a_start, a_end, elapsed / dur);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _alpha = a_end;
    }
}
