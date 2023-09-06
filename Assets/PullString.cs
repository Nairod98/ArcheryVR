using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PullString : XRBaseInteractable
{
    public static event Action<float> PullActionReleased;

    public Transform start, end;
    public GameObject notch;
    public float PullStrength { get; private set; } = 0.0f;

    private LineRenderer _lineRenderer;
    private IXRSelectInteractor _pullingInteractor = null;

    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();
        _lineRenderer = GetComponent<LineRenderer>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void SetPullInteractor(SelectEnterEventArgs args)
    {
        _pullingInteractor = args.interactorObject;
    }

    public void Release()
    {
        PullActionReleased?.Invoke(PullStrength);
        _pullingInteractor = null;
        PullStrength = 0.0f;
        notch.transform.localPosition = new Vector3(notch.transform.localPosition.x, notch.transform.localPosition.y, 0f);
        UpdateString();
        PlayReleaseSound();
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            if (isSelected) 
            {
                Vector3 pullPosition = _pullingInteractor.transform.position;
                PullStrength = CalculatePull(pullPosition);

                UpdateString();
                HapticFeedback();
            }
        }
    }

    private float CalculatePull(Vector3 pullPosition) 
    {
        Vector3 pullDirection = pullPosition - start.position;
        Vector3 targetDirection = end.position - start.position;
        float maxLength = targetDirection.magnitude;

        targetDirection.Normalize();
        float pullValue = Vector3.Dot(pullDirection, targetDirection) / maxLength;
        return Mathf.Clamp(pullValue, 0, 1);
    }

    private void UpdateString()
    {
        Vector3 linePosition = Vector3.forward * Mathf.Lerp(start.transform.localPosition.z, end.transform.localPosition.z, PullStrength);
        notch.transform.localPosition = new Vector3(notch.transform.localPosition.x, notch.transform.localPosition.y, linePosition.z + 0.2f);
        _lineRenderer.SetPosition(1, linePosition);
    }

    private void PlayReleaseSound()
    {
        _audioSource.Play();
    }

    private void HapticFeedback()
    {
        if (_pullingInteractor != null)
        {
            ActionBasedController currentController = _pullingInteractor.transform.gameObject.GetComponent<ActionBasedController>();
            currentController.SendHapticImpulse(PullStrength, 0.1f);
        }
    }
}