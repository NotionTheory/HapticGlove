#include <Arduino.h>
#include <SPI.h>
#if not defined (_VARIANT_ARDUINO_DUE_X_) && not defined (_VARIANT_ARDUINO_ZERO_)
  #include <SoftwareSerial.h>
#endif

#include "Adafruit_BLE.h"
#include "Adafruit_BluefruitLE_SPI.h"
#include "Adafruit_BLEGatt.h"
#include "BluefruitConfig.h"

//#define DEBUG

#ifdef DEBUG
  #define check(pre, cond) Serial.print(pre); Serial.print("... "); if(cond) { Serial.println(" OK!"); } else { Serial.println("failed!"); stop(); }
  #define MIN_DELAY 250
  #define VERBOSE_MODE true
#else
  #define check(pre, cond) cond
  #define VERBOSE_MODE false
#endif

Adafruit_BluefruitLE_SPI ble(BLUEFRUIT_SPI_CS, BLUEFRUIT_SPI_IRQ, BLUEFRUIT_SPI_RST);
Adafruit_BLEGatt gatt(ble);

void stop()
{
  ble.sendCommandCheckOK(F("AT+GAPDEVNAME=ERROR"));
  while(1)
  {
    if(Serial) Serial.println("STOP!");
    delay(5000);
  }
}


uint8_t addChar(uint16_t id, const char* name, uint8_t size, uint8_t props)
{
  return gatt.addCharacteristic( id, props, size, size, BLE_DATATYPE_INTEGER, name );
}

uint8_t batteryServiceID,
  out1CharID, out2CharID, out3CharID, out4CharID, out5CharID,
  in1CharID;

const uint8_t OUTPUT_PROPERTIES = GATT_CHARS_PROPERTIES_READ | GATT_CHARS_PROPERTIES_WRITE | GATT_CHARS_PROPERTIES_NOTIFY;

void setup(void)
{
#ifdef DEBUG
  while(!Serial);
  delay(500);

  Serial.println("OK!");
#endif

  check("Starting BLE", ble.begin(VERBOSE_MODE));
  check("Disabling echo", ble.echo(false));
  check("Factory reset", ble.factoryReset(true));
  check("Setting device name", ble.sendCommandCheckOK(F("AT+GAPDEVNAME=NotionTheory Haptic Glove")));
  check("Clearing GATT", gatt.clear());

  batteryServiceID = gatt.addService( 0x180F );
  out1CharID = addChar( 0x2A58, "Out 1", 1, OUTPUT_PROPERTIES );
  out2CharID = addChar( 0x2A58, "Out 2", 1, OUTPUT_PROPERTIES );
  out3CharID = addChar( 0x2A58, "Out 3", 1, OUTPUT_PROPERTIES );
  out4CharID = addChar( 0x2A58, "Out 4", 1, OUTPUT_PROPERTIES );
  out5CharID = addChar( 0x2A58, "Out 5", 1, OUTPUT_PROPERTIES );
  in1CharID = addChar( 0x2A58, "In 1", 1, GATT_CHARS_PROPERTIES_WRITE_WO_RESP );

  ble.reset(true);

#ifdef DEBUG
  ble.verbose(false);

  Serial.println("Ready!");

  while(!ble.isConnected())
  {
    Serial.println("Waiting for connection.");
    delay(1000);
  }
#endif
}

void byteOut(const char* name, uint8_t id, uint8_t val)
{
#ifdef DEBUG
  Serial.print("Writing ");
  Serial.print(name);
  Serial.print("...");
#endif

  gatt.setChar(id, val);

#ifdef DEBUG
  delay(MIN_DELAY);
#endif
}

uint8_t byteIn(const char* name, uint8_t id)
{
#ifdef DEBUG
  Serial.print("Reading ");
  Serial.print(name);
  Serial.print("...");
#endif

  uint8_t val = gatt.getCharInt8(id);

#ifdef DEBUG
  delay(MIN_DELAY);
#endif

  return val;
}

uint8_t counter = 0, lastIn1Value = -1, in1Value = 0;
void loop(void)
{
  byteOut("out1", out1CharID, counter++);

  in1Value = byteIn("in1", in1CharID);

  if(in1Value != lastIn1Value)
  {
    lastIn1Value = in1Value;
    byteOut("out2", out2CharID, in1Value);
  }

#ifdef DEBUG
  Serial.println("Delay 1000 ms");
  delay(1000);
#endif
}
