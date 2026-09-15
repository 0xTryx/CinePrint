#include <SPI.h>
#include <Ethernet.h>

byte mac[] = { 0xA8, 0x61, 0x0A, 0xAE, 0x83, 0x9C };
IPAddress ip(172, 18, 197, 99);
EthernetServer server(8080);

String bufferTCP = "";

String traiterCommande(String trame) {
  trame.trim();
  if (trame.length() == 0) return "";

  int premierVirgule = trame.indexOf(',');
  if (premierVirgule == -1) return "";

  int instruction = trame.substring(0, premierVirgule).toInt();

  if (instruction == 0) {
    String payload = trame.substring(premierVirgule + 1);
    payload.trim();
    if (payload == "ping") return "0,pong";
  }
  else if (instruction == 1) {
    String texte = trame.substring(premierVirgule + 1);
    Serial.println(texte);
    return "1,ok";
  }
  else if (instruction == 2) {
    int deuxiemeVirgule = trame.indexOf(',', premierVirgule + 1);
    if (deuxiemeVirgule == -1) return "";

    String donneesHexa = trame.substring(deuxiemeVirgule + 1);
    int indexDebut = 0;
    int octetsEnvoyes = 0;
    int lenStr = donneesHexa.length();

    for (int i = 0; i <= lenStr; i++) {
      if (i == lenStr || donneesHexa.charAt(i) == ';') {
        if (i > indexDebut) {
          String octetStr = donneesHexa.substring(indexDebut, i);
          octetStr.trim();
          if (octetStr.length() > 0) {
            uint8_t octetVal = (uint8_t) strtol(octetStr.c_str(), NULL, 16);
            Serial.write(octetVal);
            octetsEnvoyes++;
          }
        }
        indexDebut = i + 1;
      }
    }
    return "2," + String(octetsEnvoyes) + ",ok";
  }

  return "";
}

void setup() {
  Serial.begin(9600);

  // ------------------------------------------------------------------
  // OPTIMISATION DE LA QUALITÉ D'IMPRESSION (ESC 7 n1 n2 n3)
  // ------------------------------------------------------------------
  // n1 = 0x09 : Maximum de points chauffés simultanément (80 points)
  // n2 = 0xA0 : Temps de chauffe élevé (1600 µs au lieu de 800 µs) -> ralentit l'impression mais fonce le noir
  // n3 = 0x0A : Temps d'intervalle augmenté (100 µs) -> évite de baver et rend les contours très nets
  uint8_t initHeating[] = { 0x1B, 0x37, 0x09, 0xA0, 0x0A };
  Serial.write(initHeating, 5);

  Ethernet.begin(mac, ip);
  server.begin();

  bufferTCP.reserve(256);
}

void loop() {
  EthernetClient client = server.available();

  if (client) {
    while (client.connected()) {
      while (client.available()) {
        char c = client.read();
        if (c == '\n') {
          String reponse = traiterCommande(bufferTCP);
          if (reponse.length() > 0) {
            client.println(reponse);
          }
          bufferTCP = "";
        } else if (c != '\r') {
          bufferTCP += c;
        }
      }
    }
    client.stop();
  }
}
