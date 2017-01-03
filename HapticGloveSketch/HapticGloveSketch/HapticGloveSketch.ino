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
    #define VERBOSE_MODE true
#else
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
    ANALOG0, // thumb
    ANALOG1, // forefinger
    ANALOG2, // middle finger
    ANALOG3, // ring finger
    ANALOG4,  // pinkie finger
    // ANALOG5 // wrist
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
uint8_t motorCharIdx, lastMotorState, batteryLevelCharIndex, lastBatteryLevel;

void stop()
{
    ble.sendCommandCheckOK(F("AT+GAPDEVNAME=ERROR"));
    while(1)
    {
        Serial.println(F("ERROR STOP ERROR STOP!"));
        delay(5000);
    }
}

void setup(void)
{
    #ifdef DEBUG
        Serial.begin(115200);
        while(!Serial);
        delay(500);
    #endif

    Serial.println(F("Starting BLE"));
    if(!ble.begin(VERBOSE_MODE)) {
        stop();
    }

    Serial.println(F("Disabling echo"));
    if(!ble.echo(false)) {
        stop();
    }

    // Put the Bluetooth chip into a known state.
    Serial.println(F("Factory reset"));
    if(!ble.factoryReset(true)) {
       stop();
    }

    // The factory reset does not clear the GATT flash.
    Serial.println(F("Clearing GATT"));
    if(!gatt.clear()) {
        stop();
    }

    Serial.println(F("Setting device name"));
    if(!ble.sendCommandCheckOK(F("AT+GAPDEVNAME=NotionTheory Haptic Glove"))) {
        stop();
    }

    // Create a service onto which we can attach characteristics. 0x180F is the Battery service, which I am using because I wasn't able to get any other services to work. We don't need to save the service index because there is no point where we ever use it. The full list of pre-defined services is available here: https://www.bluetooth.com/specifications/gatt/services. You can also create custom services, but I wasn't able to get that to work reliably.
    Serial.println(F("Service"));
    if(!gatt.addService( 0x180F ) == 1) {
        stop();
    }

    // Tell the host computer how many haptic motors we have available.
    const uint8_t motorCountCharIdx = gatt.addCharacteristic( 0x2A58, OUTPUT_PROPERTIES, 1, 1, BLE_DATATYPE_INTEGER, "Motor Count" );
    // Setup the characteristic for receiving the motor state. We use the `Write without Response` property because the host PC doesn't care when the write operation finishes, we just want it to happen as fast as possible.
    motorCharIdx = gatt.addCharacteristic( 0x2A58, OUTPUT_PROPERTIES, 1, 1, BLE_DATATYPE_INTEGER, "Motor State" );
    lastMotorState = 0;

    // Setup the characteristic for reporting the battery level.
    batteryLevelCharIndex = gatt.addCharacteristic( 0x2A19, OUTPUT_PROPERTIES, 1, 1, BLE_DATATYPE_INTEGER );
    lastBatteryLevel = 0;

    // setup the pins for outputting the motor state.
    for(size_t i = 0; i < NUM_MOTORS; ++i) {
        pinMode(MOTOR_PINS[i], OUTPUT);
    }

    char sensorName[9];
    // Setup the characteristics for outputting the sensor values
    for(size_t i = 0; i < NUM_SENSORS; ++i) {
        sprintf(sensorName, "Sensor %d", i);

        Serial.print(F("sensor index "));
        Serial.print(i);
        Serial.print(F(", name: "));
        Serial.println(sensorName);

        SENSOR_OUTPUT_CHAR_IDXS[i] = gatt.addCharacteristic( 0x2A58, OUTPUT_PROPERTIES, 1, 1, BLE_DATATYPE_INTEGER, sensorName );

        // Make sure we don't have random garbage in the array.
        lastSensorState[i] = 0;
        // we don't need to configure a pin mode for these pins because they are analog inputs.
    }

    delay(500);

    // Setup complete. Reset the Bluetooth chip and go.
    ble.reset(true);

    delay(500);

    #ifdef DEBUG
        ble.verbose(false);
        Serial.println(F("Ready!"));

        // Wait for a device to connect
        while(!ble.isConnected())
        {
            Serial.println(F("Waiting for connection."));
            delay(1000);
        }
    #endif

    gatt.setChar(motorCountCharIdx, (uint8_t)NUM_MOTORS);
}

void loop(void)
{
    // read the battery level, and convert from the range [0, 1023], to [0, 100]
    uint16_t batteryLevel = analogRead(BATTERY_LEVEL);
    // performing the conversion in this way avoids precision loss while also avoiding integer overflow.
    batteryLevel *= 25;
    batteryLevel /= 256;
    if(batteryLevel != lastBatteryLevel)
    {
        lastBatteryLevel = batteryLevel;
        gatt.setChar(batteryLevelCharIndex, (uint8_t)batteryLevel);
    }

    // update the sensors
    for(size_t i = 0; i < NUM_SENSORS; ++i)
    {
        // read the sensor value, and convert from the range [0, 1024), to [0, 256)
        uint8_t value = (uint8_t)(analogRead(SENSOR_PINS[i]) / 4);
        // don't do anything if the value didn't change
        if(value != lastSensorState[i])
        {
            lastSensorState[i] = value;

            Serial.print(F("Writing Sensor "));
            Serial.print(i);
            Serial.print(F(" = "));
            Serial.print(value);
            Serial.println(F(" ... "));

            gatt.setChar(SENSOR_OUTPUT_CHAR_IDXS[i], value);
        }
    }

    Serial.print(F("Reading motor state... "));
    uint8_t motorState = gatt.getCharInt8(motorCharIdx);
    Serial.println(motorState);

    if(motorState != lastMotorState)
    {
        lastMotorState = motorState;
        // the motor state is a bitfield, so we iterate over the bitfield destructively to be able to get at the individual values very quickly.
        for(size_t i = 0; i < NUM_MOTORS; ++i)
        {
            // check the least-significant bit for whether it's 0 or 1
            digitalWrite(MOTOR_PINS[i], motorState & 0x1);
            // shift the value over, setting up the next pin to be written.
            motorState = motorState >> 1;
        }
    }

    #ifdef DEBUG
        Serial.println(F("Delay 1000 ms"));
        delay(1000);
    #endif
}
