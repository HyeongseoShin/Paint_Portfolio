/*
플레이어의 업적 상태를 서버와 연동하는 Singleton Manager

AchievementData - 플레이어의 업적 정보를 관리하는 Class

순서
1. void GetPlayerData() - 현재 서버에 나의 데이터가 있는지 확인

데이터가 없다면 
1-1. InitNewData() - 새로운 데이터 추가

데이터가 있다면
2. SetServerDataToLocal() - json 형태로 로컬에 저장

3. CheckNewInDateNeed() - 날마다 백업용 데이터를 만들기 위해 로그인 시간 차이 비교

4. InsertOriginalData() - 백업용 데이터 실제 서버 업로드

5. DeleteMyData() - 만약 백업용 데이터의 최대 수용치가 넘어가면 오래된 데이터부터 삭제 후 백업용 데이터 유지

그 외
- UpdateAllAchData() : 업적과 관련된 모든 정보 한 번에 서버 업로드 (업적 이름, 달성 여부, float, int data)
- UpdateAchNameData() : 업적 이름만 서버에 업로드
- UpdateAchListData() : 업적 클리어 여부 서버에 업로드
- UpdateTotIntValData() : 업적 int value 서버 업로드 (다시하기 횟수, 클리어 횟수 등등)
- UpdateTotIntFloatData() : 업적 float value 서버 업로드 (플레이 타임 등)
*/

using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;
using LitJson;
using System.Collections;
using UnityEngine.SceneManagement;

public partial class BackendAchManager : MonoBehaviour {
    private static BackendAchManager instance;
    public static BackendAchManager Instance {
        get {
            return instance;
        }
    }

    // 업적 테이블 이름
    private const string AchDataTableName = "Achievement_List";
    // 현재 사용중인 업적 게임 정보의 inDate
    private string achData_inDate;
    // 현재 사용중인 플레이어 업적 데이터(로컬)
    public AchievementData achData;
    // 데이터 불러오기를 통해 가져와진 업적 서버 데이터
    private JsonData achLoadData;

    // 뒤끝 함수 에러 발생 시 다시 요청할 최대 횟수
    private const int maxRepeatCount = 3;
    // 다시 요청한 횟수
    private int repeatCount = 0;

    // 인트로 화면에서 데이터 로딩 완료 시 퍼센트 증가하도록
    [SerializeField] private LoadingSceneManager lsm;
    
    void Awake() {
        if (instance == null) {
            instance = this;
        }

        achData = new AchievementData();
        repeatCount = 0;

        if(SendQueue.IsInitialize == false)
        {
            // SendQueue 초기화
            SendQueue.StartSendQueue(true, ExceptionHandler);
        }
    }

    void Update()
    {
        if (SendQueue.IsInitialize)
        {   
            SendQueue.Poll();
        }
    }

    // 서버에서 로컬로 받을 업적 데이터 형태 Init
    public void InitAchData() {
        achData = new AchievementData();
    }
    
    enum LoadProcess {
        GET_MY_DATA, // 내정보 불러오기
        INIT_NEW_DATA, // 내 정보가 없을 경우 새로 삽입하기(행동 후 Finish로 이동)
        SET_SERVER_DATA_TO_LOCAL, // 서버에서 불러온 데이터 로컬에 삽입
        CHECK_NEW_INDATE_NEED, // 현재 시간과 마지막으로 삽입한 데이터의 시간 비교
        INSERT_ORIGINAL_DATA, // 하루 이상 차이시 복원용 데이터 복사
        DELETE_MY_DATA, // 복원용 데이터가 n개 이상일 경우 n개를 남기고 모두 삭제
        FINISH // 해당 클래스의 데이터 불러오기 완료
    }

    private LoadProcess loadProcess;

    void ExceptionHandler(Exception e)
    {
        // 예외 처리
    }


    //불러오기 작업이 모두 완료되었을 때 호출, 다음 클래스 불러오기로 넘어감
    private void LoadFinish() {
        GoNextProcess(LoadProcess.FINISH);
    }

    // PlayerData 불러오기 시작(외부 호출용)
    public void StartPlayerDataLoad() {
        GoNextProcess(LoadProcess.GET_MY_DATA);
    }

