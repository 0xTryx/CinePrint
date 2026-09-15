#include <SPI.h>
#include <Ethernet.h>

// -------------------------------------------------------------
// CONFIGURATION RÉSEAU
// -------------------------------------------------------------
byte mac[] = { 0xA8, 0x61, 0x0A, 0xAE, 0x83, 0x9C };
IPAddress ip(172, 18, 197, 99);
EthernetServer server(8080);

String bufferTCP = "";

// -------------------------------------------------------------
// FONCTION DE TRAITEMENT ET DE RÉPONSE PROTOCOLE
// -------------------------------------------------------------
String traiterCommande(String trame) {
  trame.trim();
  if (trame.length() == 0) return "";

  int premierVirgule = trame.indexOf(',');
  if (premierVirgule == -1) return "";

  int instruction = trame.substring(0, premierVirgule).toInt();

  // -----------------------------------------------------------
  // Instruction 0 : Ping / Pong
  // -----------------------------------------------------------
  if (instruction == 0) {
    String payload = trame.substring(premierVirgule + 1);
    payload.trim();
    if (payload == "ping") {
      return "0,pong";
    }
  }

  // -----------------------------------------------------------
  // Instruction 1 : socket.println(...)
  // Format : 1,texte_a_imprimer
  // Réponse TCP : 1,ok
  // -----------------------------------------------------------
  else if (instruction == 1) {
    String texte = trame.substring(premierVirgule + 1);
    Serial.println(texte); // Imprime le texte + saut de ligne (LF)
    return "1,ok";
  }

  // -----------------------------------------------------------
  // Instruction 2 : socket.write(...) - Compatible texte & IMAGES
  // Format : 2,longueur,1B;40;1D;76;30;00;...
  // Réponse TCP : 2,longueur,ok
  // -----------------------------------------------------------
  else if (instruction == 2) {
    int deuxiemeVirgule = trame.indexOf(',', premierVirgule + 1);
    if (deuxiemeVirgule == -1) return "";

    int longueurAttendue = trame.substring(premierVirgule + 1, deuxiemeVirgule).toInt();
    String donneesHexa = trame.substring(deuxiemeVirgule + 1);

    int indexDebut = 0;
    int octetsEnvoyes = 0;
    int lenStr = donneesHexa.length();

    // Traitement au fil de l'eau (Streaming) :
    // Chaque octet reçu en Hexa est converti et immédiatement envoyé à l'imprimante
    for (int i = 0; i <= lenStr; i++) {
      if (i == lenStr || donneesHexa.charAt(i) == ';') {
        if (i > indexDebut) {
          String octetStr = donneesHexa.substring(indexDebut, i);
          octetStr.trim();
          if (octetStr.length() > 0) {
            uint8_t octetVal = (uint8_t) strtol(octetStr.c_str(), NULL, 16);
            
            // Envoi immédiat vers l'imprimante
            Serial.write(octetVal);
            octetsEnvoyes++;
          }
        }
        indexDebut = i + 1;
      }
    }

    // Réponse au client avec la longueur réelle des octets envoyés
    return "2," + String(octetsEnvoyes) + ",ok";
  }

  return "";
}

// -------------------------------------------------------------
// SETUP & LOOP
// -------------------------------------------------------------
void setup() {
  // Communication avec l'imprimante thermique (9600 baud par défaut)
  Serial.begin(9600);

  // Initialisation du shield Ethernet
  Ethernet.begin(mac, ip);

  // Lancement du serveur TCP
  server.begin();
}

void loop() {
  EthernetClient client = server.available();

  if (client) {
    while (client.connected()) {
      if (client.available()) {
        char c = client.read();

        // Réception jusqu'à la fin de la ligne
        if (c == '\n') {
          String reponse = traiterCommande(bufferTCP);

          if (reponse.length() > 0) {
            client.println(reponse);
          }

          bufferTCP = ""; // Réinitialisation pour la trame suivante
        } else if (c != '\r') {
          bufferTCP += c;
        }
      }
    }

    client.stop();
  }
}
