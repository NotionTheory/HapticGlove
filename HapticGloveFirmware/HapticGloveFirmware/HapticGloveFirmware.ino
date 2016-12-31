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
  #define check(pre, cond) Serial.print(pre); Serial.print("... "); if(cond) { Serial.println(" OK!"); } else { Serial.println(" failed!"); stop(); }
  #define MIN_DELAY 250
  #define VERBOSE_MODE true
#else
  #define check(pre, cond) cond
  #define VERBOSE_MODE false
#endif

// https://learn.adafruit.com/adafruit-feather-m0-bluefruit-le/pinouts
// Generally speaking, don't use any pins that are prefixed with `_`
enum PINS {
  _UART_RX,
  _UART_TX,
  _GPIO2_NOT_AVAILABLE, // included for completeness, these pins can't be used.
  _GPIO3_NOT_AVAILABLE, // included for completeness, these pins can't be used.
  _BLE_RST,
  GPIO5,
  GPIO6,
  _BLE_IRQ,
  _BLE_CS,
  BATTERY_LEVEL,
  GPIO10,
  GPIO11,
  GPIO12,
  ON_BOARD_LED,
  ANALOG0, // also true analog output
  ANALOG1,
  ANALOG2,
  ANALOG3,
  ANALOG4,
  ANALOG5,
  _I2C_SDA,
  _I2C_SCL,
  _SPI_MISO,
  _SPI_MOSI,
  _SPI_SCK
};

// The API for basic Bluetooth LE operation, including Generic Access Profile, used to initialize the device and set the device name .
Adafruit_BluefruitLE_SPI ble(BLUEFRUIT_SPI_CS, BLUEFRUIT_SPI_IRQ, BLUEFRUIT_SPI_RST);

// The API for Generic Attribute Profile, used to specify services and characteristics.
Adafruit_BLEGatt gatt(ble);

// These are the properties used on the sensor output characteristics.
const uint8_t OUTPUT_PROPERTIES = GATT_CHARS_PROPERTIES_READ | GATT_CHARS_PROPERTIES_NOTIFY;


const PINS SENSOR_PINS[] = {
  ANALOG0, // wrist
  ANALOG1, // thumb
  ANALOG2, // forefinger
  ANALOG3, // middle finger
  ANALOG4, // ring finger
  ANALOG5  // pinkie finger
};

const size_t NUM_SENSORS = sizeof(SENSOR_PINS) / sizeof(uint8_t);

uint8_t SENSOR_OUTPUT_CHAR_IDXS[NUM_SENSORS];
uint8_t lastSensorState[NUM_SENSORS];

const PINS MOTOR_PINS[] = {
  GPIO5,
  GPIO6,
  GPIO10,
  GPIO11,
  GPIO12
};

const size_t NUM_MOTORS = sizeof(MOTOR_PINS) / sizeof(uint8_t);
uint8_t motorCharIdx, lastMotorState;

void stop()
{
  ble.sendCommandCheckOK(F("AT+GAPDEVNAME=ERROR"));
  while(1)
  {
    if(Serial) Serial.println("ERROR STOP ERROR STOP!");
    delay(5000);
  }
}


uint8_t addChar(uint16_t id, const char* name, uint8_t size, uint8_t props)
{
  return gatt.addCharacteristic( id, props, size, size, BLE_DATATYPE_INTEGER, name );
}

