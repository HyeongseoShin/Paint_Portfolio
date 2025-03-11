/*
플레이어의 조작, 이동, 상호작용 등이 모두 포함된 스크립트

주요 제작 기능
- New Input System 을 이용한 플레이이어 조작 & 이동
- 점프 버퍼 타임, 코요테 점프를 이용한 조작감 개선

점프 버퍼 타임
- 관련 변수 : [Header("JumpBufferTime")]에 존재
- 관련 메서드
==> OnJump()
==> _Jump()
==> _Interact()

코요테 점프
- 관련 변수 : [Header("CoyoteJump")]에 존재
- 관련 메서드
==> _IsGround()
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEditor;

public class Player : MonoBehaviour
{
    
    [SerializeField]
    private bool newInpuInserted;
    [SerializeField]
    protected float Move;

    [SerializeField]
    private bool LeftMove = false;

    [SerializeField]
    private bool RightMove = false;

    [SerializeField]
    private bool clickLeft = false;

    [SerializeField]
    private bool clickRight = false;

    [SerializeField]
    protected Button leftKeyBtn;
    [SerializeField]
    protected Button rightKeyBtn;
    [SerializeField]
    protected Button jumpKeyBtn;
    [SerializeField]
    protected Button interactKeyBtn;

    public RaycastHit2D hit;

    public RaycastHit2D wallHit;

    public RaycastHit2D interactionHit;
    [SerializeField]
    protected GameObject GetItemMarker;

    //24-05-28 플레이어 물감 먹었을 때 말풍선 + 물감 뜨도록 변경
    [SerializeField]
    protected GameObject GetItemBubble;

    [SerializeField]
    public Rigidbody2D rb;
    [SerializeField]
    protected int NowLayer;
    
    [SerializeField]
    private Animator ColorChangeEffector;
    [SerializeField]
    protected List<Animator> Anis;
    [SerializeField]
    protected Animator Ani;//if Anis Work, this have to be clean
    [SerializeField]
    protected bool IsGround;

    [SerializeField]
    protected bool Interaction;

    [SerializeField]
    public bool InterDelay; // 위에 있는 Interaction과 동일한 역할을 하지만, itemscript에서 interact가 되지 않도록 막아두는건데 interaction 을 그냥 false로 만들었더니 자꾸 오류가 떠서 그냥 새로운 변수를 만들어버림!

    [SerializeField]
    protected bool Jump;
    [SerializeField]
    public int item;
    public bool key;
    [SerializeField]
    protected List<GameObject> Player_Animation;
    public  Mobile_UI UIManager;
    [SerializeField]
    protected GameObject Player_Active;
    [SerializeField]
    protected float Speed;
    [SerializeField]
    protected float MoveSpeed;

    [SerializeField]
    protected float MoveSpeed_backup;
    
    [SerializeField]
    protected float JumpPower;
    [SerializeField]
    protected float IsGroundDetection;
    
    [SerializeField]
    protected float SpeedOfPhysics;
    [SerializeField]
    protected float GravityDir;
    [SerializeField]
    protected float AdditionalGravity;
    
    
    [SerializeField]
    protected float CorrectGravity;
    
    [SerializeField]
    protected float MaxSpeed;
    [SerializeField]
    protected GameObject PlayerAnimationDir;
    public bool MobileInput;
    protected bool PlayerAnimationProtection;
    [SerializeField]
    protected GameObject StarIcon;
    [SerializeField]
    protected GameObject StarIconUI;


    protected AudioSource audioSource;

    [SerializeField]
    protected AudioClip jumpSound;

    [SerializeField]
    protected AudioClip walkSound;
    
    [SerializeField]
    protected bool JumpProtector;
    protected ObstacleMovement_v_2 MovingObstacle;

    public int keyCount = 0;
    

    public bool UIPop;

    public float speedByBox; //박스를 끌거나 밀 때 조절하기 위해

    public bool isGetbox;

    [SerializeField]
    private GameObject getBoxObj;

    // 끼임 추적을 위한 변수 추가
    [SerializeField]
    public List<Collider2D> collidedObjects = new List<Collider2D>();

    public List<GameObject> pullingBoxes = new List<GameObject>();

    private bool interactionDelayTime = false;

    [SerializeField]
    private bool onInteractionDelay = false;
    //New Input INsertion
    [SerializeField]
    private GameObject UIs;

    [Header("JumpBufferTime")]
    [SerializeField]
    private bool isForceJump = false; // 점프 보정이 필요한 시점인지 체크하는 변수

    [SerializeField]
    private int isPressedJump = 0; // 점프 키가 눌렀는지 안 눌렀는지 판단하는 변수

    [SerializeField]
    private float timeForForceJump = 0.0f; // 점프 보정 기능이 켜진 후 다시 꺼질 때까지의 시간

    [SerializeField]
    private float jumpBufferTime = 0.2f; // 점프 버퍼링 - 땅에 닿기 직전에 점프 키 누르면 땅에 닿았을 때 점프함
    [SerializeField]
    private float jumpBufferCounter; // 점프 가능 타이머 -> jumpBufferCounter > 0 일때 점프 가능

    [SerializeField]
    private float interactionBufferTime = 0.2f; // 상호작용 버퍼링 - 땅에 닿기 직전에 상호작용 키 누르면 땅에 닿았을 때 상호작용함
    [SerializeField]
    private float interactionBufferCounter; // 상호작용 가능 타이머 -> interactionBufferCounter > 0 일때 상호 가능


    [Header("Coyote Jump")]
    [SerializeField]
    private float boxCastMultiplier = 1.0f; // 끝지점에서 점프 보완 때 사용할 Boxcast의 Multiplier

    [SerializeField]
    private float timeForBoxCast = 0.0f; // boxCastMultiplier 가 늘어난 후 다시 줄어들 때까지의 시간

    [SerializeField]
    private bool isJumpingNow = false; // 점프 중 아래로 떨어지는 상태 vs 점프 없이 그냥 떨어지는 상태 구분 위해서

    [SerializeField]
    private bool isInteraction = false; // 플레이어 중앙에 세로로 긴 좁은 콜라이더가 땅에 닿으면 상호작용 가능하다는 것을 판단하는 변수

    [SerializeField]
    private bool havetoFalling = false;

    [SerializeField]
    private float timeForHavetofalling = 0f;

    [SerializeField]
    private bool resetFalling = true;
    

    // 24-09-03 힌트키 New Input System에 추가
    [SerializeField] private NewHintController hintController;
    

    [Header("Changing Tile Color")]
    [SerializeField]
    private GameObject SpriteMask_R, SpriteMask_G, SpriteMask_B, SpriteMask_Y;
    //24-04-28
    [SerializeField]
    private ColorFilterController[] colorFilterGO;

    [SerializeField]
    private Animator glassesAnim;

    [SerializeField]
    private Animator hatAnim;

    
    [SerializeField]
    private Animator maskAnim;

    [SerializeField]
    private Animator glassesAnim_filter;

    [SerializeField]
    private Animator hatAnim_filter;
    
    [SerializeField]
    private Animator maskAnim_filter;

    [SerializeField]
    private InputGuideController inputGuideController;

    [SerializeField]
    private SpecialPumpkin specialPumpkin;

    [SerializeField]
    private float defaultSpecialTime = 5f;

    public bool isTubeSpecial;

    [SerializeField]
    private GameObject SpecialAlarmObj;

    [SerializeField]
    private GameObject SpecialAlarmObj_temperature;
    [SerializeField]
    private Animator TemperatureAnimator;

    private IEnumerator moveSpeecCR;

    
    public void OnLeftMove(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            LeftMove = true;
            RightMove = false;

            clickLeft = true;
        }else if(context.canceled)
        {
            LeftMove = false;
            clickLeft = false;
            if(clickRight){
                RightMove = true;
            }
        }
    }

    public void OnRightMove(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            RightMove = true;
            LeftMove = false;

            clickRight = true;
        }else if(context.canceled)
        {
            RightMove = false;
            clickRight = false;

            if(clickLeft){
                LeftMove = true;
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        //24-05-01 플레이어가 움직일 수 있을 때만 점프 가능 (UI 켰다가 점프 키 누르고 UI 끄면 플레이어 점프하는 문제)
        if(!DataController.Instance.gameData.playerCanMove) {
            return;
        }

        DebugX.Log("Jump Inputted");
        if(context.performed)
        {
            
            DebugX.Log("뉴인풋에 의해 점프");

            // 점프 키를 눌렀을 때 jumpCounter 작동 (점프 성공 여부와 상관 없이) --> 점프 버퍼 타임
            jumpBufferCounter = jumpBufferTime;

            if(!isJumpingNow && IsGround) {
                Jump = true;
                DebugX.Log("점프 버튼 누름");
            }

            if(isForceJump) {
                isPressedJump++;
                DebugX.Log("지금 점프 눌렀음");
            }
            if (interactKeyBtn)
            {
                ColorBlock cb = jumpKeyBtn.colors;
                cb.normalColor = new Color32(200, 200, 200, 255);
                jumpKeyBtn.colors = cb;
            }
        }
        else if(context.canceled)
        {
            if (interactKeyBtn)
            {
                ColorBlock cb = jumpKeyBtn.colors;
                cb.normalColor = new Color32(255, 255, 255, 255);
                jumpKeyBtn.colors = cb;
            }
        }

    }

    // 24-09-03 힌트 NewInputSystem 추가
    public void OnHint(InputAction.CallbackContext context)
    {
        if(!DataController.Instance.gameData.playerCanMove || DataController.Instance.gameData.playMode != PlayMode.STORY || !GameObject.FindGameObjectWithTag("HintController"))
        {
            return;
        }

        if(!hintController)
        {
            hintController = GameObject.FindGameObjectWithTag("HintController").GetComponent<NewHintController>();
        }

        if(context.performed)
        {
            hintController.ShowHint();
        }
    }

    // 24-11-11 Tab 누르면 조작 가이드 띄우는 기능 추가
    public void OnInputGuideOpen(InputAction.CallbackContext context)
    {
        if(!DataController.Instance.gameData.playerCanMove || DataController.Instance.gameData.playMode != PlayMode.STORY)
        {
            return;
        }

        if(!inputGuideController)
        {
            inputGuideController = GameObject.FindGameObjectWithTag("InputGuideCanvas").transform.GetChild(0).GetComponent<InputGuideController>();
        }

        if (context.performed)
        {
            
            inputGuideController.TogglePanel();
        }
    }

    void InitSpecial (){
        specialPumpkin.isSpecial = false;
        specialPumpkin.specialTime = 0f;
        specialPumpkin.prevColor = NowLayer;
    }

    void Start() {
        PhysicsSetAUP();

        // 24-06-05 튜토리얼이라면 인디케이터 스크립트에 플레이어 변수 대입
        if(!DataController.Instance.gameData.isTutorialFinished) {
            FindObjectOfType<InteractionIndicatorController>().player = this;
        }

        // 24-08-25 만약 결과화면이라면, 이전에 클리어한 색상으로 시작하도록 한다.
        if(this.gameObject.scene.name == "HomePlayGrid_ResultScene"){
            ColorSetter(DataController.Instance.gameData.deskController.GetClearStatusColor());
        }
        
        
    }
    private void PhysicsSetAUP()
    {
        //Please Check and copy from project setting Gravity
        //this function is made from -22.0725
        AdditionalGravity = 0.03f*Mathf.Pow(SpeedOfPhysics,2.0f) * GravityDir;
        CorrectGravity = (9.81f*Mathf.Pow(SpeedOfPhysics,2.0f) - 22.0725f) * GravityDir;
        // (Mathf.Pow(SpeedOfPhysics,2.0f)*9.84f -22.0725f);
        MoveSpeed = 5*SpeedOfPhysics;// 5 is Constant of Speed;
        JumpPower = 1*SpeedOfPhysics; //1 is Constant of Jump;

        MoveSpeed_backup = MoveSpeed;
    }
    
    public void OnInteraction(InputAction.CallbackContext context)
    {

        //24-07-02 플레이어가 움직일 수 있을 때만 상호작용 가능 (UI 켰다가 상호작용 누르면 끝나고 다시 되는 문제)
        if(!DataController.Instance.gameData.playerCanMove)
        {
            return;
        }

        DebugX.Log("interaction Inputted");

        if(context.performed)
        {
            if(!DataController.Instance.gameData.isSettingPanelOn){
                if(!onInteractionDelay){
                    onInteractionDelay = true;
                    Invoke("SetonInteractionDelayFalse", 0.3f);
                    DebugX.Log("interaction Inputted performed");
                    
                    Interaction = true;
                    InterDelay = true;
                    
                    // 24-06-19 상호작용 키를 눌렀을 때 interactionCounter 작동 (상호작용 성공 여부와 상관 없이)
                    interactionBufferCounter = interactionBufferTime;

                    
                    if (interactKeyBtn)
                    {
                        ColorBlock cb = interactKeyBtn.colors;
                        cb.normalColor = new Color32(200, 200, 200, 255);
                        interactKeyBtn.colors = cb;
                    }
                }
            }
            
            
        }
        else if(context.canceled)
        {
            DebugX.Log("interaction Inputted cancel");
            
            // 2024-11-06 현재 끌고 있는 상자가 있고, 상호작용 버튼을 눌렀다 땔 때 상자를 해제하도록 한다.
            if(pullingBoxes.Count > 0) {
                pullingBoxes[0].GetComponent<BoxPullTrigger>().SetFalseisPull();
            }

            if (interactKeyBtn)
            {
                ColorBlock cb = interactKeyBtn.colors;
                cb.normalColor = new Color32(255, 255, 255, 255);
                interactKeyBtn.colors = cb;

            }
        }
        
        
    }

    private void SetonInteractionDelayFalse(){
        onInteractionDelay = false;
    }
    public void OnRestart(InputAction.CallbackContext context)
    {
        if(UIs==null)
        {
            UISFinding();
        }
        bool dcIsNullOrNotResultScene = false;

        if(DataController.Instance.gameData.deskController == null){
            dcIsNullOrNotResultScene = true;
        }
        if(!dcIsNullOrNotResultScene){
            if(!DataController.Instance.gameData.deskController.isInResultScene){
                dcIsNullOrNotResultScene = true;
            }else{
                dcIsNullOrNotResultScene = false;
            }
        }
        if(UIs && DataController.Instance.gameData.playerCanMove && dcIsNullOrNotResultScene) {
            
            switch(context.phase)
            {
                case InputActionPhase.Started:
                    UIs.GetComponent<InGamePlayer>().RestartStarted();
                break;
                case InputActionPhase.Canceled:
                    UIs.GetComponent<InGamePlayer>().RestartCanceled();
                break;
                case InputActionPhase.Performed:
                break;
            } 
        }
        
    }
    private void UISFinding()
    {
        UIs=GameObject.Find("UIs");
    }
    public void OnOption(InputAction.CallbackContext context)
    {
        DebugX.Log("OptionPushed");
        if(context.canceled)
            {
                DebugX.Log("OptionCalling");
            }
            if(UIs==null)
        {
            UISFinding();
        }
            
            
        if(UIs&&context.canceled)
        { 
            DebugX.Log("Option Called");
            UIs.GetComponent<InGamePlayer>().SetPlayerUIPopOn();
        }
    }
    
    
    private void OnEnable()
    {
        rb = this.GetComponent<Rigidbody2D>();

        // 24-07-30 색약 에셋 추가 시 원래 스프라이트 끄기 위해 Player_Active 초기 설정하기
        Player_Active = Player_Animation[NowLayer - 5];

        if(GameObject.FindWithTag("MobileManager"))
            UIManager = GameObject.FindWithTag("MobileManager").GetComponent<Mobile_UI>();
        DebugX.Log("PlayerOnENable");
        UIManager.Player = this.gameObject;
        Ani = Player_Active.GetComponent<Animator>();
        GravityDir = this.GetComponent<Rigidbody2D>().gravityScale;
        audioSource = this.GetComponent<AudioSource>();
        
        ColorSetter(NowLayer, false);
        item = 0;
        speedByBox = 1;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("BOX")){
            DebugX.Log("On player");
            if(getBoxObj == null){
                getBoxObj = other.gameObject.transform.parent.gameObject;
            }
            
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("BOX")){
            if(getBoxObj != null){
                getBoxObj = null;
            }
            
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!collidedObjects.Contains(other.collider))
        {
            collidedObjects.Add(other.collider); 
        }

    }
    
    private void OnCollisionStay2D(Collision2D other)
    {
        if (!collidedObjects.Contains(other.collider))
        {
            collidedObjects.Add(other.collider); 
        }
    }
    private void OnCollisionExit2D(Collision2D other)
    {
        if (collidedObjects.Contains(other.collider))
        {
            collidedObjects.Remove(other.collider); 
        }
    }


    private void Awake() {
        if(DataController.Instance.gameData.isColorFilterAssistant)
        {
            glassesAnim = glassesAnim_filter;
            hatAnim = hatAnim_filter;
            maskAnim = maskAnim_filter;
        }
        
    }

    void Update()
    {
        if(isJumpingNow) {
            DebugX.Log("점프 중입니다.");
        }

        if(!newInpuInserted)
        {
            if (!UIPop && DataController.Instance.gameData.playerCanMove && !DataController.Instance.gameData.tryGetGimmick)
            {
                Move = Input.GetAxisRaw("Horizontal");//temp Contorller
                                                    //DebugX.Log("IS Work"+Move);   
                if (Input.GetButtonDown("Interaction"))
                {
                    if (interactKeyBtn)
                    {

                        ColorBlock cb = interactKeyBtn.colors;
                        cb.normalColor = new Color32(200, 200, 200, 255);
                        interactKeyBtn.colors = cb;
                    }
                    Interaction = true;
                    InterDelay = true;
                }
                else
                {
                    Interaction = false;
                    InterDelay = false;
                    if (interactKeyBtn)
                    {
                        ColorBlock cb = interactKeyBtn.colors;
                        cb.normalColor = new Color32(255, 255, 255, 255);
                        interactKeyBtn.colors = cb;

                    }
                }

                if (Input.GetButtonDown("Jump"))
                {
                    if (interactKeyBtn)
                    {

                        ColorBlock cb = jumpKeyBtn.colors;
                        cb.normalColor = new Color32(200, 200, 200, 255);
                        jumpKeyBtn.colors = cb;

                    }
                    DebugX.Log("스페이스에 의해 점프");
                    Jump = true;
                }
                else if (Input.GetButtonUp("Jump"))
                {
                    if (interactKeyBtn)
                    {
                        ColorBlock cb = jumpKeyBtn.colors;
                        cb.normalColor = new Color32(255, 255, 255, 255);
                        jumpKeyBtn.colors = cb;
                    }
                }
            }
            else
            { SetRBZero(); }
        }
        else
        {

        }
    }

    protected void FixedUpdate()
    {
        if (!UIPop && DataController.Instance.gameData.playerCanMove && !DataController.Instance.gameData.tryGetGimmick)
        // if (!UIPop)
        {
            if (keyCount != 0)
            {
                rb.velocity = new Vector3(0, 0, 0);
                return;
            }
            else
            {
                //2024-12-03 시간 제한 특수 물감 시간 체크하기
                if(specialPumpkin.isSpecial){
                    if(specialPumpkin.specialTime > 1e-12){
                        //특수 상태일 때 남은 시간이 1e-12보다 크면 계속해서 시간을 차감한다.
                        specialPumpkin.specialTime -= Time.fixedDeltaTime;
                        

                        SpecialAlarmObj_temperature.transform.localScale = new Vector3(1, specialPumpkin.specialTime / defaultSpecialTime, 1 );
                        TemperatureAnimator.SetFloat("LeftTime", specialPumpkin.specialTime/defaultSpecialTime);

                    }else{
                        //특수 상태일 때 남은 시간이 1e-12보다 작으면 

                        //특수 상태를 종료한다
                        specialPumpkin.isSpecial = false;

                        //종료하면서 이전 색상으로 변경한다.
                        ColorSetter(specialPumpkin.prevColor, false);

                        //특수 상태 시간 0으로 초기화 한다.
                        specialPumpkin.specialTime = 0f;
                    }
                }
                

                if(LeftMove){
                    Move = -1;
                }else if(RightMove){
                    Move = 1;
                }else{
                    Move = 0;
                }
                _IsGround();
                if(isForceJump && !isJumpingNow) {
                    timeForForceJump += Time.fixedDeltaTime;
                    DebugX.Log("Time For ForceJUmp: " + timeForForceJump);
                    rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                    DebugX.Log("Y값 고정!!");

                }
                if(havetoFalling){//만약 계속 떨어져야 한다면
                    timeForHavetofalling += Time.deltaTime;
                    if(timeForHavetofalling >= 0.04f){ //여기로 오세요.
                        havetoFalling = false;
                        timeForHavetofalling = 0f;
                    }
                }

                if(timeForForceJump >= 0.0525f) {
                    DebugX.Log("공중에 떨어지는 중");
                    IsGround = false;
                    Ani.SetBool("IsAir", true);

                    glassesAnim.SetBool("IsAir", true);
                    hatAnim.SetBool("IsAir", true);
                    maskAnim.SetBool("IsAir", true);

                    JumpProtector = true;
                    Jump = false;
                    MovingObstacle?.SetHit(false);
                    boxCastMultiplier = 1.0f;
                    isForceJump = false;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                
                    int layermaskfalling = (1 << this.gameObject.layer) + (1 << LayerMask.NameToLayer("Ignore Raycast")) + (1 << LayerMask.NameToLayer("Items")) + (1 << LayerMask.NameToLayer("MapEditor")) + (1 << this.gameObject.layer + 7) + (1 << LayerMask.NameToLayer("BOX"))+(1<<LayerMask.NameToLayer("TextTrigger"));
                    layermaskfalling = ~ layermaskfalling;
                    RaycastHit2D hitForhavetoFalling = Physics2D.Raycast(new Vector2(transform.position.x,transform.position.y - GravityDir) , new Vector2(Move, 0), 0.9f, layermaskfalling);
                    Debug.DrawRay(transform.position - new Vector3(0,GravityDir,0), new Vector2(Move, 0)* 10, Color.blue);

                    if(resetFalling){
                        DebugX.Log("ForFalling resetFalling = true");
                        if(hitForhavetoFalling.collider != null){
                            DebugX.Log("ForFalling " + hitForhavetoFalling.collider.name);
                            havetoFalling = true;
                        }else{
                            DebugX.Log("ForFalling 없음" );
                            havetoFalling = false;
                            resetFalling = false;
                        }
                    }
                    

                }else{
                    havetoFalling = false;
                    resetFalling = true;
                }

                if(!isJumpingNow && boxCastMultiplier != 1.0) {
                    timeForBoxCast += Time.fixedDeltaTime;

                    if(timeForBoxCast >= 0.07f) {
                        boxCastMultiplier = 1.0f;
                        timeForBoxCast = 0.0f;
                    }
                }

                if (!IsGround)
                {
                    _IsFall();
                }
                _Move();
                _Jump();
                _Interact();
            }
        }
        else
        { SetRBZero(); }
        

    }
    protected void AnimationSetter(string animationString, bool boolean)
    {
        for (int i = 0; i < Anis.Count; i++)
        {
            Anis[i].SetBool(animationString, boolean);
        }

        glassesAnim.SetBool(animationString, boolean);
        hatAnim.SetBool(animationString, boolean);
        maskAnim.SetBool(animationString, boolean);
    }

    // 24-06-19 상호작용에도 Buffer Time 넣기 위해 함수 작성
    protected void _Interact()
    {
        // 상호작용 키를 눌렀던 적이 있고 땅에 닿았음
        if(interactionBufferCounter > 0.0f && IsGround)
        {
            interactionBufferCounter = 0.0f;
            DebugX.Log("Interaction = true 지금 상호작용 가능!");
        }


        // 상호작용 가능 시간 끝났음
        else if(interactionBufferCounter <= 0.0f)
        {
            Interaction = false;
            InterDelay = false;
            interactionBufferCounter -= Time.deltaTime;
        }

        // 상호작용 가능한 시간 진행 중
        else
        {
            interactionBufferCounter -= Time.deltaTime;
        }
    }

    // 점프 버퍼 타임 기능 추가
    protected void _Jump()
    {
        // 점프 카운터 > 0.0f 일때 -> 점프 보정 (착지 전에 점프 누르면 땅에 닿고 점프 하도록)
        if (jumpBufferCounter > 0.0f && (IsGround || (isForceJump && (isPressedJump > 0 || Jump))))
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            boxCastMultiplier = 1.0f;
            DebugX.Log("_Jump에서 점프");
            audioSource.clip = jumpSound;

            audioSource.PlayOneShot(jumpSound, 0.45f);
    
            rb.velocity = transform.up * 9.8f * JumpPower * GravityDir + new Vector3(rb.velocity.x,rb.velocity.y,0);//D

            Ani.SetBool("Up", true);

            glassesAnim.SetBool("Up", true);
            hatAnim.SetBool("Up", true);
            maskAnim.SetBool("Up", true);

            isJumpingNow = true;
            IsGround = false;
            JumpProtector = true;
            Jump = false;

            isPressedJump--;
            if(isPressedJump < 0) {
                isPressedJump = 0;
            }

            jumpBufferCounter = 0.0f;
            isForceJump = false;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // 땅에 닿기 직전 점프 누르면 착지했을 때 점프 가능하도록
        else {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    protected void _IsGround()
    {
        // 24-06-11 TouchCollider도 layerMask 안 먹히도록 추가
        // 24-06-20 TouchCollider --> CanNotOnAirTrigger로 Layer 이름 변경
        // 24-06-25 ItemBottomCollider도 layerMask 안 먹히도록 추가
        int layerMask = (1 << this.gameObject.layer) + (1 << LayerMask.NameToLayer("Ignore Raycast")) + (1 << LayerMask.NameToLayer("Items")) + (1 << LayerMask.NameToLayer("MapEditor")) + (1 << this.gameObject.layer + 7) + (1 << LayerMask.NameToLayer("BOX"))+(1<<LayerMask.NameToLayer("TextTrigger")) + (1 << LayerMask.NameToLayer("CanNotOnAir")) + (1 << LayerMask.NameToLayer("ItemBottomCollider"))
        + (1 << LayerMask.NameToLayer("BottomW")) + (1 << LayerMask.NameToLayer("BottomR")) + (1 << LayerMask.NameToLayer("BottomG")) + (1 << LayerMask.NameToLayer("BottomB")) + (1 << LayerMask.NameToLayer("BottomY")); // (1 << this.gameObject.layer + 18)
        layerMask = ~layerMask;

        var rigidbody = GetComponent<Rigidbody2D>();

        // 2024-01-16 원래 interactionHit으로 쓸려고 했으나 옆 벽의 존재를 확인하는 Hit로 써야할 듯

        if(GravityDir > 0) {
            wallHit = Physics2D.BoxCast(transform.position + new Vector3(0.1f,0.3f,0), new Vector2(0.87f, 0.25f), 0f, Vector2.up * -1 * GravityDir * (float)0.01, IsGroundDetection, layerMask);
            BoxCastDrawer.Draw(wallHit, transform.position + new Vector3(0.1f,0.3f,0), new Vector2(0.87f, 0.25f), 0f, Vector2.up * -1 * GravityDir * (float)0.01, IsGroundDetection);

            interactionHit = Physics2D.BoxCast(transform.position + new Vector3(0.1f,0.2f,0), new Vector2(0.1f, 0.5f), 0f, Vector2.up * -1 * 0.01f, IsGroundDetection, layerMask);
            BoxCastDrawer.Draw(wallHit, transform.position + new Vector3(0.1f,0.2f,0), new Vector2(0.1f, 0.5f), 0f, Vector2.up * -1 * GravityDir * 0.01f, IsGroundDetection);
        }
        else {
            wallHit = Physics2D.BoxCast(transform.position + new Vector3(0.1f,-0.3f,0), new Vector2(0.87f, 0.25f), 0f, Vector2.up * -1 * GravityDir * (float)0.01, IsGroundDetection, layerMask);
            BoxCastDrawer.Draw(wallHit, transform.position + new Vector3(0.1f,-0.3f,0), new Vector2(0.87f, 0.25f), 0f, Vector2.up * -1 * GravityDir * (float)0.01, IsGroundDetection);

            interactionHit = Physics2D.BoxCast(transform.position + new Vector3(0.1f,-0.2f,0), new Vector2(0.1f, 0.5f), 0f, Vector2.up * -1 * GravityDir * 0.01f, IsGroundDetection, layerMask);
            BoxCastDrawer.Draw(wallHit, transform.position + new Vector3(0.1f,-0.2f,0), new Vector2(0.1f, 0.5f), 0f, Vector2.up * -1 * GravityDir * 0.01f, IsGroundDetection);
        }
        

        if(interactionHit.collider != null && interactionHit.collider.tag == "Ground") {
            isInteraction = true;
        }
        else {
            isInteraction = false;
        }

        if(wallHit.collider != null && wallHit.collider.tag == "Ground" || !JumpProtector) {
            if (rigidbody.velocity.y * GravityDir > 0.01f) {
                hit = Physics2D.BoxCast(transform.position + new Vector3(0.01f,0.2f,0), new Vector2(0.5f, 0.05f), 0f, Vector2.up * -1 * GravityDir * (float)0.01, IsGroundDetection, layerMask);
                BoxCastDrawer.Draw(hit, transform.position + new Vector3(0.01f,0.2f,0), new Vector2(0.5f, 0.05f), 0f, Vector2.up * -1 * GravityDir * (float)0.01, IsGroundDetection);

            }
            else if (rigidbody.velocity.y * GravityDir < -0.01f){
                hit = Physics2D.BoxCast(transform.position + new Vector3(0.1f,-0.1f,0), new Vector2(1.2f * boxCastMultiplier, 0.1f), 0f, Vector2.up * -1 * GravityDir * (float)0.01, IsGroundDetection, layerMask);
                BoxCastDrawer.Draw(hit, transform.position + new Vector3(0.1f,-0.1f,0), new Vector2(1.2f * boxCastMultiplier, 0.1f), 0f, Vector2.up * -1 * GravityDir * (float)0.01, IsGroundDetection);
                DebugX.Log("박스캐스트 늘어남 " + " 길이: " + boxCastMultiplier);
            }
            else {
                hit = Physics2D.BoxCast(transform.position + new Vector3(0.1f,-0.1f,0), new Vector2(1.2f, 0.1f), 0f, Vector2.up * -1 * GravityDir * (float)0.01, IsGroundDetection, layerMask);
                BoxCastDrawer.Draw(hit, transform.position + new Vector3(0.1f,-0.1f,0), new Vector2(1.2f, 0.1f), 0f, Vector2.up * -1 * GravityDir * (float)0.01, IsGroundDetection);
            }
        }

        else {
            hit = Physics2D.BoxCast(transform.position + new Vector3(0.01f,0.2f,0), new Vector2(0.0f, 0.00f), 0f, Vector2.up * 0 * GravityDir * (float)0.01, IsGroundDetection, layerMask);
        }
        
        if (hit.collider != null)
        {
            // DebugX.Log("hit: " + hit.collider.gameObject.layer);
            if (hit.collider.tag == "Ground")
            {
                if(boxCastMultiplier != 1.0f) {
                    DebugX.Log("지금 점프 가능!");
                    isForceJump = true;
                }

                else {
                    isForceJump = false;
                    timeForForceJump = 0.0f;
                    timeForBoxCast = 0.0f;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                }

                IsGround = true;
                

                Ani.SetBool("IsAir", false);

                glassesAnim.SetBool("IsAir", false);
                hatAnim.SetBool("IsAir", false);
                maskAnim.SetBool("IsAir", false);

                JumpProtector = false;

                if (hit.collider.transform.parent != null && hit.collider.transform.parent != null && hit.collider.transform.parent.GetComponentInChildren<ObstacleMovement_v_2>() != null)
                {
                    MovingObstacle = hit.collider.transform.parent.GetComponentInChildren<ObstacleMovement_v_2>();
                    MovingObstacle.SetPlayer(this);
                    MovingObstacle.SetHit(true);
                }
                else
                {
                    MovingObstacle?.SetHit(false);
                }

                isJumpingNow = false;
            }
        }
        else
        {
            
            if(timeForForceJump >= 0.0625f) {
                DebugX.Log("공중에 떨어지는 중");
                IsGround = false;
                Ani.SetBool("IsAir", true);

                glassesAnim.SetBool("IsAir", true);
                hatAnim.SetBool("IsAir", true);
                maskAnim.SetBool("IsAir", true);

                JumpProtector = true;
                Jump = false;
                MovingObstacle?.SetHit(false);
                boxCastMultiplier = 1.0f;
                isForceJump = false;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            // 코요테 점프 : 정해진 점프 가능 시간 이후 Boxcast 크기 원래대로
            if(timeForBoxCast >= 0.07f) {
                boxCastMultiplier = 1.0f;
                timeForBoxCast = 0.0f;
            }

            // 코요테 점프 : 플레이어가 타일 끝을 벗어나도 일정 시간 동안 공중에서 점프를 가능케 하여 편의성 높임
            else if(Move != 0 && !isJumpingNow && timeForForceJump == 0.0f && timeForBoxCast == 0.0f && rb.velocity.y >= -0.5f && rb.velocity.y <= 0.5f && jumpBufferCounter <= 0.0f) {
                DebugX.Log("박스캐스트 * 2 :" + jumpBufferCounter);
                boxCastMultiplier = 1.75f;
            }


            IsGround = false;
            Ani.SetBool("IsAir", true);

            glassesAnim.SetBool("IsAir", true);
            hatAnim.SetBool("IsAir", true);
            maskAnim.SetBool("IsAir", true);
            
            
        }
    }
    protected void _IsFall()
    {
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - AdditionalGravity-CorrectGravity*Time.deltaTime);        
        // rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -13.0f));
        
        // DebugX.Log("rb.velocity.y: " + rb.velocity.y);
        if (rb.velocity.y * GravityDir > 0)
        {
            Ani.SetBool("Up", true);
            // if (rb.velocity.y * GravityDir < 10){
            //     Ani.SetBool("IsfallingBefore", true);
            // }

            glassesAnim.SetBool("Up", true);
            hatAnim.SetBool("Up", true);
            maskAnim.SetBool("Up", true);
        }
        else
        {
            Ani.SetBool("Up", false);
            // Ani.SetBool("IsfallingBefore", false);
            
            glassesAnim.SetBool("Up", false);
            hatAnim.SetBool("Up", false);
            maskAnim.SetBool("Up", false);
        }
    }

    protected void _Move()
    {

        //transform.Translate(Vector3.right*Move*Speed*Time.deltaTime);
        //rb.AddForce(Vector2.right*Move*Speed,ForceMode2D.Force);
        if(havetoFalling){
            Speed = 0f;
            DebugX.Log("ForFalling 스피드 0으로" );
        }else{
            Speed = MoveSpeed;
        }
        rb.velocity = new Vector3(Move * Speed * speedByBox, rb.velocity.y);
        // DebugX.Log("rb의 속도: " + rb.velocity.x + "speedByBox 속도: " + speedByBox);

        float Diry = 0;
        float Dirx = 0;
        if (Move == 0)
        {
            if (leftKeyBtn || rightKeyBtn)
            {
                ColorBlock cb = leftKeyBtn.colors;
                cb.normalColor = new Color32(255, 255, 255, 255);
                leftKeyBtn.colors = cb;
                rightKeyBtn.colors = cb;
            } // rb.AddForce(Vector2.right*Move*Speed,ForceMode2D.Force);
            Ani.SetBool("IsWork", false);

            glassesAnim.SetBool("IsWork", false);
            hatAnim.SetBool("IsWork", false);
            maskAnim.SetBool("IsWork", false);
            // DebugX.Log("Move Inputted 움직이는중X");
        }
        else
        {
            // DebugX.Log("Move Inputted 움직이는중");
            if (!audioSource.isPlaying && IsGround)
            {
                audioSource.clip = walkSound;
                audioSource.PlayOneShot(walkSound, 0.7f);

                DebugX.Log("뾱뾱뾱뾱");
            }
            Ani.SetBool("IsWork", true);

            glassesAnim.SetBool("IsWork", true);
            hatAnim.SetBool("IsWork", true);
            maskAnim.SetBool("IsWork", true);
            // DebugX.Log(Move);
            if (Move > 0)
            {
                Diry = 180;
                if (leftKeyBtn || rightKeyBtn)
                {

                    ColorBlock cb = rightKeyBtn.colors;
                    cb.normalColor = new Color32(200, 200, 200, 255);
                    rightKeyBtn.colors = cb;
                }
            }
            else if (Move < 0)
            {
                Diry = 0;
                if (leftKeyBtn || rightKeyBtn)
                {
                    ColorBlock cb = leftKeyBtn.colors;
                    cb.normalColor = new Color32(200, 200, 200, 255);
                    leftKeyBtn.colors = cb;
                }
            }
            if (GravityDir == 1)
            {
                Dirx = 0;
            }
            else if (GravityDir == -1)
            {
                Dirx = 180;
            }
            PlayerAnimationDir.GetComponent<Transform>().eulerAngles = new Vector3(Dirx, Diry, 0);

            // 24-05-28 더 이상 itemGetter가 대칭이 아니므로 계속 정방향 유지하려면 해당 코드 주석 해제
            if(Diry == 180) {
                if(Dirx == 180)
                {
                    GetItemMarker.GetComponent<SpriteRenderer>().flipX = false;
                }
                else
                {
                    GetItemMarker.GetComponent<SpriteRenderer>().flipX = true;
                }
                
            }
            else {
                if(Dirx == 180)
                {
                    GetItemMarker.GetComponent<SpriteRenderer>().flipX = true;
                }
                else
                {
                    GetItemMarker.GetComponent<SpriteRenderer>().flipX = false;
                }
            }
            
        }
    }
    public void GravityReverse()
    {
        //if (transform.rotation.x == 0)
        if(PlayerAnimationDir.transform.eulerAngles.z == 0)
        {
            PlayerAnimationDir.transform.eulerAngles = new Vector3(180, PlayerAnimationDir.transform.rotation.y, 0);
        }
        //else if (Mathf.Abs(transform.rotation.x) == 180)
        else if(Mathf.Abs(PlayerAnimationDir.transform.eulerAngles.z) == 180)
        {
            PlayerAnimationDir.transform.eulerAngles = new Vector3(0, PlayerAnimationDir.transform.rotation.y, 0);
        }
        
        this.GetComponent<Rigidbody2D>().gravityScale *= -1;
        GravityDir *= -1;
        // AdditionalGravity *= -1;
        // CorrectGravity *=-1;

        PhysicsSetAUP();
    }
    protected void GetItemAnimFalse()
    {
        DebugX.Log("GetItemAnimFalse에서 버블 켜짐!");
        DebugX.Log("열쇠 아이콘 사라짐!");
        
        // 24-05-29 붓 먹었을 때 붓이 무니 위에 계속 떠 있도록
        // if (StarIcon != null)
        //     if (StarIcon.activeSelf)
        //         StarIcon.SetActive(false);
        Ani.SetBool("GetItem", false);

        glassesAnim.SetBool("GetItem", false);
        hatAnim.SetBool("GetItem", false);
        maskAnim.SetBool("GetItem", false);

        GetItemMarker.SetActive(true);
        //24-05-28 플레이어 물감 먹었을 때 말풍선 + 물감 뜨도록 변경
        GetItemBubble.SetActive(true);
    }
    // protected void ProtectAnimation(){
    //     PlayerAnimationProtection=false;
    // }
    public void MoveSetter(int dir)
    {
        Move = dir;
    }
    public void ColorSetter(int color)
    {
        NowLayer = color;
        UIManager.PlayerLayerChanged(color);

        DebugX.Log("NowLayer: " + NowLayer + " Player_Active: " + Player_Active.name);

        Player_Active.GetComponent<SpriteRenderer>().enabled = false;
        
        // 애니메이션 버그 (아이템 먹었을 때 손 계속 들고 있는 애니메이션 해결을 위해 색이 바뀔 때 기존의 Animator의 모든 변수를 False로 바꿔준다.)
        Player_Active.GetComponent<Animator>().SetBool("IsAir", false);
        Player_Active.GetComponent<Animator>().SetBool("GetItem", false);
        Player_Active.GetComponent<Animator>().SetBool("IsWork", false);
        Player_Active.GetComponent<Animator>().SetBool("IsBoring", false);

        glassesAnim.SetBool("IsAir", false);
        glassesAnim.SetBool("GetItem", false);
        glassesAnim.SetBool("IsWork", false);
        glassesAnim.SetBool("IsBoring", false);

        hatAnim.SetBool("IsAir", false);
        hatAnim.SetBool("GetItem", false);
        hatAnim.SetBool("IsWork", false);
        hatAnim.SetBool("IsBoring", false);

        maskAnim.SetBool("IsAir", false);
        maskAnim.SetBool("GetItem", false);
        maskAnim.SetBool("IsWork", false);
        maskAnim.SetBool("IsBoring", false);

        // 색약모드 켰을 때 무니 애니메이터 바꾸기
        if(DataController.Instance.gameData.isColorFilterAssistant)
        {
            Player_Active = Player_Animation[NowLayer];   

            /*2024-08-05
                원래는 NowLayer - 1 이었지만, 중간에 Player_basic 이라는 오브젝트를 넣어서 [NowLayer]로 진행하면 되도록했다.
                뿐만 아니라 처음 코드가 작성될 때는 색약용 기본 플레이어가 없어서 if를 통해서 case를 나눴지만 이제는 모두 디자인이 들어가기 때문에 해당 코드를 삭제했다.
            */
        }
        
        else
        {
            Player_Active = Player_Animation[NowLayer - 5];
        }

        
        // 애니메이션 버그 (아이템 먹었을 때 손 계속 들고 있는 애니메이션 해결을 위해 색이 바뀔 때 기존의 Animator의 모든 변수를 False로 바꿔준다.)
        Player_Active.GetComponent<Animator>().SetBool("IsAir", false);
        Player_Active.GetComponent<Animator>().SetBool("GetItem", false);
        Player_Active.GetComponent<Animator>().SetBool("IsWork", false);
        Player_Active.GetComponent<Animator>().SetBool("IsBoring", false);

        glassesAnim.SetBool("IsAir", false);
        glassesAnim.SetBool("GetItem", false);
        glassesAnim.SetBool("IsWork", false);
        glassesAnim.SetBool("IsBoring", false);

        hatAnim.SetBool("IsAir", false);
        hatAnim.SetBool("GetItem", false);
        hatAnim.SetBool("IsWork", false);
        hatAnim.SetBool("IsBoring", false);

        maskAnim.SetBool("IsAir", false);
        maskAnim.SetBool("GetItem", false);
        maskAnim.SetBool("IsWork", false);
        maskAnim.SetBool("IsBoring", false);


        Player_Active.GetComponent<SpriteRenderer>().enabled = true;

        Ani = Player_Active.GetComponent<Animator>();
        DebugX.Log("ColorChangeCalled");

        int isCB = DataController.Instance.gameData.isColorFilterAssistant ? 1 : 2 ;
        ColorChangeEffector.SetInteger("isColorBlind",isCB);
        ColorChangeEffector.SetInteger("LayerNow",color);

        if (NowLayer == 5)
        {
            this.gameObject.layer = 10;
        }
        else
            this.gameObject.layer = NowLayer;

        // 2024-01-09 박스 끄는 도중 같은 색으로 바뀌었을때 간헐적으로 계속 끌리는 버그 해결
        if(pullingBoxes.Count > 0 && this.gameObject.layer == pullingBoxes[0].gameObject.layer - 7) {
            pullingBoxes[0].GetComponent<BoxPullTrigger>().SetFalseisPull();
        }

        // 2024-07-29 투명도가 바뀌는 Color Filter가 색약 모드가 아니라 초보자를 위한 기능으로 변경됨에 따라 isColorFilterAssistant -> isFullviewForNoob 변수로 변경.
        SetColorFilterAssistant(DataController.Instance.gameData.isFullviewForNoob);
        
        
    }

    //ColorSetter와 동일하지만, 특수 상황을 구분하기 위해서 bool 추가.
    public void ColorSetter(int color, bool tempSpecial)
    {
        //2024-12-03 시간 제한 특수 상황 적용하기
        if(tempSpecial){
            SpecialAlarmObj.SetActive(true);
            if(!specialPumpkin.isSpecial){
                specialPumpkin.prevColor = NowLayer;
            } 
            specialPumpkin.isSpecial = tempSpecial;
            specialPumpkin.specialTime = defaultSpecialTime;
            
            
        }else{
            /*
                이전 색상 저장하는 것을 special이 아닐 때 하는 이유.
                Y-> G* -> R*
                Y인 상태에서 G 특수 물통을 먹고 다시 R 특수 물통을 먹었을 때, 시간이 지나면 Y로 돌아가야 한다.

                따라서 상호작용한 물통이 특수물통이 아닐때만 prevColor 저장한다.
            */
            SpecialAlarmObj.SetActive(false);

            specialPumpkin.isSpecial = false;
            specialPumpkin.specialTime = 0f;
            specialPumpkin.prevColor = color;
        }


        NowLayer = color;
        UIManager.PlayerLayerChanged(color);

        DebugX.Log("NowLayer: " + NowLayer + " Player_Active: " + Player_Active.name);

        Player_Active.GetComponent<SpriteRenderer>().enabled = false;
        
        // 애니메이션 버그 (아이템 먹었을 때 손 계속 들고 있는 애니메이션 해결을 위해 색이 바뀔 때 기존의 Animator의 모든 변수를 False로 바꿔준다.)
        Player_Active.GetComponent<Animator>().SetBool("IsAir", false);
        Player_Active.GetComponent<Animator>().SetBool("GetItem", false);
        Player_Active.GetComponent<Animator>().SetBool("IsWork", false);
        Player_Active.GetComponent<Animator>().SetBool("IsBoring", false);

        glassesAnim.SetBool("IsAir", false);
        glassesAnim.SetBool("GetItem", false);
        glassesAnim.SetBool("IsWork", false);
        glassesAnim.SetBool("IsBoring", false);

        hatAnim.SetBool("IsAir", false);
        hatAnim.SetBool("GetItem", false);
        hatAnim.SetBool("IsWork", false);
        hatAnim.SetBool("IsBoring", false);

        maskAnim.SetBool("IsAir", false);
        maskAnim.SetBool("GetItem", false);
        maskAnim.SetBool("IsWork", false);
        maskAnim.SetBool("IsBoring", false);

        // 색약모드 켰을 때 무니 애니메이터 바꾸기
        if(DataController.Instance.gameData.isColorFilterAssistant)
        {
            Player_Active = Player_Animation[NowLayer];   

            /*2024-08-05
                원래는 NowLayer - 1 이었지만, 중간에 Player_basic 이라는 오브젝트를 넣어서 [NowLayer]로 진행하면 되도록했다.
                뿐만 아니라 처음 코드가 작성될 때는 색약용 기본 플레이어가 없어서 if를 통해서 case를 나눴지만 이제는 모두 디자인이 들어가기 때문에 해당 코드를 삭제했다.
            */
        }
        
        else
        {
            Player_Active = Player_Animation[NowLayer - 5];
        }

        
        // 애니메이션 버그 (아이템 먹었을 때 손 계속 들고 있는 애니메이션 해결을 위해 색이 바뀔 때 기존의 Animator의 모든 변수를 False로 바꿔준다.)
        Player_Active.GetComponent<Animator>().SetBool("IsAir", false);
        Player_Active.GetComponent<Animator>().SetBool("GetItem", false);
        Player_Active.GetComponent<Animator>().SetBool("IsWork", false);
        Player_Active.GetComponent<Animator>().SetBool("IsBoring", false);

        glassesAnim.SetBool("IsAir", false);
        glassesAnim.SetBool("GetItem", false);
        glassesAnim.SetBool("IsWork", false);
        glassesAnim.SetBool("IsBoring", false);

        hatAnim.SetBool("IsAir", false);
        hatAnim.SetBool("GetItem", false);
        hatAnim.SetBool("IsWork", false);
        hatAnim.SetBool("IsBoring", false);

        maskAnim.SetBool("IsAir", false);
        maskAnim.SetBool("GetItem", false);
        maskAnim.SetBool("IsWork", false);
        maskAnim.SetBool("IsBoring", false);


        Player_Active.GetComponent<SpriteRenderer>().enabled = true;

        Ani = Player_Active.GetComponent<Animator>();
        DebugX.Log("ColorChangeCalled");

        int isCB = DataController.Instance.gameData.isColorFilterAssistant ? 1 : 2 ;
        ColorChangeEffector.SetInteger("isColorBlind",isCB);
        ColorChangeEffector.SetInteger("LayerNow",color);

        if (NowLayer == 5)
        {
            this.gameObject.layer = 10;
        }
        else
            this.gameObject.layer = NowLayer;

        // 2024-01-09 박스 끄는 도중 같은 색으로 바뀌었을때 간헐적으로 계속 끌리는 버그 해결
        if(pullingBoxes.Count > 0 && this.gameObject.layer == pullingBoxes[0].gameObject.layer - 7) {
            pullingBoxes[0].GetComponent<BoxPullTrigger>().SetFalseisPull();
        }

        // 2024-07-29 투명도가 바뀌는 Color Filter가 색약 모드가 아니라 초보자를 위한 기능으로 변경됨에 따라 isColorFilterAssistant -> isFullviewForNoob 변수로 변경.
        SetColorFilterAssistant(DataController.Instance.gameData.isFullviewForNoob);
        
        
    }

    public void ColorSetterForGoRightBefore(int color, bool isItSpeical)
    {
        if(!isItSpeical){
            SpecialAlarmObj.SetActive(false);
        }else{
            SpecialAlarmObj.SetActive(true);
        }
        NowLayer = color;
        UIManager.PlayerLayerChanged(color);
        Player_Active.GetComponent<SpriteRenderer>().enabled = false;
        Player_Active.GetComponent<Animator>().SetBool("IsAir", false);
        Player_Active.GetComponent<Animator>().SetBool("GetItem", false);
        Player_Active.GetComponent<Animator>().SetBool("IsWork", false);
        Player_Active.GetComponent<Animator>().SetBool("IsBoring", false);

        glassesAnim.SetBool("IsAir", false);
        glassesAnim.SetBool("GetItem", false);
        glassesAnim.SetBool("IsWork", false);
        glassesAnim.SetBool("IsBoring", false);

        hatAnim.SetBool("IsAir", false);
        hatAnim.SetBool("GetItem", false);
        hatAnim.SetBool("IsWork", false);
        hatAnim.SetBool("IsBoring", false);

        maskAnim.SetBool("IsAir", false);
        maskAnim.SetBool("GetItem", false);
        maskAnim.SetBool("IsWork", false);
        maskAnim.SetBool("IsBoring", false);

        // 색약모드 켰을 때 무니 애니메이터 바꾸기
        if(DataController.Instance.gameData.isColorFilterAssistant)
        {
            Player_Active = Player_Animation[NowLayer];
            /*2024-08-05
                원래는 NowLayer - 1 이었지만, 중간에 Player_basic 이라는 오브젝트를 넣어서 [NowLayer]로 진행하면 되도록했다.
                뿐만 아니라 처음 코드가 작성될 때는 색약용 기본 플레이어가 없어서 if를 통해서 case를 나눴지만 이제는 모두 디자인이 들어가기 때문에 해당 코드를 삭제했다.
            */
            
        }
        
        else
        {
            Player_Active = Player_Animation[NowLayer - 5];
        }

        Player_Active.GetComponent<Animator>().SetBool("IsAir", false);
        Player_Active.GetComponent<Animator>().SetBool("GetItem", false);
        Player_Active.GetComponent<Animator>().SetBool("IsWork", false);
        Player_Active.GetComponent<Animator>().SetBool("IsBoring", false);

        glassesAnim.SetBool("IsAir", false);
        glassesAnim.SetBool("GetItem", false);
        glassesAnim.SetBool("IsWork", false);
        glassesAnim.SetBool("IsBoring", false);

        hatAnim.SetBool("IsAir", false);
        hatAnim.SetBool("GetItem", false);
        hatAnim.SetBool("IsWork", false);
        hatAnim.SetBool("IsBoring", false);

        maskAnim.SetBool("IsAir", false);
        maskAnim.SetBool("GetItem", false);
        maskAnim.SetBool("IsWork", false);
        maskAnim.SetBool("IsBoring", false);

        Player_Active.GetComponent<SpriteRenderer>().enabled = true;

        Ani = Player_Active.GetComponent<Animator>();
        if (NowLayer == 5)
        {
            this.gameObject.layer = 10;
        }  
        else
            this.gameObject.layer = NowLayer;
        
        // 2024-07-29 투명도가 바뀌는 Color Filter가 색약 모드가 아니라 초보자를 위한 기능으로 변경됨에 따라 isColorFilterAssistant -> isFullviewForNoob 변수로 변경.
        SetColorFilterAssistant(DataController.Instance.gameData.isFullviewForNoob);
    }

    public void ColorSetter(int color, bool tempSpecial, bool diode)
    {
        if(DataController.Instance.gameData.canLazerInteract){
            //2024-12-03 시간 제한 특수 상황 적용하기
            if(tempSpecial){
                SpecialAlarmObj.SetActive(true);
                if(!specialPumpkin.isSpecial){
                    specialPumpkin.prevColor = NowLayer;
                } 
                specialPumpkin.isSpecial = tempSpecial;
                specialPumpkin.specialTime = defaultSpecialTime;
                
                
            }else{
                /*
                    이전 색상 저장하는 것을 special이 아닐 때 하는 이유.
                    Y-> G* -> R*
                    Y인 상태에서 G 특수 물통을 먹고 다시 R 특수 물통을 먹었을 때, 시간이 지나면 Y로 돌아가야 한다.

                    따라서 상호작용한 물통이 특수물통이 아닐때만 prevColor 저장한다.
                */
                SpecialAlarmObj.SetActive(false);

                specialPumpkin.isSpecial = false;
                specialPumpkin.specialTime = 0f;
                specialPumpkin.prevColor = color;
            }


            NowLayer = color;
            UIManager.PlayerLayerChanged(color);

            DebugX.Log("NowLayer: " + NowLayer + " Player_Active: " + Player_Active.name);

            Player_Active.GetComponent<SpriteRenderer>().enabled = false;
            
            // 애니메이션 버그 (아이템 먹었을 때 손 계속 들고 있는 애니메이션 해결을 위해 색이 바뀔 때 기존의 Animator의 모든 변수를 False로 바꿔준다.)
            Player_Active.GetComponent<Animator>().SetBool("IsAir", false);
            Player_Active.GetComponent<Animator>().SetBool("GetItem", false);
            Player_Active.GetComponent<Animator>().SetBool("IsWork", false);
            Player_Active.GetComponent<Animator>().SetBool("IsBoring", false);

            glassesAnim.SetBool("IsAir", false);
            glassesAnim.SetBool("GetItem", false);
            glassesAnim.SetBool("IsWork", false);
            glassesAnim.SetBool("IsBoring", false);

            hatAnim.SetBool("IsAir", false);
            hatAnim.SetBool("GetItem", false);
            hatAnim.SetBool("IsWork", false);
            hatAnim.SetBool("IsBoring", false);

            maskAnim.SetBool("IsAir", false);
            maskAnim.SetBool("GetItem", false);
            maskAnim.SetBool("IsWork", false);
            maskAnim.SetBool("IsBoring", false);

            // 색약모드 켰을 때 무니 애니메이터 바꾸기
            if(DataController.Instance.gameData.isColorFilterAssistant)
            {
                Player_Active = Player_Animation[NowLayer];   

                /*2024-08-05
                    원래는 NowLayer - 1 이었지만, 중간에 Player_basic 이라는 오브젝트를 넣어서 [NowLayer]로 진행하면 되도록했다.
                    뿐만 아니라 처음 코드가 작성될 때는 색약용 기본 플레이어가 없어서 if를 통해서 case를 나눴지만 이제는 모두 디자인이 들어가기 때문에 해당 코드를 삭제했다.
                */
            }
            
            else
            {
                Player_Active = Player_Animation[NowLayer - 5];
            }

            
            // 애니메이션 버그 (아이템 먹었을 때 손 계속 들고 있는 애니메이션 해결을 위해 색이 바뀔 때 기존의 Animator의 모든 변수를 False로 바꿔준다.)
            Player_Active.GetComponent<Animator>().SetBool("IsAir", false);
            Player_Active.GetComponent<Animator>().SetBool("GetItem", false);
            Player_Active.GetComponent<Animator>().SetBool("IsWork", false);
            Player_Active.GetComponent<Animator>().SetBool("IsBoring", false);

            glassesAnim.SetBool("IsAir", false);
            glassesAnim.SetBool("GetItem", false);
            glassesAnim.SetBool("IsWork", false);
            glassesAnim.SetBool("IsBoring", false);

            hatAnim.SetBool("IsAir", false);
            hatAnim.SetBool("GetItem", false);
            hatAnim.SetBool("IsWork", false);
            hatAnim.SetBool("IsBoring", false);

            maskAnim.SetBool("IsAir", false);
            maskAnim.SetBool("GetItem", false);
            maskAnim.SetBool("IsWork", false);
            maskAnim.SetBool("IsBoring", false);


            Player_Active.GetComponent<SpriteRenderer>().enabled = true;

            Ani = Player_Active.GetComponent<Animator>();
            DebugX.Log("ColorChangeCalled");

            int isCB = DataController.Instance.gameData.isColorFilterAssistant ? 1 : 2 ;
            ColorChangeEffector.SetInteger("isColorBlind",isCB);
            ColorChangeEffector.SetInteger("LayerNow",color);

            if (NowLayer == 5)
            {
                this.gameObject.layer = 10;
            }
            else
                this.gameObject.layer = NowLayer;

            // 2024-01-09 박스 끄는 도중 같은 색으로 바뀌었을때 간헐적으로 계속 끌리는 버그 해결
            if(pullingBoxes.Count > 0 && this.gameObject.layer == pullingBoxes[0].gameObject.layer - 7) {
                pullingBoxes[0].GetComponent<BoxPullTrigger>().SetFalseisPull();
            }

            // 2024-07-29 투명도가 바뀌는 Color Filter가 색약 모드가 아니라 초보자를 위한 기능으로 변경됨에 따라 isColorFilterAssistant -> isFullviewForNoob 변수로 변경.
            SetColorFilterAssistant(DataController.Instance.gameData.isFullviewForNoob);
        
        }
    }
    public int ColorGetter()
    {
        return NowLayer;
    }
    public void JumpSetter()
    {
        if (!JumpProtector && IsGround) {
            Jump = true;
            DebugX.Log("점프세터에 의해 점프");
        }
            
    }
    public void InteractionSetter(bool input)
    {
        Interaction = input;
    }
    public bool InteractionGetter()
    {
        return Interaction;
    }
    public void SetUiPopOn()
    {
        UIPop = true;
    }
     public void SetUiPopOff()
    {
        UIPop = false;
    }
    
    public bool itemSetter(int color, bool tempisTubeSpecial)
    {
        if (item == 0 && color != 0)
        {
            // 이미 GetItemAnimFalse 가 진행중이라면 취소한다.
            CancelInvoke("GetItemAnimFalse");
            Ani.SetBool("GetItem", true);
            
            glassesAnim.SetBool("GetItem", true);
            hatAnim.SetBool("GetItem", true);
            maskAnim.SetBool("GetItem", true);

            DebugX.Log("itemSetter 열쇠 False");
            Invoke("GetItemAnimFalse", 0.1f);
            item = color;
            isTubeSpecial = tempisTubeSpecial;
            GetItemMarker.GetComponent<Player_GetItem>().ColorSetter(item, isTubeSpecial);
            return true;
        }
        else
            return false;

    }

    public void itemSetterForGoRightBefore(int color){
        item = color;
    }

    //2023-01-03 OYJ GoRightBefore.cs에 연결
    public int itemGetter()
    {
        int re = item;
        item = 0;

        //item이 없기 때문에 Player 의 isTubeSpecial은 false로 만든다.
        isTubeSpecial = false;
        
        GetItemMarker.GetComponent<Player_GetItem>().ColorSetter(item, isTubeSpecial);
        return re;
    }
    public void keySetter()
    {
        // 이미 GetItemAnimFalse 가 진행중이라면 취소한다.
        CancelInvoke("GetItemAnimFalse");
        DebugX.Log("열쇠 먹음!!");
        Ani.SetBool("GetItem", true);

        glassesAnim.SetBool("GetItem", true);
        hatAnim.SetBool("GetItem", true);
        maskAnim.SetBool("GetItem", true);

        DebugX.Log("keySetter 열쇠 False");
        Invoke("GetItemAnimFalse", 0.1f);
        key = true;
        
        if (StarIcon != null)
        {
            StarIcon.SetActive(true);
        }

        DebugX.Log("키세터 작동");
    }

    public void KeyUnSetter(){
        DebugX.Log("keyUnSetter에서 버블 켜짐!");
        key = false;
        GetItemMarker.SetActive(true);
        //24-05-28 플레이어 물감 먹었을 때 말풍선 + 물감 뜨도록 변경
        GetItemBubble.SetActive(true);
        

        if (StarIcon != null)
        {
            StarIcon.SetActive(false);
        }
    }
    public bool GetKeyValue(){
        return key;
    }
    public bool keyGetter()
    {
        if (key)
        {
            // 24-05-29 좌상단 붓 UI 삭제
            // if(StarIconUI == null) {
            //     StarIconUI = GameObject.Find("Run").transform.Find("Star").gameObject;
            // }
            // if (StarIconUI != null) {
            //     StarIconUI.SetActive(false);
            // }
            return true;
        }
        else
            return false;
    }

    public bool GroundChecker()
    {
        return IsGround;
    }

    public bool InteractionChecker() {
        return isInteraction;
    }
    public bool MoveChecker(){
        if(Move == 0)
            return false;
        else
            return true;
    }
    public void Pause(){
          Time.timeScale = 0f;
    }

    public void Play(){
        Time.timeScale = 1f;
    }

    public int GetKeyCount() {
        return keyCount;
    }

    public void SetKeyCount(int keycount) {
        keyCount = keycount;
    }

    public void MoblieInputSetter(bool a) {
        MobileInput = a;
    }

    public void SetRBZero(){
        rb = this.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector3(0, 0, 0);

        //뒤로가기 했을 때 선입력된 점프 값 다 초기화
        isPressedJump = 0;
    }

    public void SetSpeedByBox(float tempboxspeed){
        if(tempboxspeed < 0){
            tempboxspeed = 0;
        }
        speedByBox = tempboxspeed;
    }

    public void SetisGetbox(bool tempboxGet){
        isGetbox = tempboxGet;
    }

    public GameObject GetgetBoxobj(){
        return getBoxObj;
    }
    public void SetPlayerLock(bool isLock)
    {
         DataController.Instance.gameData.playerCanMove= isLock;
    }

    public void SetColorFilterAssistant(bool isColorFilterAssistantByBeginner) {
        DebugX.Log("isColorFilterAssistantByBeginner: " + isColorFilterAssistantByBeginner);

        if(colorFilterGO.Length == 0) {
            // FindGameObjectsByColor();
            StartCoroutine(FindGameObjectsByColor(0.1f));

            if(isColorFilterAssistantByBeginner) {
                StartCoroutine(SetColorFilterByColor(this.gameObject.layer, 0.5f));
            }
            else {
                StartCoroutine(SetColorFilterByColor(10, 0.5f));
            }
        }

        else {
            if(isColorFilterAssistantByBeginner) {
                StartCoroutine(SetColorFilterByColor(this.gameObject.layer, 0.0f));
            }
            else {
                StartCoroutine(SetColorFilterByColor(10, 0.0f));
            }
        }
        

        
    }

    //24-04-28 ColorFilterController 갖고 있는 객체들 싹 다 찾음
    public IEnumerator FindGameObjectsByColor(float delayTime) {
        var wait = new WaitForSeconds(delayTime);

        yield return wait;

        var foundColorFilterObj = FindObjectsOfType<ColorFilterController>(true);
        colorFilterGO = foundColorFilterObj;

        DebugX.Log("컬러 필터 객체 찾기 완료!");
    }


    //24-04-28 ColorFilterController 갖고 있는 객체들 싹 다 찾음
    public IEnumerator SetColorFilterByColor(int layer, float delayTime) {
        var wait = new WaitForSeconds(delayTime);
        yield return wait;

        int l = colorFilterGO.Length;

        for(int i = 0; i < l; i++) {
            if(colorFilterGO[i].layer == layer) {
                colorFilterGO[i].SetColorFilter(0.5f);
            }
            else {
                colorFilterGO[i].SetColorFilter(1.0f);
            }
        }

        DebugX.Log("컬러 필터 객체 적용 완료!");
    }

    // 24-05-29 스테이지 클리어 시 무니 위에 떠 있는 객체 (붓, 물감, 말풍선) 모두 꺼주기
    public void OnClearStage(){
        key = false;
        GetItemMarker.SetActive(false);
        GetItemBubble.SetActive(false);

        // 24-08-07 스테이지 클리어 시 만세 애니메이션 출력
        Player_Active.GetComponent<Animator>().SetBool("IsAir", false);
        Player_Active.GetComponent<Animator>().SetBool("IsWork", false);
        Player_Active.GetComponent<Animator>().SetBool("IsBoring", false);
        Player_Active.GetComponent<Animator>().SetBool("GetItem", true);
        

        glassesAnim.SetBool("IsAir", false);
        glassesAnim.SetBool("IsWork", false);
        glassesAnim.SetBool("IsBoring", false);
        glassesAnim.SetBool("GetItem", true);

        hatAnim.SetBool("IsAir", false);
        hatAnim.SetBool("IsWork", false);
        hatAnim.SetBool("IsBoring", false);
        hatAnim.SetBool("GetItem", true);

        maskAnim.SetBool("IsAir", false);
        maskAnim.SetBool("IsWork", false);
        maskAnim.SetBool("IsBoring", false);
        maskAnim.SetBool("GetItem", true);

        if (StarIcon != null)
        {
            StarIcon.SetActive(false);
        }
    }

    public void ChangeStarIconPosition(float tempx, float tempy){
        StarIcon.transform.position = new Vector3( tempx, tempy, 0);
    }

    public float GravityGetter() {
        return GravityDir;
    }

    public void SetPumpkinSpecial(SpecialPumpkin tempSpecialPumpkin){
        specialPumpkin.isSpecial = tempSpecialPumpkin.isSpecial;
        specialPumpkin.specialTime = tempSpecialPumpkin.specialTime;
        specialPumpkin.prevColor = tempSpecialPumpkin.prevColor;
    }

    public SpecialPumpkin GetSpecialPumpkin(){
        return specialPumpkin;
    }

    public void SetMoveSpeed(){
        if(moveSpeecCR != null){
            StopCoroutine(moveSpeecCR);
        }
        
        MoveSpeed = 0;
        moveSpeecCR = SetRollBackMoveSpeed();
        StartCoroutine(moveSpeecCR);
    }
    public IEnumerator SetRollBackMoveSpeed() {
        var wait = new WaitForSeconds(0.1f);

        yield return wait;

        MoveSpeed = MoveSpeed_backup;
    }
}

