unit GLTextureDistortion;

{$MODE Delphi}



interface

uses
  gl,glu,
  Dialogs,
  Useful;



  procedure InitGLTextureDistortion(var TexID: GLUint;TextureWidth,TextureHeight:integer; memoPointer:pointer);

  procedure DoGLTextureDistortionEdit(TexCoordGridLeftX,TexCoordGridLeftY,
            TexCoordGridRightX,TexCoordGridRightY,DisplayGridLeftX,
            DisplayGridLeftY,DisplayGridRightX,DisplayGridRightY: T2Darray;
            GridNcols,GridNrows,TextureWidth,
            TextureHeight: integer; TexID:GLUint);

  procedure DoGLTextureDistortionFixed(GridDisplayListID:GLUint; TextureWidth,
            TextureHeight: integer; TexID:GLUint);

  procedure DoGLTextureDistortionMakeGridDisplayList(TexCoordGridLeftX,TexCoordGridLeftY,
            TexCoordGridRightX,TexCoordGridRightY,DisplayGridLeftX,
            DisplayGridLeftY,DisplayGridRightX,DisplayGridRightY: T2Darray;
            GridNcols,GridNrows: integer);

implementation




//------------------------------------------------------------------------------
procedure InitGLTextureDistortion(var TexID: GLUint;TextureWidth,TextureHeight:integer; memoPointer:pointer);
var
  er: glenum;
  errstring: pchar;

begin

  // Get the maximum texture size supported by the hardware
  //glGetIntegerv(GL_MAX_TEXTURE_SIZE,@MaxTextureSize);

  // Do proxy texture test to see whether the hardware supports the desired texture size
  glTexImage2D(GL_PROXY_TEXTURE_2D,0,GL_RGB,TextureWidth,TextureHeight,0,GL_RGB,GL_UNSIGNED_BYTE,@NULL);



  // OpenGL error checking
  er:=glGetError;
  errstring:=gluErrorString(er);
  if (er <> GL_NO_ERROR) then
  begin
    ShowMessage('Proxy Texture test. OpenGL error: ' + errstring);
  end;



  // Select GL_TEXTURE_2D state. Texture objects initialised after this will
  // be defined as 2D texture objects.
  glEnable(GL_TEXTURE_2D);
  glGenTextures(1,@TexID);

  // create a new texture object, with ID TexID.
  glBindTexture(GL_TEXTURE_2D,TexID);

  glTexImage2D(GL_TEXTURE_2D,0,GL_RGB,TextureWidth,TextureHeight,0,GL_RGB,GL_UNSIGNED_BYTE,memoPointer);

  // The texture memory is successfully allocated if HardwareTextureWidth and
  // HardwareTextureHeight are nonzero
  //glGetTexLevelParameteriv(GL_TEXTURE_2D, 0,GL_TEXTURE_WIDTH, @HardwareTextureWidth);
  //glGetTexLevelParameteriv(GL_TEXTURE_2D, 0,GL_TEXTURE_HEIGHT, @HardwareTextureHeight);

  // Select how the texture image is combined with existing image
  glTexEnvf(GL_TEXTURE_ENV,GL_TEXTURE_ENV_MODE,GL_REPLACE);

  // Texture parameters
  glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);
  glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);

    er:=glGetError;
  errstring:=gluErrorString(er);
  if (er <> GL_NO_ERROR) then
  begin
    ShowMessage('Texture setup. OpenGL error: ' + errstring);
  end;



  glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_WRAP_S,GL_CLAMP);
  glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_WRAP_T,GL_CLAMP);

  //release the texture. ESSENTIAL OR ELSE OBJECTS ARE DRAWN IN BLACK REGARDLESS
  //OF WHETHER TEXTURE DISTORTION IS USED. THIS COST DAYS!!!!!!!!!!!!!!!!!!!!!!!

  // set the current texture object. we will select ZERO object (the default
  // no texture object). we will select the TexID texture at the point that we
  // want to use it.
  glBindTexture(GL_TEXTURE_2D,0);

  // OpenGL error checking
  er:=glGetError;
  errstring:=gluErrorString(er);
  if (er <> GL_NO_ERROR) then
  begin
    ShowMessage('Texture setup. OpenGL error: ' + errstring);
  end;


end;
//------------------------------------------------------------------------------