char sensorName[9];
void setup(void)
{
#ifdef DEBUG
  Serial.begin(115200);
  while(!Serial);
  delay(500);
  Serial.println("OK!");
#endif

  check("Starting BLE", ble.begin(VERBOSE_MODE));
  // I don't know what echo is, but all the examples show disabling it.
  check("Disabling echo", ble.echo(false));
  // Put the Bluetooth chip into a known state.
  check("Factory reset", ble.factoryReset(true));
  // The factory reset does not clear the GATT flash.
  check("Clearing GATT", gatt.clear());

  check("Setting device name", ble.sendCommandCheckOK(F("AT+GAPDEVNAME=NotionTheory Haptic Glove")));

  check("Analog 0", ANALOG0 == A0);
  check("Analog 1", ANALOG1 == A1);
  check("Analog 2", ANALOG2 == A2);
  check("Analog 3", ANALOG3 == A3);
  check("Analog 4", ANALOG4 == A4);
  check("Analog 5", ANALOG5 == A5);
  check("Number of Sensors", NUM_SENSORS == 6);
  check("Number of Motors", NUM_MOTORS == 5);

  // Create a service onto which we can attach characteristics. This is the Battery service, which I am using because I wasn't able to get any other services to work. We don't need to save the service index because there is no point where we ever use it.
  check("Service", gatt.addService( 0x180F ) == 1);

  // Setup the characteristics for outputting the sensor values
  for(size_t i = 0; i < NUM_SENSORS; ++i) {

#ifdef DEBUG
    Serial.print("sensor index ");
    Serial.println(i);
#endif

    // This is the Analog characteristic, which is supposed to be a 16-bit integer, but I have it configured as an 8-bit integer, because this was the only configuration I was able to get to work reliably.
    sprintf(sensorName, "Sensor %d", i);
    SENSOR_OUTPUT_CHAR_IDXS[i] = addChar( 0x2A58, sensorName, 1, OUTPUT_PROPERTIES );
    check(sensorName, SENSOR_OUTPUT_CHAR_IDXS[i] == i + 1);
    // Make sure we don't have random garbage in the array.
    lastSensorState[i] = 0;
    // we don't need to configure a pin mode for these pins because they are analog inputs.
  }

  // Setup the characteristic for receiving the motor state. We use the `Write without Response` property because the host PC doesn't care when the write operation finishes, we just want it to happen as fast as possible.
  motorCharIdx = addChar( 0x2A58, "Motor State", 1, GATT_CHARS_PROPERTIES_WRITE_WO_RESP );
  check("Motor", motorCharIdx == NUM_SENSORS + 1);
  lastMotorState = 0;
  // setup the pins for outputting the motor state.
  for(size_t i = 0; i < NUM_MOTORS; ++i) {
    pinMode(MOTOR_PINS[i], OUTPUT);
  }

  // Setup complete. Reset the Bluetooth chip and go.
  ble.reset(true);
  ble.verbose(false);

#ifdef DEBUG
  Serial.println("Ready!");

  // Wait for a device to connect
  while(!ble.isConnected())
  {
    Serial.println("Waiting for connection.");
    delay(1000);
  }
#endif
}

void loop(void)
{
  // update the sensors
  for(size_t i = 0; i < NUM_SENSORS; ++i)
  {
    // read the sensor value, and convert from the range [0, 1024), to [0, 256)
    uint8_t value = (uint8_t)(analogRead(SENSOR_PINS[i]) / 4);
    // don't do anything if the value didn't change
    if(value != lastSensorState[i])
    {
      lastSensorState[i] = value;
      sprintf(sensorName, "Sensor %d", i);

#ifdef DEBUG
      Serial.print("Writing ");
      Serial.print(sensorName);
      Serial.print(" = ");
      Serial.print(value);
      Serial.print("...");
#endif

      gatt.setChar(SENSOR_OUTPUT_CHAR_IDXS[i], value);

#ifdef DEBUG
      delay(MIN_DELAY);
      Serial.println(" OK");
#endif
    }
  }

  // update the motors.
#ifdef DEBUG
  Serial.print("Reading motor state... ");
#endif

  uint8_t motorState = gatt.getCharInt8(motorCharIdx);

#ifdef DEBUG
  Serial.print(motorState);
  Serial.print("...");
  delay(MIN_DELAY);
  Serial.println("OK");
#endif

  if(motorState != lastMotorState)
  {
    lastMotorState = motorState;
    // the motor state is a bitfield, so we iterate over the bitfield destructively to be able to get at the individual values very quickly.
    for(size_t i = 0; i < NUM_MOTORS; ++i) {
      // check the least-significant bit for whether it's 0 or 1
      digitalWrite(MOTOR_PINS[i], motorState & 0x1);
      // shift the value over, setting up the next pin to be written.
      motorState = motorState >> 1;
    }
  }

#ifdef DEBUG
  Serial.println("Delay 1000 ms");
  delay(1000);
#endif
}
