using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer))]
public class AnimationPlayer : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    [Range(1,30)]public int fps = 3;
    [SerializeField] bool playOnAwake = false;
    public enum LoopType{None,Restart,Yoyo}
    [SerializeField]  LoopType loopType = LoopType.Restart;
    public bool IsPlaying{get{return playing;}}
    public int CurrentFrame{get{return index;}}
    [SerializeField] bool onlyRunsInGameplayState = true;
    [SerializeField] bool ignoreTimeScale = false;
    int index = 0;
    int sens = 1;
    float chrono = 0;
    SpriteRenderer sprite=>GetComponent<SpriteRenderer>();
    bool playing = false;
    public Action OnAnimEnd;




    public void Play()
    {
        ApplySprite(0);
        Resume();
    }

    public void Resume()
    {
        playing = true;
    }

    public void Stop()
    {
        Pause();
        ApplySprite(0);
    }

    public void Pause()
    {
        playing = false;
    }


    public void Flip(bool wanted)
    {
        sprite.flipX = wanted;
    }

    public void Flip(bool xWanted, bool yWanted)
    {
        sprite.flipX = xWanted;
        sprite.flipY = yWanted;
    }


    public void ApplySprite(int desired)
    {
        index = Mathf.Clamp(desired,0,sprites.Length-1);
        sprite.sprite = sprites[index];
    }













    void Awake()
    {
        if (this.enabled==false || sprites.Length<1 || sprite==null)
        {
            Destroy(this);
            return;
        }
        playing = playOnAwake;
        ApplySprite(0);
        if (sprites.Length<2) this.enabled = false;
    }


    void Update()
    {
        if (playing==false) return;
        //if (!onlyRunsInGameplayState || GAME.MANAGER.CurrentState==State.gameplay)
        chrono+= ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        if (chrono> 1f/fps) NextSprite();
    }



    public void NextSprite()
    {
        chrono = 0;
        index+= sens;
        if (index>=sprites.Length)
        {
            switch (loopType)
            {
                case LoopType.Restart:index = 0;break;
                case LoopType.Yoyo:index-=2;sens=-1;break;
                case LoopType.None: playing = false; enabled=false;break;
            }
            OnAnimEnd?.Invoke();
        }
        if (index<0)
        {
            index = 1;
            sens=1;
            OnAnimEnd?.Invoke();
        }

        ApplySprite(index);
    }




    void OnValidate()
    {
        // Delay the sprite change until after OnValidate completes
        #if UNITY_EDITOR
        AnimationPlayer instance = this;
        EditorApplication.delayCall += () => { if (instance!=null) if (sprite!=null && sprites!=null && sprites.Length>0 && sprite.sprite!=sprites[0]) sprite.sprite=sprites[0]; };
        #endif
    }
} // FIN DU SCRIPT
