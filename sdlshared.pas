unit sdlshared;

interface

uses
  SDL2, SDL2_Mixer, sdl2_image;

// returns true, if SDL has been initialized
// returns false, if fails to initialize
function InitSDL: Boolean;

implementation

var
  isInitialized : Boolean = false;

function InitSDL: Boolean;
begin
  if (isInitialized) then begin
    Result := true;
    Exit;
  end;

  try
    //// Initialise audio
    if (SDL_Init(SDL_INIT_AUDIO) < 0) then
    begin
      //  Log.LogError(Format('Couldn''t initialize SDL : %s',
      //  [SDL_GetError]), 'Main');
      //TerminateApplication;
      Exit;
    end;

    //Open the audio device
    // NOTE : the call to  Mix_OpenAudio MUST happen before the call to
    //       SDL_SetVideoMode, otherwise you will get a ( sometimes load )
    //        audible pop.
    if (Mix_OpenAudio(44100, AUDIO_S16SYS, 2, 2048) < 0) then
    begin
      //Log.LogWarning(Format('Couldn''t set 11025 Hz 8-bit audio - Reason : %s',  [Mix_GetError]), 'Main');
    end;

    // Initialise SDL
    if ( SDL_Init( SDL_INIT_VIDEO or SDL_INIT_JOYSTICK ) < 0 ) then
    begin
      //  Log.LogError( Format( 'Could not initialize SDL : %s', [SDL_GetError] ), 'Main' );
      Exit;
    end;

    IMG_Init(IMG_INIT_PNG);
    isInitialized := true;
  finally
    Result := isInitialized;
  end;
end;

end.
