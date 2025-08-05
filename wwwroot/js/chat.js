let currentRecipientEmail = null;
let currentRecipientId = null;
const loginUserId = document.getElementById("currentUserId").value;

function showChatWelcomeMessage() {
  document.getElementById('activeChatView').style.display = 'none';
  document.getElementById('chatWelcomeMessage').style.display = 'block';
  currentRecipientEmail = null;
  currentRecipientId = null;
}

function selectUser(id, email, name) {
  // Show active chat
  document.getElementById('chatWelcomeMessage').style.display = 'none';
  document.getElementById('activeChatView').style.display = 'block';
  document.getElementById('chatHeaderName').textContent = name;

  // Clear old messages
  document.getElementById('messageList')?.remove();
  const newList = document.createElement('div');
  newList.id = 'messageList';
  document.querySelector('.active-chat-view').insertBefore(newList, document.querySelector('.chat-input-area'));

  // Store current recipient
  currentRecipientId = id;
  currentRecipientEmail = email;
}

// Append message
function appendMessage(msg, isMe) {
  console.log(msg);
  const msgDiv = document.createElement('div');
  msgDiv.className = 'message-item ' + (isMe ? 'sent' : 'received');
  msgDiv.innerHTML = `<div class="message-content">
                              <p>${msg.messageText}</p>
                              <span class="message-timestamp">${new Date(msg.createdAt).toLocaleTimeString()}</span>
                            </div>`;
  const container = document.getElementById('messageList').appendChild(msgDiv);
  container.scrollTop = container.scrollHeight;
}

// Send message
document.getElementById('btnSend').addEventListener('click', sendMessage);
document.getElementById('messageInput').addEventListener('keydown', e => {
  if (e.key === 'Enter') sendMessage();
});

function sendMessage() {
  const txt = document.getElementById('messageInput');
  const msg = txt.value.trim();

  const myMsg = {
    messageText: msg,
    createdAt: new Date(),
    isSentByMe: true
  };

  appendMessage(myMsg, true);
  txt.value = '';

  fetch('/Home/SendMessage', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      senderId: loginUserId,
      receiverId: currentRecipientId,
      messageText: msg
    })
  });
}

// Polling

setInterval(() => {
  if (!currentRecipientId) return;

  fetch('/Home/ReceiveMessage', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ receiverId: loginUserId })
  })
    .then(r => {
      if (!r.ok) throw new Error(`HTTP ${r.status}`);
      return r.json();
    })
    .then(msg => {
      // console.log('Received from server:', msg);
      if (msg) {
        const chatMsg = {
          messageText: msg.messageText,
          createdAt: msg.createdAt,
        };
        appendMessage(chatMsg, false);

      }
    })
    .catch(err => console.error('Polling error:', err));
}, 1000);
