// Display, Version 1.1. (19 june 07)
// Now includes procedurs to set up SDL and position the display window
// still requires alterations to load spatial calib function to extract screen resolution data
// may want to remove SDL mixer set up...
// This version now uses texture size 4096h x 2048v to for texture distortion to accommodate
// higher resolutions e.g. 1600x1200
// variable 'width' corresponds to the width in pixels of a single image, so this
// is doubled for stereoscope windows
// NOTE the current NVIDIA card supports an OPENGL window only  2960pixels wide
// before an artefact occurs (missing chunks in upper part of screen.


unit display2;

{$MODE Delphi}
//{$mode objfpc}//{$H+}
interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, Useful, ExtCtrls, Menus,SDL2,gl,glu,
  GLTextureDistortion, Buttons,{logger,}SDL2_Mixer;


const
  LEFT_EYE  =false; //set these two variables as booleans rather than integers
  RIGHT_EYE =true;

  // texture distortion variables
  var
   TexID,GridDisplayList: GLUint;
   memo: array[0..4096 *2048*3] of GLUByte;


 type
  TDisplayType = (VESA,BlueLine,StScopeDualVGA, StScopeSingleVGA,Cyclopean, NVIDIA);





  type
  TDisplayEnviroForm = class(TForm)
    Label1: TLabel;
    ListBox1: TListBox;
    Edit1: TEdit;
    Label3: TLabel;
    OpenDialog1: TOpenDialog;
    Button1: TButton;
    Edit6: TEdit;
    Label6: TLabel;
    Edit7: TEdit;
    Edit8: TEdit;
    Label9: TLabel;
    Label10: TLabel;
    Edit4: TEdit;
    Edit5: TEdit;
    Label8: TLabel;
    Label7: TLabel;
    Label4: TLabel;
    Edit2: TEdit;
    Edit3: TEdit;
    Label5: TLabel;
    Edit9: TEdit;
    Edit10: TEdit;
    Label11: TLabel;
    Label12: TLabel;

    procedure Initialise(Sender: TObject);
    procedure ListBox1Click(Sender: TObject);
    procedure Button1Click(Sender: TObject);
     procedure refreshForm;

  private



    { Private declarations }
  public
    displayWindowXpos,
    displaywindowYpos: integer;
    DisplayType:TDisplayType;
  //  MonitorNumber: TMonitorNumber;
    FrameRate,
    Width,
    Height,
    TextureWidth,
    TextureHeight: integer;
    WidthCM,
    HeightCM,
    Distance,
    IOD,
    HeadX,
    HeadY,
    HeadZ,
    AngleOfRegard: real;
    CurrentEye: boolean;

    GridDisplayList,TexID:GLUint;
    DisplayGridLeftX,
    DisplayGridLeftY,
    DisplayGridRightX,
    DisplayGridRightY,
    TexCoordGridLeftX,
    TexCoordGridLeftY,
    TexCoordGridRightX,
    TexCoordGridRightY: T2Darray;
    NRows,
    NCols: integer;
    Details: string;

    IsStScope:boolean;
    IsStScopeSpatCorr: boolean;
   // StScopeSpatCorrFilePath: string;
    StScopeSpatCorrFile: string;

    //SDL video
   // surface : PSDL_Surface;
    surface : PSDL_Window;
   // videoInfo : PSDL_VideoInfo;
    videoflags : Uint32;
    renderer : PSDL_Renderer;
    glcontext : TSDL_GLContext;

    //SDL joystick pointers
    joystick,joystick1, joystick2: PSDL_joystick;

    //Windows handle of SDL window
    Wnd: hWnd;



    procedure SetDisplayType(DispType: TDisplayType);
    //procedure SetMonitorNumber(MonNumber: TMonitorNumber);
    procedure SetSpatCalib(PathAndFile:string);
    procedure InitSpatCalib;

    procedure RenderStereo;
    procedure RenderBlueLineStereo;
    procedure RenderStScopeDualVGA;
    procedure RenderRedGreenStereo;
    procedure Render;
    procedure ProjectionTrans;
    procedure SwapEyes;
    procedure PositionSDLWindow;
    procedure SDLsetup;
    { Public declarations }


  end;





implementation

{$R *.lfm}

//------------------------------------------------------------------------------
// Terminate program

procedure TerminateApplication;
begin
  SDL_ShowCursor(SDL_ENABLE);
  SDL_QUIT;
  application.terminate;
  //UnLoadOpenGL;
  Halt(0);
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.Initialise;//(sender:Tobject);