    // 프로세스 이동, 재시작 카운트 초기화
    void GoNextProcess(LoadProcess process) {
        loadProcess = process;
        repeatCount = 0;

        RunLoadProcess();
    }

    // 동일한 함수 다시호출
    void RepeatThisProcess() {
        DebugX.Log("프로세스반복중");
        RunLoadProcess();
    }

    // PlayerLoad의 전체적인 호출
    void RunLoadProcess() {

        //호출할때마다 증가, 다른 프로세스로 이동할 경우 초기화
        repeatCount++;

        // maxRepeatCount 이상 에러 시, 와이파이 미접속이나 서버 장애가 지속된다고 판단하여 불러오기 중지
        // 로드가 제대로 되지 않은 채로 게임이 시작될 경우, 비정상적으로 플레이될 확률이 높음
        if (repeatCount > maxRepeatCount) {
            Debug.LogError("게임에 문제가 발생하였습니다.");
            //ErrorManager.Instance.ShowErrorPannel();
            return;
        }

        switch (loadProcess) {
            case LoadProcess.GET_MY_DATA:
                GetPlayerData();
                break;

            case LoadProcess.INIT_NEW_DATA:
                InitNewData();
                break;

            case LoadProcess.SET_SERVER_DATA_TO_LOCAL:
                SetServerDataToLocal();
                break;

            case LoadProcess.CHECK_NEW_INDATE_NEED:
                CheckNewInDateNeed();
                break;

            case LoadProcess.INSERT_ORIGINAL_DATA:
                InsertOriginalData();
                break;

            case LoadProcess.DELETE_MY_DATA:
                DeleteMyData();
                break;

            case LoadProcess.FINISH:
                // 모든 정보가 로드 완료 되면 로딩 % 증가
                AddPercentage();
                DebugX.Log("Backend 업적 정보 로드 완료!");
                break;
        }
    }

    //1. 서버에서 플레이어 데이터 불러오기
    private void GetPlayerData() {
        DebugX.Log("GetPlayerData 들어옴");
        SendQueue.Enqueue(Backend.GameData.GetMyData, AchDataTableName, new Where(), callback => {
            DebugX.Log("Player_GetMyData" + callback.ToString());
            if (callback.IsSuccess()) {

                //데이터가 존재할 경우 
                if (callback.FlattenRows().Count > 0) {
                    DebugX.Log("데이터가 있어서 로컬로 저장해야함!");
                    achLoadData = callback.FlattenRows();
                    GoNextProcess(LoadProcess.SET_SERVER_DATA_TO_LOCAL);
                }
                else {
                    //데이터가 존재하지 않을 경우
                    DebugX.Log("데이터가 없어서 새로 만들어야함!");
                    GoNextProcess(LoadProcess.INIT_NEW_DATA);
                }

            }
            else {
                DebugX.Log("콜백 실패!!");
                RepeatThisProcess();
            }
        });
        DebugX.Log("GetPlayerData 끝남");
    }

    //1-2. 데이터가 존재하지 않을 경우, 데이터 초기화 및 초기화된 데이터 삽입
    private void InitNewData() {
        DebugX.Log("InitNewData 들어옴");
        //플레이어 정보 초기화
        achData.Init();

        //초기화된 정보 삽입
        SendQueue.Enqueue(Backend.GameData.Insert, AchDataTableName, achData.GetParam(), callback => {
            DebugX.Log("Player_Insert_NewData" + callback.ToString());

            if (callback.IsSuccess()) {
                //데이터 적용
                achData_inDate = callback.GetInDate();
                LoadFinish();
            }
            else {
                RepeatThisProcess();
            }
        });
    }

    //2. 데이터가 존재할 경우, json으로 불러온 데이터 적용
    private void SetServerDataToLocal() {
        DebugX.Log("SetServerDataToLocal 들어옴");

        bool isSuccess = achData.SetData(achLoadData[0]);
        if (isSuccess) {
            GoNextProcess(LoadProcess.CHECK_NEW_INDATE_NEED);
        }
        else {
            //데이터 적용중 에러 발생!
            Debug.LogError("에러 발생!");
        }
    }

