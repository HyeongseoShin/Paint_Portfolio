/*
인게임 전반에서 사용하는 데이터 모음
DataController에 의해 Load / Save됨
*/

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Cinemachine.Examples;

public enum PlayMode {IDLE, STORY, CUTSCENE, MAPEDITOR, HOME}
public enum RewardCategory {Hat, Glasses, Mask, Title, Tile}

[Serializable]
public class GameData
{
    // 23-12-19: ClearTimeList -> RatingValueList로 이름 변경
    // {플레이 타임, 다시하기 횟수, 뒤로가기 횟수} 포함
    public List<RatingValues> ratingValueList = new List<RatingValues>();

    public int buttonMode = 0;
    public bool bgmSound = true;
    public bool effectSound = true;
    public int chapter = 1;
    public int currentStage = 0;
    public bool fromStage;
    public bool isFix = false;
    public float bgmVolume = 0.5f;
    public float sfxVolume = 0.5f;
    public float uiVolume = 0.5f;
    public bool isHardMode = false;
    public string lastVisitedTime = DateTime.Now.ToString();
    public int retryCount = 5;

    public bool isMapEditorPreview = false;

    public bool isEasy = true;

    //2023-02-20~ 플레이어 정보
    public int ticket = 10; // 입장권 개수
    public int money = 0; // 돈
    public int cash = 0; // 캐쉬 (보석)
    public int hintCount = 0; // 힌트권 개수
    public int customMapUploadTicket = 5; // 커스텀 맵 업로드권
    public int customMapPlayTicket = 5; // 커스텀 맵 플레이 입장권

    //2023-05-22 첫 로그인 시간 가져오기
    public string firstVisitedTimeForIndiecraft;

    //2023-05-23 Language Settings 값 지정
    public int language = -1;

    //2023-07-11 게임 시작 후 home grid new Assets에서 시작 카메라 애니메이션 없애기.
    //Stage_intro에서 GoogleAdsController => checkNetworking.cs에서 false로 바꿔줌. 카메라 애니메이션 이후 true로 바뀜.
    public bool startGame =false;

    public bool firstInHomeGrid =false; // 처음 homegrid에 입장할 때 (껐다가 켰을 때)
    public bool isSplashFinish = false; // 처음 입장해서 트랜지션 끝났나요?에 대한 변수.
 
    public PlayMode playMode = PlayMode.IDLE;

    public int screen_Width = 1920;
    public int screen_Height = 1080;
    public bool isFullScreen = true;
    public Edit_PlayController epOnGameData;
    public DeskSceneController deskController;

    public bool playerCanMove = false;

    public bool isTutorialFinished = false;

    public bool isSettingPanelOn = false;

    // 24-12-11 커스텀 로그인 (게스트 포함) / 스팀 로그인 / 스토브인디 로그인 구분 위한 변수
    public int loginType = -1;

    /*
    2023-08-29 
    stage 버튼을 클릭을 했을 때 true로 바꿔준다.
    만약 true라면 chapter map에서 esc 및 chapter 선택이 되지 않도록 한다.
    startonhomeEditor.cs에서 시작 때 반드시 tryEnterStage가 매번 fasle가 되도록 한다.

    */
    public bool tryEnterStage = false;

    // 2023-09-11 스토리 대화를 위한 구조체 작성
    // public List<Dialog> dialogList_Chapter = new List<Dialog>();
    public List<Dialog> dialogList_AfterStage = new List<Dialog>();

    public List<InGameDialog> dialogList_InGame = new List<InGameDialog>();

    // 2023-09-19 Reward Gimmick 얻었을 때 필요한 변수
    public bool tryGetGimmick = false;

