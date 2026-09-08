#include <SPI.h>
#include <Ethernet.h>

// -------------------------------------------------------------
// CONFIGURATION RÉSEAU
// -------------------------------------------------------------
// Remplacez l'adresse MAC par celle inscrite sur votre shield Ethernet
byte mac[] = { 0xAE, 0x61, 0x0A, 0xAF, 0x87, 0xC6 };

// Adresse IP fixe attribuée à la carte Arduino
IPAddress ip(172, 18, 197, 99);

// Port d'écoute du serveur TCP (ex: 8080 ou 23)
EthernetServer server(8080);

// Buffer pour accumuler la ligne reçue via TCP
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

  // Instruction 0 : Ping / Pong
  if (instruction == 0) {
    String payload = trame.substring(premierVirgule + 1);
    payload.trim();
    if (payload == "ping") {
      return "0,pong";
    }
  }

  // Instruction 1 : socket.println(...)
  // Format : 1,texte_a_imprimer
  // Réponse TCP : 1,ok
  else if (instruction == 1) {
    String texte = trame.substring(premierVirgule + 1);
    Serial.println(texte); // Imprime le texte + saut de ligne (LF)
    return "1,ok";
  }

  // Instruction 2 : socket.write(...)
  // Format : 2,longueur,byte1;byte2;...
  // Réponse TCP : 2,longueur,ok
  else if (instruction == 2) {
    int deuxiemeVirgule = trame.indexOf(',', premierVirgule + 1);
    if (deuxiemeVirgule == -1) return "";

    int longueur = trame.substring(premierVirgule + 1, deuxiemeVirgule).toInt();
    String donneesHexa = trame.substring(deuxiemeVirgule + 1);

    uint8_t bufferOut[longueur];
    int indexData = 0;
    int indexDebut = 0;

    // Découpage des valeurs hexadécimales séparées par ';'
    for (int i = 0; i <= donneesHexa.length() && indexData < longueur; i++) {
      if (i == donneesHexa.length() || donneesHexa.charAt(i) == ';') {
        String octetStr = donneesHexa.substring(indexDebut, i);
        bufferOut[indexData] = (uint8_t) strtol(octetStr.c_str(), NULL, 16);
        indexData++;
        indexDebut = i + 1;
      }
    }

    // Envoi des octets bruts à l'imprimante thermique via Serial
    Serial.write(bufferOut, indexData);
    
    return "2," + String(longueur) + ",ok";
  }

  return "";
}

// -------------------------------------------------------------
// SETUP & LOOP
// -------------------------------------------------------------
void setup() {
  // Initialisation de la communication avec l'imprimante thermique (ex: 9600 baud)
  Serial.begin(9600);

  // Initialisation du shield Ethernet
  Ethernet.begin(mac, ip);

  // Lancement du serveur TCP
  server.begin();
}

void loop() {
  // Écoute des clients TCP entrants
  EthernetClient client = server.available();

  if (client) {
    while (client.connected()) {
      if (client.available()) {
        char c = client.read();

        // Réception caractère par caractère jusqu'à la fin de ligne
        if (c == '\n') {
          // Traitement de la commande accumulée
          String reponse = traiterCommande(bufferTCP);

          // Si une réponse est générée, on l'envoie au client TCP
          if (reponse.length() > 0) {
            client.println(reponse);
          }

          // Réinitialisation du buffer
          bufferTCP = "";
        } else if (c != '\r') {
          bufferTCP += c;
        }
      }
    }

    // Fermeture du socket lorsque le client se déconnecte
    client.stop();
  }
}