    // 3. 하루마다 복구용 데이터를 만들기 위해 현재시간과 마지막 row의 삽입 날짜를 비교.
    // 만약 하루마다가 아닌 로그인할때마다라고 한다면 바로 INSERT_ORIGINAL_DATA로 이동하면 된다.
    private void CheckNewInDateNeed() {
        DebugX.Log("CheckNewInDateNeed 들어옴");
        //뒤끝에서 inDate는 row의 유니크한 값이자 생성 날짜
        string myLastInDate = achLoadData[0]["inDate"].ToString();

        // UTC로 통일
        DateTime myLastInDateUtc = TimeZoneInfo.ConvertTimeToUtc(System.DateTime.Parse(myLastInDate));

        BackendReturnObject curTime = Backend.Utils.GetServerTime();
        string time = curTime.GetReturnValuetoJSON()["utcTime"].ToString();
        DateTime parsedDate = DateTime.Parse(time);

        //날짜가 하루 이상 차이가 날 경우
        if (myLastInDateUtc.Day != parsedDate.Day)
        {
            //복원용 데이터를 만들기
            GoNextProcess(LoadProcess.INSERT_ORIGINAL_DATA);
        }
        else {

            // 날짜가 같을 경우(오늘 내에 데이터를 다시 삽입하려고 한 경우)
            // 복원용 데이터를 생성할 일이 없으므로 패스
            LoadFinish();
        }
    }

    // 4. 복원용 데이터를 유지하기 위해 기존 데이터는 남겨두고 새로운 데이터를 삽입, 새로운 데이터의 inDate로 업데이트
    private void InsertOriginalData() {
        DebugX.Log("InsertOriginalData 들어옴");
        SendQueue.Enqueue(Backend.GameData.Insert, AchDataTableName, achData.GetParam(), callback => {
            DebugX.Log("Player_Insert_NewIndate" + callback.ToString());

            if (callback.IsSuccess()) {
                achData_inDate = callback.GetInDate();

                //삭제할 데이터가 없어도 호출
                GoNextProcess(LoadProcess.DELETE_MY_DATA);
            }
            else {
                RepeatThisProcess();
            }
        });
    }

    // 5. 복원용 데이터가 많이 삽입될수록 DB용량이 커지므로, n개의 데이터만 유지. n개 이상일 경우 오래된 데이터 삭제  
    // 데이터가 정확히 n개라면 하나만 삭제하면 되지만, n개 이상일 경우에는 한번에 삭제하기 위해 Transaction을 사용
    private void DeleteMyData() {
        DebugX.Log("DeleteMyData 들어옴");

        //deleteRowNum 해당 숫자만큼의 데이터를 제외하고 전부 삭제
        const int maxDataCount = 2;

        //불러오는 데이터가 max보다 많을 경우
        if (achLoadData.Count == maxDataCount + 1) {

            string delete_indate = achLoadData[maxDataCount]["inDate"].ToString();

            SendQueue.Enqueue(Backend.GameData.DeleteV2, AchDataTableName, delete_indate, Backend.UserInDate, callback => {
                DebugX.Log("Player_Delete" + callback.ToString());

                if (callback.IsSuccess()) {
                    // 게임 시작
                    LoadFinish();
                }
                else {
                    Debug.LogError("에러가 발생" + callback.ToString());
                }
            });
        }

        // 자신이 가지고 있을 데이터의 최대 갯수보다 2 개 이상일 경우
        else if (achLoadData.Count >= maxDataCount + 2) {
            List<TransactionValue> transactionList = new List<TransactionValue>();

            //트랜잭션은 최대 10개까지만 지원하므로 10개를 초과하면 에러가 발생할 수 있다.
            int transCount = 0;
            for (int i = maxDataCount; i < achLoadData.Count; i++) {

                string inDate = achLoadData[i]["inDate"].ToString();

                transactionList.Add(TransactionValue.SetDeleteV2(AchDataTableName, inDate, Backend.UserInDate));
                transCount++;

                //10개가 되면 그만!
                if (transCount >= 10) {
                    break;
                }
            }

            SendQueue.Enqueue(Backend.GameData.TransactionWriteV2, transactionList, callback => {
                DebugX.Log("Player_Transaction_Delete" + callback.ToString());

                if (callback.IsSuccess()) {
                    // 게임 시작
                    LoadFinish();
                }
                else {
                    Debug.LogError("에러 발생!" + callback.ToString());
                }
            });
        }
        else {
            // 게임 시작
            LoadFinish();
        }
    }