    // 2023-09-20 HomeGrid_newAssets에서 gameobject와 상호작용(맵 에디터, 마이룸, 플레이, 세팅)하는 것과 버튼으로 눌러서하는 것의 중복을 막기 위한 변수 추가.
    public bool tryAnythingonHomeGrid = false;
    /*
        StartOnHomeEditor 에서 awake 에서 무조건 false로 만들어 줌.

        1. 맵 에디터.
            StartOnHomeEditor 에서 GotoMapEditorSceneWithDelay 에 true로 만들어 줌.
            GotoMapEditorSceneWithDelay 같은 경우에 맵 에디터 씬으로 이동하기 때문에 어차피 다시 돌아오면 false로 초기화가 된다.

        2. 마이룸.
            StartOnHomeEditor 에서 StartToMapEditor 에 true로 만들어 줌.
            Edit_PlayController 에서 SetEditingMode 에 false로 만들어 줌. (마이룸 편집이 끝나고 다시 시작버튼을 눌렀을 때)

        3. 플레이 
            StartOnHomeEditor 에서 OpenPlayPannelonME 에 if 조건.  true로는 안 만들어 줌.
            StartOnHomeEditor 에서 CloseMyRoomPanelonME 에 if 조건.  true로는 안 만들어 줌.
            GameManager 에서 SetObjMovement 에 if 조건.  true로는 안 만들어 줌.
            ingamePlayer 에서 SetPlayerUIPopOn 에 if 조건.  true로는 안 만들어 줌.
            ingamePlayer 에서 SetMenuIsOn 에 true로 만들어줌.

            StartOnHomeEditor 에서 ClosePlayPannelonME 에 false로 만들어 줌. 

        4. 세팅
            StartOnHomeEditor 에서 OpenSettingPanelonME 에 if 조건.  true로는 안 만들어 줌.
            GameManager 에서 SetObjMovement 에 if 조건.  true로는 안 만들어 줌.
            ingamePlayer 에서 SetPlayerUIPopOn 에 if 조건.  true로는 안 만들어 줌.
            ingamePlayer 에서 SetMenuIsOn 에 true로 만들어줌.

            StartOnHomeEditor 에서 CloseSettingPanelonME 에 false로 만들어 줌. 
    */

    public bool canOpenSetting = false;
    /*
        
        autoflip에서 target과 current가 같으면 true로 (Update에서 true로 만들어주는 줄 있음.)
        startonhomeeditor에서 시작할 때 true로
        items script에서 door에서 진행하면 false로
        deskscenecontroller에서 시작할때 false로
        stage btn을 누르고 갔을 때도 그때부터는 menu버튼이 나오지 않도록 chapterstageController에서 loadstage에 false로 만들어줌.
        ingameplayer에서 Menu 부분에서 조건으로 하나 더 들어갈거임.
    */

    public float mainMenuTime = 0; //mainMenu에서 보내는 시간 (여기에는 homeEditor 시간도 포함되어 있다)
    public float homeEditorTime = 0; //mainMenu 중 homeEditor 시간.
    public bool openFirstonMainmenu = false;
    public bool openFirstonStageMap = false;

    public bool isOpeningChapterBook; // ChapterBook이 열리는 중 방향키를 적용 안하기 위해 변수 추가


    /*
        2023-12-11 1-5 클리어 이후에만 HomeEditor 기능을 얻을 수 있도록 하며, HomeEditor를 한 번이라도 사용해본 사람만이 창작마당을 사용할 수 있도록 한다.
    */
    public bool canUseHomeEditor = false; //HomeEditor를 사용할 수 있나요? 
    //itemScript에 start함수에 들어가있다.
    //PreventDoubleClick.cs 에서 interactable을 막는 부분이 들어있다.

    public bool isUseHomeEditorOneTime = false; //HomeEditor를 한 번 이상 사용했나요? => True로 되었을 때 창작마당을 사용할 수 있다는 것과 같은 의미. 이를 이용해 HomeEditor 하이라이트를 조정한다.
    //itemScript에 start함수에 들어가있다.
    //PreventDoubleClick.cs 에서 interactable을 막는 부분이 들어있다.
    
    public bool isUseGridNewOneTime = false; //창작마당을 한 번 이상 들어가봤다. 이를 이용해 창작마당 하이라이트를 조정한다.

    public bool isUseplaydoor = false; //playdoor를 한 번 이상 들어가봤다.

    //allstageClear => 각 chapter에 있는 stage를 모두 클리어했을 경우. true 가 된다.
    public bool[] allstageClear = {false,false,false,false,false,false,false,false,false,false,false,false,false,false};

    /*
        2023-12-12 Chapter가 열렸는지 확인하기 위한 변수
    */
    public bool[] isOpenChapterCheck = {true,false,false,false,false,false,false,false,false,false,false,false,false,false};
    //SetUIonChapterBook.cs 에 있는 CheckOpenChapter()에 있음.