begin
  displayWindowXpos:=0;//1152;
  displayWindowYpos:=0;//864;
  Width:=1024;//1920;//1600;//1280;//1400;//1024;//1800;//800;//1152;  //WIDTH REFERS TO THE WIDTH OF ONE IMAGE, SO THIS IS DOUBLED FOR STEREOSCOPE MODE
  Height:=768;//1080;//1200;//1024;//1050;//768;//1440;//600;//864;
  FrameRate := 100;

  TextureWidth := 4096;    // ensure these have the same values as 'memo' above
  TextureHeight := 2048;
  WidthCM := 38.2;
  HeightCM := 29.3;
  Distance := 100;
  IOD := 6.25;
  HeadX := 0;
  HeadY := 0;
  HeadZ := 0; //distance from openGL's origin , along the Z axis. -ve values
              //denote positions further away in depth, i.e. default coordinate frame.
  AngleOfRegard := 0;
  CurrentEye := LEFT_EYE;
  IsStScopeSpatCorr := false;

  //width:=Monitor1Width;
  //height:=monitor1Height;

  SetDisplayType(BlueLine);
  //SetMonitorNumber(2);
  //IsStScopeSpatCorr:=false;
  //StScopeSpatCorrFile:='C:\Documents and Settings\Dr Phil Duke\My Documents\delphi_projects\spatial calibration\Spatial calibration files\Default.txt';
  StScopeSpatCorrFile:='C:\Documents and Settings\Dr Phil Duke\My Documents\delphi_projects\spatial calibration\Spatial calibration files\Large stereoscope 1400x1050.txt';

  //SetSpatCalib(StScopeSpatCorrFile);


  refreshForm;

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.SetDisplayType(DispType: TDisplayType);
begin
  DisplayType:=DispType;

  if (DisplayType = StScopeDualVGA) or (DisplayType = StScopeSingleVGA) then
  begin
    // SetMonitorNumber(1);
    IsStScope:= true;
    {
    if not fileExists (StScopeSpatCorrFile) then
    begin
      showmessage('TDisplayEnviroForm.SetDisplayType: Set a valid spatial configuration file file');
    end
    else
    begin
      SetSpatCalib(StScopeSpatCorrFile);
    end;//Load a default calibration file initially
    //SetSpatCalib('C:\Documents and Settings\Dr Phil Duke\My Documents\delphi_projects\spatial calibration\Spatial calibration files\Default calibration.txt');
  }end
  else
  begin
     IsStScope:= false;
     IsStScopeSpatCorr:=false;
  end;
  refreshForm;
end;
//------------------------------------------------------------------------------

                   {
//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.SetMonitorNumber(MonNumber: TMonitorNumber);
begin

  // Set monitor number. If stereoscope, then cannot set monitor number =2.
  if (MonNumber = 2) and IsStScope then
  begin
    showmessage('SetMonitorNumber: Cannot set monitor number = 2 for stereoscope displays')

  end


  else
  begin
    if MonNumber = 1 then
    begin
      width:=Monitor1Width;
      height:=Monitor1Height;
    end;

    if MonNumber = 2 then
    begin
      width:=Monitor2Width;
      height:=monitor2Height;
    end;

    MonitorNumber:=MonNumber;

  end;

  refreshForm;

end;
//------------------------------------------------------------------------------
         }

//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.SetSpatCalib(PathAndFile:string);
var
  StringData:string;
  IntegerData:integer;
  RealData: real;
  col,row: integer;
  TxtFile: textfile;

