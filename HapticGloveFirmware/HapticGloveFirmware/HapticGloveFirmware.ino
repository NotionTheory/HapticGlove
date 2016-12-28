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
//const uint8_t SOME_VALUE[] = {0x00, 0x40};
//uint16_t SERVICE_ID = 0x5000;
uint8_t SERVICE_ID[] = {0x40, 0x79, 0x2A, 0xF0, 0xB0, 0xA9, 0x41, 0x73, 0xB7, 0x2D, 0x54, 0x88, 0xAB, 0x30, 0x1D, 0xB5};
//const uint8_t ADV_DATA[] = {0x02, 0x01, 0x06, 0x05, 0x02, 0x00, 0x50, 0x0a, 0x18};

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
  serviceID = gatt.addService(SERVICE_ID);  
  //set_advdata(ADV_DATA);
  
  ble.reset(true);
}

void loop(void)
{
}
