#include <Arduino.h>
#include <SPI.h>
#if not defined (_VARIANT_ARDUINO_DUE_X_) && not defined (_VARIANT_ARDUINO_ZERO_)
  #include <SoftwareSerial.h>
#endif

#include "Adafruit_BLE.h"
#include "Adafruit_BluefruitLE_SPI.h"
#include "Adafruit_BLEGatt.h"

#include "BluefruitConfig.h"

#define VBATPIN A7


Adafruit_BluefruitLE_SPI ble(BLUEFRUIT_SPI_CS, BLUEFRUIT_SPI_IRQ, BLUEFRUIT_SPI_RST);

//uint8_t GATT_SERVICE_CUSTOM_ID[] = {0x40, 0x79, 0x2A, 0xF0, 0xB0, 0xA9, 0x41, 0x73, 0xB7, 0x2D, 0x54, 0x88, 0xAB, 0x30, 0x1D, 0xB5};

void stop(){
  ble.sendCommandCheckOK(F("AT+GAPDEVNAME=ERROR"));
  while(1);
}

int32_t batteryServiceID, batteryLevelCharID, 
  analog1CharID, analog2CharID, analog3CharID, analog4CharID, analog5CharID;

void sendByte(int32_t charID, uint8_t val){
  ble.print(F("AT+GATTCHAR="));
  ble.print(charID);
  ble.print(F(","));
  ble.println(val, HEX);
  ble.waitForOK();  
}

void sendShort(int32_t charID, uint16_t val){
  ble.print(F("AT+GATTCHAR="));
  ble.print(charID);
  ble.print(F(","));
  ble.println((val>>8) & 0xF, HEX);
  ble.print(F("-"));
  ble.println(val & 0xF, HEX);
  ble.waitForOK();  
}

void readBattery()
{
  float measuredvbat = analogRead(VBATPIN);
  measuredvbat *= 2;    // we divided by 2, so multiply back
  measuredvbat *= 3.3;  // Multiply by 3.3V, our reference voltage
  measuredvbat /= 10.24; // convert to voltage
  sendByte(batteryLevelCharID, (uint8_t)measuredvbat);
}

void setup(void)
{
  if ( !ble.begin(VERBOSE_MODE)
    || !ble.factoryReset(true)
    || !ble.sendCommandCheckOK(F("AT+GATTCLEAR"))
    || !ble.sendCommandWithIntReply( F("AT+GATTADDSERVICE=UUID=0x180F"), &batteryServiceID)
    || !ble.sendCommandWithIntReply( F("AT+GATTADDCHAR=UUID=0x2A19, PROPERTIES=0x3A, MIN_LEN=1, MAX_LEN=1, VALUE=00"), &batteryLevelCharID)
    || !ble.sendCommandWithIntReply( F("AT+GATTADDCHAR=UUID=0x2A58, PROPERTIES=0x3A, MIN_LEN=1, MAX_LEN=1, VALUE=00"), &analog1CharID)
    || !ble.sendCommandWithIntReply( F("AT+GATTADDCHAR=UUID=0x2A58, PROPERTIES=0x3A, MIN_LEN=1, MAX_LEN=1, VALUE=00"), &analog2CharID)
    || !ble.sendCommandWithIntReply( F("AT+GATTADDCHAR=UUID=0x2A58, PROPERTIES=0x3A, MIN_LEN=1, MAX_LEN=1, VALUE=00"), &analog3CharID)
    || !ble.sendCommandWithIntReply( F("AT+GATTADDCHAR=UUID=0x2A58, PROPERTIES=0x3A, MIN_LEN=1, MAX_LEN=1, VALUE=00"), &analog4CharID)
    || !ble.sendCommandWithIntReply( F("AT+GATTADDCHAR=UUID=0x2A58, PROPERTIES=0x3A, MIN_LEN=1, MAX_LEN=1, VALUE=00"), &analog5CharID)
    || !ble.sendCommandCheckOK(F("AT+GAPDEVNAME=NotionTheory Haptic Glove"))) {
    stop();
  }
  
  ble.reset(true);
}

int counterA = 0, counterB = 0;
void loop(void)
{
  ++counterA;
  if(counterA == 256){
    counterA = 0;
    sendByte(analog1CharID, counterB++);
    if(counterB == 256) {
      counterB = 0;
      readBattery();
    }
  }
}
