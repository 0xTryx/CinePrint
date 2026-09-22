# 🎬 CinePrint

CinePrint est un système de gestion et d'impression de tickets pour cinéma utilisant une imprimante thermique compatible **ESC/POS**.

Le projet permet de piloter l'imprimante depuis une application Windows à travers le réseau :

```text
Application CinePrint
        │
        │ TCP
        ▼
Arduino + Ethernet
        │
        │ Série / ESC-POS
        ▼
Imprimante thermique
```

L'Arduino agit comme une passerelle entre le réseau Ethernet et l'imprimante thermique.

---

## ✨ Fonctionnalités

### Application Windows

L'application `CinePrintApp` permet de :

- se connecter à l'Arduino par TCP ;
- tester la connexion avec un ping ;
- créer et imprimer un ticket de cinéma ;
- choisir le cinéma, le film, la salle et l'horaire ;
- imprimer du texte formaté en ESC/POS ;
- charger et imprimer une image ;
- convertir automatiquement une image en noir et blanc ;
- utiliser un dithering Floyd-Steinberg pour les images ;
- générer un QR code depuis un lien ou un texte ;
- afficher un aperçu du QR code avant impression ;
- régler la largeur d'impression ;
- régler la taille des paquets envoyés à l'Arduino ;
- envoyer manuellement une commande du protocole ;
- consulter les échanges avec l'Arduino dans un journal.

### Arduino

L'Arduino :

- écoute les connexions TCP sur le port `8080` ;
- reçoit les commandes envoyées par l'application ;
- interprète le protocole CinePrint ;
- transmet le texte et les commandes ESC/POS à l'imprimante ;
- renvoie une réponse au client après traitement.

---

# 🏗️ Architecture

```text
CinePrint/
│
├── Arduino.cpp
│   └── Serveur TCP + passerelle série vers l'imprimante
│
├── CinePrintApp/
│   ├── Form1.cs
│   ├── Form1.Designer.cs
│   ├── EscPosImage.cs
│   ├── EscPosQrCode.cs
│   ├── ProtocoleCinePrint.cs
│   ├── Program.cs
│   └── cineprint.csproj
│
├── app/
│   └── Prototype d'interface Web Node.js
│
├── emu.js
│   └── Émulateur du serveur Arduino en Node.js
│
├── emulator.py
│   └── Client Python de test du protocole
│
└── README.md
```

---

# 🖥️ Application CinePrint

L'application principale est développée en **C# / Windows Forms**.

### Technologies

- .NET 10
- Windows Forms
- TCP/IP
- ESC/POS
- QRCoder
- System.Drawing

Le projet se trouve dans :

```text
CinePrintApp/
```

Pour lancer l'application :

```bash
dotnet restore CinePrintApp/cineprint.csproj
dotnet run --project CinePrintApp/cineprint.csproj
```

Il est également possible d'ouvrir directement le projet avec Visual Studio.

---

# 🌐 Communication

Par défaut, l'Arduino écoute sur :

```text
Port : 8080
```

L'application ouvre une connexion TCP vers l'adresse IP configurée dans l'interface.

Chaque commande envoyée est terminée par :

```text
\n
```

Le protocole CinePrint repose actuellement sur trois instructions :

| Instruction | Fonction |
|---|---|
| `0` | Test de connexion |
| `1` | Impression de texte |
| `2` | Envoi d'octets bruts |

---

# 📡 Protocole CinePrint

## Instruction 0 — Ping

Permet de vérifier que l'Arduino est accessible.

### Requête

```text
0,ping
```

### Réponse

```text
0,pong
```

---

## Instruction 1 — Println

Permet d'envoyer directement du texte à l'imprimante.

### Format

```text
1,<texte>
```

### Exemple

```text
1,test aaa 1234567890
```

L'Arduino transmet le texte à l'imprimante via :

```cpp
Serial.println(texte);
```

### Réponse

```text
1,ok
```

Tout ce qui se trouve après la première virgule est considéré comme le texte à imprimer.

---

## Instruction 2 — Write

Permet d'envoyer des octets bruts à l'imprimante.

Cette instruction est notamment utilisée pour les commandes **ESC/POS**, les images et les QR codes.

### Format

```text
2,<nombre_octets>,<octet1>;<octet2>;<octet3>;...
```

Les octets sont représentés en hexadécimal.

### Exemple

```text
2,2,1B;40
```

correspond à :

```cpp
Serial.write(0x1B);
Serial.write(0x40);
```

`1B 40` correspond à la commande ESC/POS :

```text
ESC @
```

qui initialise l'imprimante.

### Réponse

```text
2,2,ok
```

Le premier `2` correspond au numéro de l'instruction.

Le deuxième nombre correspond au nombre d'octets effectivement transmis à l'imprimante.

---

# 🧾 Impression d'un ticket

L'application peut générer automatiquement un ticket contenant notamment :

```text
        NOM DU CINÉMA

          Nom du film

          Salle 3
     22/09/2026 à 20h30
```

Le formatage est réalisé avec différentes commandes ESC/POS :

- initialisation ;
- alignement ;
- taille du texte ;
- gras ;
- saut de lignes.

---

# 🖼️ Impression d'images

Les images sont converties avant leur envoi à l'imprimante.

Le traitement utilisé est :

