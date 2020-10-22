unit Useful;

//{$MODE Delphi}


interface


  type
  Txyz = record
    x,y,z:real;
  end;

  type
  Txy = record
    x,y:real;
  end;

  type
  T2Darray = array of array of real;

  type
  T2DarrayOfTxyz = array of array of Txyz;


 






  function RFDensity(Eccdeg:real): real;

  procedure MakeGrid(var Xvals,Yvals: T2Darray; Xmin,Xmax,Ymin,Ymax: real; Nrows,Ncols: integer );


  procedure linspace(var Xvals: array of real; Xmin,Xmax: real; Ncols: integer);



  //function MaximumValue(Vals: array of real; NVals: integer): real;

  procedure GetCircleCoords( var X,Y: array of real; Radius: real;Npoints: integer  );

  procedure GetFunnyCircleCoords( var X,Y: array of real; Radius: array of real; Npoints: integer  );

  procedure RotateXYZ(var p: Txyz; theta: real; axis: string); overload;
  procedure RotateXYZ(var X,Y,Z:real; theta: real; axis: string); overload;

  function sign(val:real) :integer;

  procedure RandomOrder1(var Order: array of integer; Seed: LongInt); //overload ;
  procedure RandomOrder(var Order: array of integer; HowManyNumbers: integer; Seed: LongInt); //overload;


  procedure RepArray(var Output: array of real; Arr: array of real; ArrLength, n: integer);
  procedure RepIntegerArray(var Output: array of integer; Arr: array of integer;  n: integer);


  procedure MakeCondsIndexTable(var CondsTable: T2Darray; NLevels: array of integer; NFactors: integer);


  procedure stereoproj(var xpl,xpr,yp: real; x,y,z,DistToScreen,iod: real);


  function Disparity2Depth(Disparity,VergenceDistance,iod:real): real;

  function FindMax(Data:array of integer):integer;

  procedure Map3DTo2D(var X2D, Y2D:real; d, X3D, Y3D, Z3D:real);



  function SimilarTriangle(Opposite0, Adjacent0, Adjacent1:real):real;

  function ExtentAtD(Angle,Dist:real): real;


  //procedure LongToLat(var HLat, HVlat: real; HLong, VLong:real); overload;
  procedure LongToLat(var Lats: Txy; Longs: Txy);


  procedure Normalise(var p:Txyz);

  procedure CalcNormal(p,p1,p2:Txyz; var n:Txyz);
  
  function CrossProduct(p1,p2: Txyz): Txyz;

  function DotProduct(p1,p2: Txyz): real;

  function Modulus(p: Txyz):real;

  function VectorAngle(p1,p2: Txyz): real;

  function ArbitraryRotate(p: Txyz; theta: real; r: Txyz): Txyz;
  
  function VectorSub(p1,p2: Txyz): Txyz;

//  procedure gaussian(var y,x:array of real; mean,sd:real); overload;
  function gaussian(x:real; mean,sd:real):real;

  function DegToRad(Deg: real): real;

  function RadToDeg(Rad: real): real;

 // procedure SaveBitmap(Filename:pchar);

  function mean( data: array of real): extended;
  function std( data: array of real): extended;

  procedure appendToArray(var data: array of real; x: real);
implementation



uses
  Math,SDL,{OpenGL12,}dialogs;


//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
// Receptive Field Density, at eccentricity Eccdeg (must be in degrees)
function RFDensity(Eccdeg:real): real;

const
  s=0.59;
  k=0.0055;

var
  a,b,c:real;

begin
  a:=(s*Eccdeg);
  b:=3*(power(Eccdeg,2))*0.000001;
  c:=8*(power((s*Eccdeg),5.5))*0.0000000001;
  result:=k*(1+a*(1+b+c));
end;

