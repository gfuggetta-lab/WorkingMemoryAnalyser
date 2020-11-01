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


function IntroShowModal: Boolean;
procedure TrialResultsShow(const accuracy, minAccuracy: Double);

implementation

{$R *.lfm}


procedure TForm2.Button1Click(Sender: TObject);
begin
 //form1.RunExperiment(Sender);
 ModalResult:=mrOK;
end;

procedure TForm2.Button2Click(Sender: TObject);
begin
end;

procedure TForm2.Button3Click(Sender: TObject);
begin
end;

procedure TForm2.FormResize(Sender: TObject);
begin
  Button3.Left := (ClientWidth - Button3.Width) div 2;
end;

function IntroShowModal: Boolean;
begin
  Form2.Button1.visible:=true;
  Form2.Button2.visible:=true;
  Form2.Button3.visible:=false;
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
end;

end.
