using UnityEngine;
using System.Collections;

public class DynamicCrosshair : MonoBehaviour {

    

    public bool HideOnFirstPersonZoom = true;
    public bool HideOnDeath = true;

    public vp_FPPlayerEventHandler Player = null;
    public vp_FPInput input;

    protected virtual void Awake()
    {
        Player = GameObject.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler; // cache the player event handler
    }

    protected virtual void OnEnable()
    {
        if (Player != null)
            Player.Register(this);
    }
    protected virtual void OnDisable()
    {
        if (Player != null)
            Player.Unregister(this);
    }

    public enum preset { none, shotgunPreset, crysisPreset }
    public preset crosshairPreset = preset.none;

    public bool showCrosshair = true;
    public Texture2D verticalTexture;
    public Texture2D horizontalTexture;

    public float cLength = 10;
    public float cWidth = 3;
 
    //Spreed setup
    public float minSpread = 45.0f;
    public float maxSpread = 250.0f;
    public float spreadPerSecond = 150.0f;
 
    //Rotation
    public float rotAngle = 0.0f;
    public float rotSpeed = 0.0f;
 
    private float wantedSpread = 0f;
    private static float spread = 0f;


    public static float Spread
    {
        get
        {
            return spread * 0.05f;
        }
    }

    public float SpreadForVelocity = 2.0f;
    public float SpreadForMouseMovement = 1.0f;
    public float SpreadWhenAttacking = 10.0f;

    public float SpreadDamping = 2f;

    public void OnMessage_Fired()
    {
        SetSpread(SpreadWhenAttacking);
    }

    void Update()
    {
        SetSpread(Player.Velocity.Get().magnitude * SpreadForVelocity);

        ChangeSpread(spreadPerSecond * Time.deltaTime);
        rotAngle += rotSpeed * Time.deltaTime;

        UpdateSpread();
    }

    public void SetSpread(float value)
    {
        wantedSpread = Mathf.Max(wantedSpread, value);
    }

    public void ChangeSpread(float value)
    {
        wantedSpread = Mathf.Min(Mathf.Max(wantedSpread + value, minSpread), maxSpread);
    }

    public void UpdateSpread()
    {
        spread = Mathf.Lerp(spread, wantedSpread, Time.deltaTime * SpreadDamping);
    }


    void OnMessage_FallImpact(float value)
    {
        SetSpread(value * SpreadForVelocity);
    }

    void OnMessage_AimMovement(Vector3 value)
    {
        SetSpread(value.magnitude * SpreadForMouseMovement);
    }

    private Texture temp = null;

    GUIStyle verticalT;
    GUIStyle horizontalT;

    void OnGUI()
    {
        if (HideOnDeath && Player.Dead.Active)
            return;
        if (input && input.MouseCursorForced)
            return;


        if (verticalT == null)
        {
            verticalT = new GUIStyle();
            verticalT.normal.background = verticalTexture;
        }
        if (horizontalT == null)
        {
            horizontalT = new GUIStyle();
            horizontalT.normal.background = horizontalTexture;
        }

        float BoxSize = 4f;
        GUI.Box(new Rect((Screen.width) / 2 - BoxSize / 2f, (Screen.height) / 2 - BoxSize / 2f, BoxSize, BoxSize), temp, horizontalT);

        if (HideOnFirstPersonZoom && Player.Zoom.Active && Player.IsFirstPerson.Get())
            return;

        

        

        if(showCrosshair && verticalTexture && horizontalTexture){
                
                spread = Mathf.Clamp(spread, minSpread, maxSpread);
                Vector2 pivot  = new Vector2(Screen.width/2, Screen.height/2);
               
                if(crosshairPreset == preset.crysisPreset){
                       
                        GUI.Box(new Rect((Screen.width - 2)/2, (Screen.height - spread)/2 - 14, 2, 14), temp, horizontalT);
                        GUIUtility.RotateAroundPivot(45,pivot);
                        GUI.Box(new Rect((Screen.width + spread) / 2, (Screen.height - 2) / 2, 14, 2), temp, verticalT);
                        GUIUtility.RotateAroundPivot(0,pivot);
                        GUI.Box(new Rect((Screen.width - 2) / 2, (Screen.height + spread) / 2, 2, 14), temp, horizontalT);
                }
               
                if(crosshairPreset == preset.shotgunPreset){
               
                        GUIUtility.RotateAroundPivot(45,pivot);
                       
                        //Horizontal
                        GUI.Box(new Rect((Screen.width - 14) / 2, (Screen.height - spread) / 2 - 3, 14, 3), temp, horizontalT);
                        GUI.Box(new Rect((Screen.width - 14) / 2, (Screen.height + spread) / 2, 14, 3), temp, horizontalT);
                        //Vertical
                        GUI.Box(new Rect((Screen.width - spread) / 2 - 3, (Screen.height - 14) / 2, 3, 14), temp, verticalT);
                        GUI.Box(new Rect((Screen.width + spread) / 2, (Screen.height - 14) / 2, 3, 14), temp, verticalT);
                }
               
                if(crosshairPreset == preset.none){
               
                        GUIUtility.RotateAroundPivot(rotAngle%360,pivot);
                       
                        //Horizontal
                        GUI.Box(new Rect((Screen.width - cWidth) / 2, (Screen.height - spread) / 2 - cLength, cWidth, cLength), temp, horizontalT);
                        GUI.Box(new Rect((Screen.width - cWidth) / 2, (Screen.height + spread) / 2, cWidth, cLength), temp, horizontalT);
                        //Vertical
                        GUI.Box(new Rect((Screen.width - spread) / 2 - cLength, (Screen.height - cWidth) / 2, cLength, cWidth), temp, verticalT);
                        GUI.Box(new Rect((Screen.width + spread) / 2, (Screen.height - cWidth) / 2, cLength, cWidth), temp, verticalT);
                }
        }


    }

}
