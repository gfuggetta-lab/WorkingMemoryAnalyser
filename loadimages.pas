unit loadImages;

{$mode delphi}

interface

uses
  Classes, SysUtils,SDL2,gl,glu, dialogs, sdl2_image;

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

function GetMaskByteIndex(Mask: UInt32): Integer;
var
  Shift: Integer;
begin
  Result := -1;

  if Mask = 0 then
    Exit;

  Shift := 0;

  while ((Mask and 1) = 0) and (Shift < 32) do
  begin
    Mask := Mask shr 1;
    Inc(Shift);
  end;

  if (Shift mod 8) <> 0 then
    Exit;

  Result := Shift div 8;
end;

function GetGLTexturePixelFormatFromSurface(
  Surface: PSDL_Surface;
  out gl_internalFormat: integer;
  out gl_format: integer;
  out gl_PixelType: integer
): Boolean;
var
  Fmt: PSDL_PixelFormat;
  Bpp: Integer;
  RIndex: Integer;
  GIndex: Integer;
  BIndex: Integer;
  AIndex: Integer;
const
  GL_RGB                       = $1907;
  GL_RGBA                      = $1908;
  GL_BGR                       = $80E0;
  GL_BGRA                      = $80E1;
  GL_UNSIGNED_BYTE             = $1401;
  GL_UNSIGNED_SHORT_5_6_5      = $8363;
  GL_UNSIGNED_SHORT_5_5_5_1    = $8034;
  GL_UNSIGNED_SHORT_4_4_4_4    = $8033;
begin
  Result := False;

  gl_internalFormat := 0;
  gl_Format := 0;
  gl_PixelType := 0;

  if (Surface = nil) or (Surface^.format = nil) then
    Exit;

  Fmt := Surface^.format;
  Bpp := Fmt^.BytesPerPixel;

  case Bpp of
    4:
      begin
        RIndex := GetMaskByteIndex(Fmt^.Rmask);
        GIndex := GetMaskByteIndex(Fmt^.Gmask);
        BIndex := GetMaskByteIndex(Fmt^.Bmask);
        AIndex := GetMaskByteIndex(Fmt^.Amask);

        if Fmt^.Amask <> 0 then
          gl_InternalFormat := GL_RGBA
        else
          gl_InternalFormat := GL_RGB;

        gl_PixelType := GL_UNSIGNED_BYTE;

        // Memory layout: R, G, B, A
        if (RIndex = 0) and (GIndex = 1) and (BIndex = 2) then
        begin
          if Fmt^.Amask <> 0 then
          begin
            if AIndex <> 3 then
              Exit;
          end;

          gl_Format := GL_RGBA;
          Result := True;
          Exit;
        end;

        // Memory layout: B, G, R, A
        if (BIndex = 0) and (GIndex = 1) and (RIndex = 2) then
        begin
          if Fmt^.Amask <> 0 then
          begin
            if AIndex <> 3 then
              Exit;
          end;

          gl_Format := GL_BGRA;
          Result := True;
          Exit;
        end;
      end;

    3:
      begin
        RIndex := GetMaskByteIndex(Fmt^.Rmask);
        GIndex := GetMaskByteIndex(Fmt^.Gmask);
        BIndex := GetMaskByteIndex(Fmt^.Bmask);

        gl_InternalFormat := GL_RGB;
        gl_PixelType := GL_UNSIGNED_BYTE;

        // Memory layout: R, G, B
        if (RIndex = 0) and (GIndex = 1) and (BIndex = 2) then
        begin
          gl_Format := GL_RGB;
          Result := True;
          Exit;
        end
        else if (RIndex = 2) and (GIndex = 1) and (BIndex = 0) then
        begin
          gl_Format := GL_BGR;
          Result := True;
          Exit;
        end;
      end;

    2:
      begin
        gl_InternalFormat := GL_RGB;

        // RGB565
        if (Fmt^.Rmask = $F800) and
           (Fmt^.Gmask = $07E0) and
           (Fmt^.Bmask = $001F) and
           (Fmt^.Amask = 0) then
        begin
          gl_Format := GL_RGB;
          gl_PixelType := GL_UNSIGNED_SHORT_5_6_5;
          Result := True;
          Exit;
        end;

        // RGBA5551
        if (Fmt^.Rmask = $F800) and
           (Fmt^.Gmask = $07C0) and
           (Fmt^.Bmask = $003E) and
           (Fmt^.Amask = $0001) then
        begin
          gl_InternalFormat := GL_RGBA;
          gl_Format := GL_RGBA;
          gl_PixelType := GL_UNSIGNED_SHORT_5_5_5_1;
          Result := True;
          Exit;
        end;

        // RGBA4444
        if (Fmt^.Rmask = $F000) and
           (Fmt^.Gmask = $0F00) and
           (Fmt^.Bmask = $00F0) and
           (Fmt^.Amask = $000F) then
        begin
          gl_InternalFormat := GL_RGBA;
          gl_Format := GL_RGBA;
          gl_PixelType := GL_UNSIGNED_SHORT_4_4_4_4;
          Result := True;
          Exit;
        end;
      end;
  end;
