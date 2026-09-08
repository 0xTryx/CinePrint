# CinePrint
System de gestion d'imprimante pour cinéma

exemple du protocole

1,test aaa 1234567890
efectue un socket.println("test aaa 1234567890")

2, 2, 1B;40
efectue un socket.write({Ox1B,0x40},2)


le numéreau corespond a l'instruction (1 pour println et 2 pour write)
pour le println, tout le contenue apret la prenierre virgulle est le text a afficher
pour le write, le nombre entre la 1er et 2eme est la longeur des donée, et le contenue apert est les donée en hexa séparez par des ";"
