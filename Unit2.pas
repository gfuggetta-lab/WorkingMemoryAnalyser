unit Unit2;

{$MODE Delphi}

interface

uses
  LCLType,
  SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls,gl, main, display2, SDL2;

type

  { TForm2 }

  TForm2 = class(TForm)
    Button3: TButton;
    Image1: TImage;
    Button1: TButton;
    Button2: TButton;
    Label1: TLabel;
    procedure Button1Click(Sender: TObject);
    procedure Button2Click(Sender: TObject);
    procedure Button3Click(Sender: TObject);
    procedure FormResize(Sender: TObject);
   // procedure showMemo(Sender: TObject);
  //  procedure showBMP(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  Form2: TForm2;


implementation

{$R *.lfm}


procedure TForm2.Button1Click(Sender: TObject);
begin
 form1.RunExperiment(Sender);
end;

procedure TForm2.Button2Click(Sender: TObject);
begin
  TerminateApplication;
end;

procedure TForm2.Button3Click(Sender: TObject);
var
  wnd:hWnd;
begin
 Form2.visible:=false;

 //SetForegroundWindow(eF.Wnd);
 // SDL_hidewindow(ef.surface);

 SDL_maximizeWindow(ef.surface);
 SDL_ShowWindow(eF.surface);

//SDL_RaiseWindow(eF.surface);
               {

 setForegroundWindow(ef.Wnd);
 SetWindowPos(ef.Wnd, HWND_TOP, 0,0,0,0, SWP_NOMOVE or SWP_NOSIZE	);
 application.processmessages;
               }
end;

procedure TForm2.FormResize(Sender: TObject);
begin
  Button3.Left := (ClientWidth - Button3.Width) div 2;
end;

end.
