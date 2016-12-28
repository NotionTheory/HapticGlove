#include <Arduino.h>
#include <SPI.h>
#if not defined (_VARIANT_ARDUINO_DUE_X_) && not defined (_VARIANT_ARDUINO_ZERO_)
  #include <SoftwareSerial.h>
  #define WTF 1
#else
  #define WTF 0
#endif

#include "Adafruit_BLE.h"
#include "Adafruit_BluefruitLE_SPI.h"
#include "Adafruit_BLEGatt.h"

#include "BluefruitConfig.h"

#define FACTORYRESET_ENABLE 1



Adafruit_BluefruitLE_SPI ble(BLUEFRUIT_SPI_CS, BLUEFRUIT_SPI_IRQ, BLUEFRUIT_SPI_RST);
Adafruit_BLEGatt gatt(ble);

uint8_t serviceID, char1ID, char2ID, char2Value;
const uint8_t SOME_VALUE[] = {0x00, 0x40};
uint16_t SERVICE_ID = 0x180D;
//uint8_t SERVICE_ID[] = {0x3A, 0x3A, 0x0D, 0x8C, 0x20, 0xFE, 0x4F, 0x2B, 0x9F, 0x06, 0x0E, 0x3D, 0x09, 0xA5, 0x4E, 0x69};
const uint8_t ADV_DATA[] = {0x02, 0x01, 0x06, 0x05, 0x02, 0x0d, 0x18, 0x0a, 0x18};

void stop(){
  while(1);
}

void atcommand(const char* str, int ms = 0) {
  ble.println(str);
  if(ms > 0){
    delay(ms);
  }
}

void set_advdata(const uint8_t data[]) {
  size_t len = sizeof(data) / sizeof(uint8_t);
  ble.setAdvData((uint8_t*)data, len);
}

void setup(void)
{
  if ( !ble.begin(VERBOSE_MODE) || FACTORYRESET_ENABLE && !ble.factoryReset() )
  {
    stop();
  }
  ble.echo(false);

  ble.factoryReset(true);
  atcommand("AT+GAPDEVNAME=NotionTheory Haptic Glove", 1000);


  gatt.clear();
  serviceID = gatt.addService(0x180D);
  char1ID = gatt.addCharacteristic(0x2A37, 0x10, 2, 3, BLE_DATATYPE_BYTEARRAY);
  char2ID = gatt.addCharacteristic(0x2A38, 0x02, 1, 1, BLE_DATATYPE_INTEGER);
  gatt.setChar(char1ID, SOME_VALUE, 2);
  char2Value = 0;
  
  set_advdata(ADV_DATA);
  
  ble.reset(true);
}

void loop(void)
{
  //char command[BUFSIZE+1];
  //getUserInput(command, BUFSIZE);
  //ble.println(command);
  //ble.waitForOK();
  char2Value = (char2Value + 1) % 256;
  gatt.setChar(char2ID, char2Value);
  delay(250);
}
