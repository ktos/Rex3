"use strict";

const goals = ["There is no special goal", "Kill all the enemies"]

let connection = new signalR.HubConnectionBuilder().withUrl("/game-hub").build();

let currentUser = "";

const votingPanel = document.getElementById("voting");
document.getElementById("main").style.display = 'none';

function hideVoting() {
    votingPanel.style.display = 'none';
}

function showVoting() {
    votingPanel.style.display = 'block';
}

function debug(msg) {
    //document.getElementById("debug-log").innerHTML += msg + "<br>";
}

hideVoting();
document.getElementById("win").style.display = 'none';
document.getElementById("lose").style.display = 'none';

connection.on("VotingStarted", function (user, action) {
    debug("voting started for action " + action)

    showVoting();
});

connection.on("VoteReceived", function (user) {
    debug("vote received");

    if (user === currentUser) { hideVoting(); }
});

connection.on("VotingFinished", function (result) {
    debug("voting finished")
    hideVoting();
});

connection.on("VotingInconclusive", function () {
    hideVoting();
    debug("voting inconclusive, you die");
});

connection.on("GameStarted", function () {
    document.getElementById('role').style.display = 'none';
    document.getElementById('main').style.display = '';
});

connection.on("MapUpdate", function (state) {
    let parsed = JSON.parse(state)
    console.log(parsed)

    let maze = parsed["Cells"];
    let pos = [parsed["X"], parsed["Y"]]
    let hp = parsed["HP"]
    let energy = parsed["Energy"]
    let energyRecovery = parsed.Level.EnergyRecoveryRate
    let energyRecoveryAmount = parsed.Level.EnergyRecoveryAmount
    let turn = parsed["Turn"]
    let votingHistory = "BadVotes: " + parsed.BadVotesCount + ", voting history: " + parsed["VotingHistory"].map(x => (x === null) ? "inconclusive" : x.toString())
    let secret = goals[parsed.Level[currentUser[0].toUpperCase() + currentUser.substring(1) + "Goal"]]

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

            if (maze[j][i].includes("s") && currentUser == "navigator") {
                content = "🧱";
            }

            if (maze[j][i].includes("e") && currentUser == "clairvoyant") {
                let enemy = maze[j][i].substr(maze[j][i].indexOf("e") + 1, 1);
                content = `👾<span class="hp">${enemy}</span><span class="future">${hp - enemy > 0 ? "" : "dead"}</span>`;
            }

            if (maze[j][i].includes("x") && currentUser == "navigator") {
                content = `🎁`;
            }

            if (i == pos[1] && j == pos[0])
                content = "🤖";

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

    if (maze[pos[0]][pos[1]].includes("t")) {
        document.getElementById("north").disabled = true;
    }

    if (maze[pos[0]][pos[1]].includes("r")) {
        document.getElementById("east").disabled = true;
    }

    if (maze[pos[0]][pos[1]].includes("b")) {
        document.getElementById("south").disabled = true;
    }

    if (maze[pos[0]][pos[1]].includes("l")) {
        document.getElementById("west").disabled = true;
    }

    if (energy == 0) {
        document.getElementById("north").disabled = true;
        document.getElementById("east").disabled = true;
        document.getElementById("south").disabled = true;
        document.getElementById("west").disabled = true;
    }

    document.getElementById('votinghistory').textContent = votingHistory;
    document.getElementById('scribe').textContent = `HP: ${hp}, Energy: ${energy} (recovers ${energyRecoveryAmount} every ${energyRecovery} turns), Turn: ${turn}`;
    if (currentUser != "scribe")
        document.getElementById("scribe").style.display = 'none';
    else
        document.getElementById("scribe").style.display = '';


    document.getElementById('secret').textContent = "Secret goal: " + secret;

});

connection.on("Win", function (mystery) {
    console.log(mystery)
    let m = JSON.parse(mystery)

    document.querySelector("#win > h2").textContent = m;
    document.getElementById("win").style.display = '';
    document.getElementById("main").style.display = 'none';
});

connection.on("Lose", function (mystery) {
    console.log(mystery)
    let m = JSON.parse(mystery)

    document.querySelector("#lose > h2").textContent = m;
    document.getElementById("lose").style.display = '';
    document.getElementById("main").style.display = 'none';
});

connection.on("RoleSelected", function (role) {
    document.querySelector("#role button#b" + role).disabled = true;
});

connection.start().then(function () {
    //document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.querySelectorAll("button.action").forEach(x => x.addEventListener("click", function (e) {
    let action = e.target.dataset.val;

    // invoking StartVotingForAction
    connection.invoke("StartVotingForAction", currentUser, action).catch(function (err) {
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
    let action = e.target.dataset.val;

    connection.invoke("Vote", currentUser, action).catch(function (err) {
        return console.error(err.toString());
    });
    e.preventDefault();
}));

document.querySelectorAll("#debug > button").forEach(x => x.addEventListener("click", function (e) {
    let action = e.target.innerText;

    connection.invoke("Debug", currentUser, action).catch(function (err) {
        return console.error(err.toString());
    });
    e.preventDefault();
}));

document.querySelectorAll("#role > button").forEach(x => x.addEventListener("click", function (e) {
    let action = e.target.id.substr(1);
    currentUser = action;

    document.querySelectorAll("#role button").forEach(x => x.disabled = true);
    connection.invoke("SetRole", action).catch(function (err) {
        return console.error(err.toString());
    });
    e.preventDefault();
}));