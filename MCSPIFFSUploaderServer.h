/*

  Module: MCSPIFFSUploaderServer 
  Author: Mario Calfizzi (MC)

  Description:

   Location: https://github.com/calfizzi/MCSPIFFSUploaderServer

*/
#pragma once
#ifndef MCSPIFFSUploaderServer_h
#define MCSPIFFSUploaderServer_h

#if defined(ESP32)
  #include <WiFi.h>  
  #include <FS.h>  
  #include <SPIFFS.h>
  #include <WiFiServer.h>
  #define SPIFFSWifiServer  WifiServer

#elif defined(ESP8266)
  #include <ESP8266WiFi.h>
  #include <FS.h>  
  #include <WiFiServer.h>
   
  class SPIFFSWifiServer_class : public WiFiServer
  {
    public:
      SPIFFSWifiServer_class (const IPAddress& addr, uint16_t port) : WiFiServer (addr, port) {}
      SPIFFSWifiServer_class(uint16_t port = 80) : WiFiServer(port) {}
  };
  #define SPIFFSWifiServer SPIFFSWifiServer_class

#endif

#define SPIFFS_UPLOAD_SERVER_PORT 2020


class MCSPIFFSUploaderServer
{
  private:
    String           _JsonArrayListFile(const char* dirname, uint8_t levels);
    String           _ListFiles(const char* dirname, uint8_t levels);
    fs::FS           *fs;
    SPIFFSWifiServer _SPIFFSUploadServer;
    class      _JsonSPIFFSMessage
    {
      private:
        String _get       ( String str, String key );
        String _getString ( String str, String key );
        size_t _getNumber ( String str, String key );
        size_t _getDouble ( String str, String key );
      public:
        String AppCode;
        String Filename;
        String Foldername;
        size_t Size;
        String Version; 
        String Author;
        String Command;
        _JsonSPIFFSMessage(String JsonMessage);
        bool IsCorrect();
    };
  public: 
    MCSPIFFSUploaderServer() {}
    void              begin  ( uint16_t port = SPIFFS_UPLOAD_SERVER_PORT );
    void              handle ( void );
};

extern MCSPIFFSUploaderServer MCSPIFFSUploader_instance;

#ifndef MCSPIFFSUploader
  #define MCSPIFFSUploader MCSPIFFSUploader_instance
#endif
#endif