    /*
        2024-01-04 기존의 MyRoomPanel이 초기 위치가 달라짐에 따라 StageMapEditor에서 왔을 떄를 추적하기 위한 변수
    */
    public bool isFromMapEditor;

    /*
        2024-01-11 새로운 힌트 방식 확정에 따른 힌트 구조체
    */
    public List<int> hintList = new List<int>();

    /*
        2024-01-25 레이저에 맞고 있을 때 뒤로하기를 누르면 레이저 색상으로 플레이어 색상이 바뀌는 것을 방지하기 위한 변수 추가.
    */
    public bool canLazerInteract = true;

    /*
        2024-04-17 OYJ 업적 보상을 위한 추가 변수
    */

    public Dictionary<string, List<AchieveGeneralEachInfo>> achGeneralInfo = new Dictionary<string, List<AchieveGeneralEachInfo>>();
    public Dictionary<string, List<int>> achGeneralCheckValInfo = new Dictionary<string, List<int>>();
    //key 값과 value 값만 저장된 딕셔너리.

    
    // public List<string> achieveOrderList = new List<string>();
    /*
        "GRBC" GoRightBefore_Count 뒤로가기 횟수
        "HEPC" HomeEditorPlay_Count HomeEditor 사용 횟수 (버튼이나 상호작용 할 때 오른다)
        "CMPC" CustomMapPlay_Count Editor를 통해서 만들어진 CustomMap을 클리어앴을 때.
        "MMPT" MainMenuPlay_Time HomeGrid_NewAssets에서 보낸 총 시간
        "HEPT" HomeEditorPlay_Time HomeGrid_NewAssets에서 Edit 한 총 시간
        "RC" Retry_Count Homegrid, grid_new, homeplaygrid 등 모든 곳에서 다시하기 했을 때.
        "CMUC" CustomMapUpload_Count Editor를 통해서 CustomMap을 올렸을 때.
        "CMPT" CustomMapPlay_Time Grid_NewAssets에서 보낸 총 시간(플레이 하는 시간)
        "CMET" CustomMapEdit_Time Grid_NewAssets에서 보낸 총 시간(Edit 하는 시간)
        "SMPT" StoryMapPlay_Time HomePlayGrid_에서 보낸 총 시간 (플레이 하는 시간)
        "TSPT" TotalSumPlay_Time 메인, 커스텀 플레이, 커스텀 에디트, 스토리 플레이 합친 시간
        "TF" Tutorial_Finish 튜토리얼이 끝났을 때 뜬다.
            아래 In__ 이렇게 되어있는 변수들은 각 맵에서만 횟수 카운트를 하기 때문에 DataController에 따로 저장이 될 필요가 없다. 
        "InSMGRBC" In_StoryMap_GoRightBefore_Count 한 스토리 맵의 스테이지에서 발생하는 뒤로가기 횟수
        "InCMGRBC" In_CustomMap_GoRightBefore_Count 한 커스텀(유저) 맵의 스테이지에서 발생하는 뒤로가기 횟수
        "InSMRC" In_StoryMap_Retry_Count 한 스토리 맵의 스테이지에서 발생하는 다시하기 횟수
        "InCMRC" In_CustomMap_Retry_Count 한 커스텀(유저) 맵의 스테이지에서 발생하는 다시하기 횟수

        "TSCC" TotalStageClear_Count 스토리 맵에서 각 stage를 처음 클리어하면, 이 변수값이 올라간다.
        "TSSC" TotalStageSkip_Count 각 chapter 별로 한 번씩만 실행이 가능하며 9 chapter까지만 가능하다. 모두 깬 사람은 얻지 못한다.
        "TSBSC" TotalStageBadSkip_Count 각 chapter 별로 한 번씩만 실행이 가능하며 9 chapter까지만 가능하다. 모두 깬 사람은 얻지 못한다.
        "ASC" AllStageClear 각 chapter 별 모든 stage를 클리어했는지 확인한다.
            위 ASC 같은 경우는 GameData에 따로 변수가 저장되어있지 않다. BackendPlayerManager에 clear 정보가 모두 있기 때문이다.
            혹 필요하다면, GameData에 있는 allstageClear를 참고할 수 있다. 하지만 플로우 상 순서가 안 맞을 수 있으니 참고용으로만 사용해야 한다.
            itemScript.cs의 CustomMapPlayCleared()에서 함수 호출을 한다.
        여기까지 업적 20개

        "GAB" GetAwardButton Editor 보상 자체가 한 chapter를 끝냈을 때 얻을 수 있는 것이다. 하지만 chapter all clear 했을 때는 stage에서 문에 갔을 때 바로 나오는 것이고 이건 보상 버튼을 클릭했을 때 나오는 것이다.

    */
    //acheiveOrderList 에는 얻은 업적 순서대로 들어올 예정이다.
    //예를 들어, 순서대로 tutorial, GRB, RertryCnt 이렇게 들어오면 아래 변수인 achieveEachInfo 에서 index를 따오기 위해 따로 변수를 저장한다.

