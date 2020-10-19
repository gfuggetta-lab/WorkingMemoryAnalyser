unit IOR_Shapes;

{$MODE Delphi}

interface

uses
  Useful,math,gl;

type
  T2DarrayOfTxyz = array of array of Txyz;

  procedure ShapeGrid(XCoords,Ycoords:T2Darray; ZCoord:real; NCols,NRows:integer; jitter:real;  OuterRadius, LineWidth: real; Npoints:integer; Colour:array of real; Seed: longint);
  procedure Circle(XCoord,Ycoord,ZCoord, OuterRadius,LineWidth:real; Npoints:integer;  Colour:array of real; filled:boolean); overload ;
  procedure Circle(XCoord,Ycoord,ZCoord, OuterRadius,LineWidth:real; Npoints:integer; filled:boolean); overload ;
  procedure bar(XCoord,Ycoord,ZCoord, length, width:real;  theta:real);
  procedure Square(XCoord,Ycoord,ZCoord,OuterRadius, Linewidth:real; Colour:array of real; filled:boolean; theta:real);
  procedure Triangle(XCoord,Ycoord,ZCoord,OuterRadius, LineWidth:real; Colour:array of real; filled:boolean; theta:real);  overload   ;
  procedure Triangle(XCoord,Ycoord,ZCoord,OuterRadius, LineWidth:real; filled:boolean; theta:real); overload  ;
  procedure Cross(XCoord,Ycoord,ZCoord,Radius, Linewidth:real; Colour:array of real;  theta:real); overload  ;
  procedure Cross(XCoord,Ycoord,ZCoord,Radius, Linewidth:real;  theta:real); overload   ;
  procedure RandomTriangleGrid( XCoords,Ycoords:T2Darray; ZCoord:real; NCols,NRows:integer; jitter:real;  OuterRadius: real; Colour:array of real; Seed: longint);
  procedure OutlinePrunedRandomTriangleGrid(Distance,Xrot,Yrot:real; XCoords,Ycoords:T2Darray; ZCoord:real; NCols,NRows:integer; jitter:real;  OuterRadius: real; Colour:array of real; Seed: longint);
  procedure pentagram(XCoord,Ycoord,ZCoord, OuterRadius:real);


implementation


//------------------------------------------------------------------------------
procedure Circle(XCoord,Ycoord,ZCoord, OuterRadius,LineWidth:real; Npoints:integer;  Colour:array of real; filled:boolean);
// coords are cm, Size is degrees
var

  Xouter,Youter,Xinner,Yinner:array of real;
  c:integer;
  InnerRadius:real;

begin
  InnerRadius:=OuterRadius-LineWidth;

  SetLength(Xouter,Npoints);
  SetLength(Youter,Npoints);
  SetLength(Xinner,Npoints);
  SetLength(Yinner,Npoints);

  GetCircleCoords(Xouter, Youter, OuterRadius, Npoints);
  GetCircleCoords(Xinner, Yinner, InnerRadius, Npoints);

  glColor4f(Colour[0],Colour[1],Colour[2],Colour[3]);

  if filled then
  begin
    glBegin(GL_triangle_strip);
      for c:=0 to Npoints-1 do
      begin
        glVertex3f(XCoord+Xouter[c],YCoord+Youter[c],ZCoord);
        glVertex3f(XCoord,YCoord,ZCoord);
      end;
        glVertex3f(XCoord+Xouter[Npoints-1],YCoord+Youter[Npoints-1],ZCoord);
        glVertex3f(XCoord,YCoord,ZCoord);
        glVertex3f(XCoord+Xouter[0],YCoord+Youter[0],ZCoord);
    glEnd;

  end
  else
  begin
  glBegin(GL_triangle_strip);
    for c:=0 to Npoints-1 do
    begin
      glVertex3f(XCoord+Xouter[c],YCoord+Youter[c],ZCoord);
      glVertex3f(XCoord+Xinner[c],YCoord+Yinner[c],ZCoord);
    end;
    glVertex3f(XCoord+Xouter[0],YCoord+Youter[0],ZCoord);
    glVertex3f(XCoord+Xinner[0],YCoord+Yinner[0],ZCoord);
  glEnd;
  end;

end;
//------------------------------------------------------------------------------



