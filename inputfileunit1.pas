unit inputfileunit1;

{$mode delphi}

interface

uses
  Classes, SysUtils, dialogs;

{type
Tcolour = array [0..2] of real;
}
type
TcolourReal = record
 r,g,b:real;
 end;

type
TcolourInteger = record
 r,g,b:integer;
 end;


type
TpauseTrialsData = record
  trialNo : integer;
  messageNo : integer;
  message : string[255];
end;


type
Tptd = record
 NpauseTrials:integer;
 pauseTrialsData:array of TpauseTrialsData;
end;





{
type
 TpauseTrialsData = record
   trialNo : integer;
   messageNo : integer;
   message : string[255];
 end;}



//procedure getColours(filename:string; var colours:Tcolour);
 procedure getShapeColours(filename:string; var colours: array of TcolourReal);
 procedure getColoursForParameter(filename:string; parameterString:string; var colour: TcolourReal);
 //procedure getIntegerForParameter(filename:string; parameterstring:string; var data:integer);
 function getIntegerForParameter(filename:string; parameterstring:string):integer;
 function getRealForParameter(filename:string; parameterstring:string):real;
 function getStringForParameter(filename:string; parameterstring:string):string;
 function getStringLineForParameter(filename:string; parameterstring:string):string;


 // procedure readPauseTrialOrderData(filename: string ;  pauseTrialsData : array of TpauseTrialsData; var NpauseTrials:integer   );

   procedure readPauseTrialOrderData(filename: string ;   var ptd:Tptd   );
 // procedure setUpPauseTrials(InputDataFileName, ConfigDataFileName : string;  pauseTrialsData: array of TpauseTrialsData);
  procedure setUpPauseTrials(InputDataFileName, ConfigDataFileName : string; var ptd: Tptd);
implementation


//------------------------------------------------------------------------------
function stripFirstIntegerFromString(str:string):integer;
var
  c:integer;
  numstr:string;

begin
  c:=0;
  repeat
    c:=c+1;
  until str[c] in ['0'..'9'];

  numstr:='                          ';
  repeat
    numstr[c]:=str[c];
    //showmessage('numstr=' +numstr);
    c:=c+1;
  until not(str[c] in ['0'..'9']) ;
  result:=strtoint(trim(numstr));

  //showmessage('r='+ inttostr(r));

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
function stripFirstRealFromString(str:string):real;
var
  c:integer;
  numstr:string;

begin
  c:=0;
  repeat
    c:=c+1;
  until str[c] in ['0'..'9','.'];

  numstr:='                          ';
  repeat
    numstr[c]:=str[c];
    //showmessage('numstr=' +numstr);
    c:=c+1;
  until not(str[c] in ['0'..'9','.']) ;
  result:=strtofloat(trim(numstr));

  //showmessage('r='+ inttostr(r));

end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
function stripFirstItemFromString(str:string):string;
var
  c:integer;
  itemstr:string;

begin
  c:=0;
  repeat
    c:=c+1;
  until str[c] in ['!'..'~'];
  itemstr:='                                                                       ';
  repeat
    itemstr[c]:=str[c];
    c:=c+1;
  until  not(str[c] in  ['!'..'~']);
  result:=trim(itemstr);
end;
//------------------------------------------------------------------------------



//------------------------------------------------------------------------------
function getRGBtripletFromString(str:string):TcolourInteger;
var
  c:integer;
  r,g,b:integer;
  numstr:string;

begin
  c:=0;
  repeat
    c:=c+1;
  until str[c] in ['0'..'9'];

  numstr:='                          ';
  repeat
    numstr[c]:=str[c];
    //showmessage('numstr=' +numstr);
    c:=c+1;
  until not(str[c] in ['0'..'9']) ;
  r:=strtoint(trim(numstr));
  //showmessage('r='+ inttostr(r));


  repeat
    c:=c+1;
  until str[c] in ['0'..'9'];


  numstr:='                          ';
  repeat
    numstr[c]:=str[c];
    //showmessage('numstr=' +numstr);
    c:=c+1;
  until not(str[c] in ['0'..'9']) ;
  g:=strtoint(trim(numstr));
  //showmessage('g='+ inttostr(g));


  repeat
    c:=c+1;
  until str[c] in ['0'..'9'];


  numstr:='                          ';
  repeat
    numstr[c]:=str[c];
    //showmessage('numstr=' +numstr);
    c:=c+1;
  until not(str[c] in ['0'..'9']) ;
  b:=strtoint(trim(numstr));
  //showmessage('b='+ inttostr(b));

  result.r:=r;
  result.g:=g;
  result.b:=b;
end;
//------------------------------------------------------------------------------

