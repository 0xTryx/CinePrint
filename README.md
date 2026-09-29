# 🎬 CinePrint

**CinePrint** permet de créer et d'imprimer des tickets de cinéma sur une imprimante thermique ESC/POS depuis une application Windows.

```text
Application Windows ── TCP ──> Arduino Ethernet ── Série ──> Imprimante thermique
```

L'Arduino reçoit les commandes sur le réseau, les transmet à l'imprimante et renvoie une réponse à l'application.

## ✨ Fonctionnalités

- Création de tickets avec cinéma, film, salle et horaire.
- Impression de texte, d'images et de QR codes.
- Conversion des images en noir et blanc avec dithering Floyd–Steinberg.
- Aperçu des QR codes, réglage de la largeur et de la taille des trames.
- Test de connexion, envoi manuel de commandes et journal des échanges.

## 🚀 Démarrage

**Prérequis :** Windows, .NET 10, un Arduino avec Ethernet et une imprimante thermique série compatible ESC/POS.

Ouvrez `CinePrintApp/cineprint.csproj` dans Visual Studio, ou lancez :

```bash
dotnet run --project CinePrintApp/cineprint.csproj
```

Configurez ensuite dans l'application l'adresse IP de l'Arduino. Le serveur écoute sur le port **8080** et la liaison série avec l'imprimante est configurée à **9600 bauds**.

Pour tester sans matériel, démarrez l'émulateur TCP avec `node emu.js`, puis connectez l'application à `localhost:8080`.

## 📡 Protocole

Les commandes sont envoyées en TCP, une par ligne. Les octets bruts sont encodés en hexadécimal et séparés par des points-virgules.

| Action | Commande | Réponse |
| --- | --- | --- |
| Tester la connexion | `0,ping` | `0,pong` |
| Imprimer du texte | `1,<texte>` | `1,ok` |
| Envoyer des octets ESC/POS | `2,<taille>,<hex>` | `2,<taille>,ok` |

Exemple : `2,2,1B;40` envoie `ESC @`, la commande d'initialisation de l'imprimante. Les images et QR codes sont convertis en données raster ESC/POS, puis envoyés par trames pour limiter la taille de chaque commande.

## 📁 Projet

| Fichier ou dossier | Rôle |
| --- | --- |
| `CinePrintApp/` | Application C# Windows Forms |
| `Arduino.cpp` | Serveur TCP et passerelle vers l'imprimante |
| `emu.js` | Émulateur TCP pour les tests sans Arduino |
| `emulator.py` | Client Python de test du protocole |
| `app/` | Prototype d'interface Web |

## 👨‍💻 Développement

Projet réalisé dans le cadre du **BTS CIEL, option IR**. Il n’est plus maintenu et a été conçu à des fins pédagogiques : son utilisation pour un usage réel est déconseillée.
