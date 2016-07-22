/*
* MPMP
* Copyright © 2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
* WebGL code is based on 'Simple MovieTextures for Unity WebGL' demo from the Assetstore. https://www.assetstore.unity3d.com/en/#!/content/38369
*/
var LibraryMPMPWebGL = {

$videoInstances: [],
$videoTextures:[],
$JsCallCsTest: {
//var videoInstances = [];
},


NewMPMP: function(isLinear){
 //console.log("NewMPMP");
var video = document.createElement('video');
 //console.log("NewMPMP.B");
	video.style.display = 'none';
    //state: IDLE,INITIALIZED,PREPARING,PREPARED,STARTED,PAUSED,PLAYBACKCOMPLETED,STOPPED,END,ERROR
    //Events: LOAD(0), LOADED(1), PLAY(2), PAUSE(3), STOP(4), DESTROY(5),ERROR(6),PLAYBACKCOMPLETED(7),AVF_PIXELBUFFER_ERROR(8),TEXTURE_CHANGED(9),BUFFERING(10);
    //http://www.w3schools.com/tags/ref_av_dom.asp
    //http://www.w3schools.com/jsref/dom_obj_event.asp
    
    video.onended = function(){
    //alert(" video ended");
     console.log("onended");
     video.state = "PLAYBACKCOMPLETED";
    Runtime.dynCall('vii', video.callback, [video.id,7]);
    };
    
     video.onloadedmetadata  = function(){
     console.log("onloadedmetadata");
     video.state = "PREPARED";
    Runtime.dynCall('vii', video.callback, [video.id,1]);
    };
     video.onloadeddata  = function(){
        //console.log("onloadeddata");
    };
    video.onprogress  = function(){
         //  console.log("onprogress");
        //Runtime.dynCall('vii', video.callback, [video.id,10]);
    };
     video.oncanplay  = function(){
        //alert("oncanplay");
    };
     video.oncanplaythrough  = function(){
        //alert("canplaythrough");
        //Runtime.dynCall('vii', video.callback, [video.id,14]);
    };
    
    
    video.onplay = function(){
        //alert("play");
        //Runtime.dynCall('vii', video.callback, [video.id,2]);
        if(video.state == "PREPARING" || video.state == "PREPARED") video.state = "STARTED";  
        };
        
        video.onpause = function(){
        //video.state = "PAUSE";
        //Runtime.dynCall('vii', video.callback, [video.id,3]);
     };
 
    
    videoTextures.push(0);
    var id = videoInstances.push(video) - 1;
     console.log("NewMPMP:"+id);
     video.id = id;//"video_"+id;
    return id;
},

SetCallbackFunction: function(id, obj){
      //console.log("SetCallbackFunction:"+id);
      //console.log(obj);
      if(videoInstances[id] == null)return;
      videoInstances[id].callback = obj;
 
},

Load: function(id, path){
    console.log("Load:"+id);
    //console.log("Load:"+path);
    var video =  videoInstances[id];
    if(video == null)return;
    if(video.played){video.pause();}
    video.src=null;
    video.state = "PREPARING";
    var str = Pointer_stringify(path);
    console.log("Load:"+str);
    video.src = str;
    video.load();
},

Play: function(id){
    console.log("Play:"+id);
    var video =  videoInstances[id];
    if(video == null)return;
    video.state = "STARTED";
    video.play();
    //videoInstances[id].play().then(function(){videoInstances[id].playflag = false;},null);
    Runtime.dynCall('vi', video.callback, [video.id,2]);
},

Pause: function(id){
    console.log("Pause:"+id);
    var video =  videoInstances[id];
    if(video == null)return;
    //if (videoInstances[id].playflag == true) return;
    video.state = "PAUSED";
    video.pause();
    Runtime.dynCall('vi', video.callback, [video.id,3]);
 
},

Stop: function(id){
    console.log("Stop:"+id);
    var video =  videoInstances[id];
    if(video == null)return;
    video.state = "STOPPED";
    video.pause();
    video.currentTime =0;
    Runtime.dynCall('vi', video.callback, [video.id,4]);
},

SetAutoPlay: function(id, status){
    if(videoInstances[id] == null)return;
    videoInstances[id].autoplay = status;
},

SeekTo: function(id, t, normalized){
    if(videoInstances[id] == null)return;
    if(normalized == true){
        videoInstances[id].currentTime = Math.floor(t * videoInstances[id].duration);
        }else{
        videoInstances[id].currentTime = Math.floor(t);
        }
},

SetVolume: function(id, t){
    if(videoInstances[id] == null)return;
    videoInstances[id].volume = t;
},

SetBalance: function(id, fbal){
    if(videoInstances[id] == null)return;
    //videoInstances[id].play();
},

SetLooping: function(id, status){
    if(videoInstances[id] == null)return;
    videoInstances[id].loop = status;
},

SetPlaybackRate: function(id, rate){
    if(videoInstances[id] == null)return;
    videoInstances[id].playbackRate = rate;
},

SetAudioTrack: function(id, track){
    if(videoInstances[id] == null)return;
},

HasAudioTrack: function(id, track){
    if(videoInstances[id] == null)return false;
    return false;
},

Destroy: function(id){
    //alert("Destroy:"+id);
    console.log("Native.WebGL.Destroy:"+id);
    if(videoInstances[id] == null)return;
    //videoInstances[id].parentNode.removeChild(videoInstances[id]);
    //videoInstances[id].parentElement.removeChild(videoInstances[id]);
    //"video_"+id;
    //var node = document.getElementById(videoInstances[id].id);
    var video = videoInstances[id];
    video.id = null;
    video.callback = null;


    var index = videoInstances.indexOf(video);
    if(index == -1){
    //nothing
    }else{
    videoTextures[index] = 0;
    }



    video.onended = null;
    video.onloadedmetadata  = null;
    video.onloadeddata  = null;
    video.onprogress  = null;
    video.oncanplay  = null;
    video.oncanplaythrough  = null;    
    video.onplay = null;   
    video.onpause = null;
    video.state = null;
    video.src = null;
    //video.load();

    videoInstances[id] = null;
},

GetNativeTexture: function(id){
    if(videoInstances[id] == null)return 0;
    var instance = videoInstances[id];
    var index = videoInstances.indexOf(instance);
    return videoTextures[index];
    /*
    var index = videoInstances.indexOf(id);
    if(index == -1){
    return 0;
    }else{
    return videoTextures[index];
    }
    */
},

SetNativeTextureID: function(id, texid)
{
    console.log("SetNativeTextureID:"+id+" : "+texid);
    var instance = videoInstances[id];
    var index = videoInstances.indexOf(instance);
    if(index == -1){
    //nothing
    }else{
    videoTextures[index] = texid;
    }
    videoTextures[index] = texid;
},

UpdateNativeTexture: function(id, tex)
{
    if(videoInstances[id] == null)return;
    if(videoInstances[id].readyState < videoInstances[id].HAVE_CURRENT_DATA)return;
    
    if(tex <=0)return;

    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[tex]);
    //GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true);
    GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, videoInstances[id]);
    //GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);
},

