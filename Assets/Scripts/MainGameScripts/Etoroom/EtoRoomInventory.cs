using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EtoRoomInventory : MonoBehaviour
{
    [SerializeField] private GameObject _etoinventor;
    [SerializeField] private Button _inventoryButton;
    // Start is called before the first frame update
    void Start()
    {
        _inventoryButton.onClick.AddListener(inventoryUpdate);
    }

    // Update is called once per frame
    private void inventoryUpdate()
    {
        _etoinventor.SetActive(true);
    }
}
