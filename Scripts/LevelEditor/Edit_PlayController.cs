/*
커스텀 레벨 & 일반 스테이지에서 맵의 정보를 저장해 맵을 Load & Save 하는 스크립트

모든 레벨은
레벨을 제작하는 상태인 Edit 모드
레벨을 플레이하는 상태인 Play 모드가 있음

struct Editors
- Edit 모드에서 맵 내의 각 객체의 정보를 저장하는 구조체 (Id, position, rotation, etc)

struct EditingObject
- Play 모드에서 실제 생성된 객체의 정보를 저장하는 구조체

맵 내 객체 정보 List
- List<Editors> LoadedEditingObjects : Json 파일을 통해 읽어온 초기의 맵 구성 요소 정보 (읽어올 때 이후로 변경 X)
- List<Editors> EditingObjects : 현재 맵에 존재하는 모든 구성요소 (맵을 편집할 때마다 변경됨) => Json 형태로 서버에 업로드 되는 대상
- List<EditingObject> EditingControlObject : 현재 맵에 존재하는 모든 구성요소의 실제 객체 정보 (Pooling을 통해 생성 or 위치 변경됨) (맵을 편집할 때마다 변경됨)

주요 제작 기능
게임에서 존재하는 모든 맵의 Load() & Save() 기능 구현
- Load~~() : 일반 스테이지, 커스텀 레벨, 홈 화면 등의 게임 내에 존재하는 "모든 맵"을 각각 서버 / 로컬로 Load
- Save~~() : 일반 스테이지, 커스텀 레벨, 홈 화면 등의 게임 내에 존재하는 "모든 맵"을 각각 서버 / 로컬로 Save

MapEditorManager에서 curretnCustomMap을 통해 보낸 정보를 토대로 해당 씬을 로드
- InitEditor() : Awake()에서 실행되어 해당 맵의 정보를 읽어 알맞은 형태로 맵을 로드(Play / Edit모드, 미리 보기 / 실제 플레이인지 구분)
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using BackEnd;
using UnityEngine.UI;
using System;
using System.Security.Cryptography;
using TMPro;
using UnityEngine.SceneManagement;
using Steamworks;
using UnityEngine.EventSystems;

[Serializable]
public class Serialization<T> { // 리스트는 직렬화 별도로 해야함!
    [SerializeField]
    List<T> target;
    public List<T> ToList() { return target; }

    public Serialization(List<T> target) {
        this.target = target;
    }

}

public class Edit_PlayController : MonoBehaviour
{

    [System.Serializable]public struct Editors
    {
        public Editors(int i,Vector3 p, Vector3 r, int t, int l,float Int, int m){
            ID=i;
            TileCode=t;
            layer =  l;
            position=p;
            rotation=r;
            InteractionLayer =Int;
            tileMode = m;
        }
        public void IDSettor(int newID){
            ID= newID;
        }
        public int ID;
        public int TileCode;
        public int layer;
        public float InteractionLayer;
        
        public Vector3 position; 
        public Vector3 rotation; 

        public int tileMode;
        
    }
    [System.Serializable]public struct EditingObject
    {
        public EditingObject(Editors e, GameObject o){
           editor = e;
           Object = o;
        }
        public Editors editor; 
        public GameObject Object;
    }
    private int EditorsSeiralCounter=0;
    
    public List<Editors> LoadedEditingObjects = new List<Editors>();
    public List<Editors> EditingObjects = new List<Editors>();   
    public List<EditingObject> EditingControlObject = new List<EditingObject>();
    public string MapName;
    public bool NowEdit;
    [SerializeField]
    private CameraToWorld tileMapEditor;
    public string initialMapName; // 먼저 만들어진 맵을 골랐을 떄 일단 맵 에디터 맵으로 이동 후 LoadMap
    public GameObject currentCustomMap; // 맵 에디터 매니저에서 현재 무슨 맵인지 찾기
    public List<Editors> InitialGameObjectList;

    public GameObject tileMapCamera;
    public GameObject CharacterFollowCamera;
    public GameObject miniMapCamera;
    public GameObject basicUIs;
    public GameObject miniMapUI;
    public bool isCreate;

     [Header("IS Others Map")]
     public bool isOthers;
     public GameObject PlayButton;
     public Animator PlayButtonAnimator;
     public GameObject EditorUI;
     public List<GameObject> EditorObjects;
     public List<GameObject> RuningObjects;
     public List<GameObject> LineObjects = new List<GameObject>();
    

    //2022-03-01 업로드 시 업로드권 확인 후 광고패널 띄우기 위해 변수 추가
    public GameObject ticketAdsPanel;
    public GameObject Star;

    [Header("Local Save Map")]
    public GameObject savePanel;
    public Text saveResultText;
    public GameObject saveBtn;
    public GameObject saveAsBtn;
    public GameObject uploadBtn;

    [Header("Scene_Manager")]
    public Scene_Manager scm;

    [Header("ForTileMap Mode")]
    public int nowTileMode; // 0은 삭제 및 이동 가능, 1은 삭제 및 이동 불가능

    [SerializeField]
    public List<Editors> AdditionalEditors; //에디터 퍼즐 모드에서 추가적으로 설치했을 때

    [SerializeField]
    private bool isTileMapPuzzle;

    [SerializeField]
    private int possibleCreateCnt; //가능한 생성 타일 개수 기준

    [SerializeField]
    private int possibleDeleteCnt; //가능한 삭제 타일 개수 기준

    [SerializeField]
    private int additionalCreateCnt; //추가로 만든 타일 개수

    public event EventHandler OnAdditionalDeleteCntChanged;

    [SerializeField]
    private int _additionalDeleteCnt; //원래 있던 타일에서 삭제한 타일 개수


    [Header("GoRightBefore")]

    [SerializeField]
    private GoRightBefore gmGRB;

    private int additionalDeleteCnt{
        get => _additionalDeleteCnt;
        set
        {
            _additionalDeleteCnt = value;
            OnAdditionalDeleteCntChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler OnPossibilityCreateTileChanged;

    [SerializeField]
    private bool _canCreateAdditional;

    public bool canCreateAdditional
    {
        get => _canCreateAdditional;
        set
        {
            _canCreateAdditional = value;
            OnPossibilityCreateTileChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler OnPossibilityDeleteTileChanged;

    [SerializeField]
    private bool _canDeleteAdditional;

    public bool canDeleteAdditional
    {
        get => _canDeleteAdditional;
        set
        {
            _canDeleteAdditional = value;
            OnPossibilityDeleteTileChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [Header("Achievement")]
    private bool firstIn = true;
    [SerializeField]
    private int retryCount =0;

    // [SerializeField]
    // private int[] eachtilemode = new int[300];
    
    private void Awake()
    {
        InitEditor();    
    }

    private void InitEditor()
    {
        retryCount =0;
        isCreate = false;
        isTileMapPuzzle = false; 
        possibleCreateCnt = 5;
        possibleDeleteCnt = 5;
        canCreateAdditional = true;
        canDeleteAdditional = true;
        OnPossibilityCreateTileChanged += Instance_PosibilityCreateTileChanged;
        OnPossibilityDeleteTileChanged += Instance_PosibilityDeleteTileChanged;
        OnAdditionalDeleteCntChanged += Instance_AdditionalDeleteChanged;

        if(SceneManager.GetActiveScene().name != "HomeGrid_NewAssets") {
            SetPlayMode();
        }
        
        MapName = "";
        currentCustomMap = GameObject.Find("CurrentCustomMap");
        if(currentCustomMap!=null)
        {
            DebugX.Log("HSH_Debug_is LoadedMap");
            if(currentCustomMap.GetComponent<CurrentCustomMap>().currentMapName != "" && SceneManager.GetActiveScene().name != "HomeGrid_NewAssets") {
                initialMapName = currentCustomMap.GetComponent<CurrentCustomMap>().currentMapName;
                MapName = initialMapName;
                // saveAsBtn.SetActive(true); 2025-01-03 기준 주석처리.
                DebugX.Log("맵 이름: "+ currentCustomMap.GetComponent<CurrentCustomMap>().currentMapName);
                SetBasicUI(!DataController.Instance.gameData.isMapEditorPreview);
                SetMiniMapUI(DataController.Instance.gameData.isMapEditorPreview);
                LoadMap();
            }
            else if(SceneManager.GetActiveScene().name != "HomeGrid_NewAssets") {
                saveAsBtn.SetActive(false);
                SetBasicUI(!DataController.Instance.gameData.isMapEditorPreview);
                SetMiniMapUI(DataController.Instance.gameData.isMapEditorPreview);
            }
        }
        if((currentCustomMap==null||currentCustomMap.GetComponent<CurrentCustomMap>().currentMapName == ""))
        {
            DebugX.Log("HSH_Debug_is FirstMap");
            foreach(Editors now in InitialGameObjectList){
                tileMapEditor.LoadMap_TileGenerator(now.TileCode,now.position, now.rotation,now.layer,now.InteractionLayer, now.tileMode);
                DebugX.Log("is Generated"+now.TileCode);
            }
        }
        PlayButtonAnimator = PlayButton.GetComponent<Animator>();
        SetEditingMode(NowEdit);
    }
    
    private void Instance_PosibilityCreateTileChanged(object sender, System.EventArgs e){
        tileMapEditor.canCreate = canCreateAdditional;
        
    }
    private void Instance_PosibilityDeleteTileChanged(object sender, System.EventArgs e){
        tileMapEditor.canDelete = canDeleteAdditional;
        
    }

    private void Instance_AdditionalDeleteChanged(object sender, System.EventArgs e){
        canDeleteAdditional = possibleDeleteCnt > additionalDeleteCnt ? true : false;
        
    }
    
    public void ChangePlayerColor(GameObject PlayerNew){
        int delete=0;
        DebugX.Log("Called");
        for(int i= 1; i<EditingObjects.Count;i++){
            if(EditingObjects[i].TileCode==2||EditingObjects[i].TileCode==3||EditingObjects[i].TileCode==4||EditingObjects[i].TileCode==5||EditingObjects[i].TileCode==6){
                delete=i;
                break;
            }
        }
        GameObject temp = Instantiate(PlayerNew,EditingObjects[delete].position,this.transform.rotation);
        temp.transform.SetParent(GameObject.Find("0").transform);
        AddEditorObject(temp,0); 
        Destroy(EditingControlObject[delete].Object);
        DeleteEditorObject(EditingObjects[delete].ID);
        
    }

    public int AddEditorObject(GameObject temp){
        EditorsSeiralCounter++;
        DebugX.Log(temp.name);
        if(temp){
            int tempTilemode = temp.GetComponent<Position>().tileMode;
            Editors test =new Editors(EditorsSeiralCounter,temp.transform.position,temp.transform.eulerAngles,temp.GetComponent<Position>().TileID,0,0.0f, tempTilemode);
            EditingObjects.Add(test);
            if(isTileMapPuzzle){
                AdditionalEditors.Add(test);
                additionalCreateCnt = AdditionalEditors.Count;

                canCreateAdditional = additionalCreateCnt >= possibleCreateCnt ? false : true;

            }
            EditingObject testForEditingObject = new EditingObject(test, temp);
            EditingControlObject.Add(testForEditingObject);

            temp.GetComponent<Position>().ID=EditorsSeiralCounter;
        }
        return EditorsSeiralCounter;
    }
    public int AddEditorObject(GameObject temp,float InteractionLayer){
        EditorsSeiralCounter++;
        DebugX.Log(temp.name);
        if(temp){

            int tempTilemode = temp.GetComponent<Position>().tileMode;
            Editors test =new Editors(EditorsSeiralCounter,temp.transform.position,temp.transform.eulerAngles,temp.GetComponent<Position>().TileID,0,InteractionLayer, tempTilemode);
            EditingObjects.Add(test);
            if(isTileMapPuzzle && tempTilemode ==3){ //tilemode 3이란 플레이어가 새로만든 mode이다.
                AdditionalEditors.Add(test);
                additionalCreateCnt = AdditionalEditors.Count;
                canCreateAdditional = additionalCreateCnt >= possibleCreateCnt ? false : true;
            }
            EditingObject testForEditingObject = new EditingObject(test, temp);
            EditingControlObject.Add(testForEditingObject);

            temp.GetComponent<Position>().ID=EditorsSeiralCounter;
        }
        return EditorsSeiralCounter;
    }
    public int AddEditorObject(GameObject temp, int layer){
        EditorsSeiralCounter++;
        DebugX.Log(temp.name);
        if(temp){
            
            int tempTilemode = temp.GetComponent<Position>().tileMode;
            Editors test =new Editors(EditorsSeiralCounter,temp.transform.position,temp.transform.eulerAngles,temp.GetComponent<Position>().TileID,layer,0.0f, tempTilemode);
           
            EditingObjects.Add(test);
            if(isTileMapPuzzle && tempTilemode ==3){ //tilemode 3이란 플레이어가 새로만든 mode이다.
                AdditionalEditors.Add(test);
                additionalCreateCnt = AdditionalEditors.Count;
                canCreateAdditional = additionalCreateCnt >= possibleCreateCnt ? false : true;
            }

            EditingObject testForEditingObject = new EditingObject(test, temp);
            EditingControlObject.Add(testForEditingObject);

            temp.GetComponent<Position>().ID=EditorsSeiralCounter;
        }
        return EditorsSeiralCounter;
    }
     public int AddEditorObject(GameObject temp, int layer,float InteractionLayer){
        EditorsSeiralCounter++;
        DebugX.Log(temp.name);
        if(temp){
            
            int tempTilemode = temp.GetComponent<Position>().tileMode;
            Editors test =new Editors(EditorsSeiralCounter,temp.transform.position,temp.transform.eulerAngles,temp.GetComponent<Position>().TileID,layer,InteractionLayer, tempTilemode);
            EditingObjects.Add(test);
            if(isTileMapPuzzle && tempTilemode ==3){ //tilemode 3이란 플레이어가 새로만든 mode이다.
                AdditionalEditors.Add(test);
                additionalCreateCnt = AdditionalEditors.Count;
                canCreateAdditional = additionalCreateCnt >= possibleCreateCnt ? false : true;
            }

            EditingObject testForEditingObject = new EditingObject(test, temp);
            EditingControlObject.Add(testForEditingObject);

            temp.GetComponent<Position>().ID=EditorsSeiralCounter;
        }
        return EditorsSeiralCounter;
    }
    public void MoveEditorObject(GameObject MoveObject){
        DebugX.Log("Move");
        for(int i= 0; i<EditingObjects.Count;i++){
            if(EditingObjects[i].ID==MoveObject.GetComponent<Position>().ID){
                if(MoveObject.GetComponent<Position>().tileMode != 1 || tileMapEditor.nowDevelopermode){
                    DebugX.Log(MoveObject.name+"is"+MoveObject.GetComponent<Position>().ID);
                    if(MoveObject.GetComponent<LineAble>())
                    {
                        AddEditorObject(MoveObject,EditingObjects[i].layer,MoveObject.GetComponent<LineAble>().InteractionLayer);    
                    }
                    else
                        AddEditorObject(MoveObject,EditingObjects[i].layer);    
                    DeleteEditorObject(EditingObjects[i].ID);
                }
                
              
            }
        }
    } 
    public void DeleteEditorObject(int ID){
         foreach(EditingObject i in EditingControlObject){
            if(i.editor.ID==ID){
                EditingControlObject.Remove(i);
                break;
            }
        }
        foreach(Editors i in EditingObjects){
            if(i.ID==ID){
                Editors temp= i;
                EditingObjects.Remove(i);
                break;
            }
        }
        foreach(Editors i in AdditionalEditors){
            if(i.ID==ID){
                Editors temp= i;
                AdditionalEditors.Remove(i);
                additionalCreateCnt = AdditionalEditors.Count;
                canCreateAdditional = additionalCreateCnt >= possibleCreateCnt ? false : true;
                break;
            }
        }

    }
    public void Reload() {
        
        if(DataController.Instance.gameData.playMode == PlayMode.STORY){
            gmGRB = GameObject.FindWithTag("GameManager").GetComponent<GoRightBefore>();
            gmGRB.SetGBTotalCount();
        }else if(DataController.Instance.gameData.playMode == PlayMode.MAPEDITOR){
            retryCount++;
            DataController.Instance.CheckAchievementOnDC("InCMRC", retryCount, true);
            
        }

        // 24-07-12 자기 맵이 아닐때만 다시하기 업적 달성되게 하기
        if(!firstIn || DataController.Instance.gameData.playMode == PlayMode.MAPEDITOR && isOthers){
            BackendAchManager.Instance.achData.totIntVal.trc++;
            DataController.Instance.CheckAchievementOnDC("RC");
        }
        
        if(DataController.Instance.gameData.playMode == PlayMode.STORY || DataController.Instance.gameData.playMode == PlayMode.HOME){
            firstIn = false;
        }
        
        PlayButton.GetComponent<Toggle>().isOn = true;
        Invoke("Aftersecond", 0.0001f);
    }

    public void ReloadonGridNewAssets(){
        PlayButton.GetComponent<Toggle>().isOn = true;
        Invoke("Aftersecond", 0.01f);
    }

    private void Aftersecond(){
        PlayButton.GetComponent<Toggle>().isOn = false;
    }

    public void SetEditingMode(bool IsEdit){

        // 25-01-14 Play/Edit 모드 전환될 때 canOpenSetting = true로 바꿔줌
        DataController.Instance.gameData.canOpenSetting = true;
        string nowCurObjName = "";
        if(EventSystem.current.currentSelectedGameObject != null){
            nowCurObjName = EventSystem.current.currentSelectedGameObject.name;
            DebugX.Log("Now current " + nowCurObjName);
        }
        if(nowCurObjName.Equals("PlayButton_MapEditor")){
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        if(NowEdit){
            DebugX.Log("Edit Player에서 tryAnythingon");
            PlayButtonAnimator.SetBool("isEdit", false);
            if(DataController.Instance.gameData.playMode != PlayMode.HOME) {
                DataController.Instance.gameData.tryAnythingonHomeGrid = false;
            }
            tileMapCamera.GetComponent<Camera>().cullingMask = ~(1 << LayerMask.NameToLayer("MapEditor"));
            tileMapEditor.AllLayerTurnOn();
            NowEdit=!NowEdit;
            
            // 스토리모드일 때는 FullView가 처음 켜져야 되서 미리 꺼놓음
            if(SceneManager.GetActiveScene().name == "HomePlayGrid_Tutorial") {
                CharacterFollowCamera.SetActive(true);
            }
            
            
            float Xmax = -500;
            float Xmin = 500;
            float Ymax = -500;
            float Ymin = 500;

            foreach(EditingObject i in EditingControlObject){
                if(i.editor.ID!=0){
                    if(i.editor.position.x > Xmax){
                        Xmax = i.editor.position.x;
                    }
                    if(i.editor.position.x < Xmin){
                        Xmin = i.editor.position.x;
                    }
                    if(i.editor.position.y > Ymax){
                        Ymax = i.editor.position.y;
                    }
                    if(i.editor.position.y < Ymin){
                        Ymin = i.editor.position.y;
                    }
                    
                    i.Object.GetComponent<Position>().Play();    
                }
                
            }
            if(DataController.Instance.gameData.playMode == PlayMode.MAPEDITOR || DataController.Instance.gameData.playMode == PlayMode.HOME){
                GameObject.Find("CamCollider").GetComponent<SetPolyCol>().SetToInitValue();
                
            }else{
                GameObject.Find("CamCollider").GetComponent<SetPolyCol>().OnMapEditor(Xmax+0.5f, Xmin-0.5f, Ymax+0.5f, Ymin-0.5f);  
            }
            
            GameObject[] dialogues = GameObject.FindGameObjectsWithTag("Dialog");

            DebugX.Log("dialogues.Length: " + dialogues.Length);

            int chapter = 0;
            int currentStage = 0;
            
            chapter = DataController.Instance.gameData.chapter;
            currentStage = DataController.Instance.gameData.currentStage;

            int cnt = 0;

            //2024-02-05 OYJ 업적 씬을 위해 데이터 적용하기. Dialog 에 적용하는 것과 동일하게 진행한다.
            DebugX.Log("여기까지는? 1");
            if(DataController.Instance.gameData.achDetailSceneIndex > -1 && DataController.Instance.gameData.achDetailSceneIndex < 3 ){
                DebugX.Log("여기까지는? 2");
                string cateString = "";
                switch(DataController.Instance.gameData.achDetailSceneIndex){
                    case 0:
                        cateString = "Except_Play";
                        break;
                    case 1:
                        cateString = "Play";
                        break;
                    case 2:
                        cateString = "Decorate";
                        break;
                    default:
                        break;
                }
                GameObject[] trophy = GameObject.FindGameObjectsWithTag("AchieveTrophy");

                DebugX.Log("trophy.Length: " + trophy.Length);
            
                int trophyIndex = 0;

                if(trophy.Length > 0){
                    foreach(KeyValuePair<string, List<AchieveGeneralEachInfo>> aa in DataController.Instance.gameData.achGeneralInfo){
                        //먼저 얻은 업적 key 값이 있는지 확인한다.
                        int isonListIndex = BackendAchManager.Instance.achData.achName.IndexOf(aa.Key);
                        for(int i = 0; i < aa.Value.Count; i++) {
                            bool isClearTrophy = false;
                            if(cateString.Equals(aa.Value[i].Category)){
                                if(isonListIndex!= -1){
                                    //achName에 있다는 뜻은 하나 이상의 업적이 있다는 뜻.
                                    if(i < BackendAchManager.Instance.achData.achInfo[isonListIndex].achList.Count){
                                        //만약 Count가 4라고 하면 같은 Key의 업적의 4번째까지 true라는 뜻.
                                        if(BackendAchManager.Instance.achData.achInfo[isonListIndex].achList[i]){
                                            /*
                                                한 번더 true인지 확인하는 이유는, 보통은 한 개의 업적을 얻을 때마다 add를 해서 count를 보면 되지만
                                                chapter all clear, Get Award button 같은 경우 순서대로 얻지 않을 가능성이 있어서 한 번에 최대의 값을 add를 한다.
                                                따라서 index에 맞게 되는지 한번더 확인하는 것이다.
                                            */
                                            isClearTrophy = true;

                                        }

                                    }

                                }

                                if(trophyIndex < trophy.Length)
                                {
                                    trophy[trophyIndex].GetComponent<TrophyController>().SetThrophy(aa.Value[i], isClearTrophy);
                                    
                                }
                                trophyIndex++;
                            }
                        }
                    }    
                }
                
            }
            
            if(DataController.Instance.gameData.playMode != PlayMode.STORY && DataController.Instance.gameData.playMode != PlayMode.HOME){
                DebugX.Log("플레이어 on");
                DataController.Instance.gameData.playerCanMove = true;
            }else if(SceneManager.GetActiveScene().name == "HomePlayGrid_Tutorial"){
                DataController.Instance.gameData.playerCanMove = true;
            }

        }
        else{
            DebugX.Log("에디트 플레이어");
            PlayButtonAnimator.SetBool("isEdit", true);
            DataController.Instance.gameData.playerCanMove = false;
            if(DataController.Instance.gameData.playMode == PlayMode.MAPEDITOR || DataController.Instance.gameData.playMode == PlayMode.HOME){
                //MapEditor에서 Create 할 때만 정해진 크기에서 왔다갔다 하면 되고, 나머지는 무조건 있는 곳 기준으로 맵 크기가 결정되도록 한다.
                tileMapCamera.GetComponent<Cinemachine.Examples.MobileCamController>().InitMobileCamState();
            }
            
            
            tileMapCamera.GetComponent<Camera>().cullingMask = -1;
            NowEdit=!NowEdit;            
            
            CharacterFollowCamera.SetActive(false);
            foreach(EditingObject i in EditingControlObject){
               if(i.editor.ID!=0)
               i.Object.GetComponent<Position>().Edit();
            }
            
        }
     
    }
    public void SaveHomeMapJson() {
        string ToJsonData = JsonUtility.ToJson(new Serialization<Editors>(EditingObjects));

        string filePath = "";

        // 24-12-11 로그인 유형에 따라 HomeEditMapDeo.pa 저장 위치 변경
        switch(DataController.Instance.gameData.loginType)
        {
            case 0:
                filePath = Application.persistentDataPath + "/" + MapName + ".pa";
                break;
            case 1:
                if(SteamManager.Initialized)
                {
                    CSteamID SteamID = SteamUser.GetSteamID();
                    filePath = Application.persistentDataPath + "/" + SteamID + "/" + MapName + ".pa";
                }
                break;
            default:
                filePath = Application.persistentDataPath + "/" + MapName + ".pa";
                break;
        }

        // ToJsonData 암호화
        string encryptString = DataController.Instance.Encrypt(ToJsonData);
        DebugX.Log("Json 맵");
        if(File.Exists(filePath) != null){
            File.WriteAllText(filePath, encryptString);
            DebugX.Log("Json 맵 파일 저장완료: "+ MapName+".pa");
        }
    }

    public void SaveMapJson() {
        string ToJsonData = JsonUtility.ToJson(new Serialization<Editors>(EditingObjects));

        string filePath = "Assets/Resources/MapData/"+MapName+".json";

        File.WriteAllText(filePath, ToJsonData);

        DebugX.Log("Json 맵 파일 저장완료: "+ MapName+".json");
    }

    public void SaveMap(){
        
        if(DataController.Instance.gameData.customMapUploadTicket > 0) {

            string ToJsonData = JsonUtility.ToJson(new Serialization<Editors>(EditingObjects));

            BackendReturnObject bro = null;
            string nickname = BackendGameData.Instance.GetUserNickName();

            Param newMap = new Param();
            newMap.Add("MapInfo", ToJsonData);
            newMap.Add("MapName", MapName);
            newMap.Add("MadeBy", nickname);
            newMap.Add("PlayCount", 0);
            newMap.Add("Like", 0);

            bro = Backend.GameData.Insert ( "CustomMap_List", newMap);

            if (bro.IsSuccess()) {
                DebugX.Log("새로운 맵 저장 완료 무야호~~" + bro);
                savePanel.SetActive(false);

                saveResultText.gameObject.SetActive(true);
                saveResultText.color = new Color32(0, 255, 0, 0);
                saveResultText.text = "Upload Success!";
                StartCoroutine(FadeTextToFullAlpha(saveResultText));
                scm.GotoMapEditorSceneAfterUpload();
            }
            else {
                DebugX.LogError("새로운 맵 업로드에 실패했습니다. : " + bro);

                savePanel.SetActive(false);

                saveResultText.gameObject.SetActive(true);
                saveResultText.color = new Color32(255, 0, 0, 0);
                saveResultText.text = "Upload Failed!\nCheck Network!";
                StartCoroutine(FadeTextToFullAlpha(saveResultText));
            }        
        }
        else
        {
            ticketAdsPanel.SetActive(true);
        }
        
        
    }
    public void GenenrateLoadedMap(){

    }
    public void  LoadMap_Lines()
    {
        for(int Lines=0;Lines<LineObjects.Count;Lines++)
        {
            for(int i=0;i<EditingControlObject.Count;i++)
            {
                    
                if(EditingControlObject[i].Object.GetComponent<LineAble>()&&EditingControlObject[i].Object.tag!="LineRederer")
                {
                   if(LineObjects[Lines].GetComponent<LineAble>().InteractionLayer ==EditingControlObject[i].Object.GetComponent<LineAble>().InteractionLayer)
                    {
                        GameObject temp = EditingControlObject[i].Object;
                        float InterTemp = LoadedEditingObjects[i].InteractionLayer;
                        LineObjects[Lines].GetComponent<LineRendererConnector>().Positions.Add(temp.transform.GetChild(0));
                        LineObjects[Lines].GetComponent<LineRendererConnector>().positionCnt = LineObjects[Lines].GetComponent<LineRendererConnector>().Positions.Count;
                        
                       temp.GetComponent<LineAble>().InteractionLayer = InterTemp;
                       temp.GetComponent<LineAble>().Line = LineObjects[Lines];
                    }
                }
            }

        }
    }

    public void LoadHomeMapJson() {
        string filePath = "";

        // 24-12-11 로그인 유형에 따라 HomeEditMapDeo.pa 저장 위치 변경
        switch(DataController.Instance.gameData.loginType)
        {
            case 0:
                filePath = Application.persistentDataPath +"/"+ MapName+".pa";
                break;
            case 1:
                if(SteamManager.Initialized)
                {
                    CSteamID SteamID = SteamUser.GetSteamID();
                    filePath = Application.persistentDataPath  +"/" + SteamID + "/"+ MapName+".pa";
                }
                break;
            default:
                filePath = Application.persistentDataPath +"/"+ MapName+".pa";
                break;
        }
       
        string FromJsonData = File.ReadAllText(filePath);

        // 데이터 복호화
        string decryptData = DataController.Instance.Decrypt(FromJsonData);

        if(decryptData != null) {
            LoadedEditingObjects = JsonUtility.FromJson<Serialization<Editors>>(decryptData).ToList();
            DebugX.Log("홈 맵 에디터 암호화된 데이터");
        }
        else {
            LoadedEditingObjects = JsonUtility.FromJson<Serialization<Editors>>(FromJsonData).ToList();
            DebugX.Log("홈 맵 에디터 그냥 데이터");
        }

        float Xmax = -500;
        float Xmin = 500;
        float Ymax = -500;
        float Ymin = 500;

        for(int i = 0; i < LoadedEditingObjects.Count; i++) {
            tileMapEditor.LoadMap_TileGenerator(LoadedEditingObjects[i].TileCode,LoadedEditingObjects[i].position,LoadedEditingObjects[i].rotation,LoadedEditingObjects[i].layer,LoadedEditingObjects[i].InteractionLayer, LoadedEditingObjects[i].tileMode);
            if(LoadedEditingObjects[i].position.x > Xmax){
                Xmax = LoadedEditingObjects[i].position.x;
            }
            if(LoadedEditingObjects[i].position.x < Xmin){
                Xmin = LoadedEditingObjects[i].position.x;
            }
            if(LoadedEditingObjects[i].position.y > Ymax){
                Ymax = LoadedEditingObjects[i].position.y;
            }
            if(LoadedEditingObjects[i].position.y < Ymin){
                Ymin = LoadedEditingObjects[i].position.y;
            }
            if(LoadedEditingObjects[i].TileCode==31)
            {
                LineObjects.Add(EditingControlObject[i].Object);
            }

        }
        GameObject.Find("CamCollider").GetComponent<SetPolyCol>().OnMapEditor(Xmax+0.5f, Xmin-0.5f, Ymax+0.5f, Ymin-0.5f);

        Invoke("LoadMap_Lines",0.01f);
    }

    public void ResetHomeMapJson() {
        LoadedEditingObjects = InitialGameObjectList;
        
        float Xmax = -500;
        float Xmin = 500;
        float Ymax = -500;
        float Ymin = 500;

        for(int i = 0; i < LoadedEditingObjects.Count; i++) {
            tileMapEditor.LoadMap_TileGenerator(LoadedEditingObjects[i].TileCode,LoadedEditingObjects[i].position,LoadedEditingObjects[i].rotation,LoadedEditingObjects[i].layer,LoadedEditingObjects[i].InteractionLayer, LoadedEditingObjects[i].tileMode);
            if(LoadedEditingObjects[i].position.x > Xmax){
                Xmax = LoadedEditingObjects[i].position.x;
            }
            if(LoadedEditingObjects[i].position.x < Xmin){
                Xmin = LoadedEditingObjects[i].position.x;
            }
            if(LoadedEditingObjects[i].position.y > Ymax){
                Ymax = LoadedEditingObjects[i].position.y;
            }
            if(LoadedEditingObjects[i].position.y < Ymin){
                Ymin = LoadedEditingObjects[i].position.y;
            }
            if(LoadedEditingObjects[i].TileCode==31)
            {
                LineObjects.Add(EditingControlObject[i].Object);
            }

        }
        GameObject.Find("CamCollider").GetComponent<SetPolyCol>().OnMapEditor(Xmax+0.5f, Xmin-0.5f, Ymax+0.5f, Ymin-0.5f);

        Invoke("LoadMap_Lines",0.01f);
    }

    IEnumerator LoadHomeMapTileDelay(){
        float Xmax = -500;
        float Xmin = 500;
        float Ymax = -500;
        float Ymin = 500;

        for(int i = 0; i < LoadedEditingObjects.Count; i++) {
            if(i % 20 == 0){
                yield return new WaitForEndOfFrame();
            }
            
            DebugX.Log("waitfor Seconds 하고 있음");
            tileMapEditor.LoadMap_TileGenerator(LoadedEditingObjects[i].TileCode,LoadedEditingObjects[i].position,LoadedEditingObjects[i].rotation,LoadedEditingObjects[i].layer,LoadedEditingObjects[i].InteractionLayer, LoadedEditingObjects[i].tileMode);

            
            if(LoadedEditingObjects[i].position.x > Xmax){
                Xmax = LoadedEditingObjects[i].position.x;
            }
            if(LoadedEditingObjects[i].position.x < Xmin){
                Xmin = LoadedEditingObjects[i].position.x;
            }
            if(LoadedEditingObjects[i].position.y > Ymax){
                Ymax = LoadedEditingObjects[i].position.y;
            }
            if(LoadedEditingObjects[i].position.y < Ymin){
                Ymin = LoadedEditingObjects[i].position.y;
            }
            if(LoadedEditingObjects[i].TileCode==31)
            {
                LineObjects.Add(EditingControlObject[i].Object);
            }

        }
        GameObject.Find("CamCollider").GetComponent<SetPolyCol>().OnMapEditor(Xmax+0.5f, Xmin-0.5f, Ymax+0.5f, Ymin-0.5f);

        Invoke("LoadMap_Lines",0.01f);

        isOthers = true;
        SetPlayMode();
        Reload();
    }
    void SaveTextToFile(string text)
    {
        string filePath = "Assets/Resources/savedTextBlock.txt";
        // 파일 경로가 존재하지 않으면 새로 만듭니다.
        if (!File.Exists(filePath))
        {
            // 경로에 파일이 없다면 새로 생성
            File.WriteAllText(filePath, text);
            Debug.Log("새로운 파일이 생성되었습니다.");
        }
        else
        {
            // 파일이 존재하면 기존 파일에 텍스트를 덧붙여 저장
            File.AppendAllText(filePath, text);
            Debug.Log("텍스트가 파일에 추가되었습니다.");
        }
    }
    public void LoadMapJson() {
        /*
            2024-12-19 OYJ
            각 씬에서 블록들이 몇 개있는지 확인하려고 작성한 코드
            이유 : 현재 게임 오브젝트들을 배치하는 방식은 Instantiate를 통해 생성한다. 다른 기믹 생성은 제외하고 블록만 계산했을 때 최대 377개, 최소 73개 평균 179개의 생성을 한다.
            하지만 Instantiate는 연산할 때 cost가 비싸다. 따라서 프레임 드랍이 발생하는 등 성능이 저하가 된다.
            결론적으로 Instantiate하는 오브젝트들을 줄이고, 씬 자체에 미리 생성하여 두는 것으로 변경한다.
            하지만 원래 씬보다 377개 가량의 오브젝트들을 미리 생성하는 것은 사용하지 않는 오브젝트들로 인해 쓸모없는 메모리를 차지할 때가 있음.
            어쩔 수 없지만 각 블럭들의 평균 개수만큼 미리 생성한다.
            전체 스테이지의 평균값과 각 챕터의 스테이지 별 평균값도 다 다르지만 전체 스테이지로 기준을 잡는다.

            CameraToWorld.cs에서 LoadMap_TileGenerator()에 코드 처리 해놨음.
        */

        string filePath = "MapData/"+MapName;
        
        TextAsset textFile = Resources.Load(filePath) as TextAsset;
        string FromJsonData = textFile.ToString();

        LoadedEditingObjects = JsonUtility.FromJson<Serialization<Editors>>(FromJsonData).ToList();
        float Xmax = -500;
        float Xmin = 500;
        float Ymax = -500;
        float Ymin = 500;

        for(int i = 0; i < LoadedEditingObjects.Count; i++) {
            // eachtilemode[LoadedEditingObjects[i].TileCode]++;
            tileMapEditor.LoadMap_TileGenerator(LoadedEditingObjects[i].TileCode,LoadedEditingObjects[i].position,LoadedEditingObjects[i].rotation,LoadedEditingObjects[i].layer,LoadedEditingObjects[i].InteractionLayer, LoadedEditingObjects[i].tileMode);
            if(LoadedEditingObjects[i].position.x > Xmax){
                Xmax = LoadedEditingObjects[i].position.x;
            }
            if(LoadedEditingObjects[i].position.x < Xmin){
                Xmin = LoadedEditingObjects[i].position.x;
            }
            if(LoadedEditingObjects[i].position.y > Ymax){
                Ymax = LoadedEditingObjects[i].position.y;
            }
            if(LoadedEditingObjects[i].position.y < Ymin){
                Ymin = LoadedEditingObjects[i].position.y;
            }
            if(LoadedEditingObjects[i].TileCode==31)
            {
                LineObjects.Add(EditingControlObject[i].Object);
            }
        }
        GameObject.Find("CamCollider").GetComponent<SetPolyCol>().OnMapEditor(Xmax+0.5f, Xmin-0.5f, Ymax+0.5f, Ymin-0.5f);
        Invoke("LoadMap_Lines",0.01f);
        if(DataController.Instance.gameData.playMode == PlayMode.CUTSCENE){
            isTileMapPuzzle = true; // Datacontroller에서 값 가져와서 true로 바꿔준다.
        }else{
            isTileMapPuzzle = false;
        }   
    }

    IEnumerator LoadMapTileDelay(){
        float Xmax = -500;
        float Xmin = 500;
        float Ymax = -500;
        float Ymin = 500;

        for(int i = 0; i < LoadedEditingObjects.Count; i++) {
            if(i % 20 == 0){
                yield return new WaitForEndOfFrame();
            }
            
            tileMapEditor.LoadMap_TileGenerator(LoadedEditingObjects[i].TileCode,LoadedEditingObjects[i].position,LoadedEditingObjects[i].rotation,LoadedEditingObjects[i].layer,LoadedEditingObjects[i].InteractionLayer, LoadedEditingObjects[i].tileMode);
            if(LoadedEditingObjects[i].position.x > Xmax){
                Xmax = LoadedEditingObjects[i].position.x;
            }
            if(LoadedEditingObjects[i].position.x < Xmin){
                Xmin = LoadedEditingObjects[i].position.x;
            }
            if(LoadedEditingObjects[i].position.y > Ymax){
                Ymax = LoadedEditingObjects[i].position.y;
            }
            if(LoadedEditingObjects[i].position.y < Ymin){
                Ymin = LoadedEditingObjects[i].position.y;
            }
            if(LoadedEditingObjects[i].TileCode==31)
            {
                LineObjects.Add(EditingControlObject[i].Object);
            }
        }
        GameObject.Find("CamCollider").GetComponent<SetPolyCol>().OnMapEditor(Xmax+0.5f, Xmin-0.5f, Ymax+0.5f, Ymin-0.5f);
        Invoke("LoadMap_Lines",0.01f);
        if(DataController.Instance.gameData.playMode == PlayMode.CUTSCENE){
            isTileMapPuzzle = true; // Datacontroller에서 값 가져와서 true로 바꿔준다.
        }else{
            isTileMapPuzzle = false;
        }
        if(DataController.Instance.gameData.playMode == PlayMode.STORY) { // 스토리모드 UI = 커스텀맵 플레이모드

            isOthers = true;
            SetPlayMode();
            Reload();
            
        }
    }

    public void LoadMap() {
        string FromJsonData = currentCustomMap.GetComponent<CurrentCustomMap>().currentMapInfo;
        DebugX.Log(FromJsonData);
        
        LoadedEditingObjects = JsonUtility.FromJson<Serialization<Editors>>(FromJsonData).ToList();

        for(int i = 0; i < LoadedEditingObjects.Count; i++) {
            tileMapEditor.LoadMap_TileGenerator(LoadedEditingObjects[i].TileCode,LoadedEditingObjects[i].position,LoadedEditingObjects[i].rotation,LoadedEditingObjects[i].layer,LoadedEditingObjects[i].InteractionLayer, LoadedEditingObjects[i].tileMode);
            if(LoadedEditingObjects[i].TileCode==31)
            {
                LineObjects.Add(EditingControlObject[i].Object);
            }
        }
        Invoke("LoadMap_Lines",0.01f);
    }

    public bool CheckDuplicateMapName(string newMapName) {

        Where where = new Where();
        where.Equal("MapName", newMapName);
        // 조건 없이 모든 데이터 조회하기
        var bro = Backend.GameData.Get("CustomMap_List", where, 10);
        if (bro.IsSuccess() == false)
        {
            // 요청 실패 처리
            DebugX.Log(bro);
            return false;
        }
        if (bro.GetReturnValuetoJSON()["rows"].Count <= 0)
        {
            // 요청이 성공해도 where 조건에 부합하는 데이터가 없을 수 있기 때문에
            // 데이터가 존재하는지 확인
            // 위와 같은 new Where() 조건의 경우 테이블에 row가 하나도 없으면 Count가 0 이하 일 수 있다.
            DebugX.Log(bro);
            return true;
        }
        DebugX.Log("중복된 맵 이름입니다.");
        return false;
    }

    public bool CheckDuplicateLocalMapName(string newMapName) {

        Where where = new Where();
        where.Equal("MapName", newMapName);
        // 조건 없이 모든 데이터 조회하기
        var bro = Backend.GameData.Get("LocalSavedMap_List", where, 10);
        if (bro.IsSuccess() == false)
        {
            // 요청 실패 처리
            DebugX.Log(bro);
            return false;
        }
        if (bro.GetReturnValuetoJSON()["rows"].Count <= 0)
        {
            // 요청이 성공해도 where 조건에 부합하는 데이터가 없을 수 있기 때문에
            // 데이터가 존재하는지 확인
            // 위와 같은 new Where() 조건의 경우 테이블에 row가 하나도 없으면 Count가 0 이하 일 수 있다.
            DebugX.Log(bro);
            return true;
        }
        DebugX.Log("중복된 맵 이름입니다.");
        return false;
    }

    //2022-03-01 Stage_MapEditor 씬에서 광고 UI 띄워야하기 때문에 기존의 SetPreviewMode를 함수 2개로 분리
    public void SetBasicUI(bool state) {
        tileMapCamera.SetActive(state);
        basicUIs.SetActive(state);
    }

    public void SetMiniMapUI(bool state) {
        miniMapCamera.SetActive(state);
        miniMapUI.SetActive(state);
    }

    public void SetIsOthers(bool state) {
        isOthers = state;
    }
    
    public void SetPlayMode() {
        DebugX.Log("플레이모드인가?: " + isOthers);
        PlayButton.GetComponent<Toggle>().isOn = !isOthers;
        PlayButton.GetComponent<MapEditorPlayButton>().toggleCheck();
        PlayButton.SetActive(!isOthers);
        EditorUI.SetActive(!isOthers);
        for(int i =0 ;i<EditorObjects.Count;i++)
        {
            EditorObjects[i].SetActive(!isOthers);
        }
        for(int i=0; i<RuningObjects.Count;i++)
        {
            RuningObjects[i].SetActive(isOthers);
        }
    }

    public void UpdateLikeCount() {
        currentCustomMap.GetComponent<CurrentCustomMap>().currentMapLike++;
        Param param = new Param();
        param.Add("Like", currentCustomMap.GetComponent<CurrentCustomMap>().currentMapLike);
        
        BackendReturnObject bro = null;
                        
        bro = Backend.GameData.UpdateV2("CustomMap_List", currentCustomMap.GetComponent<CurrentCustomMap>().currentMapinDate, currentCustomMap.GetComponent<CurrentCustomMap>().currentMapOwnerinDate, param);
        if (bro.IsSuccess()) {
            DebugX.Log("좋아요 수 업데이트에 성공했습니다. : " + bro);
        }
        else {
            DebugX.LogError("좋아요 수 업데이트에 실패했습니다. : " + bro);
        }
    }

    public void SetPossibleDeleteCnt(){
        if(isTileMapPuzzle){
            additionalDeleteCnt++;
        }
        
    }
    public void OpenOrSaveLocalMap() {
        if(string.IsNullOrEmpty(MapName)) {
            savePanel.SetActive(true);
            saveBtn.SetActive(true);
            uploadBtn.SetActive(false);
            // 24-10-30 Grid_Newssets Panel 켜져있을 때 Shift, Tab 안먹히도록 하기 위해
            DataController.Instance.gameData.isSettingPanelOn = true;

        }
        else {
            DebugX.Log("바로 저장");
            UpdateLocalMap();
        }
    }

    public void InitEditingObjects() {
        foreach(EditingObject now in EditingControlObject){
            Destroy(now.Object);
        }
        EditingObjects.Clear();
        EditingControlObject.Clear();
    }

    // 24-07-23 홈에디터 초기화
    public void ClearAndResetHomeEditor() {
        int i = 0;
        foreach(EditingObject now in EditingControlObject){
            Destroy(now.Object);
        }
        EditingObjects.Clear();
        EditingControlObject.Clear();

        ResetHomeMapJson();
    }

    public void LocalSaveMap() {
        string ToJsonData = JsonUtility.ToJson(new Serialization<Editors>(EditingObjects));

        //BackendReturnObject bro = Backend.BMember.GetUserInfo ();
        string nickname = BackendGameData.Instance.GetUserNickName();

        Param newMap = new Param();
        newMap.Add("MapInfo", ToJsonData);
        newMap.Add("MapName", MapName);
        newMap.Add("MadeBy", nickname);
        newMap.Add("PlayCount", 0);
        newMap.Add("Like", 0);

        
        Backend.GameData.Insert ("LocalSavedMap_List", newMap);
    }

    private void UpdateLocalMap() {
        string ToJsonData = JsonUtility.ToJson(new Serialization<Editors>(EditingObjects));

        string nickname = BackendGameData.Instance.GetUserNickName();

        Where where = new Where();
        where.Equal("MapName", MapName);
        where.Equal("MadeBy", nickname);

        Param newMap = new Param();
        newMap.Add("MapInfo", ToJsonData);

        BackendReturnObject bro = null;

        bro = Backend.GameData.Update("LocalSavedMap_List", where, newMap);

        if (bro.IsSuccess()) {
            DebugX.Log("로컬 세이브 맵 업데이트에 성공했습니다. : " + bro);
            saveResultText.gameObject.SetActive(true);
            saveResultText.color = new Color32(0, 255, 0, 0);
            saveResultText.text = "Save Success!";
            StartCoroutine(FadeTextToFullAlpha(saveResultText));
        }
        else {
            DebugX.LogError("로컬 세이브 맵 업데이트에 실패했습니다. : " + bro);
            saveResultText.gameObject.SetActive(true);
            saveResultText.color = new Color32(255, 0, 0, 0);
            saveResultText.text = "Save Failed!\nCheck NetWork!";
            StartCoroutine(FadeTextToFullAlpha(saveResultText));
        }
    }

    public IEnumerator FadeTextToFullAlpha(Text warningText) // 알파값 0에서 1로 전환
    {
        warningText.color = new Color(warningText.color.r, warningText.color.g, warningText.color.b, 0);
        while (warningText.color.a < 1.0f)
        {
            warningText.color = new Color(warningText.color.r, warningText.color.g, warningText.color.b, warningText.color.a + Time.deltaTime);
            yield return null;
        }
        StartCoroutine(FadeTextToZeroAlpha(warningText));
    }

    public IEnumerator FadeTextToZeroAlpha(Text warningText)  // 알파값 1에서 0으로 전환
    {
        warningText.color = new Color(warningText.color.r, warningText.color.g, warningText.color.b, 1);
        while (warningText.color.a > 0.0f)
        {
            warningText.color = new Color(warningText.color.r, warningText.color.g, warningText.color.b, warningText.color.a - Time.deltaTime);
            yield return null;
        }
        warningText.gameObject.SetActive(false);
    }
}



/*
1. 커서에서 객체 생성
2. 해당 객체 Edit_Play controoler에 저장, 일련번호 부여
    2.1 객체 삭제시 일련번호를 사용해 객체 삭제
    2.2 객체 이동시 상관 .X
    2.3
3. Edit->to Play
    1.리스트 객체들에게 Position내부 Play함수 호출
        Instantiate 객체, 해당 좌표
    2. 카메라 객체 렌더링 레이어에서 MapEditor객체 제외
4. Play to edit
    1. 리스트 객체들에게 Position내부 Edit함수 호출
        Destory Instantiated 객체
    2. 카메라 객체 렌더링 레이어에 에서 Mapeditor 레이어 포함


*/