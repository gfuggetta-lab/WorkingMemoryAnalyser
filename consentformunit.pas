unit consentformunit;

{$mode delphi}

interface

uses
  Types, LCLType, LCLIntf,
  Classes, SysUtils, Forms, Controls, Graphics, Dialogs, StdCtrls, RichMemo, Math;

type

  { TConsentForm }

  TConsentForm = class(TForm)
    CheckBox1: TCheckBox;
    CheckBox2: TCheckBox;
    CheckBox3: TCheckBox;
    Label1: TLabel;
    btnProceed: TButton;
    Label2: TLabel;
    Label3: TLabel;
    RichMemo1: TRichMemo;
    procedure FormCreate(Sender: TObject);
    procedure FormResize(Sender: TObject);
    procedure Label1Click(Sender: TObject);
    procedure Label1Resize(Sender: TObject);
    procedure Label2Click(Sender: TObject);
    procedure Label3Click(Sender: TObject);
  private

  public

  end;

var
  ConsentForm: TConsentForm;

implementation

{$R *.lfm}

{ TConsentForm }

procedure TConsentForm.Label1Resize(Sender: TObject);
begin
end;

procedure TConsentForm.Label2Click(Sender: TObject);
begin
  CheckBox2.Checked:=not CheckBox2.Checked;
end;

procedure TConsentForm.Label3Click(Sender: TObject);
begin
  CheckBox3.Checked:=not CheckBox3.Checked;
end;

procedure AutoSizeLable(l: TLabel; w: integer);
var
  b : TBitmap;
  R : TRect;
  Flags: Cardinal;
begin
  if not Assigned(l) then Exit;
  w := Max(w, 1);
  l.Width := w;
  Flags := DT_CALCRECT or DT_EXPANDTABS;
  Flags := Flags or DT_WORDBREAK;
  r := Bounds(0,0,w, 0);
  DrawText(l.Canvas.Handle, PChar(l.Caption), length(l.caption), R, Flags);
  l.Height := r.Height;
end;


procedure TConsentForm.FormResize(Sender: TObject);
var
  w: integer;
begin
  w := ClientWidth - Label1.Left - 10;
  AutoSizeLable(Label1, w);
  AutoSizeLable(Label2, w);
  AutoSizeLable(Label3, w);
end;

procedure TConsentForm.Label1Click(Sender: TObject);
begin
  CheckBox1.Checked:=not CheckBox1.Checked;
end;

procedure TConsentForm.FormCreate(Sender: TObject);
begin
  Label1.Caption:='I have read this form (or a person with parental responsibility have read the information for children under the age of 16), and consent to participate, and understand what is required.';
  Label2.Caption:='I am aware that I can withdraw at any time by pressing “Esc” while the computer task is running.';
  Label3.Caption:='I understand I can withdraw my data after the experiment has finished, up to 4 months from my participation by emailing the researcher with my unique Participant ID code.';
end;

end.