GetNativeVideoSizeW: function(id)
{
    if(videoInstances[id] == null)return 0;
    return videoInstances[id].videoWidth;
},

GetNativeVideoSizeH: function(id)
{
    if(videoInstances[id] == null)return 0;
    return videoInstances[id].videoHeight;

},
GetCurrentPosition: function(id)
{
    if(videoInstances[id] == null)return 0;
    return videoInstances[id].currentTime;
},

GetDuration: function(id)
{
    if(videoInstances[id] == null)return 0;
    return videoInstances[id].duration;
},

GetCurrentVolume: function(id){
    if(videoInstances[id] == null)return 0;
    return videoInstances[id].volume;
},

IsPlaying: function(id){
    if(videoInstances[id] == null) return false;
    return videoInstances[id].state == "STARTED";
},

IsPaused: function(id){
    if(videoInstances[id] == null) return false;
    return videoInstances[id].state == "PAUSED";
},

IsStopped: function(id){
    if(videoInstances[id] == null) return false;
    return videoInstances[id].state == "STOPPED";
},

IsLoading: function(id){
    if(videoInstances[id] == null) return false;
    return videoInstances[id].state == "PREPARING";
},

GetPlaybackRate: function(id){
    if(videoInstances[id] == null) return 0;
    return videoInstances[id].playbackRate;
},
GetBufferLevel: function(id){
    if(videoInstances[id] == null) return 0;
    //return videoInstances[id].bufferlevel;
    return 0;
},

WebGLMovieTextureIsReady: function(video)
{
	return videoInstances[video].readyState >= videoInstances[video].HAVE_CURRENT_DATA;
}


};

autoAddDeps(LibraryMPMPWebGL, '$videoInstances');
autoAddDeps(LibraryMPMPWebGL, '$videoTextures');
autoAddDeps(LibraryMPMPWebGL, '$JsCallCsTest');
mergeInto(LibraryManager.library, LibraryMPMPWebGL);
