/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Text;

namespace monoflow { 

    public class MPMP_ugui_Element : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler
    {
        public enum Mode { LOAD, PLAY,PAUSE,STOP,SEEK,LOOP,AUTOPLAY,VOLUME,TEXTURE,PLAYBACKRATE ,TIME,PATH} 
        public Mode mode;
        public monoflow.MPMP player;
        public Action OnSelected;
        public Action<float> OnDraged;
        public Action<double,double> OnUpdateTime;
        public Action OnUpdate;
        [HideInInspector]
        public string timeText = "{0}:{1} / {2}:{3}";

        //corresponding colors for the Mode
        private Color[] colors = { new Color(1f, 0f, 0f, 1f), new Color(0f, 1f, 0f, 1f), new Color(1f, 0.5f, 0f, 1f), Color.black, Color.black, Color.black };
        private Button _button;
        private Toggle _toggle;
        private Slider _slider;

        private Text _text;
        private StringBuilder _timeSB;
        private int minDur, minPos, secDur, secPos;

        private InputField _inputField;
        private string _oldPath, _curPath;

        private RawImage _rawImage;
        private Texture _texture;
        private bool _isNewTexture;

        private ColorBlock _colorBlock;
        private EventSystem _eventSystem;
        private bool _isPointerDown;
        private bool _isPlaying;

    
        void Awake()
        {
       
        }
        void Start ()
        {
		    //Debug.Log("Start");
            if (player == null) return;
            _eventSystem = FindObjectOfType<EventSystem>();

            _button = GetComponent<Button>();
            if (_button)
            {
                _colorBlock = _button.colors;
            }


            switch (mode)
            {
                case Mode.LOAD:
                    OnSelected = new Action(() => { player.Load(); });
                    break;
                case Mode.PLAY:
                    OnSelected = new Action(() => { player.Play(); });
                    break;
                case Mode.PAUSE:
                    OnSelected = new Action(() => { player.Pause(); });
                    break;
                case Mode.STOP:
                    OnSelected = new Action(() => { player.Stop(); });
                    break;
                case Mode.SEEK:
                    _slider = GetComponent<Slider>();
                    OnDraged = new Action<float>((f) => { player.SeekTo(f,true); });
                    break;
                case Mode.VOLUME:
                    _slider = GetComponent<Slider>();
                    OnDraged = new Action<float>((f) => { player.volume = f; });
                    break;
                case Mode.LOOP:
                    _toggle = GetComponent<Toggle>();
                    if (_toggle)
                        OnSelected = new Action(() => { player.looping = !_toggle.isOn; });
                    break;
                case Mode.AUTOPLAY:
                    _toggle = GetComponent<Toggle>();
                    if (_toggle)
                        OnSelected = new Action(() => { player.autoPlay = !_toggle.isOn; });
                    break;
                case Mode.TEXTURE:
                    _rawImage = GetComponent<RawImage>();
                    if (_rawImage) {                  

#if !UNITY_ANDROID || UNITY_EDITOR
                        Rect uvr = _rawImage.uvRect;
                        //#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN					                       
                        //uvr.height *= -1f;                                   
                        //#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_IOS				   
                        uvr.y = 1f;
				        uvr.height *= -1f;
                        //#endif
                        _rawImage.uvRect = uvr;
#endif

                    }

                    break;
                case Mode.PLAYBACKRATE:
                    _slider = GetComponent<Slider>();
                    OnDraged = new Action<float>((f) => { player.rate = f; });
                    break;

                case Mode.TIME:
                    _text = GetComponent<Text>();
                    _timeSB  = new StringBuilder();
                    if (_text != null)
                    {
                        OnUpdateTime = new Action<double, double>((pos, dur) => {
                           
                            _timeSB.Length = 0;
                            _timeSB.Capacity = 0;
                          
                            try {
                                minPos = (int)Mathf.Floor(Convert.ToInt32(pos * dur) / 60f);
                                secPos = (int)(Convert.ToInt32(pos * dur) - (minPos * 60));

                                minDur = (int)Mathf.Floor(Convert.ToInt32(dur) / 60f);
                                secDur = (int)(Convert.ToInt32(dur) - (minDur * 60));
                            }catch (Exception)
                            {

                            };
                          
                            /*
                            if (secPos < 10)
                            {
                                _timeSB.AppendFormat("{0}:0{1} / {2}:{3}", minPos, secPos, minDur, secDur);
                            }
                            else
                            {
                                _timeSB.AppendFormat("{0}:{1} / {2}:{3}", minPos, secPos, minDur, secDur);
                            }
                            */

                            _timeSB.AppendFormat(timeText, minPos, secPos, minDur, secDur);
                            _text.text = _timeSB.ToString();                         

                            //Debug.Log(String.Format("pos:{0}, dur:{1}", pos,dur));
                        });
                    }
                   
                    break;
                case Mode.PATH:
                    _inputField = GetComponent<InputField>();
                    if(_inputField != null)
                    {
                        _curPath = _oldPath = _inputField.text = player.videoPath;

                        OnUpdate = new Action(()=> {
                            _curPath = _inputField.text;
                            if (_curPath != _oldPath)
                            {
                                //input field change
                                player.videoPath = _curPath;
                            }else if (player.videoPath != _curPath) {
                                //videopath was changed
                                _curPath = _inputField.text  = player.videoPath;
                            }
                            _oldPath = _curPath;
                            });
                       
                    }
                    
                    break;
            }

        }
	
