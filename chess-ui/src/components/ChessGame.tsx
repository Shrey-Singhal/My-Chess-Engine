import { useState, useEffect } from "react";
import Board from "./Board";
import SetFEN from "./SetFEN";
import ResultModal from "./ResultModal";
import EngineOutput from "./EngineOutput";

type GuiPiece = {
    fileClass: string;
    rankClass: string;
    imagePath: string;
};

type EngineStats = {
    bestMove: string;
    depth: number;
    nodes: number;
    ordering: string;
    time: string;
};
let _didInitGame = false;  
const ChessGame = () => {
    const BASE = import.meta.env.VITE_API_BASE_URL as string;
    const [gameId, setGameId] = useState<string>("");
    const [pieces, setPieces] = useState<GuiPiece[]>([]);
    const [modalMsg, setModalMsg] = useState<string | null>(null);
    const [engineTime, setEngineTime] = useState<number>(1000);
    
    const [engineStats, setEngineStats] = useState<EngineStats>({
        bestMove: "",
        depth: 0,
        nodes: 0,
        ordering: "",
        time: "",
    });
    const [flipped, setFlipped] = useState<boolean>(false);

    // fetch the current board pieces
    const fetchPieces = () => {
        if (!gameId) return;
        fetch(`${BASE}/getpieces?gameId=${gameId}`)
            .then((res) => res.json())
            .then((data: GuiPiece[]) => setPieces(data))
            .catch((err) => console.error("Failed to fetch pieces:", err));
    };

    // ask engine for its move
    const handleEngineMove = (timeOverride?: number) => {
        if (!gameId) return;
        fetch(`${BASE}/engineMove?gameId=${gameId}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ Time: timeOverride ?? engineTime }),
        })
            .then((res) => res.json())
            .then((data) => {
                setPieces(data.pieces);
                if (data.result) setModalMsg(data.result);

                return fetch(`${BASE}/enginestats?gameId=${gameId}`);
            })
            .then((res) => res.json())
            .then(setEngineStats)
            .catch((err) => console.error("Engine move error:", err));
    };

    // take back last move
    const handleTakeBack = () => {
        if (!gameId) return;
        fetch(`${BASE}/takemove?gameId=${gameId}`, { method: "POST" })
            .then((res) => res.json())
            .then((data) => {
                setPieces(data.pieces);
                if (data.result) setModalMsg(data.result);
            })
            .catch((err) => console.error("Take back error:", err));
    };

    // start a brand new game
    const handleNewGame = () => {
        fetch(`${BASE}/newgame`, { method: "POST" })
            .then((res) => res.json())
            .then((data) => {
                console.log("[ChessGame] newgame →", data);
                setGameId(data.gameId);
                console.log(gameId);
                setPieces(data.pieces);
                setModalMsg(null);
            })
            .catch((err) => console.error("New game error:", err));
    };

    // on mount, initialize a game
    useEffect(() => {
        if (!_didInitGame) {
            _didInitGame = true;
            handleNewGame();
        }
    }, []);

    return (
        <>
            <SetFEN gameId={gameId} fetchPieces={fetchPieces} />
            {gameId
                ? 
                <Board
                    gameId={gameId}
                    pieces={pieces}
                    setPieces={setPieces}
                    setModalMsg={setModalMsg}
                    onEngineMove={handleEngineMove}
                    engineTime={engineTime}
                    flipped={flipped}
                />
                :<div>Loading board…</div>
            }
            
            <ResultModal show={!!modalMsg} onClose={() => setModalMsg(null)}>
                {modalMsg}
            </ResultModal>
            <EngineOutput
                onEngineMove={handleEngineMove}
                setEngineTime={setEngineTime}
                engineTime={engineTime}
                onTakeBack={handleTakeBack}
                onNewGame={handleNewGame}
                engineStats={engineStats}
                flipped={flipped}
                setFlipped={setFlipped}
            />
        </>
    );
};

export default ChessGame;
