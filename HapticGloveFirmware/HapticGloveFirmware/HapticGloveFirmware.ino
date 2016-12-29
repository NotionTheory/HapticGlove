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
uint8_t SERVICE_ID[] = {0x40, 0x79, 0x2A, 0xF0, 0xB0, 0xA9, 0x41, 0x73, 0xB7, 0x2D, 0x54, 0x88, 0xAB, 0x30, 0x1D, 0xB5};

void stop(){
  while(1);
}

void atcommand(const char* str, int ms = 0) {
  ble.println(str);
  if(ms > 0){
    delay(ms);
  }
}

void setup(void)
{
  if ( !ble.begin(VERBOSE_MODE) || FACTORYRESET_ENABLE && !ble.factoryReset() )
  {
    stop();
  }
  ble.echo(false);

  ble.factoryReset(true);
  gatt.clear();
  
  atcommand("AT+GAPDEVNAME=NotionTheory Haptic Glove", 1000);
  
  serviceID = gatt.addService(SERVICE_ID);
  delay(250);
  char1ID = gatt.addCharacteristic(0x2A58, 0x0A, 2, 2, BLE_DATATYPE_INTEGER);
  delay(250);
  ble.reset(true);
  delay(250);
  gatt.setChar(char1ID, 0xde);
  delay(250);
  ble.reset(true);  
}

void loop(void)
{
}