//------------------------------------------------------------------------------
procedure pentagram(XCoord,Ycoord,ZCoord, OuterRadius:real);
// coords are cm, Size is degrees
var

  Xouter,Youter:array of real;
  c:integer;


begin


  SetLength(Xouter,5);
  SetLength(Youter,5);


  GetCircleCoords(Xouter, Youter, OuterRadius, 5);


  //glColor4f(Colour[0],Colour[1],Colour[2],Colour[3]);


  glBegin(GL_polygon);
    glVertex3f(XCoord,YCoord,ZCoord);
    glVertex3f(XCoord+Xouter[2],YCoord+Youter[2],ZCoord);
    glVertex3f(XCoord+Xouter[0],YCoord+Youter[0],ZCoord);
    glVertex3f(XCoord,YCoord,ZCoord);
  glEnd;


  glBegin(GL_polygon);
    glVertex3f(XCoord,YCoord,ZCoord);
    glVertex3f(XCoord+Xouter[0],YCoord+Youter[0],ZCoord);
    glVertex3f(XCoord+Xouter[3],YCoord+Youter[3],ZCoord);
    glVertex3f(XCoord,YCoord,ZCoord);
  glEnd;

  glBegin(GL_polygon);
    glVertex3f(XCoord,YCoord,ZCoord);
    glVertex3f(XCoord+Xouter[1],YCoord+Youter[1],ZCoord);
    glVertex3f(XCoord+Xouter[4],YCoord+Youter[4],ZCoord);
    glVertex3f(XCoord,YCoord,ZCoord);
  glEnd;

  glBegin(GL_polygon);
    glVertex3f(XCoord,YCoord,ZCoord);
    glVertex3f(XCoord+Xouter[4],YCoord+Youter[4],ZCoord);
    glVertex3f(XCoord+Xouter[2],YCoord+Youter[2],ZCoord);
    glVertex3f(XCoord,YCoord,ZCoord);
  glEnd;

  glBegin(GL_polygon);
    glVertex3f(XCoord,YCoord,ZCoord);
    glVertex3f(XCoord+Xouter[3],YCoord+Youter[3],ZCoord);
    glVertex3f(XCoord+Xouter[1],YCoord+Youter[1],ZCoord);
    glVertex3f(XCoord,YCoord,ZCoord);
  glEnd;


end;
//------------------------------------------------------------------------------



//------------------------------------------------------------------------------
procedure Circle(XCoord,Ycoord,ZCoord, OuterRadius,LineWidth:real; Npoints:integer; filled:boolean);
// coords are cm, Size is degrees
var

  Xouter,Youter,Xinner,Yinner:array of real;
  c:integer;
  InnerRadius:real;

begin
  InnerRadius:=OuterRadius-LineWidth;

  SetLength(Xouter,Npoints);
  SetLength(Youter,Npoints);
  SetLength(Xinner,Npoints);
  SetLength(Yinner,Npoints);

  GetCircleCoords(Xouter, Youter, OuterRadius, Npoints);
  GetCircleCoords(Xinner, Yinner, InnerRadius, Npoints);



  if filled then
  begin
    glBegin(GL_triangle_strip);
      for c:=0 to Npoints-1 do
      begin
        glVertex3f(XCoord+Xouter[c],YCoord+Youter[c],ZCoord);
        glVertex3f(XCoord,YCoord,ZCoord);
      end;
        glVertex3f(XCoord+Xouter[Npoints-1],YCoord+Youter[Npoints-1],ZCoord);
        glVertex3f(XCoord,YCoord,ZCoord);
        glVertex3f(XCoord+Xouter[0],YCoord+Youter[0],ZCoord);
    glEnd;

  end
  else
  begin
  glBegin(GL_triangle_strip);
    for c:=0 to Npoints-1 do
    begin
      glVertex3f(XCoord+Xouter[c],YCoord+Youter[c],ZCoord);
      glVertex3f(XCoord+Xinner[c],YCoord+Yinner[c],ZCoord);
    end;
    glVertex3f(XCoord+Xouter[0],YCoord+Youter[0],ZCoord);
    glVertex3f(XCoord+Xinner[0],YCoord+Yinner[0],ZCoord);
  glEnd;
  end;

end;
//------------------------------------------------------------------------------

 //------------------------------------------------------------------------------
