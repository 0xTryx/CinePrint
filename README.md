# CinePrint
System de gestion d'imprimante pour cinéma

exemple du protocole

0,ping
repond avec 0,pong 

1,test aaa 1234567890
efectue un socket.println("test aaa 1234567890")
repond avec 1,ok

2, 2, 1B;40
efectue un socket.write({Ox1B,0x40},2)
repond avec 2, 2, ok

le numéreau corespond a l'instruction (1 pour println et 2 pour write)
pour le println, tout le contenue apret la prenierre virgulle est le text a afficher
pour le write, le nombre entre la 1er et 2eme est la longeur des donée, et le contenue apert est les donée en hexa séparez par des ";"

repond avec le meme numero d'instruction et Ok, pour le write, repond aussis avec la taille