begin

  If IsStScope then
  begin
    If FileExists(PathAndFile) { *Converted from FileExists*  } then
    begin
      IsStScopeSpatCorr:=true;
      StScopeSpatCorrFile := PathAndFile;
     // showmessage('Loading SpatCal');
      assignFile(TxtFile,PathAndFile);


      // Extract DISPLAY_DETAILS
      reset(TxtFile);
      while not (StringData = 'DISPLAY_DETAILS') do
      begin
        Readln(TxtFile, StringData);
      end;
      Readln(TxtFile, StringData);
      Details := StringData;

      // Extract SCREEN_WIDTH
    reset(TxtFile);
    while not (StringData = 'SCREEN_WIDTH') do
    begin
      Readln(TxtFile, StringData);
    end;
    Readln(TxtFile, IntegerData);
    width:=round(IntegerData/2);
   // SCREEN_HALF_WIDTH:= round(SCREEN_WIDTH/2);
    Edit2.text:=inttostr(width);


    // Extract SCREEN_HEIGHT
    reset(TxtFile);
    while not (StringData = 'SCREEN_HEIGHT') do
    begin
      Readln(TxtFile, StringData);
    end;
    Readln(TxtFile, IntegerData);
    height:=IntegerData;
    Edit3.text:=inttostr(IntegerData);

      // Extract GRID_NROWS
      reset(TxtFile);
      while not (StringData = 'GRID_NROWS') do
      begin
        Readln(TxtFile, StringData);
      end;
      Readln(TxtFile, IntegerData);
      nrows := IntegerData;


      // Extract GRID_NCOLS
      reset(TxtFile);
      while not (StringData = 'GRID_NCOLS') do
      begin
        Readln(TxtFile, StringData);
      end;
      Readln(TxtFile, IntegerData);
      NCols := IntegerData;


      // Extract TOTAL_HEIGHT
      reset(TxtFile);
      while not (StringData = 'TOTAL_HEIGHT') do
      begin
        Readln(TxtFile, StringData);
      end;
      Readln(TxtFile, RealData);
      HeightCM := RealData;


      //Extract TOTAL_WIDTH
      reset(TxtFile);
      while not (StringData = 'TOTAL_WIDTH') do
      begin
        Readln(TxtFile, StringData);
      end;
      Readln(TxtFile, RealData);
      WidthCM := RealData;


      // Extract DISTANCE
      reset(TxtFile);
      while not (StringData = 'DISTANCE') do
      begin
        Readln(TxtFile, StringData);
      end;
      Readln(TxtFile, RealData);
      Distance :=RealData;


      SetLength(DisplayGridLeftX,NCols,nrows);
      SetLength(DisplayGridLeftY,NCols,nrows);
      SetLength(DisplayGridRightX,NCols,nrows);
      SetLength(DisplayGridRightY,NCols,nrows);

      // Extract DisplayGridLeftX
      reset(TxtFile);
      while not (StringData = 'DisplayGridLeftX') do
      begin
        Readln(TxtFile, StringData);
      end;

      for row:=0 to NRows-1 do
      begin
        for col:=0 to NCols-1 do
        begin
          readln(Txtfile,StringData);
          DisplayGridLeftX[col,row]:=strtofloat(StringData);
        end;
      end;


      // Extract DisplayGridLeftY
      reset(TxtFile);
      while not (StringData = 'DisplayGridLeftY') do
      begin
        Readln(TxtFile, StringData);
      end;

      for row:=0 to NRows-1 do
      begin
        for col:=0 to NCols-1 do
        begin
          readln(Txtfile,StringData);
          DisplayGridLeftY[col,row]:=strtofloat(StringData);
        end;
      end;


      // Extract DisplayGridRightX
      reset(TxtFile);
      while not (StringData = 'DisplayGridRightX') do
      begin
        Readln(TxtFile, StringData);
      end;

      for row:=0 to NRows-1 do
      begin
        for col:=0 to NCols-1 do
        begin
          readln(Txtfile,StringData);
          DisplayGridRightX[col,row]:=strtofloat(StringData);
        end;
      end;


      // Extract DisplayGridRightY
      reset(TxtFile);
      while not (StringData = 'DisplayGridRightY') do
      begin
        Readln(TxtFile, StringData);
      end;

      for row:=0 to NRows-1 do
      begin
        for col:=0 to NCols-1 do
        begin
          readln(Txtfile,StringData);
          DisplayGridRightY[col,row]:=strtofloat(StringData);
        end;
      end;


      // Close the output file
      closefile(TxtFile);


      // make a grid of texture coordinates.
      // NOTE TEXTURE COORDS ARE SUCH AS TO REVERSE EACH EYES IMAGE
      // THUS IMAGES ARE CORRECT FOR STEREOSCOPE VIEWING

      SetLength(TexCoordGridRightX,NCols,NRows);
      SetLength(TexCoordGridRightY,NCols,NRows);
      SetLength(TexCoordGridLeftX,NCols,NRows);
      SetLength(TexCoordGridLeftY,NCols,NRows);

      makegrid(
        TexCoordGridLeftY, TexCoordGridLeftX,
        0,Height/TextureHeight,
        ((width*2)/TextureWidth)/2,0,
        NCols,NRows);

      makegrid(
        TexCoordGridRightY,TexCoordGridRightX,
        0,Height/TextureHeight,
        (width*2)/TextureWidth,(((width*2)/TextureWidth)/2)+ (1/(width*2)),
        NCols,NRows);
    end
    else
    begin
      showmessage('SetSpatCalib:Spatial calibration file does not exist');
    end
  end
  else
  begin
    showmessage('SetSpatCalib: Can only apply spatial correction to stereoscope displays')

  end;



  refreshForm;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.refreshForm;