	    // Update is called once per frame
	    void Update () {
            if (player == null) return;
            switch (mode)
            {
                case Mode.SEEK:
                    if (_slider && !_isPointerDown)
                    {                     
                        try {
                            _slider.value = Mathf.Clamp01(System.Convert.ToSingle(player.GetCurrentPosition(true)));
                        }
                        catch (Exception)
                        {
                            Debug.Log("ERROR Converting double to float:");
                        }
                      
                        //Debug.Log("GetCurrentPosition:" + _slider.value);
                    }
                    if (_slider && _isPointerDown)
                    {
                        _UpdateDrag();
                    }
                    break;
                case Mode.VOLUME:
                    if (_slider && !_isPointerDown)
                    {
                        _slider.value = player.volume;
                    }
                    if (_slider && _isPointerDown)
                    {
                        _UpdateDrag();
                    }
                    break;

                case Mode.PLAY:        
                    if (_button) {
                        if (player.IsPlaying())
                        {
                           // _colorBlock.normalColor = _colorBlock.highlightedColor = colors[(int)mode];
                            _colorBlock.normalColor = colors[(int)mode];
                            _colorBlock.highlightedColor = colors[(int)mode];
                        }
                    else
                        {
                        _colorBlock.normalColor = _colorBlock.highlightedColor = Color.white;
                        }
                    _button.colors = _colorBlock;
                    }
                    break;
                
                case Mode.PAUSE:
                    if (_button) {
                        if (player.IsPaused())
                        {
                            _colorBlock.normalColor = _colorBlock.highlightedColor = colors[(int)mode];
                        } else
                        {
                            _colorBlock.normalColor = _colorBlock.highlightedColor = Color.white;
                        }
                        _button.colors = _colorBlock;
                    }
                    break;
                case Mode.LOAD:
                    if (_button)
                    {
                        if (player.IsLoading())
                        {
                            _colorBlock.normalColor = _colorBlock.highlightedColor = colors[(int)mode];
                        }
                        else
                        {
                            _colorBlock.normalColor = _colorBlock.highlightedColor = Color.white;
                        }
                        _button.colors = _colorBlock;
                    }
                    break;
                case Mode.AUTOPLAY:
                    _toggle.isOn = player.autoPlay;
                    break;
                case Mode.LOOP:
                    _toggle.isOn = player.looping;
                    break;

                case Mode.TEXTURE:
                    if (_rawImage)
                    {
                      
                        _texture = (Texture)player.GetVideoTexture();
                        _isNewTexture = _rawImage.texture != _texture;
                         if (_texture != null) { }
                        if (_isNewTexture){ }
                        _rawImage.texture = _texture;
                        _rawImage.SetAllDirty();//!!!!

                    }
               
                    break;

                case Mode.PLAYBACKRATE:
                    if (_slider && !_isPointerDown)
                    {
                        _slider.value = player.rate;
                    }
                    if (_slider && _isPointerDown)
                    {
                        _UpdateDrag();
                    }
                    break;
                case Mode.TIME:
                    //_Update();
                    if (OnUpdateTime != null) OnUpdateTime(player.GetCurrentPosition(true), player.GetDuration());
                    break;
                case Mode.PATH:
                    if (OnUpdate != null) OnUpdate();                                   
                    break;
            }


           if(mode == Mode.PATH) { return; }

           //Deselect the current GUI element to display the right color state
            if (_eventSystem)
            {  
               if (_eventSystem.currentSelectedGameObject == gameObject)
                {
                    //Debug.Log("RESET");
                    _eventSystem.SetSelectedGameObject(null);
                }
           
            } 
	    }

        public void OnPointerEnter(PointerEventData eventData)
        {
             //Debug.Log("OnPointerEnter:");         
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("OnPointerExit:");
        }
        public void OnPointerDown(PointerEventData eventData)
        {
           // Debug.Log("OnPointerDown:");
            _isPointerDown = true;

            if (mode == Mode.SEEK)
            {
                player.SetSeeking(true);
                _isPlaying = player.IsPlaying();
                if (_isPlaying){player.Pause();}
            }

            _UpdateDrag();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Debug.Log("OnPointerUp:"+ eventData);
            _isPointerDown = false;
            if (OnSelected != null) OnSelected();

            _UpdateDrag();

            if (mode == Mode.SEEK)
            {
                player.SetSeeking(false);
                if (_isPlaying) { player.Play(); }
               #if (UNITY_ANDROID && !UNITY_EDITOR)             
                StartCoroutine(_SeekDelay());
                #endif
            }
              
        }
        
        private IEnumerator _SeekDelay()
        {
            //On Android we can use the AwesomePlayer or the NuPlayer. 
            //There is a bug with the NuPlayer when seeking while we are pausing. 
            //If we call a seekTo with a short delay after OnPointerUp we can fix this issue so the player will update after the seek the right way.
            //If you have any problems you can increase the WaitForSeconds time
            yield return new WaitForSeconds(0.05f);
            //Debug.Log("_SeekDelay");
            _UpdateDrag();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            // Debug.Log("OnDrag:"+ eventData);   
           // _UpdateDrag();
        }

        private void _UpdateDrag()
        {
            if (mode == Mode.SEEK || mode == Mode.VOLUME || mode == Mode.PLAYBACKRATE)
            {
                if (_slider)
                {
                    // Debug.Log("_Seek:" + _slider.value);
                    if (OnDraged != null) OnDraged(_slider.value);
                }
            }
        }

       
   
    }
}