    // public List<AchieveList> achieveEachInfo = new List<AchieveList>();
    //List로 해둔 이유.
    // GRB Cnt 같은 경우 100, 200, 300, 400 이렇게 GRB Cnt로 4개의 업적이 있다고 해보자
    // 하지만 Retry Cnt 같은 경우 10, 50, 100 으로 관련된 업적의 개수가 3개로 위와 다를 가능성이 있다.
    //따라서 array가 아닌 List로 진행하도록 한다.

    public GenerateAchievementBanner gameDataAchievementObject;


    /*
    public int totalGorightBeforeCount = 0; 
    //GoRightBefore.cs 에서 PopState() 에 뒤로가기를 누를 때마다 gbCount++가 되는데, totalGorightbeforeCount도 동일한 때에 올라가도록 한다.

    public int totalHomeEditorPlayCount = 0; 
    //StartOnHomeEditor.cs 에서 StartToMapEditor()에서 ++ 된다.

    public int totalCustomMapPlayCount = 0; 
    //Scene_Manager.cs 에서 UploadCustomPlayTime()에서 ++ 된다.

    
    public int totalRetryCount = 0; 
    //Edit_PlayController.cs 에서 Reload()에서 ++ 된다.

    public int totalCustomMapUploadCount = 0; 
    //MapNameController.cs 에서 SaveNewMap()에서 ++ 된다.

    public int totalStageClearCount = 0; 
    //itemScripts.cs 에서 CustomMapPlayCleared()에서 ++ 된다.

    public int totalStageSkipCount = 0; 
    //GoNextScene.cs 에서 GoFirst()에서 ++ 된다.

    public int totalStageBadSkipCount = 0; 
    //GoNextScene.cs 에서 GoFirst()에서 ++ 된다.

    */

    //mainMenuTime:
    //HomeGrid_NewAssets Scene인 Main Menu에서 보낸 시간 => 위쪽에서 mainMenuTime이 있음. 전체 타임이라고 생각하면되고, HomeEditor 시간도 포함되어있다.
    //원래 있어서 따로 변수 추가 하지 않았음.
    //StartOnHomeEditor에서 측정한다.
        //Awake에서 temp 값에 저장 (DataController에 접근하는 횟수를 줄이기 위해서)
        //코루틴을 통해서 60초마다 체크하는 것으로 한다. 하지만 테스트 때는 1초로 줄여놓는다.

    public int[] mainMenuAchieveValList = {20,30, 40, 50, 90};
    //main menu 업적 시간 리스트 (float 값을 측정해야 하는 변수들은 모두 Gamedata에 list화 해둔다.)

    //homeEditorTime:
    //HomeGrid_NewAssets Scene인 Main Menu > HomeEditor에서 보낸 시간 => 위쪽에서 homeEditorTime이 있음. 전체 타임이라고 생각하면 된다.
    //원래 있어서 따로 변수 추가 하지 않았음.
    //StartOnHomeEditor에서 측정한다.
        //위와 같다.

    public int[] homeEditorAchieveValList = {5,10, 15, 70, 90};
    //homeEditor 업적 시간 리스트
    
    // public float totalCustomMapPlayTime = 0f;
    //Scene_Manager.cs 에서 Update에 ++ 된다.

    public int[] customMapPlayTimeAchieveValList ={};
    //custommapPlayTime 업적 시간 리스트.

    // public float totalCustomMapEditTime = 0f;
    //Scene_Manager.cs 에서 Update에 ++ 된다.

    public int[] customMapEditTimeAchieveValList ={};
    //custommapEditTime 업적 시간 리스트.