function colourIntegerToColourReal(colour:TcolourInteger):TcolourReal;
begin

  result.r := colour.r/255;
  result.g := colour.g/255;
  result.b := colour.b/255;

end;

//------------------------------------------------------------------------------
procedure getShapeColours(filename:string; var colours:array of TcolourReal);
// extract rgb triplets for Shapes_colour_ definitions inthe input file

var
  f:textfile;
  str0,str1,str2: string;
  i,j:integer;
  num:integer;
  r,g,b:integer;

  c:integer;
  numstr:string;
 // colour: array [0..15] of Tcolour;
  //colourArray: array [0..15] of TcolourInteger;
begin
  AssignFile(f, filename);

  try
    reset(f);

    str2:='Not_specified';

    // read each line of the text file to parse Shapes_colour_ information
    while not eof(f) do
    begin
      readln(f,str0);
      i := pos('Shapes_colour_',str0);
      if i<>0 then
      begin // text found
        //showmessage (str0 );


        // find ':' following the shapes colour number
        j := pos(':',str0);
        if j<>0 then
        begin  // text found. Get the shapes colour number before the colon
          num := strtoint(copy(str0,15, (j-15)));
          //showmessage('colour num ='+inttostr(num));
        end;

        // get the text following the colon, with any white spaces at the beginning and end trimmed
        str1:=trim(copy(str0,j+1,length(str0) - (j+1) ));
        //showmessage ('trimmed:' + str1 );

        // put the triplet into the colour array
        //colourArray[num]:=colourIntegerToColourReal(getRGBtripletFromString(str1));
        //colours[num]:=getRGBtripletFromString(str1);
        colours[num]:= colourIntegerToColourReal(getRGBtripletFromString(str1));
        //showmessage('rgb['+inttostr(num)+']=' + floattostr(colourArray[num].r) +' ' +  floattostr(colourArray[num].g) +' ' + floattostr(colourArray[num].b) );
      end;
    end;
    closeFile(f);


  except
   on E: EInOutError do
     showmessage('File error: '+ E.Message);
  end;

  //result:=str2;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure getColoursForParameter(filename:string; parameterString:string; var colour: TcolourReal);

var
  f:textfile;
  i:integer;
  str0,str1: string;


begin
AssignFile(f, filename);

  try
    reset(f);

    // read each line of the text file to find parameterString
    while not eof(f) do
    begin
      readln(f,str0);
      i := pos(parameterString,str0);
      if i<>0 then
      begin // text found
        //showmessage (str0 );
        // get the text following the colon, with any white spaces at the beginning and end trimmed
        str1:=trim(copy(str0,i+length(parameterString),length(str0) - i+length(parameterString) ));
        //showmessage (str1 );
        colour:= colourIntegerToColourReal(getRGBtripletFromString(str1));
        //colour:= getRGBtripletFromString(str1);
      end;
    end;
    closeFile(f);
    //showmessage(parameterString +'=' + floattostr(colour.r) +' '+  floattostr(colour.g)   +' '+  floattostr(colour.b));
  except
   on E: EInOutError do
     showmessage('File error: '+ E.Message);
  end;
end;





//------------------------------------------------------------------------------
function getIntegerForParameter(filename:string; parameterstring:string):integer;
var
  f:textfile;
  i:integer;
  str0,str1: string;

  data:integer;

begin
  AssignFile(f, filename);

  try
    reset(f);

    // read each line of the text file to parse
    while not eof(f) do
    begin
      readln(f,str0);
      i := pos(parameterString,str0);
      if i<>0 then
      begin // text found
        //showmessage (str0 );
        // get the text following the colon, with any white spaces at the beginning and end trimmed
        str1:=trim(copy(str0,i+length(parameterString),length(str0) - i+length(parameterString) ));
        //showmessage (str1 );
        //colour:= colourIntegerToColourReal(getRGBtripletFromString(str1));
        data:=stripFirstIntegerFromString(str1);

      end;
    end;
    closeFile(f);

  except
   on E: EInOutError do
     showmessage('File error: '+ E.Message);
  end;

  result:= data;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
function getRealForParameter(filename:string; parameterstring:string):real;
var
  f:textfile;
  i:integer;
  str0,str1: string;

 // data:integer;
  data: real;

begin
  data := 0;
  AssignFile(f, filename);

  try
    reset(f);

    // read each line of the text file to parse
    while not eof(f) do
    begin
      readln(f,str0);
      i := pos(parameterString,str0);
      if i<>0 then
      begin // text found

        //showmessage (str0 );
        // get the text following the colon, with any white spaces at the beginning  trimmed
       // str1:=trim(copy(str0,i+length(parameterString),length(str0) - i+length(parameterString) ));
        str1:=trimleft(copy(str0,i+length(parameterString),length(str0) - i+length(parameterString) ));

        //colour:= colourIntegerToColourReal(getRGBtripletFromString(str1));
        data:=stripFirstRealFromString(str1);
        //showmessage (floattostr(data) );
      end;
    end;
    closeFile(f);

  except
   on E: EInOutError do
     showmessage('File error: '+ E.Message);
  end;

  result:= data;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
