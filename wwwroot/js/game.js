"use strict";

const goals = ["There is no special goal", "Kill all the enemies", "Do not kill the enemies", "Do not rest", "Die", "Do not take objects"]
const specialActions = { 5: "Reduce HP of enemies", 6: "Teleport randomly", 7: "Sacrifice" }

let connection = new signalR.HubConnectionBuilder().withUrl("/game-hub").withAutomaticReconnect().build();

let currentUser = "";

const votingPanel = document.getElementById("voting");

function hideVoting() {
    votingPanel.style.display = 'none';
}

function showVoting() {
    votingPanel.style.display = 'block';
}

hideVoting();
document.getElementById("win").style.display = 'none';
document.getElementById("lose").style.display = 'none';
document.getElementById("game").style.display = 'none';

connection.on("VotingStarted", function (user, action) {
    console.log("voting started for action " + action);
    document.getElementById("sfx-votestart").play();

    showVoting();
});

connection.on("VoteReceived", function (user) {
    console.log("vote received");

    if (user === currentUser) { hideVoting(); }
});

connection.on("VotingFinished", function (result) {
    console.log("voting finished")

    if (result)
        document.getElementById("sfx-voteend").play();
    else
        document.getElementById("sfx-voteendfail").play();

    hideVoting();
});

connection.on("VotingInconclusive", function () {
    document.getElementById("sfx-voteendfail").play();
    hideVoting();
    console.log("voting inconclusive, you die");
});

connection.on("GameStarted", function () {
    console.log("game started");
    document.getElementById('character-selection').style.display = 'none';
    document.getElementById('game').style.display = '';
    document.getElementById("audio-level1").play();
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
    let html = "<table id='main-map'>";
    for (let i = 0; i < maze.length; i++) {
        html += "<tr>";
        for (let j = 0; j < maze[i].length; j++) {
            let content = "";
            let s = [];

            if (maze[j][i].includes("v")) {

                if (maze[j][i].includes("l")) {
                    s.push("left");
                }

                if (maze[j][i].includes("t")) {
                    s.push("top");
                }

                if (maze[j][i].includes("r")) {
                    s.push("right");
                }

                if (maze[j][i].includes("b")) {
                    s.push("bottom");
                }
            }
            else
                s.push("hidden");

            let cl = "background_" + s.join("_");

            if (cl == "background_") cl = "background_hidden";

            if (maze[j][i].includes("s") && currentUser == "navigator") {
                content = `<div class="element endpoint"></div>`;
            }

            if (maze[j][i].includes("e") && currentUser == "clairvoyant") {
                let enemy = maze[j][i].substr(maze[j][i].indexOf("e") + 1, 1);
                content = `<div class="element enemy"><span class="enemy-hp">${enemy}</span><span class="future">${hp - enemy > 0 ? "" : "dead"}</span></div>`;
            }

            if (maze[j][i].includes("x") && currentUser == "navigator") {
                content = `<div class="element item"></div>`;
            }

            if (i == pos[1] && j == pos[0])
                content = `<div class="element player"></div>`;

            html += `<td class="${cl}">${content}</td>`;
        }
        html += "</tr>";
    }
    html += '</table>';

    /*
    <td class="background_hidden">
                            <div class="element player"></div>
                        </td>
                        <td class="background_hidden">
                            <div class="element enemy"></div>
                        </td>
                        <td class="background_hidden">
                            <div class="element item"></div>
                        </td>
                        <td class="background_hidden">
                            <div class="element endpoint"></div>
                        </td>
                        */

    // update map
    document.getElementById("game-map").innerHTML = html;

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
    document.getElementById("special").title = specialActions[specialAction];
    document.getElementById("special-skill-desc").textContent = "Special skill: " + specialActions[specialAction];
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
    document.getElementById("stat-hp").textContent = `HP:${hp}`;
    document.getElementById("stat-energy").textContent = `EN:${energy}`;
    document.getElementById("energy-desc").textContent = `recovers ${energyRecoveryAmount} every ${energyRecovery} turns, turn is ${turn}`;

    if (currentUser != "scribe") {
        document.getElementById("basic-stats").style.visibility = 'hidden';
        document.getElementById("energy-desc").style.visibility = 'hidden';
    }
    else {
        document.getElementById("basic-stats").style.visibility = '';
        document.getElementById("energy-desc").style.visibility = '';
    }

    // update secret goal
    document.getElementById('secret').textContent = "Secret goal: " + secret;
});

connection.on("Win", function (mystery) {
    console.log("win");
    console.log(mystery)
    let m = JSON.parse(mystery)

    document.querySelector("#win > h2").textContent = m;
    document.getElementById("win").style.display = '';
    document.getElementById("game").style.display = 'none';
    document.getElementById("audio-final").play();
});

connection.on("Lose", function (mystery) {
    console.log("lose");
    console.log(mystery)
    let m = JSON.parse(mystery)

    document.querySelector("#lose > h2").textContent = m;
    document.getElementById("lose").style.display = '';
    document.getElementById("game").style.display = 'none';
    document.getElementById("audio-final").play();
});

connection.on("RoleSelected", function (role) {
    console.log("role selected " + role);
    document.querySelector("#character-selection input#b" + role).disabled = true;
    document.querySelector("#character-selection input#b" + role).classList.add("active");
    document.getElementById("connection-status").textContent = 'waiting for upload';
});

connection.start().then(function () {
    document.getElementById("connection-status").textContent = 'connected';
}).catch(function (err) {
    return console.error(err.toString());
});

document.querySelectorAll("#actions-list button").forEach(x => x.addEventListener("click", function (e) {
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

document.querySelectorAll("#character-selection input").forEach(x => x.addEventListener("click", function (e) {
    let action = e.target.id.substr(1);
    currentUser = e.target.id.substr(1);

    if (currentUser == "scribe") {
        document.querySelector(".right-container .main-portret").src = 'images/character_1.PNG';
        document.querySelector(".left-container #other-portret1").src = 'images/character_2.PNG';
        document.querySelector(".left-container #other-portret2").src = 'images/character_3.PNG';
    } else if (currentUser == "navigator") {
        document.querySelector(".right-container .main-portret").src = 'images/character_3.PNG';
        document.querySelector(".left-container #other-portret1").src = 'images/character_1.PNG';
        document.querySelector(".left-container #other-portret2").src = 'images/character_2.PNG';
    }
    else {
        document.querySelector(".right-container .main-portret").src = 'images/character_2.PNG';
        document.querySelector(".left-container #other-portret1").src = 'images/character_1.PNG';
        document.querySelector(".left-container #other-portret2").src = 'images/character_3.PNG';
    }


    console.log("selected role " + action);
    console.log(currentUser)
    document.querySelectorAll("#character-selection input").forEach(x => x.disabled = true);
    connection.invoke("SetRole", action).catch(function (err) {
        return console.error(err.toString());
    });
    e.preventDefault();
}));

var audiointro = document.getElementById("audio-intro");

audiointro.addEventListener("canplaythrough", (event) => {
    /* the audio is now playable; play it if permissions allow */
    audiointro.play();
});