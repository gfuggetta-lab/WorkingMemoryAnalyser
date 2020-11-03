unit ParticipantID;

{$mode delphi}

interface

uses
  Classes, SysUtils, FileUtil, Forms, Controls, Graphics, Dialogs, StdCtrls;

type

  { TParticipantIDForm }

  TParticipantIDForm = class(TForm)
    Button1: TButton;
    Button2: TButton;
    ComboBox10: TComboBox;
    ComboBox11: TComboBox;
    ComboBox12: TComboBox;
    ComboBox8: TComboBox;
    ComboBox9: TComboBox;
    Label14: TLabel;
    Label15: TLabel;
    Label16: TLabel;
    Label17: TLabel;
    Label18: TLabel;
    Label19: TLabel;
    Label20: TLabel;
    procedure Button1Click(Sender: TObject);
    procedure Button2Click(Sender: TObject);
    procedure FormClose(Sender: TObject; var CloseAction: TCloseAction);
    procedure FormCreate(Sender: TObject);
    procedure UpdateLabel(Sender: Tobject);

  private
    { private declarations }
    didHitOk: Boolean;
  public
    { public declarations }
  end;

var
  ParticipantIDForm: TParticipantIDForm;

implementation

{$R *.lfm}

uses main;

//------------------------------------------------------------------------------
procedure TParticipantIDForm.FormCreate(Sender: TObject);


begin
   Button2.Enabled:=true;
   Label20.visible:=false;
   Form1.hide;
   UpdateLabel(nil);
end;

procedure TParticipantIDForm.Button1Click(Sender: TObject);
begin
  Close;
end;

procedure TParticipantIDForm.Button2Click(Sender: TObject);
begin
  didHitOk := true;
  hide;
  Form1.show;
end;

procedure TParticipantIDForm.FormClose(Sender: TObject;
  var CloseAction: TCloseAction);
begin
  if (not didHitOk) then Application.Terminate;
end;

procedure TParticipantIDForm.UpdateLabel(Sender: Tobject);
begin
     if ((combobox8.itemindex > -1) and
     (combobox9.itemindex > -1) and
     (combobox10.itemindex > -1) and
     (combobox11.itemindex > -1) and
     (combobox12.itemindex > -1)) then
     begin
      label20.caption := (combobox8.items[combobox8.itemindex]+
      combobox9.items[combobox9.itemindex]+
      combobox10.items[combobox10.itemindex]+
      combobox11.items[combobox11.itemindex]+
      combobox12.items[combobox12.itemindex]);
      Label20.visible:=true
     end;
  Button2.Enabled:=(ComboBox8.Text <> '')
    and (ComboBox9.Text <> '')
    and (ComboBox10.Text <> '')
    and (ComboBox11.Text <> '')
    and (ComboBox12.Text <> '');
end;

end.

