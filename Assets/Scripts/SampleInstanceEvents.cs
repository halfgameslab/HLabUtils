using Mup.EventSystem.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleInstanceEvents : MonoBehaviour
{
    void Start()
    {
        Debug.Log(IS_InstanceManager.GetInstanceByID("Teste123", "SampleScene"));
        Debug.Log(IS_InstanceManager.GetInstanceByID("Teste123", "Demo"));
        Debug.Log(IS_InstanceManager.GetInstanceByID("Teste123", "MultilanguageDemo"));
        Debug.Log(IS_InstanceManager.GetInstanceByID("11112", "Demo"));
        Debug.Log(IS_InstanceManager.GetAllInstancesInOpenScenesByID("Teste123").Length);

        foreach(System.Object obj in IS_InstanceManager.GetAllInstancesInOpenScenesByID("Teste123"))
        {
            Debug.Log(obj);
        }

        this.SetInstanceName("Facebook");
        Debug.Log(this.GetInstanceName());

        this.AddEventListener("ET", TestET1);
        this.AddEventListener("ET", TestET2);
        this.AddEventListener("ET", TestET3);

        this.AddEventListener("ES", Test1);
        this.AddEventListener("ES", Test2);
        this.AddEventListener("ES", Test3);

        this.AddEventListener("ES", (e)=>print("Funcionou1"));
        this.AddEventListener("ES", (e) => print("Funcionou2"));
        this.AddEventListener("ES", (e) => print("Funcionou3"));
        this.DispatchEvent("ES");

        this.SetInstanceName("Facebook2");
        Debug.Log(this.GetInstanceName());
        this.AddEventListener("ES", (e) => print("Funcionou4"));
        //this.DispatchEvent("ES");

        print(this.HasEventListener("ES", Test1));
        print(this.HasEventListener("ES", Test2));
        print(this.HasEventListener("ES", Test3));
        print(this.HasEventListener("ES", Test4));

        //this.RemoveAllEventListeners();
        //print("Removed");
        //this.RemoveEventListener("ES", Test2);
        //this.RemoveEventListener("ES", Test4);
        //this.RemoveEventListeners("ES");

        print(this.HasEventListener("ES", Test1));
        print(this.HasEventListener("ES", Test2));
        print(this.HasEventListener("ES", Test3));
        print(this.HasEventListener("ES", Test4));

        print(this.HasEventListener("ET", TestET1));
        print(this.HasEventListener("ET", TestET2));
        print(this.HasEventListener("ET", TestET3));
        
        this.DispatchEvent("ES");
        this.DispatchEvent("ET");
    }

    public void Test1(ES_Event ev)
    {
        print("1");
    }

    public void Test2(ES_Event ev)
    {
        print("2");
    }

    public void Test3(ES_Event ev)
    {
        print("3");
    }

    public void Test4(ES_Event ev)
    {
        print("4");
    }

    public void TestET1(ES_Event ev)
    {
        print("ET1");
    }

    public void TestET2(ES_Event ev)
    {
        print("ET2");
    }

    public void TestET3(ES_Event ev)
    {
        print("ET3");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
