/*

  Module: MCSPIFFSUploaderServer 
  Author: Mario Calfizzi (MC)

  Description:

   Location: https://github.com/calfizzi/MCSPIFFSUploaderServer

*/
//#include <freertos/FreeRTOS.h>
//#include <freertos/task.h>

#include "MCSPIFFSUploaderServer.h"

#ifndef  FILE_READ
  #define  FILE_READ    "r"
#endif
#ifndef  FILE_WRITE
  #define  FILE_WRITE   "w"
#endif
#ifndef  FILE_APPEND
  #define  FILE_APPEND  "a"
#endif


std::thread               MCSPIFFSUploaderThread;
std::chrono::milliseconds wait_millis;
MCSPIFFSUploaderServer    MCSPIFFSUploader_instance;

String MCSPIFFSUploaderServer::_JsonArrayListFile(const char* dirname, uint8_t levels)
{
  return "\"fs\":[" + this->_ListFiles( dirname, levels) + "]";
}
String MCSPIFFSUploaderServer::_ListFiles(const char* dirname, uint8_t levels) 
{
  Serial.printf("Listing directory: %s\r\n", dirname);
  String Data = "";
#if defined(ESP32)
  File root = this->fs->open(dirname);
  if (!root) {
    Serial.println("- failed to open directory");
    return Data;
  }
  if (!root.isDirectory()) {
    Serial.println(" - not a directory");
    return Data;
  }
#elif defined(ESP8266)
  Dir root = this->fs->openDir(dirname);
#endif

#if defined (ESP32)
  File file = root.openNextFile();
  while (file) {
    if (Data != "") Data += ",";
    if (file.isDirectory()) {
      Serial.print("  DIR : ");
      Serial.println(file.name());

      if (levels) {
        Data += this->_ListFiles(file.name(), levels - 1);
      }
    }
    else {
      Data += "{\"filename\":\"" + String(file.name()) + "\", \"filesize\":" + String(file.size()) + "}";
      
      Serial.print("  FILE: ");
      Serial.print(file.name());
      Serial.print("\tSIZE: ");
      Serial.println(file.size());
    }
    file = root.openNextFile();
  }
#elif defined (ESP8266)
  while (root.next()) {
    if (Data != "") Data += ",";
    if (root.isDirectory()) {
      Serial.print("  DIR : ");
      Serial.println(root.fileName());
    
      if (levels) {
        Data += this->_ListFiles(root.fileName().c_str(), levels - 1);
      }
    }
    else {
      Data += "{\"filename\":\"" + String(root.fileName()) + "\", \"filesize\":" + String(root.fileSize()) + "}";
      
      Serial.print("  FILE: ");
      Serial.print(root.fileName());
      Serial.print("\tSIZE: ");
      Serial.println(root.fileSize());
    }
  }

#endif
  return Data;
}
void MCSPIFFSUploaderServer::begin   ( uint16_t port)
{
#if defined(ESP32)
    SPIFFS.begin(true);
#elif defined(ESP8266)
    SPIFFS.begin();
#endif
  this->_SPIFFSUploadServer.begin(port);
  this->fs = &SPIFFS;
}
void MCSPIFFSUploaderServer::_handle ( void )
{
  WiFiClient client = this->_SPIFFSUploadServer.available();
  while (client)
  {
    String jsonMessage = "";
    bool IsJsonMessage = false;
    int jsonCounter = 0;
    while (client.connected()) {

      while (client.available() > 0) {
        char c = client.read();
        if (c == '{')
        {
          IsJsonMessage = true;
          jsonCounter++;
        }
        if (c == '}')
          jsonCounter--;

        jsonMessage += c;
        //Serial.print(c);
        if (IsJsonMessage)
        {
          if (jsonCounter == 0)
          {
            Serial.println(jsonMessage);
            _JsonSPIFFSMessage msg(jsonMessage);
            if (msg.IsCorrect())
            {
              if (msg.Command == "put_file")
              {
                uint32_t ms = millis();
                client.write("{\"command\":\"ok\"}");
                Serial.printf("File Size %u\n", msg.Size);
                while (client.available() == 0 && millis() - ms < 2000)  // wait 2 Seconds
                {
                  yield();
                }
                if (client.available())
                {
                  String FileName = msg.Filename.c_str()[0]=='/'? msg.Filename : "/" + msg.Filename;
                  if (SPIFFS.exists(FileName))
                  {
                    Serial.println("File " + FileName + " already exist will be overwritten!");
                  }
                  File file = SPIFFS.open(FileName, FILE_WRITE);
                  if (!file)
                  {
                    Serial.println("There was an error opening the file for writing");
                    client.stop();
                    return;
                  }
                  int count = 0;
                  ms = millis();
                  while ((client.available() || count < msg.Size) && millis() - ms < 2000)
                  {
                    if (client.available())
                    {
                      char c = client.read();
                      //Serial.print(c);
                      file.write(c);
                      count++;
                      ms = millis();
                    }
                  }
                  Serial.printf("Wrote %u Bytes\n", count);
                  file.close();
                  client.write("{\"command\":\"ok\"}");
                  client.flush();
                  client.stop();
                  //listDir(SPIFFS, "/",0);
                  return;
                }
              }
              if (msg.Command == "get_file")
              {
                String FileName = msg.Filename.c_str()[0]=='/'? msg.Filename : "/" + msg.Filename;
                Serial.println(FileName);
                if (SPIFFS.exists(FileName))
                {
                  File file = SPIFFS.open(FileName, FILE_READ);
                  if (!file)
                  {
                    Serial.println("There was an error opening the file for reading");
                    client.stop();
                    return;
                  }
                  size_t size = file.size();
                  String Response = "{\"command\":\"ok\",\"size\":" + String(size) + " }";
                  client.write(Response.c_str());
                  Serial.println("- read from file:");
                  #define BUFFER_SIZE 128
                  byte buffer[BUFFER_SIZE];
                  int  buffer_size = BUFFER_SIZE;
                  size_t count = 0;;
                  while(file.available() || size-count>0)
                  {
                    if (size-count<buffer_size)
                      buffer_size = size-count;
                    file.read(buffer, buffer_size);
                    client.write(buffer, buffer_size);
                    count += buffer_size;

                  }
                  file.close();
                }else
                  client.write("{\"command\":\"fail\"}");
                client.flush();
                delay(100);
                client.stop();
                return;
              }
              if (msg.Command == "get_fs")
              {
                String response = "{\"command\":\"ok\"";

#if defined(ESP32)
                response += ",\"total_size\":" + String(SPIFFS.totalBytes());
                response += ",\"used_size\":" + String(SPIFFS.usedBytes());
#elif defined(ESP8266)
                FSInfo info;
                SPIFFS.info(info);
                response += ",\"total_size\":" + String(info.totalBytes);
                response += ",\"used_size\":" + String(info.usedBytes);
#endif
                response += "," + this->_JsonArrayListFile("/", 0) + "}";
                client.write(response.c_str());
                client.flush();
                client.stop();
                return;
              }
              if (msg.Command == "delete_file")
              {
                String response = "{\"command\":";
                String FileName = msg.Filename.c_str()[0]=='/'? msg.Filename : "/" + msg.Filename;

                if (SPIFFS.remove(FileName))
                {
                  response += "\"ok\"";
                  Serial.println(FileName + " Deleted!");
                }else 
                {
                  response += "\"fail\"";
                  Serial.println(FileName + " delete Failed!");
                }
                client.write(response.c_str());
                client.flush();
                client.stop();
                //listDir(SPIFFS, "/",0);
                return;
              }
              if (msg.Command == "create_folder")
              {
                String response = "{\"command\":";
                String FolderName = msg.Foldername.c_str()[0]=='/'? msg.Foldername : "/" + msg.Foldername;
                Serial.printf("Exists= %d\n", (int) SPIFFS.exists(FolderName));
                if (SPIFFS.mkdir(FolderName))
                {
                  response += "\"ok\"";
                  Serial.println(FolderName + " created!");
                }else 
                {
                  response += "\"fail\"";
                  Serial.println(FolderName + " create Failed!");
                }
                Serial.printf("Exists= %d\n", (int) SPIFFS.exists(FolderName));
                client.write(response.c_str());
                client.flush();
                client.stop();
                //listDir(SPIFFS, "/",0);
                return;
              }
              if (msg.Command == "delete_folder")
              {
                String response = "{\"command\":";
                String FolderName = msg.Foldername.c_str()[0]=='/'? msg.Foldername : "/" + msg.Foldername;

                if (SPIFFS.rmdir(FolderName))
                {
                  response += "\"ok\"";
                  Serial.println(FolderName + " Deleted!");
                }else 
                {
                  response += "\"fail\"";
                  Serial.println(FolderName + " delete Failed!");
                }
                client.write(response.c_str());
                client.flush();
                client.stop();
                //listDir(SPIFFS, "/",0);
                return;
              }
              if (msg.Command == "format")
              {
                String response = "{\"command\":";
                
                if (SPIFFS.format())
                {
                  response += "\"ok\"";
                  Serial.println("Formatted!");
                }else 
                {
                  response += "\"fail\"";
                  Serial.println("Format Failed!");
                }
                client.write(response.c_str());
                client.flush();
                client.stop();
                //listDir(SPIFFS, "/",0);
                return;
              }
                                

              client.write("{\"command\":\"fail\"}");
              client.stop();
            }
          }

        }
        else
          client.stop();


      }

      delay(10);
    }
    client.stop();
    Serial.println("Client disconnected");
  }
}
bool MCSPIFFSUploaderServer::_JsonSPIFFSMessage::IsCorrect()
{
  return AppCode == "MCESPFFS" && Author == "MC" && Version == "1.0.0.0" && Command.length()>0;
}
MCSPIFFSUploaderServer::_JsonSPIFFSMessage::_JsonSPIFFSMessage(String JsonMessage)
{
  this->AppCode   = _getString(JsonMessage, "appCode");
  this->Author    = _getString(JsonMessage, "author");
  this->Version   = _getString(JsonMessage, "version");
  this->Command   = _getString(JsonMessage, "command");
  this->Filename  = _getString(JsonMessage, "filename");
  this->Foldername= _getString(JsonMessage, "foldername");
  this->Size      = _getNumber(JsonMessage, "size");
  Serial.println  ( this->AppCode   );
  Serial.println  ( this->Author    );
  Serial.println  ( this->Version   );
  Serial.println  ( this->Command   );
  Serial.println  ( this->Filename  );
  Serial.println  ( this->Size      );
}
String MCSPIFFSUploaderServer::_JsonSPIFFSMessage::_get(String str, String key)
{
  String Value = "";
  int index = -1;
  int endIndex = -1;
  index = str.indexOf("\"" + key + "\"");
  if (index >= 0)
  {
    index += 3 + key.length(); // "":
    endIndex = str.indexOf(',', index);
    if (endIndex < 0)
      endIndex = str.indexOf('}', index);
    if (endIndex >= 0)
    {
      Value = str.substring(index, endIndex);
    }
  }
  return Value;
}
String MCSPIFFSUploaderServer::_JsonSPIFFSMessage::_getString(String str, String key)
{
  String Value = "";
  Value = _get(str, key);
  if (Value[Value.length() - 1] == '"')
    Value = Value.substring(0, Value.length() - 1);
  if (Value[0] == '"')
    Value = Value.substring(1);
  return Value;
}
size_t MCSPIFFSUploaderServer::_JsonSPIFFSMessage::_getNumber(String str, String key)
{
  String Value = _get(str, key);
  return atoll(Value.c_str());
}
size_t MCSPIFFSUploaderServer::_JsonSPIFFSMessage::_getDouble(String str, String key)
{
  String Value = _get(str, key);
  return atof(Value.c_str());
}
void MCSPIFFSUploaderServer::handle(void)
{
  if (!this->isAsync)
    this->_handle();
}
void MCSPIFFSUploaderServer::_AsyncLoop(void) {
    Serial.println("starting MCSPIFFSUploaderServer as Async");
    while(true) {
        MCSPIFFSUploader._handle();
        std::this_thread::sleep_for(wait_millis);
    }
}
void MCSPIFFSUploaderServer::StartAsync( uint32_t wait)
{
  this->isAsync = true;
  esp_pthread_cfg_t cfg;
  cfg.prio = 0; 
  cfg.stack_size = 1024*4; // adjust as needed
  esp_pthread_set_cfg(&cfg);
  wait_millis = std::chrono::milliseconds{wait};
  MCSPIFFSUploaderThread = std::thread(_AsyncLoop);
}

