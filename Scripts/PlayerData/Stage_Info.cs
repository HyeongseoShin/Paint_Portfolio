/*
플레이어의 스테이지 클리어 정보를 관리하는 Class

- class AllStageInfo : 해당 스테이지의 클리어 정보
- class AllChapterInfo : 해당 챕터의 전체 스테이지 클리어 정보

- class StagePlayLog : 해당 스테이지의 플레이 로그 정보

- Param GetParam() : 모든 챕터의 모든 스테이지 클리어 정보 Column들 리턴
- Param GetParamOneChapter() : 한 챕터의 모든 스테이지 클리어 정보 Column 리턴
- Param GetParamStagePlayLog() : 현재 스테이지 플레이 로그 Column들 리턴
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BackEnd;
using LitJson;
using System.Text;

[Serializable]
public class AllChapterInfo {
    public List<AllStageInfo> allStages = new List<AllStageInfo>();
}

[Serializable]
public class AllStageInfo {
    public int c; // 클리어 여부
    public int s; // 별 개수
    public int pc; // 스테이지 클리어 후 컷신 (퍼즐) 클리어 여부 -> bool로 했을 때 string이 길어져 int로 대체
}

// 24-09-12 플레이어 로그 데이터 구조체
[Serializable]
public class StagePlayLog {
    public int c; // 챕터
    public int s; // 스테이지
    public string n; // 닉네임
    public int t; // 플레이타임
    public int grb; // 뒤로 가기
    public int r; // 다시하기 횟수
    public string cb; // 흑백 여부 => O (흑백) / X (원래 모드)
    public string b; // 초보자 여부 => O (초보자) / X (일반)
    public string h; // 힌트 여부 => O (힌트 봄) / X (힌트 안 봄)
    public int ht; // 처음 힌트를 누른 시간 (힌트 등장 여부 상관 없이)
    public int hc; // 힌트 등장 이후 총 힌트 본 횟수
}

public class Stage_Info : MonoBehaviour
{
    public int chapterCnt = 11;
    public int stageCnt = 10;
    public List<AllChapterInfo> allChapters = new List<AllChapterInfo>();
    public DateTime MyLastUpdate { get; set; }     // LastUpdate
    public StagePlayLog stagePlayLog = new StagePlayLog(); // 24-09-12

    // 24-09-12 플레이로그 데이터 초기화
    public void InitStagePlayLog()
    {
        stagePlayLog.c = 0;
        stagePlayLog.s = 0;
        stagePlayLog.n = "";
        stagePlayLog.t = 0;
        stagePlayLog.grb = 0;
        stagePlayLog.r = 0;
        stagePlayLog.cb = "X";
        stagePlayLog.b = "X";
        stagePlayLog.h = "X";
        stagePlayLog.ht = 0;
        stagePlayLog.hc = 0;
    }


    public void Init() {
        for(int i = 0; i < chapterCnt; i++) {
            allChapters.Add(new AllChapterInfo());

            DataController.Instance.gameData.ratingValueList.Add(new RatingValues());
            for(int j = 0; j < stageCnt; j++) {
                allChapters[i].allStages.Add(new AllStageInfo());

                DataController.Instance.gameData.ratingValueList[i].ct.Add(0.0f);
                DataController.Instance.gameData.ratingValueList[i].grb.Add(0);
                DataController.Instance.gameData.ratingValueList[i].restart.Add(0);
            }
        }

        MyLastUpdate = DateTime.Now;
    }

    //언마샬된 rows[0]의 json
    public bool SetData(JsonData json) {
        try {
            Init();
            
            for(int i = 0; i < chapterCnt; i++) {
                // 24-07-19 0챕터는 원래 없었기 때문에 빈 칸이라서 에러가 남 따라서 0챕터일 때는 따로 저장하지 않음
                // 서버 업로드할 때는 잘 업데이트 될 것
                try
                {
                    // 24-07-18 i+1 => i로 변경
                    string columnName = "Chap" + i.ToString();
                    string FromJsonData = json[columnName].ToString();
                    allChapters[i].allStages = JsonUtility.FromJson<Serialization<AllStageInfo>>(FromJsonData).ToList();
                }
                catch (Exception ex)
                {
                    DebugX.Log(i + " 챕터는 서버에 정보가 없어서 저장을 스킵함!");
                }
                
            }
            
            MyLastUpdate = DateTime.Parse(json["myLastUpdate"].ToString());
            DebugX.Log("MyLastUpdate: " + MyLastUpdate);

            return true;
        }
        catch (Exception e) {
            Debug.LogError(e);

            return false;
        }
    }

    public Param GetParam() {
        Param param = new Param();

        param.Add("NickName", BackendGameData.Instance.GetUserNickName());
        
        for(int i = 0; i < chapterCnt; i++) {

            // 24-07-18 i+1 => i로 변경
            string columnName = "Chap" + i.ToString();
            string ToJsonData = JsonUtility.ToJson(new Serialization<AllStageInfo>(allChapters[i].allStages));
            
            param.Add(columnName, ToJsonData);
        }
        
        param.Add("myLastUpdate", MyLastUpdate);

        return param;
    }

    // 테이블 전체 Update X -> 한 챕터 컬럼만 Update
    public Param GetParamOneChapter(int chapter) {
        Param param = new Param();

        string columnName = "Chap" + chapter.ToString();
        string ToJsonData = JsonUtility.ToJson(new Serialization<AllStageInfo>(allChapters[chapter].allStages));

        param.Add(columnName, ToJsonData);

        return param;
    }

    // 24-09-12 플레이로그 Column Update
    public Param GetParamStagePlayLog()
    {
        Param param = new Param();


        StringBuilder sb = new StringBuilder();

        sb.Append("c: ").Append(stagePlayLog.c)
        .Append(" s: ").Append(stagePlayLog.s)
        .Append(" n: ").Append(stagePlayLog.n)
        .Append(" cb: ").Append(stagePlayLog.cb)
        .Append(" b: ").Append(stagePlayLog.b)
        .Append(" h: ").Append(stagePlayLog.h);

        param.Add("NormalInfo", sb.ToString());
        param.Add("PlayTime", stagePlayLog.t);
        param.Add("GRB", stagePlayLog.grb);
        param.Add("Retry", stagePlayLog.r);
        param.Add("HintTime", stagePlayLog.ht);
        param.Add("HintCnt", stagePlayLog.hc);

        return param;
    }
}
