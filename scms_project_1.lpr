program scms_project_1;

{$MODE Delphi}
{$ifdef darwin}
{$linkframework SDL2}
{$linkframework SDL2_mixer}
{$linkframework SDL2_ttf}
{$endif}

uses
  Forms, Interfaces,
  main in 'main.pas' {Form1},
  Unit2 in 'Unit2.pas' {Form2},
  aboutFormUnit in 'aboutFormUnit.pas' {AboutForm},
  ParticipantID in 'ParticipantID.pas' {ParticipantIDForm},
  Display2 in 'Display2.pas'; {Displayenviroform}

{$R *.res}

begin
  Application.Scaled:=True;
  Application.Initialize;
  Application.ShowMainForm:=false;
  Application.CreateForm(TForm1, Form1);
  Application.CreateForm(Tdisplayenviroform, eF);
  Application.CreateForm(TForm2, Form2);
  Application.CreateForm(TParticipantIDForm, ParticipantIDForm);
  Application.CreateForm(TAboutForm, AboutForm);
  Application.Run;
end.
