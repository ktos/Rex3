"use strict";

const goals = ["There is no special goal", "Kill all the enemies"]
const specialActions = { 5: "Reduce HP of enemies" }

let connection = new signalR.HubConnectionBuilder().withUrl("/game-hub").withAutomaticReconnect().build();

let currentUser = "";

const votingPanel = document.getElementById("voting");
document.getElementById("main").style.display = 'none';

function hideVoting() {
    votingPanel.style.display = 'none';
}

function showVoting() {
    votingPanel.style.display = 'block';
}

hideVoting();
document.getElementById("win").style.display = 'none';
document.getElementById("lose").style.display = 'none';

connection.on("VotingStarted", function (user, action) {
    console.log("voting started for action " + action)

    showVoting();
});

connection.on("VoteReceived", function (user) {
    console.log("vote received");

    if (user === currentUser) { hideVoting(); }
});

connection.on("VotingFinished", function (result) {
    console.log("voting finished")
    hideVoting();
});

connection.on("VotingInconclusive", function () {
    hideVoting();
    console.log("voting inconclusive, you die");
});

connection.on("GameStarted", function () {
    console.log("game started");
    document.getElementById('role').style.display = 'none';
    document.getElementById('main').style.display = '';
});

connection.on("MapUpdate", function (state) {
    let parsed = JSON.parse(state)
    console.log("map update");
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
    let specialAction = parsed.Level.SpecialAction

    // generate map
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

    // update map
    document.getElementById("map").innerHTML = html;

    // update buttons
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

    // update special action
    document.getElementById("special").disabled = false;
    document.getElementById("special").dataset.action = specialAction;
    document.getElementById("special").textContent = specialActions[specialAction];
    if (parsed.Level.SpecialActionUsed) {
        document.getElementById("special").disabled = true;
    }

    // disable buttons if no energy
    if (energy == 0) {
        document.getElementById("north").disabled = true;
        document.getElementById("east").disabled = true;
        document.getElementById("south").disabled = true;
        document.getElementById("west").disabled = true;
        document.getElementById("special").disabled = true;
    }

    // update voting history
    document.getElementById('votinghistory').textContent = votingHistory;

    // update scribe panel
    document.getElementById('scribe').textContent = `HP: ${hp}, Energy: ${energy} (recovers ${energyRecoveryAmount} every ${energyRecovery} turns), Turn: ${turn}`;
    if (currentUser != "scribe")
        document.getElementById("scribe").style.display = 'none';
    else
        document.getElementById("scribe").style.display = '';

    // update secret goal
    document.getElementById('secret').textContent = "Secret goal: " + secret;
});

connection.on("Win", function (mystery) {
    console.log("win");
    console.log(mystery)
    let m = JSON.parse(mystery)

    document.querySelector("#win > h2").textContent = m;
    document.getElementById("win").style.display = '';
    document.getElementById("main").style.display = 'none';
});

connection.on("Lose", function (mystery) {
    console.log("lose");
    console.log(mystery)
    let m = JSON.parse(mystery)

    document.querySelector("#lose > h2").textContent = m;
    document.getElementById("lose").style.display = '';
    document.getElementById("main").style.display = 'none';
});

connection.on("RoleSelected", function (role) {
    console.log("role selected " + role);
    document.querySelector("#role button#b" + role).disabled = true;
});

connection.start().then(function () {
    document.getElementById("debug-log").innerHTML = 'connected';
}).catch(function (err) {
    return console.error(err.toString());
});

document.querySelectorAll("button.action").forEach(x => x.addEventListener("click", function (e) {
    let action = e.target.dataset.val;

    console.log("startvoting for " + action);
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

    console.log("voting, " + action);
    connection.invoke("Vote", currentUser, action).catch(function (err) {
        return console.error(err.toString());
    });
    e.preventDefault();
}));

document.querySelectorAll("#debug > button").forEach(x => x.addEventListener("click", function (e) {
    let action = e.target.innerText;

    console.log("debug " + action);
    connection.invoke("Debug", currentUser, action).catch(function (err) {
        return console.error(err.toString());
    });
    e.preventDefault();
}));

document.querySelectorAll("#role > button").forEach(x => x.addEventListener("click", function (e) {
    let action = e.target.id.substr(1);
    currentUser = e.target.id.substr(1);

    console.log("selected role " + action);
    console.log(currentUser)
    document.querySelectorAll("#role button").forEach(x => x.disabled = true);
    connection.invoke("SetRole", action).catch(function (err) {
        return console.error(err.toString());
    });
    e.preventDefault();
}));