begin

  case DisplayType of
    VESA:
    begin
      ListBox1.ItemIndex := 0;
     // ListBox2.Enabled:=true;
    //  Label2.Caption:='Display monitor:';
    end;

    BlueLine:
    begin
      ListBox1.ItemIndex := 1;
     // ListBox2.Enabled:=true;
     // Label2.Caption:='Display monitor:';
    end;

    StScopeDualVGA:
    begin
      ListBox1.ItemIndex := 2;
    //  ListBox2.Enabled:=false;
    //  Label2.Caption:='Dual monitors';
    end;

    StScopeSingleVGA:
    begin
      ListBox1.ItemIndex := 3;
    //  ListBox2.Enabled:=false;
    //  Label2.Caption:='Dual monitors';
    end;

  end;

  {
  case MonitorNumber of
    1: ListBox2.ItemIndex:=0;
    2: ListBox2.ItemIndex:=1;
  end;
    }
  if IsStScope then
  begin
    Button1.Enabled:=true;
    Edit1.Enabled:=true;
  end
  else
  begin
    Button1.Enabled:=false;
    Edit1.Enabled:=false;
  end;

 {
  if IsStScopeSpatCorr then
  begin
    GroupBox1.enabled:=false;
    Edit1.Enabled:=true;
  end
  else
  begin
    GroupBox1.enabled:=true;
    Edit1.Enabled:=false;
  end;
    }


  Edit1.Text:=StScopeSpatCorrFile;

  Edit2.Text := IntToStr(Width);
  Edit3.Text := IntToStr(Height);
  Edit4.Text := floatToStr(WidthCM);
  Edit5.Text := floatToStr(HeightCM);

  Edit6.Text := Details;

  Edit7.Text := IntToStr(TextureWidth);
  Edit8.Text := IntToStr(TextureHeight);

  Edit9.Text := inttostr(displayWindowXpos);
  Edit10.Text := inttostr(displayWindowYpos);

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.ListBox1Click(Sender: TObject);
begin

  case ListBox1.Itemindex of
    0:
    SetDisplayType(VESA);
    1:
    SetDisplayType(BlueLine);
    2:
    SetDisplayType(StScopeDualVGA);
    3:
    SetDisplayType(StScopeSingleVGA);
    end;

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.Button1Click(Sender: TObject);
begin

  OpenDialog1.InitialDir:='C:\Documents and Settings\Dr Phil Duke\My Documents\delphi_projects\spatial calibration\Spatial calibration files\';
  OpenDialog1.Filter:='Text files (*.txt)|*.txt|All files (*.*)|*.*';
  OpenDialog1.title:='Load File';
  OpenDialog1.execute;
  SetSpatCalib(OpenDialog1.FileName);

  refreshForm;

end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------

procedure TDisplayEnviroForm.ProjectionTrans;
const
  NearClipQuantity=0.01; //Near Clip Plane distance as a proportion of the distance to the screen

var
  EyePosX,EyePosZ, nWidthCM1,nwidthcm2:real;


begin

     // use quad buffer stereo in NVDIA mode
    if (displayType=NVIDIA) then
    begin
      if CurrentEye = LEFT_EYE then
      begin
        glDrawBuffer(GL_BACK_LEFT);
        glClear(GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT);      //clear color and depth buffers
      end
      else
      begin
        glDrawBuffer(GL_BACK_RIGHT);
        glClear(GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT);      //clear color and depth buffers
      end;
    end;

    if CurrentEye=LEFT_EYE then // Work out left eye position
    begin;
      EyePosX:=-(IOD/2)*cos(AngleofRegard);
      EyePosZ:=(IOD/2)*sin(AngleofRegard);
    end
    else        // Work out right eye position
    begin
      EyePosX:=(IOD/2)*cos(AngleofRegard);
      EyePosZ:=-(IOD/2)*sin(AngleofRegard);
    end;

    // do the projection transformation
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity;

    glFrustum(
    ((-WidthCM/2)-EyePosX-HeadX)*NearClipQuantity,
    ((WidthCM/2)-EyePosX-HeadX)*NearClipQuantity,
    ((-HeightCM/2)-HeadY)*NearClipQuantity,
    ((HeightCM/2)-HeadY)*NearClipQuantity,
    (Distance+HeadZ+EyePosZ)*NearClipQuantity,
    500000);

   glulookat(EyePosX+HeadX, HeadY, EyePosZ+HeadZ, EyePosX+HeadX, HeadY, -distance,
    0, 1, 0);

    if IsStScope then
    begin
      if CurrentEye = LEFT_EYE then
      begin
        glViewport(0, 0,  width, height );
      end
      else
      begin
        glViewport(width, 0,  width, height );
      end;
    end;

