import socket

HOST = "172.18.197.99"
PORT = 8080

def send(sock, cmd):
    sock.sendall((cmd + "\n").encode())
    return sock.recv(1024).decode().strip()

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.connect((HOST, PORT))

    # Ping
    print(send(s, "0,ping"))

    # Println
    print(send(s, "1,test aaa 1234567890"))

    # Write (ESC @ = reset imprimante thermique)
    print(send(s, "2,2,1B;40"))

