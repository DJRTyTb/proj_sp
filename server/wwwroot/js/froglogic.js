let connection;
let playerToken = null;
let currentGameId = null;

async function connectToServer() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/gamehub")
        .build();

    connection.on("GameCreated", (gameId, token, color) => {
        currentGameId = gameId;
        playerToken = token;
        myColor = color;

        updateStatus({
            turn: "🐸 WAITING",
            players: [],
            msg: `Game ID: ${gameId}`
        });

        const msg = document.getElementById("gameMsg");

        msg.innerHTML = `
                    <span>🎲 ${gameId}</span>
                    <button id="copyGameIdBtn"
                            style="margin-left:8px;padding:2px 8px;border-radius:8px;background:#22c55e;color:white;border:none;cursor:pointer;">
                        📋
                    </button>
                `;

        document.getElementById("copyGameIdBtn").onclick = async () => {
            await navigator.clipboard.writeText(gameId);

            const btn = document.getElementById("copyGameIdBtn");

            btn.textContent = "✅";

            setTimeout(() => btn.textContent = "📋", 1500);
        };

        //alert(`Send this Game ID to opponent:\n${gameId}`);
    });

    connection.on("JoinedGame", (gameId, token, color) => {
        currentGameId = gameId;
        playerToken = token;
        myColor = color;

        updateStatus({
            turn: "🐸 WAITING",
            players: [],
            msg: `Joined game`
        });
    });

    connection.on("GameStarted", state => {
        applyGameState(state);
    });

    connection.on("BoardUpdated", state => {
        applyGameState(state);
    });

    connection.on("GameFinished", winner => {
        alert(getWinnerMessage(winner));
    });

    connection.on("PlayerDisconnected", player => {
        alert(`${player} disconnected.`);
    });

    connection.on("PlayerReconnected", player => {
        alert(`${player} reconnected.`);
    });

    await connection.start();
}

function getWinnerMessage(winner) {
    if (winner === null) return 'Draw';
    else return `Winner: ${winner}`;
}

function applyGameState(state) {
    currentState = state;

    setBoardState(state.cells);

    const me = state.players.find(
        p => p.name === document.getElementById("playerNameInput").value);

    let message = "";

    if (state.isFinished)                       message = getWinnerMessage(winner);
    else if (state.message)                     message = state.message;
    else if (me && !me.hasUsedInitialRemoval)   message = "Remove any frog";
    else                                        message = `Current: ${state.currentPlayer}`;

    updateStatus({
        turn: state.currentPlayer === document.getElementById("playerNameInput").value
            ? "🐸 YOUR TURN"
            : "⏳ THEIR TURN",

        players: state.players.map(p => ({
            name: `${p.name} (${p.color})`,
            active: p.isCurrentTurn
        })),

        msg: message
    });
}

connectToServer();

document.getElementById("joinBtn").onclick = async e => {
    e.preventDefault();

    const name = document.getElementById("playerNameInput").value.trim();
    const gameId = document.getElementById("gameIdInput").value.trim();

    if (name === "") {
        alert("Please enter a player name.");
        return;
    }

    try {
        if (gameId === "")
            await connection.invoke("CreateGame", name);
        else
            await connection.invoke("JoinGame", gameId, name);

        document.getElementById("joinModalBackdrop").style.display = "none";
    }
    catch (err) {
        alert(getErrorMessage(err));
    }
};

document.getElementById("passBtn").onclick = async () => {
    console.log('trying to pass');
    if (!currentGameId || !playerToken) return;

    try {
        await connection.invoke("PassTurn", currentGameId, playerToken);
    }
    catch (err) {
        alert(getErrorMessage(err));
    }
};

let currentState = null;

/*function onCellClicked(col, row) {
    if (!currentState) return;

    const me = document.getElementById("playerNameInput").value;
    if (currentState.currentPlayer !== me) return;

    const player = currentState.players.find(p => p.name === me);
    if (!player) return;

    if (!player.hasUsedInitialRemoval) {
        connection.invoke(
            "RemoveInitialFrog",
            currentGameId,
            playerToken,
            row,
            col)
            .catch(console.error);

        return;
    }

    const frog = currentState.cells.find(c => c.col === col && c.row === row);
    if (!frog) return;
    if (frog.color !== myColor) return;

    selectedPath = [{ row, col }];

    highlightSelection(col, row);
}*/

