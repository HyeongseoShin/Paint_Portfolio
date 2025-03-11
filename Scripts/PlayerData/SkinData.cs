/*
보유 스킨 정보를 저장하는 Class

- List<string> glassesList : 안경류 스킨 아이템 아이템키 (서버에서 Json 형태로 받아 string으로 저장)
- List<string> hatList : 모자류 스킨 아이템 아이템키 (서버에서 Json 형태로 받아 string으로 저장)
- List<string> maskList : 인형탈류 스킨 아이템 아이템키 (서버에서 Json 형태로 받아 string으로 저장)

- List<int> glassesListInt : 안경류 스킨 아이템 아이템키 (string으로 변환된 데이터 Int로 변환해 Index 확인)
- List<int> hatListInt : 모자류 스킨 아이템 아이템키 (string으로 변환된 데이터 Int로 변환해 Index 확인)
- List<int> maskListInt : 인형탈류 스킨 아이템 아이템키 (string으로 변환된 데이터 Int로 변환해 Index 확인)

- Param GetParam() : 스킨 관련 모든 데이터를 서버에 올리기 위해 Column 리턴
- Param GetGlassListParam() : 안경류 스킨 관련 데이터를 서버에 올리기 위해 Column 리턴
- Param GetHatListParam() : 모자류 스킨 관련 데이터를 서버에 올리기 위해 Column 리턴
- Param GetMaskListParam() : 인형탈류 스킨 관련 데이터를 서버에 올리기 위해 Column 리턴

Table 형태
각 유저마다 {[안경류 스킨 키 값 List] [모자류 스킨 키 값 List] [인형탈류 스킨 키 값 List]} Column 형태의 row를 가짐
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BackEnd;
using LitJson;

public class SkinData : MonoBehaviour
{
    public List<string> glassesList = new List<string>(); // 안경류 스킨 아이템 아이템키 (서버에서 Json 형태로 받아 string으로 저장)
    public List<string> hatList = new List<string>(); // 모자류 스킨 아이템 아이템키 (서버에서 Json 형태로 받아 string으로 저장)
    public List<string> maskList = new List<string>(); // 인형탈류 스킨 아이템 아이템키 (서버에서 Json 형태로 받아 string으로 저장)

    public List<int> glassesListInt = new List<int>(); // 안경류 스킨 아이템 아이템키 (string으로 변환된 데이터 Int로 변환해 Index 확인)
    public List<int> hatListInt = new List<int>(); // 모자류 스킨 아이템 아이템키 (string으로 변환된 데이터 Int로 변환해 Index 확인)
    public List<int> maskListInt = new List<int>(); // 인형탈류 스킨 아이템 아이템키 (string으로 변환된 데이터 Int로 변환해 Index 확인)

    public DateTime MyLastUpdate { get; set; } // LastUpdate

    public void Init() {
        MyLastUpdate = DateTime.Now;
    }

    //언마샬된 rows[0]의 json
    public bool SetData(JsonData json) {
        try {
            Init();

            // Glasses
            string glassesListFromJsonData = json["GlassesList"].ToString();

            int glassesListCnt = JsonUtility.FromJson<Serialization<string>>(glassesListFromJsonData).ToList().Count;

            for(int i = 0; i < glassesListCnt; i++) {
                glassesList.Add("");
            }

            glassesList = JsonUtility.FromJson<Serialization<string>>(glassesListFromJsonData).ToList();

            glassesListInt = ConvertStringListToIntList(glassesList);

            // Hat
            string hatListFromJsonData = json["HatList"].ToString();

            int hatListCnt = JsonUtility.FromJson<Serialization<string>>(hatListFromJsonData).ToList().Count;

            for(int i = 0; i < hatListCnt; i++) {
                hatList.Add("");
            }

            hatList = JsonUtility.FromJson<Serialization<string>>(hatListFromJsonData).ToList();
            hatListInt = ConvertStringListToIntList(hatList);

            // Mask
            string maskListFromJsonData = json["MaskList"].ToString();

            int maskListCnt = JsonUtility.FromJson<Serialization<string>>(maskListFromJsonData).ToList().Count;

            for(int i = 0; i < maskListCnt; i++) {
                maskList.Add("");
            }

            maskList = JsonUtility.FromJson<Serialization<string>>(maskListFromJsonData).ToList();
            maskListInt = ConvertStringListToIntList(maskList);

            MyLastUpdate = DateTime.Parse(json["myLastUpdate"].ToString());
            DebugX.Log("MyLastUpdate: " + MyLastUpdate);

            return true;
        }
        catch (Exception e) {
            Debug.LogError(e);

            return false;
        }
    }

    // 스킨 관련 모든 데이터를 서버에 올리기 위해 Column 리턴
    public Param GetParam() {
        Param param = new Param();

        glassesList.Clear();
        hatList.Clear();
        maskList.Clear();

        glassesList = ConvertIntListToStringList(glassesListInt);
        hatList = ConvertIntListToStringList(hatListInt);
        maskList = ConvertIntListToStringList(maskListInt);


        string glassesListToJsonData = JsonUtility.ToJson(new Serialization<string>(glassesList));

        string hatListToJsonData = JsonUtility.ToJson(new Serialization<string>(hatList));

        string maskListToJsonData = JsonUtility.ToJson(new Serialization<string>(maskList));
        
        param.Add("NickName", BackendGameData.Instance.GetUserNickName());
        param.Add("GlassesList", glassesListToJsonData);
        param.Add("HatList", hatListToJsonData);
        param.Add("MaskList", maskListToJsonData);
        param.Add("myLastUpdate", MyLastUpdate); 

        return param;
    }

    // 안경류 스킨 관련 데이터를 서버에 올리기 위해 Column 리턴
    public Param GetGlassListParam() {
        Param param = new Param();

        glassesList.Clear();

        glassesList = ConvertIntListToStringList(glassesListInt);

        string glassesListToJsonData = JsonUtility.ToJson(new Serialization<string>(glassesList));

        param.Add("GlassesList", glassesListToJsonData);

        return param;
    }

    // 모자류 스킨 관련 데이터를 서버에 올리기 위해 Column 리턴
    public Param GetHatListParam() {
        Param param = new Param();

        hatList.Clear();

        hatList = ConvertIntListToStringList(hatListInt);

        string hatListToJsonData = JsonUtility.ToJson(new Serialization<string>(hatList));

        param.Add("HatList", hatListToJsonData);

        return param;
    }

    // 인형탈류 스킨 관련 데이터를 서버에 올리기 위해 Column 리턴
    public Param GetMaskListParam() {
        Param param = new Param();

        maskList.Clear();

        maskList = ConvertIntListToStringList(maskListInt);

        string maskListToJsonData = JsonUtility.ToJson(new Serialization<string>(maskList));

        param.Add("MaskList", maskListToJsonData);

        return param;
    }   
    
    // String -> Int 변환
    List<int> ConvertStringListToIntList(List<string> stringList)
    {
        List<int> intList = new List<int>();
        
        foreach (string str in stringList)
        {
            if (int.TryParse(str, out int number))
            {
                intList.Add(number);
            }
            else
            {
                Debug.LogWarning($"'{str}'는 유효한 정수가 아닙니다.");
            }
        }
        
        return intList;
    }

    // Int -> String 변환
    List<string> ConvertIntListToStringList(List<int> intList)
    {
        List<string> stringList = new List<string>();
        
        foreach (int number in intList)
        {
            stringList.Add(number.ToString());
        }
        
        return stringList;
    }

}
