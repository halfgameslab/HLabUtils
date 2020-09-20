using UnityEngine;
using System.Collections;
using Mup.ShopSystem;
using Mup.ShopSystem.Data;
using Mup.ShopSystem.UI;

public class M_StorePopulate : MonoBehaviour
{
    /// <summary>
    /// Payment that all buttons will have. (use the same used in the database).
    /// </summary>
    [SerializeField] private string _payment;
    /// <summary>
    /// Prefab of the button.
    /// </summary>
    [SerializeField] private M_ItemButton _storeItemPrefab;
    /// <summary>
    /// Check if already populated first time.
    /// </summary>
    private bool _alreadPopulated;

    private void OnEnable()
    {
        if (!_alreadPopulated)
        {
            PopulateStoreWIndow();
        }
    }
    /// <summary>
    /// Create all button prefabs and initialize them.
    /// </summary>
    private void PopulateStoreWIndow()
    {
        foreach (M_GenericItem item in M_ItemDataManager.GetAllAvaliableProducts())
        {
            M_ItemButton temp = Instantiate(_storeItemPrefab, transform);
            temp.Init(item, true, _payment);
        }
        _alreadPopulated = true;
    }

}
