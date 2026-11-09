using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class playerSlots : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("General")]
    public List<itemType> inventoryList;
    public int selectedItem;
    public float playerReach;

    [Space(20)]
    [Header("Keys")]
    [SerializeField] KeyCode pickItemKey;

    [Space(20)]
    [Header("Item gameObjects")]
    [SerializeField] GameObject brawlerItem;
    [SerializeField] GameObject shooterItem;
    [SerializeField] GameObject supportItem;
    [SerializeField] GameObject sheilderItem;
    [SerializeField] GameObject bar;

    [Space(20)]
    [Header("UI")]
    [SerializeField] Image[] inventorySlotsImage = new Image [9];
    [SerializeField] Image[] inventoryBackImage = new Image[9];
    [SerializeField] Sprite emptySlotSprite;


    [SerializeField] Camera cam;
    [SerializeField] GameObject pickUpItem_gameObject;

    private Dictionary<itemType, GameObject> itemSetActive = new Dictionary<itemType, GameObject>() { }; 


    // Update is called once per frame
    void Start()
    {
        itemSetActive.Add(itemType.Brawler, brawlerItem);
        itemSetActive.Add(itemType.Shooter, shooterItem);
        itemSetActive.Add(itemType.Support, supportItem);
        itemSetActive.Add(itemType.Sheilder, sheilderItem);

        if (inventoryList.Count > 0)
        {
            selectedItem = 0;
            NewItemSelected();
        }
    }
    private void Update()
    {
        //Item pickup
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, playerReach))
        {
            IPickable item = hitInfo.collider.GetComponent<IPickable>();//camera selection
            if (item != null)
            {
                pickUpItem_gameObject.SetActive(true);
                if (Input.GetKey(pickItemKey))
                {
                    inventoryList.Add(hitInfo.collider.GetComponent<charPickup>().itemScriptableObject.item_Type);//press f to pick
                    item.PickItem();
                }

            }
            else
            {
                pickUpItem_gameObject.SetActive(false);
            }
        }
        else
        {
            pickUpItem_gameObject.SetActive(false);
        }


        //UI
        for (int i = 0; i < 4; i++)// char icon appear when pcikup
        {
            if (i < inventoryList.Count)
            {
                inventorySlotsImage[i].sprite = itemSetActive[inventoryList[i]].GetComponent<holdfour>().itemScriptableObject.item_sprite;
            }
            else
            {
                inventorySlotsImage[i].sprite = emptySlotSprite;
            }
        }
        for (int i = 0; i < 1; i++)// bar apear when pickup
        {
            if (i < inventoryList.Count)
            {
                bar.SetActive(true);
            }
            else
            {
                bar.SetActive(false);
            }
        }
        int a = 0;
        foreach (Image image in inventoryBackImage) //Color change when selection
        {
            if (a == selectedItem)
            {
                image.color = new Color32(145, 145, 126, 255);
            }
            else
            {
                image.color = new Color32(219, 219, 219, 255);
            }
            a++;
        }





        if (Input.GetKeyDown(KeyCode.Alpha1) && inventoryList.Count > 0)//key selection
        {
            selectedItem = 0;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && inventoryList.Count > 1)
        {
            selectedItem = 1;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && inventoryList.Count > 2)
        {
            selectedItem = 2;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && inventoryList.Count > 3)
        {
            selectedItem = 3;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) && inventoryList.Count > 4)
        {
            selectedItem = 4;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha6) && inventoryList.Count > 5)
        {
            selectedItem = 5;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha7) && inventoryList.Count > 6)
        {
            selectedItem = 6;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha8) && inventoryList.Count > 7)
        {
            selectedItem = 7;
            NewItemSelected();
        }
        if (Input.GetKeyDown(KeyCode.Alpha9) && inventoryList.Count > 8)
        {
            selectedItem = 8;
            NewItemSelected();
        }
       
    }

    private void NewItemSelected()
    {
        if (inventoryList == null || inventoryList.Count == 0)
            return;

        if (selectedItem < 0 || selectedItem >= inventoryList.Count)
            return;

        brawlerItem.SetActive(false);
        shooterItem.SetActive(false);
        sheilderItem.SetActive(false);
        supportItem.SetActive(false);

        GameObject selectedItemGameobject =
            itemSetActive[inventoryList[selectedItem]];

        selectedItemGameobject.SetActive(true);
    }
}

public interface IPickable
{
    void PickItem();
}
