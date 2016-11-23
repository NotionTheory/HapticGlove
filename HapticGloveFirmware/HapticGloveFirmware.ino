void printWord(const char* name, const char* buff, const size_t len){
  static char output[5];
  Serial.print(name);
  Serial.print(": ");
  memset(output, 0, 5);
  memcpy(output, buff, len);
  Serial.println(output); 
}

void printNumber(const char* name, const int32_t number){
  Serial.print(name);
  Serial.print(": ");
  Serial.println(number);
}

void setup() {
  pinMode(A0, OUTPUT);
  pinMode(A4, OUTPUT);
  digitalWrite(A4, LOW);
  //while(!Serial);
}

size_t s = 0,
   t = 0;

int state = 0;

void loop() {
  digitalWrite(A0, state);
  if(s > t){
    s = 0;
    state = !state;
    t = analogRead(A1);
    //printNumber("A1", t);
  }
  ++s;
}
