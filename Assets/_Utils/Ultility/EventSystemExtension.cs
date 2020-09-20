// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventSystemExtension.cs" company="Dauler Palhares">
//  © Copyright Dauler Palhares da Costa Viana 2017.
//          http://github.com/DaulerPalhares
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ultility
{
    [Serializable]
    public class E_String : UnityEvent<string>{}
    [Serializable]
    public class E_GameObject : UnityEvent<GameObject> { }
    [Serializable]
    public class E_Bool : UnityEvent<bool> { }
    [Serializable]
    public class E_Int : UnityEvent<int> { }
    [Serializable]
    public class E_Float : UnityEvent<float> { }
}