end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.InitSpatCalib;
begin
  InitGLTextureDistortion(TexID, TextureWidth,TextureHeight,@memo);

  //Make the texture mapped mesh into a display list
  GridDisplayList := glGenLists(1);
  glNewList(GridDisplayList,GL_COMPILE);
	DoGLTextureDistortionMakeGridDisplayList(TexCoordGridLeftX,TexCoordGridLeftY,
            TexCoordGridRightX,TexCoordGridRightY,DisplayGridLeftX,
            DisplayGridLeftY,DisplayGridRightX,DisplayGridRightY,
             NCOLS,NROWS);
  glEndList;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.RenderStereo;
begin

  case DisplayType of
  BlueLine: RenderBlueLineStereo;
  //if DAS6014 then DAS6014Stereo;
  StScopeDualVGA:RenderStScopeDualVGA;
  StScopeSingleVGA:RenderRedGreenStereo;
  Cyclopean:Render;
  NVIDIA:Render;
  end;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.RenderBlueLineStereo;

begin

  glMatrixMode(GL_MODELVIEW);
  glLoadIdentity;

  glMatrixMode(GL_PROJECTION);
  glLoadIdentity;

  glOrtho(0.0, 1.0, 0.0, 1.0, -1, 1);

  gldisable(gl_lighting);

  glcolor3f(0,0,1);

  gllinewidth(3);

  if CurrentEye = LEFT_EYE then
  begin
    glbegin(GL_LINES);
      glvertex2f(0,0);
      glvertex2f(0.25,0);
    glend;
  end
  else
  begin
    glbegin(GL_LINES);
      glvertex2f(0,0);
      glvertex2f(0.75,0);
    glend;
  end;

  gllinewidth(1);
 // COMMENTED FOR SDL2 SDL_GL_swapbuffers;
  glClear(GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT);

end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.RenderStScopeDualVGA;

begin

  glMatrixMode(GL_PROJECTION);
  glLoadIdentity;

  glOrtho(0.0, 1.0, 0.0, 1.0, -1, 1);

  gldisable(gl_lighting);

 // Do texture mapped distortion correction
 if currentEye = RIGHT_EYE then
  begin
    glEnable(GL_TEXTURE_2D);

    gldisable(GL_CULL_FACE);

    glviewport(0,0,width*2,height);
    if IsStScopeSpatCorr then DoGLTextureDistortionFixed(GridDisplayList,texturewidth,textureheight,TexID);   ////

     // COMMENTED FOR SDL2 SDL_GL_swapbuffers;
    glClear(GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT);
  end;

end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.RenderRedGreenStereo;

begin

  if currentEye = LEFT_EYE then
  begin
    glColorMask(GL_TRUE, GL_TRUE, GL_TRUE, GL_TRUE);
    glClear( GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT );
    glColorMask(GL_TRUE, GL_FALSE, GL_FALSE, GL_FALSE);
    glColor3f (0.0, 1.0, 0.0);
  end
  else
  begin
    glColorMask(GL_TRUE, GL_TRUE, GL_TRUE, GL_TRUE);
    glClear( GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT );
    glColorMask(GL_TRUE, GL_FALSE, GL_FALSE, GL_FALSE);
    glColor3f (1.0, 0.0, 0.0);
     // COMMENTED FOR SDL2 SDL_GL_swapbuffers;
    glClear(GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT);
  end;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.Render;