function getStringForParameter(filename:string; parameterstring:string):string;
var
  f:textfile;
  i:integer;
  str0,str1, str2: string;

  //data:string;

begin
  AssignFile(f, filename);

  try
    reset(f);

    // read each line of the text file to parse
    while not eof(f) do
    begin
      readln(f,str0);
      i := pos(parameterString,str0);
      if i<>0 then
      begin // text found
        //showmessage (str0 );
        // get the text following the colon, with any white spaces at the beginning and end trimmed
        str1:=trim(copy(str0,i+length(parameterString),length(str0) - i+length(parameterString) ));
        str2:= stripFirstItemFromString(str1);
       // showmessage (str2 );
        //colour:= colourIntegerToColourReal(getRGBtripletFromString(str1));
        //data:=stripFirstIntegerFromString(str1);

      end;
    end;
    closeFile(f);

  except
   on E: EInOutError do
     showmessage('File error: '+ E.Message);
  end;

  result:= str2;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
function getStringLineForParameter(filename:string; parameterstring:string):string;
var
  f:textfile;
  i:integer;
  str0,str1, str2: string;

  //data:string;

begin
  AssignFile(f, filename);

  try
    reset(f);

    // read each line of the text file to parse
    while not eof(f) do
    begin
      readln(f,str0);
      i := pos(parameterString,str0);
      if i<>0 then
      begin // text found
        //showmessage (str0 );
        // get the text following the colon, with any white spaces at the beginning and end trimmed
        str1:=trim(copy(str0,i+length(parameterString),length(str0) - i+length(parameterString) ));

       // showmessage (str2 );
        //colour:= colourIntegerToColourReal(getRGBtripletFromString(str1));
        //data:=stripFirstIntegerFromString(str1);

      end;
    end;
    closeFile(f);

  except
   on E: EInOutError do
     showmessage('File error: '+ E.Message);
  end;

  result:= str1;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure readPauseTrialOrderData(filename: string ;   var ptd:Tptd   );

var
  dat: char;
  datstring: string;

  c:integer;
  ff:textfile;

begin

  AssignFile(ff, filename);

  // advance to the start of the pause trials data'
  reset(ff);
  repeat
    readln(ff,datstring);
  until (trim(datstring)='#PAUSE_TRIALS');

  // count the pause trials
  ptd.NpauseTrials:=0;
  repeat
    readln(ff,datstring) ;
    if ((trim(datstring)<>'') and (trim(datstring)<>'#END'))  then
    begin
      ptd.NpauseTrials:=ptd.NpauseTrials+1;
     //showmessage('valid line' + trim(datstring));
    end;
  until (trim(datstring)='#END');


  // advance to the start of the pause trials data'
  reset(ff);
  repeat
    readln(ff,datstring);
  until (trim(datstring)='#PAUSE_TRIALS');

 // read the pause trials data
  for c:=0 to  ptd.NpauseTrials-1 do
  begin
    readln(ff,ptd.pauseTrialsData[c].trialNo, ptd.pauseTrialsData[c].messageNo ) ;
   //showmessage('trialNo: '+ inttostr(pauseTrialsData[c].trialNo) + ' messageNo: ' + inttostr(pauseTrialsData[c].messageNo));
  end;
  closeFile(ff);
end;
//------------------------------------------------------------------------------




//------------------------------------------------------------------------------
procedure setUpPauseTrials(InputDataFileName, ConfigDataFileName : string;  var ptd: Tptd);
var
  pauseMessages:array[0..10] of string;
  c:integer;
begin

  // load 11 pause trial messages
   for c:=0 to 10 do
   begin
     pauseMessages[c]:= getStringLineForParameter(configDataFilename, 'Message_'+inttostr(c)+':');
     //showmessage(pauseMessages[c]);
   end;

   // get the list of pause trials and message indices
   readPauseTrialOrderData(InputDataFileName, ptd);

   //for each pause trial, set the message text corresponding to the message index
   for c:=0 to ptd.NpauseTrials-1 do
   begin
     ptd.pauseTrialsData[c].message:= pauseMessages[ptd.pauseTrialsData[c].messageNo];
     //showmessage(ptd.pauseTrialsData[c].message);
   end;
end;
//------------------------------------------------------------------------------

end.

