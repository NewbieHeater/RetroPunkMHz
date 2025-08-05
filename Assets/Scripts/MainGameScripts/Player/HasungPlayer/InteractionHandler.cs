using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class InteractionHandler : MonoBehaviour
{
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private IInteractable currentTarget;

    private void Awake()
    {
        if (promptUI) promptUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentTarget = interactable;
            promptText.text = interactable.GetInteractPrompt();
            if (promptUI) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<IInteractable>() == currentTarget)
        {
            currentTarget = null;
            if (promptUI) promptUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (currentTarget != null && Input.GetKeyDown(interactKey))
        {
            if (promptUI) promptUI.SetActive(!promptUI.activeSelf);
            currentTarget.Interact();
        }
            
    }
}