begin
   // COMMENTED FOR SDL2
  SDL_GL_SwapWindow(surface);
  //SDL_GL_swapbuffers;
  glClear(GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT);
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.SwapEyes;
begin
    CurrentEye := not(CurrentEye);
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.PositionSDLWindow;
begin
  // displayWindowXpos := strtoint(edit9.text);
  // displayWindowYpos := strtoint(edit10.text);
  // Width := strtoint(edit2.text);
   //Height := strtoint(edit3.text);


  // set window width for single screen or 2xwidth for stereoscope
  if IsStScope then
  begin
    SetWindowPos(Wnd, HWND_TOP	, displayWindowXpos,0,Width*2, Height,SWP_SHOWWINDOW	);
  end
  else
  begin
  SetWindowPos(Wnd, HWND_TOP	,displayWindowXpos, displayWindowYpos,Width, Height,SWP_SHOWWINDOW	);
 // SDL_hidewindow(surface);

  end;
  SDL_ShowCursor(SDL_DISABLE);

end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TDisplayEnviroForm.SDLsetup;

var
  present : integer;

const
  // Video parameters
  FULLSCREEN : integer = 1;
  SCREEN_BPP = 24;


begin


//// Initialise audio
if (SDL_Init(SDL_INIT_AUDIO) < 0) then
begin
//  Log.LogError(Format('Couldn''t initialize SDL : %s',
 //  [SDL_GetError]), 'Main');
 //TerminateApplication;
  exit;
end;

 //Open the audio device
// NOTE : the call to  Mix_OpenAudio MUST happen before the call to
 //       SDL_SetVideoMode, otherwise you will get a ( sometimes load )
//        audible pop.
if (Mix_OpenAudio(22050, AUDIO_U8, 1, 1024) < 0) then
begin
 // Log.LogWarning(Format('Couldn''t set 11025 Hz 8-bit audio - Reason : %s',
   // [Mix_GetError]), 'Main');
end;

  // Initialise SDL
  if ( SDL_Init( SDL_INIT_VIDEO or SDL_INIT_JOYSTICK ) < 0 ) then
  begin
  //  Log.LogError( Format( 'Could not initialize SDL : %s', [SDL_GetError] ),
    //  'Main' );
    TerminateApplication;
  end;


  surface:= SDL_CreateWindow(
    'My window',
    SDL_WINDOWPOS_UNDEFINED,
    SDL_WINDOWPOS_UNDEFINED,
    width,
    height,
   SDL_WINDOW_FULLSCREEN or SDL_WINDOW_OPENGL);


  glcontext := SDL_GL_CreateContext(surface);
  SDL_GL_SetSwapInterval(1);


  // Set the OpenGL Attributes
  SDL_GL_SetAttribute( SDL_GL_RED_SIZE, 8 );
  SDL_GL_SetAttribute( SDL_GL_GREEN_SIZE, 8 );
  SDL_GL_SetAttribute( SDL_GL_BLUE_SIZE, 8 );
  SDL_GL_SetAttribute( SDL_GL_DEPTH_SIZE, 16 );
  SDL_GL_SetAttribute( SDL_GL_DOUBLEBUFFER, 1 );
  {
  present := -1;
  if displayType = NVIDIA then
  begin
    SDL_GL_SetAttribute( SDL_GL_STEREO, 1 );
    SDL_GL_GetAttribute( SDL_GL_STEREO, present ); // present==0 means the video card can do quad buffering
    if (present <>0) then
    begin
         showmessage('NVIDIA stero not available: ' + inttostr(present));
    end;
  end;
   }

  {
  // Set the title bar in environments that support it
  SDL_WM_SetCaption( 'STIMULUS', nil);



  if FULLSCREEN=1 then
  begin
    videoflags := videoFlags or SDL_NOFRAME;
  end else
  begin
    videoflags := videoFlags  or SDL_RESIZABLE;    // Enable window resizing
  end;


  if IsStScope then
  begin
    surface := SDL_SetVideoMode( Width*2, height, SCREEN_BPP,videoflags );
  end
  else
  begin
    surface := SDL_SetVideoMode( Width-1, height, SCREEN_BPP,videoflags );
  end;
  }

  if ( surface = nil ) then
  begin
   // Log.LogError( Format( 'Unable to create OpenGL screen : %s', [SDL_GetError]
   //   ),
   //   'Main' );
    TerminateApplication;
  end;


  // Now some windows commands to centre the SDL_spp window across both monitors
  Wnd:=GetForegroundWindow;


  PositionSDLWindow;

  //joystick
  if SDL_numjoysticks>0 then
  begin
      SDL_joystickeventstate(SDL_ENABLE);
      joystick:=SDL_joystickopen(0);
      joystick1:=SDL_joystickopen(1);
      joystick2:=SDL_joystickopen(2);
     //showmessage(inttostr(SDL_numjoysticks));
  end;
end;
//------------------------------------------------------------------------------



end.