    // public float totalStoryMapPlayTime = 0f;
    //GoRightBefore.cs 에서 Update에 ++ 된다.

    public int[] storyMapPlayTimeAchieveValList ={};
    //custommapEditTime 업적 시간 리스트.

    // public float totalSumPlayTime = 0f; 
    // Gorightbefore.cs, Scene_Manager.cs, StartOnHomeEditor.cs에서 측정한다.
    // 따로 시간을 ++를 하는 것은 아니고, 각 스크립트에서 DataController.Instance.CheckAchievementOnDC(" ", __) 을 진행할 때 같이 한다.
    // 측정하는 것들 중 mainMenuTime, totalCustomMapPlaytime, totalCustomMapEditTime, totalStoryMapPlaytime의 합을 전체 시간이라고 판단한다.
    // 각각의 변수들은 동시에 증가하는 경우가 없다.

    public int[] totalSumPlayTimeAchieveValList ={};

    public int achDetailSceneIndex = 0;
    /*
        2024-02-02 OYJ
        0 => play 업적
        1 => play 외 업적
        2 => 꾸미기 업적
    */

    public List<string> playAchKeyList = new List<string>();
    public List<string> exceptPlayAchKeyList = new List<string>();
    public List<string> decorateAchKeyList = new List<string>();
    public List<string> otherAchKeyList = new List<string>();
    /*
        2024-02-06 OYJ 
        각 카테고리들에 어떤 key 값들이 있는지 분류해둔다.
    */

    public int[] totalAchieveCountEachCategory = new int[4];
    /*
        2024-02-06 OYJ
        0 => "Except_Play"
        1 => "Play"
        2 => "Decorate"
        3 => "Others"
    */


    // 2024-01-19 색 보정 기능 On / Off를 위해 변수 추가
    public bool isColorFilterAssistant = false; 

    // 2024-04-16 OYJ Grid_NewAssets에서 Create 상태 확인
    public bool isNowCreateMap = false; //(Stage_MapEditor) PlayerInfoManager.cs에서 start에서 false 만들어주고, MapEditorManager.cs에서 true로 만듬.



    // 2024-04-26 OYJ Book Pro 에서 버튼이 활성화 되는 애니메이션 실행하는 것을 확인하기 위해서 생성.
    public bool[] isFirstOpenChapter = {true,false,false,false,false,false,false,false,false,false,false,false,false,false};

        // 2024-05-13 플레이어 치장 아이템 설정
    public GameObject currentGlassesPrefabs = null;
    public GameObject currentHatPrefabs = null ;
    public GameObject currentMaskPrefabs = null ;

    public int currentGlassesIndex = -1;
    public int currentHatIndex = -1;

    public int currentMaskIndex = -1;

    public bool canChangeSkin = false;

    // 2024-05-21 OYJ 업적에 따른 받을 수 있는 보상 리스트업 하기.
    public Dictionary<int, List<RewardItemEachInfo>> rewardItemGeneralInfo = new Dictionary<int, List<RewardItemEachInfo>>();

    // 2024-05-21 OYJ 업적 보상 카테고리에 따른 획득 정보
    public List<int> hatRewardIndexList = new List<int>();
    public List<int> glassesRewardIndexList = new List<int>();
    public List<int> maskRewardIndexList = new List<int>();
    public List<int> titleRewardIndexList = new List<int>();
    public List<int> tileRewardIndexList = new List<int>();


    //24-05-17 챕터 컷씬 끝났는지 확인하는 변수 
    public bool isChapterCutSceneFinished = false;

    //24-05-28 초보자 용 설정 (전체 화면)
    public bool isFullviewForNoob = false;

    //24-05-31 새기능 잠금해제 팝업 UI 한 번 뜨고 나면 다음에는 안 뜨도록
    public bool isHomeEditorUnlockUIPopUp = false;
    public bool isLevelEditorUnlockUIPopUp = false;

    // 24-06-04 플레이어의 조작 타입 => 0: 양손 조작, 1: 왼손 조작, 2: 오른손 조작
    public int controlType = 0;

    // 24-06-11 OYJ achieve_info csv의 순서대로 저장된 최소한의 행 정보들.
    public List<AchieveCSVOrder> achieveCSVinOrder = new List<AchieveCSVOrder>();

