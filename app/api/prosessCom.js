const { WebSocketServer } = require('ws')
const handlers = new Map()
let _wss

module.exports = {
  attach(server) {
    _wss = new WebSocketServer({ server })
    _wss.on('connection', (ws) => {
      ws.on('message', async (raw) => {
        const { id, channel, args } = JSON.parse(raw)
        const fn = handlers.get(channel)
        if (!fn) return
        const result = await fn(...args)
        ws.send(JSON.stringify({ id, result }))
      })
    })
  },
  handle: (channel, fn) => handlers.set(channel, fn),
  send: (channel, data) => {
    const msg = JSON.stringify({ channel, data })
    _wss.clients.forEach(ws => ws.send(msg))
  }
}