/*
업적 정보를 저장하는 Class

- class TotIntVal : 업적 관련 Int 변수 (다시하기 횟수, 커스텀 맵 업로드 횟수, 플레이횟수 등)
- class TotFloatVal: 업적 관련 Float 변수(플레이 시간 등)
- List<bool> AchList : 각 업적의 달성 여부 bool 값으로 저장
- List<string> achName : 각 업적의 이름 저장 (Key 값)

- Param GetParam() : 업적 관련 모든 데이터를 서버에 올리기 위해 Column 리턴
- Param GetAchNameParam() : 업적 이름 데이터를 서버에 올리기 위해 Column 리턴
- Param GetAchListParam() : 업적 달성 여부 데이터를 서버에 올리기 위해 Column 리턴
- Param GetTotIntValParam() : 업적 관련 Int 데이터를 서버에 올리기 위해 Column 리턴
- Param GetTotFloatValParam() : 업적 관련 Float 데이터를 서버에 올리기 위해 Column 리턴

Table 형태
각 유저마다 {[업적 이름 List] [업적달성 여부 List] [업적 Int 데이터 리스트] [업적 Float 데이터 리스트]} Column 형태의 row를 가짐
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BackEnd;
using LitJson;

[Serializable]
public class TotIntVal {
    public int tgbc; // totalGorightBeforeCount
    public int thpc; // totalHomeEditorPlayCount
    public int tcpc; // totalCustomMapPlayCount
    public int trc; // totalRetryCount
    public int tcuc; // totalCustomMapUploadCount
    public int tscc; // totalStageClearCount
    public int tssc; // totalStageSkipCount
    public int tsbsc; // totalStageBadSkipCount
}

[Serializable]
public class TotFloatVal {
    public float tcpt; // totalCustomMapPlayTime
    public float tcet; // totalCustomMapEditTime
    public float tsmp; // totalStoryMapPlayTime
    public float tspt; // totalSumPlayTime

    // 24-10-14 홈에디터, 메인메뉴 시간 서버 업로드
    public float thep; // totalHomeEditorPlayTime
    public float tmmp; // totalMainMenuPlayTime;
}

[Serializable]
public class AchList {
    public List<bool> achList = new List<bool>();
}

public class AchievementData : MonoBehaviour
{
    public List<string> achName = new List<string>(); // 업적 키 값
    public TotIntVal totIntVal = new TotIntVal();
    public TotFloatVal totFloatVal = new TotFloatVal();
    public List<AchList> achInfo = new List<AchList>(); // 업적 bool 값
    public DateTime MyLastUpdate { get; set; } // LastUpdate

    public void Init() {
        MyLastUpdate = DateTime.Now;
    }

    //언마샬된 rows[0]의 json
    public bool SetData(JsonData json) {
        try {
            Init();

            string achNameFromJsonData = json["AchName"].ToString();

            int achNameCnt = JsonUtility.FromJson<Serialization<string>>(achNameFromJsonData).ToList().Count;

            for(int i = 0; i < achNameCnt; i++) {
                achName.Add("");
            }

            achName = JsonUtility.FromJson<Serialization<string>>(achNameFromJsonData).ToList();

            string achInfoFromJsonData = json["AchList"].ToString();
            achInfo = JsonUtility.FromJson<Serialization<AchList>>(achInfoFromJsonData).ToList();


            string totIntValFromJsonData = json["TotIntVal"].ToString();
            totIntVal = JsonUtility.FromJson<TotIntVal>(totIntValFromJsonData);

            DebugX.Log("totIntVal.tsbsc: " + totIntVal.tsbsc);

            string totFloatValFromJsonData = json["TotFloatVal"].ToString();
            totFloatVal = JsonUtility.FromJson<TotFloatVal>(totFloatValFromJsonData);

            DebugX.Log("totFloatVal.tspt: " + totFloatVal.tspt);

            // 24-10-14 홈에디터, 메인메뉴 시간 서버 업로드
            if(totFloatVal.thep <= 0.0f)
            {
                totFloatVal.thep = DataController.Instance.gameData.homeEditorTime;

            }

            if(totFloatVal.tmmp <= 0.0f)
            {
                totFloatVal.tmmp = DataController.Instance.gameData.mainMenuTime;
            }
            
            DebugX.Log("totFloatVal.thep: " + totFloatVal.thep);
            DebugX.Log("totFloatVal.tmmp: " + totFloatVal.tmmp);
            
            MyLastUpdate = DateTime.Parse(json["myLastUpdate"].ToString());
            DebugX.Log("MyLastUpdate: " + MyLastUpdate);

            return true;
        }
        catch (Exception e) {
            Debug.LogError(e);

            return false;
        }
    }

    // 업적 관련 모든 데이터를 서버에 올리기 위해 Column 추가
    public Param GetParam() {
        Param param = new Param();

        string achNameToJsonData = JsonUtility.ToJson(new Serialization<string>(achName));
        
        string achInfoToJsonData = "";
        achInfoToJsonData = JsonUtility.ToJson(new Serialization<AchList>(achInfo));

        string totIntValToJsonData = JsonUtility.ToJson(totIntVal);


        totFloatVal.tcpt = Mathf.Floor(totFloatVal.tcpt * 10) * 0.1f;
        totFloatVal.tcet = Mathf.Floor(totFloatVal.tcet * 10) * 0.1f;
        totFloatVal.tsmp = Mathf.Floor(totFloatVal.tsmp * 10) * 0.1f;
        totFloatVal.tspt = Mathf.Floor(totFloatVal.tspt * 10) * 0.1f;
        
        // 24-10-14 홈에디터, 메인메뉴 시간 서버 업로드
        totFloatVal.thep = Mathf.Floor(totFloatVal.thep * 10) * 0.1f;
        totFloatVal.tmmp = Mathf.Floor(totFloatVal.tmmp * 10) * 0.1f;

        string totFloatValToJsonData = JsonUtility.ToJson(totFloatVal);

        param.Add("NickName", BackendGameData.Instance.GetUserNickName());
        param.Add("AchName", achNameToJsonData);
        param.Add("AchList", achInfoToJsonData);
        param.Add("TotIntVal", totIntValToJsonData);
        param.Add("TotFloatVal", totFloatValToJsonData);
        param.Add("myLastUpdate", MyLastUpdate);

        return param;
    }

    // 업적 이름 데이터를 서버에 올리기 위해 Column 추가
    public Param GetAchNameParam() {
        Param param = new Param();

        string achNameToJsonData = JsonUtility.ToJson(new Serialization<string>(achName));

        param.Add("AchName", achNameToJsonData);

        return param;
    }

    // 업적 달성 여부 데이터를 서버에 올리기 위해 Column 추가
    public Param GetAchListParam() {
        Param param = new Param();

        string achInfoToJsonData = "";

        achInfoToJsonData = JsonUtility.ToJson(new Serialization<AchList>(achInfo));

        param.Add("AchList", achInfoToJsonData);

        return param;
    }

    // 업적 관련 Int 데이터를 서버에 올리기 위해 Column 추가
    public Param GetTotIntValParam() {
        Param param = new Param();

        string totIntValToJsonData = JsonUtility.ToJson(totIntVal);

        param.Add("TotIntVal", totIntValToJsonData);

        return param;

    }

    // 업적 관련 Float 데이터를 서버에 올리기 위해 Column 추가
    public Param GetTotFloatValParam() {
        Param param = new Param();

        totFloatVal.tcpt = Mathf.Floor(totFloatVal.tcpt * 10) * 0.1f;
        totFloatVal.tcet = Mathf.Floor(totFloatVal.tcet * 10) * 0.1f;
        totFloatVal.tsmp = Mathf.Floor(totFloatVal.tsmp * 10) * 0.1f;
        totFloatVal.tspt = Mathf.Floor(totFloatVal.tspt * 10) * 0.1f;

        // 24-10-14 홈에디터, 메인메뉴 시간 서버 업로드
        totFloatVal.thep = Mathf.Floor(totFloatVal.thep * 10) * 0.1f;
        totFloatVal.tmmp = Mathf.Floor(totFloatVal.tmmp * 10) * 0.1f;

        DebugX.Log("totFloatVal.tspt: " + totFloatVal.tspt);

        string totFloatValToJsonData = JsonUtility.ToJson(totFloatVal);

        param.Add("TotFloatVal", totFloatValToJsonData);

        return param;

    }
}