//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
// Makes a linearly spaced regular grid
{ NOTE: REQUIRES THAT THE NUMBER OF ROWS AND COLUMNS ARE EACH GREATER THAN 1
I.E. T2DARRAYS CANT BE REPLACED BY 1 D ARRAYS. OTHERWISE, UNPREDICTABLE ERRORS MAY OCCUR.
ONE SUCH ERROR IS A DIVISION BY ZERO ERROR WHEN USING THE POWER FUNCTION
}
procedure MakeGrid(var Xvals,Yvals: T2Darray; Xmin,Xmax,Ymin,Ymax: real; Nrows,Ncols: integer);

var
  q: real;
  i,j: integer;
   a1:extended;
begin



  // define X grid
  for i:=0 to Nrows-1 do
  begin
    q:=Xmin;
    for j:=0 to Ncols-1 do
    begin
      Xvals[i,j]:=q;
      q:=q+((Xmax-Xmin)/(Ncols-1));
    end;
  end;

    // define Y grid
  q:=Ymin;
  for i:=0 to Nrows-1 do
  begin
    for j:=0 to Ncols-1 do
    begin
      Yvals[i,j]:=q;
    end;
      q:=q+((Ymax-Ymin)/(Nrows-1));
  end;
end;
//------------------------------------------------------------------------------




//------------------------------------------------------------------------------
// Makes a linearly spaced array
procedure linspace(var Xvals: array of real; Xmin,Xmax: real; Ncols: integer);
var
  q: real;
  c: integer;

begin

  q:=Xmin;
  for c:=0 to Ncols-1 do
  begin
    Xvals[c]:=q;
    q:=q+((Xmax-Xmin)/(Ncols-1));
  end;

end;
//------------------------------------------------------------------------------












//------------------------------------------------------------------------------
procedure GetCircleCoords( var X,Y: array of real; Radius: real;Npoints: integer  );
var
c: integer;
theta:real;
begin
  for c:=0 to Npoints-1 do
  begin
  theta:=((2*pi)/Npoints)*c;
  X[c]:=Radius*sin(theta);
  Y[c]:=Radius*cos(theta);
  end;
end;
//------------------------------------------------------------------------------






//------------------------------------------------------------------------------
procedure GetFunnyCircleCoords( var X,Y: array of real; Radius:array of real; Npoints: integer  );
var
c: integer;
theta:real;
begin
  for c:=0 to Npoints-1 do
  begin
  theta:=((2*pi)/Npoints)*c;
  X[c]:=Radius[c]*sin(theta);
  Y[c]:=Radius[c]*cos(theta);
  end;
end;
//------------------------------------------------------------------------------







//------------------------------------------------------------------------------
// Rotate a point X,Y,Z through angle theta around axis 'x','y' or 'z'
procedure RotateXYZ(var p: Txyz; theta: real; axis: string);
var
  Xt,Yt,Zt: real;

begin
  if axis='y' then
  begin
    Xt:=(p.x*cos(theta))+(p.z*sin(theta));
	  Yt:=p.y;
  	Zt:=-(p.x*sin(theta))+(p.z*cos(theta));
  end;

  if axis='x' then
  begin
    Xt:=p.x;
    Yt:=(p.y*cos(theta))-(p.z*sin(theta));
    Zt:=(p.y*sin(theta))+(p.z*cos(theta));
  end;

  if axis='z' then
  begin
    Xt:=(p.x*cos(theta))-(p.y*sin(theta));
    Yt:=(p.x*sin(theta))+(p.y*cos(theta));
    Zt:=p.z;
  end;
  p.x:=Xt;
  p.y:=Yt;
  p.z:=Zt;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
// Rotate a point X,Y,Z through angle theta around axis 'x','y' or 'z'
procedure RotateXYZ(var X,Y,Z:real; theta: real; axis: string);
var
  Xt,Yt,Zt: real;

begin
  if axis='y' then
  begin
    Xt:=(X*cos(theta))+(Z*sin(theta));
	  Yt:=Y;
  	Zt:=-(Z*sin(theta))+(Z*cos(theta));
  end;

  if axis='x' then
  begin
    Xt:=X;
    Yt:=(Y*cos(theta))-(Z*sin(theta));
    Zt:=(Y*sin(theta))+(Z*cos(theta));
  end;

  if axis='z' then
  begin
    Xt:=(X*cos(theta))-(Y*sin(theta));
    Yt:=(X*sin(theta))+(Y*cos(theta));
    Zt:=Z;
  end;
  X:=Xt;
  Y:=Yt;
  Z:=Zt;
