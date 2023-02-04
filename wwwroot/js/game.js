"use strict";

let connection = new signalR.HubConnectionBuilder().withUrl("/game-hub").build();

const votingPanel = document.getElementById("voting");

function hideVoting() {
    votingPanel.style.display = 'none';
}

function showVoting() {
    votingPanel.style.display = 'block';
}

function debug(msg) {
    document.getElementById("debug-log").innerHTML += msg + "<br>";
}

hideVoting();

connection.on("VotingStarted", function (user, action) {
    debug("voting started for action " + action)

    showVoting();
});

connection.on("VoteReceived", function (user) {
    debug("vote received");
    let currentUser = document.getElementById("userInput").value;

    if (user === currentUser) { hideVoting(); }
});

connection.on("VotingFinished", function (result) {
    debug("voting finished")
    hideVoting();
    //alert("voting finished " + result);
});

connection.on("VotingInconclusive", function () {
    hideVoting();
    debug("voting inconclusive, you die");
});

connection.on("MapUpdate", function (state) {
    let currentUser = document.getElementById("userInput").value;
    let parsed = JSON.parse(state)
    console.log(parsed)

    let maze = parsed["Cells"];
    let pos = [parsed["X"], parsed["Y"]]
    let hp = parsed["HP"]
    let energy = parsed["Energy"]
    let energyRecovery = parsed.Level.EnergyRecoveryRate
    let turn = parsed["Turn"]
    let votingHistory = parsed["VotingHistory"]
    let secret = parsed.Level.ClairvoyantGoal

    let html = "<table>";

    for (let i = 0; i < maze.length; i++) {
        html += "<tr>";
        for (let j = 0; j < maze[i].length; j++) {
            let content = "";
            let s = "";

            if (maze[j][i].includes("v")) {
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
            }

            if (maze[j][i].includes("s")) {
                content = "🧱";
            }

            if (i == pos[0] && j == pos[1])
                content = "@";

            html += `<td style="${s}">${content}</td>`;
        }
        html += "</tr>";
    }

    html += "</table>";

    document.getElementById("map").innerHTML = html;

    //alert(maze[pos[1]][pos[0]])

    document.getElementById("north").disabled = false;
    document.getElementById("east").disabled = false;
    document.getElementById("south").disabled = false;
    document.getElementById("west").disabled = false;

    if (maze[pos[1]][pos[0]].includes("t")) {
        document.getElementById("north").disabled = true;
    }

    if (maze[pos[1]][pos[0]].includes("r")) {
        document.getElementById("east").disabled = true;
    }

    if (maze[pos[1]][pos[0]].includes("b")) {
        document.getElementById("south").disabled = true;
    }

    if (maze[pos[1]][pos[0]].includes("l")) {
        document.getElementById("west").disabled = true;
    }

    document.getElementById('votinghistory').textContent = votingHistory;
    document.getElementById('scribe').textContent = `HP: ${hp}, Energy: ${energy} ${energyRecovery}, Turn: ${turn}`;

    document.getElementById('secret').textContent = secret;

});

connection.start().then(function () {
    //document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.querySelectorAll("button.action").forEach(x => x.addEventListener("click", function (e) {
    let user = document.getElementById("userInput").value;
    let action = e.target.dataset.val;

    // invoking StartVotingForAction
    connection.invoke("StartVotingForAction", user, action).catch(function (err) {
        return console.error(err.toString());
    });

    // but also invoking voting timeout function
    // setTimeout(function () {
    //     connection.invoke("VotingTimeout", user).catch(function (err) {
    //         return console.error(err.toString());
    //     });
    // }, 5000);

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