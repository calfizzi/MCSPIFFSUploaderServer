/*
 Name:		MCUploaderServer.ino
 Created:	4/22/2020 12:24:36 PM
 Author:	Mario
*/

#include <MCSPIFFSUploaderServer.h>

#if defined(ESP32)
  #include <WiFi.h>
  #include <WiFiServer.h>
  #include <ESPmDNS.h>
  #include <WebServer.h>
  #include <FS.h>   // Include the SPIFFS library
  #include <SPIFFS.h> // Include the SPIFFS library
  #define WEBServer WebServer
#elif defined(ESP8266)
  #include <ESP8266WiFi.h>
  //#include <ESP8266WiFiServer.h>
  #include <ESP8266WebServer.h>
  #include <ESP8266mDNS.h>
  #define WEBServer ESP8266WebServer
#endif

#ifndef  FILE_READ
  #define  FILE_READ "r"
#endif
#ifndef  FILE_WRITE
  #define  FILE_WRITE "w"
#endif


const char* ssid      = "[YOUR SAID]";
const char* password  = "[YOUR SSID PASSWORD]";

WEBServer webServer(80);

void BoardData()
{
    uint32_t ideSize = ESP.getFlashChipSize();
    FlashMode_t ideMode = ESP.getFlashChipMode();
#if defined(ESP32)
    Serial.printf("Flash real id:                 %08X\n", ESP.getChipRevision());
#elif defined (ESP8266)
    Serial.printf("Flash real id:                 %08X\n",      ESP.getChipId());
#endif
    Serial.printf("Flash ide size:                %u Bytes\n",  ideSize);
    Serial.printf("Flash ide speed:               %u bps\n",    ESP.getFlashChipSpeed());
    Serial.printf("Flash ide frequency:           %u MHz\n",    ESP.getCpuFreqMHz());
    Serial.printf("Flash ide total  memory size:  %u Bytes\n",  ESP.getFlashChipSize());
    Serial.printf("Flash ide sketch memory size:  %u Bytes\n",  ESP.getFreeSketchSpace());
    Serial.printf("Flash ide mode:                %s\n",        (ideMode == FM_QIO ? "QIO" : ideMode == FM_QOUT ? "QOUT" : ideMode == FM_DIO ? "DIO" : ideMode == FM_DOUT ? "DOUT" : "UNKNOWN"));
    Serial.println("Flash Chip configuration ok.\n");
}


String  WebServerGetContentType     ( String filename){
  if(webServer.hasArg("download")) return "application/octet-stream";
  else if(filename.endsWith(".htm")) return "text/html";
  else if(filename.endsWith(".html")) return "text/html";
  else if(filename.endsWith(".css")) return "text/css";
  else if(filename.endsWith(".js")) return "application/javascript";
  else if(filename.endsWith(".png")) return "image/png";
  else if(filename.endsWith(".gif")) return "image/gif";
  else if(filename.endsWith(".jpg")) return "image/jpeg";
  else if(filename.endsWith(".ico")) return "image/x-icon";
  else if(filename.endsWith(".xml")) return "text/xml";
  else if(filename.endsWith(".pdf")) return "application/x-pdf";
  else if(filename.endsWith(".zip")) return "application/x-zip";
  else if(filename.endsWith(".gz")) return "application/x-gzip";
  return "text/plain";
}
void    WebServerNoCacheHeader      ( void )
{
  webServer.sendHeader("Cache-Control", "no-cache, no-store, must-revalidate");
  webServer.sendHeader("Pragma", "no-cache");
  webServer.sendHeader("Expires", "-1");
}
void    WebServerCachedHeader       ( void )
{
  webServer.sendHeader        ( "Cache-Control", " max-age=3600"); // 1 hour
}
bool    WebServerFileReadAndSend    ( String path){
  if(path.endsWith("/")) path += "/index.html";
  String contentType = WebServerGetContentType(path);
  if(SPIFFS.exists(path))
  {
    File file = SPIFFS.open(path, FILE_READ);
    webServer.streamFile(file, contentType);
    file.close();
    webServer.client().stop();
    return true;
  }
  return false;
}
void    WebServer_send_SPIFFS_file  ( String filePathName)
{
  String content_type = WebServerGetContentType(filePathName);
  
  if(!WebServerFileReadAndSend(filePathName ))
    webServer.send(404, "text/plain", webServer.uri() + " Not Found!");

}
void    handle_index                ( void )
{
  String path = "/index.html";
  WebServerCachedHeader();
  WebServer_send_SPIFFS_file(path);
}
void    handle_index2               ( void ){
  String path = "/index2.html";
  WebServerCachedHeader();
  WebServer_send_SPIFFS_file(path);
}
void    handle_monkey_logo          ( void ){
  Serial.println("handle_monkey_logo Start");
  String path = "/images/monkey-logo.png";
  WebServerCachedHeader();
  WebServer_send_SPIFFS_file(path);
  Serial.println("handle_monkey_logo End");
}
void    handle_css_mc_styles_css    ( void ){
  String path = "/css/mc_styles.css";
  WebServerCachedHeader();
  WebServer_send_SPIFFS_file(path);
}

void reconnectWiFi()
{
  static bool FirstTime = true;
  static bool WasConnected = WiFi.status() == WL_CONNECTED;
  bool IsConnected = WiFi.status() == WL_CONNECTED;
  static uint32_t ms = 0;
  if (!IsConnected  && (millis()-ms>10000 || FirstTime))
  {
    Serial.println("Connecting to WiFi..");
    WiFi.begin(ssid, password);
    WiFi.setAutoReconnect(true);
#if defined(ESP32)
    WiFi.setSleep(false);
#elif defined(ESP8266)
    WiFi.setSleepMode(WiFiSleepType::WIFI_NONE_SLEEP);
#endif
    
    webServer.begin();
    FirstTime = false;
    WasConnected = false;
    ms = millis();
    Serial.print("WiFi Status: ");
    Serial.println(WiFi.status());
  }
  if (IsConnected && !WasConnected )
  {
    WasConnected = true;
    Serial.println("Connected to the WiFi network");
    Serial.println(WiFi.localIP());
    ms = millis();
    Serial.print("WiFi Status: ");
    Serial.println(WiFi.status());
    webServer.on("/", handle_index);
    webServer.on("/index.html", handle_index);
    webServer.on("/index2.html", handle_index2);
    webServer.on("/images/monkey-logo.png", handle_monkey_logo);
    webServer.on("/css/mc_styles.css", handle_css_mc_styles_css);
    MCSPIFFSUploader.begin();
    MCSPIFFSUploader.StartAsync(100);
  }
  if (!IsConnected && WasConnected )
  {
    ms = millis();
  }
}

// the setup function runs once when you press reset or power the board
void setup() {
  Serial.begin(115200);
  delay(1000);
  BoardData();
  reconnectWiFi();
}

// the loop function runs over and over again until power down or reset
void loop() {
   reconnectWiFi();
}