    // 모든 업적 데이터 서버에 업로드
    public void UpdateAllAchData() {
        SendQueue.Enqueue(Backend.GameData.UpdateV2, AchDataTableName, achData_inDate, Backend.UserInDate, achData.GetParam(), callback => {
            DebugX.Log("Player_Update AllAchData" + callback.ToString());

            if (callback.IsSuccess()) {
                DebugX.Log("전체 업적 데이터 서버 업로드");
            }
            else {
                RepeatThisProcess();
            }
        });

        DebugX.Log("전체 업적 데이터 서버 업로드");

    }

    // 업적 이름 데이터 서버에 업로드
    public void UpdateAchNameData() {
        SendQueue.Enqueue(Backend.GameData.UpdateV2, AchDataTableName, achData_inDate, Backend.UserInDate, achData.GetAchNameParam(), callback => {
            DebugX.Log("Player_Update AchNameData" + callback.ToString());

            if (callback.IsSuccess()) {
                DebugX.Log("업적 이름 데이터 서버 업로드");
            }
            else {
                RepeatThisProcess();
            }
        });

        DebugX.Log("업적 이름 데이터 서버 업로드");
    }

    // 업적 클리어 여부 데이터 서버에 업로드
    public void UpdateAchListData() {
        SendQueue.Enqueue(Backend.GameData.UpdateV2, AchDataTableName, achData_inDate, Backend.UserInDate, achData.GetAchListParam(), callback => {
            DebugX.Log("Player_Update AchListData" + callback.ToString());

            if (callback.IsSuccess()) {
                DebugX.Log("업적 클리어 여부 데이터 서버 업로드");
            }
            else {
                RepeatThisProcess();
            }
        });

        DebugX.Log("업적 클리어 여부 데이터 서버 업로드");
    }

    // 업적 int value 데이터 서버에 업로드 (Retry 횟수, 클리어 횟수 등등)
    public void UpdateTotIntValData() {
        SendQueue.Enqueue(Backend.GameData.UpdateV2, AchDataTableName, achData_inDate, Backend.UserInDate, achData.GetTotIntValParam(), callback => {
            DebugX.Log("Player_Update TotIntValData" + callback.ToString());

            if (callback.IsSuccess()) {
                DebugX.Log("업적 int value 데이터 서버 업로드");
            }
            else {
                RepeatThisProcess();
            }
        });

        DebugX.Log("업적 int value 데이터 서버 업로드");
    }

    // 업적 float value 데이터 서버에 업로드 (플레이 타임 등등)
    public void UpdateTotFloatValData() {
        SendQueue.Enqueue(Backend.GameData.UpdateV2, AchDataTableName, achData_inDate, Backend.UserInDate, achData.GetTotFloatValParam(), callback => {
            DebugX.Log("Player_Update TotFloatValData" + callback.ToString());

            if (callback.IsSuccess()) {
                DebugX.Log("업적 float value 데이터 서버 업로드");
            }
            else {
                RepeatThisProcess();
            }
        });

        DebugX.Log("업적 float value 데이터 서버 업로드");
    }

    // 데이터 로딩 완료 되었을 때 로딩 % 증가
    public void AddPercentage()
    {
        if(SceneManager.GetActiveScene().name == "Stage_Intro") {
            lsm.percentage += 10.0f;
        }   
    }
    

    // 게임 종료 시 자동 서버 업로드
    void OnApplicationQuit()
    {
        #if UNITY_EDITOR
            DebugX.Log("Not Update Server cause UNITY_EDITOR");
        #else
            DebugX.Log("Update Server cause not UNITY_EDITOR");
            UpdateAllAchData();
        #endif
        
        SendQueue.StopSendQueue();
    }
}