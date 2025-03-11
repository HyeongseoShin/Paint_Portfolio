/*
레벨 에디터의 모드가 Edit - Play 바뀔 때마다 실제 객체를 Instantiate / Destroy 하는 스크립트
CameraToWorld.cs에서 생성된 Edit 객체 내에 Position.cs 존재해서 모드 바뀔 때마다 객체 On/Off
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public class Position : MonoBehaviour
{
    [SerializeField]
    private GameObject IngameObject;
    public GameObject Instantiate_Object;

    [SerializeField]
    private GameObject Photozone;
    public GameObject Photozone_Object;
    [SerializeField]
    private Mobile_UI MobileUI;
    [SerializeField]
    private bool Tutorial;
    [SerializeField]
    private GameObject Tutorial_Object;
    public int Layer;
    public int ID;
    public int TileID;

    public bool isPlay;
    public event EventHandler OnTileModeChanged;
    public int _tileMode;
    public int tileMode
    {
        get => _tileMode;
        set
        {
            _tileMode = value;
            OnTileModeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [SerializeField]
    private Color[] initColor;
    [SerializeField]
    private SpriteRenderer[] sr;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Sprite[] eachImg; //[0] 색상, [1] color blind

    [SerializeField]
    private SpriteRenderer hilightedSpriteRenderer;

    [SerializeField]
    private Sprite[] hilightedEachImg; //[0] 색상, [1] color blind
    private void Start()
    {
        if(spriteRenderer != null)
        {
            spriteRenderer.sprite = !DataController.Instance.gameData.isColorFilterAssistant ? eachImg[0] : eachImg[1];
        }
        if(hilightedSpriteRenderer != null)
        {
            hilightedSpriteRenderer.sprite = !DataController.Instance.gameData.isColorFilterAssistant ? hilightedEachImg[0] : hilightedEachImg[1];
        }
        

        isPlay = false;
        OnTileModeChanged += Instance_modeChanged;
        sr = gameObject.GetComponentsInChildren<SpriteRenderer>();
        initColor = new Color[sr.Length];

        var index = 0;
        foreach(SpriteRenderer temp in sr){
            initColor[index] = new Color(temp.color.r,temp.color.g,temp.color.b,temp.color.a);
            index++;
        }
        if(tileMode == 1){
            foreach(SpriteRenderer temp in sr){
                temp.color = new Color(1f,1f,1f,0.4f);
                index++;
            }
        }
        
    }

    private void Instance_modeChanged(object sender, System.EventArgs e){
        TileModeChanged(); 
    }

    private void TileModeChanged(){
        if(isPlay){
            if(tileMode == 1){
                var index1 = 0;
                foreach(SpriteRenderer temp in sr){
                    temp.color = initColor[index1];
                    index1++;
                }
            }
        }else{
            if(tileMode == 1){
                var index2 = 0;
                foreach(SpriteRenderer temp in sr){
                    if(temp!= null){
                        temp.color = new Color(initColor[index2].r,initColor[index2].g,initColor[index2].b,0.4f);
                    }
                    
                }
                index2++;
            }else{
                var index3 = 0;
                foreach(SpriteRenderer temp in sr){
                    if(temp != null){
                        temp.color = initColor[index3];
                        index3++;
                    }
                    
                }
            }
        }
    }
    
    public void Play(){
        isPlay = true;
        DebugX.Log("Play_on_Position Play: " + isPlay.ToString());
        if(IngameObject){
            Instantiate_Object= Instantiate(IngameObject,this.transform.position,this.transform.rotation);
            
            Instantiate_Object.transform.SetParent(this.transform);

            SpriteRenderer[] sr = Instantiate_Object.GetComponentsInChildren<SpriteRenderer>();

            foreach(SpriteRenderer sprite in sr) 
            {
                sprite.sortingOrder += this.transform.GetComponent<Position>().ID;
                if(sprite.gameObject.layer == LayerMask.NameToLayer("Items")) 
                {
                    if (sprite.gameObject.transform.Find("NopeYouCannot") != null) 
                    {
                        sprite.gameObject.transform.Find("NopeYouCannot").transform.GetComponent<SpriteRenderer>().sortingOrder += this.transform.GetComponent<Position>().ID;
                    }
                    if (sprite.gameObject.transform.Find("Brush") != null) 
                    {
                        sprite.gameObject.transform.Find("Brush").transform.GetComponent<SpriteRenderer>().sortingOrder += this.transform.GetComponent<Position>().ID;
                    }
                }
            }
            

            if(1< TileID && TileID < 7)
            {
                if(SceneManager.GetActiveScene().name != "HomePlayGrid_Tutorial" && !this.gameObject.scene.name.Contains("Result") && !this.gameObject.scene.name.Contains("Ach") && !this.gameObject.scene.name.Contains("HomeGrid_NewAssets"))
                {
                    Photozone_Object= Instantiate(Photozone,this.transform.position,this.transform.rotation);
                    
                    Photozone_Object.GetComponent<toZoomOut>().InitFuncOnMapEditor();
                    Photozone_Object.transform.SetParent(this.transform);
                }
                if(this.gameObject.scene.name.Contains("AchDetail"))
                {
                    if(DataController.Instance.gameData.achDetailSceneIndex == 0 || DataController.Instance.gameData.achDetailSceneIndex == 1)
                    {    
                        Photozone_Object= Instantiate(Photozone,this.transform.position,this.transform.rotation);
                        Photozone_Object.GetComponent<toZoomOut>().InitFuncOnMapEditor();
                        Photozone_Object.transform.SetParent(this.transform);
                    }
                }
                
            }
            if(this.transform.tag=="Player_Position")
            {
            
                if(GameObject.Find("MapEditor_CO")!=null)
                {
                    GameObject.Find("MapEditor_CO").transform.Find("CM vcam2D").GetComponent<Cinemachine.CinemachineVirtualCamera>().m_Follow= Instantiate_Object.transform;
                }
                else if( GameObject.Find("MapEditorUINew_Neccessary") !=null)
                {
                    GameObject.Find("MapEditorUINew_Neccessary").transform.Find("CM vcam2D").GetComponent<Cinemachine.CinemachineVirtualCamera>().m_Follow= Instantiate_Object.transform;

                }
                
            }
        }
        
        TileModeChanged();
        if(this.name =="DoorPosition(Clone)")
        {
            if(Tutorial)
            {
                Instantiate_Object.GetComponent<ItemScripts>().Tutorial = true;
                Instantiate_Object.GetComponent<ItemScripts>().Tutorial_Object = Tutorial_Object;
                Instantiate_Object.GetComponent<ItemScripts>().IsEditing=false;
            }
            else
            {
                 Instantiate_Object.GetComponent<ItemScripts>().IsEditor=true;
                 if(GameObject.Find("EditorController").GetComponent<Edit_PlayController>().isOthers)
                 {
                    Instantiate_Object.GetComponent<ItemScripts>().IsEditorPlayMode = true;
                 }
                 else
                 {
                    Instantiate_Object.GetComponent<ItemScripts>().IsEditorPlayMode = false;
                 }
            }
        }
        if(this.name == "BrushPosition(Clone)")
        {
            DebugX.Log("Is BrushPosition");
            Instantiate_Object.GetComponent<ItemScripts>().starBox = GameObject.Find("EditorController").GetComponent<Edit_PlayController>().Star;
        }
       
    }
    public void Edit(){
        isPlay = false;

        TileModeChanged();
        if(Instantiate_Object)
        {
            Destroy(Instantiate_Object);
        }
        else
        {
            if(IngameObject!= null)
            {
                if(gameObject.transform.childCount > 1)
                {
                    Destroy(gameObject.transform.GetChild(1).gameObject);
                }
                
            }
        }
        if(Photozone_Object)
        {
            Photozone_Object.GetComponent<toZoomOut>().SetBeforeDestroy();
            Destroy(Photozone_Object);
        }
    }
}
