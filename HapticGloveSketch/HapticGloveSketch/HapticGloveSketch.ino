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

// Only enable this the first time uploading a new sketch, then disable and upload again.
// #define FACTORY_RESET

#ifdef DEBUG
    #define VERBOSE_MODE true
#else
    #define VERBOSE_MODE false
#endif

// https://learn.adafruit.com/adafruit-feather-m0-bluefruit-le/pinouts
// Generally speaking, don't use any pins that are prefixed with `_`
enum PINS {
    // PA11
    _UART_RX,
    // PA10
    _UART_TX,
    _GPIO2_NOT_AVAILABLE, // included for completeness, these pins can't be used.
    _GPIO3_NOT_AVAILABLE, // included for completeness, these pins can't be used.
    // PA08
    _BLE_RST,
    // PA15
    GPIO5,
    // PA20
    GPIO6,
    // PA21
    _BLE_IRQ,
    // PA06
    _BLE_CS,
    // PA07
    GPIO9,
    // PA18
    GPIO10,
    // PA16
    GPIO11,
    // PA19
    GPIO12,
    // PA17
    GPIO13,
    // PA02
    ANALOG0,
    // PB08
    ANALOG1,
    // PB09
    ANALOG2,
    // PA04
    ANALOG3,
    // PA05
    ANALOG4,
    // PB02
    ANALOG5,
    // PA22
    GPIO20,
    // PA23
    GPIO21,
    // PA12
    _SPI_MISO,
    // PB10
    _SPI_MOSI,
    // PB11
    _SPI_SCK,


    BATTERY_LEVEL = GPIO9,
    ON_BOARD_LED = GPIO13,
    DAC_OUTPUT = ANALOG0,
    _I2C_SDA = GPIO20,
    _I2C_SCL = GPIO21
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

struct Characteristic
{
    const char* name;
    PINS pin;
    uint8_t charIdx;
    uint8_t value;
};

struct Finger
{
    Characteristic sensor;
    Characteristic motor;
    bool mayPWM;
};

Finger fingers[] = {
    { { "S0", ANALOG0, 0, 0 }, { "M0", GPIO5 /* PA15 */, 0, 0 }, false },
    { { "S1", ANALOG1, 0, 0 }, { "M1", GPIO6 /* PA21 */, 0, 0 }, false },
    { { "S2", ANALOG2, 0, 0 }, { "M2", GPIO10 /* PA18 */, 0, 0 }, true },
    { { "S3", ANALOG3, 0, 0 }, { "M3", GPIO11 /* PA16 */, 0, 0 }, true },
    { { "S4", ANALOG4, 0, 0 }, { "M4", GPIO12 /* PA19 */, 0, 0 }, true },
    // { { "S5", ANALOG5, 0, 0 }, { "M5", GPIO20 /* PA22 */, 0, 0, 0 } }
};

const size_t NUM_FINGERS = sizeof(fingers) / sizeof(Finger);

const int MOTOR_ON = HIGH;
const int MOTOR_OFF = LOW;

bool wasConnected = false;

void stop()
{
    ble.sendCommandCheckOK(F("AT+GAPDEVNAME=ERROR"));
    while(1)
    {
        Serial.println(F("ERROR STOP ERROR STOP!"));
        delay(5000);
    }
}

uint8_t addChar(const char* name, uint8_t props)
{
    return gatt.addCharacteristic( GATT_ANALOG_CHARACTERISTIC, props, 1, 1, BLE_DATATYPE_AUTO, name );
}

void setMotor(Finger* f, uint8_t value)
{
    Characteristic* m = &(f->motor);
    m->value = value;
    if(f->mayPWM) {
        analogWrite(m->pin, m->value);
    }
    else {
        digitalWrite(m->pin, m->value > 127 ? MOTOR_ON : MOTOR_OFF);
    }
}

void loop(void)
{
    bool connected = ble.isConnected();

    if(connected)
    {
        #ifdef DEBUG
            if(!wasConnected)
            {
                Serial.println(F("New device connected."));
            }
        #endif
        for(Finger* f = fingers; f < fingers + NUM_FINGERS; ++f)
        {
            setMotor(f, gatt.getCharInt8(f->motor.charIdx));
            f->sensor.value = (uint8_t)(analogRead(f->sensor.pin) >> 2);
            #ifdef DEBUG
              Serial.print(F("Sensor "));
              Serial.print(f->sensor.name);
              Serial.print(F(" = "));
              Serial.print(f->sensor.value);
              Serial.println();
            #endif
            gatt.setChar(f->sensor.charIdx, f->sensor.value);
        }
    }
    else if(wasConnected)
    {
        #ifdef DEBUG
            Serial.println(F("No device connected."));
        #endif
        for(Finger* f = fingers; f < fingers + NUM_FINGERS; ++f)
        {
            setMotor(f, 0);
        }
    }

    wasConnected = connected;

    #ifdef DEBUG
        delay(1000);
    #endif
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

    #ifdef FACTORY_RESET
        // Put the Bluetooth chip into a known state.
        Serial.println(F("Factory reset"));
        if(!ble.factoryReset(true)) {
           stop();
        }

        Serial.println(F("Setting device name"));
        if(!ble.sendCommandCheckOK(F("AT+GAPDEVNAME=NotionTheory Haptic Glove"))) {
            stop();
        }
    #endif

    // The factory reset does not clear the GATT flash.
    Serial.println(F("Clearing GATT"));
    if(!gatt.clear()) {
        stop();
    }

    // Create a service onto which we can attach characteristics. 0x180F is the Battery service, which I am using because I wasn't able to get any other services to work. We don't need to save the service index because there is no point where we ever use it. The full list of pre-defined services is available here: https://www.bluetooth.com/specifications/gatt/services. You can also create custom services, but I wasn't able to get that to work reliably.
    Serial.println(F("Service"));
    if(!gatt.addService( 0x180F ) == 1) {
        stop();
    }

    for(Finger* f = fingers; f < fingers + NUM_FINGERS; ++f)
    {
        // setup the pins for outputting the motor state.
        pinMode(f->motor.pin, OUTPUT);
        // Setup the characteristics for outputting the sensor values
        f->sensor.charIdx = addChar( f->sensor.name, OUTPUT_PROPERTIES );
        // Setup the characteristic for receiving the motor state. We use the `Write without Response` property because the host PC doesn't care when the write operation finishes, we just want it to happen as fast as possible.
        f->motor.charIdx = addChar( f->motor.name, INPUT_PROPERTIES );

        #ifdef DEBUG
            Serial.print(F("sensor name: "));
            Serial.print(f->sensor.name);
            Serial.print(F("+"));
            Serial.print(f->motor.name);
            Serial.print(F(", sensor char "));
            Serial.print(f->sensor.charIdx);
            Serial.print(F(", motor char "));
            Serial.print(f->motor.charIdx);
            Serial.println(".");
        #endif
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
}