```text
Image originale
      │
      ▼
Redimensionnement
      │
      ▼
Niveaux de gris
      │
      ▼
Dithering Floyd-Steinberg
      │
      ▼
Bitmap 1 bit
      │
      ▼
ESC/POS GS v 0
      │
      ▼
Découpage en trames
      │
      ▼
Arduino
      │
      ▼
Imprimante
```

La commande ESC/POS utilisée pour le raster est :

```text
GS v 0
```

soit :

```text
1D 76 30 00 xL xH yL yH [pixels]
```

Pour une imprimante thermique 58 mm équipée d'une tête de 384 points, une largeur de :

```text
384
```

permet d'utiliser toute la largeur disponible.

---

# 🔳 QR codes

Les QR codes sont générés directement dans l'application avec **QRCoder**.

Le contenu peut être :

- une URL ;
- du texte ;
- un identifiant ;
- toute chaîne compatible avec un QR code.

Le QR code est transformé en une matrice noir/blanc puis converti dans le même format raster ESC/POS que les images.

```text
Texte / URL
    │
    ▼
QRCoder
    │
    ▼
Matrice noir / blanc
    │
    ▼
Bitmap ESC/POS
    │
    ▼
Découpage
    │
    ▼
TCP
    │
    ▼
Arduino
    │
    ▼
Imprimante
```

Contrairement aux photos, aucun dithering n'est appliqué au QR code afin de conserver des modules parfaitement nets.

---

# 📦 Découpage en trames

Les données ESC/POS peuvent être relativement volumineuses.

Pour éviter d'envoyer une commande trop importante à l'Arduino en une seule fois, CinePrint découpe les données.

Exemple avec :

```text
Octets par trame : 32
```

Une commande de 3200 octets sera envoyée en environ :

```text
100 trames
```

Chaque trame utilise l'instruction `2`.

Exemple :

```text
2,32,1D;76;30;00;30;00;...
```

La taille des trames influence principalement le transport réseau et l'utilisation de la mémoire de l'Arduino.

Elle ne modifie pas la qualité finale de l'image.

---

# 🖨️ ESC/POS

CinePrint utilise notamment les commandes suivantes :

| Commande | Hexadécimal | Fonction |
|---|---|---|
| `ESC @` | `1B 40` | Initialisation |
| `ESC a n` | `1B 61 n` | Alignement |
| `ESC E n` | `1B 45 n` | Gras |
| `GS ! n` | `1D 21 n` | Taille du texte |
| `ESC d n` | `1B 64 n` | Avance papier |
| `GS v 0` | `1D 76 30` | Impression raster |

---

# 🔥 Réglage de l'imprimante thermique

Au démarrage, l'Arduino configure les paramètres de chauffe de l'imprimante avec :

```text
ESC 7 n1 n2 n3
```

Configuration actuelle :

```text
1B 37 09 A0 0A
```

Ces paramètres permettent d'ajuster notamment :

- le nombre de points chauffés simultanément ;
- le temps de chauffe ;
- l'intervalle entre les chauffes.

L'objectif est d'obtenir un noir suffisamment intense tout en conservant des contours propres.

---

# 🧪 Émulateur

Le projet contient également un émulateur Node.js :

```text
emu.js
```

Il permet de tester l'application sans utiliser l'Arduino.

Lancer l'émulateur :

```bash
node emu.js
```

Il écoute sur :

```text
localhost:8080
```

Il comprend les mêmes principales commandes que l'Arduino :

```text
0,ping
1,<texte>
2,<taille>,<hex>
```

---

# 🐍 Client de test Python

`emulator.py` permet d'effectuer rapidement quelques tests du protocole depuis Python.

Il teste notamment :

```text
0,ping
```

```text
1,test aaa 1234567890
```

et :

```text
2,2,1B;40
```

Lancement :

```bash
python emulator.py
```

---

# 🔧 Matériel

Le projet est conçu autour de :

- Arduino ;
- shield/module Ethernet compatible avec la bibliothèque `Ethernet` ;
- imprimante thermique série compatible ESC/POS ;
- ordinateur Windows pour CinePrintApp.

Communication :

```text
PC ──TCP/Ethernet──> Arduino ──Serial──> Imprimante
```

La communication série est actuellement configurée à :

```text
9600 bauds
```

---

# 🚧 État du projet

CinePrint est actuellement en développement.

Fonctionnalités déjà présentes :

- [x] Serveur TCP Arduino
- [x] Ping réseau
- [x] Impression texte
- [x] Envoi d'octets ESC/POS
- [x] Interface Windows
- [x] Génération de tickets
- [x] Impression d'images
- [x] Dithering d'images
- [x] Génération de QR codes
- [x] Aperçu des QR codes
- [x] Découpage des images en petites trames
- [x] Émulateur TCP
- [ ] Gestion avancée des erreurs réseau
- [ ] Gestion d'acquittement trame par trame
- [ ] Configuration automatique de l'imprimante
- [ ] Sauvegarde de modèles de tickets

---

# 📌 Exemple complet

### Vérification de la connexion

Client :

```text
0,ping
```

Arduino :

```text
0,pong
```

### Impression texte

Client :

```text
1,CinePrint
```

Arduino :

```text
1,ok
```

### Envoi d'une commande ESC/POS

Client :

```text
2,2,1B;40
```

Arduino :

```text
2,2,ok
```

---

## 👨‍💻 Développement

Projet réalisé dans le cadre du développement d'un système d'impression réseau pour cinéma.

Contributions et améliorations bienvenues.
