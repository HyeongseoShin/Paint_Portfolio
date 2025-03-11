/*
Local에 저장된 설정값을 서버와 연동하는 Singleton Manager

다른 Manager와 달리 사용자의 플레이 기기가 달라졌을 때만 Read
설정 값이 바뀔 때만 Write

- GetSaveData() : 나의 설정값 데이터 가져오기
- InsertAllSaveData() : 모든 설정값 데이터 한 번에 서버에 새롭게 추가
- UpdateAllSaveData() : 모든 설정값 데이터 한 번에 서버에 업로드

- UpdateChap() : 최근 플레이 챕터 정보 서버 업로드
- UpdateIsUseHomeEditorOneTime() : 홈에디터를 사용 여부 서버 업로드
- UpdateIsUseGridNewOneTime() : 커스텀 레벨 에디터 사용 여부 서버 업로드
- UpdateIsUsePlayDoor() : 첫 게임 시작 여부 서버 업로드
- UpdateIsHomeEditorUnlockUIPopUp() : 홈 화면 편집 기능 잠금해제 UI 팝업 여부 서버 업로드
- UpdateIsLevelEditorUnlockUIPopUp() : 커스텀 레벨 에디터 기능 잠금해제 UI 팝업 여부 서버 업로드
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;
using LitJson;
using UnityEngine.SceneManagement;

public class BackendSaveDataManager : MonoBehaviour
{

    private static BackendSaveDataManager instance;
    public static BackendSaveDataManager Instance {
        get {
            return instance;
        }
    }
    // 해당 데이터를 저장/불러오기할 테이블 이름
    private const string stageInfoTableName = "SaveData_Info";

    // 현재 사용중인 게임정보의 inDate
    private string inDate;


    [SerializeField] private LoadingSceneManager lsm;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        if(SendQueue.IsInitialize == false)
        {
            SendQueue.StartSendQueue(true, ExceptionHandler);
        }
    }

    void ExceptionHandler(Exception e)
    {
        // 예외 처리
    }
    

    // 내 데이터 가져오기
    public void GetSaveData()
    {
        SendQueue.Enqueue(Backend.GameData.GetMyData, stageInfoTableName, new Where(), 1, bro =>
        {
            if(bro.IsSuccess() == false)
            {
                // 요청 실패 처리
                Debug.Log(bro);
                return;
            }
            if(bro.GetReturnValuetoJSON()["rows"].Count <= 0)
            {
                // 요청이 성공해도 where 조건에 부합하는 데이터가 없을 수 있기 때문에
                // 데이터가 존재하는지 확인
                // 위와 같은 new Where() 조건의 경우 테이블에 row가 하나도 없으면 Count가 0 이하 일 수 있다.  
                // Debug.Log(bro);

                // 데이터가 없다면 새로 삽입
                InsertAllSaveData();
                return;
            }

            inDate = bro.GetInDate();

            DataController.Instance.gameData.chapter = int.Parse(bro.FlattenRows()[0]["Chap"].ToString());
            DataController.Instance.gameData.isUseHomeEditorOneTime = bool.Parse(bro.FlattenRows()[0]["IsUseHomeEditorOneTime"].ToString());
            DataController.Instance.gameData.isUseGridNewOneTime = bool.Parse(bro.FlattenRows()[0]["IsUseGridNewOneTime"].ToString());
            DataController.Instance.gameData.isUseplaydoor = bool.Parse(bro.FlattenRows()[0]["IsUsePlayDoor"].ToString());
            DataController.Instance.gameData.isHomeEditorUnlockUIPopUp = bool.Parse(bro.FlattenRows()[0]["IsHomeEditorUnlockUIPopUp"].ToString());
            DataController.Instance.gameData.isLevelEditorUnlockUIPopUp = bool.Parse(bro.FlattenRows()[0]["IsLevelEditorUnlockUIPopUp"].ToString());

            AddPercentage();
        });
    }
    
    // 데이터가 없을 때 새롭게 서버에 추가
    public void InsertAllSaveData()
    {
        Param param = new Param();

        param.Add("NickName", BackendGameData.Instance.GetUserNickName());
        param.Add("Chap", DataController.Instance.gameData.chapter);
        param.Add("IsUseHomeEditorOneTime", DataController.Instance.gameData.isUseHomeEditorOneTime);
        param.Add("IsUseGridNewOneTime", DataController.Instance.gameData.isUseGridNewOneTime);
        param.Add("IsUsePlayDoor", DataController.Instance.gameData.isUseplaydoor);
        param.Add("IsHomeEditorUnlockUIPopUp", DataController.Instance.gameData.isHomeEditorUnlockUIPopUp);
        param.Add("IsLevelEditorUnlockUIPopUp", DataController.Instance.gameData.isLevelEditorUnlockUIPopUp);

        SendQueue.Enqueue(Backend.GameData.Insert, stageInfoTableName, param, callback => {
            if (callback.IsSuccess())
            {
                inDate = callback.GetInDate();
                DebugX.Log("전체 SaveData 서버 업로드");
                AddPercentage();
            }
        });
    }

    // 데이터가 이미 있을 때 값만 서버업데이트
    public void UpdateAllSaveData()
    {
        Param param = new Param();

        param.Add("Chap", DataController.Instance.gameData.chapter);
        param.Add("IsUseHomeEditorOneTime", DataController.Instance.gameData.isUseHomeEditorOneTime);
        param.Add("IsUseGridNewOneTime", DataController.Instance.gameData.isUseGridNewOneTime);
        param.Add("IsUsePlayDoor", DataController.Instance.gameData.isUseplaydoor);
        param.Add("IsHomeEditorUnlockUIPopUp", DataController.Instance.gameData.isHomeEditorUnlockUIPopUp);
        param.Add("IsLevelEditorUnlockUIPopUp", DataController.Instance.gameData.isLevelEditorUnlockUIPopUp);

        SendQueue.Enqueue(Backend.GameData.UpdateV2, stageInfoTableName, inDate, Backend.UserInDate, param, callback => {
            if (callback.IsSuccess())
            {
                DebugX.Log("전체 SaveData 서버 업로드");
            }
        });
    }

    // 최근 플레이 챕터 정보 서버 업로드
    public void UpdateChap()
    {
        Param param = new Param();

        param.Add("Chap", DataController.Instance.gameData.chapter);

        SendQueue.Enqueue(Backend.GameData.UpdateV2, stageInfoTableName, inDate, Backend.UserInDate, param, callback => {
            if (callback.IsSuccess())
            {
                DebugX.Log("최근 플레이 챕터 서버 업데이트");
            }
        });
    }

    // 홈에디터를 사용 여부 서버 업로드
    public void UpdateIsUseHomeEditorOneTime()
    {
        Param param = new Param();

        param.Add("IsUseHomeEditorOneTime", DataController.Instance.gameData.isUseHomeEditorOneTime);

        SendQueue.Enqueue(Backend.GameData.UpdateV2, stageInfoTableName, inDate, Backend.UserInDate, param, callback => {
            if (callback.IsSuccess())
            {
                DebugX.Log("홈에디터 입장 여부 서버 업데이트");
            }
        });
    }

    // 커스텀 레벨 에디터 사용 여부 서버 업로드
    public void UpdateIsUseGridNewOneTime()
    {
        Param param = new Param();

        param.Add("IsUseGridNewOneTime", DataController.Instance.gameData.isUseGridNewOneTime);

        SendQueue.Enqueue(Backend.GameData.UpdateV2, stageInfoTableName, inDate, Backend.UserInDate, param, callback => {
            if (callback.IsSuccess())
            {
                DebugX.Log("레벨 에디터 입장 여부 서버 업데이트");
            }
        });
    }

    // 첫 게임 시작 여부 서버 업로드
    public void UpdateIsUsePlayDoor()
    {
        Param param = new Param();

        param.Add("IsUsePlayDoor", DataController.Instance.gameData.isUseplaydoor);

        SendQueue.Enqueue(Backend.GameData.UpdateV2, stageInfoTableName, inDate, Backend.UserInDate, param, callback => {
            if (callback.IsSuccess())
            {
                DebugX.Log("게임시작 입장 여부 서버 업데이트");
            }
        });
    }

    // UI 팝업 여부 서버 업로드
    public void UpdateIsHomeEditorUnlockUIPopUp()
    {
        Param param = new Param();

        param.Add("IsHomeEditorUnlockUIPopUp", DataController.Instance.gameData.isHomeEditorUnlockUIPopUp);

        SendQueue.Enqueue(Backend.GameData.UpdateV2, stageInfoTableName, inDate, Backend.UserInDate, param, callback => {
            if (callback.IsSuccess())
            {
                DebugX.Log("홈에디터 팝업 등장 여부 서버 업데이트");
            }
        });
    }

    // UI 팝업 여부 서버 업로드
    public void UpdateIsLevelEditorUnlockUIPopUp()
    {
        Param param = new Param();

        param.Add("IsLevelEditorUnlockUIPopUp", DataController.Instance.gameData.isLevelEditorUnlockUIPopUp);

        SendQueue.Enqueue(Backend.GameData.UpdateV2, stageInfoTableName, inDate, Backend.UserInDate, param, callback => {
            if (callback.IsSuccess())
            {
                DebugX.Log("레벨에디터 팝업 등장 여부 서버 업데이트");
            }
        });
    }

    private void AddPercentage()
    {
        if(SceneManager.GetActiveScene().name == "Stage_Intro")
        {
            lsm.percentage += 10.0f;
        }
    }

    void OnApplicationQuit()
    {
        #if UNITY_EDITOR
            DebugX.Log("Not Update Server cause UNITY_EDITOR");
        #else
            DebugX.Log("Update Server cause not UNITY_EDITOR");
            UpdateChap();
        #endif
        
        SendQueue.StopSendQueue();
    }
}
