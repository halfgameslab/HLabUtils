using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mup.ShopSystem;
using Mup.ShopSystem.Data;
using Mup.EventSystem.Events;
using System;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public static List<M_GenericItem> MyItems { get { return Instance._myItens; } }
    private List<M_GenericItem> _myItens = new List<M_GenericItem>();

    //To select skin
    public int SelectedSkin;

    Inventory()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        SelectedSkin = PlayerPrefs.GetInt("MUP_CurSelectSkin");
        foreach (M_GenericItem item in M_ItemDataManager.GetAllAvaliableProducts())
        {
            if (PlayerPrefs.GetString("ITEM_BOUGHT" + item.Id) == "true")
            {
                _myItens.Add(item);
            }
        }
        IS_InstanceManager.GetInstanceByID("ShopManager").AddEventListener(M_ShopEvents.ON_SUCCESS, OnSuccesBuy);
        UserData.Instance.InitSavedData();
    }

    private void OnSuccesBuy(ES_Event ev)
    {
        M_GenericItem item = (M_GenericItem)ev.Data;
        PlayerPrefs.SetString("ITEM_BOUGHT" + item.Id, "true");
        if (_myItens.Contains(item))
        {
            return;
        }
        else
        {
            _myItens.Add(item);
        }
    }

    private void OnDisable()
    {
        IS_InstanceManager.GetInstanceByID("ShopManager").RemoveEventListener(M_ShopEvents.ON_SUCCESS, OnSuccesBuy);
    }

    public static void Equip(M_GenericItem item)
    {
        if (item.Id == Instance.SelectedSkin)
        {
            PlayerPrefs.SetInt("MUP_CurSelectSkin", 0);
            Instance.SelectedSkin = 0;
        }
        else
        {
            PlayerPrefs.SetInt("MUP_CurSelectSkin", item.Id);
            Instance.SelectedSkin = item.Id;
        }
    }
}