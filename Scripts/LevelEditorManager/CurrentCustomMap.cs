/*
MapEditorManager에서 선택한 레벨 - Edit_PlayController에서 실제로 맵 로딩
을 중간에서 서로에게 정보 전달하는 관리자 스크립트

1. MapEditorManager.cs에서 레벨 선택 시 해당 레벨 정보 저장 (레벨 이름, 제작자, 맵 정보 등등)
2. Edit_PlayController에서 CurrentCustomMap의 정보를 읽어 실제 맵 로딩
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentCustomMap : MonoBehaviour
{
    private static CurrentCustomMap instance = null;
    public string currentMapName;
    public string currentMapMadeBy;
    public string currentMapInfo;
    public int currentMapLike;
    public string currentMapOwnerinDate;
    public string currentMapinDate;

    public GameObject ep;

    void Awake() {
        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        else {
            Destroy(this.gameObject);
        }
    }

    public void ChangePreviewMode(bool state) {
        if(ep == null) {
            ep = GameObject.Find("EditorController");
            DebugX.Log(ep.transform.name);
        }
        DataController.Instance.gameData.isMapEditorPreview = state;
        ep.GetComponent<Edit_PlayController>().SetBasicUI(!state);
        ep.GetComponent<Edit_PlayController>().SetMiniMapUI(state);

    }

    public void IsAdsPanelOpen(bool state) {
        if(ep == null) {
            ep = GameObject.Find("EditorController");
            DebugX.Log(ep.transform.name);
        }
        DebugX.Log("광고패널함수실행됨");
        ep.GetComponent<Edit_PlayController>().SetMiniMapUI(!state);
    }

    public void ChangeIsOthers(bool state) {
        if(ep == null) {
            ep = GameObject.Find("EditorController");
            DebugX.Log(ep.transform.name);
        }
        DebugX.Log("플레이 모드 변경: 에디터 / 플레이어");
        ep.GetComponent<Edit_PlayController>().isOthers = state;
        
        ep.GetComponent<Edit_PlayController>().SetPlayMode();
    }
}