end;
//------------------------------------------------------------------------------






            {

//------------------------------------------------------------------------------
// Rotate a point X,Y,Z through angle theta around axis 'x','y' or 'z'
procedure ScaleXYZ(var p: Txyz; ScaleFactor: real; axis: string);
var
  Xt,Yt,Zt: real;

begin
  if axis='y' then
  begin
    Xt:=(p.x*cos(theta))+(p.z*sin(theta));
	  Yt:=p.y;
  	Zt:=-(p.x*sin(theta))+(p.z*cos(theta));
  end;

  if axis='x' then
  begin
    Xt:=p.x;
    Yt:=(p.y*cos(theta))-(p.z*sin(theta));
    Zt:=(p.y*sin(theta))+(p.z*cos(theta));
  end;

  if axis='z' then
  begin
    Xt:=(p.x*cos(theta))-(p.y*sin(theta));
    Yt:=(p.x*sin(theta))+(p.y*cos(theta));
    Zt:=p.z;
  end;
  p.x:=Xt;
  p.y:=Yt;
  p.z:=Zt;
end;
//------------------------------------------------------------------------------

         }











//------------------------------------------------------------------------------
function sign(val:real) :integer;
begin

if val<0 then
begin
result:=-1;
end;

if val=0 then
begin
result:=0;
end;

if val>0 then
begin
result:=1;
end;
end;
//------------------------------------------------------------------------------








 
//------------------------------------------------------------------------------
// Make a list of randomly ordered numbers from 0 to HowManyNumbers-1
procedure RandomOrder1(var Order: array of integer; Seed: LongInt);
var
  Nums: array of integer;
  c,q: integer;
  Nmax: integer;
  RandomNo: real;

begin
  RandSeed:=Seed;
  if Seed=0 then randomize;
// Make random order
  setlength(Nums,length(Order));
  //setlength(Order,HowManyNumbers);
  for c:=0 to (length(Order))-1 do
  begin
    Nums[c]:=c;
  end;

  Nmax:=length(Order);
  for c:=0 to length(Order)-1 do
  begin
    RandomNo:=round(random*(Nmax-1));
    Order[c]:=Nums[round(RandomNo)];
    for q:=round(RandomNo) to (Nmax)-2 do
    begin
      Nums[q]:=Nums[q+1];
    end;
    Nums[Nmax-1]:=0;
    Nmax:=Nmax-1;
  end;
end;
//------------------------------------------------------------------------------



//------------------------------------------------------------------------------
// Make a list of randomly ordered numbers from 0 to HowManyNumbers-1
procedure RandomOrder(var Order: array of integer; HowManyNumbers: integer; Seed: LongInt);
var
  Nums: array of integer;
  c,q: integer;
  Nmax: integer;
  RandomNo: real;

begin
  RandSeed:=Seed;
  if Seed=0 then randomize;
// Make random order
  setlength(Nums,HowManyNumbers);
  //setlength(Order,HowManyNumbers);
  for c:=0 to (HowManyNumbers)-1 do
  begin
    Nums[c]:=c;
  end;

  Nmax:=HowManyNumbers;
  for c:=0 to HowManyNumbers-1 do
  begin
    RandomNo:=round(random*(Nmax-1));
    Order[c]:=Nums[round(RandomNo)];
    for q:=round(RandomNo) to (Nmax)-2 do
    begin
      Nums[q]:=Nums[q+1];
    end;
    Nums[Nmax-1]:=0;
    Nmax:=Nmax-1;
  end;
end;
//------------------------------------------------------------------------------




//------------------------------------------------------------------------------
procedure RepArray(var Output: array of real; Arr: array of real; ArrLength, n: integer);
var
  c,i,k: integer;

begin
  for c:=1 to n do
  begin
  k:=0;
    for i:=(ArrLength*c)-Arrlength to (ArrLength*c)-1 do
    begin
      Output[i]:=Arr[k];
      k:=k+1;
    end;
  end;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
