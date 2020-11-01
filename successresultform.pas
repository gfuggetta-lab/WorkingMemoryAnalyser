unit SuccessResultForm;

{$mode delphi}

interface

uses
  LCLIntf,
  Classes, SysUtils, Forms, Controls, Graphics, Dialogs, StdCtrls;

type

  { TSuccessForm }

  TSuccessForm = class(TForm)
    Button1: TButton;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    procedure Button1Click(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure Label3Click(Sender: TObject);
    procedure Label4Click(Sender: TObject);
  private

  public
    outputDir : string;
    uploadUrl : string;
    questioneerUrl: string;
  end;

var
  SuccessForm: TSuccessForm;

procedure ShowSuccessForm(const outputDir, resultFileName : string;
  const uploadUrl : string = 'https://www.cognitoforms.com/GiorgioFuggetta/WorkingMemoryAnalyserUploadOutputFile2';
  const questioneerUrl: string = 'https://roehamptonpsych.az1.qualtrics.com/jfe/form/SV_9zU7Nf3Ped7Ep8h'
);

implementation

{$R *.lfm}

{ TSuccessForm }

procedure TSuccessForm.Label4Click(Sender: TObject);
begin
  if questioneerUrl <> '' then OpenURL(questioneerUrl);
end;

procedure TSuccessForm.Label3Click(Sender: TObject);
begin
  if uploadUrl <> '' then OpenURL(uploadUrl);
end;

procedure TSuccessForm.Button1Click(Sender: TObject);
begin
  if outputDir <> '' then OpenDocument(outputDir);
end;

procedure TSuccessForm.FormShow(Sender: TObject);
begin
  Button1.Visible := outputDir <> '';
  Label3.Visible := uploadUrl <> '';
  Label4.Visible := questioneerUrl <> '';
end;

procedure ShowSuccessForm(const outputDir, resultFileName, uploadUrl, questioneerUrl: string);
var
  sf : TSuccessForm;
begin
  sf := TSuccessForm.Create(Application);
  try
    sf.Position:=poOwnerFormCenter;
    sf.Caption := sf.Caption+' ('+resultFileName+')';
    sf.outputDir := outputDir;
    sf.uploadUrl := uploadUrl;
    sf.questioneerUrl:=questioneerUrl;
    sf.ShowModal;
  finally
    sf.Free;
  end;

end;

end.

