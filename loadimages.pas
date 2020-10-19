unit loadImages;

{$mode delphi}

interface

uses
  Classes, SysUtils,SDL2,gl,glu, dialogs;

function  LoadGLTextures():integer;
procedure  LoadGLTexture(pathAndFilename: pchar; var textureID : GLUint; var widthPixels, heightPixels: integer);

type
  TBMPimages = record
     TextureImage :PSDL_Surface; // surface into which bmp is loaded
    textureID: GLUint;
    widthPixels : integer;
    heightPixels: integer;
  end;

  function selectBMPimage (BMPimages : array of TBMPimages; imageNo:integer): integer;

  function loadBMPimages (experiment_dir: string; var BMPimages : array of TBMPimages): integer;
  function displayBMPimage(var BMPimages: array of TBMPimages; imageNo:integer):integer;
  function displayBMPimageXYcm(var BMPimages: array of TBMPimages; imageNo:integer; xcm, ycm :real; screenWidthPix, screenHeightPix: integer; screenWidthCM,  screenHeightCM: real;
    scaleH: double = 1.0;
    scaleV: double = 1.0):integer;
  function displayBMPimageXYSizecm(var BMPimages: TBMPimages; xcm, ycm :real; screenWidthCM, screenHeightCM: real;
    widthCm, HeightCm: real):integer;

implementation

//------------------------------------------------------------------------------
function  LoadGLTextures():integer;


var
    // Status indicator
    Status : integer = 0;
     TextureImage : array  of PSDL_Surface;
     destSurface : PSDL_Surface;

     texture : array of GLUint;
begin
     setlength(TextureImage,1);
     setlength(texture,1);
    //* Create storage space for the texture */
   // SDL_Surface *TextureImage[1];

   glEnable(GL_TEXTURE_2D);
      TextureImage[0] := SDL_LoadBMP('abstract_1a.bmp' );

      destSurface  := SDL_createRGBsurface(0,TextureImage[0].w, TextureImage[0].h, 24, $000000ff,$0000ff00,$00ff0000,$ff000000);
      SDL_blitsurface(TextureImage[0],NIL, destSurface, NIL);
    //* Load The Bitmap, Check For Errors, If Bitmap's Not Found Quit */
    if ( TextureImage[0].h > 0 ) then
    begin

      //* Set the status to true */
      Status := 1;
      //OutputDebugStringW(L"My output string.");
      //* Create The Texture */
      glGenTextures( 1, @texture[0] );



      //* Typical Texture Generation Using Data From The Bitmap */
      glBindTexture( GL_TEXTURE_2D, texture[0] );

      //glPixelStorei(GL_UNPACK_ALIGNMENT, 4);
      {
      //* Generate The Texture */
      glTexImage2D( GL_TEXTURE_2D, 0, 3, TextureImage[0].w,
      TextureImage[0].h, 0, $80E0,
      GL_UNSIGNED_BYTE, TextureImage[0].pixels );
      }
      glTexImage2D( GL_TEXTURE_2D, 0, 3, destSurface.w,
      destSurface.h, 0, GL_RGB,
      GL_UNSIGNED_BYTE, destSurface.pixels );

      //showmessage(inttostr(glGetError()));

      //* Linear Filtering */
      glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);
      glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);


     end;

    //  showmessage('Width = ' + inttostr(TextureImage[0].w) + '. Height = ' + inttostr(TextureImage[0].h));

    //* Free up any memory we may have used */
   // if ( TextureImage[0] = 0 ) then
   // begin
	    SDL_FreeSurface( TextureImage[0] );
   // end;

    result := Status;
end;
//------------------------------------------------------------------------------



//------------------------------------------------------------------------------
procedure  LoadGLTexture(pathAndFilename: pchar; var textureID : GLUint; var widthPixels, heightPixels: integer);

var
    // Status indicator
    //Status : integer = 0;

    TextureImage : PSDL_Surface; // surface into which bmp is loaded
    destSurface : PSDL_Surface;  // surface into which Texture image is blitted to give correct RGB order of bytes

  //  textureID : GLUint;          // ID of GL texture

begin

    //* Create storage space for the texture */
    glEnable(GL_TEXTURE_2D);

    TextureImage := SDL_LoadBMP(pathAndFilename);

    // copy image into new surface, with correct order of RGB
    destSurface  := SDL_createRGBsurface(0,TextureImage.w, TextureImage.h, 24, $000000ff,$0000ff00,$00ff0000,$ff000000);

    SDL_blitsurface(TextureImage,NIL, destSurface, NIL);

    //* Load The Bitmap, Check For Errors, If Bitmap's Not Found Quit */
    if ( TextureImage<> nil ) then
    begin

      //* Set the status to true */
      //Status := 1;

      //* Create The Texture */
      glGenTextures( 1, @textureID );

      //* Typical Texture Generation Using Data From The Bitmap */
      glBindTexture( GL_TEXTURE_2D, textureID );

      glTexImage2D( GL_TEXTURE_2D, 0, 3, destSurface.w,
      destSurface.h, 0, GL_RGB,
      GL_UNSIGNED_BYTE, destSurface.pixels );

      //* Linear Filtering */
      glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);
      glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);
   end;

	  SDL_FreeSurface(TextureImage );

   //result := Status;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
