using Mup.ShopSystem.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Class that handle all the item database information.
/// </summary>
public class M_ItemDataManager : MonoBehaviour
{
    private static M_ItemDataManager _instance;
    /// <summary>
    /// Static reference to this stance.
    /// </summary>
    public static M_ItemDataManager Instance
    {
        get {
            if (_instance == null)
            {
                _instance = FindObjectOfType<M_ItemDataManager>();
            }
            return _instance;
        }
    }

    [SerializeField] private M_ItemDatabase _itemDatabase;
    /// <summary>
    /// Default constructor.
    /// </summary>
    
    private void OnEnable()
    {
        if(Instance == this)
        {
            name = "(MUP) Database Manager";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject,5f);
            return;
        }
        if (_itemDatabase == null)
        {
            Debug.LogError("<color=red>[ERROR]</color> <b>Assign the ItemData in the M_ItemDataManager component</b> ");
            #if UNITY_EDITOR
            Debug.LogWarning("<color=yellow>[EXIT PLAY] Forced Exit Play.</color>");
            EditorApplication.isPlaying = false;
            #endif
            return;
        }
        foreach (string currency in _itemDatabase.PaymentMethods)
        {
            if (!PlayerPrefs.HasKey("MUP_" + currency))
            {
                print(string.Concat("MUP_" + currency + " Set to 0"));
                PlayerPrefs.SetInt(string.Concat("MUP_" + currency), 0);
            }
        }
    }

    public string[] PaymentMethod()
    {
        return _itemDatabase.PaymentMethods;
    }

    /// <summary>
    /// Get the product information.
    /// </summary>
    /// <param name="productName">product name to check</param>
    /// <returns>Return the element on the database with that specific name.</returns>
    public static M_GenericItem GetProductInfo(string productName)
    {
        return Instance._itemDatabase.FetchItemByName(productName);
    }
    /// <summary>
    /// Get the product information.
    /// </summary>
    /// <param name="productName">product id to check</param>
    /// <returns>Return the element on the database with that specific id.</returns>
    public static M_GenericItem GetProductInfo(int productId)
    {
        return Instance._itemDatabase.FetchItemById(productId);
    }
    /// <summary>
    /// Get all Avaliable products.
    /// </summary>
    /// <returns>A list with all Avaliable products in the database.</returns>
    public static List<M_GenericItem> GetAllAvaliableProducts()
    {
        return Instance._itemDatabase._itemData;
    }
    /// <summary>
    /// Get the product value.
    /// </summary>
    /// <param name="itemToCheck">Product to check the value</param>
    /// <param name="paymentMethod">Payment method to check the value</param>
    /// <returns>Value of that product related to that payment.</returns>
    public static int GetProductValue(M_GenericItem itemToCheck, string paymentMethod)
    {
        return itemToCheck.ItemValue(paymentMethod);
    }
}