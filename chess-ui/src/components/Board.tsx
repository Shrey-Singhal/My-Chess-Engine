import React, { useRef, useState, type Dispatch, type JSX, type SetStateAction } from "react";

type GuiPiece = {
    fileClass: string;
    rankClass: string;
    imagePath: string;
};

type BoardProps = {
    pieces: GuiPiece[];
    setPieces: Dispatch<SetStateAction<GuiPiece[]>>;
    setModalMsg: (msg: string | null) => void;
    onEngineMove: (t: number) => void;
    engineTime: number;
    flipped: boolean;
};

function Board({
    pieces,
    setPieces,
    setModalMsg,
    onEngineMove,
    engineTime,
    flipped,
}: BoardProps) {
    //const [pieces, setPieces] = useState<GuiPiece[]>([]);
    const BASE = import.meta.env.VITE_API_BASE_URL as string;
    const BOARD_SIZE = 600;
    const SQUARE_SIZE = BOARD_SIZE / 8;
    const [selectedSquares, setSelectedSquares] = useState<{
        from: { file: number; rank: number } | null;
        to: { file: number; rank: number } | null;
    }>({ from: null, to: null });

    //const [gameStatus, setGameStatus] = useState<string | null>(null);

    function toLogicalSquare(displayFile: number, displayRank: number) {
        const file = flipped ? 7 - displayFile : displayFile;
        const rank = flipped ? displayRank : 7 - displayRank;
        return { file, rank };
    }

    const boardRef = useRef<HTMLDivElement>(null);

    //render squares
    const generateBoardSquares = (): JSX.Element[] => {
        const squares: JSX.Element[] = [];

        for (let displayRank = 0; displayRank < 8; displayRank++) {
            for (let displayFile = 0; displayFile < 8; displayFile++) {
                const { file, rank } = toLogicalSquare(
                    displayFile,
                    displayRank
                );

                const isLight = (file + rank) % 2 === 0;
                const isSelected =
                    (selectedSquares.from?.file === file &&
                        selectedSquares.from?.rank === rank) ||
                    (selectedSquares.to?.file === file &&
                        selectedSquares.to?.rank === rank);

                //const combinedClass = `${squares_style} ${rankClass} ${fileClass} ${colorClass} ${selectedClass}`;
                squares.push(
                    <div
                        key={`sq-${displayRank}-${displayFile}`}
                        className={`absolute ${isLight ? "Light" : "Dark"} ${
                            isSelected ? "SqSelected" : ""
                        }`}
                        style={{
                            width: SQUARE_SIZE,
                            height: SQUARE_SIZE,
                            top: displayRank * SQUARE_SIZE,
                            left: displayFile * SQUARE_SIZE,
                        }}
                        onClick={(e) => handleClick(e, "Square")}
                    />
                );
            }
        }

        return squares;
    };

    const handleClick = (e: React.MouseEvent, type: "Piece" | "Square") => {
        if (!boardRef.current) return;
        const { left, top } = boardRef.current.getBoundingClientRect();
        const x = e.clientX - left;
        const y = e.clientY - top;

        const displayFile = Math.floor(x / SQUARE_SIZE);
        const displayRank = Math.floor(y / SQUARE_SIZE);
        const { file, rank } = toLogicalSquare(displayFile, displayRank);

        // get the 0–119 index and printable square
        fetch(`${BASE}/fr2sq?file=${file}&rank=${rank}`, {
            credentials: "include",
        })
            .then((res) => res.json())
            .then((data) => {
                const newSq = data.sq;
                const prSq = data.prSq;
                console.log(`Selected square: ${prSq} (${file}, ${rank})`);

                // --- first click (set from) ---
                if (type === "Piece" && !selectedSquares.from) {
                    setSelectedSquares({ from: { file, rank }, to: null });
                    return fetch(`${BASE}/setusermove`, {
                        method: "POST",
                        credentials: "include",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify(newSq),
                    });
                }

                // --- second click (set to + make move) ---
                if (
                    (type === "Piece" && selectedSquares.from) ||
                    (type === "Square" && selectedSquares.from)
                ) {
                    // set ‘to’
                    setSelectedSquares({
                        from: selectedSquares.from!,
                        to: { file, rank },
                    });

                    // 1) set the to‐square on the server
                    return (
                        fetch(`${BASE}/setusermove`, {
                            method: "POST",
                            credentials: "include",
                            headers: { "Content-Type": "application/json" },
                            body: JSON.stringify(newSq),
                        })
                            // 2) then immediately request makeusermove
                            .then(() =>
                                fetch(`${BASE}/makeusermove`, {
                                    method: "POST",
                                    credentials: "include",
                                })
                            )
                            // 3) parse JSON or throw if not ok
                            .then((res) => {
                                if (!res.ok) throw new Error("Move failed");
                                return res.json();
                            })
                            // 4) handle the response: update board, modal, reset state
                            .then(
                                (moveData: {
                                    message: string;
                                    fromSq: string;
                                    toSq: string;
                                    pieces: GuiPiece[];
                                    result?: string;
                                }) => {
                                    // update board immediately
                                    setPieces(moveData.pieces);

                                    // show any result (check/mate)
                                    if (moveData.result) {
                                        setModalMsg(moveData.result);
                                    }

                                    // clear UI selection
                                    setSelectedSquares({
                                        from: null,
                                        to: null,
                                    });

                                    // reset backend userMove
                                    fetch(`${BASE}/resetusermove`, {
                                        method: "POST",
                                        credentials: "include",
                                    });

                                    // trigger engine move
                                    onEngineMove(engineTime);
                                    return undefined;
                                }
                            )
                            // 5) catch errors, reset UI & backend
                            .catch((err) => {
                                console.error("Error making move:", err);
                                setSelectedSquares({ from: null, to: null });
                                fetch(`${BASE}/resetusermove`, {
                                    method: "POST",
                                    credentials: "include",
                                });
                                return undefined;
                            })
                    );
                }
            })
            .catch((err) => console.error("Error fetching square:", err));
    };

    return (
        <div
            ref={boardRef}
            id="Board"
            className="relative left-38"
            style={{ width: BOARD_SIZE, height: BOARD_SIZE }}
        >
            {generateBoardSquares()}

            {pieces.map((p, i) => {
                // parse the backend‐provided `fileClass` & `rankClass`
                const fileIdx =
                    parseInt(p.fileClass.replace("file", ""), 10) - 1;
                const rankIdx =
                    parseInt(p.rankClass.replace("rank", ""), 10) - 1;

                // compute where to draw it on screen
                const displayFile = flipped ? 7 - fileIdx : fileIdx;
                const displayRank = flipped ? rankIdx : 7 - rankIdx;

                return (
                    <img
                        key={`pc-${i}-${p.imagePath}`}
                        src={`/images/${p.imagePath}`}
                        className="Piece absolute"
                        style={{
                            width: SQUARE_SIZE,
                            height: SQUARE_SIZE,
                            left: displayFile * SQUARE_SIZE,
                            top: displayRank * SQUARE_SIZE,
                        }}
                        alt={p.imagePath}
                        onClick={(e) => handleClick(e, "Piece")}
                    />
                );
            })}
        </div>
    );
}
export default Board;