function selectBMPimage (BMPimages : array of TBMPimages; imageNo:integer): integer;
var
    destSurface : PSDL_Surface;  // surface into which Texture image is blitted to give correct RGB order of bytes

begin
  Result := 0;
  // copy image into new surface, with correct order of RGB
  destSurface  := SDL_createRGBsurface(0,BMPimages[imageNo].TextureImage.w, BMPimages[imageNo].TextureImage.h, 24, $000000ff,$0000ff00,$00ff0000,$ff000000);

  SDL_blitsurface(BMPimages[imageNo].TextureImage,NIL, destSurface, NIL);

  //* Create storage space for the texture */
  glEnable(GL_TEXTURE_2D);

  //* Load The Bitmap, Check For Errors, If Bitmap's Not Found Quit */
  if ( BMPimages[imageNo].TextureImage<> nil ) then
  begin

    //* Set the status to true */
    //Status := 1;

    //* Create The Texture */
    glGenTextures( 1, @BMPimages[imageNo].textureID );

    //* Typical Texture Generation Using Data From The Bitmap */
    glBindTexture( GL_TEXTURE_2D, BMPimages[imageNo].textureID );

    glTexImage2D( GL_TEXTURE_2D, 0, 3, destSurface.w,
    destSurface.h, 0, GL_RGB,
    GL_UNSIGNED_BYTE, destSurface.pixels );

    //* Linear Filtering */
    glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);
    glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);
  end;

  SDL_FreeSurface(destSurface);
  //SDL_FreeSurface(BMPimages[imageNo].TextureImage );

  //result := Status;
end;
//------------------------------------------------------------------------------



//------------------------------------------------------------------------------
// load BMP images with filenames 300.bmp up to 399.bmp
function loadBMPimages (experiment_dir: string; var BMPimages : array of TBMPimages): integer;

var
    c : integer;
    BMPimageFilename: string;

begin
  Result := 0;
  for c:=0 to 99 do
  begin
    BMPimageFilename := experiment_dir + 'Stimulus images\' + {'Image_'+} inttostr(c+300)+'.bmp';

    if fileExists(BMPimageFilename) then
    begin
    // showmessage(BMPimageFilename + '  ok');
    BMPimages[c].TextureImage := SDL_LoadBMP(pchar(BMPimageFilename));
    end;
  end;

  // load 'correct.bmp'
  BMPimageFilename := experiment_dir + 'Stimulus images\' + 'correct.bmp';
  if fileExists(BMPimageFilename) then
  begin
    // showmessage(BMPimageFilename + '  ok');
    BMPimages[100].TextureImage := SDL_LoadBMP(pchar(BMPimageFilename));
  end;

  // load 'correct.bmp'
  BMPimageFilename := experiment_dir + 'Stimulus images\' + 'incorrect.bmp';
  if fileExists(BMPimageFilename) then
  begin
    // showmessage(BMPimageFilename + '  ok');
    BMPimages[101].TextureImage := SDL_LoadBMP(pchar(BMPimageFilename));
  end;


end;
//------------------------------------------------------------------------------




//------------------------------------------------------------------------------
function displayBMPimage(var BMPimages: array of TBMPimages; imageNo:integer):integer;

var

  destSurface : PSDL_Surface;  // surface into which Texture image is blitted to give correct RGB order of bytes

begin
  Result := 0;
  //* Load The Bitmap, Check For Errors, If Bitmap's Not Found Quit */
  if ( BMPimages[imageNo].TextureImage<> nil ) then
  begin

    // create 24bit destination surface with little-endian byte order
    destSurface  := SDL_createRGBsurface(0,BMPimages[imageNo].TextureImage.w, BMPimages[imageNo].TextureImage.h, 24, $000000ff,$0000ff00,$00ff0000,$ff000000);

    SDL_blitsurface(BMPimages[imageNo].TextureImage,NIL, destSurface, NIL);

    //* Create storage space for the texture */
    glEnable(GL_TEXTURE_2D);

    //* Create The Texture */
    glGenTextures( 1, @BMPimages[imageNo].textureID );

    //* Typical Texture Generation Using Data From The Bitmap */
    glBindTexture( GL_TEXTURE_2D, BMPimages[imageNo].textureID );

    glTexImage2D( GL_TEXTURE_2D, 0, 3, destSurface.w, destSurface.h, 0, GL_RGB, GL_UNSIGNED_BYTE, destSurface.pixels );

    //* Linear Filtering */
    glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);
    glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);

    // Select how the texture image is combined with existing image
    glTexEnvf(GL_TEXTURE_ENV,GL_TEXTURE_ENV_MODE,GL_REPLACE);

    glBegin(GL_QUADS);
    // top left
    glTexCoord2f(0,0);
    glVertex2f(-BMPimages[imageNo].TextureImage.w/2 ,BMPimages[imageNo].TextureImage.h/2);

    // bottom left
    glTexCoord2f(0,1);
    glVertex2f(-BMPimages[imageNo].TextureImage.w/2,-BMPimages[imageNo].TextureImage.h/2);

    // bottom right
    glTexCoord2f(1,1);
    glVertex2f(BMPimages[imageNo].TextureImage.w/2,-BMPimages[imageNo].TextureImage.h/2);

    // top right
    glTexCoord2f(1,0);
    glVertex2f(BMPimages[imageNo].TextureImage.w/2, BMPimages[imageNo].TextureImage.h/2);
    glEnd;

    glDisable(GL_TEXTURE_2D);
    glDeleteTextures(1,@BMPimages[imageNo].textureID );

    // clean up
    SDL_freeSurface(destSurface);
  end;
