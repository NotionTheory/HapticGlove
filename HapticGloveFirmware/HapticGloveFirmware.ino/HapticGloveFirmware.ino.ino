/*********************************************************************
 This is an example for our nRF51822 based Bluefruit LE modules

 Pick one up today in the adafruit shop!

 Adafruit invests time and resources providing this open source code,
 please support Adafruit and open-source hardware by purchasing
 products from Adafruit!

 MIT license, check LICENSE for more information
 All text above, and the splash screen below must be included in
 any redistribution
*********************************************************************/

#include <Arduino.h>
#include <SPI.h>
#if not defined (_VARIANT_ARDUINO_DUE_X_) && not defined (_VARIANT_ARDUINO_ZERO_)
  #include <SoftwareSerial.h>
#endif

#include "Adafruit_BLE.h"
#include "Adafruit_BluefruitLE_SPI.h"
#include "Adafruit_BluefruitLE_UART.h"

#include "BluefruitConfig.h"
#define FACTORYRESET_ENABLE 1
Adafruit_BluefruitLE_SPI ble(BLUEFRUIT_SPI_CS, BLUEFRUIT_SPI_IRQ, BLUEFRUIT_SPI_RST);

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
  atcommand(F("AT+FACTORYRESET"), 1000)  // Wait 1s for this to complete
  atcommand(F("AT+GATTCLEAR"));
  atcommand(F("AT+GATTADDSERVICE=UUID=0x180D"));
  atcommand(F("AT+GATTADDCHAR=UUID=0x2A37, PROPERTIES=0x10, MIN_LEN=2, MAX_LEN=3, VALUE=00-40"));
  atcommand(F("AT+GATTADDCHAR=UUID=0x2A38, PROPERTIES=0x02, MIN_LEN=1, VALUE=3"));
  atcommand(F("AT+GAPSETADVDATA=02-01-06-05-02-0d-18-0a-18"));
  // Perform a system reset and wait 1s to come back online
  atcommand(F("ATZ"), 1000);
}

void loop(void)
{
  char command[BUFSIZE+1];
  // getUserInput(command, BUFSIZE);
  ble.println(command);
  ble.waitForOK();
}