end;

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


function TryToLoadImage(const fnNoExt: string): PSDL_Surface;
var
  fn  : string;
  i   : integer;
const
  ext : array [0..1] of string = ('.png','.bmp');
begin
  Result := nil;
  for i:=0 to length(ext)-1 do begin
    fn := ChangeFileExt(fnNoExt, ext[i]);
    if fileExists(fn) then begin
      // showmessage(BMPimageFilename + '  ok');
      Result := IMG_Load(PAnsiChar(fn));
      if Assigned(Result) then EXit;
    end;
  end;
end;

//------------------------------------------------------------------------------
// load BMP images with filenames 300.bmp up to 10300.bmp
function loadBMPimages (experiment_dir: string; var BMPimages : array of TBMPimages): integer;
var
  c   : integer;
  fn  : string;
begin
  Result := 0;
  for c:=0 to 10000 do
  begin
    fn := experiment_dir + 'Stimulus images' + PathDelim + {'Image_'+} inttostr(c+300);
    BMPimages[c].TextureImage := TryToLoadImage(fn);
  end;

  // load 'correct.bmp'
  fn := experiment_dir + 'Stimulus images' + PathDelim + 'correct';
  BMPimages[100].TextureImage := TryToLoadImage(fn);

  // load 'correct.bmp'
  fn := experiment_dir + 'Stimulus images' + PathDelim + 'incorrect';
  BMPimages[101].TextureImage := TryToLoadImage(fn);
end;
//------------------------------------------------------------------------------




//------------------------------------------------------------------------------
function displayBMPimage(var BMPimages: array of TBMPimages; imageNo:integer):integer;

var

  isEnabled  : Boolean;
  gl_intFmt  : integer;
  gl_Fmt     : integer;
  gl_PixType : integer;
begin
  isEnabled := glIsEnabled( GL_BLEND) <> GL_NONE;
  if not isEnabled then begin
    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
  end;

  Result := 0;
  //* Load The Bitmap, Check For Errors, If Bitmap's Not Found Quit */
  if ( BMPimages[imageNo].TextureImage<> nil ) then
  begin


    //* Create storage space for the texture */
    glEnable(GL_TEXTURE_2D);

    //* Create The Texture */
    glGenTextures( 1, @BMPimages[imageNo].textureID );

    //* Typical Texture Generation Using Data From The Bitmap */
    glBindTexture( GL_TEXTURE_2D, BMPimages[imageNo].textureID );

    GetGLTexturePixelFormatFromSurface(BMPimages[imageNo].TextureImage, gl_intFmt, gl_Fmt, gl_PixType);

    glTexImage2D( GL_TEXTURE_2D, 0,
      gl_intFmt,
      BMPimages[imageNo].TextureImage.w,
      BMPimages[imageNo].TextureImage.h,
      0, gl_Fmt, gl_PixType,
      BMPimages[imageNo].TextureImage.pixels );

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

  end;

  if not isEnabled then begin
    glDisable(GL_BLEND);
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
  isEnabled : Boolean;
  gl_intFmt  : integer;
  gl_Fmt     : integer;
  gl_PixType : integer;
begin
  Result := 0;
  if BMPimages.TextureImage = nil then Exit;

  isEnabled := glIsEnabled( GL_BLEND) <> GL_NONE;
  if not isEnabled then begin
    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
  end;

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


  //* Create storage space for the texture */
  glEnable(GL_TEXTURE_2D);

  //* Create The Texture */
  glGenTextures( 1, @BMPimages.textureID );

  //* Typical Texture Generation Using Data From The Bitmap */
  glBindTexture( GL_TEXTURE_2D, BMPimages.textureID );

  GetGLTexturePixelFormatFromSurface(BMPimages.TextureImage, gl_intFmt, gl_Fmt, gl_PixType);

  glTexImage2D( GL_TEXTURE_2D, 0, 
    gl_intFmt, 
    BMPimages.TextureImage.w, 
    BMPimages.TextureImage.h, 
    0, 
    gl_Fmt, gl_PixType, 
    BMPimages.TextureImage.pixels );

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



  glpopmatrix();
  glMatrixMode(GL_PROJECTION);
  glpopmatrix();

  if not isEnabled then begin
    glDisable(GL_BLEND);
  end;
end;

end.

