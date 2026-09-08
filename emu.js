const net = require('net');
const PORT = 8080;

function handleCommand(line) {
  line = line.trim();
  if (!line) return null;

  const first = line.indexOf(',');
  if (first === -1) return null;

  const instruction = parseInt(line.substring(0, first));

  if (instruction === 0) {
    if (line.substring(first + 1).trim() === 'ping') return '0,pong';
  }

  else if (instruction === 1) {
    const text = line.substring(first + 1);
    process.stdout.write(text + '\n');
    return '1,ok';
  }

  else if (instruction === 2) {
    const second = line.indexOf(',', first + 1);
    if (second === -1) return null;

    const length = parseInt(line.substring(first + 1, second));
    const bytes = Buffer.from(
      line.substring(second + 1).split(';').map(h => parseInt(h.trim(), 16))
    );

    process.stdout.write(bytes);
    return `2,${length},ok`;
  }

  return null;
}

const server = net.createServer((socket) => {
  process.stderr.write(`[SERVER] Client connecté: ${socket.remoteAddress}\n`);
  let buffer = '';

  socket.on('data', (data) => {
    buffer += data.toString();
    const lines = buffer.split('\n');
    buffer = lines.pop();

    for (const line of lines) {
      process.stderr.write(`[SERVER] reçu: ${line}\n`);
      const rep = handleCommand(line);
      if (rep) socket.write(rep + '\n');
    }
  });

  socket.on('close', () => process.stderr.write('[SERVER] Client déconnecté\n'));
  socket.on('error', (e) => process.stderr.write(`[SERVER] Erreur: ${e.message}\n`));
});

server.listen(PORT, () => process.stderr.write(`[SERVER] Écoute sur le port ${PORT}\n`));