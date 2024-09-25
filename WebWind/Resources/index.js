"use strict";

var socket = null;
function reconnect() {
    const checkInterval = 100;

    const checkServer = function () {
        fetch('/')
            .then(response => {
                if (response.ok) {
                    console.log("Server is back online. Reloading page...");
                    location.reload();
                } else {
                    setTimeout(checkServer, checkInterval);
                }
            })
            .catch((error) => {
                console.log("Server still down, retrying...");
                setTimeout(checkServer, checkInterval);
            });
    };

    checkServer();
}
window.onload = function() {
    socket = new WebSocket(`ws://${window.location.host}`);

    socket.onopen = function (e) {
        socket.send(`view ${window.location.pathname}`);
    }

    socket.onclose = function (e) {
        reconnect();
    }

    socket.onmessage = function (e) {
        console.log(e.data);
        eval(e.data);
    }

    socket.onerror = function (e) {
        alert('An error has occured!\n' + e.message);
    }
};

function SendToServer() {
    var text = document.getElementById('tb-send').value;
    socket.send(text);
}

function createRipple(event) {
    const button = event.currentTarget;

    // Remove existing ripple elements (if any)
    const existingRipple = button.querySelector('.ripple');
    if (existingRipple) {
        existingRipple.remove();
    }

    // Create a new ripple
    const ripple = document.createElement('span');
    ripple.classList.add('ripple');

    // Get the button's size and set the ripple size accordingly
    const buttonRect = button.getBoundingClientRect();
    const size = Math.max(buttonRect.width, buttonRect.height);
    ripple.style.width = ripple.style.height = `${size}px`;

    // Calculate position of the ripple (center it at the click point)
    const x = event.clientX - buttonRect.left - size / 2;
    const y = event.clientY - buttonRect.top - size / 2;
    ripple.style.left = `${x}px`;
    ripple.style.top = `${y}px`;

    // Append the ripple to the button
    button.appendChild(ripple);

    // Remove the ripple after animation ends
    ripple.addEventListener('animationend', () => {
        ripple.remove();
    });
}