"use strict";

let connection = new signalR.HubConnectionBuilder().withUrl("/game-hub").build();

const votingPanel = document.getElementById("voting");

function hideVoting() {
    votingPanel.style.display = 'none';
}

function showVoting() {
    votingPanel.style.display = 'block';
}

hideVoting();

connection.on("VotingStarted", function (user, action) {
    console.log("voting started for action " + action)

    showVoting();
});

connection.on("VoteReceived", function (user) {
    console.log("vote received");
    let user2 = document.getElementById("userInput").value;

    if (user === user2) { hideVoting(); }
});

connection.on("VotingFinished", function (result) {
    hideVoting();
    alert("voting finished " + result);
});

connection.on("VotingInconclusive", function () {
    hideVoting();
    alert("voting inconclusive, you die");
});

connection.on("MapUpdate", function (maze) {
    maze = JSON.parse(maze)

    let html = "<table>";

    for (let i = 0; i < maze.length; i++) {
        html += "<tr>";
        for (let j = 0; j < maze[i].length; j++) {
            let s = ""

            if (maze[j][i].includes("t")) {
                s += "border-top: 1px black solid;";
            }

            if (maze[j][i].includes("r")) {
                s += "border-right: 1px black solid;";
            }

            if (maze[j][i].includes("b")) {
                s += "border-bottom: 1px black solid;";
            }

            if (maze[j][i].includes("l")) {
                s += "border-left: 1px black solid;";
            }

            html += `<td style="${s}"></td>`;
        }
        html += "</tr>";
    }

    html += "</table>";

    document.getElementById("map").innerHTML = html;

});

connection.start().then(function () {
    //document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.querySelectorAll("button.action").forEach(x => x.addEventListener("click", function (e) {
    let user = document.getElementById("userInput").value;
    let action = e.target.dataset.val;

    connection.invoke("StartVotingForAction", user, action).catch(function (err) {
        return console.error(err.toString());
    });
    e.preventDefault();
}));

document.querySelectorAll("#voting > button").forEach(x => x.addEventListener("click", function (e) {
    let user = document.getElementById("userInput").value;
    let action = e.target.dataset.val;

    connection.invoke("Vote", user, action).catch(function (err) {
        return console.error(err.toString());
    });
    e.preventDefault();
}));

document.querySelectorAll("#debug > button").forEach(x => x.addEventListener("click", function (e) {
    let user = document.getElementById("userInput").value;
    let action = e.target.innerText;

    connection.invoke("Debug", user, action).catch(function (err) {
        return console.error(err.toString());
    });
    e.preventDefault();
}));