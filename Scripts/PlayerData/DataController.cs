/*
인게임 내에서 전반적으로 사용하는 GameData의 Load / Save 기능을 담당하는 스크립트
- LoadGameData()
- SaveGameData()

AES 이용해 로컬 Json 파일 Save 시 암호화 & Load 시 복호화
- Encrypt()
- Decrypt()
- CreateRijndaelManaged()

GameData 내에 있는 전체 업적 List를 활용해 업적이 달성되면 GenerateAchieveBanner 스크립트에 전달하는 기능도 존재
- SetGenAchievementOnDataController() - 우상단 업적 달성 배너 객체 등장
- SetAchintValueOnDC() - 전체 업적 정보 List 데이터 Init
- CheckAchievementOnDC() - 해당 업적이 달성되었는지 조건 확인
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Security.Cryptography;
using Steamworks;

public class DataController : MonoBehaviour
{
    private static readonly string privateKey;
    

    // 싱글톤 ============
    static GameObject _container;
    static GameObject Container
    {
        get
        {
            return _container;
        }
    }

    static DataController _instance;
    public static DataController Instance
    {
        get
        {
            if(!_instance)
            {
                _container = new GameObject();
                _container.name = "DataController";
                _instance = _container.AddComponent(typeof(DataController)) as DataController;

                DontDestroyOnLoad(_container);
            }
            return _instance;
        }
    }

    // 게임 데이터 파일이름 설정
    private string GameDataFileName = ".pa";

    // "원하는 이름(영문).json"
    public GameData _gameData;
    public GameData gameData
    {
        get
        {
            // 게임이 시작되면 자동으로 실행되도록
            if(_gameData == null)
            {
                LoadGameData();
            }
            return _gameData;
        }
    }

    private GenerateAchievementBanner generateAchievementBanner;

    // [Header("Acheivement")]
    //value 값들이 int인 것들
    private List<int> grbcList = new List<int>();
    private List<int> hepcList = new List<int>();
    private List<int> cmpcList = new List<int>();
    private List<int> rcList = new List<int>();
    private List<int> cmucList = new List<int>();
    private List<int> tfList = new List<int>();
    private List<int> insmgrbcList = new List<int>();
    private List<int> incmgrbcList = new List<int>();
    private List<int> insmrcList = new List<int>();
    private List<int> incmrcList = new List<int>();
    private List<int> tsccList = new List<int>();
    private List<int> tsscList = new List<int>();
    private List<int> tsbscList = new List<int>();
    private List<int> ascList = new List<int>();
    private List<int> gabList = new List<int>();

    [SerializeField]
    private int[] grbcArray;
    [SerializeField]
    private int[] hepcArray;
    [SerializeField]
    private int[] cmpcArray;
    [SerializeField]
    private int[] rcArray;
    [SerializeField]
    private int[] cmucArray;
    private int[] tfArray;
    private int[] insmgrbcArray;
    private int[] incmgrbcArray;
    private int[] insmrcArray;
    private int[] incmrcArray;
    private int[] tsccArray;
    private int[] tsscArray;
    private int[] tsbscArray;

    [SerializeField]
    private int[] ascArray;
    [SerializeField]
    private int[] gabArray;

    public void LoadGameData(string filePath, int _loginType)
    {
        // 저장된 게임 있으면
        if(File.Exists(filePath))
        {   
            //DebugX.Log("스크립트값: " + DataController.Instance.gameData.totalStagesInfo.Length);
            // DebugX.Log("불러오기 성공");
            string FromJsonData = File.ReadAllText(filePath);

            // 데이터 복호화
            string decryptData = Decrypt(FromJsonData);

            if(decryptData != null) {
                // 복호화된 데이터 불러오기
                //DebugX.Log(decryptData);
                _gameData = JsonUtility.FromJson<GameData>(decryptData);
                // DebugX.Log("암호화된 데이터");
                
            }
            else {
                //DebugX.Log(FromJsonData);
                _gameData = JsonUtility.FromJson<GameData>(FromJsonData);
                // DebugX.Log("그냥 데이터");
            }

        }

        // 저장된 게임 없으면
        else
        {
            // DebugX.Log("새로운 파일 생성");
            _gameData = new GameData();
            
        }

        _gameData.loginType = _loginType;
    }
    
    // 로드
    public void LoadGameData()
    {
        string filePath = Application.persistentDataPath + "/SaveData" + GameDataFileName;
        
        // 저장된 게임 있으면
        if(File.Exists(filePath))
        {   
            string FromJsonData = File.ReadAllText(filePath);

            // 데이터 복호화
            string decryptData = Decrypt(FromJsonData);

            if(decryptData != null) {
                // 복호화된 데이터 불러오기
                //DebugX.Log(decryptData);
                _gameData = JsonUtility.FromJson<GameData>(decryptData);
                // DebugX.Log("암호화된 데이터");
                
            }
            else {
                //DebugX.Log(FromJsonData);
                _gameData = JsonUtility.FromJson<GameData>(FromJsonData);
                // DebugX.Log("그냥 데이터");
            }
        }

        // 저장된 게임 없으면
        else
        {
            // DebugX.Log("새로운 파일 생성");
            _gameData = new GameData();
        }
    }

    // 세이브
    public void SaveGameData()
    {
        string ToJsonData = JsonUtility.ToJson(gameData);
        string filePath = Application.persistentDataPath + "/SaveData"+GameDataFileName;

        //ToJsonData 암호화
        string encryptString = Encrypt(ToJsonData);

        // 파일 이미 존재하면 덮어쓰기
        // File.WriteAllText(filePath, ToJsonData);

        //암호화된 string 저장
        File.WriteAllText(filePath, encryptString);

        //저장 되어있는지 확인
        DebugX.Log("저장완료");
    }

    

    private bool isActiveTrueAchivementBanner = false;
    public void SetGenAchievementOnDataController(GenerateAchievementBanner tempGenAchieve){
        generateAchievementBanner = tempGenAchieve;
        isActiveTrueAchivementBanner = true;
    }

    public void SetAchIntValueOnDC(){
        grbcList = DataController.Instance.gameData.achGeneralCheckValInfo["GRBC"];

        hepcList = DataController.Instance.gameData.achGeneralCheckValInfo["HEPC"];

        rcList = DataController.Instance.gameData.achGeneralCheckValInfo["RC"];

        tfList = DataController.Instance.gameData.achGeneralCheckValInfo["TF"];
        insmgrbcList = DataController.Instance.gameData.achGeneralCheckValInfo["InSMGRBC"];

        insmrcList = DataController.Instance.gameData.achGeneralCheckValInfo["InSMRC"];

        tsccList = DataController.Instance.gameData.achGeneralCheckValInfo["TSCC"];
        
        ascList = DataController.Instance.gameData.achGeneralCheckValInfo["ASC"];
        gabList = DataController.Instance.gameData.achGeneralCheckValInfo["GAB"];

        grbcArray = new int[grbcList.Count];
        hepcArray = new int[hepcList.Count];
        cmpcArray = new int[cmpcList.Count];
        rcArray = new int[rcList.Count];
        cmucArray = new int[cmucList.Count];
        tfArray = new int[tfList.Count];
        insmgrbcArray = new int[insmgrbcList.Count];
        incmgrbcArray = new int[incmgrbcList.Count];
        insmrcArray = new int[insmrcList.Count];
        incmrcArray = new int[incmrcList.Count];
        tsccArray = new int[tsccList.Count];
        tsscArray = new int[tsscList.Count];
        tsbscArray = new int[tsbscList.Count];
        ascArray = new int[ascList.Count];
        gabArray = new int[gabList.Count];

        grbcArray = grbcList.ToArray();
        hepcArray = hepcList.ToArray();
        cmpcArray = cmpcList.ToArray();
        rcArray = rcList.ToArray();
        cmucArray = cmucList.ToArray();
        tfArray = tfList.ToArray();
        insmgrbcArray = insmgrbcList.ToArray();
        incmgrbcArray = incmgrbcList.ToArray();
        insmrcArray = insmrcList.ToArray();
        incmrcArray = incmrcList.ToArray();
        tsccArray = tsccList.ToArray();
        tsscArray = tsscList.ToArray();
        tsbscArray = tsbscList.ToArray();
        ascArray = ascList.ToArray();
        gabArray = gabList.ToArray();
        
    }
    public void CheckAchievementOnDC(string tempKey){
        if(isActiveTrueAchivementBanner){
            switch (tempKey){
                case "GRBC":
                    generateAchievementBanner.ConfirmAchiveValue("GRBC", grbcArray, BackendAchManager.Instance.achData.totIntVal.tgbc);
                    return;
                case "HEPC":
                    generateAchievementBanner.ConfirmAchiveValue("HEPC", hepcArray, BackendAchManager.Instance.achData.totIntVal.thpc);
                    return;
                case "CMPC":
                    generateAchievementBanner.ConfirmAchiveValue("CMPC", cmpcArray, BackendAchManager.Instance.achData.totIntVal.tcpc);
                    return;
                case "RC":
                    generateAchievementBanner.ConfirmAchiveValue("RC", rcArray, BackendAchManager.Instance.achData.totIntVal.trc);
                    return;
                case "CMUC":
                    generateAchievementBanner.ConfirmAchiveValue("CMUC", cmucArray, BackendAchManager.Instance.achData.totIntVal.tcuc);
                    return;
                case "TF":
                    if(DataController.Instance.gameData.isTutorialFinished){
                        generateAchievementBanner.ConfirmAchiveValue("TF", tfArray, 1); //튜토리얼이 끝나면 한 번 밖에 체크를 안하니, 굳이 변수 추가하지 않고 1로 넣어준다.
                    }
                    return;
                case "TSCC":
                    generateAchievementBanner.ConfirmAchiveValue("TSCC", tsccArray, BackendAchManager.Instance.achData.totIntVal.tscc);
                    return;
                case "TSSC":
                    generateAchievementBanner.ConfirmAchiveValue("TSSC", tsscArray, BackendAchManager.Instance.achData.totIntVal.tssc);
                    return;
                case "TSBSC":
                    generateAchievementBanner.ConfirmAchiveValue("TSBSC", tsbscArray, BackendAchManager.Instance.achData.totIntVal.tsbsc);
                    return;
                default:
                    generateAchievementBanner.ConfirmAchiveValueForJustTriggerOne(tempKey); //업적 종류가 하나 밖에 없어서 업적 이름만 인자로 보낸다.
                    return;
            }
        }
        
    }

    public void CheckAchievementOnDC(string tempKey, int tempVal, bool tempBool){
        //측정값이 int인 변수들이지만 DataController.gameData에 저장할 필요 없는 변수들을 검사할 때.
        if(isActiveTrueAchivementBanner){
            switch (tempKey){
                case "InSMGRBC":
                    generateAchievementBanner.ConfirmAchiveValue("InSMGRBC", insmgrbcArray, tempVal);
                    //GoRightBefore.cs에서 체크한다.
                    return;
                case "InCMGRBC":
                    generateAchievementBanner.ConfirmAchiveValue("InCMGRBC", incmgrbcArray, tempVal);
                    //GoRightBefore.cs에서 체크한다.
                    return;
                case "InSMRC":
                    generateAchievementBanner.ConfirmAchiveValue("InSMRC", insmrcArray, tempVal);
                    //GoRightBefore.cs에서 체크한다.
                    return;
                case "InCMRC":
                    generateAchievementBanner.ConfirmAchiveValue("InCMRC", incmrcArray, tempVal);
                    //Edit_PlayController.cs에서 체크한다.
                    return;
                case "ASC":
                    generateAchievementBanner.ConfirmAchiveValueForAllClear("ASC", ascArray, tempVal);
                    return;
                case "GAB":
                    generateAchievementBanner.ConfirmAchiveValueForAllClear("GAB", gabArray, tempVal);
                    return;

                default:
                    return;
            }
        }
        
    }

    public void CheckAchievementOnDC(string tempKey, int tempVal){
        //위 함수는 int 값을 측정하는 함수. 이 함수는 float 변수 값을 측정하는 함수.
        //int 변수들과는 달리 float들은 먼저 index 위치를 각각의 스크립트에서 계산해서 보내준다.
        //따라서 아래와 같이 코드를 간단하게 바꾼다.
        
        if(isActiveTrueAchivementBanner){
            generateAchievementBanner.ConfirmAchiveValue(tempKey, tempVal);
            
        }
        
    }

    // 데이터 암호화
    public string Encrypt(string data)
    {
        byte[] bytes;
        try {
            bytes = System.Text.Encoding.UTF8.GetBytes(data);
        } catch(System.FormatException e) {
            DebugX.Log("Enctypt Exception" + e);
            return null;
        }
        
        RijndaelManaged rm = CreateRijndaelManaged();
        ICryptoTransform ct = rm.CreateEncryptor();
        byte[] results = ct.TransformFinalBlock(bytes, 0, bytes.Length);
        return System.Convert.ToBase64String(results, 0, results.Length);
 
    }
 
    // 데이터 복호화
    public string Decrypt(string data)
    { 
        byte[] bytes;
        try {
            bytes = System.Convert.FromBase64String(data);
        } catch(System.FormatException e) {
            DebugX.Log("Decrypt Exception" + e);
            return null;
        }

        RijndaelManaged rm = CreateRijndaelManaged();
        ICryptoTransform ct = rm.CreateDecryptor();
        byte[] resultArray = ct.TransformFinalBlock(bytes, 0, bytes.Length);
        return System.Text.Encoding.UTF8.GetString(resultArray);
    }
 
 
    private static RijndaelManaged CreateRijndaelManaged()
    {
        byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(privateKey);
        RijndaelManaged result = new RijndaelManaged();
 
        byte[] newKeysArray = new byte[16];
        System.Array.Copy(keyArray, 0, newKeysArray, 0, 16);
 
        result.Key = newKeysArray;
        result.Mode = CipherMode.ECB;
        result.Padding = PaddingMode.PKCS7;
        return result;
    }

    // 게임 종료시 자동 저장되도록
    private void OnApplicationQuit()
    {
        SaveGameData();
    }
}
