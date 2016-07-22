//
//  mpMediaPlayer.h
//
//
//  Created by Stefan Schlupek on 04.12.15.
//  Copyright © 2015 Stefan Schlupek. All rights reserved.
//

#include "Unity/IUnityGraphics.h"

#include <map>

#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>
#import <CoreMedia/CoreMedia.h>

#if TARGET_OS_IPHONE

#import <OpenGLES/ES2/gl.h>
#import <OpenGLES/ES2/glext.h>


#else

#import <OpenGL/gl.h>
#import <QuartzCore/QuartzCore.h>
#import <QuartzCore/CoreImage.h>

#endif

#if TARGET_OS_MAC

#endif

typedef void (*callbackFunc)(const char *);
typedef void (*callbackFuncEvents)(int _id,int _evt);

enum Events{LOAD,LOADED,PLAY,PAUSE,STOP,DESTROY,_ERROR,PLAYBACKCOMPLETED,AVF_PIXELBUFFER_ERROR,TEXTURE_CHANGED, BUFFERING};

@protocol VideoPlayerDelegate<NSObject>
- (void)onPlayerReady;
- (void)onPlayerDidFinishPlayingVideo;
@end

@interface  mpMediaPlayer :NSObject{
    
    id<VideoPlayerDelegate> delegate;
 
    intptr_t texIDptr;
    GLuint texID;
    
    int _id;
}
@property (nonatomic, retain) id delegate;

-(int)GetID;
-(void)RenderEvent;
-(void)unityLog:(NSString*) msg;
-(BOOL)load:(NSString*)filePath;
-(void)play;
-(void)pause;
-(void)stop;
-(void)seekTo:(float) time;
-(void)setAutoPlay:(BOOL)status;
-(void)setVolume:(float) volume;
-(void)setBalance:(double) balance;
-(void)setLooping:(BOOL) status;
-(void)setIsLinear:(BOOL) status;

- (void)unloadPlayer;

@property (nonatomic, retain) AVPlayer *videoPlayer;
@property (nonatomic, retain) AVPlayerItemVideoOutput *videoOutput;
@property (nonatomic, assign) intptr_t texIDptr;
@property (nonatomic, assign) GLuint texID;

@property callbackFunc logCallback;
@property callbackFuncEvents eventCallback;

@end
