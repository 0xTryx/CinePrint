const net = require('net')
// tcpCom.js — accepte string ou objet
module.exports = {
  send: (ip, port, data) => new Promise((resolve, reject) => {
    let buffer = ''
    const socket = net.createConnection(port, ip, () => {
      const msg = typeof data === 'string' ? data : JSON.stringify(data)
      socket.write(msg + '\n')
    })
    socket.on('data', (raw) => {
      buffer += raw.toString()
      if (buffer.includes('\n')) {
        resolve(buffer.split('\n')[0])
        socket.destroy()
      }
    })
    socket.on('error', reject)
  })
}