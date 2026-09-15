const ipc = require('./api/prosessCom')
const tcp = require('./api/tcpCom')
const express = require('express')
const { createServer } = require('http')
const net = require('net')

const app = express()
const server = createServer(app)

app.get('/', (req, res) => res.sendFile(__dirname + '/src/index.html'))
app.use(express.static('src'))
app.use(express.static('api'))

ipc.attach(server) // WebSocket sur le même serveur HTTP

server.listen(8080, () => console.log('Server running on port http://localhost:8080'))

ipc.handle('tcp-send', async (ip, port, msg) => {
    const res = await tcp.send(ip, port, msg)
    ipc.send('tcp-response', res)
    return res
})