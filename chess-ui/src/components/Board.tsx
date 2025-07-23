import React, {
    useRef,
    useState,
    type Dispatch,
    type SetStateAction,
} from "react";

type GuiPiece = {
    fileClass: string;
    rankClass: string;
    imagePath: string;
};

type BoardProps = {
    gameId: string;
    pieces: GuiPiece[];
    setPieces: Dispatch<SetStateAction<GuiPiece[]>>;
    setModalMsg: (msg: string | null) => void;
    onEngineMove: (t: number) => void;
    engineTime: number;
    flipped: boolean;
};

export default function Board({
    gameId,
    pieces,
    setPieces,
    setModalMsg,
    onEngineMove,
    engineTime,
    flipped,
}: BoardProps) {
    const BASE = import.meta.env.VITE_API_BASE_URL as string;
    const BOARD_SIZE = 600;
    const SQUARE_SIZE = BOARD_SIZE / 8;
    const boardRef = useRef<HTMLDivElement>(null);
    const [selectedSquares, setSelectedSquares] = useState<{
        from: { file: number; rank: number } | null;
        to: { file: number; rank: number } | null;
    }>({ from: null, to: null });

    function toLogicalSquare(displayFile: number, displayRank: number) {
        const file = flipped ? 7 - displayFile : displayFile;
        const rank = flipped ? displayRank : 7 - displayRank;
        return { file, rank };
    }

    const handleClick = async (
        e: React.MouseEvent,
        type: "Piece" | "Square"
    ): Promise<void> => {
        if (!boardRef.current) return; // only bail if no board

        const { left, top } = boardRef.current.getBoundingClientRect();
        const x = e.clientX - left;
        const y = e.clientY - top;
        const displayFile = Math.floor(x / SQUARE_SIZE);
        const displayRank = Math.floor(y / SQUARE_SIZE);
        const { file, rank } = toLogicalSquare(displayFile, displayRank);

        try {
            // convert to engine square index + algebraic
            const { sq: newSq, prSq } = await fetch(
                `${BASE}/fr2sq?file=${file}&rank=${rank}`
            ).then((r) => r.json());
            console.log(`Selected square: ${prSq} (${file}, ${rank})`);

            // first click = set "from"
            if (type === "Piece" && !selectedSquares.from) {
                setSelectedSquares({ from: { file, rank }, to: null });
                await fetch(`${BASE}/setusermove?gameId=${gameId}`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(newSq),
                });
                return;
            }

            // second click = set "to" + make user move
            if (
                (type === "Piece" || type === "Square") &&
                selectedSquares.from
            ) {
                setSelectedSquares({
                    from: selectedSquares.from!,
                    to: { file, rank },
                });

                await fetch(`${BASE}/setusermove?gameId=${gameId}`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(newSq),
                });

                const moveRes = await fetch(
                    `${BASE}/makeusermove?gameId=${gameId}`,
                    { method: "POST" }
                );
                if (!moveRes.ok) {
                    console.error("Move failed", moveRes.statusText);
                    setSelectedSquares({ from: null, to: null });
                    return;
                }
                const moveData: { pieces: GuiPiece[]; result?: string } =
                    await moveRes.json();

                // update UI
                setPieces(moveData.pieces);
                if (moveData.result) setModalMsg(moveData.result);

                // reset selection & trigger engine move
                setSelectedSquares({ from: null, to: null });
                onEngineMove(engineTime);
            }
        } catch (err) {
            console.error("Error in click handler:", err);
        }
    };

    return (
        <div
            ref={boardRef}
            className="left-38 mt-6"
            style={{
                width: BOARD_SIZE,
                height: BOARD_SIZE,
                position: "relative",
            }}
        >
            {/* board squares */}
            {Array.from({ length: 8 }).flatMap((_, r) =>
                Array.from({ length: 8 }).map((_, f) => {
                    const { file, rank } = toLogicalSquare(f, r);
                    const isLight = (file + rank) % 2 === 0;
                    const isSelected =
                        (selectedSquares.from?.file === file &&
                            selectedSquares.from?.rank === rank) ||
                        (selectedSquares.to?.file === file &&
                            selectedSquares.to?.rank === rank);
                    return (
                        <div
                            key={`sq-${r}-${f}`}
                            className={`${isLight ? "Light" : "Dark"}${
                                isSelected ? " SqSelected" : ""
                            }`}
                            style={{
                                width: SQUARE_SIZE,
                                height: SQUARE_SIZE,
                                position: "absolute",
                                top: r * SQUARE_SIZE,
                                left: f * SQUARE_SIZE,
                            }}
                            onClick={(e) => handleClick(e, "Square")}
                        />
                    );
                })
            )}

            {/* pieces */}
            {pieces.map((p, i) => {
                const fileIdx =
                    parseInt(p.fileClass.replace("file", ""), 10) - 1;
                const rankIdx =
                    parseInt(p.rankClass.replace("rank", ""), 10) - 1;
                const displayFile = flipped ? 7 - fileIdx : fileIdx;
                const displayRank = flipped ? rankIdx : 7 - rankIdx;
                return (
                    <img
                        key={`pc-${i}-${p.imagePath}`}
                        src={`/images/${p.imagePath}`}
                        alt={p.imagePath}
                        style={{
                            width: SQUARE_SIZE,
                            height: SQUARE_SIZE,
                            position: "absolute",
                            left: displayFile * SQUARE_SIZE,
                            top: displayRank * SQUARE_SIZE,
                            cursor: "pointer",
                        }}
                        onClick={(e) => handleClick(e, "Piece")}
                    />
                );
            })}
        </div>
    );
}