procedure RepIntegerArray(var Output: array of integer; Arr: array of integer;  n: integer);
var
  c,i,k: integer;

begin
  for c:=1 to n do
  begin
  k:=0;
    for i:=(length(Arr)*c)-length(Arr) to (length(Arr)*c)-1 do
    begin
      Output[i]:=Arr[k];
      k:=k+1;
    end;
  end;
end;
//------------------------------------------------------------------------------




procedure MakeCondsIndexTable(var CondsTable: T2Darray; NLevels: array of integer; NFactors: integer);
var

  c,i,h: integer;
  Total,SubBlockTotal,BlockTotal: integer;
  OneFactor: array of real;
  block: array of real;


begin
  Total:=1;
  for c:=0 to NFactors-1 do
  begin
    Total:=Total*NLevels[c];
  end;

  SetLength(CondsTable,round(Total),NFactors);
  SetLength(OneFactor,Total);

  // Make the conditions table, one factor at a time
  for c:=0 to NFactors-1 do
  begin

    // work out number of same condition index within one factor
    SubBlockTotal:=1;
    for i:=c+1 to NFactors-1 do
    begin
      SubBlockTotal:=SubBlockTotal*NLevels[i];
    end;


    SetLength(block,SubBlockTotal*NLevels[c]);


    // Make a block within current factor
    for h:=0 to NLevels[c]-1 do
    begin
      for i:=SubBlockTotal*(h) to (SubBlockTotal*(h+1))-1 do
      begin
       block[i]:=h;
      end;
    end;


    //work out the size of a block within current factor
    BlockTotal:=1;
    for i:=c to NFactors-1 do
    begin
      BlockTotal:=BlockTotal*NLevels[i];
    end;


    // Repeat blocks throughout current factor's list
    RepArray(OneFactor,block,BlockTotal,round(Total/BlockTotal));


    // Put current factor's list into the column of the conditions table.
    for i:=0 to Total-1 do
    begin
      CondsTable[i,c]:=OneFactor[i];
    end;

  end;
end;
//------------------------------------------------------------------------------
















//------------------------------------------------------------------------------
procedure stereoproj(var xpl,xpr,yp: real; x,y,z,DistToScreen,iod: real);
begin

xpl:=((DistToScreen*x)-((iod*z)/2))/(DistToScreen+z);

xpr:=((DistToScreen*x)+((iod*z)/2))/(DistToScreen+z);

yp:=(DistToScreen*y)/(DistToScreen+z);
end;
//------------------------------------------------------------------------------



//------------------------------------------------------------------------------
function Disparity2Depth(Disparity,VergenceDistance,iod:real): real;
var
VergenceAngle: real;
begin
   VergenceAngle:=arctan(VergenceDistance/(iod/2));
   Result:=(iod/2)*tan(VergenceAngle+(Disparity/2));//-VergenceDistance
end;
//------------------------------------------------------------------------------



//------------------------------------------------------------------------------
function FindMax(Data:array of integer):integer;

var
  TheMax:integer;
  UberMax:integer;
  i:integer;

begin

  TheMax:=0;
  UberMax:=0;

  for i:=0 to length(Data)-1 do
  begin

    TheMax:=max(TheMax, Data[i] );

    if TheMax>UberMax then
    begin
      UberMax := TheMax;
    end;
  end;
  Result:=UberMax;

end;
//------------------------------------------------------------------------------









//------------------------------------------------------------------------------
procedure Map3DTo2D(var X2D, Y2D:real; d, X3D, Y3D, Z3D:real);
begin
  X2D := (d/Z3d) * X3D;
  Y2D := (d/Z3d) * Y3D;
end;
//------------------------------------------------------------------------------











//------------------------------------------------------------------------------

function SimilarTriangle(Opposite0, Adjacent0, Adjacent1:real):real;
begin

  Result := (Opposite0 / Adjacent0) * Adjacent1;

end;

//------------------------------------------------------------------------------




