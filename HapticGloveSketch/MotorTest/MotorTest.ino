#include <Arduino.h>
#include <SPI.h>
#if not defined (_VARIANT_ARDUINO_DUE_X_) && not defined (_VARIANT_ARDUINO_ZERO_)
    #include <SoftwareSerial.h>
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

struct Finger {
    const char* name;
    const PINS sensorPin;
    const PINS motorPin;
};

Finger FINGERS[] = {
    { "Thumb", ANALOG0, GPIO5 },
    { "Index", ANALOG1, GPIO6 },
    { "Middle", ANALOG2, GPIO10 },
    { "Ring", ANALOG3, GPIO11 },
    { "Pinkie", ANALOG4, GPIO12 }
};

const size_t NUM_FINGERS = sizeof(FINGERS) / sizeof(Finger);

const int MOTOR_ON = HIGH;
const int MOTOR_OFF = LOW;

int tick;

void loop(void)
{

    for(size_t i = 0; i < NUM_FINGERS; ++i)
    {
        digitalWrite(
            FINGERS[i].motorPin,
            tick > 12);
    }

    tick = (tick + 1) % 15;
}


void setup(void)
{
    tick = 0;
    for(size_t i = 0; i < NUM_FINGERS; ++i)
    {
        Finger *f = &FINGERS[i];
        // setup the pins for outputting the motor state.
        pinMode(f->motorPin, OUTPUT);
        digitalWrite(f->motorPin, MOTOR_OFF);
    }
}