procedure Square(XCoord,Ycoord,ZCoord,OuterRadius, Linewidth:real; Colour:array of real; filled:boolean; theta:real);
// coords are cm, Size is degrees
var

  Xouter,Youter,Xinner,Yinner:array of real;
  OutSz, InSz:real;
  p1,p2,p3,p4, p5, p6, p7, p8:Txyz;
  Xo,Yo, Xi, Yi  :array [0..3] of real;
  InnerRadius:real;
begin

  getcirclecoords(Xo,Yo,OuterRadius,4);

  InnerRadius:=  Outerradius - (LineWidth / (sin((pi/4)) ));
  getcirclecoords(Xi,Yi,InnerRadius,4);



  p1.x:=Xo[0];
  p1.y:=Yo[0];
  p1.z:=0;

  p2.x:=Xo[1];
  p2.y:=Yo[1];
  p2.z:=0;

  p3.x:=Xo[2];
  p3.y:=Yo[2];
  p3.z:=0;

  p4.x:=Xo[3];
  p4.y:=Yo[3];
  p4.z:=0;


  p5.x:=Xi[0];
  p5.y:=Yi[0];
  p5.z:=0;

  p6.x:=Xi[1];
  p6.y:=Yi[1];
  p6.z:=0;

  p7.x:=Xi[2];
  p7.y:=Yi[2];
  p7.z:=0;

  p8.x:=Xi[3];
  p8.y:=Yi[3];
  p8.z:=0;


  RotateXYZ(p1,theta,'z');
  RotateXYZ(p2,theta,'z');
  RotateXYZ(p3,theta,'z');
  RotateXYZ(p4,theta,'z');

  RotateXYZ(p5,theta,'z');
  RotateXYZ(p6,theta,'z');
  RotateXYZ(p7,theta,'z');
  RotateXYZ(p8,theta,'z');


  glColor4f(Colour[0],Colour[1],Colour[2],Colour[3]);

  if filled then
  begin
     glBegin(gl_polygon);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
    glEnd;
  end
  else
  begin
    glBegin(gl_polygon);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p5.x,Ycoord+p5.y,Zcoord+p5.z);
      glVertex3f(Xcoord+p6.x,Ycoord+p6.y,Zcoord+p6.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
    glEnd;

    glBegin(gl_polygon);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p6.x,Ycoord+p6.y,Zcoord+p6.z);
      glVertex3f(Xcoord+p7.x,Ycoord+p7.y,Zcoord+p7.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
    glEnd;

    glBegin(gl_polygon);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p7.x,Ycoord+p7.y,Zcoord+p7.z);
      glVertex3f(Xcoord+p8.x,Ycoord+p8.y,Zcoord+p8.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
    glEnd;

    glBegin(gl_polygon);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
      glVertex3f(Xcoord+p8.x,Ycoord+p8.y,Zcoord+p8.z);
      glVertex3f(Xcoord+p5.x,Ycoord+p5.y,Zcoord+p5.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
    glEnd;

  end;

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure Triangle(XCoord,Ycoord,ZCoord,OuterRadius, LineWidth:real; Colour:array of real; filled:boolean; theta:real);
// coords are cm, Size is degrees
var

  Xouter,Youter,Xinner,Yinner:array of real;
  OutSz, InSz:real;
  p1,p2,p3,p4, p5, p6, p7, p8:Txyz;

  Xo,Yo, Xi, Yi  :array [0..2] of real;
  InnerRadius:real;


begin

  getcirclecoords(Xo,Yo,OuterRadius,3);


  InnerRadius:=  Outerradius - (LineWidth / (sin((pi/6)) ));
  getcirclecoords(Xi,Yi,InnerRadius,3);


  p1.x:=Xo[0];
  p1.y:=Yo[0];
  p1.z:=0;

  p2.x:=Xo[1];
  p2.y:=Yo[1];
  p2.z:=0;

  p3.x:=Xo[2];
  p3.y:=Yo[2];
  p3.z:=0;


  p4.x:=Xi[0];
  p4.y:=Yi[0];
  p4.z:=0;

  p5.x:=Xi[1];
  p5.y:=Yi[1];
  p5.z:=0;

  p6.x:=Xi[2];
  p6.y:=Yi[2];
  p6.z:=0;



  RotateXYZ(p1,theta,'z');
  RotateXYZ(p2,theta,'z');
  RotateXYZ(p3,theta,'z');

  RotateXYZ(p4,theta,'z');
  RotateXYZ(p5,theta,'z');
  RotateXYZ(p6,theta,'z');



  glColor4f(Colour[0],Colour[1],Colour[2],Colour[3]);

  if filled then
  begin
     glBegin(gl_polygon);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
    glEnd;
  end
  else
  begin
    glBegin(gl_polygon);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p6.x,Ycoord+p6.y,Zcoord+p6.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
    glEnd;

    glBegin(gl_polygon);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p5.x,Ycoord+p5.y,Zcoord+p5.z);
      glVertex3f(Xcoord+p6.x,Ycoord+p6.y,Zcoord+p6.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
    glEnd;

    glBegin(gl_polygon);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
      glVertex3f(Xcoord+p5.x,Ycoord+p5.y,Zcoord+p5.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
    glEnd;


  end;

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure Triangle(XCoord,Ycoord,ZCoord,OuterRadius, LineWidth:real; filled:boolean; theta:real);
// coords are cm, Size is degrees
var

  Xouter,Youter,Xinner,Yinner:array of real;
  OutSz, InSz:real;
  p1,p2,p3,p4, p5, p6, p7, p8:Txyz;

  Xo,Yo, Xi, Yi  :array [0..2] of real;
  InnerRadius:real;


begin

  getcirclecoords(Xo,Yo,OuterRadius,3);


  InnerRadius:=  Outerradius - (LineWidth / (sin((pi/6)) ));
  getcirclecoords(Xi,Yi,InnerRadius,3);


  p1.x:=Xo[0];
  p1.y:=Yo[0];
  p1.z:=0;

  p2.x:=Xo[1];
  p2.y:=Yo[1];
  p2.z:=0;

  p3.x:=Xo[2];
  p3.y:=Yo[2];
  p3.z:=0;


  p4.x:=Xi[0];
  p4.y:=Yi[0];
  p4.z:=0;

  p5.x:=Xi[1];
  p5.y:=Yi[1];
  p5.z:=0;

  p6.x:=Xi[2];
  p6.y:=Yi[2];
  p6.z:=0;



  RotateXYZ(p1,theta,'z');
  RotateXYZ(p2,theta,'z');
  RotateXYZ(p3,theta,'z');

  RotateXYZ(p4,theta,'z');
  RotateXYZ(p5,theta,'z');
  RotateXYZ(p6,theta,'z');



  //glColor4f(Colour[0],Colour[1],Colour[2],Colour[3]);

  if filled then
  begin
     glBegin(gl_polygon);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
    glEnd;
  end
  else
  begin
    glBegin(gl_polygon);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p6.x,Ycoord+p6.y,Zcoord+p6.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
    glEnd;

    glBegin(gl_polygon);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p5.x,Ycoord+p5.y,Zcoord+p5.z);
      glVertex3f(Xcoord+p6.x,Ycoord+p6.y,Zcoord+p6.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
    glEnd;

    glBegin(gl_polygon);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
      glVertex3f(Xcoord+p5.x,Ycoord+p5.y,Zcoord+p5.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
    glEnd;


  end;

end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure Cross(XCoord,Ycoord,ZCoord,Radius, Linewidth:real; Colour:array of real;  theta:real);
// coords are cm, Size is degrees
var
  Xouter,Youter,Xinner,Yinner:array of real;
  OutSz, InSz:real;
  p1,p2,p3,p4, p5, p6, p7, p8:Txyz;
  w:real;
begin

  w:=linewidth/2;

  p1.x:=-(Radius);
  p1.y:=w;
  p1.z:=0;

  p2.x:=-(Radius);
  p2.y:=-w;
  p2.z:=0;

  p3.x:=Radius;
  p3.y:=-w;
  p3.z:=0;

  p4.x:=Radius;
  p4.y:=w;
  p4.z:=0;


  p5.x:=-w;
  p5.y:=-(Radius);
  p5.z:=0;

  p6.x:=w;
  p6.y:=-(Radius);
  p6.z:=0;

  p7.x:=w;
  p7.y:=(Radius);
  p7.z:=0;

  p8.x:=-w;
  p8.y:=(Radius);
  p8.z:=0;

  RotateXYZ(p1,theta,'z');
  RotateXYZ(p2,theta,'z');
  RotateXYZ(p3,theta,'z');
  RotateXYZ(p4,theta,'z');
  RotateXYZ(p5,theta,'z');
  RotateXYZ(p6,theta,'z');
  RotateXYZ(p7,theta,'z');
  RotateXYZ(p8,theta,'z');


  glColor4f(Colour[0],Colour[1],Colour[2],Colour[3]);

     glBegin(gl_polygon);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
    glEnd;

    glBegin(gl_polygon);
      glVertex3f(Xcoord+p5.x,Ycoord+p5.y,Zcoord+p5.z);
      glVertex3f(Xcoord+p6.x,Ycoord+p6.y,Zcoord+p6.z);
      glVertex3f(Xcoord+p7.x,Ycoord+p7.y,Zcoord+p7.z);
      glVertex3f(Xcoord+p8.x,Ycoord+p8.y,Zcoord+p8.z);
    glEnd;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure Cross(XCoord,Ycoord,ZCoord,Radius, Linewidth:real;  theta:real);
// coords are cm, Size is degrees
var

  Xouter,Youter,Xinner,Yinner:array of real;
  OutSz, InSz:real;
  p1,p2,p3,p4, p5, p6, p7, p8:Txyz;


  w:real;


begin

  w:=linewidth/2;



  p1.x:=-(Radius);
  p1.y:=w;
  p1.z:=0;

  p2.x:=-(Radius);
  p2.y:=-w;
  p2.z:=0;

  p3.x:=Radius;
  p3.y:=-w;
  p3.z:=0;

  p4.x:=Radius;
  p4.y:=w;
  p4.z:=0;


  p5.x:=-w;
  p5.y:=-(Radius);
  p5.z:=0;

  p6.x:=w;
  p6.y:=-(Radius);
  p6.z:=0;

  p7.x:=w;
  p7.y:=(Radius);
  p7.z:=0;

  p8.x:=-w;
  p8.y:=(Radius);
  p8.z:=0;

  RotateXYZ(p1,theta,'z');
  RotateXYZ(p2,theta,'z');
  RotateXYZ(p3,theta,'z');
  RotateXYZ(p4,theta,'z');
  RotateXYZ(p5,theta,'z');
  RotateXYZ(p6,theta,'z');
  RotateXYZ(p7,theta,'z');
  RotateXYZ(p8,theta,'z');




     glBegin(gl_polygon);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
    glEnd;

    glBegin(gl_polygon);
      glVertex3f(Xcoord+p5.x,Ycoord+p5.y,Zcoord+p5.z);
      glVertex3f(Xcoord+p6.x,Ycoord+p6.y,Zcoord+p6.z);
      glVertex3f(Xcoord+p7.x,Ycoord+p7.y,Zcoord+p7.z);
      glVertex3f(Xcoord+p8.x,Ycoord+p8.y,Zcoord+p8.z);
    glEnd;



end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure blob(XCoord,Ycoord,ZCoord,Radius, Linewidth:real; Colour:array of real);
//unfinished gaussian distorted circles. see matlab blob.m
var

  Xouter,Youter,Xinner,Yinner:array of real;
  c:integer;
  InnerRadius:real;



  Npoints:integer;

  xmin,xmax:real;
  proximity:real;

  y,xpos:real;
begin

  xmin:=-10;
  xmax:=10;

  y:=gaussian(1,0,1);

  Npoints:=20;



  InnerRadius:=Radius-LineWidth;

  SetLength(Xouter,Npoints);
  SetLength(Youter,Npoints);
  SetLength(Xinner,Npoints);
  SetLength(Yinner,Npoints);

  GetCircleCoords(Xouter, Youter, Radius, Npoints);






  glColor4f(Colour[0],Colour[1],Colour[2],Colour[3]);



    glBegin(gl_polygon);
      for c:=0 to Npoints-2 do
      begin
      Xouter[c]:=Xouter[c]+(random*0.2);
      glVertex3f(XCoord+Xouter[c+1],YCoord+Youter[c+1],ZCoord);
        glVertex3f(XCoord+Xouter[c],YCoord+Youter[c],ZCoord);

      end;
       glVertex3f(XCoord+Xouter[0],YCoord+Youter[0],ZCoord);
       glVertex3f(XCoord+Xouter[Npoints-1],YCoord+Youter[Npoints-1],ZCoord);


    glEnd;



 // proximity:


end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure bar(XCoord,Ycoord,ZCoord, length, width:real;  theta:real);
// coords are cm, Size is degrees
var
  p1,p2,p3,p4: Txyz;
  w:real;
begin
  w:=width/2;

  p1.x:=-(length/2);
  p1.y:=w;
  p1.z:=0;

  p2.x:=-(length/2);
  p2.y:=-w;
  p2.z:=0;

  p3.x:=length/2;
  p3.y:=-w;
  p3.z:=0;

  p4.x:=length/2;
  p4.y:=w;
  p4.z:=0;

  RotateXYZ(p1,theta,'z');
  RotateXYZ(p2,theta,'z');
  RotateXYZ(p3,theta,'z');
  RotateXYZ(p4,theta,'z');
  //glColor4f(Colour[0],Colour[1],Colour[2],Colour[3]);

  glBegin(gl_polygon);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
  glEnd;
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure RandomTriangle(XCoord,Ycoord,ZCoord,OuterRadius:real; Colour:array of real; theta:real);
// coords are cm, Size is degrees
var

  Xouter,Youter,Xinner,Yinner:array of real;
  OutSz, InSz:real;
  p1,p2,p3,p4, p5, p6, p7, p8:Txyz;

  Xo,Yo, Xi, Yi  :array [0..2] of real;
  InnerRadius:real;

  c: integer;
theta1, theta2, radius:real;
  sc:real;
begin

  sc:=(random-0.5)*Outerradius;
  Radius:=OuterRadius+sc;
  Xo[0]:=Radius*sin(0);
  Yo[0]:=Radius*cos(0);

  //Radius:=OuterRadius+(sc*(random-0.5)*Outerradius);
  theta1:=((2*pi)*(1/3))+((random-0.5)*((2*pi)*(1/3)));
  Xo[1]:=Radius*sin(theta1);
  Yo[1]:=Radius*cos(theta1);

  // Radius:=OuterRadius+(sc*(random-0.5)*Outerradius);
  theta2:=((2*pi)*(2/3))+((random-0.5)*((2*pi)*(1/3)));
  Xo[2]:=Radius*sin(theta2);
  Yo[2]:=Radius*cos(theta2);




  //getcirclecoords(Xo,Yo,OuterRadius,3);



  getcirclecoords(Xi,Yi,InnerRadius,3);


  p1.x:=Xo[0];
  p1.y:=Yo[0];
  p1.z:=0;

  p2.x:=Xo[1];
  p2.y:=Yo[1];
  p2.z:=0;

  p3.x:=Xo[2];
  p3.y:=Yo[2];
  p3.z:=0;


  p4.x:=Xi[0];
  p4.y:=Yi[0];
  p4.z:=0;

  p5.x:=Xi[1];
  p5.y:=Yi[1];
  p5.z:=0;

  p6.x:=Xi[2];
  p6.y:=Yi[2];
  p6.z:=0;



  RotateXYZ(p1,theta,'z');
  RotateXYZ(p2,theta,'z');
  RotateXYZ(p3,theta,'z');

  RotateXYZ(p4,theta,'z');
  RotateXYZ(p5,theta,'z');
  RotateXYZ(p6,theta,'z');



  glColor4f(Colour[0],Colour[1],Colour[2],Colour[3]);

  //if filled then
 // begin
     glBegin(gl_polygon);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
    glEnd;
 { end
  else
  begin
    glBegin(gl_polygon);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p6.x,Ycoord+p6.y,Zcoord+p6.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
    glEnd;

    glBegin(gl_polygon);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p5.x,Ycoord+p5.y,Zcoord+p5.z);
      glVertex3f(Xcoord+p6.x,Ycoord+p6.y,Zcoord+p6.z);
      glVertex3f(Xcoord+p3.x,Ycoord+p3.y,Zcoord+p3.z);
    glEnd;

    glBegin(gl_polygon);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
      glVertex3f(Xcoord+p1.x,Ycoord+p1.y,Zcoord+p1.z);
      glVertex3f(Xcoord+p4.x,Ycoord+p4.y,Zcoord+p4.z);
      glVertex3f(Xcoord+p5.x,Ycoord+p5.y,Zcoord+p5.z);
      glVertex3f(Xcoord+p2.x,Ycoord+p2.y,Zcoord+p2.z);
    glEnd;


  end;
    }
end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure ShapeGrid(XCoords,Ycoords:T2Darray; ZCoord:real; NCols,NRows:integer; jitter:real;  OuterRadius, LineWidth: real; Npoints:integer; Colour:array of real; Seed: longint);
var
  GridX,GridY: integer;
//  Esy,Esx: real;
  jx,jy: real;


 maxrand:real;
  LastShapeType,ShapeType:integer;
  filled:boolean;
  r:real;

begin
  LastShapeType:=0;
  randseed:=seed;
  MaxRand:= jitter;

  // Draw a grid
  for GridY := 0 to NRows-1 do

  begin
      for GridX := 0 to NCols-1 do
    begin
       jx:=(random*MaxRand)-(MaxRand/2); //* (SCREEN_HEIGHT/SCREEN_WIDTH);
       jy:=(random*MaxRand)-(MaxRand/2);


       ShapeType:=round(random*3);
       If round(random)=1 then
       begin
        filled:=true;
       end
       else
       begin
        filled:=false;
       end;
       r:=OuterRadius+((random-0.5)*Outerradius);
       case ShapeType of

       0: Circle(XCoords[GridX,GridY]+jx,YCoords[GridX,GridY]+jy,ZCoord,r, LineWidth,Npoints, Colour, filled);
       1: square(XCoords[GridX,GridY]+jx,YCoords[GridX,GridY]+jy,ZCoord,r,LineWidth,Colour, filled, random*pi);
       2: triangle(XCoords[GridX,GridY]+jx,YCoords[GridX,GridY]+jy,ZCoord,r,LineWidth,Colour, filled, random*(pi*2));
       3: cross(XCoords[GridX,GridY]+jx,YCoords[GridX,GridY]+jy,ZCoord,r,LineWidth,Colour, random*pi);

       end;
    end;
  end;
 end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure RandomTriangleGrid(XCoords,Ycoords:T2Darray; ZCoord:real; NCols,NRows:integer; jitter:real;  OuterRadius: real; Colour:array of real; Seed: longint);
var
  GridX,GridY: integer;
//  Esy,Esx: real;
  jx,jy: real;

  X,Y:real;

 maxrand:real;
   DisplaysizeDeg :real;


begin
 DisplaysizeDeg:=100;
  randseed:=seed;
  MaxRand:= jitter;

  // Draw a grid
  for GridY := 0 to NRows-1 do

  begin
      for GridX := 0 to NCols-1 do
    begin
       jx:=(random*MaxRand)-(MaxRand/2);
       jy:=(random*MaxRand)-(MaxRand/2);

       X := XCoords[GridX,GridY]+jx;
       Y := YCoords[GridX,GridY]+jy;



       RandomTriangle(X,Y,ZCoord,OuterRadius,Colour, random*(pi*2));


    end;
  end;
 end;
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
procedure OutlinePrunedRandomTriangleGrid(Distance,Xrot,Yrot:real; XCoords,Ycoords:T2Darray; ZCoord:real; NCols,NRows:integer; jitter:real;  OuterRadius: real; Colour:array of real; Seed: longint);
var
  GridX,GridY: integer;
//  Esy,Esx: real;
  jx,jy: real;

  X,Y:real;

 maxrand:real;
   DisplaysizeDeg :real;

   X0,Y0,Z0:real;

begin
 DisplaysizeDeg:=1;
  randseed:=seed;
  MaxRand:= jitter;

  // Draw a grid
  for GridY := 0 to NRows-1 do

  begin
      for GridX := 0 to NCols-1 do
    begin
       jx:=(random*MaxRand)-(MaxRand/2);
       jy:=(random*MaxRand)-(MaxRand/2);

       X := XCoords[GridX,GridY]+jx;
       Y := YCoords[GridX,GridY]+jy;

       X0:=X;
       Y0:=Y;
       Z0:=0;

       RotateXYZ(X0,Y0,Z0,xrot,'x');
       RotateXYZ(X0,Y0,Z0,yrot,'y');

       //prune



       if sqrt(power(X0,2)+power(Y0,2))< 9 then
       begin
        RandomTriangle(X,Y,ZCoord,OuterRadius,Colour, random*(pi*2));
        end;

    end;
  end;
 end;
//------------------------------------------------------------------------------


end.
