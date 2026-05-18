unit Unit2;

{$MODE Delphi}

interface

uses
  LCLType,
  SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls,gl, main, display2, SDL2, sdlshared, sdl2_mixer;

type

  { TForm2 }

  TForm2 = class(TForm)
    Button3: TButton;
    btnPlay: TButton;
    btnStop: TButton;
    Image1: TImage;
    Button1: TButton;
    Button2: TButton;
    Label1: TLabel;
    procedure btnPlayClick(Sender: TObject);
    procedure btnStopClick(Sender: TObject);
    procedure Button1Click(Sender: TObject);
    procedure Button2Click(Sender: TObject);
    procedure Button3Click(Sender: TObject);
    procedure FormClose(Sender: TObject; var CloseAction: TCloseAction);
    procedure FormDestroy(Sender: TObject);
    procedure FormResize(Sender: TObject);
   // procedure showMemo(Sender: TObject);
  //  procedure showBMP(Sender: TObject);
  private
    { Private declarations }

    loadedAudio: string;
    audio: PMix_Chunk;

    function PrepareAudio(const wavFile: string): PMix_Chunk;
    procedure ReleaseAudio;
    procedure StopAudio(immediate: Boolean);

  public
    { Public declarations }
    instructionAudio: string;
  end;

var
  Form2: TForm2;

const
  PlayChannel = 1;


function IntroShowModal: Boolean;
procedure TrialResultsShow(const accuracy, minAccuracy: Double);

implementation

{$R *.lfm}


procedure TForm2.Button1Click(Sender: TObject);
begin
 //form1.RunExperiment(Sender);
 ModalResult:=mrOK;
end;

procedure TForm2.btnStopClick(Sender: TObject);
begin
  StopAudio(false);
end;

procedure TForm2.btnPlayClick(Sender: TObject);
var
  wav : PMix_Chunk;
begin
  StopAudio(true);
  wav := PrepareAudio(instructionAudio);
  Mix_PlayChannel(PlayChannel, wav, 0);
end;

procedure TForm2.Button2Click(Sender: TObject);
begin

end;

procedure TForm2.Button3Click(Sender: TObject);
begin

end;

procedure TForm2.FormClose(Sender: TObject; var CloseAction: TCloseAction);
begin
  StopAudio(false);
end;

procedure TForm2.FormDestroy(Sender: TObject);
begin
  ReleaseAudio;
end;

procedure TForm2.FormResize(Sender: TObject);
begin
  Button3.Left := (ClientWidth - Button3.Width) div 2;
end;

function TForm2.PrepareAudio(const wavFile: string): PMix_Chunk;
begin
  if (wavFile = loadedAudio) then begin
    Result := audio;
    Exit;
  end;
  ReleaseAudio;
  audio := Mix_LoadWAV(PAnsiChar(wavFile));
  if audio <> nil then begin
    loadedAudio := wavFile;
  end;
  Result := audio;
end;

procedure TForm2.ReleaseAudio;
begin
  if audio = nil then Exit;
  StopAudio(true);
  Mix_FreeChunk(audio);
  loadedAudio := '';
  audio := nil;
end;

procedure TForm2.StopAudio(immediate: Boolean);
begin
  if Mix_Playing(PlayChannel) = 0 then Exit;
  if (immediate) then
    Mix_HaltChannel(PlayChannel)
  else
    Mix_FadeOutChannel(PlayChannel, 200);
end;

function IntroShowModal: Boolean;
var
  hasAudio: boolean;
begin
  InitSDL;
  Form2.Button1.visible:=true;
  Form2.Button2.visible:=true;
  Form2.Button3.visible:=false;
  hasAudio := (Form2.instructionAudio<>'') and FileExists(Form2.instructionAudio);
  Form2.btnPlay.visible:=hasAudio;
  Form2.btnStop.visible:=hasAudio;
  Form2.label1.visible:=false;
  Result := Form2.ShowModal = mrOK;
end;

procedure TrialResultsShow(const accuracy, minAccuracy: Double);
const
  BadAccuracy  : string = 'Your accuracy is %f%%. You need to reach %f%%. Please continue training.';
  GoodAccuracy : string = 'Your accuracy is %f%%. Well done. Please continue with the main experiment.';
begin
  Form2.Button1.visible:=false;
  Form2.Button2.visible:=false;
  Form2.Button3.visible:=true;
  Form2.label1.visible:=true;
  if accuracy < minAccuracy then
    Form2.label1.Caption:=format(BadAccuracy, [accuracy*100, minAccuracy*100])
  else
    Form2.label1.Caption:=format(GoodAccuracy, [accuracy*100]);
  Form2.ShowModal;
  Form2.visible:=false;
end;

end.
