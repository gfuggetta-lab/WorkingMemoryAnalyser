
unit main;


{$MODE Delphi}

interface


uses
  LCLIntf, LCLType, LMessages, Messages, SysUtils, Classes, Graphics, Controls,
  Forms, Dialogs, StdCtrls, ExtCtrls, ActnList, SDL2, SDL2_Mixer, SDL2_ttf, gl,
  Useful, GLTextureDistortion, Display2, Timing, math, IOR_Shapes, FileUtil, inputfileunit1, ParticipantID, loadImages

  ,TriggerStationDevice_DLL_1_0_TLB;

function Inp32(wAddr:word):byte; stdcall; external 'inpout32.dll';
function Out32(wAddr:word;bOut:byte):byte; stdcall; external 'inpout32.dll';
procedure Terminateapplication;




type

  { TForm1 }

  TForm1 = class(TForm)
    Button1: TButton;
    Button3: TButton;
    ComboBox13: TComboBox;
    ComboBox14: TComboBox;
    ComboBox4: TComboBox;
    ComboBox5: TComboBox;
    ComboBox6: TComboBox;
    ComboBox7: TComboBox;
    Edit3: TEdit;
    Label11: TLabel;
    Label12: TLabel;
    Label13: TLabel;
    Label14: TLabel;
    Label15: TLabel;
    Label5: TLabel;
    Label7: TLabel;
    Label8: TLabel;
    Label9: TLabel;
    Memo1: TMemo;
    RadioGroup4: TRadioGroup;
    SaveDialog1: TSaveDialog;
    Button2: TButton;
    Edit1: TEdit;
    Label1: TLabel;
    ComboBox1: TComboBox;
    ComboBox2: TComboBox;
    Label2: TLabel;
    Label3: TLabel;
    OpenDialog1: TOpenDialog;
    ComboBox3: TComboBox;
    Label4: TLabel;
    Label6: TLabel;

    procedure Button3Click(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure Button1Click(Sender: TObject);
    procedure Button2Click(Sender: TObject);
    procedure ShowControlsIfReady(Sender: TObject);


    procedure RunExperiment(Sender: TObject);


  private
    { Private declarations }
    trialOrderFileNo: integer;
    participantID : string;
  public
    { Public declarations }

  end;


type
 TSounds = (
    //BEEP_WAV,
   // HIBEEP_WAV,
    CORRECT_WAV,
    INCORRECT_WAV,
    NUMBER_WAVES
    );






var

  //----------------------------------------------------
  // TriggerStation variables     - not used with fmri

  TriggerStation: ITriggerStationDevice;
  iNumDevices, i: Integer;
  userID: Byte;
  serialNumber: Integer;

  isTriggerStation : boolean = false;

  //----------------------------------------------------


  Form1: TForm1;
  GO:boolean;

  Tform1Handle: Hwnd;
  eF: TDisplayEnviroForm;

  currentDir: string;

  //SDL sound
  sounds: array[0..2] of PMix_Chunk;

  //display lists
  DL_CIRCLE, DL_HEX, DL_DIAMOND, DL_CIRCLE_BACKGROUND, DL_CIRCLE_OUTLINE,DL_TRIANGLE, DL_BOX, DL_RING, DL_CROSS,
  DL_BAR_HORIZ, DL_BAR_VERT, DL_SQUARE_EA, DL_CIRCLE_EA, DL_STAR, DL_PHOTODIODE_PATCH_LEFT, DL_PHOTODIODE_PATCH_RIGHT, DL_PHOTODIODE_PATCH_CENTRE: GLUint;
  rr: float = 0;
  gg: float = 0;
  bb: float = 0;




  colours: array [0..15] of TcolourReal;

  Pause_background_circle_colour : TcolourReal;
  Run_background_circle_colour : TcolourReal ;
  Fixation_and_placeholders_colour: TcolourReal ;
  Incorrect_feedback_colour : TcolourReal ;
  Correct_feedback_colour : TcolourReal ;

  Placeholder_diameter_deg, Shape_diameter_deg, Shape_scale_factor:		  real;



  fixSpotSizeCM:real;
  targetRadiusCM:real;

  REFRESH_RATE :integer = 100; //Hz

  DISPLAY_TYPE: integer; // output to e.g. Giorgio's g-sync monitor or fmri display

  pfontStim1, pfontStim2, pfontFeedback, pfontGeneral: PTTF_font;

  IS_SUSPENDED    :boolean = false;

  BMPimages : array [0..101] of TBMPimages;

  const

    // Display device definitions, as they appear on the display radiobox
    DISPLAY_GIORGIO_GSYNC = 1;
  //  DISPLAY_FMRI = 1;
  //  DISPLAY_PHIL_GSYNC = 2;
  //  DISPLAY_CRT = 3;
    DISPLAY_PC_LAB = 0;

    // participant responses (left or right button, either on keyboard, keypad or button box)
    RESPONSE_LEFT_BUTTON = 1;
    RESPONSE_RIGHT_BUTTON = 2;

    // scale the image angles by a factor

     SCALE_FACTOR = 1;// (23/29.8);



    PHOTODIODE_PATCH_SIZE_CM = 1;

    MAX_PAUSE_TRIALS = 100;



implementation

uses Unit2;

{$R *.lfm}




//------------------------------------------------------------------------------
//----------Connect TriggerStation----------------------------------------------
//------------------------------------------------------------------------------
procedure ConnectTriggerStation;
begin

  try
    TriggerStation := CoTriggerStationDevice.Create;
    isTriggerStation := true;
    try
      // Enumerate devices (this step must never be ommited!)
      iNumDevices := TriggerStation.EnumerateDevices();

      // Connect to every device and list data
      for i:= 0 to iNumDevices-1 do
      begin
          TriggerStation.ConnectToDevice(i);
          TriggerStation.GetUserID(userID);
          serialNumber:= TriggerStation.GetDeviceSerialNumber;
          isTriggerStation := true;
      end;
    except
      on E: Exception do
      Writeln(E.ClassName, ': ', E.Message);
    end;

  except
     on E: Exception do
     begin
      // showmessage(E.ClassName +  ': ' + E.Message);
         serialNumber := -1;
         isTriggerStation := false;
     end;
  end;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Terminate program
procedure TerminateApplication;
begin
  Mix_CloseAudio;
  SDL_QUIT;
  //UnLoadOpenGL;
  Halt(0);
  Application.terminate;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
function DegToCm(deg:real; distance: real):real;
begin
  result := (((deg/2) * (pi/180)) * distance) *2;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
// A general OpenGL initialisation function.
procedure InitGL;

begin
  // Clear Colour buffer
  glClearColor(0.0, 0.0, 0.0, 0.0);

  // Depth buffer setup
  glClearDepth( 1000.0 );
  glClear(GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT);

  // Enable GL antialiasing
  glenable(GL_LINE_SMOOTH);
  glenable(GL_CULL_FACE);

  // Enable Depth Testing
  glEnable( GL_DEPTH_TEST );

  // The Type Of Depth Test To Do
  glDepthFunc( GL_less );

  // Clear the modelview and projection matrices
  glMatrixMode( GL_MODELVIEW );
  glLoadIdentity;

  glMatrixMode( GL_PROJECTION );
  glLoadIdentity;
end;
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
// render text as a texture
procedure renderText(pfont:PTTF_font;  text: string; x,y:real; col1:PSDL_color; scale:real);

var
  textureID : GLUint;
  fontSurface, destSurface : PSDL_Surface;

begin
  // render text to surface
 // fontSurface :=TTF_renderText_solid(pfont,pchar(text),col1^);
  fontSurface :=TTF_renderText_solid(pfont,pchar(text),col1^);

  // create 24bit destination surface   with little-endian byte order
  destSurface := SDL_createRGBsurface(0,fontSurface^.w, fontSurface^.h, 24, $000000ff,$0000ff00,$00ff0000,$ff000000);
  SDL_blitsurface(fontsurface,NIL, destSurface, NIL);

 //  glEnable(GL_BLEND);
 // glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

  // create a new texture
  glGenTextures(1,@textureID);
  glBindTexture(GL_TEXTURE_2D,textureID);
  glTexImage2D(GL_TEXTURE_2D,0,GL_RGB,fontsurface^.w,fontsurface^.h,0,GL_RGB,GL_UNSIGNED_BYTE,destSurface^.pixels);

  // Select how the texture image is combined with existing image
  glTexEnvf(GL_TEXTURE_ENV,GL_TEXTURE_ENV_MODE,GL_REPLACE);

  // Texture parameters
  glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_NEAREST);
  glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_NEAREST);

 // glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_WRAP_S,GL_CLAMP);
 // glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_WRAP_T,GL_CLAMP);

  // Do texture mapping

//  glcolor4f(1,1,1,0.5);

  glEnable(GL_TEXTURE_2D);
  glBindTexture(GL_TEXTURE_2D,textureID);
  glbegin(GL_QUADS);
    glTexCoord2f(0,1); glvertex2f(x,y);

    glTexCoord2f(1,1); glvertex2f(x+fontSurface^.w*scale*(1),y);
    glTexCoord2f(1,0); glvertex2f(x+fontSurface^.w*scale*(1),y+fontSurface^.h*scale*(1));
    glTexCoord2f(0,0); glvertex2f(x,y+fontSurface^.h*scale*(1));

  glend();

  glDisable(GL_TEXTURE_2D);
  glDeleteTextures(1,@textureID);

  // clean up
  SDL_freeSurface(fontSurface);
  SDL_freeSurface(destSurface);
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// render text as a texture
procedure renderTextWithBackgroundColCentred(pfont:PTTF_font;  text: string; col1, bgrCol:PSDL_color; x,y:real; scale:real);

var
  textureID : GLUint;
  fontSurface, destSurface : PSDL_Surface;
  kol:TSDL_color;  texty: utf8string;
begin
  // render text to surface
  if (text<>'') then
  begin

    //fontSurface :=TTF_renderText_shaded(pfont,pchar(text),col1^, bgrCol^);
    fontSurface :=TTF_RenderUTF8_Shaded(pfont,pchar(text),col1^, bgrCol^);

    // create 24bit destination surface with little-endian byte order
    destSurface := SDL_createRGBsurface(0,fontSurface^.w, fontSurface^.h, 24, $000000ff,$0000ff00,$00ff0000,$ff000000);

    SDL_blitsurface(fontsurface,NIL, destSurface, NIL);

    // create a new texture
    glGenTextures(1,@textureID);
    glBindTexture(GL_TEXTURE_2D,textureID);
    glTexImage2D(GL_TEXTURE_2D,0,GL_RGB,fontsurface^.w,fontsurface^.h,0,GL_RGB,GL_UNSIGNED_BYTE,destSurface^.pixels);

    // Select how the texture image is combined with existing image
    glTexEnvf(GL_TEXTURE_ENV,GL_TEXTURE_ENV_MODE,GL_REPLACE);

    // Texture parameters
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_NEAREST);

    glEnable(GL_TEXTURE_2D);
    glBindTexture(GL_TEXTURE_2D,textureID);
    glbegin(GL_QUADS);

    glTexCoord2f(0,1); glvertex2f(x-((fontSurface^.w/2)*scale), y-((fontSurface^.h/2)*scale));
    glTexCoord2f(1,1); glvertex2f(x+((fontSurface^.w/2)*scale), y-((fontSurface^.h/2)*scale));
    glTexCoord2f(1,0); glvertex2f(x+((fontSurface^.w/2)*scale), y+((fontSurface^.h/2)*scale));
    glTexCoord2f(0,0); glvertex2f(x-((fontSurface^.w/2)*scale), y+((fontSurface^.h/2)*scale));
    glend();

    glDisable(GL_TEXTURE_2D);
    glDeleteTextures(1,@textureID);

    // clean up
    SDL_freeSurface(fontSurface);
    SDL_freeSurface(destSurface);
  end;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure drawTextStim(pfont:PTTF_font ;fontCol,bgrCol:TcolourReal;x,y:real; text:string);
var

  fcol,bcol: pSDL_color;
begin

  //convert opengl colour triplet into sdl colour triplet
  new(fcol);
  fcol^.r:=round(fontCol.r*255); fcol^.g:=round(fontCol.g*255); fcol^.b:=round(fontCol.b*255);

  new(bcol);
  bcol^.r:=round(bgrCol.r*255); bcol^.g:=round(bgrCol.g*255); bcol^.b:=round(bgrCol.b*255);

  // convert screen cm locations to pixel locations
  x:=x*(ef.Width/ef.widthCM);
  y:=y*(ef.height/ef.heightCM);

  glMatrixMode(GL_PROJECTION);
  glpushmatrix();
  glLoadIdentity;
  glviewport(0,0,ef.width,ef.height);


  glMatrixMode(GL_MODELVIEW);
  glLoadIdentity;
  glortho(-ef.width/2,ef.width/2,-ef.height/2,ef.height/2,-1,1);
  renderTextWithBackgroundColCentred(pfont,text,fcol,bcol,x,y,1{ef.Distance/57} ) ; // note size is scaled relative to stim at 57cm
  glpopmatrix();


  glMatrixMode(GL_PROJECTION);
  glpopmatrix();
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure DrawFixationSpot(sizeDeg:real;colour:TcolourReal);
var
  HalfSizeCM:real;

begin

 //(XCoord,Ycoord,ZCoord, OuterRadius,LineWidth:real; Npoints:integer;  Colour:array of real; filled:boolean);
