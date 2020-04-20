# MCSPIFFSUploaderServer

## Methods
```
    void              begin                         ( uint16_t port = SPIFFS_UPLOAD_SERVER_PORT );
    void              handle                        ( void );
    void              StartAsync                    ( uint32_t wait = 100 ); // 100mills
```

## Use of MCSPIFFSUploaderServer
```
  #include <MCSPIFFSUploaderServer.h>
  void setup()
  {
    MCSPIFFSUploader.begin();
  }
  void loop()
  {
    MCSPIFFSUploader.handle();
  }
```  
 ## Or....
```
  #include <MCSPIFFSUploaderServer.h>
  void setup()
  {
    MCSPIFFSUploader.begin();
    MCSPIFFSUploader.StartAsync();
  }
  void loop()
  {
  }
```