//------------------------------------------------------------------------------
procedure DoGLTextureDistortionEdit(TexCoordGridLeftX,TexCoordGridLeftY,
            TexCoordGridRightX,TexCoordGridRightY,DisplayGridLeftX,
            DisplayGridLeftY,DisplayGridRightX,DisplayGridRightY: T2Darray;
            GridNcols,GridNrows,TextureWidth,
            TextureHeight: integer; TexID:GLUint);
var
  GridX,GridY: integer;
  er: glenum;
  errstring: pchar;
  orientation:boolean;
begin


  // Copy back buffer into texture memory
  glBindTexture(GL_TEXTURE_2D,TexID);
  glReadBuffer(GL_BACK);
  glCopyTexSubImage2D(GL_TEXTURE_2D,0,0,0,0,0,TextureWidth,TextureHeight);


  // Clear the image in the back buffer
  glClearColor(0.0, 0.0, 0.0, 0.0);
  glClearDepth( 1.0 );
  glClear(GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT);



  orientation:=true;
  // Draw the texturemap as quads ONCE THE CALIBRATION IS FINALISED, THIS
  // CAN BE PUT IN A DISPLAY LIST
  for GridX := 0 to GridNcols-2 do
  begin
    for GridY := 0 to GridNrows-2 do
    begin

      glBegin(GL_QUADS);
        // top left
        glTexCoord2f(TexCoordGridLeftX[GridX,GridY],TexCoordGridLeftY[GridX,GridY]);
        glVertex2f(DisplayGridLeftX[GridX,GridY],DisplayGridLeftY[GridX,GridY]);

        // bottom left
        glTexCoord2f(TexCoordGridLeftX[GridX,GridY+1],TexCoordGridLeftY[GridX,GridY+1]);
        glVertex2f(DisplayGridLeftX[GridX,GridY+1],DisplayGridLeftY[GridX,GridY+1]);

        // bottom right
        glTexCoord2f(TexCoordGridLeftX[GridX+1,GridY+1],TexCoordGridLeftY[GridX+1,GridY+1]);
        glVertex2f(DisplayGridLeftX[GridX+1,GridY+1],DisplayGridLeftY[GridX+1,GridY+1]);

        // top right
        glTexCoord2f(TexCoordGridLeftX[GridX+1,GridY],TexCoordGridLeftY[GridX+1,GridY]);
        glVertex2f(DisplayGridLeftX[GridX+1,GridY],DisplayGridLeftY[GridX+1,GridY]);

      glEnd;


      glBegin(GL_QUADS);
        // top left
        glTexCoord2f(TexCoordGridRightX[GridX,GridY],TexCoordGridRightY[GridX,GridY]);
        glVertex2f(DisplayGridRightX[GridX,GridY],DisplayGridRightY[GridX,GridY]);

        // bottom left
        glTexCoord2f(TexCoordGridRightX[GridX,GridY+1],TexCoordGridRightY[GridX,GridY+1]);
        glVertex2f(DisplayGridRightX[GridX,GridY+1],DisplayGridRightY[GridX,GridY+1]);

        // bottom right
        glTexCoord2f(TexCoordGridRightX[GridX+1,GridY+1],TexCoordGridRightY[GridX+1,GridY+1]);
        glVertex2f(DisplayGridRightX[GridX+1,GridY+1],DisplayGridRightY[GridX+1,GridY+1]);

        //top right
        glTexCoord2f(TexCoordGridRightX[GridX+1,GridY],TexCoordGridRightY[GridX+1,GridY]);
        glVertex2f(DisplayGridRightX[GridX+1,GridY],DisplayGridRightY[GridX+1,GridY]);
      glEnd;

    end;

  end;


  // Realease the texture
  glBindTexture(GL_TEXTURE_2D,0);

  er:=glGetError;
  errstring:=gluErrorString(er);
  if (er <> GL_NO_ERROR) then
  begin
    ShowMessage('kaka Texture setup. OpenGL error: ' + errstring);
  end;
end;
//------------------------------------------------------------------------------






//------------------------------------------------------------------------------
procedure DoGLTextureDistortionMakeGridDisplayList(TexCoordGridLeftX,TexCoordGridLeftY,
            TexCoordGridRightX,TexCoordGridRightY,DisplayGridLeftX,
            DisplayGridLeftY,DisplayGridRightX,DisplayGridRightY: T2Darray;
            GridNcols,GridNrows: integer);

var
  GridX,GridY: integer;

  begin
