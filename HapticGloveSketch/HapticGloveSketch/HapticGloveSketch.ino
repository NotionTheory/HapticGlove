#include <Arduino.h>
#include <SPI.h>
#if not defined (_VARIANT_ARDUINO_DUE_X_) && not defined (_VARIANT_ARDUINO_ZERO_)
    #include <SoftwareSerial.h>
#endif

#include "Adafruit_BLE.h"
#include "Adafruit_BluefruitLE_SPI.h"
#include "Adafruit_BLEGatt.h"
#include "BluefruitConfig.h"

// #define DEBUG

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
    _SPI_SCK,
    GPIO9 = BATTERY_LEVEL,
    GPIO13 = ON_BOARD_LED,
    DAC_OUTPUT = ANALOG0,
    GPIO20 = _I2C_SDA,
    GPIO21 = _I2C_SCL
};

// The API for basic Bluetooth LE operation, including Generic Access Profile, used to initialize the device and set the device name .
Adafruit_BluefruitLE_SPI ble(BLUEFRUIT_SPI_CS, BLUEFRUIT_SPI_IRQ, BLUEFRUIT_SPI_RST);

// The API for Generic Attribute Profile, used to specify services and characteristics.
Adafruit_BLEGatt gatt(ble);

// The `Analog` characteristic
const uint16_t GATT_ANALOG_CHARACTERISTIC = 0x2A58;

// These are the properties used on the sensor output characteristics.
const uint8_t OUTPUT_PROPERTIES = GATT_CHARS_PROPERTIES_READ | GATT_CHARS_PROPERTIES_NOTIFY;

const uint8_t INPUT_PROPERTIES = GATT_CHARS_PROPERTIES_READ | GATT_CHARS_PROPERTIES_WRITE | GATT_CHARS_PROPERTIES_WRITE_WO_RESP;

struct Finger {
    const char* name;
    PINS sensorPin;
    PINS motorPin;
    uint8_t outputCharIdx;
};

Finger FINGERS[] = {
    { "Thumb", ANALOG0, GPIO5, 0 },
    { "Index", ANALOG1, GPIO6, 0 },
    { "Middle", ANALOG2, GPIO10, 0 },
    { "Ring", ANALOG3, GPIO11, 0 },
    { "Pinkie", ANALOG4, GPIO12, 0 },
    // { "Wrist", ANALOG5, GPIO20, 0 }
};

const size_t NUM_FINGERS = sizeof(FINGERS) / sizeof(Finger);

const int MOTOR_ON = HIGH;
const int MOTOR_OFF = LOW;

uint8_t motorCharIdx;

void stop()
{
    ble.sendCommandCheckOK(F("AT+GAPDEVNAME=ERROR"));
    while(1)
    {
        Serial.println(F("ERROR STOP ERROR STOP!"));
        delay(5000);
    }
}

void setMotorState(uint8_t motorState) {

    #ifdef DEBUG
        Serial.print(F("Motor state = "));
        Serial.println(motorState);
    #endif

    // the motor state is a bitfield, so we iterate over the bitfield destructively to be able to get at the individual values very quickly.
    for(size_t i = 0; i < NUM_FINGERS; ++i)
    {
        // check the least-significant bit for whether it's 0 or 1
        digitalWrite(FINGERS[i].motorPin, motorState & 0x1 ? MOTOR_ON : MOTOR_OFF);
        // shift the value over, setting up the next pin to be written.
        motorState = motorState >> 1;
    }

}

uint8_t addChar(const char* name, uint8_t props) {
    return gatt.addCharacteristic( GATT_ANALOG_CHARACTERISTIC, props, 1, 2, BLE_DATATYPE_AUTO, name );
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
    const uint8_t motorCountCharIdx = addChar( "Motor Count", OUTPUT_PROPERTIES );


    // Setup the characteristic for receiving the motor state. We use the `Write without Response` property because the host PC doesn't care when the write operation finishes, we just want it to happen as fast as possible.
    motorCharIdx = addChar( "Motor State", INPUT_PROPERTIES );


    // setup the pins for outputting the motor state.
    for(size_t i = 0; i < NUM_FINGERS; ++i) {
        pinMode(FINGERS[i].motorPin, OUTPUT);
        digitalWrite(FINGERS[i].motorPin, MOTOR_OFF);
    }


    // Setup the characteristics for outputting the sensor values
    for(size_t i = 0; i < NUM_FINGERS; ++i) {
        Serial.print(F("sensor index "));
        Serial.print(i);
        Serial.print(F(", name: "));
        Serial.println(FINGERS[i].name);

        // Make sure we don't have random garbage in the array.
        FINGERS[i].outputCharIdx = addChar( FINGERS[i].name, OUTPUT_PROPERTIES );

        // we don't need to configure a pin mode for these pins because they are analog inputs.
    }

    delay(500);

    // Setup complete. Reset the Bluetooth chip and go.
    ble.reset(true);

    delay(500);

    gatt.setChar(motorCountCharIdx, (uint16_t)NUM_FINGERS);

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
}

void loop(void)
{
    if(ble.isConnected())
    {
        // update the fingers
        for(size_t i = 0; i < NUM_FINGERS; ++i)
        {
            uint16_t value = (uint16_t)analogRead(FINGERS[i].sensorPin);
            bool success = gatt.setChar(FINGERS[i].outputCharIdx, value);
            #ifdef DEBUG
                Serial.print(FINGERS[i].name);
                Serial.print(F(" = "));
                Serial.print(value);
                Serial.print(success ? "." : "!");
                Serial.println();
            #endif
        }

        setMotorState(gatt.getCharInt8(motorCharIdx));
    }
    else
    {
        #ifdef DEBUG
            Serial.println(F(" no device connected."));
        #endif
    }

    #ifdef DEBUG
        delay(1000);
    #endif
}