//------------------------------------------------------------------------------
function ExtentAtD(Angle,Dist: real) : real;
begin

  result := Dist * tan(DegToRad(Angle));

end;
//------------------------------------------------------------------------------





//------------------------------------------------------------------------------
procedure LongToLat(var Lats: Txy; Longs: Txy);
var
  X,Y:real;
  HypZX, HypZY  : real;
  Hyp:extended;

begin

X := tan(Longs.x);
Y := tan(Longs.y);
 {
Hyp := sqrt( 1 +(X*X) + (Y*Y) );
//Hyp := power(1,2.0);


Lats.y := arcsin(Y /  Hyp);
Lats.x := arcsin(X /  Hyp);
 }

HypZX := sqrt( 1 + power(X,2) );
HypZY := sqrt( 1 + power(Y,2) );

Lats.y := arctan(Y/HypZX);
Lats.x := arctan(X/HypZY);

end;


//------------------------------------------------------------------------------











//------------------------------------------------------------------------------
// Normalise a vector

procedure Normalise(var p:Txyz);
var
  length: real;
begin

  length := p.x * p.x + p.y * p.y + p.z * p.z;
  if (length > 0) then
  begin
    length := sqrt(length);
    p.x := p.x / length;
    p.y := p.y / length;
    p.z := p.z / length;
   end
   else
   begin
		p.x := 0;
		p.y := 0;
		p.z := 0;
	end;
end;

//------------------------------------------------------------------------------









//------------------------------------------------------------------------------
{
Calculate the unit normal at p given two other points
	p1,p2 on the surface. The normal points in the direction
	of p1 crossproduct p2
  }

procedure CalcNormal(p,p1,p2:Txyz; var n:Txyz);
var
	pa,pb:Txyz;
begin

  pa.x := p1.x - p.x;
  pa.y := p1.y - p.y;
  pa.z := p1.z - p.z;

	pb.x := p2.x - p.x;
	pb.y := p2.y - p.y;
	pb.z := p2.z - p.z;

  n.x := pa.y * pb.z - pa.z * pb.y;
  n.y := pa.z * pb.x - pa.x * pb.z;
  n.z := pa.x * pb.y - pa.y * pb.x;
 	Normalise(n);

end;
//------------------------------------------------------------------------------








//------------------------------------------------------------------------------
function CrossProduct(p1,p2: Txyz): Txyz;

var
  p : Txyz;

begin

  p.x := p1.y * p2.z - p1.z * p2.y;
  p.y := p1.z * p2.x - p1.x * p2.z;
  p.z := p1.x * p2.y - p1.y * p2.x;

  Result := p;
end;
//------------------------------------------------------------------------------






//------------------------------------------------------------------------------

function DotProduct(p1,p2: Txyz): real;
begin


   Result := (p1.x*p2.x + p1.y*p2.y + p1.z*p2.z);
end;
//------------------------------------------------------------------------------






//---------------------------------------------------------------------------
// Calculate the length of a vector
function Modulus(p: Txyz):real;
begin
    Result := (sqrt(p.x * p.x + p.y * p.y + p.z * p.z));
end;
//-------------------------------------------------------------------------






//-------------------------------------------------------------------------
// Return the angle in radians between two vectors (0..pi)

function VectorAngle(p1,p2: Txyz): real;
var
  m1,m2 : real;
  costheta : real;

begin
  m1 := Modulus(p1);
  m2 := Modulus(p2);

	if (m1*m2 <= 0.01) then
  begin
		Result := 0;
  end
	else
  begin
		costheta := (p1.x*p2.x + p1.y*p2.y + p1.z*p2.z) / (m1*m2);
  end;

	if (costheta <= -1) then
  begin
		Result := pi;
  end
	else if (costheta >= 1) then
		Result := 0
	else
		Result := arccos(costheta);
end;

//-------------------------------------------------------------------------






//------------------------------------------------------------------------------
//	Rotate a point p by angle theta around an arbitrary normal r
//	Return the rotated point.
//	Positive angles are anticlockwise looking down the axis
//	towards the origin.
//   Assume right hand coordinate system.

