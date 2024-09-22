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