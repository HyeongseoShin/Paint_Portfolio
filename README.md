# Paint_Portfolio
유니티 2D 퍼즐 플랫포머 게임 Pa!nt 포트폴리오

* 해당 프로젝트는 이미 출시된 프로젝트로 전체 코드가 아닌 제가 작업한 Scripts의 일부분만 있습니다.


# Pa!nt


## 프로젝트 개요

<img src = "https://github.com/user-attachments/assets/ddbb748a-d41e-4daf-9620-41d15d27f792" width = "40%" height = "40%"/>

<img src = "https://github.com/user-attachments/assets/6a0353f1-98da-422c-8429-bf71f36ed5ec" width = "40%" height = "40%"/>

* 게임 장르: 캐주얼, 퍼즐
* 플랫폼 : PC (STEAM)
* 개발 환경 : Unity (2020.3.25f LTS)
* 서버 : 뒤끝 서버 (게임 서버 SaaS)
* 기간 : 22.1 ~ 24.11
* 인원 : 4명
  - 프로그래밍 : 2명 (본인)
  - 기획 : 1명
  - 아트 : 1명

* 스팀 페이지 : https://store.steampowered.com/app/2516270/Pant/?l=koreana

## 게임 소개

https://github.com/user-attachments/assets/d70e27aa-bd56-4050-adcd-15a6b6f71380


한 줄 소개
- "플레이어와 같은 색은 지나가고 다른 색은 부딪힌다"라는 성질을 이용해 플레이어의 색을 바꿔가며 퍼즐을 클리어하는 게임


## 전체 씬 구조
<img src="https://github.com/user-attachments/assets/7d772814-a33c-4af8-a6f8-d3bf5d465b25" />


### 주요 씬 이미지

#### 메인 씬
<img src = "https://github.com/user-attachments/assets/ec4c8409-c6c2-4f11-9daa-0dc98d682275" width = 40% height = 40% />


#### 게임 플레이
<img src = "https://github.com/user-attachments/assets/eb8924fc-f140-4d27-9a09-5c089d3f6ffd" width = 40% height = 40% />

<img src = "https://github.com/user-attachments/assets/87f48318-36b9-4b5e-b817-4cbd02356579" width = 40% height = 40% />

<img src = "https://github.com/user-attachments/assets/f5cc1c51-01f1-4e56-8b18-b42459d2e7a9" width = 40% height = 40% />


#### 커스텀 레벨 에디터 제작 씬
<img src = "https://github.com/user-attachments/assets/abc4d4f5-07b0-43bd-a14e-299869758274" width = 40% height = 40% />

<img src = "https://github.com/user-attachments/assets/bef8ad33-b206-4924-aff0-44d322916821" width = 40% height = 40% />

#### 커스텀 레벨 에디터 관리 시스템 씬
<img src = "https://github.com/user-attachments/assets/e54977ec-bb4b-41e4-bc60-7be9010635ef" width = 40% height = 40%/>

<img src = "https://github.com/user-attachments/assets/a391d18a-85d3-4ce7-99f8-e1c398771b4d" width = 40% height = 40%/>

## 메인 게임 로직
<img src="https://github.com/user-attachments/assets/9b57469d-f6b9-401e-8fe7-2306d96f2485" />



## 담당 업무

|주요 기능|세부 사항|
|Player Data 관리 및 서버 연동|인게임 데이터 Local Json 파일로 저장 (암호화)<br>스팀 계정 연동<br>유저 정보, 클리어 정보, 업적 달성 정보, 스테이지 플레이 로그, 커스텀 맵 데이터 등등 Read / Write|

#### 
#### 

### 커스텀 레벨 에디터 제작 참여
#### 레벨 데이터 파일 json 관리
#### CRUD

SDK 연결
* Steamworks
* Google Play Games (현재 사용 X)
* 뒤끝 서버 (게임서버 SaaS)

Player 조작감 개선
* 점프 버퍼 타임, 코요테 타임

Scene 관리 및 유기적 연결
* 다중 씬이 열려 있을 때 예외 처리
* 인게임 내 카메라 전환 관리

Post Processing을 이용한 흑백 연출

최적화
* Sprite Atlas
* Addressable Asset System
* 카메라 및 스크립트 최적화

힌트 기능 제작

인트로 컷신 & 튜토리얼 제작

퍼즐 레벨 디자인 (44개)

UI / UX



## 스크립트 폴더 설명
- BackendServer
- LevelEditor
- LevelEditorManager
- Player
- PlayerData

각 담당업무별 세부 구현 사항 PPT처럼 표현 필요??
플레이어 조작감 개선 부분 움짤 추가 필요?