// Draw the texturemap as quads ONCE THE CALIBRATION IS FINALISED, THIS
  // CAN BE PUT IN A DISPLAY LIST
  for GridX := 0 to GridNcols-2 do
  begin
    for GridY := 0 to GridNrows-2 do
    begin

      glBegin(GL_QUADS);
        // top left
        glTexCoord2f(TexCoordGridLeftX[GridX,GridY],TexCoordGridLeftY[GridX,GridY]);
        glVertex2f(DisplayGridLeftX[GridX,GridY],DisplayGridLeftY[GridX,GridY]);

        // bottom left
        glTexCoord2f(TexCoordGridLeftX[GridX,GridY+1],TexCoordGridLeftY[GridX,GridY+1]);
        glVertex2f(DisplayGridLeftX[GridX,GridY+1],DisplayGridLeftY[GridX,GridY+1]);

        // bottom right
        glTexCoord2f(TexCoordGridLeftX[GridX+1,GridY+1],TexCoordGridLeftY[GridX+1,GridY+1]);
        glVertex2f(DisplayGridLeftX[GridX+1,GridY+1],DisplayGridLeftY[GridX+1,GridY+1]);

        // top right
        glTexCoord2f(TexCoordGridLeftX[GridX+1,GridY],TexCoordGridLeftY[GridX+1,GridY]);
        glVertex2f(DisplayGridLeftX[GridX+1,GridY],DisplayGridLeftY[GridX+1,GridY]);

      glEnd;


      glBegin(GL_QUADS);
        // top left
        glTexCoord2f(TexCoordGridRightX[GridX,GridY],TexCoordGridRightY[GridX,GridY]);
        glVertex2f(DisplayGridRightX[GridX,GridY],DisplayGridRightY[GridX,GridY]);

        // bottom left
        glTexCoord2f(TexCoordGridRightX[GridX,GridY+1],TexCoordGridRightY[GridX,GridY+1]);
        glVertex2f(DisplayGridRightX[GridX,GridY+1],DisplayGridRightY[GridX,GridY+1]);

        // bottom right
        glTexCoord2f(TexCoordGridRightX[GridX+1,GridY+1],TexCoordGridRightY[GridX+1,GridY+1]);
        glVertex2f(DisplayGridRightX[GridX+1,GridY+1],DisplayGridRightY[GridX+1,GridY+1]);

        //top right
        glTexCoord2f(TexCoordGridRightX[GridX+1,GridY],TexCoordGridRightY[GridX+1,GridY]);
        glVertex2f(DisplayGridRightX[GridX+1,GridY],DisplayGridRightY[GridX+1,GridY]);
      glEnd;

    end;
  end;
end;
//------------------------------------------------------------------------------





//------------------------------------------------------------------------------
procedure DoGLTextureDistortionFixed(GridDisplayListID: GLUint; TextureWidth,
            TextureHeight: integer; TexID:GLUint);
var
  GridX,GridY: integer;
  er: glenum;
  errstring: pchar;

begin

  // Set the current to texture object to the one we created in InitGLTextureDistortion
  glBindTexture(GL_TEXTURE_2D,TexID);


   er:=glGetError;
  errstring:=gluErrorString(er);
  if (er <> GL_NO_ERROR) then
  begin
    ShowMessage('binding Texture setup. OpenGL error: ' + errstring);  // THIS IS THE PROBLEM
  end;


  // Copy from back buffer into texture memory
  glReadBuffer(GL_BACK);
  glCopyTexSubImage2D(GL_TEXTURE_2D,0,0,0,0,0,TextureWidth,TextureHeight);

  er:=glGetError;
  errstring:=gluErrorString(er);
  if (er <> GL_NO_ERROR) then
  begin
    ShowMessage('end Texture setup. OpenGL error: ' + errstring);
  end;


  // Clear the image in the back buffer
  glClearColor(0.0, 0.0, 0.0, 0.0);
  glClearDepth( 1.0 );
  glClear(GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT);

  glMatrixMode(GL_MODELVIEW);
  glLoadIdentity;


  gldisable(gl_lighting);

  // Draw the texturemap as quads
  glCallList(GridDisplayListID);

  er:=glGetError;
  errstring:=gluErrorString(er);
  if (er <> GL_NO_ERROR) then
  begin
    ShowMessage('end Texture setup. OpenGL error: ' + errstring);
  end;
  
  // Revert to the default ZERO texture object.
  glBindTexture(GL_TEXTURE_2D,0);

  er:=glGetError;
  errstring:=gluErrorString(er);
  if (er <> GL_NO_ERROR) then
  begin
    ShowMessage('end Texture setup. OpenGL error: ' + errstring);
  end;
end;
//------------------------------------------------------------------------------

end.
