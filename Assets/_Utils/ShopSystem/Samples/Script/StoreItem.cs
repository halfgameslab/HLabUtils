using System.Collections;
using System.Collections.Generic;
using Mup.ShopSystem.Data;
using UnityEngine;

public class StoreItem : M_GenericItem
{
    [SerializeField] internal Sprite _itemSprite;
    public  Sprite ItemSpite { get { return _itemSprite; } }
    
}