    // 24-07-18 OYJ 지도에서 다음 chapter 이동하기 상황 구분
    public bool clickNextChapterOnTempdeskScene = false;

    //24-08-26 새로운 힌트 정보 담을 구조체 <객체 순서, 화살표 색상>
    public List<HintInfo> hintInfoList = new List<HintInfo>();
    
}

// 24-08-26 새로운 힌트 정보 담을 class
[SerializeField]
public class HintInfo
{
    List<string> orderInfo = new List<string>(); // 객체 순서
    List<string> colorInfo = new List<string>(); // 화살표 색상
}


// 23-12-19: ClearTime -> RatingVluaes로 이름 변경
// {플레이 타임, 다시하기 횟수, 뒤로가기 횟수} 포함
[SerializeField]
public class RatingValues {
    public List<float> ct = new List<float>(); // 플레이 타임
    public List<int> grb = new List<int>(); // 뒤로가기 횟수
    public List<int> restart = new List<int>(); // 다시하기 횟수
}

[SerializeField]
public class Dialog {
    public List<DialogStages> dialogList_Stage = new List<DialogStages>();
}

[SerializeField]
public class DialogStages {
    public List<string> dl_ko = new List<string>();
    public List<string> dl_en = new List<string>();
    public List<string> npc_ko = new List<string>();
    public List<string> npc_en = new List<string>();
    public List<string> npcFilePath = new List<string>();
}

[SerializeField]
public class InGameDialog {
    public string key;
    public int chapter;
    public int stage;
    public string npc_FilePath;
    public string font;

    public InGameDialog(string _key, int _chapter, int _stage, string _npc_FilePath, string _font) {
        key = _key;
        chapter = _chapter;
        stage = _stage;
        npc_FilePath = _npc_FilePath;
        font = _font;
    }
}

/*
    2024-01-18 OYJ 업적 기능을 위한 구조체
*/
[Serializable]
public class AchieveList {
    public List<bool> alist = new List<bool>();
}

[SerializeField]
public class AchieveGeneralEachInfo { // csv 에서 받아올 거고, value 값, 설명 등이 저장될 것이다.
    public int value;
    public string key;
    public string Kor_Contents;
    public string Eng_Contents;
    public string Jpn_Contents;

    public string Kor_Title;
    public string Eng_Title;
    public string Jpn_Title;
    
    public string Kor_prevAch;
    public string Eng_prevAch;
    public string Jpn_prevAch;

    public string SpritePath;

    public string Category;
    public int Reward_Key;
    
    public AchieveGeneralEachInfo(int _value,string _key, string _Kor_Contents, string _Eng_Contents, string _Jpn_Contents, string _Kor_Title, string _Eng_Title, string _Jpn_Title, string _Kor_prevAch, string _Eng_prevAch, string _Jpn_prevAch, string _SpritePath, string _Category, int _Reward_Key) {
        value = _value;
        key = _key;

        Kor_Contents = _Kor_Contents;
        Eng_Contents = _Eng_Contents;
        Jpn_Contents = _Jpn_Contents;

        Kor_Title = _Kor_Title;
        Eng_Title = _Eng_Title;
        Jpn_Title = _Jpn_Title;

        Kor_prevAch = _Kor_prevAch;
        Eng_prevAch = _Eng_prevAch;
        Jpn_prevAch = _Jpn_prevAch;

        SpritePath = _SpritePath;
        Category = _Category;
        Reward_Key = _Reward_Key;
    }
}

[SerializeField]
public class RewardItemEachInfo { // reward_Info.csv에서 받아올 것.
    public RewardCategory rewardCategory;
    public int itemIndex;
    public RewardItemEachInfo(RewardCategory _rewardCategory, int _itemIndex) {
        rewardCategory = _rewardCategory;
        itemIndex = _itemIndex;
    }
}

[Serializable]
public class AchieveCSVOrder{
    public string cateKey;
    public int standardCnt; //기준점. 만약 값이 15라면 cateKey에 해당하는 카테고리에서 15의 index를 구한다.
    // index를 구했으면 achGenerInfo에서 같은 index 값을 구하면 된다. 

    public AchieveCSVOrder(string _cateKey, int _standardCnt){
        cateKey = _cateKey;
        standardCnt = _standardCnt;
    }
}