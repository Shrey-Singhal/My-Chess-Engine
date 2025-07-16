import React, { type JSX } from "react";

type GuiPiece = {
    fileClass: string;
    rankClass: string;
    imagePath: string;
}

function Board({pieces}: {pieces: GuiPiece[]}) {
    //const [pieces, setPieces] = useState<GuiPiece[]>([]);
    const squares_style = "absolute w-[60px] h-[60px]";

    //render squares
    const generateBoardSquares = (): JSX.Element[] => {
        const squares: JSX.Element[] = [];
        let light = 1;

        for (let rank = 7; rank >= 0; rank--) {
            light ^= 1;
            const rankClass = `rank${rank+1}`;

            for (let file = 0; file <= 7; file++) {
                const fileClass = `file${file+1}`;
                const colorClass = light === 0 ? "Light" : "Dark";
                light ^= 1;

                const combinedClass = `${squares_style} ${rankClass} ${fileClass} ${colorClass}`;
                squares.push(
                    <div
                        key={`${rankClass}-${fileClass}`}
                        className={combinedClass}
                    ></div>
                );
            }
        }

        return squares;
    }

    return (
        <div className="relative top-5 left-14 w-[480px] h-[480px]">
            {generateBoardSquares()}
            {pieces.map((p, i) => {
                const rankClass = p.rankClass;
                const fileClass = p.fileClass;
                const imgSrc = `/images/${p.imagePath}`;
                return (
                <img
                    key={i}
                    src={imgSrc}
                    className={`Piece ${rankClass} ${fileClass} absolute w-[60px] h-[60px]`}
                    alt={p.imagePath}
                />
                );
            })}
        </div>
    );
}
export default Board;