function onCellClicked(col, row) {
    if (!currentState) return;

    const me = document.getElementById("playerNameInput").value;
    if (currentState.currentPlayer !== me) return;

    const player = currentState.players.find(p => p.name === me);
    if (!player) return;

    if (!player.hasUsedInitialRemoval) {
        connection.invoke(
            "RemoveInitialFrog",
            currentGameId,
            playerToken,
            row,
            col)
            .catch(console.error);

        return;
    }

    const frog = currentState.cells.find(c => c.col === col && c.row === row);

    if (!frog) return;
    if (frog.color !== myColor) return;

    if (moveInProgress && startCell.row === row && startCell.col === col) {
        selectedPath.push({ row, col });
        drawPath();
        return;
    }

    moveInProgress = true;
    startCell = { row, col };
    selectedPath = [{ row, col }];

    showMoveControls(true);
    highlightSelection(col, row);
    return;
}

function onBoardCellClicked(col, row) {
    if (selectedPath.length === 0) return;

    const frog = currentState.cells.find(c => c.col === col && c.row === row);
    if (frog) return;

    if (selectedPath[selectedPath.length - 1].row === row && selectedPath[selectedPath.length - 1].col === col) return;

    selectedPath.push({ row, col });
    drawPath();
}

function showMoveControls(show) {
    document.getElementById("moveControls").style.display = show ? "flex" : "none";
}

function clearMove() {
    moveInProgress = false;
    startCell = null;
    selectedPath = [];
    clearHighlight();
    clearPathVisuals();
    showMoveControls(false);
}

function getErrorMessage(err) {
    if (!err || !err.message)
        return "Unknown error.";

    let message = err.message;

    const index = message.indexOf("HubException:");

    if (index >= 0)
        message = message.substring(index + "HubException:".length);

    return message.trim();
}

let moveInProgress = false;
let startCell = null;
let pathLines = [];
let selectedPath = [];
let selectionCircle = null;

function highlightSelection(col, row) {
    clearHighlight();

    const scene = window.game.scene.keys.GridScene;

    const CELL = 65;
    const OX = (520 - 8 * CELL) / 2;
    const OY = (520 - 8 * CELL) / 2;

    const x = OX + (col + 1) * CELL + CELL / 2;
    const y = OY + (row + 1) * CELL + CELL / 2;

    selectionCircle = scene.add.circle(
        x,
        y,
        CELL * 0.42,
        0xffff00,
        0.3);

    selectionCircle.setStrokeStyle(3, 0xffff00);
}

function drawPath() {
    clearPathVisuals();

    if (selectedPath.length < 2) return;

    const scene = window.game.scene.keys.GridScene;

    const CELL = 65;
    const OX = (520 - 8 * CELL) / 2;
    const OY = (520 - 8 * CELL) / 2;

    let graphics = scene.add.graphics();
    graphics.lineStyle(3, 0xffff00, 0.9);

    for (let i = 0; i < selectedPath.length - 1; i++) {
        const a = selectedPath[i];
        const b = selectedPath[i + 1];

        const ax = OX + (a.col + 1) * CELL + CELL / 2;
        const ay = OY + (a.row + 1) * CELL + CELL / 2;

        const bx = OX + (b.col + 1) * CELL + CELL / 2;
        const by = OY + (b.row + 1) * CELL + CELL / 2;

        graphics.beginPath();
        graphics.moveTo(ax, ay);
        graphics.lineTo(bx, by);
        graphics.strokePath();
    }

    pathLines.push(graphics);
}

function clearPathVisuals() {
    const scene = window.game.scene.keys.GridScene;

    for (const line of pathLines)
        line.destroy();

    pathLines = [];
}

function clearHighlight() {
    if (!selectionCircle)
        return;

    selectionCircle.destroy();
    selectionCircle = null;
}

document.getElementById("cancelMoveBtn").onclick = () => {
    clearMove();
};

document.getElementById("confirmMoveBtn").onclick = async () => {
    if (!currentGameId || !playerToken) return;

    try {
        await connection.invoke(
            "MakeMove",
            currentGameId,
            playerToken,
            selectedPath
        );
    }
    catch (err) {
        updateStatus({
            state: 'error',
            turn: turnEl.textContent,
            players: currentState.players.map(p => ({
                name: p.name,
                color: p.color,
                active: p.isCurrentTurn
            })),
            msg: getErrorMessage(err)
        });
    }

    clearMove();
};
