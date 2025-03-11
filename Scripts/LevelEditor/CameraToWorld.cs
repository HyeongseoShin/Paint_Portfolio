/*
Edit_PlayController에서 내린 명령으로 Edit 모드에서 실제 객체를 맵에 설치하는 스크립트

주요 제작 기능
Edit_PlayController.cs에서 맵 내 객체 정보 List의 각각의 요소를 실제로 생성하는 기능
- LoadMap_TileGenerator() : 전달 받은 정보를 통해 실제 객체 Instantiate 혹은 Pool 에서 꺼내서 배치
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraToWorld : MonoBehaviour 
{
    public bool UI_Touched;
    public enum Mode{Move,Create,Delete,Camera};
    public Mode nowMode;
    public int nowModeNum;
    [SerializeField]
    private Vector3 TouchInput;
    [SerializeField]
    private GameObject Cursur;
    public List<GameObject> Tiles;
    public int TileNum;
    private Vector3 StartPosition;

    [SerializeField]
    private bool IsClick;
    [SerializeField]
    private GameObject Moving;
    private int Moving_Origin_layer;
    public bool IsOut;
    public int testInt;
    public Toggle[] setMode;
    public float OSh,OSw, h, w, MapSizeHeight, MapSizeWidth;
    public Edit_PlayController Edit;
    private float priviousX,priviousY=0.0f;

    [SerializeField]
    private bool IsMouseMove=false;     
    
    public bool UIInput;             
    public float InteractionLayer=0.0f;
    public int ConnectedInteraction=0;   
    private GameObject DrwaingLineObject;    
    public MapEditorPlayButton MapEditorPlayButton;      
    public List<GameObject> LayerPrefabs;                                                                                                                                                    
    private int LayerCount;

    //For Moving Platform and layered Platformed
    public int DrawingLayer;
    public List<GameObject> Layers;

    [SerializeField]
    private Vector3 objRotationME;

    [SerializeField]
    public int nowtilemode;

    [SerializeField]
    public bool nowDevelopermode;

    public bool canCreate; //tilemode 3인 Player가 에디터 퍼즐 모드에서 추가로 생성할 수 있는지 없는지.
    public bool canDelete; //Player가 에디터 퍼즐 모드에서 원래 있던 tilemode 1인 block을 더 삭제할 수 있는지 없는지. 

    [Header("Input by button")]
    [SerializeField]
    private bool IsButtonMove = false; //클릭 버튼을 누르고 있을 때 true가 된다. mouse로 했을 때 ismousemove와 같은 역할을 한다.
    private bool IsButtonDelete = false;

    private float getbuttonDown = 0f;
    private float getbuttonUp = 0f;
    private float getbuttonLeft = 0f;
    private float getbuttonRight = 0f;

    private bool isButtonKeepDown = false;
    private bool isButtonKeepUp = false;
    private bool isButtonKeepLeft = false;
    private bool isButtonKeepRight = false;

    [SerializeField]
    private bool isMouseInput = true;

    [SerializeField]
    private float RightLeftMove;
    [SerializeField]
    private float UpDownMove;

    private bool isButtonDown = false;
    private bool isButtonUp = false;
    private bool isButtonLeft = false;
    private bool isButtonRight = false;

    private bool preventDoubleDown = false;
    private bool preventDoubleUp = false;
    private bool preventDoubleLeft = false;
    private bool preventDoubleRight = false;

    // 24-08-26
    [SerializeField]
    private NewHintController newHintController;

    [SerializeField]
    private Transform[] BlockPrev;

    private int prevBlockindex = 0;

    // 25-01-16 맵에디터 패널 여는 버튼
    [SerializeField]
    private Button mapEditorOpenBtn;

    // 25-01-16 Quick Select Tile
    [SerializeField]
    private GameObject topBanner;
    void Awake()
    {
        if(DataController.Instance.gameData.playMode == PlayMode.MAPEDITOR){
            nowtilemode = 0;
            Edit.nowTileMode = 0;
        }else{
            nowtilemode = 3;
            Edit.nowTileMode = 3;
        }
        
        nowDevelopermode = false;
        objRotationME = new Vector3(0,0,0);
        h=Screen.height;
        w=Screen.width;
            OSh=(h*20)/w;
    }
    void Start()
    {
        canCreate = true;
        canDelete = true;
        testInt=0;
        IsClick=false;
          
        Camera.main.orthographicSize +=OSh;
        
    }
    
    // 25-01-16 Tab키와 MapEditor 패널 여는 것 연결
    public void OnClickTabButton(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            DebugX.Log("Tab button performed");
            mapEditorOpenBtn.onClick.Invoke();
        }
    }

    public void OnClickQuickTile(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            int buttonIndex = int.Parse(context.control.name);
            topBanner.transform.GetChild(buttonIndex-1).gameObject.GetComponent<Button>().onClick.Invoke();

        }
    }

    public void OnCursorMove(InputAction.CallbackContext context)
    {
        if(!DataController.Instance.gameData.isSettingPanelOn){
            if(context.performed){
                RightLeftMove = context.ReadValue<Vector2>().x;
                UpDownMove = context.ReadValue<Vector2>().y;

                if(RightLeftMove > 0.7072f){
                    isButtonLeft = false;
                    isButtonRight = true;
                    isButtonDown = false;
                    isButtonUp = false;
                }else if(RightLeftMove < -0.7072f){
                    isButtonLeft = true;
                    isButtonRight = false;
                    isButtonDown = false;
                    isButtonUp = false;
                }
                else{
                    isButtonLeft = false;
                    isButtonRight = false;
                    isButtonDown = false;
                    isButtonUp = false;
                }

                if(UpDownMove > 0.7072f){
                    isButtonLeft = false;
                    isButtonRight = false;
                    isButtonDown = false;
                    isButtonUp = true;
                }else if(UpDownMove < -0.7072f){
                    isButtonLeft = false;
                    isButtonRight = false;
                    isButtonDown = true;
                    isButtonUp = false;
                }else{
                    isButtonDown = false;
                    isButtonUp = false;
                }
                /*
                    2024-02-22 OYJ
                    아래 코드는 원래 MoveSettingbyKeyBoard() 함수에 들어있던 코드들이다.
                    하지만 원래는 GetbuttonDown으로 단 한번만 실행되고, 그 이후는 실행이 되면 안된다.
                    하지만 bool 값으로 바꿔뒀을 경우에는 연속해서 실행되기 때문에 본 함수인 OnCursorMove()에서 한 번 실행되도록 하고
                    연속적으로 되지 않도록 한다. 그 이후 연속해서 이동할 때는 MoveSettingbyKeyBoard()에서 실행되도록 코드를 변경한다.

                */
                

                if(isButtonRight && !preventDoubleRight){
                    preventDoubleRight = true;
                    TouchInput = TouchInput + new Vector3(1, 0, 0);
                    TouchInputtoInt();
                    Cursur.transform.position=TouchInput;
                }

                if(isButtonLeft && !preventDoubleLeft){
                    preventDoubleLeft = true;
                    TouchInput = TouchInput + new Vector3(-1, 0, 0);
                    TouchInputtoInt();
                    Cursur.transform.position=TouchInput;
                }

                if(isButtonUp && !preventDoubleUp){
                    preventDoubleUp = true;
                    TouchInput = TouchInput + new Vector3(0, 1, 0);
                    TouchInputtoInt();
                    // IsButtonMove = true;
                    // TouchInputtoInt();
                    Cursur.transform.position=TouchInput;
                }
                
                if(isButtonDown && !preventDoubleDown){
                    preventDoubleDown = true;
                    TouchInput = TouchInput + new Vector3(0, -1, 0);
                    TouchInputtoInt();
                    
                    Cursur.transform.position=TouchInput;
                }
            }else if(context.canceled){
                RightLeftMove = context.ReadValue<Vector2>().x;
                UpDownMove = context.ReadValue<Vector2>().y;
                if(RightLeftMove > 0){
                    isButtonLeft = false;
                    preventDoubleLeft = false;
                }else if(RightLeftMove < 0){
                    isButtonRight = false;
                    preventDoubleRight = false;
                }else if(RightLeftMove == 0){
                    isButtonLeft = false;
                    isButtonRight = false;
                    preventDoubleRight = false;
                    preventDoubleLeft = false;
                }

                if(UpDownMove > 0){
                    isButtonDown = false;
                    preventDoubleDown = false;
                }else if(UpDownMove < 0){
                    isButtonUp = false;
                    preventDoubleUp = false;
                }else if(UpDownMove == 0){
                    isButtonDown = false;
                    isButtonUp = false;
                    preventDoubleDown = false;
                    preventDoubleUp = false;
                }
            }
        }
        
    }

    public void OnTileGenerate(InputAction.CallbackContext context)
    {
        if(!DataController.Instance.gameData.isSettingPanelOn){
            if(context.performed)
            {
                IsButtonMove = true;
                IsClick = true;

                if(IsClick){//Clicked
                    var  temp = Cursur.GetComponent<Cursur_Detecting>().CheckingBlockExist();
                    if(temp)
                    {
                        DebugX.Log("선택한거 없는데용?");
                        nowMode = Mode.Move;
                    }     
                    else
                    {
                        nowMode = Mode.Create;
                    } 
                    MapMakingMode(TouchInput);
                    
                }
            }
            else if(context.canceled)
            {
                IsButtonMove = false;
                IsClick = false;
                if(Moving){
                    UnsetMoveBlock();
                }
                    
                if(nowMode==Mode.Create&&TileNum==31)
                {
                    StopDrawingLine();
                }
                nowMode = Mode.Create;
                
            }
        }
        
       
    }

    public void OnTileDelete(InputAction.CallbackContext context)
    {
        if(!DataController.Instance.gameData.isSettingPanelOn){
            
            if(context.performed)
            {
                IsButtonDelete = true;
            }
            else if(context.canceled)
            {
            IsButtonDelete = false;
            nowMode = Mode.Create;
            }
        }
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S)){
            //Edit.SaveMap();
        }
        // if(Input.touchCount==1&&!EventSystem.current.IsPointerOverGameObject())
        // {
            if(Input.touchCount>0) 
            {
                if(!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)&&Input.touchCount<2)
                {
                    Update_Basic();
                }
            }
            else if(!EventSystem.current.IsPointerOverGameObject())
            {
                Update_PC();
                
            }
            else
            {
                IsClick=false;
                if(Moving)
                    UnsetMoveBlock();
                if(nowMode==Mode.Create&&TileNum==31)
                {
                    StopDrawingLine();
                }
            }
    }

    private void Update_PC()
    {
        /*
            회전하는거 코드들임 지우면 안됩니다.
        */
        // if(!DataController.Instance.gameData.playerCanMove){
        //     if(Input.GetKeyDown(KeyCode.Q)){
        //         objRotationME.z += 90f; 
        //         if(objRotationME.z == -360f || objRotationME.z == 360f){
        //             objRotationME.z = 0;
        //         }
        //     }
        //     if(Input.GetKeyDown(KeyCode.E)){
        //         objRotationME.z -= 90f;
        //         if(objRotationME.z == -360f || objRotationME.z == 360f){
        //             objRotationME.z = 0;
        //         }
        //     }
        //     if(Input.GetKeyDown(KeyCode.Alpha1)){
        //         nowtilemode = 0;
        //         Edit.nowTileMode = 0;
        //     }
        //     if(Input.GetKeyDown(KeyCode.Alpha2)){
        //         nowtilemode = 1;
        //         Edit.nowTileMode = 1;
        //     }
        //     if(Input.GetKeyDown(KeyCode.Alpha3)){
        //         nowDevelopermode = !nowDevelopermode;
        //     }
        // }
        
        if (Input.anyKeyDown)
        {
            if (!Input.GetMouseButtonDown(0) || !Input.GetMouseButtonDown(1) || !Input.GetMouseButtonDown(2)){
                isMouseInput = false;
            }else{
                isMouseInput = true;
            }
            //키보드로 입력했을 때는 무조건 키보드만 되도록 하고, 마우스를 움직이면 다시 마우스로 이동 가능하도록 한다.
        }
        if(Mouse.current.delta.ReadValue().x > 0 || Mouse.current.delta.ReadValue().y >0){
            isMouseInput = true;
        }
        if(isMouseInput){
            TouchInput = Camera.main.ScreenToWorldPoint(Input.mousePosition); //이건 마우스를 안움직여도 그 때의 위치를 가져온다.
            TouchInputtoInt();
            Cursur.transform.position=TouchInput;
        }
        
        MoveSettingbyKeyBoard();
        
        if(IsClick)
                MouseMoveChecker();
                if(!IsOut)
                if(IsClick&& (Input.GetMouseButton(0) || IsButtonMove))//Drag
                {
                    if(nowMode != Mode.Create){
                        if(Moving&&Moving.tag=="Ground")
                            Moving.GetComponent<Tile_OutLiner>().LineDelete();
                    }
                    
                    MapMakingMode(TouchInput);
                    if(Moving&&Moving.tag=="Ground")
                    Moving.GetComponent<Tile_OutLiner>().LineGenerator();
                    return;
                } 
                else if(Input.GetMouseButtonDown(0)){
                    IsClick=true;//Click
                }
                else if (Input.GetMouseButtonUp(0)){//Unclick
                    IsClick=false;
                    if(Moving){
                        UnsetMoveBlock();
                    }
                        
                    if(nowMode==Mode.Create&&TileNum==31)
                    {
                        StopDrawingLine();
                    }
                    nowMode = Mode.Create;
                    return;
                }
                if( Input.GetMouseButton(1) || IsButtonDelete)
                {
                    nowMode = Mode.Delete;
                    TileDeletor();
                    return;
                }
                if( Input.GetMouseButtonUp(1))
                {
                    nowMode = Mode.Create;
                    return;
                }
                if(IsClick){//Clicked
                    var  temp = Cursur.GetComponent<Cursur_Detecting>().CheckingBlockExist();
                    if(temp)
                    {
                        DebugX.Log("선택한거 없는데용?");
                        nowMode = Mode.Move;
                    }     
                    else
                    {
                        nowMode = Mode.Create;
                    } 
                    MapMakingMode(TouchInput);
                    
                }
            
    }
    private void Update_Basic()
    {
        TouchInput = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        TouchInputtoInt();
        Cursur.transform.position=TouchInput;
        if(IsClick)
        MouseMoveChecker();
        if(!IsOut)

        if(Input.GetMouseButtonDown(0)){
            IsClick=true;
        }
        else if (Input.GetMouseButtonUp(0)){
            IsClick=false;
            if(Moving)
                UnsetMoveBlock();
            if(nowMode==Mode.Create&&TileNum==31)
            {
                StopDrawingLine();
            }
        }
        if(IsClick&&(IsMouseMove || IsButtonMove)){
            MapMakingMode(TouchInput);
            
        }
    }
    public void ChangePlayer(int playeNum){
        Edit.ChangePlayerColor(Tiles[playeNum]);
    }
    void MapMakingMode(Vector3 TouchInput){
        switch(nowMode){
            case Mode.Move: //Move Block
                if(Moving){//We Already Choose Block;
                    MoveBlock();
                }
                else{
                    SetMoveBlock();
                }
                break;
            case Mode.Delete:
                TileDeletor();
                break;
            case Mode.Create:
                TileGenerator();
                break;
            case Mode.Camera:  
                break;

        }

    }
    void MoveBlock(){
        //Check Position Before to move
        var  temp = Cursur.GetComponent<Cursur_Detecting>().CheckingBlockExist(); //처음 Move 할 때 그 자리에 block이 있는지 없는지 확인한다.
        if(Moving.GetComponent<Position>().tileMode != 1 || nowDevelopermode){ //tileMode가 1이면 움직이지 못하도록 한다. 하지만 지금 nowDevelopermode가 true가 되면 모두 이동가능하다.
            DebugX.Log("Moving tilemode 값이 " + Moving.GetComponent<Position>().tileMode.ToString() +" 이에요!!");
            if(!temp){ //Cursur_Detecting에서 return 되는 값은 GameObject 혹은 null 값이다. !temp 는 null이 들어왔을 때 진행하라는 뜻이다.
                if(Moving.GetComponent<Position>().TileID < 32 || (Moving.GetComponent<Position>().TileID > 36 && Moving.GetComponent<Position>().TileID < 69) || (Moving.GetComponent<Position>().TileID > 72 && Moving.GetComponent<Position>().TileID != 77 && Moving.GetComponent<Position>().TileID != 95 && Moving.GetComponent<Position>().TileID != 96 && Moving.GetComponent<Position>().TileID != 121)){ //1x1 크기 객체를 움직일때.
                // 2024-07-23 홈화면 기본 버튼 (play, edit, levelEdit, Setting, Ach 2x2 사이즈 변경에 따른 코드 변경) if(Moving.GetComponent<Position>().TileID < 32 || Moving.GetComponent<Position>().TileID > 36){ 
                    Moving.transform.position=TouchInput;    
                }else{ //2x2 크기 객체를 움직일 때.
                    Moving.transform.position=TouchInput + new Vector3(0.5f, 0.5f, 0);
                }

                // 24-06-25 타일 옮길 때 마다 tileAnimController.originPos 업데이트
                TileAnimController tileAnimController = Moving.GetComponentInChildren<TileAnimController>();

                if(tileAnimController)
                {
                    tileAnimController.originPos = Moving.transform.position;
                }

                
            }
            else{ //null이 아닌 GameObject가 들어왔다는 뜻은 해당 자리에 객체가 있다는 뜻이다. 하지만 1x1 제외 크기가 더 큰 값이 있다면 자기 자신을 체크한다.
                // 2024-07-23 홈화면 기본 버튼 (play, edit, levelEdit, Setting, Ach 2x2 사이즈 변경에 따른 코드 변경)  if(Moving.GetComponent<Position>().TileID < 32 || Moving.GetComponent<Position>().TileID > 36){ 
                if(Moving.GetComponent<Position>().TileID < 32 || (Moving.GetComponent<Position>().TileID > 36 && Moving.GetComponent<Position>().TileID < 69) || (Moving.GetComponent<Position>().TileID > 72 && Moving.GetComponent<Position>().TileID != 77 && Moving.GetComponent<Position>().TileID != 95 && Moving.GetComponent<Position>().TileID != 96 && Moving.GetComponent<Position>().TileID != 121)){ 
                    //혹시나 해서 1x1도 같이 조건을 넣어주긴 했지만 실행되는 코드는 없다.
                    
                }else{
                    
                    
                    if(System.Object.ReferenceEquals(temp.transform.parent.gameObject, Moving.gameObject)){

                        //현재 자기 자신을 다시 선택하고 있는 상태이기 때문에, 자기 크기와 똑같은 범위를 다시 체크해봐야한다.
                        //지금은 상자 밖에 없기 때문에 2x2만 체크하면된다.
                        //따라서 checkingBlockExistForFour()를 해주되, var 값 obj를 이용하지는 않는다. 그저 해당 함수 안에서 boxCastLen을 설정하기 위함이다.
                        var obj = Cursur.GetComponent<Cursur_Detecting>().CheckingBlockExistForFour(); 

                        //GetBoxCastLen()이 1이라는 뜻은 자기 자신만 체크를 한다는 뜻이다. 만약 2 이상이라면 자기를 제외하고도 물체가 더 잡힌다는 뜻이다.
                        //만약 GetBoxCastLen()이 0이라면, 자기 자신도 체크를 못한다는 뜻으로 버그가 발생한다는 것이다. 따라서 1일 때만 이동하도록 한다.
                        if(Cursur.GetComponent<Cursur_Detecting>().GetBoxCastLen() == 1){ 
                            Moving.transform.position=TouchInput + new Vector3(0.5f, 0.5f, 0);
                            //Cursor 위치 기준으로 (0,0,0)이 아니라 0.5씩 이동해야 칸에 딱 맞게 설정이 된다.
                        }
                        
                    }
                
                }

            }
        }
        
    }
    void UnsetMoveBlock(){
        
        if(Moving.tag=="Ground"){
            
            testInt--;
            DebugX.Log("Is Test Unset:"+testInt);
            Moving.GetComponent<Tile_OutLiner>().LineGenerator();
        }
        else{
            Moving.layer=Moving_Origin_layer;
        
        }
       
        Edit.MoveEditorObject(Moving);
        Moving=null;
    }
    void SetMoveBlock(){
        
        var  temp = Cursur.GetComponent<Cursur_Detecting>().CheckingBlockExist(); 
        if(temp)
        switch(temp.name){
            case "Base":
            break;
            case "MapEditorBox":
                DebugX.Log("IsMEB");
                Moving =temp.transform.parent.gameObject;
                Moving_Origin_layer = Moving.layer;
                Moving.layer = 2;
            break;
            default:
                if(temp.tag=="Ground"){
                    testInt++;
                    DebugX.Log("Is Test Set"+testInt);
                    temp.gameObject.GetComponent<Tile_OutLiner>().LineDelete();
                }
                else
                {
                    Moving=temp;
                    Moving_Origin_layer = Moving.layer;
                    Moving.layer = 2; 
                }
                Moving = temp;    
            break;
        }
    }

    // 실제 객체 생성
    public void LoadMap_TileGenerator(int TileID, Vector3 GenenratePosition, Vector3 GennerateRotation, int layer,float Interaction, int tilemodeGenerate)
    {   
        GameObject newObject = null;
        prevBlockindex = TileID % 10;
        
        // 맵 발판 타일은 Pool에서 꺼내서 사용
        if( TileID < 15 && TileID > 9 && BlockPrev[prevBlockindex].childCount > 0)
        {
            newObject = BlockPrev[prevBlockindex].GetChild(0).gameObject;
            BlockPrev[prevBlockindex].GetChild(0).position = GenenratePosition;
            BlockPrev[prevBlockindex].GetChild(0).rotation = Quaternion.Euler(GennerateRotation);
            BlockPrev[prevBlockindex].GetChild(0).gameObject.SetActive(true);
        }

        // 나머지 객체는 Instantiate
        else
        {
            newObject = Instantiate(Tiles[TileID],GenenratePosition, Quaternion.Euler(GennerateRotation)); //rotation도 저장할
        }

        newObject.GetComponent<Position>().tileMode = tilemodeGenerate;

        //SpriteRenderer Sorting Layer Order 조정
        SpriteRenderer[] sr = newObject.GetComponentsInChildren<SpriteRenderer>();

        foreach(SpriteRenderer sprite in sr)
        {
            sprite.sortingOrder += newObject.transform.GetComponent<Position>().ID;
        }
        
        if(Layers[layer].transform.childCount != 0&&Layers[layer].transform.GetChild(0).name=="Platform"&&TileID!=7&&TileID!=8)
        {
            newObject.transform.SetParent(Layers[layer].transform.GetChild(0).transform);
        }
        else
        {
            newObject.transform.SetParent(Layers[layer].transform);
        }
        if(newObject)
        {
            if(newObject.GetComponent<LineAble>())
            {
                newObject.GetComponent<LineAble>().InteractionLayer = Interaction;
            }
            newObject.GetComponent<Position>().ID=Edit.AddEditorObject(newObject,layer,Interaction);
            newObject.GetComponent<Position>().Layer = layer;
            
            if(TileID==31)
            {
                newObject.GetComponent<LineRendererConnector>().InteractionLayer = Interaction;
                if(Interaction>InteractionLayer)
                {
                    InteractionLayer = Interaction+0.01f;
                }
            }
            newObject.GetComponent<Position>().tileMode = tilemodeGenerate;
        }
        if(newObject.tag=="Ground")
        {
            LayerSetter(layer);
            newObject.gameObject.GetComponent<Tile_OutLiner>().LineGenerator();
            AllLayerTurnOn();
        }

        // 24-08-26 힌트일 때
        if(newHintController && TileID == 94)
        {
            newHintController.AddHintObj(newObject);
        }
        
    }
    public void LayerSetter(GameObject active){
        AllLayerTurnOff();
        DrawingLayer = layerChecker(active);
        Layers[DrawingLayer].SetActive(true);
    }
    public void LayerSetter(int active)
    {
        AllLayerTurnOff();
        DrawingLayer = active;
        Layers[active].SetActive(true);
    }
    public void AllLayerTurnOff(){
        for(int i = 0 ; i< Layers.Count ; i++)
            Layers[i].SetActive(false);
    }
    public void AllLayerTurnOn(){
        DrawingLayer= 0;
        for(int i  = 0 ; i<Layers.Count; i++){
            Layers[i].SetActive(true);
        }
    }
    
    private int layerChecker(GameObject layer)
    {
        for(int i = 0; i<Layers.Count;i++)
        {
            if(Layers[i]==layer)
            {
                return i;
            }
        }
        return 0;
    }
    void TileGenerator()
    {
        if(canCreate){ //Edit_PlayController와 연결하는 변수
            if(TileNum!=31)//if It isn't Line
            {
                if(TileDeletor()){
                    switch(TileNum){
                        default:
                        
                            GameObject newObject;
                            // 2024-07-23 홈화면 기본 버튼 (play, edit, levelEdit, Setting, Ach 2x2 사이즈 변경에 따른 코드 변경) if(TileNum < 32 || TileNum > 36){
                            if(TileNum < 32 || (TileNum > 36 && TileNum < 69) || (TileNum > 72 && TileNum != 77 && TileNum != 95 && TileNum != 96 && TileNum != 121)){ //1x1 크기 객체를 생성할 때.
                                if(TileNum == 101 || TileNum == 105 || TileNum == 109 || TileNum == 113 || TileNum == 117){ // Deco tile 일 때.
                                    int random = Random.Range(0,4);
                                    newObject=Instantiate(Tiles[TileNum + random],TouchInput,Quaternion.Euler(objRotationME));
                                }else{
                                    newObject=Instantiate(Tiles[TileNum],TouchInput,Quaternion.Euler(objRotationME));    
                                }

                                //Sorting Order Layer 조정
                                //SpriteRenderer Sorting Layer Order 조정
                                SpriteRenderer[] sr = newObject.GetComponentsInChildren<SpriteRenderer>();

                                foreach(SpriteRenderer sprite in sr) {
                                    sprite.sortingOrder += newObject.transform.GetComponent<Position>().ID;
                                }
                                DebugX.Log("32보다 작고 36보다 크다면: " + newObject.GetComponent<Position>().ID);
                                
                            }else{
                                /*
                                    2 x 2 2x2 사이즈 친구들
                                    상자
                                    빨 32
                                    초 33 
                                    파 34
                                    노 35
                                    흰 36

                                    홈 에디터의
                                    홈에디터 69
                                    세팅 70
                                    플레이 71
                                    레벨에디터 72
                                    업적 77

                                    스킨 95
                                    크레딧 96
                                */
                                nowMode = Mode.Move;
                                
                                newObject=Instantiate(Tiles[TileNum],TouchInput + new Vector3(0.5f, 0.5f, 0),Tiles[TileNum].transform.rotation);

                                //Sorting Order Layer 조정
                                //SpriteRenderer Sorting Layer Order 조정
                                SpriteRenderer[] sr = newObject.GetComponentsInChildren<SpriteRenderer>();

                                foreach(SpriteRenderer sprite in sr) {
                                    sprite.sortingOrder += newObject.transform.GetComponent<Position>().ID;
                                }

                                DebugX.Log("32보다 크고 36보다 작다면: " + newObject.GetComponent<Position>().ID);
                                
                            }
                            if(Layers[DrawingLayer].tag=="MovingPlatform")
                            {
                                    newObject.transform.SetParent(Layers[DrawingLayer].transform.GetChild(0).GetChild(1).transform);
                            }
                            else
                            {
                                newObject.transform.SetParent(Layers[DrawingLayer].transform);
                            }
                            if(newObject.tag=="TeleportPositionLayer")
                            {
                                newObject.GetComponent<Position>().tileMode = nowtilemode;
                                newObject.GetComponent<Position>().ID=Edit.AddEditorObject(newObject,DrawingLayer,InteractionLayer); 
                                newObject.GetComponent<Position>().Layer = DrawingLayer;
                                DebugX.Log("Portal Generated");
                            }
                            else if(newObject){
                                newObject.GetComponent<Position>().tileMode = nowtilemode;
                                newObject.GetComponent<Position>().ID=Edit.AddEditorObject(newObject,DrawingLayer);
                                newObject.GetComponent<Position>().Layer = DrawingLayer;

                                SpriteRenderer[] sr = newObject.GetComponentsInChildren<SpriteRenderer>();

                                foreach(SpriteRenderer sprite in sr) {
                                    sprite.sortingOrder += newObject.transform.GetComponent<Position>().ID;
                                }
                            }
                            
                            
                            if(newObject.tag=="Ground")
                            {
                                DebugX.Log("Is Ground");
                                newObject.gameObject.GetComponent<Tile_OutLiner>().LineGenerator();
                            }
                            newObject.GetComponent<Position>().tileMode = nowtilemode;
                        break;

                    }
                }
            }
            else
            {
                var  temp = Cursur.GetComponent<Cursur_Detecting>().CheckingBlockExist();
            
                if(temp)
                DebugX.Log("LineRenderer is"+ temp.name);
                if(temp!=null&&temp.transform.parent.GetComponent<LineAble>())
                {
                    if(ConnectedInteraction==0)
                    {
                        
                        InteractionLayer+=0.01f;
                        var newObject=Instantiate(Tiles[TileNum],TouchInput,Tiles[TileNum].transform.rotation);
                        newObject.transform.SetParent(Layers[DrawingLayer].transform);
                        newObject.GetComponent<Position>().ID=Edit.AddEditorObject(newObject,0,InteractionLayer);
                        DebugX.Log("엘스라면 : " + newObject.GetComponent<Position>().ID);
                        newObject.GetComponent<Position>().Layer = 0;
                        
                        //Sorting Order Layer 조정
                        //SpriteRenderer Sorting Layer Order 조정
                        SpriteRenderer[] sr = newObject.GetComponentsInChildren<SpriteRenderer>();

                        foreach(SpriteRenderer sprite in sr) {
                            sprite.sortingOrder += newObject.transform.GetComponent<Position>().ID;
                        }
                        
                        DrwaingLineObject = newObject;

                        DrwaingLineObject.GetComponent<LineRendererConnector>().InteractionLayer= InteractionLayer;
                        DrwaingLineObject.GetComponent<LineRendererConnector>().positionCnt = ConnectedInteraction;
                    }else
                    {
                        DrwaingLineObject.GetComponent<LineRendererConnector>().Positions.Remove(Cursur.transform);
                        DrwaingLineObject.GetComponent<LineRendererConnector>().positionCnt = ConnectedInteraction;
                    }
                    for(int i=0;i< DrwaingLineObject.GetComponent<LineRendererConnector>().Positions.Count;i++)
                    {
                        if(DrwaingLineObject.GetComponent<LineRendererConnector>().Positions[i]==temp.transform)
                        {
                            DrwaingLineObject.GetComponent<LineRendererConnector>().Positions.Add(Cursur.transform);
                            goto DuplicatedInteractionTarget;
                        }
                    }
                    
                    if(ConnectedInteraction > 0 ){
                        LineAble.ObjMode DrawingNowObjMode = DrwaingLineObject.GetComponent<LineRendererConnector>().Positions[0].gameObject.transform.parent.GetComponent<LineAble>().nowObjMode;
                        LineAble.ObjMode tempNowObjMode = temp.transform.parent.GetComponent<LineAble>().nowObjMode;

                        if(DrawingNowObjMode != tempNowObjMode)
                        {
                            //첫번째로 연결된 Linable과 Objmode가 같아야만 이어질 수 있도록 한다.
                            if((DrawingNowObjMode == LineAble.ObjMode.Base) || (tempNowObjMode == LineAble.ObjMode.Base)){
                                //하지만 둘 중 하나의 mode가 Base라면 이어질 수 있게 한다.

                                if((DrawingNowObjMode == LineAble.ObjMode.Cannon) || (tempNowObjMode == LineAble.ObjMode.Cannon)){
                                    //지금은 그 중에서도 Base와 Cannon과의 연결만 허용하도록 한다. 
                                    //상위 If에서 Base가 하나라도 있는 것을 확인했으니, 둘 중에 Cannon이 있는지 확인한다.
                                }else{
                                    //만약 둘 중에 Cannon이 없다면, 이어지지 않도록 한다.
                                    DrwaingLineObject.GetComponent<LineRendererConnector>().Positions.Add(Cursur.transform);
                                    goto DuplicatedInteractionTarget;
                                }

                            }else{
                                DrwaingLineObject.GetComponent<LineRendererConnector>().Positions.Add(Cursur.transform);
                                goto DuplicatedInteractionTarget;
                            }
                            
                        }else{
                            LineAble.DetailMode DrawingDetailMode = DrwaingLineObject.GetComponent<LineRendererConnector>().Positions[0].gameObject.transform.parent.GetComponent<LineAble>().nowDetailMode;
                            LineAble.DetailMode tempDetailMode = temp.transform.parent.GetComponent<LineAble>().nowDetailMode;
                            
                            if(DrawingDetailMode == tempDetailMode){
                                //여기서 거르는 것은 Lazer - Lazer, Cannon - Cannon, Button - Button, Water - Water 등이 이어질 수 없게 하는 것이다.
                                if( DrawingDetailMode != LineAble.DetailMode.Teleport){
                                    //Objmode가 같아서 들어왔다. 그 중에서도 detailmode가 달라야만 이어질 수 있도록 한다.
                                    //하지만, 이 때 Teleport 같은 경우 같은 객체를 연결하는 것이기 때문에 ObjMode가 Teleport라면 이어질 수 있게한다.
                                    DrwaingLineObject.GetComponent<LineRendererConnector>().Positions.Add(Cursur.transform);
                                    goto DuplicatedInteractionTarget;
                                }        
                                
                            }
                            
                            /*
                                예시) Lazer와 Button을 연결할 때
                                                Lazer       Button
                                ObjMode         Lazer       Lazer       : ObjMode 같음 (O)
                                DetailMode      Lazer       Button      : DetailMode 다름 (O)
                                                                    ====> 연결 가능

                                            
                                예시) Lazer와 Lazer을 연결할 때
                                                Lazer       Lazer
                                ObjMode         Lazer       Lazer       : ObjMode 같음 (O)
                                DetailObjMode   Lazer       Lazer       : DetailMode 같음 (X)
                                                                    ====> 연결 불가능

                                예시) Lazer와 Cannon을 연결할 때
                                                Lazer       Cannon
                                ObjMode         Lazer       Cannon       : ObjMode 다름 (X)
                                DetailObjMode   Lazer       Cannon       : DetailMode 다름 (O)
                                                                    ====> 연결 불가능

                                !!!!특이 사항!!!!

                                    보통은 위와 같은 규칙을 적용하지만, ObjMode가 Base일 경우에는 다르다.
                                    -> 이 때 ObjMode가 Base란 뜻은, 물통과 같이 특정 기믹만 연결되는 것이 아니라 다양한 기믹들과 연결되는 기믹을 말한다.

                                    예시) Water와 Cannon을 연결할 때
                                                    Water       Cannon
                                    ObjMode         Base        Cannon       : ObjMode 다름 (O)
                                    DetailObjMode   Bowl        Cannon       : DetailMode 다름 (O)
                                                                        ====> 연결 가능

                                    예시) Water와 Water를 연결할 때
                                                    Water       Water
                                    ObjMode         Base        Base       : ObjMode 같음 (X)
                                    DetailObjMode   Bowl        Bowl       : DetailMode 같음 (X)
                                                                        ====> 연결 불가능

                                    이렇게 규칙과 위배되는 부분이 만들어지는 이유는, Base라는 것이 들어갔기 때문이다. 
                                    하나의 기믹으로 여러 기믹을 연결할 경우 이렇게 세세하게 나눠야 한다.
                                    만약, 각 기믹 별로 연결되는 물통이 있다면 본래 세웠던 규칙으로 진행하면 된다. 
                                        (원래 규칙으로 돌아가는거면, if 어쩌구 Base 라고 되어있는거 삭제해도 된다. 하지만 Base만 따로 예외 처리하는 것이니 놔둬도 된다.)
                                !!!!         !!!!
                            */
                        }
                    }
                    DrwaingLineObject.GetComponent<LineRendererConnector>().Positions.Add(temp.transform);
                    temp.transform.parent.GetComponent<LineAble>().LineObjectAdd(InteractionLayer,DrwaingLineObject);
                    ConnectedInteraction ++;
                    DebugX.Log("ConnectedInteraction 개수지롱: " + ConnectedInteraction.ToString());
                    DrwaingLineObject.GetComponent<LineRendererConnector>().positionCnt = ConnectedInteraction;
                    DrwaingLineObject.GetComponent<LineRendererConnector>().Positions.Add(Cursur.transform);
                    if(ConnectedInteraction==3){
                        StopDrawingLine();
                    }        
                    
                    
                
                }
                DuplicatedInteractionTarget:
                DebugX.Log("LineRederer Code End " + ConnectedInteraction.ToString());
                    //isFirstLine=false;

            }
        }
        

    }
    void StopDrawingLine()
    {
        
        if(DrwaingLineObject)
        {
            if(ConnectedInteraction==1)
        {
            DebugX.Log(ConnectedInteraction+"is ConnectedInteraction");
            Edit.DeleteEditorObject(DrwaingLineObject.GetComponent<Position>().ID);
            Destroy(DrwaingLineObject);
            ConnectedInteraction=0;
            
        }
        else
        {
            Edit.DeleteEditorObject(DrwaingLineObject.GetComponent<Position>().ID);
            DrwaingLineObject.GetComponent<Position>().ID=Edit.AddEditorObject(DrwaingLineObject,0,InteractionLayer); 
            DrwaingLineObject.GetComponent<Position>().Layer = 0;
        }
            DrwaingLineObject.GetComponent<LineRendererConnector>().Positions.Remove(Cursur.transform);
            ConnectedInteraction=0;
        }

    }
    bool  TileDeletor(){
        var  temp = Cursur.GetComponent<Cursur_Detecting>().CheckingBlockExist();
        if(temp){
            if(temp.name=="MapEditorBox")
            {
                if(temp.gameObject.transform.parent.gameObject.GetComponent<Position>().tileMode != 1 || nowDevelopermode){ //tilemode가 0 이거나 개발자 모드일 때만 자유롭게 삭제 가능.
                    if(temp.transform.tag!="MapEditor"){ 
                        //Door, Brush, Player의 MapEditor 박스는 tag가 MapEditor로 되어있다.
                        //즉, 만약 지울 수 없게 만들고 싶으면 해당 객체의 자식 객체인 mapeditor box의 tag를 MapEditor로 바꿔주면된다.
                        
                        if(temp.gameObject.transform.parent.gameObject.GetComponent<Position>().tileMode == 0 ){
                            if((nowMode != Mode.Create && canDelete) || nowDevelopermode){
                                Edit.SetPossibleDeleteCnt();
                                Edit.DeleteEditorObject(temp.transform.parent.GetComponent<Position>().ID);
                                if(temp.transform.parent.GetComponent<LineAble>())
                                {
                                    temp.transform.parent.GetComponent<LineAble>().DeletedSelf();
                                }
                                    Destroy(temp.transform.parent.gameObject);
                            }else{
                                return false;
                            }
                            
                        }else{
                            Edit.DeleteEditorObject(temp.transform.parent.GetComponent<Position>().ID);
                            if(temp.transform.parent.GetComponent<LineAble>())
                            {
                                temp.transform.parent.GetComponent<LineAble>().DeletedSelf();
                            }
                                Destroy(temp.transform.parent.gameObject);
                        }
                        
                    }
                    else
                    {
                        return false;
                    }
                }else{
                    return false;
                }
                
                
                
            }
            else
            {
                
                if(temp.name!="Base"){
                    if(temp.gameObject.GetComponent<Position>().tileMode != 1 || nowDevelopermode){ //tilemode가 0 이거나 개발자 모드일 때만 자유롭게 삭제 가능.
                        if(temp.tag=="Ground" && nowMode == Mode.Delete)//Is A GroundBlock Then Recheck About Outline
                        {
                            temp.gameObject.GetComponent<Tile_OutLiner>().LineDelete();
                        }
                        
                        if(temp.gameObject.GetComponent<Position>().tileMode == 0 ){
                            if((nowMode != Mode.Create && canDelete) || nowDevelopermode){
                                Edit.SetPossibleDeleteCnt();
                                Edit.DeleteEditorObject(temp.GetComponent<Position>().ID);//Destroy A Block That Doesn't Have any Map Editor Box
                                Destroy(temp.gameObject); 
                                    // return true;    
                            }else{
                                return false;
                            }
                            
                        }else{
                            Edit.DeleteEditorObject(temp.GetComponent<Position>().ID);//Destroy A Block That Doesn't Have any Map Editor Box
                            Destroy(temp.gameObject); 
                        }
                        
                    }else{
                        return false;
                    }
                
                }   
                    
            }
        }
        return true;
    }
    void TouchInputtoInt(){
        //여기 함수에 Create 시에 맵 생성 제한을 거는 코드들 있음.
        TouchInput.z=0;
        
        if(!nowDevelopermode){
            if(TouchInput.y>7){
                TouchInput.y=7;
                IsOut= true;
            }
            else
                IsOut= false;
        }
        
        
        IsOut=false;
        if((int)TouchInput.x-0.5f>=TouchInput.x)
        {
            TouchInput.x=(int)TouchInput.x-1;
        }else if(TouchInput.x>=(int)TouchInput.x+0.5f){
            TouchInput.x=(int)TouchInput.x+1;
        }
        else
            TouchInput.x=(int)TouchInput.x;
        
        
        // 다시 키셈 (커서 제한 풀어 놓은 것)
        if(!nowDevelopermode){
            if(TouchInput.x>16)
            {
                TouchInput.x=16;
            }
            else if(TouchInput.x<-19)
            {
                TouchInput.x= -19;
            }
            
        }
        
        


        if((int)TouchInput.y-0.5f>=TouchInput.y)
        {
            TouchInput.y=(int)TouchInput.y-1;
        }else if(TouchInput.y>=(int)TouchInput.y+0.5f){
            TouchInput.y=(int)TouchInput.y+1;
        }
        else
            TouchInput.y=(int)TouchInput.y;

        
        if(!nowDevelopermode){
            if(TouchInput.y>6)
            {
                TouchInput.y=6;
            }
            else if(TouchInput.y<-10)
            {
                TouchInput.y=-10;
            }
        }
       
        
        
    }
    public void MouseMoveChecker(){
        if(priviousX==TouchInput.x&&TouchInput.y==priviousY)
        {
            IsMouseMove=false;
        }
        else{
            IsMouseMove=true;
        }
        priviousY=TouchInput.y;
        priviousX=TouchInput.x;
    }
    public void ModeReset(){
        nowMode=Mode.Move;
        setMode[1].isOn = true;
    }
    public void modeChange(int value) {
        
                Camera.main.transform.GetComponent<Cinemachine.Examples.MobileCamController>().canMove=false;
        switch(value) {
            case 0:
                nowMode = Mode.Create;
                break;
            case 1:
                nowMode = Mode.Move;
                break;
            case 2:
                nowMode = Mode.Delete;
                break;
            case 3:
                nowMode = Mode.Camera;
                Camera.main.gameObject.GetComponent<Cinemachine.Examples.MobileCamController>().canMove=true;
            break;

        }
    }

    public GameObject GetMovingInfo(){ //Cursur_Detecting과 연결하기 위한 Get 함수.
        return Moving;
    }

    private void MoveSettingbyKeyBoard(){ 
        if(isButtonKeepRight){
            TouchInput = TouchInput + new Vector3(1, 0, 0);
            TouchInputtoInt();
            Cursur.transform.position=TouchInput;
        }

        if(isButtonKeepLeft){
            TouchInput = TouchInput + new Vector3(-1, 0, 0);
            TouchInputtoInt();
            Cursur.transform.position=TouchInput;
        }

        if(isButtonKeepUp){
            TouchInput = TouchInput + new Vector3(0, 1, 0);
            TouchInputtoInt();
            Cursur.transform.position=TouchInput;
        }
        
        if(isButtonKeepDown){
            
            TouchInput = TouchInput + new Vector3(0, -1, 0);
            
            
            TouchInputtoInt();
            Cursur.transform.position=TouchInput;
        }
        
        if(isButtonRight){
            getbuttonRight = getbuttonRight + Time.deltaTime;
            if(getbuttonRight > 0.15f){
                isButtonKeepRight = true;
                getbuttonRight = 0f;    
            }else{
                isButtonKeepRight = false;    
            }
        }

        else{ // isButtonRight가 false => 버튼을 땠을 때
            isButtonKeepRight = false;
            getbuttonRight = 0f;
        }
        
        if(isButtonLeft){
            getbuttonLeft = getbuttonLeft + Time.deltaTime;
            if(getbuttonLeft > 0.15f){
                isButtonKeepLeft = true;
                getbuttonLeft = 0f;    
            }else{
                isButtonKeepLeft = false;    
            }
        }

        else{
            isButtonKeepLeft = false;
            getbuttonLeft = 0f;
        }

        if(isButtonUp){
            getbuttonUp = getbuttonUp + Time.deltaTime;
            if(getbuttonUp > 0.15f){
                isButtonKeepUp = true;
                getbuttonUp = 0f;    
            }else{
                isButtonKeepUp = false;    
            }
        }

        else{
            isButtonKeepUp = false;
            getbuttonUp = 0f;
        }

        if(isButtonDown){
            getbuttonDown = getbuttonDown + Time.deltaTime;
            if(getbuttonDown > 0.15f){
                isButtonKeepDown = true;
                getbuttonDown = 0f;    
            }else{
                isButtonKeepDown = false;    
            }
        }
        else{
            isButtonKeepDown = false;
            getbuttonDown = 0f;
        }
    }
}
