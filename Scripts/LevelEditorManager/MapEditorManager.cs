/*
커스텀 레벨 에디터 관리 시스템

주요 기능
전체 커스텀 레벨 목록 Read
- ReadCustomMapTable() : 서버에 올라간 전체 커스텀 맵 List Read
- ReadLocalMapTable() : 현재 내가 제작 중인 임시 저장 맵 List Read

전체 커스텀 레벨 목록 Sort
- SortMapByDateTime() : 업로드 날짜 순 정렬
- SortMyMap() : 서버에 올라간 커스텀 레벨 중 내가 제작한 맵만 보기
- SortLocalMap() : 임시 제작중인 커스텀 레벨을 날짜순으로 정렬
- SortMapByPlayCount() : 플레이 횟수 순으로 정렬
- SortMapByLikeCount() : 좋아요 순으로 정렬
- SearchMap() : 맵 검색

해당 커스텀 레벨 Load / Unload
- OpenPreviewMode() : 커스텀 레벨 목록에서 레벨을 선택해 미리보기로 열어 실제 맵 이미지 미리 보기
- OpenMapInfo() : OpenPreviewMode()로 커스텀 레벨을 열 때 해당 맵 정보도 UI를 통해 보여주기 & 버튼 연결
- LoadSelectedMap() : 현재 미리보기로 열려 있는 커스텀 레벨 실제 Loading
- ClosePreviewMode() : 미리 보기 끄기

새로운 커스텀 레벨 제작
- CreateNewMap() : 새로운 레벨 제작을 위한 맵 제작 툴로 이동

임시 저장 중인 레벨 삭제
- DeleteSavedMap() : 임시 제작 중인 레벨 삭제 (단, 한 번 전체 공개로 서버에 올라간 레벨은 삭제 불가)

유저 차단 기능
- ReportUser() : 해당 커스텀 레벨을 만든 유저 차단 (앞으로 해당 유저가 만든 맵 플레이 불가능 & 게임사에게 신고 내용 전달)
- CheckBlockedUser() : 차단 유저를 확인해 커스텀 레벨 비활성화
- DeleteBlockedUser() : 차단 유저 차단 해제

맵 신고 기능
- ReportMap() : 타 유저에게 불쾌감을 주는 맵 신고하는 기능 (게임사에게 신고 내용 전달) 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using BackEnd;
using System.Linq; // List 정렬할 때 사용함

public class MapInformation {
    public string mapName;
    public string dateTime;
    public string madeBy;
    public int playCount;
    public int like;
    public string mapInformation;
    public string owner_inDate;
    public string inDate;

    public MapInformation(string _mapName, string _dateTime, string _madeBy, int _playCount, int _like, string _mapInfo, string _owner_inDate, string _inDate) {
        mapName = _mapName;
        dateTime = _dateTime;
        madeBy = _madeBy;
        playCount = _playCount;
        like = _like;
        mapInformation = _mapInfo;
        owner_inDate = _owner_inDate;
        inDate = _inDate;
    }
}

public class MapEditorManager : MonoBehaviour
{
    string dirPath;
    DirectoryInfo dir;
    FileInfo[] info;
    public GameObject mapBtn;
    public GameObject currentCustomMap;
    private int rowCount;
    private List<MapInformation> mapInfos = new List<MapInformation>();
    private List<MapInformation> localMapInfos = new List<MapInformation>();
    public GameObject mapOpenPanel;
    // public GameObject mapOpenPanelDefaultButton;
    public GameObject deletePanel;
    public Button playBtn;
    public Button userReportBtn;
    public Button mapReportBtn;
    public Button realMapReportBtn;
    public Button editBtn;
    public Button deleteBtn;
    public GameObject savedMapBtn;
    public Text mapName;
    public Text mapMadeBy;
    public string mapOwnerInDate;
    public List<GameObject> mapBtns = new List<GameObject>();

    public Toggle[] reportToggle;
    public InputField explain;

    public InputField searchField;

    public Toggle[] mapToggle; // New, Most Played, Most Liked, My Map 선택하는 토글

    //2023-03-01 맵에디터 입장권 소모했을 시 띄울 광고 패널
    public GameObject ticketAdsPanel;

    //플레이어 / 에디터 구분
    public bool isOthers;

    public CursorController cursorController;

    public FirstMapSelectController fmsController;

    public Button prevBtn;

    public Sprite[] RandomImg;
    
    void Start() {
        ReadCustomMapTable();
        ReadLocalMapTable();
        BackendGameData.Instance.BlockedUsersGet();
        
        currentCustomMap = GameObject.Find("CurrentCustomMap");
        currentCustomMap.GetComponent<CurrentCustomMap>().currentMapName = "";
         
        DeleteAllMapBtns();

        int index = 0;

        foreach(MapInformation map in mapInfos)
        {
            index++;
        
            GameObject go = Instantiate(mapBtn);
            go.transform.SetParent(GameObject.Find("Content").transform);
            go.transform.localScale= new Vector3(1,1,1);
            go.GetComponent<Image>().sprite = RandomImg[index%4];
            go.GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = map.mapName;
            go.GetComponent<Button>().transform.GetChild(1).GetComponent<Text>().text = map.madeBy;
            go.GetComponent<Button>().transform.GetChild(2).GetComponent<Text>().text = "Played: " + map.playCount.ToString();
            go.GetComponent<Button>().transform.GetChild(3).GetComponent<Text>().text = "Like: " + map.like.ToString();
            go.GetComponent<Button>().transform.GetChild(4).GetComponent<Text>().text = "inDate: " + map.inDate.ToString();
            go.GetComponent<Button>().transform.GetChild(5).GetComponent<Text>().text = map.owner_inDate.ToString();
            //go.GetComponent<Button>().transform.GetChild(4).GetComponent<Text>().text = map.dateTime.ToString();
            mapBtns.Add(go);
            //go.GetComponent<Button>().onClick.AddListener(LoadSelectedMap);
            go.GetComponent<Button>().onClick.AddListener(OpenMapInfo);
            go.GetComponent<Button>().onClick.AddListener(delegate{MemoryPrevSelectedObject(go);});

        }

        if(mapBtns.Count > 0){
            fmsController.SetNavigationDown(mapBtns[0]);
        }
        CheckBlockedUser();
        
    }

    public void DeleteAllMapBtns() { // 모든 맵 리스트에서 지우기
        mapBtns.Clear();
        GameObject[] maps = GameObject.FindGameObjectsWithTag("MapBtn");
        for (int i = 0; i < maps.Length; i++) {
            Destroy(maps[i]);
        }
    }

    public void SortMapByDateTime() { // 최근 날짜 순 정렬
        DeleteAllMapBtns();
        
        int index = 0;

        foreach(MapInformation map in mapInfos) {
            index++;
            GameObject go = Instantiate(mapBtn);
            go.transform.SetParent(GameObject.Find("Content").transform);
            go.transform.localScale= new Vector3(1,1,1);
            go.GetComponent<Image>().sprite = RandomImg[index%4];
            go.GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = map.mapName;
            go.GetComponent<Button>().transform.GetChild(1).GetComponent<Text>().text = map.madeBy;
            go.GetComponent<Button>().transform.GetChild(2).GetComponent<Text>().text = "Played: " + map.playCount.ToString();
            go.GetComponent<Button>().transform.GetChild(3).GetComponent<Text>().text = "Like: " + map.like.ToString();
            go.GetComponent<Button>().transform.GetChild(4).GetComponent<Text>().text = "inDate: " + map.inDate.ToString();
            go.GetComponent<Button>().transform.GetChild(5).GetComponent<Text>().text = map.owner_inDate.ToString();
            mapBtns.Add(go);
            go.GetComponent<Button>().onClick.AddListener(OpenMapInfo);
            go.GetComponent<Button>().onClick.AddListener(delegate{MemoryPrevSelectedObject(go);});
        }

        if(mapBtns.Count > 0){
            fmsController.SetNavigationDown(mapBtns[0]);
        }
        CheckBlockedUser();
    }

    public void SortMyMap() { // 내가 제작한 맵만 보기
        DeleteAllMapBtns();
        
        int index = 0;

        foreach(MapInformation map in mapInfos) {
            index++;
            DebugX.Log("Sort MyMap 맵 주인: " + map.owner_inDate + "내꺼: " + BackendGameData.Instance.GetUserInDate());
            if(map.owner_inDate == BackendGameData.Instance.GetUserInDate()) {
                GameObject go = Instantiate(mapBtn);
                go.transform.SetParent(GameObject.Find("Content").transform);
                go.transform.localScale= new Vector3(1,1,1);
                go.GetComponent<Image>().sprite = RandomImg[index%4];
                go.GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = map.mapName;
                go.GetComponent<Button>().transform.GetChild(1).GetComponent<Text>().text = map.madeBy;
                go.GetComponent<Button>().transform.GetChild(2).GetComponent<Text>().text = "Played: " + map.playCount.ToString();
                go.GetComponent<Button>().transform.GetChild(3).GetComponent<Text>().text = "Like: " + map.like.ToString();
                go.GetComponent<Button>().transform.GetChild(4).GetComponent<Text>().text = "inDate: " + map.inDate.ToString();
                go.GetComponent<Button>().transform.GetChild(5).GetComponent<Text>().text = map.owner_inDate.ToString();
                mapBtns.Add(go);
                go.GetComponent<Button>().onClick.AddListener(OpenMapInfo);
                go.GetComponent<Button>().onClick.AddListener(delegate{MemoryPrevSelectedObject(go);});
            }
        }

        if(mapBtns.Count > 0){
            fmsController.SetNavigationDown(mapBtns[0]);
        }
    }

    public void SortLocalMap() { // 내가 제작한 맵만 보기
        DeleteAllMapBtns();
        int index = 0;

        foreach(MapInformation map in localMapInfos) {

            index++;
            DebugX.Log("Sort MyMap 맵 주인: " + map.owner_inDate + "내꺼: " + BackendGameData.Instance.GetUserInDate());
            if(map.owner_inDate == BackendGameData.Instance.GetUserInDate()) {
                GameObject go = Instantiate(mapBtn);
                go.transform.SetParent(GameObject.Find("Content").transform);
                go.transform.localScale= new Vector3(1,1,1);
                go.GetComponent<Image>().sprite = RandomImg[index%4];
                go.GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = map.mapName;
                go.GetComponent<Button>().transform.GetChild(1).GetComponent<Text>().text = map.madeBy;
                go.GetComponent<Button>().transform.GetChild(2).GetComponent<Text>().text = "Played: " + map.playCount.ToString();
                go.GetComponent<Button>().transform.GetChild(3).GetComponent<Text>().text = "Like: " + map.like.ToString();
                go.GetComponent<Button>().transform.GetChild(4).GetComponent<Text>().text = "inDate: " + map.inDate.ToString();
                go.GetComponent<Button>().transform.GetChild(5).GetComponent<Text>().text = map.owner_inDate.ToString();
                mapBtns.Add(go);
                go.GetComponent<Button>().onClick.AddListener(OpenMapInfo);
                go.GetComponent<Button>().onClick.AddListener(delegate{MemoryPrevSelectedObject(go);});
            }
        }

        if(mapBtns.Count > 0){
            fmsController.SetNavigationDown(mapBtns[0]);    
        }
        
    }

    public void SortMapByPlayCount(bool isDescending) { // 플레이횟수대로 정렬, isDescending == true이면 내림차순
        DeleteAllMapBtns();

        List<MapInformation> newList = new List<MapInformation>();
        if(isDescending) {
            newList = mapInfos.OrderByDescending(p => p.playCount).ThenBy(x => x.mapName).ToList();
        }
        else {
            newList = mapInfos.OrderBy(p => p.playCount).ThenBy(x => x.mapName).ToList();
        }
        
        int index = 0;

        foreach(MapInformation map in newList) {
            index++;
            GameObject go = Instantiate(mapBtn);
            go.transform.SetParent(GameObject.Find("Content").transform);
            go.transform.localScale= new Vector3(1,1,1);
            go.GetComponent<Image>().sprite = RandomImg[index%4];
            go.GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = map.mapName;
            go.GetComponent<Button>().transform.GetChild(1).GetComponent<Text>().text = map.madeBy;
            go.GetComponent<Button>().transform.GetChild(2).GetComponent<Text>().text = "Played: " + map.playCount.ToString();
            go.GetComponent<Button>().transform.GetChild(3).GetComponent<Text>().text = "Like: " + map.like.ToString();
            go.GetComponent<Button>().transform.GetChild(4).GetComponent<Text>().text = "inDate: " + map.inDate.ToString();
            go.GetComponent<Button>().transform.GetChild(5).GetComponent<Text>().text = map.owner_inDate.ToString();
            mapBtns.Add(go);
            go.GetComponent<Button>().onClick.AddListener(OpenMapInfo);
            go.GetComponent<Button>().onClick.AddListener(delegate{MemoryPrevSelectedObject(go);});
        }

        if(mapBtns.Count > 0){
            fmsController.SetNavigationDown(mapBtns[0]);
        }
        CheckBlockedUser();
    }

    public void SortMapByLikeCount(bool isDescending) { // 좋아요 수대로 정렬, isDescending == true이면 내림차순
        DeleteAllMapBtns();

        List<MapInformation> newList = new List<MapInformation>();
        if(isDescending) {
            newList = mapInfos.OrderByDescending(p => p.like).ThenBy(x => x.mapName).ToList();
        }
        else {
            newList = mapInfos.OrderBy(p => p.like).ThenBy(x => x.mapName).ToList();
        }
        
        int index1 = 0;

        foreach(MapInformation map in newList) {
            index1++;
            GameObject go = Instantiate(mapBtn);
            go.transform.SetParent(GameObject.Find("Content").transform);
            go.transform.localScale= new Vector3(1,1,1);
            go.GetComponent<Image>().sprite = RandomImg[index1%4];
            go.GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = map.mapName;
            go.GetComponent<Button>().transform.GetChild(1).GetComponent<Text>().text = map.madeBy;
            go.GetComponent<Button>().transform.GetChild(2).GetComponent<Text>().text = "Played: " + map.playCount.ToString();
            go.GetComponent<Button>().transform.GetChild(3).GetComponent<Text>().text = "Like: " + map.like.ToString();
            go.GetComponent<Button>().transform.GetChild(4).GetComponent<Text>().text = "inDate: " + map.inDate.ToString();
            go.GetComponent<Button>().transform.GetChild(5).GetComponent<Text>().text = map.owner_inDate.ToString();
            mapBtns.Add(go);
            go.GetComponent<Button>().onClick.AddListener(OpenMapInfo);
            go.GetComponent<Button>().onClick.AddListener(delegate{MemoryPrevSelectedObject(go);});
        }

        if(mapBtns.Count > 0){
            fmsController.SetNavigationDown(mapBtns[0]);
        }
        CheckBlockedUser();
    }

    public void LoadSelectedMap() {
        DataController.Instance.gameData.playMode = PlayMode.MAPEDITOR;
        DataController.Instance.gameData.playerCanMove = true;

        /*
            24-12-10 
            Level Editor에서도 플레이하면서 낙서 기능을 이용할 수 있도록 DoodleParent 생성.
            하지만 Level Editor에서 미리 보기로 맵이 생성이 되면 doodleCanvas가 켜지면서 Stage_Mapeditor에서 클릭이 안되는 상황 발생
            따라서 LoadSelectedMap을 진행할 때 DoodleCanvas를 찾아서 켜주도록 하였다.

            24-01-02
            LoadSelectedMap() 안에서 내 맵이 아닐 때만 DoodleCanvas를 찾아서 켜주는 것으로 변경
            내 맵일때는 Edit 모드이므로 켜지면 안됨
        */
        
         
        if(isOthers) {
            if(DataController.Instance.gameData.customMapPlayTicket > 0) { // 맵에디터 입장권 있으면 입장권 소모하고 입장
                currentCustomMap.GetComponent<CurrentCustomMap>().ChangePreviewMode(false);

                // 25-01-02 내 맵이 아닐 때만 DoodleCanvas 켜기
                GameObject.Find("DoodleParentOnGridNew").transform.GetChild(0).gameObject.SetActive(true);

                SceneManager.UnloadSceneAsync("Stage_MapEditor");
            }
            else { // 맵에디터 입장권 다 쓰면
                currentCustomMap.GetComponent<CurrentCustomMap>().IsAdsPanelOpen(true);
                ticketAdsPanel.SetActive(true); // 광고 패널 띄우기
            }
        }
        
        else {
            DebugX.Log("내 맵 수정");
            currentCustomMap.GetComponent<CurrentCustomMap>().ChangePreviewMode(false);
            SceneManager.UnloadSceneAsync("Stage_MapEditor");
        }

        
    }

    public void OpenPreviewMode() {
        //yield return new WaitForSeconds(0.0f);
        SceneManager.LoadScene("Grid_NewAssets", LoadSceneMode.Additive);
        currentCustomMap.GetComponent<CurrentCustomMap>().currentMapName = mapName.text;
        currentCustomMap.GetComponent<CurrentCustomMap>().currentMapMadeBy = mapMadeBy.text;
        
        if(savedMapBtn.GetComponent<Toggle>().isOn == true) {
            foreach(MapInformation map in localMapInfos) {
                if(map.mapName == mapName.text && map.madeBy == mapMadeBy.text) {
                    currentCustomMap.GetComponent<CurrentCustomMap>().currentMapInfo = map.mapInformation;
                    currentCustomMap.GetComponent<CurrentCustomMap>().currentMapLike = map.like;
                    currentCustomMap.GetComponent<CurrentCustomMap>().currentMapOwnerinDate = map.owner_inDate;
                    currentCustomMap.GetComponent<CurrentCustomMap>().currentMapinDate = map.inDate;
                    
                }
            }    
        }

        else {
            foreach(MapInformation map in mapInfos) {
                if(map.mapName == mapName.text && map.madeBy == mapMadeBy.text) {
                    currentCustomMap.GetComponent<CurrentCustomMap>().currentMapInfo = map.mapInformation;
                    currentCustomMap.GetComponent<CurrentCustomMap>().currentMapLike = map.like;
                    currentCustomMap.GetComponent<CurrentCustomMap>().currentMapOwnerinDate = map.owner_inDate;
                    currentCustomMap.GetComponent<CurrentCustomMap>().currentMapinDate = map.inDate;
                    
                }
            }
        }
        
        DataController.Instance.gameData.isMapEditorPreview = true;
    }
    public void ClosePreviewMode() {
        SceneManager.UnloadSceneAsync("Grid_NewAssets");
        DataController.Instance.gameData.isMapEditorPreview = false;
    }

    public void SetCurrentSelectedButton(){
        if(prevBtn != null){
            EventSystem.current.SetSelectedGameObject(prevBtn.gameObject);
        }
    }
    void OpenMapInfo() {
        playBtn.onClick.RemoveAllListeners();
        userReportBtn.onClick.RemoveAllListeners();
        mapReportBtn.onClick.RemoveAllListeners();
        realMapReportBtn.onClick.RemoveAllListeners();
        editBtn.onClick.RemoveAllListeners();

        mapOpenPanel.SetActive(true);
        playBtn.gameObject.SetActive(true);
        
        userReportBtn.gameObject.SetActive(true);
        userReportBtn.interactable = true;
        
        mapReportBtn.gameObject.SetActive(true);
        mapReportBtn.interactable = true;
        
        mapName.text = EventSystem.current.currentSelectedGameObject.GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text;
        mapMadeBy.text = EventSystem.current.currentSelectedGameObject.GetComponent<Button>().transform.GetChild(1).GetComponent<Text>().text;
        mapOwnerInDate = EventSystem.current.currentSelectedGameObject.GetComponent<Button>().transform.GetChild(5).GetComponent<Text>().text;
        
        playBtn.onClick.AddListener(delegate{SetPlayEditMode(true);});
        playBtn.onClick.AddListener(delegate{UpdatePlayCount(mapName.text, mapMadeBy.text);});
        playBtn.onClick.AddListener(LoadSelectedMap);
        

        editBtn.onClick.AddListener(delegate{SetPlayEditMode(false);});
        editBtn.onClick.AddListener(LoadSelectedMap);

        deleteBtn.onClick.AddListener(OpenDeletePanel);

        userReportBtn.onClick.AddListener(ReportUser);

        playBtn.Select();

        Where where = new Where();
        where.Equal("MapName", mapName.text);
        where.Equal("MadeBy", mapMadeBy.text);

        var bro = Backend.GameData.GetMyData("ReportedMap_List", where);

        
        if (bro.GetReturnValuetoJSON()["rows"].Count > 0)
        {
            // 요청이 성공해도 where 조건에 부합하는 데이터가 없을 수 있기 때문에
            // 데이터가 존재하는지 확인
            // 위와 같은 new Where() 조건의 경우 테이블에 row가 하나도 없으면 Count가 0 이하 일 수 있다.
            DebugX.Log(bro);
            mapReportBtn.interactable = false;
            DebugX.Log("이미 신고된 맵");
        }
        

        realMapReportBtn.onClick.AddListener(() => ReportMap(mapName.text, mapMadeBy.text));

        DebugX.Log("만든 사람: " + mapMadeBy.text + " 현재 닉네임: " + BackendGameData.Instance.GetUserNickName());
        

        if(savedMapBtn.GetComponent<Toggle>().isOn == true) {
            LocalSaveMapHandler();
        }

        else {
            DebugX.Log("맵주인: " + mapOwnerInDate + " " + BackendGameData.Instance.GetUserInDate());
            if(mapOwnerInDate == BackendGameData.Instance.GetUserInDate())
            {
                userReportBtn.gameObject.SetActive(false);
                mapReportBtn.gameObject.SetActive(false);
                editBtn.gameObject.SetActive(false);
                deleteBtn.gameObject.SetActive(false);
                
            }
            else
            {
                userReportBtn.gameObject.SetActive(true);
                mapReportBtn.gameObject.SetActive(true);
                editBtn.gameObject.SetActive(false);
                deleteBtn.gameObject.SetActive(false);
            }
        }
        
        OpenPreviewMode();

    }

    public void MemoryPrevSelectedObject(GameObject temp){
        //MapOpenHandler가 꺼질 때 다시 해당 버튼으로 select 되도록하기 위해 MapBtn을 눌렀을 때, 현재 어떤 버튼인지 기억한다. 
        prevBtn = temp.GetComponent<Button>();
    }

    public void ReportUser() {
        if(!BackendGameData.Instance.AddBlockedUser(mapMadeBy.text)) {
            return;
        }
        BackendGameData.Instance.BlockedUsersUpdate();
        CheckBlockedUser();

        userReportBtn.interactable = false;
    }

    public void ReadCustomMapTable() {
        // 조건 없이 모든 데이터 조회하기
        var bro = Backend.GameData.Get("CustomMap_List", new Where(), 100);
        if (bro.IsSuccess() == false)
        {
            // 요청 실패 처리
            DebugX.Log(bro);
            return;
        }
        if (bro.GetReturnValuetoJSON()["rows"].Count <= 0)
        {
            // 요청이 성공해도 where 조건에 부합하는 데이터가 없을 수 있기 때문에
            // 데이터가 존재하는지 확인
            // 위와 같은 new Where() 조건의 경우 테이블에 row가 하나도 없으면 Count가 0 이하 일 수 있다.
            DebugX.Log(bro);
            return;
        }

        rowCount = bro.Rows().Count;
        DebugX.Log("rowCount: " + rowCount);

        // 검색한 데이터의 모든 row의 inDate 값 확인
        for(int i=0; i<bro.Rows().Count; ++i)
        {
            MapInformation map = new MapInformation (
                bro.Rows()[i]["MapName"]["S"].ToString(),
                bro.Rows()[i]["inDate"]["S"].ToString(),
                bro.Rows()[i]["MadeBy"]["S"].ToString(),
                int.Parse(bro.Rows()[i]["PlayCount"]["N"].ToString()),
                int.Parse(bro.Rows()[i]["Like"]["N"].ToString()),
                bro.Rows()[i]["MapInfo"]["S"].ToString(),
                bro.Rows()[i]["owner_inDate"]["S"].ToString(),
                bro.Rows()[i]["inDate"]["S"].ToString()
                
            );

            mapInfos.Add(map); 
        }
        
        while(bro.HasFirstKey() == true) {
            var firstKey = bro.FirstKeystring();
            DebugX.Log("FirstKey는 존재한다! " + firstKey);

            bro = Backend.GameData.Get("CustomMap_List", new Where(), 100, firstKey);
            if(bro.IsSuccess() == false)
            {
                // 실패 처리
                return;
            }
            // 검색한 데이터의 모든 row의 inDate 값 확인
            for(int i=0; i<bro.Rows().Count; ++i)
            {
                MapInformation map = new MapInformation (
                    bro.Rows()[i]["MapName"]["S"].ToString(),
                    bro.Rows()[i]["inDate"]["S"].ToString(),
                    bro.Rows()[i]["MadeBy"]["S"].ToString(),
                    int.Parse(bro.Rows()[i]["PlayCount"]["N"].ToString()),
                    int.Parse(bro.Rows()[i]["Like"]["N"].ToString()),
                    bro.Rows()[i]["MapInfo"]["S"].ToString(),
                    bro.Rows()[i]["owner_inDate"]["S"].ToString(),
                    bro.Rows()[i]["inDate"]["S"].ToString()
                    
                );


                mapInfos.Add(map); 
            }
        }

        
    }

    public void ReadLocalMapTable() {
        // 조건 없이 모든 데이터 조회하기
        var bro = Backend.GameData.GetMyData("LocalSavedMap_List", new Where(), 100);
        if (bro.IsSuccess() == false)
        {
            // 요청 실패 처리
            DebugX.Log(bro);
            return;
        }
        if (bro.GetReturnValuetoJSON()["rows"].Count <= 0)
        {
            // 요청이 성공해도 where 조건에 부합하는 데이터가 없을 수 있기 때문에
            // 데이터가 존재하는지 확인
            // 위와 같은 new Where() 조건의 경우 테이블에 row가 하나도 없으면 Count가 0 이하 일 수 있다.
            DebugX.Log(bro);
            return;
        }

        rowCount = bro.Rows().Count;
        DebugX.Log("rowCount: " + rowCount);

        // 검색한 데이터의 모든 row의 inDate 값 확인
        for(int i=0; i<bro.Rows().Count; ++i)
        {
            MapInformation map = new MapInformation (
                bro.Rows()[i]["MapName"]["S"].ToString(),
                bro.Rows()[i]["inDate"]["S"].ToString(),
                bro.Rows()[i]["MadeBy"]["S"].ToString(),
                int.Parse(bro.Rows()[i]["PlayCount"]["N"].ToString()),
                int.Parse(bro.Rows()[i]["Like"]["N"].ToString()),
                bro.Rows()[i]["MapInfo"]["S"].ToString(),
                bro.Rows()[i]["owner_inDate"]["S"].ToString(),
                bro.Rows()[i]["inDate"]["S"].ToString()
                
            );

            localMapInfos.Add(map); 
        }
    }

    public void CheckBlockedUser() {
        List<string> blockedUsers = BackendGameData.Instance.GetBlockedUsers();
        foreach(GameObject map in mapBtns) {
            if(blockedUsers.Contains(map.GetComponent<Button>().transform.GetChild(1).GetComponent<Text>().text)) {
                map.SetActive(false);
            }
            else {
                map.SetActive(true);
            }
        }
    }

    public void DeleteBlockedUser(string nickName) {
        BackendGameData.Instance.RemoveBlockedUser(nickName);
        CheckBlockedUser();
    }

    public void ReportMap(string mapName, string nickName) { // 맵 신고
        Param param = new Param();
        param.Add("MapName", mapName);
        param.Add("MadeBy", nickName);
        param.Add("ReportedBy", BackendGameData.Instance.GetUserNickName());

        string reason = "";
        foreach (Toggle toggle in reportToggle) { // 맵 신고 항목: 체크박스
            if(toggle.isOn == true) {
                reason = reason + toggle.transform.GetChild(1).GetComponent<Text>().text + "\n";
            }
        }
        param.Add("Reason", reason);
        param.Add("Explain", explain.text); // 신고 사유

        Backend.GameData.Insert ("ReportedMap_List", param);

        mapReportBtn.interactable = false;
        
    }

    public void CreateNewMap() {
        DataController.Instance.gameData.playMode = PlayMode.MAPEDITOR;
        currentCustomMap.GetComponent<CurrentCustomMap>().currentMapName = "";

        // Create Map 들어갔을 때 tryAnythingonHomeGrid = false 시키기
        DataController.Instance.gameData.tryAnythingonHomeGrid = false;

        DataController.Instance.gameData.isNowCreateMap = true;

        DebugX.Log("CreateNewMap tryAnything ~" + DataController.Instance.gameData.tryAnythingonHomeGrid);
    }

    public void OnToggleClicked() { // 맵 신고할 때, 항목이 선택됐을 때만 Inputfield가 활성화됨
        if(reportToggle[0].isOn == false && reportToggle[1].isOn == false && reportToggle[2].isOn == false) {
            explain.interactable = false;
        }
        else {
            explain.interactable = true;
        }
    }

    public void OnEditingInputField() {
        if(explain.text.Length >= 10) {
            realMapReportBtn.interactable = true;
        }
        else {
            realMapReportBtn.interactable = false;
        }
    }

    public void SearchMap(InputField searchField) {
        if(searchField.text == null) {
            return;
        }
        savedMapBtn.GetComponent<Toggle>().isOn = false;
        DeleteAllMapBtns();

        int index = 0;
        foreach(MapInformation map in mapInfos) {
            if(map.madeBy.ToLower().Contains(searchField.text.ToLower()) || map.mapName.ToLower().Contains(searchField.text.ToLower())) {
                index++;
                GameObject go = Instantiate(mapBtn);
                go.transform.SetParent(GameObject.Find("Content").transform);
                go.transform.localScale= new Vector3(1,1,1);
                go.GetComponent<Image>().sprite = RandomImg[index%4];
                go.GetComponent<Button>().transform.GetChild(0).GetComponent<Text>().text = map.mapName;
                go.GetComponent<Button>().transform.GetChild(1).GetComponent<Text>().text = map.madeBy;
                go.GetComponent<Button>().transform.GetChild(2).GetComponent<Text>().text = "Played: " + map.playCount.ToString();
                go.GetComponent<Button>().transform.GetChild(3).GetComponent<Text>().text = "Like: " + map.like.ToString();
                mapBtns.Add(go);
                go.GetComponent<Button>().onClick.AddListener(OpenMapInfo);
                go.GetComponent<Button>().onClick.AddListener(delegate{MemoryPrevSelectedObject(go);});
                
            }
            
        }
        CheckBlockedUser();
    }

    public void OnOrderToggleClicked(Toggle orderToggle) {
        if(orderToggle.isOn == true) {
            if(mapToggle[1].isOn == true) {
                SortMapByPlayCount(true);
            }
            else if(mapToggle[2].isOn == true) {
                SortMapByLikeCount(true);
            }
        }
        else {
            if(mapToggle[1].isOn == true) {
                SortMapByPlayCount(false);
                
            }
            else if(mapToggle[2].isOn == true) {
                SortMapByLikeCount(false);                
            }
        }
    }

    public void CloseAdsPanel() {
        currentCustomMap.GetComponent<CurrentCustomMap>().IsAdsPanelOpen(false);
    }

    public void SetPlayEditMode(bool state) {
        DataController.Instance.gameData.canOpenSetting = true;
        DataController.Instance.gameData.isNowCreateMap = !state;
        isOthers = state;
        currentCustomMap.GetComponent<CurrentCustomMap>().ChangeIsOthers(state);

        // Play / Edit에 따라 cursorHidingME를 true / false로 바꿔줘야함
        SetCursorHidingME(state);
    }

    public void UpdatePlayCount(string clickedMapName, string clickedMapMadeBy) {
        
        if(DataController.Instance.gameData.customMapPlayTicket <= 0) {
            return;
        }
        

         foreach(MapInformation map in mapInfos) {
            if(map.mapName.Equals(clickedMapName) && map.madeBy.Equals(clickedMapMadeBy)) {
                map.playCount++;
                DebugX.Log("맵 에디터 플레이카운트 ++ "+ map.playCount);

                Param param = new Param();
                param.Add("PlayCount", map.playCount);

                BackendReturnObject bro = null;
                        
                bro = Backend.GameData.UpdateV2("CustomMap_List", currentCustomMap.GetComponent<CurrentCustomMap>().currentMapinDate, currentCustomMap.GetComponent<CurrentCustomMap>().currentMapOwnerinDate, param);
                        
                if (bro.IsSuccess()) {
                    DebugX.Log("플레이카운트 업데이트에 성공했습니다. : " + bro);
                }
                else {
                    DebugX.LogError("플레이카운트 업데이트에 실패했습니다. : " + bro);
                }
            }
        }
    }

    public void LocalSaveMapHandler() {
        userReportBtn.gameObject.SetActive(false);
        mapReportBtn.gameObject.SetActive(false);
        playBtn.gameObject.SetActive(false);
        editBtn.gameObject.SetActive(true);
        editBtn.GetComponent<Button>().Select();
        deleteBtn.gameObject.SetActive(true);
    }

    public void DeleteSavedMap() {
        BackendReturnObject bro = null;

        Where where = new Where();
        where.Equal("MapName", currentCustomMap.GetComponent<CurrentCustomMap>().currentMapName);
        where.Equal("MadeBy", currentCustomMap.GetComponent<CurrentCustomMap>().currentMapMadeBy);
        
        bro = Backend.GameData.Delete("LocalSavedMap_List", where);

        if (bro.IsSuccess()) {
            DebugX.Log("로컬 세이브 맵 삭제에 성공했습니다. : " + bro);
            
            foreach(MapInformation map in localMapInfos) {
                if(map.mapName == mapName.text && map.madeBy == mapMadeBy.text) {
                    localMapInfos.Remove(map);
                    DebugX.Log("로컬 리스트에서 삭제됨");
                    break;
                }
            }
            SortLocalMap();
            
        }
        else {
            DebugX.LogError("로컬 세이브 맵 삭제에 실패했습니다. : " + bro);
        }
    }

    public void OpenDeletePanel() {
        deletePanel.SetActive(true);
        currentCustomMap.GetComponent<CurrentCustomMap>().IsAdsPanelOpen(true);
    }

    // Play / Edit에 따라 cursorHidingME를 true / false로 바꿔줘야함
    public void SetCursorHidingME(bool state) {
        if(cursorController == null) {
            cursorController = GameObject.Find("CursorController").GetComponent<CursorController>();
        }
        
        cursorController.isCursorHidingME = state;
    }
        
}
