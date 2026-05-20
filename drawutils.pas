unit drawutils;

interface

uses
  Math, gl,glu;

var
  //display lists
  DL_CIRCLE, DL_HEX, DL_DIAMOND, DL_CIRCLE_BACKGROUND, DL_CIRCLE_OUTLINE,DL_TRIANGLE, DL_BOX, DL_RING, DL_CROSS,
  DL_BAR_HORIZ, DL_BAR_VERT, DL_SQUARE_EA, DL_CIRCLE_EA, DL_STAR, DL_PHOTODIODE_PATCH_LEFT, DL_PHOTODIODE_PATCH_RIGHT, DL_PHOTODIODE_PATCH_CENTRE: GLUint;


// drawing N placeholders starting with NE placeholder
procedure drawPlaceholders(targetRadiusCM: real; count: integer);

// drawing 4 placeholders at NE, NW, SE, SW placeholder
procedure drawPlaceholders4(targetRadiusCM: real);

// N must be 0 to count-1.
procedure objectLocationNofCount(out x, y: real; cueRadiusCM: real; N, Count: integer);

procedure objectLocation(var x, y:real ; cueRadiusCM: real; cueQuadrant: integer);

procedure drawShape(shapeNo:integer);

implementation

procedure drawPlaceholders4(targetRadiusCM: real);
var
x,y: real;
begin
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

procedure drawPlaceholders(
   targetRadiusCM       : real;
   count: integer);
var
  i : integer;
  x,y: real;
begin
  if count = 4 then begin
    drawPlaceholders4(targetRadiusCm);
    Exit;
  end;

  glMatrixMode(GL_MODELVIEW);
  for i := 0 to count-1 do begin
    glLoadIdentity;
    objectLocationNofCount(x,y, targetRadiusCM, i, count);
    gltranslatef(x,y,0);
    glCallList(DL_CIRCLE_OUTLINE);
  end;

end;

procedure objectLocationNofCount(out x, y: real; cueRadiusCM: real; N, Count: integer);
var
  d  : double;
begin
  N := Min(Max(0, N), Count-1);
  d := pi * 2 / Count;
  x := -cueRadiusCM*cos(pi/4 + d * N);
  y :=  cueRadiusCM*sin(pi/4 + d * N);
end;

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

end.
