const ws = new WebSocket(`ws://${location.host}`)
const pending = new Map()
const listeners = new Map()
let _id = 0

const ready = new Promise(resolve => ws.addEventListener('open', resolve))

ws.onmessage = ({ data }) => {
  const msg = JSON.parse(data)
  if (msg.id && pending.has(msg.id)) {
    pending.get(msg.id)(msg.result)
    pending.delete(msg.id)
  } else if (msg.channel) {
    listeners.get(msg.channel)?.forEach(cb => cb(msg.data))
  }
}

window.api = {
  invoke: async (channel, ...args) => {
    await ready
    return new Promise(resolve => {
      const id = ++_id
      pending.set(id, resolve)
      ws.send(JSON.stringify({ id, channel, args }))
    })
  },
  on: (channel, cb) => {
    if (!listeners.has(channel)) listeners.set(channel, [])
    listeners.get(channel).push(cb)
  }
}