end;
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
function displayBMPimageXYcm(var BMPimages: array of TBMPimages; imageNo:integer; xcm, ycm :real; screenWidthPix, screenHeightPix: integer; screenWidthCM,  screenHeightCM: real;
  scaleH: double = 1.0; scaleV: double = 1.0):integer;
begin
  Result := 0;
  glviewport(0,0,screenWidthPix,screenHeightPix);
  glMatrixMode(GL_PROJECTION);
  glpushmatrix();
  glLoadIdentity;
  glMatrixMode(GL_MODELVIEW);
  glpushmatrix();
  glLoadIdentity;
  glortho(-screenWidthPix/2,screenWidthPix/2,-screenHeightPix/2,screenHeightPix/2,-1,1);
  gltranslatef( xcm * (screenWidthPix/ screenWidthCM) , ycm * (screenHeightPix / screenHeightCM), 0);
  glScaled(scaleH, scaleH, 1.0);
  displayBMPimage(BMPimages,imageNo);

  glpopmatrix();
  glMatrixMode(GL_PROJECTION);
  glpopmatrix();
end;
//------------------------------------------------------------------------------

function displayBMPimageXYSizecm(var BMPimages: TBMPimages; xcm, ycm :real; screenWidthCM, screenHeightCM: real;
  widthCm, HeightCm: real):integer;
var
  destSurface : PSDL_Surface;  // surface into which Texture image is blitted to give correct RGB order of bytes
begin
  Result := 0;
  if BMPimages.TextureImage = nil then Exit;

  //glviewport(0,0,screenWidthPix,screenHeightPix);
  glMatrixMode(GL_PROJECTION);
  glpushmatrix();
  glLoadIdentity;
  glMatrixMode(GL_MODELVIEW);
  glpushmatrix();
  glLoadIdentity;
  glortho(-screenWidthCm/2,screenWidthCm/2,-screenHeightCm/2,screenHeightCm/2,-1,1);
  gltranslatef( xcm, ycm, 0);
  glScaled(widthCm*0.5, heightCm*0.5, 1.0);

  Result := 0;
  //* Load The Bitmap, Check For Errors, If Bitmap's Not Found Quit */

  // create 24bit destination surface with little-endian byte order
  destSurface  := SDL_createRGBsurface(0,BMPimages.TextureImage.w, BMPimages.TextureImage.h, 24, $000000ff,$0000ff00,$00ff0000,$ff000000);

  SDL_blitsurface(BMPimages.TextureImage,NIL, destSurface, NIL);

  //* Create storage space for the texture */
  glEnable(GL_TEXTURE_2D);

  //* Create The Texture */
  glGenTextures( 1, @BMPimages.textureID );

  //* Typical Texture Generation Using Data From The Bitmap */
  glBindTexture( GL_TEXTURE_2D, BMPimages.textureID );

  glTexImage2D( GL_TEXTURE_2D, 0, 3, destSurface.w, destSurface.h, 0, GL_RGB, GL_UNSIGNED_BYTE, destSurface.pixels );

  //* Linear Filtering */
  glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);
  glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);

  // Select how the texture image is combined with existing image
  glTexEnvf(GL_TEXTURE_ENV,GL_TEXTURE_ENV_MODE,GL_REPLACE);

  glBegin(GL_QUADS);
  // top left
  glTexCoord2f(0,0);
  glVertex2f(-1.0 ,1.0);

  // bottom left
  glTexCoord2f(0,1);
  glVertex2f(-1.0,-1.0);

  // bottom right
  glTexCoord2f(1,1);
  glVertex2f(1.0,-1.0);

  // top right
  glTexCoord2f(1,0);
  glVertex2f(1.0, 1.0);
  glEnd;

  glDisable(GL_TEXTURE_2D);
  glDeleteTextures(1, @BMPimages.textureID );

  // clean up
  SDL_freeSurface(destSurface);


  glpopmatrix();
  glMatrixMode(GL_PROJECTION);
  glpopmatrix();
end;

end.