// coords are cm, Size is degrees

  circle(0,0,-ef.distance,tan(sizeDeg*(pi/180))*ef.distance,0,10,[colour.r, colour.g, colour.b,1],true);

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Return a string filename to access a file
function DataFile(filename: string): string;
begin
  result := PChar(currentDir + '\Sounds\' + filename);
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure loadSounds;
begin

//  sounds[ord(BEEP_WAV)] := Mix_LoadWAV( PChar( DataFile( 'beep.wav' ) ) );
//  sounds[ord(HIBEEP_WAV)] := Mix_LoadWAV( PChar( DataFile( 'hibeep.wav' ) ) );
  sounds[ord(CORRECT_WAV)] := Mix_LoadWAV( PChar( DataFile( 'correct.wav' ) ) );
  sounds[ord(INCORRECT_WAV)] := Mix_LoadWAV( PChar( DataFile( 'incorrect.wav' ) ) );
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// APPEND string to a file
procedure WriteString(text:string);
var
f:    TextFile;

filename:string;

begin

  FileName := Form1.Savedialog1.filename;
  AssignFile(f, FileName);
  if FileExists(FileName) then
  begin
    Append(f);
  end
  else
  begin
    Rewrite(f);
  end;

  write(f,text);
  writeln(F);
  writeln(F);
  CloseFile(F);
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// APPEND header data to a file
procedure WriteHeader;
var
 f:    TextFile;

 Filename:string;

begin
  FileName := Form1.Savedialog1.filename;

  AssignFile(f, FileName);
  if FileExists(FileName) then
  begin
    Append(f);
  end
  else
  begin
    Rewrite(f);
  end;

  // session details
  writeln(f, DateToStr(date));
  writeln(f, TimeToStr(time));
  writeln(f, 'ParticipantID: ' + ParticipantIDForm.Label20.caption);
  writeln(f, 'Age: ' + Form1.combobox3.items[Form1.combobox3.itemindex]);
  writeln(f, 'Sex: ' + Form1.combobox13.items[Form1.combobox13.itemindex]);

  writeln(f);
  closeFile(f);
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure SaveBMP;
var
 surface1 : PSDL_surface;

begin

  surface1 := SDL_createRGBsurface(0,ef.width, ef.height, 24, $000000ff,$0000ff00,$00ff0000,$ff000000);

  glReadBuffer(GL_FRONT);
  glreadPixels(0,0,ef.width, ef.height, GL_RGB, GL_UNSIGNED_BYTE, surface1.pixels);
 SDL_SaveBMP(surface1,'Screenshot.bmp');
  SDL_freeSurface(surface1);

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// handle event
procedure pollevent(var State: integer; var eventTime : uint32) ;

var
  event : TSDL_Event;
  keysym : PSDL_keysym;

begin
  State:=-1;
  eventTime:=0;
  while ( SDL_PollEvent( @event ) = 1 ) do
  begin
  //  State:=-1;


    case event.type_ of

      SDL_JOYBUTTONDOWN :
      begin
        state := event.jbutton.button;
        eventTime:=event.jbutton.timestamp;
        //showmessage('Event time: = '+ inttostr(eventTime));
      end;

      SDL_MOUSEBUTTONDOWN:
      begin
        state := event.button.button;
        eventTime:=event.button.timestamp;
       // showmessage('mousebutton Event time: = '+ inttostr(eventTime));
      end;


      SDL_KEYDOWN :
      begin
          if (event.key._repeat = 0) then
          begin
            keysym:=@event.key.keysym;
            state:=keysym.sym;
          //  showmessage('     keysym.sym = ' + inttostr(keysym.sym));
            eventTime:=event.key.timestamp;

            case keysym.sym of



              SDLK_P :
               begin
                  if IS_SUSPENDED then
                  begin
                    IS_SUSPENDED := false;
                    // showmessage('unsuspended');

                  end
                  else
                  begin
                    IS_SUSPENDED := true;
                  end;
               end;


              SDLK_ESCAPE :
                TerminateApplication;

              SDLK_PRINTSCREEN :
              begin
                SaveBMP;
              end;
            end;
          end;
      end; //SDL_KEYDOWN :
    end; //case
  end; //while

//  if ((state <> -1) ) then showmessage('kkkkkkkkkkkkkkkkkkkkkkkkkkkk ' + inttostr(state));
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure fMRIimageTrans;
begin
  // gltranslatef(0,((ef.heightCM/2) - (ef.heightCM/3)),0);
   glscalef(9/13, 9/13,1);
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure GetNtrials(filename: string ; sessionNo:integer; var Ntrials:integer);

var
   dats:integer;
  datstring:string;

  c:integer;
  ff:textfile;
begin
 //showmessage(filename);
  AssignFile(ff, filename);

  reset(ff);

  //advance to the start of the data
  repeat
    readln(ff,datstring);
  until (trim(datstring)='#TRIAL_DATA');
  //showmessage('datstring: '+ datstring);
 // readln(ff);


  // advance to the specified sessionNo
  repeat
    readln(ff,dats) ;
  until (dats=sessionNo);


  // count the trials in that session
  c:=0;
   repeat
    readln(ff,datstring) ;
     //showmessage('datstring: '+ datstring);
    c:=c+1;
  until (trim(datstring)='#END');
     //showmessage('datstring: '+ datstring);

  Ntrials:=c;

  //showmessage('session: '+inttostr(sessionNo)+' data=  Ntrials: '+inttostr(c));
  closeFile(ff);

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// read the input filename and get the text following the string 'Experiment:'
function GetExperimentName(filename: string):string;

var
  dat: char;
  //dat1: integer;
  dats :integer;
  str0,str1,str2: string;
  c:integer;
  ff:textfile;

  s:TstringList;
  found:boolean =false;
  i:integer;
  flg:TreplaceFlags;
begin

  AssignFile(ff, filename);

  try
    reset(ff);

    str2:='Not_specified';

    while not eof(ff) do
    begin
      readln(ff,str0);
      i := pos('Experiment:',str0);
      if i<>0 then
      begin
         //showmessage (str0 );
        //showmessage (inttostr(length(str0)) );
        str1:=copy(str0,i+11,length(str0));
        //showmessage(str1+ ' ' + inttostr(length(str1)));
        str2:=trim(str1);
        //showmessage(str2 + ' ' + inttostr(length(str2)));
      end;
    end;
    closeFile(ff);

  except
   on E: EInOutError do
     showmessage('File error: '+ E.Message);
  end;
  //showmessage(str2);
  result:=str2;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
 procedure readTrialOrderData(filename: string ;
    sessionNo:integer;
    trialNo:integer;
    var s1_marker,s2_marker,s3_marker, s4_marker:integer;
   // var TMS_marker: integer;
    var s1_shape, s1_cue_quad:integer;
    var s1_duration, s1_s2_isi:integer;
    var s2_shape_position_1, s2_shape_position_2, s2_shape_position_3, s2_shape_position_4, s2_shape_position_5  :integer;
    var s2_duration, s2_s3_isi:integer;
    var s3_shape, s3_distractor_shape, s3_quad:integer;
    var s3_duration, s3_s4_isi:integer;
    var s4_shape, s4_distractor_shape, s4_quad:integer;
    var s4_duration:integer;
    var Response_Time_after_S4, Feedback_shape, Feedback_duration_after_response_time, ITI_after_feedback  : integer;
    var s1_colour, s2_colour_position_1, s2_colour_position_2, s2_colour_position_3, s2_colour_position_4, s2_colour_position_5 ,s3_colour, s3_distractor_colour,s4_colour, s4_distractor_colour, keyMapping, taskType:integer;
    var TMS_s3_SOA:integer;
    var Experimental_Condition: string);
var
  dat: char;
  dat1: integer;
  datstring:string;

  c:integer;
  ff:textfile;
begin
//showmessage(filename);
  AssignFile(ff, filename);
  reset(ff);
  //advance to the start of the data
  repeat
    readln(ff,datstring);
  until (trim(datstring)='#TRIAL_DATA');
 // showmessage('datstring: '+ trim(datstring));
 // readln(ff);


  // advance to the specified sessionNo
  repeat
    readln(ff,dat1,s1_marker,s2_marker,s3_marker, s4_marker, {TMS_marker,  }
    s1_shape, s1_cue_quad,
    s1_duration, s1_s2_isi,
    s2_shape_position_1, s2_shape_position_2, s2_shape_position_3, s2_shape_position_4,s2_shape_position_5,
    s2_duration, s2_s3_isi,
    s3_shape, s3_distractor_shape, s3_quad,
    s3_duration, s3_s4_isi,
    s4_shape, s4_distractor_shape, s4_quad,
    s4_duration,
    Response_Time_after_S4, Feedback_shape, Feedback_duration_after_response_time, ITI_after_feedback,
    s1_colour, s2_colour_position_1,s2_colour_position_2,s2_colour_position_3,s2_colour_position_4,s2_colour_position_5 ,s3_colour, s3_distractor_colour,s4_colour, s4_distractor_colour, keyMapping, taskType,TMS_s3_SOA,Experimental_Condition);

  until (dat1=sessionNo);
   { showmessage( 'dat1 :' + inttostr(dat1)  +
    's3_marker :' + inttostr(s3_marker)  +
    's4_marker :' + inttostr(s4_marker) +
    's1_shape :' + inttostr(s1_shape)   +
     's1_cue_quad :' + inttostr(s1_cue_quad));  }

  // advance to the specified trialNo
  for c:=1 to trialno do
  begin
   readln(ff,dat1,s1_marker,s2_marker,s3_marker, s4_marker, {TMS_marker,  }
    s1_shape, s1_cue_quad,
    s1_duration, s1_s2_isi,
    s2_shape_position_1, s2_shape_position_2, s2_shape_position_3, s2_shape_position_4,s2_shape_position_5,
    s2_duration, s2_s3_isi,
    s3_shape, s3_distractor_shape, s3_quad,
    s3_duration, s3_s4_isi,
    s4_shape, s4_distractor_shape, s4_quad,
    s4_duration,
    Response_Time_after_S4, Feedback_shape, Feedback_duration_after_response_time, ITI_after_feedback,
    s1_colour, s2_colour_position_1,s2_colour_position_2,s2_colour_position_3,s2_colour_position_4,s2_colour_position_5 ,s3_colour, s3_distractor_colour,s4_colour, s4_distractor_colour, keyMapping, taskType,TMS_s3_SOA,Experimental_Condition);

  end;


  Experimental_Condition:=trim(Experimental_Condition);
  closeFile(ff);
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure makeDisplayLists(radiusCM, backgroundradiusCM: real);
var
    r, r1, r2: real;


 begin


  DL_CIRCLE:= glGenLists(1);
  glNewList(DL_CIRCLE,GL_COMPILE);
    circle(0,0,-ef.distance+0.01,radiusCM,tan(0.0629*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance,20,true);
  glEndList;

  DL_CIRCLE_OUTLINE:= glGenLists(1);
  glNewList(DL_CIRCLE_OUTLINE,GL_COMPILE);
  //  circle(0,0,-ef.distance+0.01,radiusCM+  tan(0.138461538*(pi/180) * SCALE_FACTOR)*ef.distance  ,tan(0.055384615*(pi/180) * SCALE_FACTOR)*ef.distance  ,20,false);
    circle(0,0,-ef.distance+0.01,DegToCM(Placeholder_diameter_deg/2,ef.distance),DegToCM(0.05,ef.distance)  ,20,false);
  glEndList;

  // shapes are based on an area of 1.471814539 cm2

  // Shape 1.
  //diamond with equal area to bar
  r := tan(0.978923077*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance;
  DL_DIAMOND:= glGenLists(1);
  glNewList(DL_DIAMOND,GL_COMPILE);
    bar(0,0,-ef.distance+0.01, r, r, pi/4);
  glEndList;



  // Shape 2.
  //hexagon with equal area to bar
  r := tan(0.607418775*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance;
  DL_HEX:= glGenLists(1);
  glNewList(DL_HEX,GL_COMPILE);
    circle(0,0,-ef.distance+0.01,r,0.069230769,6,true);
  glEndList;


  // Shape 3.
  // Triangle, area equal to bar
  r := tan(0.858912367 *(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance;
  DL_TRIANGLE:= glGenLists(1);
  glNewList(DL_TRIANGLE,GL_COMPILE);
    circle(0,0,-ef.distance+0.01,r,0.069230769,3,true);
  glEndList;

  // Shape 4.
  // Box, area equal to bar
  DL_BOX:= glGenLists(1);
  glNewList(DL_BOX,GL_COMPILE);
   glpushmatrix;
   glmatrixmode(GL_MODELVIEW);
   glrotatef(45,0,0,1);
   circle(0,0,-ef.distance+0.01, tan(0.734223563*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance      ,   tan(0.489470467*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance,4,false);
   glpopmatrix;
  glEndList;


  // Shape 5.
  // ring, area equal to bar
  DL_RING:= glGenLists(1);
  glNewList(DL_RING,GL_COMPILE);
    circle(0,0,-ef.distance+0.01, tan(0.641766589*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance      ,   tan(0.315204306*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance,20,false);
  glEndList;


  // Shape 6.
  // Plus, area equal to bar
  r1 := tan(0.437821791*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance;
  r2 := tan(1.313260911*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance;
  DL_CROSS:= glGenLists(1);
  glNewList(DL_CROSS,GL_COMPILE);
    bar(0,0,-ef.distance+0.01, r1, r2, 0);
    bar(0,0,-ef.distance+0.01, r2, r1, 0);
  glEndList;


  // Shape 7.
  // Horiz bar
  r1 := tan(1.384615385*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance;
  r2 := tan(0.692307692*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance;
  DL_BAR_HORIZ:= glGenLists(1);
  glNewList(DL_BAR_HORIZ,GL_COMPILE);
    bar(0,0,-ef.distance+0.01, r1, r2, 0);
  glEndList;


  // Shape 8.
  // Vert bar
  r1 := tan(0.692307692*(pi/180) * SCALE_FACTOR  * Shape_scale_factor)*ef.distance;
  r2 := tan(1.384615385*(pi/180) * SCALE_FACTOR  * Shape_scale_factor)*ef.distance;
  DL_BAR_VERT:= glGenLists(1);
  glNewList(DL_BAR_VERT,GL_COMPILE);
    bar(0,0,-ef.distance+0.01,r1 , r2, 0);
  glEndList;


  // Shape 9.
  // square with area equal to bar
  r := tan(0.978923077*(pi/180) * SCALE_FACTOR  * Shape_scale_factor)*ef.distance;
  DL_SQUARE_EA:= glGenLists(1);
  glNewList(DL_SQUARE_EA,GL_COMPILE);
    bar(0,0,-ef.distance+0.01, r, r, 0);
  glEndList;

  // Shape 10.
  // circle with area equal to bar
  r := tan(0.5532*(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance;
  DL_CIRCLE_EA:= glGenLists(1);
  glNewList(DL_CIRCLE_EA,GL_COMPILE);
    circle(0,0,-ef.distance+0.01,r,0,20,true);
  glEndList;

   // Shape 11.
  // Star with radius equal to placeholder's radius. Area of 0.82627615cm2 on AOC monitor at 71cm
  // To make this equal area to bar, replace the decimal value below with 1.145
  r := tan(0.69230769  *(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance;
  DL_STAR:= glGenLists(1);
  glNewList(DL_STAR,GL_COMPILE);
   pentagram(0,0,-ef.distance+0.01, r);
  glEndList;

  DL_CIRCLE_BACKGROUND:= glGenLists(1);
  glNewList(DL_CIRCLE_BACKGROUND,GL_COMPILE);
    circle(0,0,-ef.distance-0.2,backgroundRadiusCM,0,100,true);
  glEndList;


 // Photodiode patches
  DL_PHOTODIODE_PATCH_LEFT:= glGenLists(1);
  glNewList(DL_PHOTODIODE_PATCH_LEFT,GL_COMPILE);
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity;
    gltranslatef((-ef.WidthCM/2) + 2,(ef.HeightCM/2) - 1.4,0);
    glcolor3f(1,1,1);
    bar(0,0,-ef.distance+0.01, PHOTODIODE_PATCH_SIZE_CM,PHOTODIODE_PATCH_SIZE_CM, 0);
  glEndList;

  DL_PHOTODIODE_PATCH_RIGHT:= glGenLists(1);
  glNewList(DL_PHOTODIODE_PATCH_RIGHT,GL_COMPILE);
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity;
    gltranslatef((ef.WidthCM/2) - 2,(ef.HeightCM/2) - 1.4,0);
    glcolor3f(1,1,1);
    bar(0,0,-ef.distance+0.01, PHOTODIODE_PATCH_SIZE_CM,PHOTODIODE_PATCH_SIZE_CM, 0);
  glEndList;

    DL_PHOTODIODE_PATCH_CENTRE:= glGenLists(1);
  glNewList(DL_PHOTODIODE_PATCH_CENTRE,GL_COMPILE);
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity;
    gltranslatef(0,(ef.HeightCM/2) - 1.4,0);
    glcolor3f(1,1,1);
    bar(0,0,-ef.distance+0.01, PHOTODIODE_PATCH_SIZE_CM,PHOTODIODE_PATCH_SIZE_CM, 0);
  glEndList;

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// work out the x and y location of the trajectory object on a given quadrant
procedure objectLocation(var x: real; var y:real ; cueRadiusCM: real; cueQuadrant: integer);
begin
  case cueQuadrant of

    1: // 45degrees "North West"
    begin
      x :=  -cueRadiusCM*cos(pi/4);
      y :=  cueRadiusCM*sin(pi/4);
    end;

    2:
    begin
      x :=  cueRadiusCM*cos(pi/4);
      y :=  cueRadiusCM*sin(pi/4);
    end;

    3:
    begin
      x :=  cueRadiusCM*cos(pi/4);
      y :=  -cueRadiusCM*sin(pi/4);
    end;

    4:
    begin
      x :=  -cueRadiusCM*cos(pi/4);
      y :=  -cueRadiusCM*sin(pi/4);
    end;

    5:    //special case of all quadrants
    begin
      x :=  0;
      y :=  0;
    end;

  end;

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure drawShape(shapeNo:integer);
begin

  case shapeNo of
   1: glCallList(DL_DIAMOND);
   2: glCallList(DL_HEX);
   3: glCallList(DL_TRIANGLE);
   4: glCallList(DL_BOX);
   5: glCallList(DL_RING);
   6: glCallList(DL_CROSS);
   7: glCallList(DL_BAR_HORIZ);
   8: glCallList(DL_BAR_VERT);
   9: glCallList(DL_SQUARE_EA);
   10: glCallList(DL_CIRCLE_EA);
   11: glCallList(DL_STAR);

 end;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure DrawShapeOrChar(shapeNo:integer; shapeCol, bgrCol:TcolourReal;x,y:real);
begin

  case (shapeNo) of

  0..32:
    begin
      glcolor3f(shapeCol.r,shapeCol.g,shapeCol.b);
      drawShape(shapeNo);
    end;

  33..255:
    begin
      drawTextStim(pfontStim1,shapeCol,Run_background_circle_colour,x,y,char(shapeNo));
    end;

  300..397:
    begin
       displayBMPimageXYcm(BMPimages,shapeNo-300, x,y, ef.Width, ef.height, ef.WidthCM, ef.heightCM);
    end;

  1033..1255:
    begin
      drawTextStim(pfontStim2,shapeCol,Run_background_circle_colour,x,y,char(shapeNo-1000));
    end;

  end;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------

procedure drawBackgroundFixation(fixSpotSizeCM:real; contextColour:TcolourReal;Fixation_and_placeholders_colour:TcolourReal  );
begin

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      drawFixationSpot(fixSpotSizeCM,Fixation_and_placeholders_colour);
      glcolor3f(contextColour.r,contextColour.g,contextColour.b);
      glCallList(DL_CIRCLE_BACKGROUND);
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure drawBackgroundFixationWithPlaceholders(fixSpotSizeCM:real; targetRadiusCM: real; contextColour:TcolourReal;Fixation_and_placeholders_colour:TcolourReal  );

var x,y:real;
  begin

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      drawFixationSpot(fixSpotSizeCM,Fixation_and_placeholders_colour);
      glcolor3f(contextColour.r,contextColour.g,contextColour.b);
      glCallList(DL_CIRCLE_BACKGROUND);

      glcolor3f(Fixation_and_placeholders_colour.r,Fixation_and_placeholders_colour.g,Fixation_and_placeholders_colour.b);

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      objectLocation(x,y,targetRadiusCM,1);
      gltranslatef(x,y,0);
      glCallList(DL_CIRCLE_OUTLINE);

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      objectLocation(x,y,targetRadiusCM,2);
      gltranslatef(x,y,0);
      glCallList(DL_CIRCLE_OUTLINE);

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      objectLocation(x,y,targetRadiusCM,3);
      gltranslatef(x,y,0);
      glCallList(DL_CIRCLE_OUTLINE);

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      objectLocation(x,y,targetRadiusCM,4);
      gltranslatef(x,y,0);
      glCallList(DL_CIRCLE_OUTLINE);

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------

procedure drawFixationWithPlaceholders(fixSpotSizeCM:real; targetRadiusCM: real;Fixation_and_placeholders_colour:TcolourReal;
  doDrawFixationSpot: Boolean = true);

var x,y:real;
  begin

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      if doDrawFixationSpot then
        drawFixationSpot(fixSpotSizeCM,Fixation_and_placeholders_colour);

      glcolor3f(Fixation_and_placeholders_colour.r,Fixation_and_placeholders_colour.g,Fixation_and_placeholders_colour.b);

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      objectLocation(x,y,targetRadiusCM,1);
      gltranslatef(x,y,0);
      glCallList(DL_CIRCLE_OUTLINE);

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      objectLocation(x,y,targetRadiusCM,2);
      gltranslatef(x,y,0);
      glCallList(DL_CIRCLE_OUTLINE);

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      objectLocation(x,y,targetRadiusCM,3);
      gltranslatef(x,y,0);
      glCallList(DL_CIRCLE_OUTLINE);

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      objectLocation(x,y,targetRadiusCM,4);
      gltranslatef(x,y,0);
      glCallList(DL_CIRCLE_OUTLINE);

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure targetImage(targetRadiusCM: real;  targetShape:integer; targetQuadrant,distractorShape, target_colour, distractor_colour: integer;
  drawPeripheral: boolean; showPlaceholderCentre: Boolean );


var
  x,y, theta:real;
  c:integer;

begin

  for c :=1 to 16 do
  begin

    if (not( ((c=2) and (targetQuadrant=2)) or ((c=6) and (targetQuadrant=3)) or
         ((c=10) and (targetQuadrant=4)) or  ((c=14) and (targetQuadrant=1)) ) or (targetshape=0))  then
    begin
      if drawPeripheral then
      begin

      // draw distractors
      theta := ((2*pi)/16)*c;
      x :=  targetRadiusCM * sin(theta);
      y :=  targetRadiusCM * cos(theta);

      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
      gltranslatef(x,y,0);

      //glcolor3f(colours[distractor_colour].r,colours[distractor_colour].g,colours[distractor_colour].b);

      //drawShape(distractorShape);
      DrawShapeOrChar(distractorShape,colours[distractor_colour], Run_background_circle_colour,x,y);
      //if (distractorShape<1033)then begin drawShape(distractorShape); end else begin drawTextStim(pfont2,colours[distractor_colour],Run_background_circle_colour,x,y,char(distractorShape-1000)); end;

      // draw outline of circle
      glcolor3f(Fixation_and_placeholders_colour.r,Fixation_and_placeholders_colour.g,Fixation_and_placeholders_colour.b);
      glCallList(DL_CIRCLE_OUTLINE);
      end;
    end;

    // draw target
    if (targetshape<>0) then
    begin

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity;
    objectLocation(x,y,targetRadiusCM,targetQuadrant);
    gltranslatef(x,y,0);

    //glcolor3f(colours[target_colour].r,colours[target_colour].g,colours[target_colour].b);

    //drawShape(targetShape);

    DrawShapeOrChar(targetShape,colours[target_colour], Run_background_circle_colour,x,y);
    //if (targetShape<1033)then begin drawShape(targetShape); end else begin drawTextStim(pfont2,colours[target_colour],Run_background_circle_colour,x,y,char(targetShape-1000)); end;

    // draw outline of circle
    if (showPlaceholderCentre or (targetQuadrant <> 5)) then
    begin
    glcolor3f(Fixation_and_placeholders_colour.r,Fixation_and_placeholders_colour.g,Fixation_and_placeholders_colour.b);
    glCallList(DL_CIRCLE_OUTLINE);
    end;
    end;

  end;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure drawS2cues(s2_shape_position_1, s2_shape_position_2, s2_shape_position_3, s2_shape_position_4, s2_shape_position_5,
  s2_colour_position_1, s2_colour_position_2, s2_colour_position_3, s2_colour_position_4, s2_colour_position_5: integer);

var
  c:integer;
  x,y,theta,radiusCM:real;
  shape,col:integer;

const
  radiusDeg = 2;

begin

  for c:=1 to 5 do
  begin

    case c of
      1: shape := s2_shape_position_1;
      2: shape := s2_shape_position_2;
      3: shape := s2_shape_position_3;
      4: shape := s2_shape_position_4;
      5: shape := s2_shape_position_5;
    end;

    case c of
      1: col := s2_colour_position_1;
      2: col := s2_colour_position_2;
      3: col := s2_colour_position_3;
      4: col := s2_colour_position_4;
      5: col := s2_colour_position_5;
    end;

   // glcolor3f(colours[col].r,colours[col].g,colours[col].b);

    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity;
    radiusCM := ef.distance * (radiusDeg*(pi/180));
    objectLocation(x,y,radiusCM,c);
    gltranslatef(x,y,0);

    //drawShape(shape);
    DrawShapeOrChar(shape,colours[col], Run_background_circle_colour,x,y)
  end;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure checkKeypressAndButtonbox(var state:integer; var eventTime : uint32);
var
  dataValue :word;
begin

  pollevent(state, eventTime) ;

 //  if ((state <> -1) ) then showmessage('checkKeypressAndButtonbox' + inttostr(state));

  // keyboard 'a' or keypad 1, or mouse button left.
  if ((state = SDLK_a) or (state = SDLK_KP_1) or (state = SDL_BUTTON_LEFT)) then state := RESPONSE_LEFT_BUTTON;

  // keyboard 'f' or keypad 4 or mouse button right
  if ((state = SDLK_f) or (state = SDLK_KP_4) or (state = SDL_BUTTON_RIGHT)) then state := RESPONSE_RIGHT_BUTTON;


  if (state = RESPONSE_LEFT_BUTTON) then
  begin
 //   showmessage('LEFT');
  end;
  if (state = RESPONSE_RIGHT_BUTTON) then
  begin
  //  showmessage('RIGHT');
  end;

  // only accept left or right responses
  if ((state<> RESPONSE_LEFT_BUTTON) and (state<>RESPONSE_RIGHT_BUTTON)) then
  begin
    state :=-1;
    eventTime:=0;
  end;

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure rect(hsize,vsize,x,y:real);
begin
  glBegin(gl_polygon);
    glVertex2f(x- (hsize/2), y+ (vsize/2) );
    glVertex2f(x+ (hsize/2), y+ (vsize/2) );
    glVertex2f(x+ (hsize/2), y- (vsize/2) );
    glVertex2f(x- (hsize/2), y- (vsize/2) );
    glVertex2f(x- (hsize/2), y+ (vsize/2) );
  glEnd;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure showCountdown(pfont:PTTF_font ;col1:PSDL_color ;text:string);

begin
glpushmatrix();
glviewport(0,0,ef.width,ef.height);
glMatrixMode(GL_PROJECTION);
glLoadIdentity;
glMatrixMode(GL_MODELVIEW);
glLoadIdentity;
glcolor3f(1,1,1);
glortho(-ef.width/2,ef.width/2,-ef.height/2,ef.height/2,-1,1);
renderText(pfont,'Remaining:' + text, ef.width*0.35,-ef.height*0.5,col1,1 ) ;
//renderText(pfont,text, ef.width*0.45,-ef.height*0.5,col1,1 ) ;
glpopmatrix()

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure showTextFeedback(pfont:PTTF_font ;fontCol,bgrCol:PSDL_color; text:string);

begin
  glpushmatrix();
  glviewport(0,0,ef.width,ef.height);
  glMatrixMode(GL_PROJECTION);
  glLoadIdentity;
  glMatrixMode(GL_MODELVIEW);
  glLoadIdentity;

  glortho(-ef.width/2,ef.width/2,-ef.height/2,ef.height/2,-1,1);
  renderTextWithBackgroundColCentred(pfont,text,fontCol,bgrCol,0,0,1 ) ;

  glpopmatrix();
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function unitCol(colour:TcolourInteger):TcolourReal;
begin
result.r:=colour.r/255;
result.g:=colour.g/255;
result.b:=colour.b/255;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
// if paused by user ('suspended') then show a message until unpaused.
procedure handledSuspended(var isRuinedTrial: integer);

var
  state, c,Nframes: integer  ;
  eventTime : uint32 ;
  fontCol, fontBgrColBlack:pSDL_color;

  wasSuspended:boolean = false;
begin
    // set font colours
  new(fontCol);
  fontCol^.r:=255; fontCol^.g:=255; fontCol^.b:=255; fontCol^.a:=0;

  new(fontBgrColBlack);
  fontBgrColBlack^.r := 0;
  fontBgrColBlack^.g := 0;
  fontBgrColBlack^.b := 0;
  fontBgrColBlack^.a :=0;

  while IS_SUSPENDED do
  begin
    pollevent(state, eventTime) ;

    ef.ProjectionTrans;
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity;
    drawBackgroundFixation(fixSpotSizeCM,Pause_background_circle_colour, Fixation_and_placeholders_colour);
    drawFixationWithPlaceholders(fixSpotSizeCM,targetRadiusCM, Fixation_and_placeholders_colour);
    glpushmatrix();
    glviewport(0,0,ef.width,ef.height);
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity;
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity;
    glcolor3f(1,1,1);
    glortho(-ef.width/2,ef.width/2,-ef.height/2,ef.height/2,-1,1);
    renderTextWithBackgroundColCentred(pfontGeneral,  'PAUSED', fontCol, fontBgrColBlack, 0,0,1);
    glpopmatrix();
    ef.renderStereo;

    isRuinedTrial:=1; // flag that the trial is garbage
   // showmessage('suspended');
   if not(IS_SUSPENDED) then
   begin
      // 2 sec pause   after resuming from suspense
      Nframes:=round (2 / (1/REFRESH_RATE))  ;
      for c:=0 to Nframes-1 do
      begin
        ef.ProjectionTrans;
        glMatrixMode(GL_MODELVIEW);
        glLoadIdentity;
        drawBackgroundFixation(fixSpotSizeCM,Run_background_circle_colour, Fixation_and_placeholders_colour);
        drawFixationWithPlaceholders(fixSpotSizeCM,targetRadiusCM, Fixation_and_placeholders_colour);
        ef.renderStereo;
      end;
      ef.ProjectionTrans;
        glMatrixMode(GL_MODELVIEW);
        glLoadIdentity;
        drawBackgroundFixation(fixSpotSizeCM,Run_background_circle_colour, Fixation_and_placeholders_colour);
        drawFixationWithPlaceholders(fixSpotSizeCM,targetRadiusCM, Fixation_and_placeholders_colour);
    end;
  end;
end;
//------------------------------------------------------------------------------





//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
//------------------------------------------------------------------------------
procedure TForm1.RunExperiment(Sender: TObject);

var
  DataValue :word; //DAS6014 auxport vals

  configDataFilename :string;
  inputDataFilename: string;
  outputDataFilename: string;
  f: TextFile;
  fTest : TextFile;
  frameTimingFile  : string;


  observerNo: integer;
  sessionNo: integer;
  experiment_part_no : integer;

  Ntrials, trialNo: integer;

  //NtrialsBeforePause: integer;

  observedDataRTRecord:  array of integer;
  RT_ms:integer;


  observedDataResponseRecord, observedDataCorrectResponseRecord:  array of integer;
  timeStampRecord:  array of integer;
  timeStamp_ms:integer;

  timer1, timer2,TimerPulse: TTimer;

  state: integer;
  informativeCueShape: integer; //0: none, 1: diamond, 2: hexagon, 3: circle
  targetShape: integer;     //1: diamond, 2: hexagon, 3: circle

  s1_colour, s2_colour_position_1, s2_colour_position_2, s2_colour_position_3, s2_colour_position_4, s2_colour_position_5, s3_colour, s3_distractor_colour, s4_colour, s4_distractor_colour: integer;


  //0: target is black, 1: traj+target col1 & distractor col2, 2 traj+target col2 & distractor col1,
  keyMapping: integer; // 0: keypad 0=same, .=different ; 1: vice-versa
  motionOrStatic: integer;// 0:static, 1:motion

  initialFixationDuration_ms:integer; // duration of initial 'blank' display
  initialFixationDuration:real;
  cueDuration_ms, ISIDuration_ms, targetDuration_ms: integer; //duration of trajectory  and trajectory offset in ms
  cueDuration, ISIDuration: real; //duration of trajectory  and trajectory offset in seconds
  targetDuration:real; //duration of target presentation in seconds

  //timeOfFirstImageOnset, timeOfFirstBlankImageOnset: real;//

  s1_onsetTime,
  s2_onsetTime,
  s3_onsetTime,
  s4_onsetTime,
  response_onsetTime,
  feedback_onsetTime,
  blank_onsetTime,    // ( 1000ms after auditory feedback)
  TMS_onsetTime:integer;


 s1_marker,s2_marker,s3_marker,s4_marker:integer;




  s1_shape, s1_quad: integer;
  s1_duration, s1_s2_isi: integer;

  s2_shape_position_1, s2_shape_position_2, s2_shape_position_3, s2_shape_position_4, s2_shape_position_5: integer;
  s2_duration, s2_s3_isi: integer;

  s3_shape, s3_distractor_shape, s3_quad: integer;
  s3_duration, s3_s4_isi: integer;

  s4_shape, s4_distractor_shape, s4_quad:integer;
  s4_duration: integer;

  Feedback_shape: integer;

  TMS_s3_SOA: integer;



  Experimental_Condition:string;

  //max_response_time: real; //maximum duration allowed for response, after which the program advances to the next trial
  Response_Time_after_S4:  integer;
  Feedback_duration_after_response_time: integer;
  ITI_after_feedback:  integer;

  taskType: integer; // task type 0 = shape task, 1= colour task
  cueRadiusCM: real;

  cueCurrentRadiusCM: real; //object's current radius;

  backgroundRadiusCM:real;

  x,y: real;  //locations in cm of trajectory object

  trialDone: boolean; // advance trial when true;
  hasResponded : boolean; // has responded?
  triggerState:boolean; //trigger for one-shot sending of parallel port data
  hasBlankedTheTarget:boolean; //flag for whether the target has been blanked or not
  Nframes:integer;
  frameNo:integer;



  isAuditoryFeedback:boolean;
  isTargetMatchesCue:boolean; //flag set if target (s4) matches cue (s2) on the task dimension (either shape or colour, according to taskType)

 // parallelPortPulseDur :integer;


  doPhotodiode:boolean;
  triggerStationData:integer;

  c,k: integer; //general purpose counter
  photodiodeReading: integer;
  photodiodeReadings : array[0..9] of real;
  photodiodeThreshold: integer;

  text:string;
  fontCol, fontColStim, font_col_correct, font_col_incorrect, fontBgrCol, fontBgrColBlack:pSDL_color;

  showTrialsRemaining:boolean;

  isBaselineCondition:boolean;


  // strings for output file
  experimentName:string;
  initials:string;
  age:string;
  sex:string;
  handedness:string;
  currentDate:string;
  currentTime:string;
  displayType:string;
  {
  colours: array [0..15] of TcolourReal;

  Pause_background_circle_colour : TcolourReal;
  Run_background_circle_colour : TcolourReal ;
  Fixation_and_placeholders_colour: TcolourReal ;
  }
  Minimum_training_accuracy: real;
  N_trials_before_pause: integer;
  N_trials_before_pause_main : integer;
  N_trials_before_pause_training: integer;

  Instructions_ODD_participants : string;
  Instructions_EVEN_participants : string;

  Stimulus_font_1, Stimulus_font_2, Feedback_font :string;
  Stimulus_font_1_size, Stimulus_font_2_size, Feedback_font_size :real;
  Stimulus_font_1_style, Stimulus_font_2_style, Feedback_font_style:string;
  Feedback_text_correct, Feedback_text_incorrect : string;

  RT_constant_error_ms:integer;
  RT_ms_minus_constant_error: integer;

  experiment_dir:string;

  // Timing variables
  t1: uint32; //time before s4 is drawn
  timeOfExperimentStart : uint32;
  timeOfS4Start : uint32;
  eventTime:uint32;

  // response state and time
  responseState:integer;
  responseEventTime:uint32;

 // triggerStationRT:integer; // RT measured via triggerstation: time between photodiode patch detection and button press
  TMS_frameNo:integer;
  frameNoTotal:integer = 0;
  s3_onset_frameNo : integer =  0;

  tot:integer;

  Monitor_name:string;
  Monitor_width_cm, Monitor_height_cm :real;
  Monitor_resolution_h, Monitor_resolution_v :integer;
  Monitor_refresh_rate :integer;
  Monitor_distance_cm :real;

  //pauseTrialsData: array of TpauseTrialsData;
  ptd: Tptd;
  isPauseTrial:boolean = false;

  isRuinedTrial :integer = 0; // flag if user has suspended the trial (garbage trial)
  Show_S3_photodiode_patch:integer = 0;
  Show_S4_photodiode_patch:integer = 0;
  Show_S3_peripheral_placeholders: Boolean = true;
  Show_S4_peripheral_placeholders: Boolean = true;
  Show_S3_placeholder_when_centre: Boolean = true;
  Show_S4_placeholder_when_centre: Boolean = true;

  isVeryFirstTrial : boolean = true; // flag which is cleared after the very first trial is started to beginthe main timer, so if a block of trials is
                                     // repeated because a participant fails to reach criterion, then the timer isn't restarted




 // bmp related variables
     BMPtextureID: GLUint;
     BMPwidthPixels, BMPheightPixels:integer;



const
  show_s1:boolean=true;
  show_s2:boolean=true;
  show_s3:boolean=true;
  doCheckForResponse:boolean=true;

 parallelPortPulseDur = 0.002;
  placeholderRadiusDeg = SCALE_FACTOR * 0.69230769;//1.0;
  backgroundRadiusDeg = SCALE_FACTOR  * 9; //13;
  fixSpotSizeDeg = SCALE_FACTOR  * 0.069230769 ;

  // target parameters
  targetRadiusDeg = SCALE_FACTOR  * 6.923;// 10; // radius of target position in degrees





  bgrColour: array [0..2] of real = (0, 0, 0);



 // NphotodiodeCalibImages = 10; // number of photodiode calibration images



begin
  N_trials_before_pause_main := 10000;
  N_trials_before_pause_training := 0;

  // get the current directory
  currentDir := getCurrentDir;

  // get the directory where files and folders for this experiment are located
  experiment_dir:=extractfilepath(Opendialog1.filename);
  //showmessage('Ex dir: '+ experiment_dir);

 // observerNo:= strtoint(Combobox1.items[Combobox1.Itemindex]);
  observerNo:=0;
  sessionNo:= strtoint(Combobox2.items[Combobox2.Itemindex]);
  experiment_part_no :=  strtoint(Combobox4.items[Combobox4.Itemindex]);






  //////////////////////////////////////////////////////////////////////////////
  // Read experiment parameters from Configuration.txt file

  configDataFilename := experiment_dir +  'Configuration.txt';

  if not FileExists(configDataFilename)  then
  begin
    showmessage('File does not exist: '+ configDataFilename);
    terminateApplication;
  end;


  experimentName:=GetExperimentName(configDataFilename);

  Monitor_name:= getStringForParameter(configDataFilename, 'Monitor_name:');
  radiogroup4.items[0]:= Monitor_name;
  Monitor_width_cm := getRealForParameter(configDataFilename, 'Monitor_width_cm:');
  Monitor_height_cm := getRealForParameter(configDataFilename, 'Monitor_height_cm:');
  Monitor_resolution_h := getIntegerForParameter(configDataFilename, 'Monitor_resolution_h:');
  Monitor_resolution_v := getIntegerForParameter(configDataFilename, 'Monitor_resolution_v:');
  Monitor_refresh_rate := getIntegerForParameter(configDataFilename, 'Monitor_refresh_rate:');
  Monitor_distance_cm := getRealForParameter(configDataFilename, 'Monitor_distance_cm:');

  Placeholder_diameter_deg := getRealForParameter(configDataFilename, 'Placeholder_diameter_deg:');
  Shape_scale_factor := getRealForParameter(configDataFilename, 'Shape_scale_factor:');

  Minimum_training_accuracy := getRealForParameter(configDataFilename, 'Minimum_training_accuracy:');

  N_trials_before_pause_training := getIntegerForParameter(configDataFilename, 'N_trials_before_pause_training:');
  N_trials_before_pause_main := getIntegerForParameter(configDataFilename, 'N_trials_before_pause_main:');

  Instructions_ODD_participants:= getStringForParameter(configDataFilename, 'Instructions_ODD_participants:');
  Instructions_EVEN_participants:= getStringForParameter(configDataFilename, 'Instructions_EVEN_participants:');

  Stimulus_font_1:= getStringForParameter(configDataFilename, 'Stimulus_font_1:');
  Stimulus_font_1_size := getRealForParameter(configDataFilename, 'Stimulus_font_1_size:');
  Stimulus_font_1_style:= getStringForParameter(configDataFilename, 'Stimulus_font_1_style:');

  Stimulus_font_2:= getStringForParameter(configDataFilename, 'Stimulus_font_2:');
  Stimulus_font_2_size := getRealForParameter(configDataFilename, 'Stimulus_font_2_size:');
  Stimulus_font_2_style:= getStringForParameter(configDataFilename, 'Stimulus_font_2_style:');

  Feedback_font:= getStringForParameter(configDataFilename, 'Feedback_font:');
  Feedback_font_size := getRealForParameter(configDataFilename, 'Feedback_font_size:');
  Feedback_font_style:= getStringForParameter(configDataFilename, 'Feedback_font_style:');

  Feedback_text_correct := getStringLineForParameter(configDataFilename, 'Feedback_text_correct:');
  Feedback_text_incorrect := getStringLineForParameter(configDataFilename, 'Feedback_text_incorrect:');
  RT_constant_error_ms := getIntegerForParameter(configDataFilename, 'RT_constant_error_ms:');

  if (comparestr(Feedback_font, 'symbola.ttf')=0) then
  begin
    Feedback_text_correct := #$E2#$98#$BA    ; // smiley
    Feedback_text_incorrect := #$E2#$98#$B9    ; //sad
  end;

  getColoursForParameter(configDataFilename, 'Pause_background_circle_colour:', Pause_background_circle_colour);
  getColoursForParameter(configDataFilename, 'Run_background_circle_colour:', Run_background_circle_colour);
  getColoursForParameter(configDataFilename, 'Fixation_&_placeholders_colour:', Fixation_and_placeholders_colour);

  getColoursForParameter(configDataFilename, 'Incorrect_feedback_colour:', Incorrect_feedback_colour);
  getColoursForParameter(configDataFilename, 'Correct_feedback_colour:', Correct_feedback_colour);

  getShapeColours(configDataFilename,colours)  ;

  Monitor_name:= getStringForParameter(configDataFilename, 'Monitor_name:');

  Show_S3_photodiode_patch := getIntegerForParameter(configDataFilename, 'Show_S3_photodiode_patch:');

  Show_S4_photodiode_patch := getIntegerForParameter(configDataFilename, 'Show_S4_photodiode_patch:');
  Show_S3_peripheral_placeholders := getIntegerForParameter(configDataFilename, 'Show_S3_peripheral_placeholders:') <> 0;
  Show_S4_peripheral_placeholders := getIntegerForParameter(configDataFilename, 'Show_S4_peripheral_placeholders:') <> 0;
  Show_S3_placeholder_when_centre := getIntegerForParameter(configDataFilename, 'Show_S3_placeholder_when_centre:') <> 0;
  Show_S4_placeholder_when_centre := getIntegerForParameter(configDataFilename, 'Show_S4_placeholder_when_centre:') <> 0;



  //////////////////////////////////////////////////////////////////////////////

  //////////////////////////////////////////////////////////////////////////////
  // Check  InputDataFileName data

  inputDataFilename := experiment_dir + 'Input data\' + 'InputData_'+inttostr(trialOrderFileNo)+'.txt';
  if not FileExists(inputDataFilename)  then
  begin
    showmessage('File does not exist: '+ inputDataFilename);
    terminateApplication;
  end;

  // Check if Config and InputData files match

  if (comparestr(GetExperimentName(configDataFilename), GetExperimentName(inputDataFilename))<>0) then
  begin
    showmessage('Experiment name mismatch between input data and configuration data files');
    terminateApplication;
  end;
  //////////////////////////////////////////////////////////////////////////////


   // set display properties
   radiogroup4.items[0]:=Monitor_name;
  DISPLAY_TYPE := radiogroup4.itemindex;

 // DISPLAY_TYPE := DISPLAY_PC_LAB;

  case (DISPLAY_TYPE) of

    //PC LAB
    DISPLAY_PC_LAB:
    begin
      REFRESH_RATE := Monitor_refresh_rate;
      ef.Width := Monitor_resolution_h;
      ef.Height := Monitor_resolution_v;
      ef.widthCM := Monitor_width_cm;
      ef.heightCM := Monitor_height_cm;
      ef.distance := Monitor_distance_cm;
    end;

    //Giorgio gsync
    DISPLAY_GIORGIO_GSYNC:
    begin
      REFRESH_RATE := 100;
      ef.Width :=1920;// 1024;
      ef.Height := 1080;//768;//1024;
      ef.widthCM := 53.2;//40;
      ef.heightCM := 30;//30;
      ef.distance := 71;//57;
    end;

    {
    //Phil gsync
    DISPLAY_PHIL_GSYNC:
    begin
      REFRESH_RATE := 100;
      ef.Width :=1920;// 1024;
      ef.Height := 1080;//768;//1024;
      ef.widthCM := 53.1;//40;
      ef.heightCM := 29.8;//30;
      ef.distance := 57;
    end;


    //fMRI. Viewing distance  = 71cm, screen max width ~50cm height ~39cm,
    // estimated at 50 and 37.5 for a 4:3 display
    DISPLAY_FMRI:
    begin
      showTrialsRemaining:=true;
      REFRESH_RATE := 60;
      ef.Width :=1024;// 1024;
      ef.Height := 768;//768;//1024;
      ef.widthCM := 50;
      ef.heightCM := 37.5;
      ef.distance := 71;
    end;


    //CRT
    DISPLAY_CRT:
    begin
      REFRESH_RATE := 100;
      ef.Width := 1024;
      ef.Height := 768;
      ef.widthCM := 40;
      ef.heightCM := 30;
      ef.distance := 57;
    end;
    }


  end;













  // open font for general text display
  if (TTF_init=-1) then TerminateApplication;
  pfontGeneral:=TTF_openFont('C:\windows\fonts\arial.ttf',round(2*(ef.width/ef.widthCM) * (ef.distance/57)));
  ttf_setFontStyle(pfontGeneral,TTF_STYLE_NORMAL);



  // open font 1 for stimulus display
  pfontStim1:=TTF_openFont(pchar('C:\windows\fonts\'+ Stimulus_font_1) ,round(Stimulus_font_1_size*(ef.width/ef.widthCM) * (ef.distance/57)));

  if (comparestr(Stimulus_font_1_style,'normal')=0) then
  begin
    ttf_setFontStyle(pfontStim1,TTF_STYLE_NORMAL);
  end;

  if (comparestr(Stimulus_font_1_style,'bold')=0) then
  begin
    ttf_setFontStyle(pfontStim1,TTF_STYLE_BOLD);
  end;

  if (comparestr(Stimulus_font_1_style,'italic')=0) then
  begin
    ttf_setFontStyle(pfontStim1,TTF_STYLE_ITALIC);
  end;


  // open font 2 for stimulus display
  pfontStim2:=TTF_openFont(pchar('C:\windows\fonts\'+ Stimulus_font_2) ,round(Stimulus_font_2_size*(ef.width/ef.widthCM) * (ef.distance/57)));

  if (comparestr(Stimulus_font_2_style,'normal')=0) then
  begin
    ttf_setFontStyle(pfontStim2,TTF_STYLE_NORMAL);
  end;

  if (comparestr(Stimulus_font_2_style,'bold')=0) then
  begin
    ttf_setFontStyle(pfontStim2,TTF_STYLE_BOLD);
  end;

  if (comparestr(Stimulus_font_2_style,'italic')=0) then
  begin
    ttf_setFontStyle(pfontStim2,TTF_STYLE_ITALIC);
  end;


    // open font for feedback
  pfontFeedback:=TTF_openFont(pchar('C:\windows\fonts\'+ Feedback_font) ,round(Feedback_font_size*(ef.width/ef.widthCM) * (ef.distance/57)));

  if (comparestr(Feedback_font_style,'normal')=0) then
  begin
    ttf_setFontStyle(pfontFeedback,TTF_STYLE_NORMAL);
  end;

  if (comparestr(Feedback_font_style,'bold')=0) then
  begin
    ttf_setFontStyle(pfontFeedback,TTF_STYLE_BOLD);
  end;

  if (comparestr(Feedback_font_style,'italic')=0) then
  begin
    ttf_setFontStyle(pfontFeedback,TTF_STYLE_ITALIC);
  end;




  // set font colours
  new(fontCol);
  fontCol^.r:=255; fontCol^.g:=255; fontCol^.b:=255; fontCol^.a:=0;

  new(fontColStim);
  fontColStim^.r:=255; fontColStim^.g:=255; fontColStim^.b:=255; fontColStim^.a:=0;

  new(font_col_correct);
  font_col_correct^.r := round(Correct_feedback_colour.r*255);
  font_col_correct^.g := round(Correct_feedback_colour.g*255);
  font_col_correct^.b := round(Correct_feedback_colour.b*255);
  font_col_correct^.a :=0;

  new(font_col_incorrect);
  font_col_incorrect^.r := round(Incorrect_feedback_colour.r*255);
  font_col_incorrect^.g := round(Incorrect_feedback_colour.g*255);
  font_col_incorrect^.b := round(Incorrect_feedback_colour.b*255);
  font_col_incorrect^.a :=0;

  new(fontBgrCol);
  fontBgrCol^.r := round(Run_background_circle_colour.r*255);
  fontBgrCol^.g := round(Run_background_circle_colour.g*255);
  fontBgrCol^.b := round(Run_background_circle_colour.b*255);
 fontBgrCol^.a :=0;

   new(fontBgrColBlack);
  fontBgrColBlack^.r := 0;
  fontBgrColBlack^.g := 0;
  fontBgrColBlack^.b := 0;
 fontBgrColBlack^.a :=0;



  showTrialsRemaining:=false;


   // set trigger station parameters
  if (isTriggerstation) then
  begin
    TriggerStation.ParallelPort(0);
    triggerStation.WriteSharedRam(1,1); // enable photodiode 1 (left side)
    triggerStation.WriteSharedRam(2,1); // enable photodiode 2 (right side)
    triggerStation.WriteSharedRam(3,1); // enable BNC1 for the TMS. (BNC1 triggers TMS 1ms after Photodiode 1 is triggered)
  end;

  fixSpotSizeCM:=ef.distance*(fixSpotSizeDeg*(pi/180));



  //showmessage(experimentName);
  // find how many trials there are in the file
  GetNtrials(inputDataFilename, sessionNo, Ntrials);

  //Initialise the array that will contain the observed data and record of the v modulation
  setlength(observedDataRTRecord,Ntrials);
  setlength(observedDataResponseRecord,Ntrials);
  setlength(observedDataCorrectResponseRecord,Ntrials);
  SetLength(timestampRecord,NTrials);

  //showmessage(inttostr(Ntrials));

  // create timer objects
  timer1 :=TTimer.create;
  timer2 :=TTimer.create;
  TimerPulse := TTimer.create;








  ef.SetDisplayType(Cyclopean);

  ef.displayWindowXpos:=0;//1280;
  //ef.distance := 57;
  ef.iod:=0;
  ef.Refreshform;
  ef.SDLsetup;
  ef.positionSDLwindow;

  // load sounds
  loadSounds;
  //Mix_PlayChannel(Ord(CORRECT_WAV),  sounds[Ord(CORRECT_WAV)], 0);

  // initialise openGL
  InitGL;

  // create displaylists;
  backgroundRadiusCM := tan(backgroundRadiusDeg*(pi/180))*ef.distance;
  makeDisplayLists(tan(placeholderRadiusDeg*(pi/180))*ef.distance, backgroundRadiusCM );


  //load images

   loadBMPimages (experiment_dir, BMPimages);


  //----------------------------------------------------------------------------
  // frame timing output file
    {
  frameTimingFile := experiment_dir + 'Output data\' + 'frametimes.txt';
  AssignFile(fTest, frameTimingFile);
  if FileExists(frameTimingFile) then
  begin
    Append(fTest);
  end
  else
  begin
    Rewrite(fTest);
  end;
        }




    //----------------------------------------------------------------------------
  // write header to observed data file
  writeHeader;


  // session details
  currentDate :=  DateToStr(date);
  currentTime :=  TimeToStr(time);
  initials :=  Form1.Edit1.text;
  age:= Form1.combobox3.items[Form1.combobox3.itemindex];
  sex :=  Form1.combobox13.items[Form1.combobox13.itemindex];
  handedness := Form1.combobox14.items[Form1.combobox14.itemindex];
  displayType:= radiogroup4.items[radiogroup4.itemindex];

  outputDataFilename := Form1.Savedialog1.filename;
  AssignFile(f, outputDataFilename);
  if FileExists(outputDataFilename) then
  begin
    Append(f);
  end
  else
  begin
    Rewrite(f);
  end;


  writeln(f, 'Experiment:' + #9+ experimentName);
  writeln(f, 'N_trials_before_pause_training:' + #9+ inttostr(N_trials_before_pause_training));
  writeln(f, 'N_trials_before_pause_main:' + #9+ inttostr(N_trials_before_pause_main));
  writeln(f, 'Instructions_ODD_participants:' + #9+ Instructions_ODD_participants);
  writeln(f, 'Instructions_EVEN_participants:' + #9+ Instructions_EVEN_participants);
  writeln(f, 'Pause_background_circle_colour:	' + #9+
    inttostr(round(Pause_background_circle_colour.r*255)) +','+
    inttostr(round(Pause_background_circle_colour.g*255)) +','+
    inttostr(round(Pause_background_circle_colour.b*255)));
  writeln(f, 'Run_background_circle_colour:	' + #9+
    inttostr(round(Run_background_circle_colour.r*255)) +','+
    inttostr(round(Run_background_circle_colour.g*255)) +','+
    inttostr(round(Run_background_circle_colour.b*255)));
  writeln(f, 'Fixation_&_placeholders_colour:	' + #9+
    inttostr(round(Fixation_and_placeholders_colour.r*255)) +','+
    inttostr(round(Fixation_and_placeholders_colour.g*255)) +','+
    inttostr(round(Fixation_and_placeholders_colour.b*255)));

  writeln(f, 'Incorrect_feedback_colour:	' + #9+
    inttostr(round(Incorrect_feedback_colour.r*255)) +','+
    inttostr(round(Incorrect_feedback_colour.g*255)) +','+
    inttostr(round(Incorrect_feedback_colour.b*255)));

  writeln(f, 'Correct_feedback_colour:	' + #9+
    inttostr(round(Correct_feedback_colour.r*255)) +','+
    inttostr(round(Correct_feedback_colour.g*255)) +','+
    inttostr(round(Correct_feedback_colour.b*255)));


  for c:=0 to 15 do
  begin
   writeln(f,'Shapes_colour_'+inttostr(c)+':' +#9+
   inttostr(round(colours[c].r*255)) +','+
   inttostr(round(colours[c].g*255)) +','+
   inttostr(round(colours[c].b*255)));
  end;
  writeln(f);

  writeln(f, 'Experiment' + #9+
             'Date' + #9+
             'Start_Time' +#9+
             'Trial_Order_File_No' +#9+
             'ParticipantID' +#9+
             'Age' +#9+
             'sex' +#9+
             'Handedness' +#9+
             'Display_Type' +#9+
             'Observer_number' +#9+
             'session_no'+ #9+
             's1_marker' + #9+
             's2_marker' + #9+
             's3_marker' + #9+
             's4_marker' + #9+
             //'TMS_marker' +#9+
             's1_shape' +#9+
             's1_quad' +#9+
             's1_duration' +#9+
             's1_s2_isi' +#9+
             's2_shape_position_1(NW)' +#9+
             's2_shape_position_2(NE)' +#9+
             's2_shape_position_3(SE)' +#9+
             's2_shape_position_4(SW)' +#9+
             's2_shape_position_5(centre)' +#9+
             's2_duration' +#9+
             's2_s3_isi' +#9+
             's3_shape' +#9+
             's3_distractor_shape' +#9+
             's3_quad' +#9+
             's3_duration' +#9+
             's3_s4_isi' +#9+
             's4_shape' +#9+
             's4_distractor_shape' +#9+
             's4_quad' +#9+
             's4_duration' +#9+
             'Response_Time_after_S4' +#9+
             'Feedback_shape' +#9+
             'Feedback_duration_after_response_time' +#9+
             'ITI_after_feedback' +#9+
             's1_colour' +#9+
             's2_colour_position_1(NW)' +#9+
             's2_colour_position_2(NE)' +#9+
             's2_colour_position_3(SE)' +#9+
             's2_colour_position_4(SW)' +#9+
             's2_colour_position_5(centre)' +#9+
             's3_colour' +#9+
             's3_distractor_colour' +#9+
             's4_colour' +#9+
             'S4_distractor_colour' +#9 +
             'keyMapping' +#9+
             'taskType' +#9+
             'TMS_s3_SOA' +#9+
             'Experimental_Condition' +#9+
             'response' +#9+
             'observedDataCorrectResponseRecord' +#9+
             'RT_ms' +#9+
             'RT_ms_minus_constant_error' +#9+
          //   'timeStamp_ms' +#9+
             's1_onsetTime_ms'+#9+
             's2_onsetTime_ms'+#9+
             's3_onsetTime_ms'+#9+
             'TMS_onsetTime_ms'+#9+
             's4_onsetTime_ms'+#9+
             'response_onsetTime_ms'+#9+
             'feedback_onsetTime_ms'+#9+
             'blank_onsetTime_ms' +#9+
             'user_paused_the_trial'
             //'triggerStation_RT_ms'//+#9+
            // 'photodiode_threshold'

             );
  //writeln(f);
  closefile(f);



  //set up pause trials - load the list from input data files and populate with message text taken from configuration file

  setlength( ptd.pauseTrialsData,MAX_PAUSE_TRIALS);
  setUpPauseTrials(InputDataFileName, ConfigDataFileName, ptd);




  // Start --------------------------------------------------------------
   N_trials_before_pause := N_trials_before_pause_training;

   timer1.start;
  //end;


  // trial loop
  trialNo:=0;

  while trialNo<Ntrials do
  begin

    if (isTriggerstation) then
    begin
      triggerStation.WriteSharedRam(1,0); // set triggerstation slot 1 to zero: required to initialise triggerstation timer;
    end;

    trialDone := false;
    hasResponded := false;
    isRuinedTrial := 0;

    readTrialOrderData(InputDataFileName, sessionNo, trialNo,s1_marker,s2_marker,s3_marker,s4_marker, {TMS_marker,}
      s1_shape, s1_quad,s1_duration, s1_s2_isi,
      s2_shape_position_1, s2_shape_position_2, s2_shape_position_3, s2_shape_position_4, s2_shape_position_5, s2_duration, s2_s3_isi,
      s3_shape, s3_distractor_shape, s3_quad, s3_duration, s3_s4_isi,
      s4_shape, s4_distractor_shape, s4_quad, s4_duration, Response_Time_after_S4, Feedback_shape, Feedback_duration_after_response_time,ITI_after_feedback,
      s1_colour, s2_colour_position_1, s2_colour_position_2, s2_colour_position_3, s2_colour_position_4, s2_colour_position_5, s3_colour, s3_distractor_colour, s4_colour, s4_distractor_colour, keyMapping, taskType, TMS_s3_SOA,Experimental_Condition);


    targetRadiusCM := tan(targetRadiusDeg*(pi/180))*ef.distance;
    cueRadiusCM := tan(targetRadiusDeg*(pi/180))*ef.distance;




    //determine which stimuli (s1,s2,s3 to be shown before s4)
    case (s1_shape) of
    12:
      begin
      show_s1:=false;
      show_s2:=true;
      show_s3:=true;
      end;

    13:
      begin
      show_s1:=false;
      show_s2:=false;
      show_s3:=true;
      end;

    14:
      begin
      show_s1:=false;
      show_s2:=false;
      show_s3:=false;
      end;
    end;


     // Determine the frame numbers of s3 . The TMS onset frame is specified relative to this
     // If s3 is not shown then there is no TMS photodiode patch.
     s3_onset_frameNo:=0;
     if (show_s1) then s3_onset_frameNo:= s3_onset_frameNo + round (s1_duration/1000 / (1/REFRESH_RATE)) + round(s1_s2_isi/1000 / (1/REFRESH_RATE))  ;
     if (show_s2) then s3_onset_frameNo:= s3_onset_frameNo + round (s2_duration/1000 / (1/REFRESH_RATE)) + round(s2_s3_isi/1000 / (1/REFRESH_RATE))  ;

     TMS_frameNo := s3_onset_frameNo + round(TMS_s3_SOA/1000 / (1/REFRESH_RATE));

     if(show_s3=false) then TMS_frameNo:=-1;




    // if none of the stimuli s2-s4 are being shown (shape=11) then this is a baseline condition, and we want no audio 'feedback'
    if ((s2_shape_position_1=11) and(s3_shape=11) and (s4_shape=11)) then
    begin
      isBaselineCondition:=true;
    end
    else
    begin
      isBaselineCondition:=false;
    end;



    //----------------------------------------------------------------------------
    // A mouse button down event triggers the first trial (only).
    // wait for user to begin
    // Pause every N_trials_before_pause trials
    // The first pause occurs after N_trials_before_pause_training trials
    // Subsequent pauses occur after each N_trials_before_pause_main trials





   { // pause on trial 0, after traiing and every N_trials_before_pause_main trials thereafter
    if (   ((trialNo = 0 ) or (trialNo = N_trials_before_pause_training))  or
      ( (trialNo>N_trials_before_pause_training) and   (((trialNo-N_trials_before_pause_training) mod N_trials_before_pause_main) = 0) )     )
     then}


     //check if the current trial is a pause trial.NB pause trials start at 1, but the trialNo counter starts at 0.
     isPauseTrial:=false;
     c:=0;
     while ((isPauseTrial = false) and (c<ptd.NpauseTrials)) do
     begin
         if (trialNo = ptd.pauseTrialsData[c].trialNo-1 ) then
         begin
           isPauseTrial:=true;
         end
         else
         begin
           c:=c+1;
         end;
     end;


     if isPauseTrial then
     begin
     // showmessage('trialNo ' + inttostr(trialNo)  + ' is a pause trial: '+ inttostr(ptd.pauseTrialsData[c].trialNo) + '  '  + ptd.pauseTrialsData[c].message);
      // wait for user input
      state:=-1;


      // all trials except the first one
      if (trialNo<>0) then
      begin
        // check button box and keyboard
        repeat
          state:=-1;
          checkKeypressAndButtonbox(state, eventTime) ;

          ef.ProjectionTrans;
          gldisable(gl_lighting);
          glMatrixMode(GL_MODELVIEW);
          glLoadIdentity;
          drawBackgroundFixation(fixSpotSizeCM,Pause_background_circle_colour, Fixation_and_placeholders_colour);
          drawFixationWithPlaceholders(fixSpotSizeCM,targetRadiusCM, Fixation_and_placeholders_colour);

          glpushmatrix();
          glviewport(0,0,ef.width,ef.height);
          glMatrixMode(GL_PROJECTION);
          glLoadIdentity;
          glMatrixMode(GL_MODELVIEW);
          glLoadIdentity;
          glcolor3f(1,1,1);
          glortho(-ef.width/2,ef.width/2,-ef.height/2,ef.height/2,-1,1);
          renderTextWithBackgroundColCentred(pfontGeneral,  ptd.pauseTrialsData[c].message, fontCol, fontBgrColBlack, 0,0,1);
          glpopmatrix();



          if showTrialsRemaining then showCountdown(pfontGeneral,fontCol,inttostr(Ntrials-trialNo));
          ef.renderStereo;

        until ( (state <>-1));
      end
      else
      begin // on the first trial, wait for a mouse button press to advance.
        repeat
          state:=-1;
          //pollevent(state, eventTime) ;
          checkKeypressAndButtonbox(state, eventTime) ;

          ef.ProjectionTrans;
          drawBackgroundFixation(fixSpotSizeCM,Pause_background_circle_colour, Fixation_and_placeholders_colour);
          drawFixationWithPlaceholders(fixSpotSizeCM,targetRadiusCM, Fixation_and_placeholders_colour);

          glpushmatrix();
          glviewport(0,0,ef.width,ef.height);
          glMatrixMode(GL_PROJECTION);
          glLoadIdentity;
          glMatrixMode(GL_MODELVIEW);
          glLoadIdentity;
          glcolor3f(1,1,1);
          glortho(-ef.width/2,ef.width/2,-ef.height/2,ef.height/2,-1,1);
          renderTextWithBackgroundColCentred(pfontGeneral,  ptd.pauseTrialsData[c].message, fontCol, fontBgrColBlack, 0,0,1);
          glpopmatrix();

          if showTrialsRemaining then showCountdown(pfontGeneral,fontCol,inttostr(Ntrials-trialNo));
          ef.renderStereo;
        until ((state= SDL_BUTTON_LEFT) or (state = SDL_BUTTON_RIGHT));

        if (isVeryFirstTrial = true) then
        begin
          timer2.start; // Start main timer immediately after mouse button is pressed
          timeOfExperimentStart := SDL_GetTicks;
          isVeryFirstTrial:=false;
        end;
      end;


      // 2 sec pause
      Nframes:=round (2 / (1/REFRESH_RATE))  ;
      for c:=0 to Nframes-1 do
      begin
         ef.ProjectionTrans;
         drawBackgroundFixation(fixSpotSizeCM,Run_background_circle_colour, Fixation_and_placeholders_colour);
         drawFixationWithPlaceholders(fixSpotSizeCM,targetRadiusCM, Fixation_and_placeholders_colour);
         handledSuspended(isRuinedTrial);
         pollevent(state, eventTime) ;
        ef.renderStereo;

      end;


    end;
    //=======================================================================================
    //=======================================================================================
    //=======================================================================================







    //=======================================================================================
    //=======================================================================================
    //=======================================================================================
    //     S1
    //=======================================================================================
    // skip S1 if show_s1=false
    frameNoTotal:=0;
    s1_onsetTime:=-1;
    if (show_s1) then
    begin
      Nframes:=round (s1_duration/1000 / (1/REFRESH_RATE))  ;
      triggerState:= true; // re-latch the trigger for one-shot parallel port data

      if (isTriggerstation) then
      begin
        triggerStationData:= s1_marker;
        TriggerStation.ParallelPort(triggerStationData);   // load the trigger station with trigger data
      end;

      doPhotodiode:=true;

      for frameNo:=0 to Nframes-1 do
      begin
        ef.ProjectionTrans;

        drawBackgroundFixation(fixSpotSizeCM,Run_background_circle_colour, Fixation_and_placeholders_colour);

        cueCurrentRadiusCM:=cueRadiusCM;

        // s1 onset photodiode patch: left. trigger station will send s3_marker on detecting the patch
        if (doPhotodiode=true) then
        begin
          if (isTriggerstation) then glCallList(DL_PHOTODIODE_PATCH_LEFT);
          doPhotodiode:=false;
        end;


        if (s1_quad=6) then
        begin  //cue in screen centre
          glMatrixMode(GL_MODELVIEW);
          glLoadIdentity;
          gltranslatef(0,0,0);
          DrawShapeOrChar(s1_shape,colours[s1_colour], Run_background_circle_colour,x,y);
        end
        else
        begin
          if (s1_quad<>5) then
          begin
            glMatrixMode(GL_MODELVIEW);
            glLoadIdentity;
            objectLocation(x,y,cueCurrentRadiusCM,s1_quad);
            gltranslatef(x,y,0);
            DrawShapeOrChar(s1_shape,colours[s1_colour], Run_background_circle_colour,x,y)
          end
          else
          begin
            glMatrixMode(GL_MODELVIEW);
            glLoadIdentity;
            objectLocation(x,y,cueCurrentRadiusCM,1);
            gltranslatef(x,y,0);
            DrawShapeOrChar(s1_shape,colours[s1_colour], Run_background_circle_colour,x,y);

            glMatrixMode(GL_MODELVIEW);
            glLoadIdentity;
            objectLocation(x,y,cueCurrentRadiusCM,2);
            gltranslatef(x,y,0);
            DrawShapeOrChar(s1_shape,colours[s1_colour], Run_background_circle_colour,x,y);

            glMatrixMode(GL_MODELVIEW);
            glLoadIdentity;
            objectLocation(x,y,cueCurrentRadiusCM,3);
            gltranslatef(x,y,0);
            DrawShapeOrChar(s1_shape,colours[s1_colour], Run_background_circle_colour,x,y);

            glMatrixMode(GL_MODELVIEW);
            glLoadIdentity;
            objectLocation(x,y,cueCurrentRadiusCM,4);
            gltranslatef(x,y,0);
            DrawShapeOrChar(s1_shape,colours[s1_colour], Run_background_circle_colour,x,y);
            end;
        end;

        glclear(GL_DEPTH_BUFFER_BIT);
        drawFixationWithPlaceholders(fixSpotSizeCM, targetRadiusCM, Fixation_and_placeholders_colour);

        pollevent(state, eventTime) ;
        if showTrialsRemaining then showCountdown(pfontGeneral,fontCol,inttostr(Ntrials-trialNo));

        if ((frameNoTotal = TMS_frameNo) and (isTriggerStation)) then
        begin
          glCallList(DL_PHOTODIODE_PATCH_RIGHT);
        end;

        handledSuspended(isRuinedTrial); // suspend rendering the stimulus if IS_SUSPENDED

        ef.renderStereo;
        //writeln(fTest,  inttostr(frameNo) + chr(9) + inttostr(SDL_GetTicks - timeOfExperimentStart) );

        // get the time after the first image is displayed
        if (frameNo=0) then
        begin
         // s1_onsetTime:=timer2.query;
          s1_onsetTime := SDL_GetTicks - timeOfExperimentStart;
        end;

        // get time of TMS onset
        if (frameNoTotal = TMS_frameNo) then
        begin
          TMS_onsetTime := SDL_GetTicks - timeOfExperimentStart;
        end;



        {//send parallel port data (value cueQuadrant) immediately after  first trajectory frame is drawn
        if triggerState then
        begin
          //***************************************************************
          Out32($378, s1_quad);
          //Out32($378, 255);
          TimerPulse.start;
          repeat until (TimerPulse.query>=parallelPortPulseDur);

          TimerPulse.start;
          Out32($378, 0);
          repeat until (TimerPulse.query>=parallelPortPulseDur);

          //***************************************************************
          triggerState:=false;
        end;
        }

        frameNoTotal := frameNoTotal+1;
      end;

      //closefile(fTest);
      //=======================================================================================
      //=======================================================================================
      //=======================================================================================





      //=======================================================================================
      //=======================================================================================
      //=======================================================================================
      //  S1-S2 ISI   -------------------------------------------
      //=======================================================================================
      Nframes:=round(s1_s2_isi/1000 / (1/REFRESH_RATE))  ;
      timer1.start;
      triggerState:= true; // re-latch the trigger for one-shot parallel port data
      for frameNo:=0 to Nframes-1 do
      begin
        ef.ProjectionTrans;
        drawBackgroundFixationWithPlaceholders(fixSpotSizeCM, targetRadiusCM, Run_background_circle_colour, Fixation_and_placeholders_colour);

        pollevent(state, eventTime) ;
        if showTrialsRemaining then showCountdown(pfontGeneral,fontCol,inttostr(Ntrials-trialNo));

        if ((frameNoTotal = TMS_frameNo) and (isTriggerStation)) then
        begin
          glCallList(DL_PHOTODIODE_PATCH_RIGHT);
        end;


        // get time of TMS onset
        if (frameNoTotal = TMS_frameNo) then
        begin
          TMS_onsetTime := SDL_GetTicks - timeOfExperimentStart;
        end;

         handledSuspended(isRuinedTrial); // suspend rendering the stimulus if IS_SUSPENDED

        ef.renderStereo;
        //send parallel port data immediately after 'blank screen' is first drawn
        //if triggerState then
        //begin
        //  //showmessage('Parallel port data 1');
        //  //***************************************************************
        //  Out32($378, s1_quad*10);
        //  TimerPulse.start;
        //  repeat until (TimerPulse.query>=parallelPortPulseDur);
        //
        //  TimerPulse.start;
        //  Out32($378, 0);
        //  repeat until (TimerPulse.query>=parallelPortPulseDur);
        //
        //  //***************************************************************
        //  triggerState:=false;
        //end;
        frameNoTotal := frameNoTotal+1;
      end;
  end;
  //=======================================================================================
  //=======================================================================================
  //=======================================================================================







  //=======================================================================================
  //=======================================================================================
  //=======================================================================================
  //     S2 informative cue ---------------------------------------------------
  //=======================================================================================
  // skip S2 if show_s2=false
  s2_onsetTime:=-1;
  if (show_s2) then
  begin

    Nframes:=round (s2_duration/1000 / (1/REFRESH_RATE))  ;

    triggerState:= true; // re-latch the trigger for one-shot parallel port data

    if (isTriggerstation) then
    begin
      triggerStationData:= s2_marker;
      TriggerStation.ParallelPort(triggerStationData);   // load the trigger station with trigger data
    end;

    doPhotodiode:=true;

    timer1.start;

    for frameNo:=0 to Nframes-1 do
    begin
      ef.ProjectionTrans;
      glMatrixMode(GL_MODELVIEW);
      glLoadIdentity;
     // drawFixationSpot(fixSpotSizeCM,Fixation_and_placeholders_colour);
     // glcolor3f(Run_background_circle_colour.r,Run_background_circle_colour.g,Run_background_circle_colour.b);
      //glCallList(DL_CIRCLE_BACKGROUND);
      drawBackgroundFixation(fixSpotSizeCM,Run_background_circle_colour, Fixation_and_placeholders_colour);

      glMatrixMode(GL_MODELVIEW);
      gltranslatef(0,0,+0.00015);
      cueCurrentRadiusCM:=cueRadiusCM;
      objectLocation(x,y,cueCurrentRadiusCM,5);
      gltranslatef(x,y,0);

      // s2 onset photodiode patch: left. trigger station will send s3_marker on detecting the patch
      if (doPhotodiode=true) then
      begin
        if (isTriggerstation) then   glCallList(DL_PHOTODIODE_PATCH_LEFT);
        doPhotodiode:=false;
      end;

      drawS2cues(s2_shape_position_1, s2_shape_position_2, s2_shape_position_3, s2_shape_position_4, s2_shape_position_5,
        s2_colour_position_1, s2_colour_position_2, s2_colour_position_3, s2_colour_position_4, s2_colour_position_5);


      glClear(GL_DEPTH_BUFFER_BIT);

      drawFixationWithPlaceholders(fixSpotSizeCM, targetRadiusCM, Fixation_and_placeholders_colour);

      pollevent(state, eventTime) ;
      if showTrialsRemaining then showCountdown(pfontGeneral,fontCol,inttostr(Ntrials-trialNo));

      if ((frameNoTotal = TMS_frameNo) and (isTriggerStation)) then
      begin
        glCallList(DL_PHOTODIODE_PATCH_RIGHT);
      end;

       handledSuspended(isRuinedTrial); // suspend rendering the stimulus if IS_SUSPENDED

      ef.renderStereo;

      // get the time after the first image is displayed
      if (frameNo=0) then
      begin
        s2_onsetTime := SDL_GetTicks - timeOfExperimentStart;
      end;

      // get time of TMS onset
      if (frameNoTotal = TMS_frameNo) then
      begin
        TMS_onsetTime := SDL_GetTicks - timeOfExperimentStart;
      end;


    //   showmessage(  inttostr(frameNo));
      {//send parallel port data (value 99) immediately after  first trajectory frame is drawn
      if triggerState then
      begin
      //***************************************************************
      timestampRecord[trialNo]:=timer2.query;// timestamp
       Out32($378, s1_quad+10);
       //Out32($378, 255);
        TimerPulse.start;
        repeat until (TimerPulse.query>=parallelPortPulseDur);
       Out32($378, 0);
        //***************************************************************
        triggerState:=false;
      end;
      }
    //until timer1.query>= 0.5-0.01;//trajectoryDuration-0.01;
      frameNoTotal := frameNoTotal+1;
    end;

        //Out32($378, s1_quad+10);
      //  Out32($378, 255);
     //   TimerPulse.start;
     //   repeat until (TimerPulse.query>=parallelPortPulseDur);
     //   Out32($378, 0);
    //=======================================================================================
    //=======================================================================================
    //=======================================================================================





    //=======================================================================================
    //=======================================================================================
    //=======================================================================================
    // S2 - S3 ISI Blank screen---------------------------------------------
    //=======================================================================================
   Nframes:=round (s2_s3_isi/1000 / (1/REFRESH_RATE))  ;
    timer1.start;

    for frameNo:=0 to Nframes-1 do
    begin
      ef.ProjectionTrans;
      drawBackgroundFixationWithPlaceholders(fixSpotSizeCM, targetRadiusCM, Run_background_circle_colour, Fixation_and_placeholders_colour);
      pollevent(state, eventTime) ;
      if showTrialsRemaining then showCountdown(pfontGeneral,fontCol,inttostr(Ntrials-trialNo));

      if ((frameNoTotal = TMS_frameNo) and (isTriggerStation)) then
      begin
        glCallList(DL_PHOTODIODE_PATCH_RIGHT);
      end;

       handledSuspended(isRuinedTrial); // suspend rendering the stimulus if IS_SUSPENDED

      ef.renderStereo;

      // get time of TMS onset
      if (frameNoTotal = TMS_frameNo) then
      begin
        TMS_onsetTime := SDL_GetTicks - timeOfExperimentStart;
      end;

      frameNoTotal := frameNoTotal+1;
    end;
  end;
  //=======================================================================================
  //=======================================================================================
  //=======================================================================================







  //=======================================================================================
  //=======================================================================================
  //=======================================================================================
  //     S3 spatial cue onset with optional TMS photodiode patch
  //=======================================================================================
  // skip S3 if show_s3=false

  s3_onsetTime:=-1;
  if (show_s3) then
  begin

    Nframes:=round(s3_duration/1000 / (1/REFRESH_RATE))  ;
    triggerState:= true; // re-latch the trigger for one-shot parallel port data

    if (isTriggerstation) then
    begin
      triggerStationData:= s3_marker;
      TriggerStation.ParallelPort(triggerStationData);   // load the trigger station with trigger data
    end;

    doPhotodiode:=true;
    timer1.start;
    for frameNo:=0 to Nframes-1 do
    begin
      ef.ProjectionTrans;

      drawBackgroundFixation(fixSpotSizeCM, Run_background_circle_colour, Fixation_and_placeholders_colour);

      if (s3_shape<>12) then targetImage(targetRadiusCM, s3_shape, s3_quad,s3_distractor_shape, s3_colour, s3_distractor_colour
        , Show_S3_peripheral_placeholders, Show_S3_placeholder_when_centre);

      glClear(GL_DEPTH_BUFFER_BIT);
      drawFixationWithPlaceholders(fixSpotSizeCM, targetRadiusCM, Fixation_and_placeholders_colour, s3_quad <> 5);


      // s3 onset photodiode patch: left. trigger station will send s3_marker on detecting the patch
      if (doPhotodiode=true) then
      begin
        if (isTriggerstation) then  glCallList(DL_PHOTODIODE_PATCH_LEFT);
        if (Show_S3_photodiode_patch = 1) then glCallList(DL_PHOTODIODE_PATCH_CENTRE); //photodiode patch at location specified for S3
      end;


      pollevent(state, eventTime) ;
      if showTrialsRemaining then showCountdown(pfontGeneral,fontCol,inttostr(Ntrials-trialNo));

      if ((frameNoTotal = TMS_frameNo) and (isTriggerStation)) then
      begin
        glCallList(DL_PHOTODIODE_PATCH_RIGHT);
      end;

       handledSuspended(isRuinedTrial); // suspend rendering the stimulus if IS_SUSPENDED

      ef.renderStereo;

      // get time of TMS onset
      if (frameNoTotal = TMS_frameNo) then
      begin
        TMS_onsetTime := SDL_GetTicks - timeOfExperimentStart;
      end;

      doPhotodiode:=false;
      // get the time after the first image is displayed
      if (frameNo=0) then
      begin
        //s3_onsetTime:=timer2.query;
        s3_onsetTime := SDL_GetTicks - timeOfExperimentStart;
      end;



      {//load the trigger station with TMS_marker once the first frame of s3 has been presented
      if (frameNo>0) then
      begin
        triggerStationData:= TMS_marker;
        TriggerStation.ParallelPort(triggerStationData);   // load the trigger station with trigger data
      end;}

      //send parallel port data immediately after  first trajectory frame is drawn
      if triggerState then
      begin
        //***************************************************************
        //timestampRecord[trialNo]:=timer2.query;// timestamp
        timestampRecord[trialNo]:= SDL_GetTicks - timeOfExperimentStart; //timestamp
        {
        Out32($378, s3_marker);
        TimerPulse.start;
        repeat until (TimerPulse.query>=parallelPortPulseDur);
       // TriggerStation.WriteSharedRAM(10,0); // no longer needed as Giorgio has a latch cable
        Out32($378, 0);
        }
        //***************************************************************
        triggerState:=false;
      end;

      frameNoTotal := frameNoTotal+1;
    end;
    //showmessage(floattostr(frameNo));
    //=======================================================================================
    //=======================================================================================
    //=======================================================================================





    //=======================================================================================
    //=======================================================================================
    //=======================================================================================
    // S3 - S4 ISI Blank screen---------------------------------------------
    //=======================================================================================
    Nframes:=round   (s3_s4_isi/1000 / (1/REFRESH_RATE))  ;
    timer1.start;
    for frameNo:=0 to Nframes-1 do
    begin
      ef.ProjectionTrans;
      drawBackgroundFixationWithPlaceholders(fixSpotSizeCM, targetRadiusCM, Run_background_circle_colour, Fixation_and_placeholders_colour);

      pollevent(state, eventTime) ;
      if showTrialsRemaining then showCountdown(pfontGeneral,fontCol,inttostr(Ntrials-trialNo));

      if ((frameNoTotal = TMS_frameNo) and (isTriggerStation)) then
      begin
        glCallList(DL_PHOTODIODE_PATCH_RIGHT);
      end;

       handledSuspended(isRuinedTrial); // suspend rendering the stimulus if IS_SUSPENDED

      ef.renderStereo;

      // get time of TMS onset
      if (frameNoTotal = TMS_frameNo) then
      begin
        TMS_onsetTime := SDL_GetTicks - timeOfExperimentStart;
      end;

      frameNoTotal := frameNoTotal+1;
    end;
  end;
  //=======================================================================================
  //=======================================================================================
  //=======================================================================================




  //=======================================================================================
  //=======================================================================================
  //=======================================================================================
  //     S4  target onset
  //=======================================================================================
  Nframes:=round  (s4_duration/1000 / (1/REFRESH_RATE))  ;
  triggerState:= true; // re-latch the trigger for one-shot parallel port data

  if (isTriggerstation) then
  begin
    triggerStationData:=s4_marker;
    TriggerStation.ParallelPort(triggerStationData);   // load the trigger station with trigger data
    triggerStation.WriteSharedRam(1,0); // set triggerstation slot 1 to zero: required to initialise triggerstation timer;
  end;

  doPhotodiode:=true;
  timer1.start;

  observedDataResponseRecord[trialNo] := -1;
  observedDataCorrectResponseRecord[trialNo] := -1;
  observedDataRTRecord[trialNo] :=-1;
  response_onsetTime := -1;

  hasResponded:=false;
  doCheckForResponse:=true;

  responseState:=-1;
  responseEventTime:=0;

  for frameNo:=0 to Nframes-1 do
  begin
    ef.ProjectionTrans;
    drawBackgroundFixation(fixSpotSizeCM, Run_background_circle_colour, Fixation_and_placeholders_colour);

    if (s4_shape<>12) then targetImage(targetRadiusCM, s4_shape, s4_quad,s4_distractor_shape, s4_colour, s4_distractor_colour
      , Show_S4_peripheral_placeholders, Show_S4_placeholder_when_centre);

    // photodiode patch left trigger station will send s4_marker on detecting the patch
    if (doPhotodiode=true) then
    begin
      if (isTriggerstation) then  triggerStation.WriteSharedRam(1,2000); // set triggerstation to start timer on photodiode patch detection
      if (isTriggerstation) then    glCallList(DL_PHOTODIODE_PATCH_LEFT); //photodiode patch left side
      if (Show_S4_photodiode_patch = 1) then glCallList(DL_PHOTODIODE_PATCH_CENTRE); //photodiode patch at location specified for S4.

      doPhotodiode:=false;
    //  showmessage('s4 on triggerStationData = '+ inttostr(triggerStationData));
    end;

    if showTrialsRemaining then showCountdown(pfontGeneral,fontCol,inttostr(Ntrials-trialNo));

    glClear(GL_DEPTH_BUFFER_BIT);
    drawFixationWithPlaceholders(fixSpotSizeCM, targetRadiusCM, Fixation_and_placeholders_colour, s4_quad <> 5);

    // TMS photodiode patch
    if ((frameNoTotal = TMS_frameNo) and (isTriggerStation)) then
    begin
      glCallList(DL_PHOTODIODE_PATCH_RIGHT);
    end;

    handledSuspended(isRuinedTrial); // suspend rendering the stimulus if IS_SUSPENDED

    ef.renderStereo;

    // get time of TMS onset
    if (frameNoTotal = TMS_frameNo) then
    begin
      TMS_onsetTime := SDL_GetTicks - timeOfExperimentStart;
    end;

    // get the time after the first image is displayed
    if (frameNo=0) then
    begin
      //s4_onsetTime:=timer2.query;
      t1 := SDL_GetTicks;
      s4_onsetTime := t1 - timeOfExperimentStart;
    end;

    checkKeypressAndButtonbox(state, eventTime) ;

    renderTextWithBackgroundColCentred(pfontGeneral,  inttostr(state), fontCol, fontBgrColBlack, 0,0,1);
    if ((hasResponded=false) and (state<>-1)) then
    begin
      //showmessage('has responded');
      hasResponded:=true;
      responseState:=state;
      responseEventTime:=eventTime;
     // triggerStationRT := triggerStation.ReadSharedRam(2); // read trigger station timer from slot 2. RT


     if (isTriggerstation) then  triggerStation.WriteSharedRam(1,0); // set triggerstation slot 1 to zero: required to initialise triggerstation timer;

      //showmessage('reading trigger station during stimulus');
    end;
    frameNoTotal := frameNoTotal+1;
  end;
  //=======================================================================================
  //=======================================================================================
  //=======================================================================================





  //=======================================================================================
  //=======================================================================================
  //=======================================================================================
  //'blank' screen duration-----------------------------------------------------
  isAuditoryFeedback:=true;
  triggerState:=true;  // re-latch the trigger for one-shot parallel port data
  hasBlankedTheTarget:= false; //set flag  to false


  doPhotodiode:=false;
  Nframes:=round  ((Response_Time_after_S4 + Feedback_duration_after_response_time + ITI_after_feedback)/1000  / (1/REFRESH_RATE))  ;  // max response time from the start of the S4 blank period
  frameNo:=0;
  //state:=-1;
  repeat;
    ef.projectionTrans;
    drawBackgroundFixationWithPlaceholders(fixSpotSizeCM, targetRadiusCM, Run_background_circle_colour, Fixation_and_placeholders_colour);

    //do photodiode so send response data via triggerstation
    if doPhotodiode then
    begin
       glMatrixMode(GL_MODELVIEW);
       glLoadIdentity;
       gltranslatef((-ef.WidthCM/2) + 2,(ef.HeightCM/2) - 1,0);
       glcolor3f(1,1,1);

       if (isTriggerstation) then   glCallList(DL_PHOTODIODE_PATCH_LEFT); // bar(0,0,-ef.distance+0.01, photodiodePatchSizeCM,photodiodePatchSizeCM, 0);

       doPhotodiode:=false;
    //   showmessage('blank triggerStationData = '+ inttostr(triggerStationData));
    end;
    if showTrialsRemaining then showCountdown(pfontGeneral,fontCol,inttostr(Ntrials-trialNo));

    if ((frameNoTotal = TMS_frameNo) and (isTriggerStation)) then
    begin
      glCallList(DL_PHOTODIODE_PATCH_RIGHT);
    end;

     handledSuspended(isRuinedTrial); // suspend rendering the stimulus if IS_SUSPENDED

    ef.renderStereo;

    // get time of TMS onset
    if (frameNoTotal = TMS_frameNo) then
    begin
      TMS_onsetTime := SDL_GetTicks - timeOfExperimentStart;
    end;

    {TimerPulse.start;
    repeat until (TimerPulse.query>=parallelPortPulseDur);
    }
    // TriggerStation.WriteSharedRAM(10,0); // no longer needed as Giorgio has a latch cable
    // pollevent(state);

    if (hasBlankedTheTarget=false) then
    begin
      //timeOfFirstBlankImageOnset:=timer2.query;
      hasBlankedTheTarget:= true;
    end;
    frameNo:=frameNo+1;
    if(frameNo=Nframes) then trialDone:=true; // trial is done only once the required number of frames has been displayed
     // end;



    if (doCheckForResponse=true) then
    begin
      if (not hasResponded) then begin
        checkKeypressAndButtonbox(state, eventTime);

        if (state<>-1) then
        begin
          //showmessage('has responded');
          hasResponded:=true;
          responseState:=state;
          responseEventTime:=eventTime;

          //##################################################
          // read trigger station timer from slot 2. RT
         // triggerStationRT := triggerStation.ReadSharedRam(2);

          if (isTriggerstation) then  triggerStation.WriteSharedRam(1,0); // set triggerstation slot 1 to zero: required to initialise triggerstation timer;

          //showmessage('reading trigger station during blank');
          //##################################################

        end;
      end;

      if ((triggerstate=true) and (responseState<>-1)) then  //if there has been a response
      begin
        doCheckForResponse:=false;
        //  showmessage('responded');
        //observedDataRTRecord[trialNo]:=timer1.query; // record Reaction Time
        observedDataRTRecord[trialNo]:= responseEventTime - t1; // record Reaction Time  between response event and time t1 taken immediately after s4 render command is sent;

        //response_onsetTime:=timer2.query; // record response time relative to experiment start
        response_onsetTime := responseEventTime - timeOfExperimentStart;  // record response time relative to experiment start
        // Mix_PlayChannel(Ord(CORRECT_WAV),  sounds[Ord(CORRECT_WAV)], 0);
        //  showmessage('responseEventTime : ' + inttostr(responseEventTime));
       // showmessage('timeOfExperimentStart = ' + inttostr(timeOfExperimentStart) + '. RT = ' + inttostr(observedDataRTRecord[trialNo]) +  'diff = ' + inttostr(s4_onsetTime - t1));

       // showmessage(inttostr(state));
        hasResponded := true;

        // showmessage(inttostr(state));
        if keyMapping=0 then
        begin  
          if (responseState=RESPONSE_LEFT_BUTTON) then observedDataResponseRecord[trialNo] := 0; //'different' (253 on DAS6014)
          if (responseState=RESPONSE_RIGHT_BUTTON) then observedDataResponseRecord[trialNo] := 1; //'same'   (254 on DAS6014)
        end
        else
        begin
          if (responseState=RESPONSE_LEFT_BUTTON) then observedDataResponseRecord[trialNo] := 1; //'same' (253 on DAS6014)
          if (responseState=RESPONSE_RIGHT_BUTTON) then observedDataResponseRecord[trialNo] := 0; //'different'  (254 on DAS6014)
        end;

        // determine whether cue and target match on the task dimension in same/different tasks (taskType 1 and 2)
        case (taskType) of
          1: //shape matching task
            if  ((s2_shape_position_1 = s4_shape) or
            (s2_shape_position_2 = s4_shape) or
            (s2_shape_position_3 = s4_shape) or
            (s2_shape_position_4 = s4_shape) or
            (s2_shape_position_5 = s4_shape)  )
            then isTargetMatchesCue:=true else isTargetMatchesCue:=false; //shape task


          2: // colour matching task
            if  (((s2_shape_position_1 <> 0) and (s2_colour_position_1= s4_colour)) or
            ((s2_shape_position_2 <> 0) and (s2_colour_position_2= s4_colour)) or
            ((s2_shape_position_3 <> 0) and (s2_colour_position_3= s4_colour)) or
            ((s2_shape_position_4 <> 0) and (s2_colour_position_4= s4_colour)) or
            ((s2_shape_position_5 <> 0) and (s2_colour_position_5= s4_colour)))
            then isTargetMatchesCue:=true else isTargetMatchesCue:=false; //colour task

        end;

      { if  (isTargetMatchesCue=true) then
        begin
          showmessage('target matches cue');
        end
        else
        begin
        showmessage ('target does not match cue');
        end;}

        // check for correct/incorrect response.
        if (taskType<>3) then // same/different tasks (taskType 1 and 2)
        begin
          if ((observedDataResponseRecord[trialNo]=1) and  (isTargetMatchesCue=true)) then
          begin
            //showmessage('good');
            observedDataCorrectResponseRecord[trialNo]:=1;
          end;
          if ((observedDataResponseRecord[trialNo]=0) and  (isTargetMatchesCue=true)) then
          begin
            //showmessage('bad');
            observedDataCorrectResponseRecord[trialNo]:=0;
          end;
          if ((observedDataResponseRecord[trialNo]=0) and  (isTargetMatchesCue=false)) then
          begin
            //showmessage('good');
            observedDataCorrectResponseRecord[trialNo]:=1;
          end;
          if ((observedDataResponseRecord[trialNo]=1) and  (isTargetMatchesCue=false)) then
          begin
               // showmessage('bad');
            observedDataCorrectResponseRecord[trialNo]:=0;
          end;
        end
        else  // Task Type 3 identification task
        begin
          if (observedDataResponseRecord[trialNo]=1) then
          begin
            //showmessage('ident good');
           observedDataCorrectResponseRecord[trialNo]:=1;
          end;
          if (observedDataResponseRecord[trialNo]=0) then
          begin
            //showmessage('ident bad');
           observedDataCorrectResponseRecord[trialNo]:=0;
          end;
        end;
        //showmessage('response ' + inttostr(observedDataResponseRecord[trialNo]));

        // send trigger to indicate correct vs incorrect response
        if (observedDataCorrectResponseRecord[trialNo]=1) then
        begin
          //***************************************************************
          // send signal to indicate observer's response  : 254 = correct
          //doPhotodiode:=true; //COMMENTED OUT 14/3/2018 FOR GIORGIO EEG SAMPLING TEST
          if (isTriggerstation) then
          begin
            triggerStationData:=254;
            TriggerStation.ParallelPort(triggerStationData);
          end;
          {TimerPulse.start;
          Out32($378, 101);
          repeat until (TimerPulse.query>=parallelPortPulseDur);
          Out32($378, 0);
          }
         // showmessage('trig 101 correct');
          //***************************************************************
        end
        else
        begin
          //***************************************************************
          // send signal to indicate observer's response  : 255 = incorrect
          //doPhotodiode:=true; //COMMENTED OUT 14/3/2018 FOR GIORGIO EEG SAMPLING TEST
          if (isTriggerstation) then
          begin
            triggerStationData:=255;
            TriggerStation.ParallelPort(triggerStationData);
          end;
          {TimerPulse.start;
          Out32($378, 100);
          repeat until (TimerPulse.query>=parallelPortPulseDur);
          Out32($378, 0);
          }
         // showmessage('trig 100 incorrect');
          //***************************************************************
        end;

        // send parallel port trigger
        {TimerPulse.start;
        Out32($378, 0);
        repeat until (TimerPulse.query>=0.005);
        }
        triggerstate:=false;
      end; //state check
    end;   //any keypress check








    // At Response_Time_after_S4 sec, stop checking for responses and give auditory feedback
    // auditory feedback at 2 sec
    if (  (frameNo*(1/REFRESH_RATE)>=Response_Time_after_S4/1000)  and (isAuditoryFeedback=true)      )  then
    begin
      doCheckForResponse:=false; // stop checking for responses
      if observedDataCorrectResponseRecord[trialNo] = 1  then
      begin
        if (isBaselineCondition=false) then
        begin
           if (Feedback_shape=0) then Mix_PlayChannel(Ord(CORRECT_WAV),  sounds[Ord(CORRECT_WAV)], 0);
        end;

        feedback_onsetTime := SDL_GetTicks - timeOfExperimentStart;
        isAuditoryFeedback:=false;
        //***************************************************************
        // send signal to indicate observer's response  : 254 = correct
        //doPhotodiode:=true; //COMMENTED OUT 14/3/2018 FOR GIORGIO EEG SAMPLING TEST
        if (isTriggerstation) then
        begin
          triggerStationData:=254;
          TriggerStation.ParallelPort(triggerStationData);
          doPhotodiode := true;
        end;
        {TimerPulse.start;
        Out32($378, 201);
        repeat until (TimerPulse.query>=parallelPortPulseDur);
        Out32($378, 0);
        }
        //showmessage('trig 251 correct');
        //***************************************************************
      end
      else
      begin
        if (isBaselineCondition=false) then
        begin
          if (Feedback_shape=0) then Mix_PlayChannel(Ord(INCORRECT_WAV),  sounds[Ord(INCORRECT_WAV)], 0);
         end;

        feedback_onsetTime := SDL_GetTicks - timeOfExperimentStart;
        isAuditoryFeedback:=false;
        //***************************************************************
        // send signal to indicate observer's response  : 255 = incorrect
        //doPhotodiode:=true; //COMMENTED OUT 14/3/2018 FOR GIORGIO EEG SAMPLING TEST
        if (isTriggerstation) then
        begin
          triggerStationData:=255;
          TriggerStation.ParallelPort(triggerStationData);
          doPhotodiode := true;
        end;
        {TimerPulse.start;
        Out32($378, 200);
        repeat until (TimerPulse.query>=parallelPortPulseDur);
        Out32($378, 0);
        }
        //showmessage('trig 250 incorrect');
        //***************************************************************
      end;
    end ;


    // visual feedback
    if (Feedback_shape<>0) then
    begin

      if (  (frameNo*(1/REFRESH_RATE)>=Response_Time_after_S4/1000)  and ( frameNo*(1/REFRESH_RATE) <= (Response_Time_after_S4+Feedback_duration_after_response_time)/1000 )) {and (isAuditoryFeedback=true)}  then
      begin

        if (isBaselineCondition=false) then
        begin
          if observedDataCorrectResponseRecord[trialNo] = 1  then
          begin
            case (Feedback_shape) of
              1..11:
              begin
                glMatrixMode(GL_MODELVIEW);
                glLoadIdentity;
                gltranslatef(x,y,0);
                glcolor3f(Correct_feedback_colour.r, Correct_feedback_colour.g, Correct_feedback_colour.b);
                drawShape(Feedback_shape);
              end;

              12:
               showTextFeedback(pfontFeedback, font_col_correct,fontBgrCol,Feedback_text_correct);

              13:
              displayBMPimageXYcm(BMPimages,100, x,y, ef.Width, ef.height, ef.WidthCM, ef.heightCM);

            end;

          end
          else
          begin
            case (Feedback_shape) of
              1..11:
              begin
                glMatrixMode(GL_MODELVIEW);
                glLoadIdentity;
                gltranslatef(x,y,0);
                glcolor3f(Incorrect_feedback_colour.r, Incorrect_feedback_colour.g, Incorrect_feedback_colour.b);
                drawShape(Feedback_shape);
              end;
            12:
              showTextFeedback(pfontFeedback, font_col_incorrect,fontBgrCol,Feedback_text_incorrect);

            13:
              displayBMPimageXYcm(BMPimages,101, x,y, ef.Width, ef.height, ef.WidthCM, ef.heightCM);
            end;
          end;
        end;
      end;
      // photodiode patch left trigger station will send s4_marker on detecting the patch

    end;





    if ((hasResponded=false) and (trialDone=true)) then
    begin
       observedDataResponseRecord[trialNo] := -1;
       observedDataCorrectResponseRecord[trialNo] := -1;
       //trialDone:=true;
       //showmessage('not responded');
    end;

    frameNoTotal := frameNoTotal+1;
  until trialDone; // this trial is done

  //blank_onsetTime:=timer2.query; // record end of trial relative to experiment start
  blank_onsetTime := SDL_GetTicks - timeOfExperimentStart;



  // write the data  for the trial----------------------------------------------
  outputDataFilename := Form1.Savedialog1.filename;

  AssignFile(f, outputDataFilename);
  if FileExists(outputDataFilename)  then
  begin
     Append(f);
  end
  else
  begin
     rewrite(f);
  end;


  RT_ms :=  observedDataRTRecord[trialNo];//*1000;

  if (observedDataResponseRecord[trialNo] = -1) then
  begin
    RT_ms_minus_constant_error := -1;
  end
  else
  begin
    RT_ms_minus_constant_error := observedDataRTRecord[trialNo] - RT_constant_error_ms;
  end;

  timeStamp_ms :=  timeStampRecord[trialNo];//*1000);


  writeln(f, experimentName +#9+
             currentDate +#9+
             currentTime +#9+
             inttostr(trialOrderFileNo) +#9+
             participantID +#9+
             age +#9+
             sex +#9+
             handedness +#9+
             displayType +#9+
             inttostr(observerNo) +#9+
             inttostr(sessionNo) +#9+
             inttostr(s1_marker) +#9+
             inttostr(s2_marker) +#9+
             inttostr(s3_marker) +#9+
             inttostr(s4_marker) +#9+
             //inttostr(TMS_marker) +#9+
             inttostr(s1_shape) +#9+
             inttostr(s1_quad) +#9+
             floattostr(s1_duration) +#9+
             floattostr(s1_s2_isi) +#9+
             inttostr(s2_shape_position_1) +#9+
             inttostr(s2_shape_position_2) +#9+
             inttostr(s2_shape_position_3) +#9+
             inttostr(s2_shape_position_4) +#9+
             inttostr(s2_shape_position_5) +#9+
             floattostr(s2_duration) +#9+
             floattostr(s2_s3_isi) +#9+
             inttostr(s3_shape) +#9+
             inttostr(s3_distractor_shape) +#9+
             inttostr(s3_quad) +#9+
             floattostr(s3_duration) +#9+
             floattostr(s3_s4_isi) +#9+
             inttostr(s4_shape) +#9+
             inttostr(s4_distractor_shape) +#9+
             inttostr(s4_quad) +#9+
             floattostr(s4_duration) +#9+
             floattostr(Response_Time_after_S4) +#9+
             inttostr(Feedback_shape) +#9+
             floattostr(Feedback_duration_after_response_time) +#9+
             floattostr(ITI_after_feedback) +#9+
             inttostr(s1_colour) +#9+
             inttostr(s2_colour_position_1) +#9+
             inttostr(s2_colour_position_2) +#9+
             inttostr(s2_colour_position_3) +#9+
             inttostr(s2_colour_position_4) +#9+
             inttostr(s2_colour_position_5) +#9+
             inttostr(s3_colour) +#9+
             inttostr(s3_distractor_colour) +#9+
             inttostr(s4_colour) +#9+
             inttostr(s4_distractor_colour) +#9+
             inttostr(keyMapping) +#9+
             inttostr(taskType) +#9+
            // floattostr(TMS_s3_SOA) +#9+
             inttostr(TMS_s3_SOA) +#9+
             Experimental_Condition +#9+
             inttostr(observedDataResponseRecord[trialNo]) +#9+
             inttostr(observedDataCorrectResponseRecord[trialNo]) +#9+
            // floattostrf(RT_ms,fffixed,6,2) +#9+
             inttostr(RT_ms)  +#9+
             inttostr(RT_ms_minus_constant_error)  +#9+
           //  inttostr(timeStamp_ms)  +#9+
             inttostr(s1_onsetTime) +#9+
             inttostr(s2_onsetTime) +#9+
             inttostr(s3_onsetTime) +#9+
             inttostr(TMS_onsetTime) +#9+
             inttostr(s4_onsetTime) +#9+
             inttostr(response_onsetTime) +#9+
             inttostr(feedback_onsetTime) +#9+
             inttostr(blank_onsetTime) +#9+
             inttostr(isRuinedTrial)

             {inttostr(triggerStationRT)   }

             );

    closefile(f);


    // If there are practice trials, check if performance is satisfactory to proceed once training trials are completed

    if ((N_trials_before_pause_training>0) and (trialNo = N_trials_before_pause_training-1)) then
    begin

      // find the number of correct responses in the training trials
      tot:=0;
      for c:=0 to trialNo do
      begin
        if (observedDataCorrectResponseRecord[c]=1) then tot := tot + 1;
      end;


      if ((tot/(trialNo+1)) < Minimum_training_accuracy) then  // performance is not yet at minimum acceptable level
      begin
       // showmessage ('Not good enough');
      //
        SDL_hidewindow(ef.surface);
        Form2.visible:=true;

        Form2.BringToFront;
        Form2.Button1.visible:=false;
        Form2.Button2.visible:=false;
        Form2.Button3.visible:=true;
        Form2.label1.visible:=true;
        Form2.label1.Caption:=('Your accuracy is ' + format('%f',[( tot/(trialNo+1)*100)]) + '%. You need to reach ' + format('%f',[( Minimum_training_accuracy*100)]) + '%. Please continue training.');
        trialNo:=0;
      end
      else
      begin
      //  showmessage('Satisfactory');
         // Display the instructions again at the end of the training phase

          SDL_hidewindow(ef.surface);
          Form2.visible:=true;
          Form2.BringToFront;
          Form2.Button1.visible:=false;
          Form2.Button2.visible:=false;
          Form2.Button3.visible:=true;
          Form2.label1.visible:=true;
          Form2.label1.Caption:=('Your accuracy is ' + format('%f',[( tot/(trialNo+1)*100)]) + '%. Well done. Please continue with the main experiment.');

        trialNo:=trialNo+1;
      end;


    end
    else
    begin
      trialNo:=trialno+1;
    end;

  end;// when all trials are done

  TerminateApplication;
end;//end of experiment
//------------------------------------------------------------------------------




//------------------------------------------------------------------------------
procedure TForm1.Button1Click(Sender: TObject);

var
  experiment_dir:string;
  inputDataFilename: string;

  Instructions_ODD_participants,Instructions_EVEN_participants:string;


  fl: Tstringlist;
  inputFileDir, configDataFilename:string;
  nInputFiles:integer;

  yr,mon,day,hr, min,sec,msec:word;

  sessionNo:integer;
  numstr:string;
  c:integer;
begin

  experiment_dir:=extractfilepath(Opendialog1.filename);
  inputFileDir :=     experiment_dir + 'Input data\' ;


  //determine which input file to load : random or specified
  if (combobox1.itemindex = 0) then
  begin
    // find the number of input files
    fl := Tstringlist.create;
    fl:= findallfiles(inputFileDir, '*.txt', false);

    nInputFiles :=  fl.count;
    randomize;
    trialOrderFileNo := random(nInputFiles)+1;
  end
  else
  begin
    trialOrderFileNo:=strtoint(combobox1.Items[combobox1.itemindex]);
  end;


  SaveDialog1.initialdir:= experiment_dir + 'Output data\';
  SaveDialog1.defaultext:='txt';

  participantID := ParticipantIDForm.Label20.caption;

  // generate a unique output data file name based on the date and time
  decodeDate(date, yr, mon,day);
  decodeTime(time, hr, min,sec,msec);
  numstr:= inttostr(yr) + '_' +  inttostr(mon) + '_' + inttostr(day) + '_'+ inttostr(hr) + '-' +  inttostr(min) + '-' + inttostr(sec);


  sessionNo := strtoint(Combobox2.items[Combobox2.Itemindex]);
  SaveDialog1.filename:='OutputData_'+ numstr + '_' + inttostr(trialOrderFileNo) + '_' + inttostr(sessionNo) + '_'+ participantID;

  SaveDialog1.execute;

  configDataFilename := experiment_dir +  'Configuration.txt';

  Instructions_ODD_participants:= getStringForParameter(configDataFilename, 'Instructions_ODD_participants:');
  Instructions_EVEN_participants:= getStringForParameter(configDataFilename, 'Instructions_EVEN_participants:');

  //observerNo:= strtoint(Combobox1.items[Combobox1.Itemindex]);
   if (trialOrderFileNo mod 2)=1 then
 begin
  form2.image1.picture.Loadfromfile(experiment_dir + Instructions_ODD_participants);
 end
 else
 begin
  form2.image1.picture.Loadfromfile(experiment_dir+ Instructions_EVEN_participants);
 end;

 //// change this back************************************************************
 Form2.visible:=true;
 Form1.visible:=false;
// RunExperiment(Sender);      //delete this after debugging


end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TForm1.Button2Click(Sender: TObject);
begin
  TerminateApplication;
end;

//------------------------------------------------------------------------------



//------------------------------------------------------------------------------
procedure TForm1.FormCreate(Sender: TObject);

begin

  Form1.visible:=false;
   Form1.visible:=true;

  TForm1Handle:=GetForegroundWindow;
  Combobox1.itemindex:=0;
  Combobox2.itemindex:=0;

  ConnectTriggerStation;
  label12.visible:=true;
  if (isTriggerStation) then
  begin
    label12.caption:=('TriggerStationUSB Serial: ' + IntToStr(serialNumber));
  end
  else
  begin
    label12.caption:=('No triggerstation');
  end;

  hide;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TForm1.ShowControlsIfReady(Sender: TObject);

begin
   if((combobox3.itemindex > -1) and
   (combobox13.itemindex > -1) and
   (combobox14.itemindex > -1))then
   begin
      Button3.enabled := true;
   end;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure TForm1.Button3Click(Sender: TObject);

var
  configDataFilename,experiment_dir:string;

begin

  OpenDialog1.initialdir:= (getCurrentDir + '\Experiment Library');
  //showmessage(  OpenDialog1.initialdir);
  OpenDialog1.execute;
  //showmessage(OpenDialog1.filename);


  if FileExists(Opendialog1.filename) then
  begin
    memo1.lines.Loadfromfile(Opendialog1.filename);
    button1.enabled:=true;
  end;


  // get monitor name
  experiment_dir:=extractfilepath(Opendialog1.filename);
  configDataFilename := experiment_dir +  'Configuration.txt';

  if not FileExists(configDataFilename)  then
  begin
    showmessage('File does not exist: '+ configDataFilename);
    terminateApplication;
  end;

  radiogroup4.items[0]:= getStringForParameter(configDataFilename, 'Monitor_name:');

end;

//------------------------------------------------------------------------------



end.