function ArbitraryRotate(p: Txyz; theta: real; r: Txyz): Txyz;
var
  q : Txyz;
  costheta,sintheta : real;

begin
	q.x := 0;
  q.y := 0;
  q.z := 0;


	Normalise(r);
	costheta := cos(theta);
	sintheta := sin(theta);

	q.x := q.x + (costheta + (1 - costheta) * r.x * r.x) * p.x;
	q.x := q.x + ((1 - costheta) * r.x * r.y - r.z * sintheta) * p.y;
	q.x := q.x + ((1 - costheta) * r.x * r.z + r.y * sintheta) * p.z;

	q.y := q.y + ((1 - costheta) * r.x * r.y + r.z * sintheta) * p.x;
	q.y := q.y + (costheta + (1 - costheta) * r.y * r.y) * p.y;
	q.y := q.y + ((1 - costheta) * r.y * r.z - r.x * sintheta) * p.z;

	q.z := q.z + ((1 - costheta) * r.x * r.z - r.y * sintheta) * p.x;
	q.z := q.z + ((1 - costheta) * r.y * r.z + r.x * sintheta) * p.y;
	q.z := q.z + (costheta + (1 - costheta) * r.z * r.z) * p.z;

	result := q;
end;

//------------------------------------------------------------------------------






//------------------------------------------------------------------------------
//Subtract two vectors p = p2 - p1

function VectorSub(p1,p2: Txyz): Txyz;
var
  p : Txyz;

begin

  p.x := p2.x - p1.x;
  p.y := p2.y - p1.y;
  p.z := p2.z - p1.z;

  Result := p;
  
end;
//------------------------------------------------------------------------------


 {

 //------------------------------------------------------------------------------
procedure gaussian(var y,x:array of real; mean,sd:real);
var                                                                    //
  c:integer;

begin
  for c:=0 to sizeof(y)-1 do
  begin
    y[c]:=exp(          -(power(x[c]-mean,2) / power(2*sd,2))            );
  end;
end;
//------------------------------------------------------------------------------
 }



//------------------------------------------------------------------------------
function gaussian(x:real; mean,sd:real):real;
begin

    result:=exp(          -(power(x-mean,2) / power(2*sd,2))            );

end;
//------------------------------------------------------------------------------




//------------------------------------------------------------------------------
function DegToRad(Deg: real): real;
begin
   Result := Deg * (pi/180);
end;
//------------------------------------------------------------------------------




//------------------------------------------------------------------------------
function RadToDeg(Rad: real): real;
begin
   Result := Rad * (180/pi);
end;
//------------------------------------------------------------------------------


 {
//------------------------------------------------------------------------------
procedure SaveBitmap(Filename:pchar);

var
 temp: PSDL_surface;




begin



  glReadBuffer( GL_BACK);

  SDL_FreeSurface(temp);

  temp:=SDL_CreateRGBSurface(SDL_SWSURFACE,1024,768,24,0,0,0,0);

  zeromemory(temp.pixels,1024*768*3);


  glReadPixels(0,0,1024,768,GL_BGR,GL_UNSIGNED_BYTE,temp.pixels);


  SDL_SaveBMP(temp,Filename);

 // showmessage('BMP of left image saved (upside-down)');
end;
//------------------------------------------------------------------------------
         }


//------------------------------------------------------------------------------
function mean( data: array of real): extended;
var
  c:integer;
  val: extended;

begin
  val:=0;
  for c:=0 to length(data)-1 do
  begin
    val:=val+data[c];
  end;
  result:=(val/length(data));

end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
function std( data: array of real): extended;
var
  c:integer;
  val: extended;
  xbar: extended;
begin

  xbar := mean(data);
  val:=0;
  for c:=0 to length(data)-1 do
  begin

  val:=val + power((data[c] - Xbar),2);
  end;
  result:=(sqrt(val/(length(data)-1)));

end;
//------------------------------------------------------------------------------

procedure appendToArray(var data: array of real; x: real);
begin
  //setlength(data, length(data)+1);
  data[length(data)-1] := x;
end;

end.
